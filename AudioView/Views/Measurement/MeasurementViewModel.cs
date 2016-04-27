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
using AudioView.Common.Listeners;
using AudioView.UserControls.CountDown;
using AudioView.Views.History;
using AudioView.Views.Measurement;
using AudioView.Views.PopOuts;
using GalaSoft.MvvmLight.Threading;
using MahApps.Metro.Controls;
using Microsoft.Win32;
using NLog;
using Prism.Commands;
using Prism.Mvvm;

namespace AudioView.ViewModels
{
    public class MeasurementViewModel : BindableBase, IMeterListener
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private LinkedList<MetroWindow> popOutWindows;
        public AudioViewEngine engine { get; set; }
        private MeasurementSettings settings;
        private DataStorageMeterListener dataStorage;
        private DateTime started;
        private TCPServerListener tcpServer;
        private bool ConnectionStatus;

        private ConcurrentQueue<Tuple<DateTime,ReadingData>> MinorReadings;
        private ConcurrentQueue<Tuple<DateTime, ReadingData>> MajorReadings;
        private ObservableCollection<Tuple<DateTime, double>> barMinorValues;
        public ObservableCollection<Tuple<DateTime, double>> BarMinorValues
        {
            get { return barMinorValues; }
            set { SetProperty(ref barMinorValues, value); }
        }
        private ObservableCollection<Tuple<DateTime, double>> barMajorValues;
        public ObservableCollection<Tuple<DateTime, double>> BarMajorValues
        {
            get { return barMajorValues; }
            set { SetProperty(ref barMajorValues, value); }
        }
        private ObservableCollection<Tuple<DateTime, double>> lineValues;
        public ObservableCollection<Tuple<DateTime, double>> LineValues
        {
            get { return lineValues; }
            set { SetProperty(ref lineValues, value); }
        }
        private TimeSpan minorSpan;
        public TimeSpan MinorSpan
        {
            get { return minorSpan; }
            set { SetProperty(ref minorSpan, value); }
        }
        private TimeSpan majorSpan;
        public TimeSpan MajorSpan
        {
            get { return majorSpan; }
            set { SetProperty(ref majorSpan, value); }
        }
        
        public MeasurementViewModel(Guid id, MeasurementSettings settings, IMeterReader reader)
        {
            MinorReadings = new ConcurrentQueue<Tuple<DateTime, ReadingData>>();
            MajorReadings = new ConcurrentQueue<Tuple<DateTime, ReadingData>>();
            BarMajorValues = new ObservableCollection<Tuple<DateTime, double>>();
            BarMinorValues = new ObservableCollection<Tuple<DateTime, double>>();
            LineValues = new ObservableCollection<Tuple<DateTime, double>>();
            MinorSpan = TimeSpan.FromTicks(settings.MinorInterval.Ticks * 15);
            MajorSpan = TimeSpan.FromTicks(settings.MajorInterval.Ticks * 15);


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

            this.engine.RegisterListener(this);
            this.engine.EngineStartDelayedEvent += (waitTime) =>
            {
                MinorClock = new AudioViewCountDownViewModel(false,
                        settings.MinorInterval,
                        settings.MinorDBLimit,
                        settings.MinorClockMainItemId,
                        settings.MinorClockSecondaryItemId);
                MajorClock = new AudioViewCountDownViewModel(true,
                        settings.MajorInterval,
                        settings.MajorDBLimit,
                        settings.MajorClockMainItemId,
                        settings.MajorClockSecondaryItemId);
                MinorClock.NextMinor(DateTime.Now + waitTime);
                MinorClock.NextMajor(DateTime.Now + waitTime);
                MajorClock.NextMinor(DateTime.Now + waitTime);
                MajorClock.NextMajor(DateTime.Now + waitTime);
            };
            this.engine.EngineStartedEvent += () =>
            {
                this.engine.RegisterListener(MinorClock);
                this.engine.RegisterListener(MajorClock);
            };
            this.engine.Start();

            Title = settings.ProjectName;
        }

        private string title;
        public string Title
        {
            get { return title; }
            set { title = value; }
        }

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

