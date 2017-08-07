using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using AudioView.Common.Data;
using AudioView.Common.Export;
using AudioView.Common.Services;
using GalaSoft.MvvmLight.Threading;
using Microsoft.Win32;
using NLog;
using Prism.Commands;
using Prism.Mvvm;

namespace AudioView.ViewModels
{
    public class HistorySearchResult : BindableBase
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        
        private IDatabaseService databaseService;
        private DateTime? firstDate;
        private DateTime? lastDate;
        private TimeSpan dateSpan;
        private Task resultDownloadTask;
        private Project project;

        public HistorySearchResult()
        {
            MinorGraphValues = new ObservableCollection<Tuple<DateTime, double>>();
            MajorGraphValues = new ObservableCollection<Tuple<DateTime, double>>();
        }

        public string ProjectName
        {
            get { return project.Name; }
        }

        public int ProjectNumber
        {
            get
            {
                int tryInt;
                if (!int.TryParse(project.Number, out tryInt))
                {
                    return 0;
                }
                return tryInt;
            }
        }

        public DateTime Date
        {
            get { return project.Created; }
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

        public Project Project
        {
            get { return project; }
        }

        public int MeasurementsCount
        {
            get { return project.Readings; }
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
                GraphSpan = RightTime - GetLeftDate();
            }
        }

