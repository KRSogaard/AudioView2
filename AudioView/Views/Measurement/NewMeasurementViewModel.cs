using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity.Infrastructure.Interception;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using AudioView.Annotations;
using AudioView.Common;
using AudioView.Common.Engine;
using AudioView.UserControls.CountDown;
using AudioView.Views.Measurement;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;
using NLog;
using Prism.Commands;
using Prism.Mvvm;

namespace AudioView.ViewModels
{
    public class NewMeasurementViewModel : BindableBase
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private bool testFailed;
        private int DefaultPort = 13674;
        private MeasurementsViewModel MainViewModel { get; set; }
        private MeasurementSettings remoteSettings;

        public NewMeasurementViewModel(MeasurementsViewModel mainViewModel)
        {
            PropertyChanged += (sender, args) =>
            {
                logger.Trace("NewMeasurementViewModel {0} was change", args.PropertyName);
            };

            MainViewModel = mainViewModel;
            ProjectName = "Untitled - " + DateTime.Now.ToString("yyyy-mm-dd hh-mm-ss");
            UseLocal = true;
            IsRemoteTested = false;
            LocalDevices = new ObservableCollection<string>();

            MajorIntervalSeconds = 0.ToString();
            MinorIntervalSeconds = 0.ToString();
            MajorIntervalMinutes = 15.ToString();
            MinorIntervalMinutes = 1.ToString();
            MajorIntervalHours = 0.ToString();
            MinorIntervalHours = 0.ToString();

            GraphBoundUpper = 120.ToString();
            GraphBoundLower = 60.ToString();

            MinorDBLimit = 90.ToString();
            MajorDBLimit = 90.ToString();
            ClockItems = UserControls.CountDown.ClockItems.Get;

            MinorClockMainItem = ClockItems.FirstOrDefault(x => x.Id == 3);
            MinorClockSecondaryItem = ClockItems.FirstOrDefault(x => x.Id == 0);
            MajorClockMainItem = ClockItems.FirstOrDefault(x => x.Id == 3);
            MajorClockSecondaryItem = ClockItems.FirstOrDefault(x => x.Id == 2);

            RemoteIpAddress = "localhost";
            RemotePort = DefaultPort.ToString();

            //// Find an avalible port
            while (mainViewModel.Measurements.Any(x => x.Settings.Port == DefaultPort))
            {
                logger.Trace("Port {0} is in use, trying next.", DefaultPort);
                DefaultPort++;
            }
            ListenPort = DefaultPort.ToString();
        }

        public IList<ClockItem> ClockItems { get; set; }

        private ClockItem _minorClockMainItem;
        public ClockItem MinorClockMainItem
        {
            get { return _minorClockMainItem; }
            set
            {
                SetProperty(ref _minorClockMainItem, value);
            }
        }
        private ClockItem _minorClockSecondaryItem;
        public ClockItem MinorClockSecondaryItem
        {
            get { return _minorClockSecondaryItem; }
            set { SetProperty(ref _minorClockSecondaryItem, value); }
        }
        private ClockItem _majorClockMainItem;
        public ClockItem MajorClockMainItem
        {
            get { return _majorClockMainItem; }
            set { SetProperty(ref _majorClockMainItem, value); }
        }
        private ClockItem _majorClockSecondaryItem;
        public ClockItem MajorClockSecondaryItem
        {
            get { return _majorClockSecondaryItem; }
            set { SetProperty(ref _majorClockSecondaryItem, value); }
        }

        private bool _isRemoteTested;
        private bool IsRemoteTested
        {
            get { return _isRemoteTested; }
            set { SetProperty(ref _isRemoteTested, value); }
        }

        private int _listenPort;
        public string ListenPort
        {
            get { return _listenPort.ToString(); }
            set
            {
                int tryParse;
                if (int.TryParse(value, out tryParse) && tryParse >= 0)
                {
                    SetProperty(ref _listenPort, tryParse);
                }
            }
        }

