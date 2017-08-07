using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using AudioView.Common;
using AudioView.Common.Data;
using AudioView.Common.Engine;
using AudioView.Common.Export;
using AudioView.Common.Listeners;
using AudioView.UserControls.CountDown;
using AudioView.UserControls.CountDown.ClockItems;
using AudioView.UserControls.Graphs;
using AudioView.Views.History;
using AudioView.Views.Measurement;
using AudioView.Views.PopOuts;
using GalaSoft.MvvmLight.Threading;
using MahApps.Metro;
using MahApps.Metro.Controls;
using Microsoft.Win32;
using NLog;
using Prism.Commands;
using Prism.Mvvm;

namespace AudioView.ViewModels
{
    public partial class MeasurementViewModel
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private LinkedList<MetroWindow> popOutWindows;
        public AudioViewEngine engine { get; set; }
        private MeasurementSettings settings;
        private DataStorageMeterListener dataStorage;
        private DateTime started;
        private TCPServerListener tcpServer;
        private bool ConnectionStatus;
        private TimeSpan minorSpan;
        private TimeSpan majorSpan;

        private ConcurrentQueue<Tuple<DateTime,ReadingData>> MinorReadings;
        private ConcurrentQueue<Tuple<DateTime, ReadingData>> MajorReadings;
        private ObservableCollection<Tuple<DateTime, double>> barMinorValues;
        private ObservableCollection<Tuple<DateTime, double>> barMajorValues;
        private ObservableCollection<Tuple<DateTime, double>> lineValues;
        private ObservableCollection<OctaveBandGraphValue> octaveValues;
        private ReadingsStorage readingHistory;
        private ReadingsStorage minorIntervalHistory;
        private ReadingsStorage majorIntervalHistory;
        private AudioViewCountDownViewModel minorClockViewModel;
        private AudioViewCountDownViewModel majorClockViewModel;
        private DateTime lastMinorInterval;
        private DateTime lastMajorInterval;
        
        private GraphViewModel minorGraphViewModel;
        private GraphViewModel majorGraphViewModel;

        public MeasurementViewModel(Guid id, MeasurementSettings settings, IMeterReader reader)
        {
            MinorReadings = new ConcurrentQueue<Tuple<DateTime, ReadingData>>();
            MajorReadings = new ConcurrentQueue<Tuple<DateTime, ReadingData>>();
            OctaveValues = new ObservableCollection<OctaveBandGraphValue>();
            MinorSpan = TimeSpan.FromTicks(settings.MinorInterval.Ticks * 15);
            MajorSpan = TimeSpan.FromTicks(settings.MajorInterval.Ticks * 15);
            readingHistory = new ReadingsStorage(MajorSpan);
            minorIntervalHistory = new ReadingsStorage(MinorSpan);
            majorIntervalHistory = new ReadingsStorage(majorSpan);
            minorGraphViewModel = new GraphViewModel("LAeq", readingHistory, minorIntervalHistory, MinorSpan);
            majorGraphViewModel = new GraphViewModel("LAeq", null, majorIntervalHistory, MajorSpan, false, true);
            lastMinorInterval = new DateTime();
            lastMajorInterval = new DateTime();

            started = DateTime.Now;
            popOutWindows = new LinkedList<MetroWindow>();
            this.engine = new AudioViewEngine(settings.MinorInterval, settings.MajorInterval, reader);
            this.settings = settings;

            this.engine.ConnectionStatusEvent += connected =>
            {
                if(ConnectionStatus == connected)
                    return; // No need

                Task.Run(() =>
                {
                    ConnectionStatus = connected;
                    DispatcherHelper.CheckBeginInvokeOnUI(() =>
                    {
                        OnPropertyChanged(nameof(IsDisconnected));
                    });
                });
            };
            
            if (settings.IsLocal)
            {
                // Registering the TCP Server for remote connections
                tcpServer = new TCPServerListener(settings);
                this.engine.RegisterListener(tcpServer);

                // Register the data storange unit
                dataStorage = new DataStorageMeterListener(id, DateTime.Now, settings);
                this.engine.RegisterListener(dataStorage);
            }

            this.engine.RegisterListener(new LocalStorageListener(AudioViewSettings.Instance.AutoSaveLocation, settings.ProjectName));

            this.engine.RegisterListener(this);
            MinorClock = new AudioViewCountDownViewModel(false,
                    settings.MinorInterval,
                    settings.MinorDBLimit,
                    settings.MinorClockMainItem,
                    settings.MinorClockSecondaryItem);
            MajorClock = new AudioViewCountDownViewModel(true,
                    settings.MajorInterval,
                    settings.MajorDBLimit,
                    settings.MajorClockMainItem,
                    settings.MajorClockSecondaryItem);
            this.engine.RegisterListener(MinorClock);
            this.engine.RegisterListener(MajorClock);
            this.engine.Start();

            Title = settings.ProjectName;
        }