        private AudioViewCountDownViewModel minorClockViewModel;
        public AudioViewCountDownViewModel MinorClock
        {
            get { return minorClockViewModel; }
            set { SetProperty(ref minorClockViewModel, value); }
        }

        private AudioViewCountDownViewModel majorClockViewModel;
        public AudioViewCountDownViewModel MajorClock
        {
            get { return majorClockViewModel; }
            set { SetProperty(ref majorClockViewModel, value); }
        }

        public void NewLiveReadingsPopUp(bool isMajor, int mainClockItemId, int secondayClockItemId)
        {
            var window = new LiveReadingWindow()
            {
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                BorderThickness = new Thickness(1),
                GlowBrush = null
            };
            window.SetResourceReference(MetroWindow.BorderBrushProperty, "AccentColorBrush");

            popOutWindows.AddLast(window);
            var model = new LiveReadingViewModel(isMajor,
                isMajor ?  settings.MajorInterval : settings.MinorInterval,
                isMajor ? settings.MajorDBLimit : settings.MinorDBLimit,
                mainClockItemId,
                secondayClockItemId);
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

        private ICommand _displayReadingsTabel;
        public ICommand DisplayReadingsTabel
        {
            get
            {
                if (_displayReadingsTabel == null)
                {
                    _displayReadingsTabel = new DelegateCommand(() =>
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
                    });
                }
                return _displayReadingsTabel;
            }
        }

        private ICommand _displayReadingsGraph;
        public ICommand DisplayReadingsGraph
        {
            get
            {
                if (_displayReadingsGraph == null)
                {
                    _displayReadingsGraph = new DelegateCommand(() =>
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
                    });
                }
                return _displayReadingsGraph;
            }
        }

        private ICommand _downloadAsCSV;
        public ICommand DownloadAsCSV
        {
            get
            {
                if (_downloadAsCSV == null)
                {
                    _downloadAsCSV = new DelegateCommand(() =>
                    {
                        try
                        {
                            SaveFileDialog saveFileDialog = new SaveFileDialog();
                            saveFileDialog.FileName = settings.ProjectName + ".csv";
                            saveFileDialog.Filter = "CSV file (*.csv)|*.csv";
                            if (saveFileDialog.ShowDialog() == true)
                            {
                                var readingToSave = MajorReadings.Select(x => x.ToInternal(true))
                                    .Union(MinorReadings.Select(x => x.ToInternal(false)))
                                    .ToList();
                                var ordered = readingToSave.OrderBy(x => x.Time).ToList();
                                File.WriteAllText(saveFileDialog.FileName, Reading.CSV(ordered));
                            }
                        }
                        catch (Exception exp)
                        {
                            logger.Error(exp, "Failed to save the readinds as CSV.");
                        }
                    });
                }
                return _downloadAsCSV;
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
                    foreach (var method in typeof(ReadingData).GetMethods().Where(
                        x => x.IsPublic &&
                        x.ReturnType == typeof(double)))
                    {
                        _liveGraphReadings.Add(new LiveGraphItemViewModel(this, method));
                    }
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
        public Task OnMinor(DateTime time, ReadingData data)
        {
            return Task.Run(() =>
            {
                MinorReadings.Enqueue(new Tuple<DateTime, ReadingData>(time, data));
                DispatcherHelper.CheckBeginInvokeOnUI(() =>
                {
                    BarMinorValues.Add(new Tuple<DateTime, double>(time, data.LAeq));
                });
            });
        }

        public Task OnMajor(DateTime time, ReadingData data)
        {
            return Task.Run(() =>
            {
                MajorReadings.Enqueue(new Tuple<DateTime, ReadingData>(time, data));
                DispatcherHelper.CheckBeginInvokeOnUI(() =>
                {
                    BarMajorValues.Add(new Tuple<DateTime, double>(time, data.LAeq));
                });
            });
        }

        public Task OnSecond(DateTime time, ReadingData data, ReadingData minorData, ReadingData majorData)
        {
            return Task.Run(() =>
            {
                DispatcherHelper.CheckBeginInvokeOnUI(() =>
                {
                    LineValues.Add(new Tuple<DateTime, double>(time, data.LAeq));
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
    }
}