        private string _projectName;
        public string ProjectName
        {
            get { return _projectName; }
            set
            {
                SetProperty(ref _projectName, value);
            }
        }

        private string _projectNumber;
        public string ProjectNumber
        {
            get { return _projectNumber; }
            set
            {
                SetProperty(ref _projectNumber, value);
            }
        }

        private string _remoteIpAddress;
        public string RemoteIpAddress
        {
            get { return _remoteIpAddress; }
            set
            {
                IsRemoteTested = false;
                SetProperty(ref _remoteIpAddress, value);
            }
        }

        private string _remotePort;
        public string RemotePort
        {
            get { return _remotePort; }
            set
            {
                IsRemoteTested = false;
                SetProperty(ref _remotePort, value);
            }
        }

        private bool _useLocal;
        public bool UseLocal
        {
            get { return _useLocal; }
            set
            {
                _useLocal = value;
                _useRemote = !value;
                OnPropertyChanged();
                OnPropertyChanged("UseRemote");
            }
        }

        private bool _useRemote;
        public bool UseRemote
        {
            get { return _useRemote; }
            set
            {
                _useRemote = value;
                _useLocal = !value;
                OnPropertyChanged();
                OnPropertyChanged("UseLocal");
            }
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get { return _isLoading; }
            set
            {
                SetProperty(ref _isLoading, value);
            }
        }

        private bool _isTesting;
        public bool IsTesting
        {
            get { return _isTesting; }
            set
            {
                SetProperty(ref _isTesting, value);
            }
        }

        private int _minorIntervalSeconds;
        public string MinorIntervalSeconds
        {
            get { return _minorIntervalSeconds.ToString(); }
            set
            {
                int tryParse;
                if (int.TryParse(value, out tryParse) && tryParse >= 0)
                {
                    if (tryParse > 60)
                        tryParse = 60;
                    if (tryParse < 0)
                        tryParse = 0;
                    SetProperty(ref _minorIntervalSeconds, tryParse);
                }
            }
        }

        private int _majorIntervalSeconds;
        public string MajorIntervalSeconds
        {
            get { return _majorIntervalSeconds.ToString(); }
            set
            {
                int tryParse;
                if (int.TryParse(value, out tryParse) && tryParse >= 0)
                {
                    if (tryParse > 60)
                        tryParse = 60;
                    if (tryParse < 0)
                        tryParse = 0;
                    SetProperty(ref _majorIntervalSeconds, tryParse);
                }
            }
        }

        private int _minorIntervalMinutes;
        public string MinorIntervalMinutes
        {
            get { return _minorIntervalMinutes.ToString(); }
            set
            {
                int tryParse;
                if (int.TryParse(value, out tryParse) && tryParse >= 0)
                {
                    if (tryParse > 60)
                        tryParse = 60;
                    if (tryParse < 0)
                        tryParse = 0;
                    SetProperty(ref _minorIntervalMinutes, tryParse);
                }
            }
        }

        private int _majorIntervalMinutes;
        public string MajorIntervalMinutes
        {
            get { return _majorIntervalMinutes.ToString(); }
            set
            {
                int tryParse;
                if (int.TryParse(value, out tryParse) && tryParse >= 0)
                {
                    if (tryParse > 60)
                        tryParse = 60;
                    if (tryParse < 0)
                        tryParse = 0;
                    SetProperty(ref _majorIntervalMinutes, tryParse);
                }
            }
        }

        private int _minorIntervalHours;
        public string MinorIntervalHours
        {
            get { return _minorIntervalHours.ToString(); }
            set
            {
                int tryParse;
                if (int.TryParse(value, out tryParse) && tryParse >= 0)
                {
                    if (tryParse > 24)
                        tryParse = 24;
                    if (tryParse < 0)
                        tryParse = 0;
                    SetProperty(ref _minorIntervalHours, tryParse);
                }
            }
        }

