using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AudioView.Common.Data;
using AudioView.Common.DataAccess;
using AudioView.Common.Engine;
using Newtonsoft.Json;
using NLog;
using Project = AudioView.Common.DataAccess.Project;
using Reading = AudioView.Common.DataAccess.Reading;

namespace AudioView.Common.Listeners
{
    public delegate void SQLServerStatus(bool status);

    public class DataStorageMeterListener : IMeterListener
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        public static event SQLServerStatus OnSQLServerStatusChanged;

        private bool isServerDown = false;
        private static object LogFileLock = new object();
        private static string DataFolder = "DataStorage";
        private Guid Id { get; set; }
        private MeasurementSettings Settings { get; set; }
        private object initalCreateLock = new object();
        private Task initalCreate;
        private DateTime created;

        public DataStorageMeterListener(Guid id, DateTime started, MeasurementSettings settings)
        {
            Id = id;
            Settings = settings;
            created = started;

            initalCreate = Task.Run(async () =>
            {
                try
                {
                    using (var audioViewEntities = new AudioViewEntities())
                    {
                        if (!audioViewEntities.Projects.Where(x => x.Id == id).Any())
                        {
                            audioViewEntities.Projects.Add(new Project()
                            {
                                Id = id,
                                Created = started,
                                MinorDBLimit = settings.MinorDBLimit,
                                MajorDBLimit = settings.MajorDBLimit,
                                MajorInterval = settings.MajorInterval.Ticks,
                                MinorInterval = settings.MinorInterval.Ticks,
                                Name = settings.ProjectName,
                                Number = settings.ProjectNumber
                            });
                            await audioViewEntities.SaveChangesAsync().ConfigureAwait(false);
                        }
                    }
                }
                catch (Exception exp)
                {
                    logger.Warn(exp, "Unable to add project \"{0}\" to database, do we have internet access?", settings.ProjectName);
                }
            });
        }

        public Task OnMinor(DateTime time, ReadingData data)
        {
            return Task.Run(() =>
            {
                WriteReading(false, time, data);
            });
        }

        public Task OnMajor(DateTime time, ReadingData data)
        {
            return Task.Run(() =>
            {
                WriteReading(true, time, data);
            });
        }

        public Task OnSecond(DateTime time, ReadingData data, ReadingData minorData, ReadingData majorData)
        {
            return Task.FromResult<object>(null);
        }

        public Task NextMinor(DateTime time)
        {
            return Task.FromResult<object>(null);
        }

        public Task NextMajor(DateTime time)
        {
            return Task.FromResult<object>(null);
        }

        public Task StopListener()
        {
            return Task.FromResult<object>(null);
        }

        private async void WriteReading(bool major, DateTime time, ReadingData data)
        {
            var readingGuid = Guid.NewGuid();
            try
            {
                // Ensure the inital create have finished
                if (initalCreate != null)
                {
                    lock (initalCreateLock)
                    {
                        if (initalCreate != null)
                        {
                            Task.WaitAll(initalCreate);
                            initalCreate = null;
                        }
                    }
                }

                var reading = new Reading()
                {
                    Id = readingGuid,
                    Project = this.Id,
                    Major = major ? (byte)1 : (byte)0,
                    Time = time,
                    LAeq = data.LAeq
                };

                using (var audioViewEntities = new AudioViewEntities())
                {
                    audioViewEntities.Readings.Add(reading);
                    await audioViewEntities.SaveChangesAsync();
                }


                if (isServerDown && OnSQLServerStatusChanged != null)
                {
                    OnSQLServerStatusChanged(false);
                }
                isServerDown = false;

                // Reading ok, try to load offline files
                await UploadLocalFiles();
            }
            catch (Exception exp)
            {
                logger.Debug(exp, "Got database exception, starting to write offline for \"{0}\".", this.Settings.ProjectName);
                WriteOfline(major, time, data, readingGuid);

                if (!isServerDown && OnSQLServerStatusChanged != null)
                {
                    OnSQLServerStatusChanged(true);
                }
                isServerDown = true;
            }
        }

        private void WriteOfline(bool major, DateTime time, ReadingData data, Guid readingGuid)
        {

            lock (LogFileLock)
            {
                DirectoryInfo directory = new DirectoryInfo(DataFolder);
                if (!directory.Exists)
                {
                    directory.Create();
                }
                FileInfo file = new FileInfo(Path.Combine(directory.FullName, Id + ".bin"));
                if (!file.Exists)
                {
                    using (var f = file.Create())
                    {
                    }
                    File.WriteAllLines(file.FullName, new[]{ JsonConvert.SerializeObject(new MeasurementSettingsDataStorageWarpper()
                    {
                        Id = Id,
                        Created = created,
                        MeasurementSettings = Settings
                    })});
                }
                File.AppendAllLines(file.FullName, new []{ JsonConvert.SerializeObject(new ReadingDataDataStorageWarpper()
                {
                    Id = readingGuid,
                    Data = data,
                    IsMajor = major,
                    Time = time
                }) });
            }
        }

