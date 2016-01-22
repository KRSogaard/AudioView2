using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Core.Metadata.Edm;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using AudioView.Common.Data;
using AudioView.Common.Services;
using AudioView.UserControls.Graph;
using GalaSoft.MvvmLight.Threading;
using NLog;
using Prism.Commands;
using Prism.Mvvm;

namespace AudioView.ViewModels
{
    public class HistorySearchResult : BindableBase
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();


        private HistoryViewModel parent;
        private IDatabaseService databaseService;
        private DateTime? firstDate;
        private DateTime? lastDate;
        private TimeSpan dateSpan;

        private Project project;

        public string ProjectName
        {
            get { return project.Name; }
        }

        public string ProjectNumber
        {
            get { return project.Number; }
        }

        public string Date
        {
            get { return project.Created.ToString("f"); }
        }

        public string MinorDBLimit
        {
            get { return project.MinorDBLimit + " dB"; }
        }

        public string MajorDBLimit
        {
            get { return project.MajorDBLimit + " dB"; }
        }

        public string MinorInterval
        {
            get { return project.MinorInterval.ToString(); }
        }

        public string MajorInterval
        {
            get { return project.MajorInterval.ToString(); }
        }

        public string MeasurementsCount
        {
            get { return project.Readings.ToString(); }
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get { return _isLoading; }
            set { SetProperty(ref _isLoading, value); }
        }

        private int _leftBound;
        public int LeftBound
        {
            get { return _leftBound; }
            set
            {
                SetProperty(ref _leftBound, value); 
                OnPropertyChanged("LeftBoundText");
                MajorGraph.LeftDate = GetLeftDate();
                MinorGraph.LeftDate = GetLeftDate();
            }
        }

        private int _rightBound;
        public int RightBound
        {
            get { return _rightBound; }
            set {
                SetProperty(ref _rightBound, value);
                OnPropertyChanged("RightBoundText");
                MajorGraph.RightDate = GetRightDate();
                MinorGraph.RightDate = GetRightDate();
            }
        }

        private DateTime GetLeftDate()
        {
            if (firstDate == null)
                return DateTime.Now;
            var procentage = ((double) LeftBound/1000.0);
            var ticks = procentage * (double) dateSpan.Ticks;
            var date = firstDate.Value.AddTicks((long)ticks);
            return date;
        }

        private DateTime GetRightDate()
        {
            if (firstDate == null)
                return DateTime.Now;
            var procentage = ((double)RightBound / 1000.0);
            var ticks = procentage * (double)dateSpan.Ticks;
            var date = firstDate.Value.AddTicks((long)ticks);
            return date;
        }

        public string LeftBoundText
        {
            get { return GetLeftDate().ToString("f"); }
        }
        public string RightBoundText
        {
            get { return GetRightDate().ToString("f"); }
        }

        private ObservableCollection<HistoryReadingViewModel> _readingsMinor;
        public ObservableCollection<HistoryReadingViewModel> ReadingsMinor
        {
            get { return _readingsMinor; }
            set { SetProperty(ref _readingsMinor, value); }
        }

        private ObservableCollection<HistoryReadingViewModel> _readingsMajor;
        public ObservableCollection<HistoryReadingViewModel> ReadingsMajor
        {
            get { return _readingsMajor; }
            set { SetProperty(ref _readingsMajor, value); }
        }

        public HistorySearchResult(HistoryViewModel parent, Project project)
        {
            this.parent = parent;
            this.project = project;
            databaseService = new DatabaseService();
            ReadingsMajor = new ObservableCollection<HistoryReadingViewModel>();
            ReadingsMinor = new ObservableCollection<HistoryReadingViewModel>();

            MinorGraph = new AudioViewGraphViewModel(false,
                    10, // Ignored
                    project.MinorDBLimit,
                    project.MinorInterval,
                    50,
                    150)
            {
                IsEnabled = true
            };
            MajorGraph = new AudioViewGraphViewModel(true,
                    10,
                    project.MajorDBLimit,
                    project.MajorInterval,
                    50,
                    150)
            {
                IsEnabled = true
            };
        }

        public AudioViewGraphViewModel MajorGraph { get; set; }
        public AudioViewGraphViewModel MinorGraph { get; set; }

        public void LoadReadings()
        {
            if (ReadingsMinor.Count > 0 || ReadingsMajor.Count > 0)
            {
                logger.Trace("Already got reading for {0} skipping load", project.Id);
                return;
            }
            logger.Debug("Loading reading for {0} ({1})", project.Name, project.Id);
            IsLoading = true;
            databaseService.GetReading(project.Id).ContinueWith((Task<IList<Reading>> task) =>
            {
                var readings = task.Result.OrderBy(x=>x.Time);
                logger.Debug("Got {0} readings for {1}", task.Result.Count, project.Id);
                var minor = readings.Where(x => !x.Major).Select(x=>new HistoryReadingViewModel(x, this)).ToList();
                var major = readings.Where(x => x.Major).Select(x => new HistoryReadingViewModel(x, this)).ToList();

                // Ok to do on ther other thread
                firstDate = readings.FirstOrDefault()?.Time;
                lastDate = readings.LastOrDefault()?.Time;
                if(firstDate != null)
                {
                    dateSpan = (lastDate.Value - firstDate.Value);
                }

                // Graph minipolations can be done outside of the UI thread
                Task.Factory.StartNew(() =>
                {
                    var minMaxOrder = readings.OrderBy(x => x.LAeq).Select(x => x.LAeq).ToList();
                    var min = minMaxOrder.FirstOrDefault();
                    var max = minMaxOrder.LastOrDefault();
                    if (min == 0) min = 50;
                    if (max == 0) max = 150;

                    var majorGraphTask = Task.Factory.StartNew(() =>
                    {
                        while (!MajorGraph.Readings.IsEmpty)
                        {
                            Tuple<DateTime, double> result;
                            MajorGraph.Readings.TryDequeue(out result);
                        }
                    });
                    var minorGraphTask = Task.Factory.StartNew(() =>
                    {
                        while (!MinorGraph.Readings.IsEmpty)
                        {
                            Tuple<DateTime, double> result;
                            MinorGraph.Readings.TryDequeue(out result);
                        }
                    });
                    Task.WaitAll(majorGraphTask, minorGraphTask);

                    foreach (var model in minor)
                    {
                        MinorGraph.Readings.Enqueue(new Tuple<DateTime, double>(model.Reading.Time, model.Reading.LAeq));
                    }
                    foreach (var model in major)
                    {
                        MajorGraph.Readings.Enqueue(new Tuple<DateTime, double>(model.Reading.Time, model.Reading.LAeq));
                    }

                    DispatcherHelper.CheckBeginInvokeOnUI(() =>
                    {
                        MajorGraph.IsCustomSpan = true;
                        MajorGraph.MinHeight = min;
                        MajorGraph.MaxHeight = max;

                        MinorGraph.IsCustomSpan = true;
                        MinorGraph.MinHeight = min;
                        MinorGraph.MaxHeight = max;

                        LeftBound = 800;
                        RightBound = 1000;
                    });
                });

                DispatcherHelper.CheckBeginInvokeOnUI(() =>
                {
                    ReadingsMinor.Clear();
                    ReadingsMajor.Clear();
                    ReadingsMinor.AddRange(minor);
                    ReadingsMajor.AddRange(major);
                    IsLoading = false;
                });
            });
        }

        public void RemoveReading(HistoryReadingViewModel historyReadingViewModel)
        {
            ReadingsMajor.Remove(historyReadingViewModel);
            ReadingsMinor.Remove(historyReadingViewModel);
        }
    }
}