        private int _rightBound;
        public int RightBound
        {
            get { return _rightBound; }
            set {
                SetProperty(ref _rightBound, value);
                OnPropertyChanged("RightBoundText");
                RightTime = GetRightDate();
                GraphSpan = RightTime - GetLeftDate();
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

        private List<HistoryReadingViewModel> _readingsMinor;
        public List<HistoryReadingViewModel> ReadingsMinor
        {
            get { return _readingsMinor; }
            set { SetProperty(ref _readingsMinor, value); }
        }

        private List<HistoryReadingViewModel> _readingsMajor;
        public List<HistoryReadingViewModel> ReadingsMajor
        {
            get { return _readingsMajor; }
            set { SetProperty(ref _readingsMajor, value); }
        }
        
        public HistorySearchResult(Project project)
        {
            this.project = project;
            databaseService = new DatabaseService();
        }

        private double _graphYMin;
        public double GraphYMin
        {
            get { return _graphYMin; }
            set
            {
                SetProperty(ref _graphYMin, value);
            }
        }

        private double _graphYMax;
        public double GraphYMax
        {
            get { return _graphYMax; }
            set
            {
                SetProperty(ref _graphYMax, value);
            }
        }

        private DateTime _rightTime;
        public DateTime RightTime
        {
            get { return _rightTime; }
            set
            {
                SetProperty(ref _rightTime, value);
            }
        }

        private TimeSpan _graphSpan;
        public TimeSpan GraphSpan
        {
            get { return _graphSpan; }
            set
            {
                SetProperty(ref _graphSpan, value);
            }
        }

        private ObservableCollection<Tuple<DateTime, double>> _minorGraphValues;
        public ObservableCollection<Tuple<DateTime, double>> MinorGraphValues {
            get { return _minorGraphValues; }
            set
            {
                SetProperty(ref _minorGraphValues, value);
            }
        }
        private ObservableCollection<Tuple<DateTime, double>> _majorGraphValues;
        public ObservableCollection<Tuple<DateTime, double>> MajorGraphValues
        {
            get { return _majorGraphValues; }
            set
            {
                SetProperty(ref _majorGraphValues, value);
            }
        }

        public async Task Preloadreadings(IList<Reading> majorReadings, IList<Reading> minoReadings)
        {
            ReadingsMajor = majorReadings.Select(x => new HistoryReadingViewModel(x, this)).ToList();
            ReadingsMinor = minoReadings.Select(x => new HistoryReadingViewModel(x, this)).ToList();

            firstDate = ReadingsMajor.FirstOrDefault()?.Reading.Time;
            var first = ReadingsMinor.FirstOrDefault()?.Reading.Time;
            if (first < firstDate || firstDate == new DateTime()) firstDate = first;
            lastDate = ReadingsMajor.LastOrDefault()?.Reading.Time;
            var last = ReadingsMajor.FirstOrDefault()?.Reading.Time;
            if (last > lastDate || lastDate == new DateTime()) lastDate = last;

            if (firstDate != null)
            {
                dateSpan = (lastDate.Value - firstDate.Value);
            }

            await LoadGraph(ReadingsMajor, ReadingsMinor);
        }

        public Task LoadReadings()
        {
            if (ReadingsMinor != null && ReadingsMajor != null 
                && (ReadingsMinor.Count > 0 || ReadingsMajor.Count > 0))
            {
                logger.Trace("Already got reading for {0} skipping load", project.Id);
                return Task.FromResult<object>(null);
            }

            logger.Debug("Loading reading for {0} ({1})", project.Name, project.Id);
            IsLoading = true;
            return databaseService.GetReading(project.Id).ContinueWith((Task<IList<Reading>> task) =>
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
                LoadGraph(major, minor);

                _readingsMinor = new List<HistoryReadingViewModel>();
                _readingsMajor = new List<HistoryReadingViewModel>();
                _readingsMinor.AddRange(minor);
                _readingsMajor.AddRange(major);

                DispatcherHelper.CheckBeginInvokeOnUI(() =>
                {
                    OnPropertyChanged("ReadingsMinor");
                    OnPropertyChanged("ReadingsMajor");
                    IsLoading = false;
                });
            });
        }

        private Task LoadGraph(List<HistoryReadingViewModel> major, List<HistoryReadingViewModel> minor)
        {
            return Task.Run(() =>
            {
                var minMaxOrder = major.Union(minor).OrderBy(x => x.Reading.Data.LAeq).Select(x => x.Reading.Data.LAeq).ToList();
                var min = minMaxOrder.FirstOrDefault();
                var max = minMaxOrder.LastOrDefault();
                if (min == 0) min = 50;
                if (max == 0) max = 150;

                DispatcherHelper.CheckBeginInvokeOnUI(() =>
                {
                    if (MinorGraphValues == null)
                    {
                        MinorGraphValues = new ObservableCollection<Tuple<DateTime, double>>();
                    }
                    if (MajorGraphValues == null)
                    {
                        MajorGraphValues = new ObservableCollection<Tuple<DateTime, double>>();
                    }
                    MinorGraphValues.Clear();
                    MajorGraphValues.Clear();

                    MinorGraphValues.AddRange(minor.Select(model =>
                    new Tuple<DateTime, double>(model.Reading.Time, model.Reading.Data.LAeq)));
                    MajorGraphValues.AddRange(major.Select(model =>
                    new Tuple<DateTime, double>(model.Reading.Time, model.Reading.Data.LAeq)));

                    GraphYMin = min;
                    GraphYMax = max;

                    LeftBound = 800;
                    RightBound = 1000;
                });
            });
        }

        public void RemoveReading(HistoryReadingViewModel historyReadingViewModel)
        {
            ReadingsMajor.Remove(historyReadingViewModel);
            ReadingsMinor.Remove(historyReadingViewModel);
        }

        private ICommand _downloadExcel;
        public ICommand DownloadExcel
        {
            get
            {
                if (_downloadExcel == null)
                {
                    _downloadExcel = new DelegateCommand(async () =>
                    {
                        try
                        {
                            SaveFileDialog saveFileDialog = new SaveFileDialog();
                            saveFileDialog.FileName = ProjectName + ".xlsx";
                            saveFileDialog.Filter = "Excel file (*.xlsx)|*.xlsx";
                            if (saveFileDialog.ShowDialog() == true)
                            {
                                await LoadReadings();
                                var readingToSave = ReadingsMajor.Select(x => x.Reading)
                                    .Union(ReadingsMinor.Select(x => x.Reading))
                                    .ToList();
                                var ordered = readingToSave.OrderBy(x => x.Time).ToList();
                                var excel = new ExcelExport(project, ordered);
                                excel.writeFile(saveFileDialog.FileName);
                            }
                        }
                        catch (Exception exp)
                        {
                            logger.Error(exp, "Failed to save the readinds as CSV.");
                        }
                    });
                }
                return _downloadExcel;
            }
        }

        public void OnSelected()
        {
        }
    }
}