        public static Task UploadLocalFiles()
        {
            return Task.Run(() =>
            {
                lock (LogFileLock)
                {
                    DirectoryInfo directory = new DirectoryInfo(DataFolder);
                    logger.Trace("Looking or unuploade files in \"{0}\".", directory.FullName);
                    if (!directory.Exists)
                    {
                        logger.Trace("No files where found in \"{0}\".", directory.FullName);
                        return;
                    }

                    List<string> filesToDelete = new List<string>(); 
                    foreach (var fileInfo in directory.GetFiles())
                    {
                        try
                        {
                            logger.Info("Found file to upload \"{0}\"", fileInfo.FullName);
                            var lines = File.ReadAllLines(fileInfo.FullName);
                            var settings = JsonConvert.DeserializeObject<MeasurementSettingsDataStorageWarpper>(lines[0]);
                            var readings =
                                lines.Skip(1)
                                    .Select(JsonConvert.DeserializeObject<ReadingDataDataStorageWarpper>)
                                    .ToList();
                            logger.Debug("File had {0} readings.", readings.Count);
                            
                            using (var audioViewEntities = new AudioViewEntities())
                            {
                                if (!audioViewEntities.Projects.Any(x => x.Id == settings.Id))
                                {
                                    logger.Debug("Project \"{0}\" was not in the database adding.", settings.MeasurementSettings.ProjectName);
                                    audioViewEntities.Projects.Add(new Project()
                                    {
                                        Id = settings.Id,
                                        Created = settings.Created,
                                        MinorDBLimit = settings.MeasurementSettings.MinorDBLimit,
                                        MajorDBLimit = settings.MeasurementSettings.MajorDBLimit,
                                        MajorInterval = settings.MeasurementSettings.MajorInterval.Ticks,
                                        MinorInterval = settings.MeasurementSettings.MinorInterval.Ticks,
                                        Name = settings.MeasurementSettings.ProjectName,
                                        Number = settings.MeasurementSettings.ProjectNumber
                                    });
                                    audioViewEntities.SaveChanges();
                                }

                                var knownReadings =
                                    audioViewEntities.Readings.Where(x => x.Project == settings.Id)
                                        .Select(x => x.Id)
                                        .ToList();
                                logger.Trace("Got {0} known readings for {1}", knownReadings.Count, settings.Id);
                                var readingsToUpload = readings.Where(x => !knownReadings.Contains(x.Id)).ToList();
                                foreach (var r in readingsToUpload)
                                {
                                    audioViewEntities.Readings.Add(new Reading()
                                    {
                                        Id = r.Id,
                                        Project = settings.Id,
                                        Major = r.IsMajor ? (byte) 1 : (byte) 0,
                                        Time = r.Time,
                                        LAeq = r.Data.LAeq,
                                    });
                                }
                                logger.Trace("Added {0} new readings for {1}", readingsToUpload.Count, settings.Id);
                                audioViewEntities.SaveChanges();
                            }
                            // If we got to here is the file uploaded
                            filesToDelete.Add(fileInfo.FullName);
                        }
                        catch (DbEntityValidationException e)
                        {
                            logger.Error(e, "Was unable to uploade \"{0}\"", fileInfo.FullName);
                            foreach (var eve in e.EntityValidationErrors)
                            {
                                logger.Error("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                                    eve.Entry.Entity.GetType().Name, eve.Entry.State);
                                foreach (var ve in eve.ValidationErrors)
                                {
                                    logger.Error("- Property: \"{0}\", Error: \"{1}\"",
                                        ve.PropertyName, ve.ErrorMessage);
                                }
                            }
                            throw;
                        }
                        catch (Exception exp)
                        {
                            logger.Error(exp, "Was unable to uploade \"{0}\"", fileInfo.FullName);
                        }
                    }

                    foreach (var f in filesToDelete)
                    {
                        try
                        {
                            new FileInfo(f).Delete();
                        }
                        catch (Exception exp)
                        {
                            logger.Error(exp, "Was unable to delete \"{0}\" all data have been uploaded.", f);
                        }
                    }
                }
            });
        }
    }

    public class ReadingDataDataStorageWarpper
    {
        public Guid Id { get; set; }
        public bool IsMajor { get; set; }
        public DateTime Time { get; set; }
        public ReadingData Data { get; set; }
    }

    public class MeasurementSettingsDataStorageWarpper
    {
        public Guid Id { get; set; }
        public DateTime Created { get; set; }
        public MeasurementSettings MeasurementSettings { get; set; }
    }
}