        private int _majorIntervalHours;
        public string MajorIntervalHours
        {
            get { return _majorIntervalHours.ToString(); }
            set
            {
                int tryParse;
                if (int.TryParse(value, out tryParse) && tryParse >= 0)
                {
                    if (tryParse > 24)
                        tryParse = 24;
                    if (tryParse < 0)
                        tryParse = 0;
                    SetProperty(ref _majorIntervalHours, tryParse);
                }
            }
        }

        private int _graphBoundLower;
        public string GraphBoundLower
        {
            get { return _graphBoundLower.ToString(); }
            set
            {
                int tryParse;
                if (int.TryParse(value, out tryParse) && tryParse < _graphBoundUpper)
                {
                    SetProperty(ref _graphBoundLower, tryParse);
                }
                MinorDBLimit = MinorDBLimit;
                MajorDBLimit = MajorDBLimit;
            }
        }

        private int _graphBoundUpper;
        public string GraphBoundUpper
        {
            get { return _graphBoundUpper.ToString(); }
            set
            {
                int tryParse;
                if (int.TryParse(value, out tryParse) && tryParse > _graphBoundLower)
                {
                    SetProperty(ref _graphBoundUpper, tryParse);
                }
                MinorDBLimit = MinorDBLimit;
                MajorDBLimit = MajorDBLimit;
            }
        }

        private int _MinordBLimit;
        public string MinorDBLimit
        {
            get { return _MinordBLimit.ToString(); }
            set
            {
                int tryParse;
                if (int.TryParse(value, out tryParse) && tryParse >= 10)
                {
                    SetProperty(ref _MinordBLimit, Math.Max(_graphBoundLower, Math.Min(_graphBoundUpper, tryParse)));
                }
            }
        }

        private int _MajordBLimit;
        public string MajorDBLimit
        {
            get { return _MajordBLimit.ToString(); }
            set
            {
                int tryParse;
                if (int.TryParse(value, out tryParse) && tryParse >= 10)
                {
                    SetProperty(ref _MajordBLimit, Math.Max(_graphBoundLower, Math.Min(_graphBoundUpper, tryParse)));
                }
            }
        }

        public MeasurementSettings GetSettings()
        {
            logger.Trace("Creating setting object.");
            return new MeasurementSettings()
            {
                ProjectName = ProjectName,
                ProjectNumber = ProjectNumber,
                BarsDisplayed = 15,
                MinorDBLimit = _MinordBLimit,
                MajorDBLimit = _MajordBLimit,
                GraphLowerBound = _graphBoundLower,
                GraphUpperBound = _graphBoundUpper,
                MajorClockMainItemId = MajorClockMainItem.Id,
                MajorClockSecondaryItemId = MajorClockSecondaryItem.Id,
                MinorClockMainItemId = MinorClockMainItem.Id,
                MinorClockSecondaryItemId = MinorClockSecondaryItem.Id,
                MajorInterval = new TimeSpan(_majorIntervalHours, _majorIntervalMinutes, _majorIntervalSeconds),
                MinorInterval = new TimeSpan(_minorIntervalHours, _minorIntervalMinutes, _minorIntervalSeconds),
                Port = int.Parse(ListenPort),
                IsLocal = UseLocal
            };
        }

        private ICommand _startTest;
        public ICommand StartTest
        {
            get
            {
                if (_startTest == null)
                {
                    _startTest = new DelegateCommand(() =>
                    {
                        logger.Debug("Starting remote test.");
                        IsTesting = true;
                        IsRemoteTested = false;

                        TestDevice().ContinueWith(result =>
                        {
                            logger.Debug("Parsing remote test result to UI thread.");
                            DispatcherHelper.CheckBeginInvokeOnUI(() => GotTest(result.Result));
                        });
                    }, () =>
                    {
                        return !IsRemoteTested && !IsTesting;
                    })
                    .ObservesProperty(() => IsRemoteTested)
                    .ObservesProperty(() => IsTesting);
                }
                return _startTest;
            }
        }