        private string title;

        public Brush TextColor
        {
            get
            {
                return new SolidColorBrush(Colors.White);
            }
        }

        public MeasurementSettings Settings
        {
            get { return settings; }
        }

        public string MinorDBLimit
        {
            get { return settings.MinorDBLimit + "dB."; }
        }
        public string MajorDBLimit
        {
            get { return settings.MajorDBLimit + "dB."; }
        }
        public string MinorInterval
        {
            get { return settings.MinorInterval.ToString(); }
        }
        public string MajorInterval
        {
            get { return settings.MajorInterval.ToString(); }
        }
        public string Port
        {
            get { return settings.Port.ToString(); }
        }
        public string Started
        {
            get { return started.ToString("g"); }
        }
        public string MyIp
        {
            get
            {
                var host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        return ip.ToString();
                    }
                }
                return "Unknown";
            }
        }
        public bool IsDisconnected
        {
            get { return !ConnectionStatus; }
        }
        public bool IsLocal
        {
            get { return settings.IsLocal; }
        }

        public void NewLiveReadingsPopUp(bool isMajor, Type mainClockItemId, Type secondayClockItemId)
        {
            var window = new LiveReadingWindow()
            {
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                BorderThickness = new Thickness(1),
                GlowBrush = null
            };
            window.SetResourceReference(MetroWindow.BorderBrushProperty, "AccentColorBrush");

            var shouldShowArc = !(mainClockItemId == typeof(LiveLAegClockItem));

            popOutWindows.AddLast(window);
            var model = new LiveReadingViewModel(isMajor,
                isMajor ?  settings.MajorInterval : settings.MinorInterval,
                isMajor ? settings.MajorDBLimit : settings.MinorDBLimit,
                mainClockItemId,
                secondayClockItemId,
                // Only the LAeq, 1 sek should not have the arcs
                shouldShowArc);
            var minor = minorIntervalHistory.GetLatests();
            if (minor != null)
            {
                model.OnMinor(DateTime.Now, minor.Item1, minor.Item2);
            }
            var major = majorIntervalHistory.GetLatests();
            if (major != null)
            {
                model.OnMajor(DateTime.Now, major.Item1, major.Item2);
            }
            model.OnNext(isMajor ? MajorClock.NextReadingTime : MinorClock.NextReadingTime);

            model.Title = Title;
            this.engine.RegisterListener(model);
            window.DataContext = model;
            window.Closed += (sender, args) =>
            {
                this.engine.UnRegisterListener(model);
                window.DataContext = null;
                popOutWindows.Remove(window);
                window = null;
            };
            window.Show();
        }

        public void NewGraphReadingsPopUp(string methodName)
        {
            var window = new LiveGraphWindow()
            {
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                BorderThickness = new Thickness(1),
                GlowBrush = null
            };
            window.SetResourceReference(MetroWindow.BorderBrushProperty, "AccentColorBrush");

            popOutWindows.AddLast(window);
            window.DataContext = this;
            var model = new LiveGraphWindowViewModel(methodName);
            model.Settings = Settings;
            model.Title = Title;
            this.engine.RegisterListener(model);
            window.DataContext = model;
            window.Closed += (sender, args) =>
            {
                this.engine.UnRegisterListener(model);
                window.DataContext = null;
                popOutWindows.Remove(window);
                window = null;
            };
            window.Show();
        }

        public void NewOctaveBandPopUp(OctaveBandWindowViewModel.OctaveBand band, bool building)
        {
            var window = new OctaveBandWindow()
            {
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                BorderThickness = new Thickness(1),
                GlowBrush = null
            };
            window.SetResourceReference(MetroWindow.BorderBrushProperty, "AccentColorBrush");

            popOutWindows.AddLast(window);
            var model = new OctaveBandWindowViewModel(settings, band, building);
            model.Title = Title;
            if (band == OctaveBandWindowViewModel.OctaveBand.OneOne)
            {
                model.Title += " - 1/1 Octave band.";
            }
            else
            {
                model.Title += " - 1/3 Octave band.";
            }
            this.engine.RegisterListener(model);
            window.DataContext = model;
            window.Closed += (sender, args) =>
            {
                this.engine.UnRegisterListener(model);
                window.DataContext = null;
                popOutWindows.Remove(window);
                window = null;
            };
            window.Show();
        }

        public void Close()
        {
            this.engine.Stop();
            if (popOutWindows.Count <= 0)
                return;

            logger.Debug("Closing all {0} pop-up windows.", popOutWindows.Count);
            while (popOutWindows.First != null)
            {
                logger.Trace("Closing pop-op window {0}.", popOutWindows.First);
                try
                {
                    var first = popOutWindows.First.Value;
                    first.Close();
                    // It should be removed by the even, but lets check just to be sure
                    if (popOutWindows.First.Value == first)
                    {
                        popOutWindows.RemoveFirst();
                    }
                }
                catch (Exception exp)
                {
                    logger.Error(exp, "Failed to close pop-op window.");
                }
            }
            // Clear to make sure everyting ise ok
            popOutWindows.Clear();
        }

        private bool _isEnabled;
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set {
                _isEnabled = value;
                OnPropertyChanged();

                if (MinorClock != null)
                    MinorClock.IsEnabled = value;
                if (MajorClock != null)
                    MajorClock.IsEnabled = value;
            }
        }

        private void OnDisplayReadingsTabel()
        {
            var window = new DataTabelWindow()
            {
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                BorderThickness = new Thickness(1),
                GlowBrush = null
            };
            window.SetResourceReference(MetroWindow.BorderBrushProperty, "AccentColorBrush");
            popOutWindows.AddLast(window);
            var model = new DataTabelViewModel(GetProject());
            model.OnSelected();
            model.Preloadreadings(MajorReadings.Select(x => x.ToInternal(true)).ToList(), MinorReadings.Select(x => x.ToInternal(false)).ToList());
            model.Title = Title;
            window.DataContext = model;
            window.Closed += (sender, args) =>
            {
                window.DataContext = null;
                popOutWindows.Remove(window);
                window = null;
            };
            window.Show();
        }

        private void OnDisplayReadingsGraph()
        {
            var window = new DataGraphWindow()
            {
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                BorderThickness = new Thickness(1),
                GlowBrush = null
            };
            window.SetResourceReference(MetroWindow.BorderBrushProperty, "AccentColorBrush");
            popOutWindows.AddLast(window);
            var model = new DataTabelViewModel(GetProject());
            model.OnSelected();
            model.Preloadreadings(MajorReadings.Select(x => x.ToInternal(true)).ToList(), MinorReadings.Select(x => x.ToInternal(false)).ToList());
            model.Title = Title;
            model.OnSelected();
            window.DataContext = model;
            window.Closed += (sender, args) =>
            {
                window.DataContext = null;
                popOutWindows.Remove(window);
                window = null;
            };
            window.Show();
        }

        private void OnDownloadAsCsv()
        {
            try
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.FileName = settings.ProjectName + ".xlsx";
                saveFileDialog.Filter = "Excel file (*.xlsx)|*.xlsx";
                if (saveFileDialog.ShowDialog() == true)
                {
                    var readingToSave = MajorReadings.Select(x => x.ToInternal(true))
                        .Union(MinorReadings.Select(x => x.ToInternal(false)))
                        .ToList();
                    var ordered = readingToSave.OrderBy(x => x.Time).ToList();
                    var excel = new ExcelExport(this.GetProject(), ordered);
                    excel.writeFile(saveFileDialog.FileName);
                }
            }
            catch (Exception exp)
            {
                logger.Error(exp, "Failed to save the readings as Excel.");
            }
        }

        private ObservableCollection<LiveGraphItemViewModel> _liveGraphReadings;
        public ObservableCollection<LiveGraphItemViewModel> LiveGraphReadings
        {
            get
            {
                if (_liveGraphReadings == null)
                {
                    _liveGraphReadings = new ObservableCollection<LiveGraphItemViewModel>();
                    _liveGraphReadings.Add(new LiveGraphItemViewModel(this, "LAeq, 1s", "LAeq"));
                    _liveGraphReadings.Add(new LiveGraphItemViewModel(this, "LCeq, 1s", "LCeq"));
                    _liveGraphReadings.Add(new LiveGraphItemViewModel(this, "LAMax", "LAMax"));
                    _liveGraphReadings.Add(new LiveGraphItemViewModel(this, "LAMin", "LAMin"));
                    _liveGraphReadings.Add(new LiveGraphItemViewModel(this, "LZMax", "LZMax"));
                    _liveGraphReadings.Add(new LiveGraphItemViewModel(this, "LZMin", "LZMin"));
                }
                return _liveGraphReadings;
            }
        }

        private Project GetProject()
        {
            return new Project()
            {
                Id = Guid.Empty,
                Created = started,
                MajorInterval = settings.MajorInterval,
                MajorDBLimit = settings.MajorDBLimit,
                MinorInterval = settings.MinorInterval,
                MinorDBLimit = settings.MinorDBLimit,
                Number = settings.ProjectNumber,
                Name = settings.ProjectName,
                Readings = MajorReadings.Count + MinorReadings.Count
            };
        }

        #region IMeterListener Members
        public Task OnMinor(DateTime time, DateTime starTime, ReadingData data)
        {
            return Task.Run(() =>
            {
                MinorReadings.Enqueue(new Tuple<DateTime, ReadingData>(starTime, data));
                DispatcherHelper.CheckBeginInvokeOnUI(() =>
                {
                    minorIntervalHistory.Add(starTime, data);
                    minorGraphViewModel.OnBarReading();
                });
            });
        }

        public Task OnMajor(DateTime time, DateTime starTime, ReadingData data)
        {
            return Task.Run(() =>
            {
                MajorReadings.Enqueue(new Tuple<DateTime, ReadingData>(starTime, data));
                DispatcherHelper.CheckBeginInvokeOnUI(() =>
                {
                    majorIntervalHistory.Add(starTime, data);
                    majorGraphViewModel.OnBarReading();
                });
            });
        }

        public Task OnSecond(DateTime time, DateTime starTime, ReadingData data, ReadingData minorData, ReadingData majorData)
        {
            return Task.Run(() =>
            {
                DispatcherHelper.CheckBeginInvokeOnUI(() =>
                {
                    readingHistory.Add(starTime, data);
                    minorGraphViewModel.OnLineReading();

                    // Add data for the octave bar
                    OctaveValues.Clear();
                    
                    if (minorData != null)
                    {
                        foreach (var obp in DecibelHelper.GetOneOneOctaveBand())
                        {
                            OctaveValues.Add(new OctaveBandGraphValue(obp.Display, minorData.GetValue("1-1-" + obp.Method), obp.LimitAjust, settings.MinorDBLimit));
                        }
                    }
                });
            });
        }

        public Task NextMinor(DateTime time)
        {
            return Task.FromResult<object>(null);
        }

        public Task NextMajor(DateTime time)
        {
            return Task.FromResult<object>(null);
        }
        #endregion

        public void SettingsChanged()
        {
            OnPropertyChanged(nameof(Settings));
            OnPropertyChanged(nameof(MinorDBLimit));
            if (minorClockViewModel != null)
            {
                minorClockViewModel.ChangeLimitDb(settings.MinorDBLimit);
            }
            OnPropertyChanged(nameof(MajorDBLimit));
            if (majorClockViewModel != null)
            {
                majorClockViewModel.ChangeLimitDb(settings.MajorDBLimit);
            }

            OnPropertyChanged(nameof(MinorInterval));
            OnPropertyChanged(nameof(MajorInterval));
        }

        public void OnMinorGraphSettingsChanged(string value)
        {
            minorGraphViewModel.ChangeDisplayItem(value, DecibelHelper.GetLimitOffSet(value));
            majorGraphViewModel.ChangeDisplayItem(value, DecibelHelper.GetLimitOffSet(value));
        }

        
    }
}