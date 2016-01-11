using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using AudioView.Annotations;
using AudioView.Common;
using AudioView.UserControls.CountDown;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using Prism.Mvvm;

namespace AudioView.ViewModels
{
    public class NewMeasurementViewModel : BindableBase
    {
        private int DefaultPort = 13674;
        private bool isRemoteTested { get; set; }
        private MainViewModel MainViewModel { get; set; }
        private MainViewModel mainViewModel;

        public NewMeasurementViewModel(MainViewModel mainViewModel)
        {
            MainViewModel = mainViewModel;
            ProjectName = "Untitled - " + DateTime.Now.ToString("yyyy-mm-dd hh-mm-ss");
            UseLocal = true;
            IsLoading = true;
            isRemoteTested = false;
            LocalDevices = new ObservableCollection<string>();

            MajorIntervalSeconds = 0.ToString();
            MinorIntervalSeconds = 0.ToString();
            MajorIntervalMinutes = 15.ToString();
            MinorIntervalMinutes = 1.ToString();
            MajorIntervalHours = 0.ToString();
            MinorIntervalHours = 0.ToString();

            GraphBoundLower = 30.ToString();
            GraphBoundUpper = 150.ToString();

            DBLimit = 90.ToString();
            ClockItems = AudioView.UserControls.CountDown.ClockItems.Get;

            MinorClockMainItem = ClockItems.FirstOrDefault(x => x.Id == 1);
            MinorClockSecondaryItem = ClockItems.FirstOrDefault(x => x.Id == 0);
            MajorClockMainItem = ClockItems.FirstOrDefault(x => x.Id == 1);
            MajorClockSecondaryItem = ClockItems.FirstOrDefault(x => x.Id == 2);

            // Find an avalible port
            while (mainViewModel.Measurements.Any(x => x.Settings.Port == DefaultPort))
            {
                DefaultPort++;
            }
            ListenPort = DefaultPort.ToString();

            GetConnectedDevices().ContinueWith(result =>
            {
                Execute.OnUIThread(() => GotDevices(result.Result));
            });
            this.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "RemotePort" || args.PropertyName == "RemoteIpAddress")
                {
                    CanTest = true;
                    isRemoteTested = false;
                    OnPropertyChanged("CanStart");
                }
            };
        }

        public IList<ClockItem> ClockItems { get; set; }

        private ClockItem _minorClockMainItem;
        public ClockItem MinorClockMainItem
        {
            get { return _minorClockMainItem; }
            set { SetProperty(ref _minorClockMainItem, value); }
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
                OnPropertyChanged(); }
        }

        private string _projectName;
        public string ProjectName
        {
            get { return _projectName; }
            set { SetProperty(ref _projectName, value); }
        }

        private string _remoteIpAddress;
        public string RemoteIpAddress
        {
            get { return _remoteIpAddress; }
            set { SetProperty(ref _remoteIpAddress, value); }
        }

        private string _remotePort;
        public string RemotePort
        {
            get { return _remotePort; }
            set { SetProperty(ref _remotePort, value); }
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
            set { SetProperty(ref _isLoading, value); }
        }

        private bool _isTesting;
        public bool IsTesting
        {
            get { return _isTesting; }
            set { SetProperty(ref _isTesting, value); }
        }

        private bool _canTest;
        public bool CanTest
        {
            get { return _canTest; }
            set { SetProperty(ref _canTest, value); }
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
                    SetProperty(ref _minorIntervalSeconds, tryParse);
                }
                OnPropertyChanged();
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
                    SetProperty(ref _majorIntervalSeconds, tryParse);
                }
                OnPropertyChanged();
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
                    SetProperty(ref _minorIntervalMinutes, tryParse);
                }
                OnPropertyChanged();
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
                    SetProperty(ref _majorIntervalMinutes, tryParse);
                }
                OnPropertyChanged();
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
                    SetProperty(ref _minorIntervalHours, tryParse);
                }
                OnPropertyChanged();
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
                    SetProperty(ref _majorIntervalHours, tryParse);
                }
                OnPropertyChanged();
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
                DBLimit = DBLimit;
                OnPropertyChanged();
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
                DBLimit = DBLimit;
            }
        }

        private int _dBLimit;
        public string DBLimit
        {
            get { return _dBLimit.ToString(); }
            set
            {
                int tryParse;
                if(int.TryParse(value, out tryParse) && tryParse >= 10)
                {
                    SetProperty(ref _dBLimit, Math.Max(_graphBoundLower, Math.Min(_graphBoundUpper, tryParse)));
                }
            }
        }

        public MeasurementSettings GetSettings()
        {
            return new MeasurementSettings()
            {
                ProjectName = ProjectName,
                BarsDisplayed = 15,
                DBLimit = _dBLimit,
                GraphLowerBound = _graphBoundLower,
                GraphUpperBound = _graphBoundUpper,
                MajorClockMainItemId = MajorClockMainItem.Id,
                MajorClockSecondaryItemId = MajorClockSecondaryItem.Id,
                MinorClockMainItemId = MinorClockMainItem.Id,
                MinorClockSecondaryItemId = MinorClockSecondaryItem.Id,
                MajorInterval = new TimeSpan(_majorIntervalHours, _majorIntervalMinutes, _majorIntervalSeconds),
                MinorInterval = new TimeSpan(_minorIntervalHours, _minorIntervalMinutes, _minorIntervalSeconds),
                Port = int.Parse(ListenPort)
            };
        }

        public ICommand StartTest
        {
            get
            {
                return new RelayCommand(() =>
                {
                    IsTesting = true;
                    isRemoteTested = false;
                    OnPropertyChanged("CanStart");
                    TestDevice().ContinueWith(result =>
                    {
                        Execute.OnUIThread(() => GotTest(result.Result));
                    });
                });
            }
        }

        public ICommand StartMeasurement
        {
            get
            {
                return new RelayCommand(() =>
                {
                    MainViewModel.AddNewMeasurement();
                });
            }
        }

        private void GotTest(bool result)
        {
            IsTesting = false;
            isRemoteTested = result;
            CanTest = !result;
            OnPropertyChanged("CanStart");
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
                Task.Delay(new TimeSpan(0, 0, 1)).Wait();
                return new List<string>()
                {
                    "Device 1",
                    "Device 2"
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
            
            OnPropertyChanged("LocalDevices");
            OnPropertyChanged("CanStart");
        }

        private Task<bool> TestDevice()
        {
            return Task.Factory.StartNew(() =>
            {
                Task.Delay(new TimeSpan(0, 0, 10)).Wait();
                return true;
            });
        }

        public bool CanStart
        {
            get
            {
                if (UseLocal && (IsLoading || SelectedLocalDevice == null))
                    return false;
                if (UseRemote && (IsTesting || !isRemoteTested))
                    return false;
                return true;
            }
        }
    }
}