        private ICommand _searchForDevices;
        public ICommand SearchForDevices
        {
            get
            {
                if (_searchForDevices == null)
                {
                    _searchForDevices = new DelegateCommand(() =>
                    {
                        IsLoading = true;

                        GetConnectedDevices().ContinueWith(result =>
                        {
                            logger.Trace("Got connected local devices, sending to UI thread.");
                            DispatcherHelper.CheckBeginInvokeOnUI(() =>
                            {
                                GotDevices(result.Result);
                            });
                        });
                    });
                }
                return _searchForDevices;
            }
        }

        private ICommand _startMeasurement;
        public ICommand StartMeasurement
        {
            get
            {
                if (_startMeasurement == null)
                {
                    _startMeasurement = new DelegateCommand(() =>
                    {
                        try
                        {
                            logger.Debug("Adding new measurement to the main view model.");
                            MeasurementViewModel newModel;
                            var settings = GetSettings();
                            IMeterReader reader;
                            if (UseLocal)
                            {
                                reader = new MockMeterReader();
                            }
                            else
                            {
                                reader = new RemoteMeterReader(RemoteIpAddress, int.Parse(RemotePort));
                                settings.ProjectName = remoteSettings.ProjectName;
                                settings.ProjectNumber = remoteSettings.ProjectNumber;
                                settings.MajorInterval = remoteSettings.MajorInterval;
                                settings.MinorInterval = remoteSettings.MinorInterval;
                                settings.MinorDBLimit = remoteSettings.MinorDBLimit;
                                settings.MajorDBLimit = remoteSettings.MajorDBLimit;
                            }

                            newModel = new MeasurementViewModel(Guid.NewGuid(), settings, reader);

                            MainViewModel.AddNewMeasurement(newModel);
                        }
                        catch (Exception exp)
                        {
                            logger.Error(exp, "Failed to start measurement.");
                        }
                    }, () =>
                    {
                        if (UseLocal && (IsLoading || SelectedLocalDevice == null))
                            return false;
                        if (UseRemote && (IsTesting || !IsRemoteTested))
                            return false;
                        return true;
                    })
                    .ObservesProperty(() => UseRemote)
                    .ObservesProperty(() => UseLocal)
                    .ObservesProperty(() => IsLoading)
                    .ObservesProperty(() => SelectedLocalDevice)
                    .ObservesProperty(() => IsTesting)
                    .ObservesProperty(() => IsRemoteTested);
                }
                return _startMeasurement;
            }
        }

        private void GotTest(bool result)
        {
            logger.Debug("Remote test result {0}.", result);
            IsTesting = false;
            testFailed = result;
            IsRemoteTested = result;
        }

        private ObservableCollection<string> _localDevices;
        public ObservableCollection<string> LocalDevices
        {
            get { return _localDevices; }
            set { SetProperty(ref _localDevices, value); }
        }

        private string _selectedLocalDevice;

        public string SelectedLocalDevice
        {
            get { return _selectedLocalDevice; }
            set { SetProperty(ref _selectedLocalDevice, value); }
        }

        private Task<List<string>> GetConnectedDevices()
        {
            return Task.Factory.StartNew(() =>
            {
                logger.Debug("Get connect local devices.");
                Task.Delay(new TimeSpan(0, 0, 1)).Wait();
                return new List<string>()
                {
                    "Test Device 1",
                    "Test Device 2"
                };
            });
        }
        private void GotDevices(List<string> devices)
        {
            IsLoading = false;
            LocalDevices.Clear();
            foreach (var device in devices)
            {
                LocalDevices.Add(device);
            }
            SelectedLocalDevice = LocalDevices.FirstOrDefault();
        }

        private Task<bool> TestDevice()
        {
            return Task.Factory.StartNew(() =>
            {
                logger.Debug("Testing remote device {0}:{1}", RemoteIpAddress, RemotePort);
                var settings = RemoteMeterReader.TestConenction(RemoteIpAddress, int.Parse(RemotePort)).Result;
                this.remoteSettings = settings;
                return settings != null;
            });
        }

        public string Error { get; }
    }
}
