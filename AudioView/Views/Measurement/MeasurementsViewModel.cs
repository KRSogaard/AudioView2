using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using AudioView.Common;
using AudioView.Common.Engine;
using AudioView.UserControls.CountDown.ClockItems;
using AudioView.ViewModels;
using AudioView.Views.PopOuts;
using NLog;
using Prism.Commands;
using Prism.Mvvm;

namespace AudioView.Views.Measurement
{
    public class MeasurementsViewModel : BindableBase
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public MainViewModel MainViewModel { get; set; }

        public MeasurementsViewModel(MainViewModel mainViewModel)
        {
            MainViewModel = mainViewModel;
            Measurements = new ObservableCollection<MeasurementViewModel>();
            SelectedMeasurement = null;
            PropertyChanged += OnPropertyChanged;
        }
        
        private ObservableCollection<MeasurementViewModel> measurements;
        public ObservableCollection<MeasurementViewModel> Measurements
        {
            get { return measurements; }
            set { SetProperty(ref measurements, value); }
        }

        private MeasurementViewModel _selectedMeasurement;
        public MeasurementViewModel SelectedMeasurement
        {
            get { return _selectedMeasurement; }
            set
            {
                _selectedMeasurement = value;
                foreach (var model in Measurements)
                {
                    model.IsEnabled = false;
                }
                if (SelectedMeasurement != null)
                {
                    SelectedMeasurement.IsEnabled = true;
                }
                OnPropertyChanged();
                OnPropertyChanged(nameof(MeasurementSelected));
            }
        }

        public bool MeasurementSelected
        {
            get { return SelectedMeasurement != null; }
        }

        
        public LiveMeasurementSettingsViewModel _changeSettingsViewModel;
        public LiveMeasurementSettingsViewModel ChangeSettingsViewModel
        {
            get { return _changeSettingsViewModel; }
            set { SetProperty(ref _changeSettingsViewModel, value); }
        }

        public NewMeasurementViewModel _newViewModel;
        public NewMeasurementViewModel NewViewModel
        {
            get { return _newViewModel; }
            set { SetProperty(ref _newViewModel, value); }
        }

        public void AddNewMeasurement(MeasurementViewModel newModel)
        {
            NewViewModel = null;
            ShowNew = false;
            ShowSettings = false;

            Measurements.Add(newModel);
            if (SelectedMeasurement == null)
            {
                SelectedMeasurement = newModel;
                SelectedMeasurement.IsEnabled = true;
            }
        }

        private ICommand _newMeasurementCommand;
        public ICommand NewMeasurementCommand
        {
            get
            {
                if (_newMeasurementCommand == null)
                {
                    _newMeasurementCommand = new DelegateCommand(() =>
                    {
                        NewViewModel = new NewMeasurementViewModel(this);
                        ShowNew = true;
                    });
                }
                return _newMeasurementCommand;
            }
        }
        
        private ICommand _changeSettingstCommand;
        public ICommand ChangeSettingsCommand
        {
            get
            {
                if (_changeSettingstCommand == null)
                {
                    _changeSettingstCommand = new DelegateCommand(() =>
                    {
                        ChangeSettingsViewModel = new LiveMeasurementSettingsViewModel(this, SelectedMeasurement);
                        ShowSettings = true;
                    });
                }
                return _changeSettingstCommand;
            }
        }
        
        private ICommand _starNewMeasurementCommand;
        public ICommand StarNewMeasurementCommand
        {
            get
            {
                if (_starNewMeasurementCommand == null)
                {
                    _starNewMeasurementCommand = new DelegateCommand(() =>
                    {
                        var newModel = new MeasurementViewModel(Guid.NewGuid(), new MeasurementSettings()
                        {
                            BarsDisplayed = 10,
                            GraphLowerBound = 60,
                            GraphUpperBound = 150,
                            IsLocal = true,
                            MajorInterval = new TimeSpan(0, 0, 0, 25),
                            MajorClockSecondaryItem = typeof(TimeToIntervalClockItem),
                            MajorClockMainItem = typeof(BuildingReadingClockItem),
                            MinorInterval = new TimeSpan(0,0,0,5),
                            MinorClockMainItem = typeof(BuildingReadingClockItem),
                            MinorClockSecondaryItem = typeof(LatestIntervalClockItem)
                        }, new MockMeterReader());
                        AddNewMeasurement(newModel);
                    });
                }
                return _starNewMeasurementCommand;
            }
        }

        private ICommand _octaveBandOneOnePopUp;
        public ICommand OctaveBandOneOnePopUp
        {
            get
            {
                if (_octaveBandOneOnePopUp == null)
                {
                    _octaveBandOneOnePopUp = new DelegateCommand(() =>
                    {
                        SelectedMeasurement.NewOctaveBandPopUp(OctaveBandWindowViewModel.OctaveBand.OneOne);
                    }, () =>
                    {
                        return SelectedMeasurement != null;
                    }).ObservesProperty(() => SelectedMeasurement);
                }
                return _octaveBandOneOnePopUp;
            }
        }

        private ICommand _octaveBandOneThirdPopUp;
        public ICommand OctaveBandOneThirdPopUp
        {
            get
            {
                if (_octaveBandOneThirdPopUp == null)
                {
                    _octaveBandOneThirdPopUp = new DelegateCommand(() =>
                    {
                        SelectedMeasurement.NewOctaveBandPopUp(OctaveBandWindowViewModel.OctaveBand.OneThird);
                    }, () =>
                    {
                        return SelectedMeasurement != null;
                    }).ObservesProperty(() => SelectedMeasurement);
                }
                return _octaveBandOneThirdPopUp;
            }
        }

        private ICommand _readingsPopUpLive;
        public ICommand ReadingsPopUpLive
        {
            get
            {
                if (_readingsPopUpLive == null)
                {
                    _readingsPopUpLive = new DelegateCommand(() =>
                    {
                        SelectedMeasurement.NewLiveReadingsPopUp(false, typeof(LiveLAegClockItem), typeof(InactiveClockItem));
                    }, () =>
                    {
                        return SelectedMeasurement != null;
                    }).ObservesProperty(() => SelectedMeasurement);
                }
                return _readingsPopUpLive;
            }
        }

        private ICommand _readingsPopUpLatestMajor;
        public ICommand ReadingsPopUpLatestMajor
        {
            get
            {
                if (_readingsPopUpLatestMajor == null)
                {
                    _readingsPopUpLatestMajor = new DelegateCommand(() =>
                    {
                        SelectedMeasurement.NewLiveReadingsPopUp(true, typeof(LatestIntervalClockItem), typeof(InactiveClockItem));
                    }, () =>
                    {
                        return SelectedMeasurement != null;
                    }).ObservesProperty(() => SelectedMeasurement);
                }
                return _readingsPopUpLatestMajor;
            }
        }

        private ICommand _readingsPopUpLatestMinor;
        public ICommand ReadingsPopUpLatestMinor
        {
            get
            {
                if (_readingsPopUpLatestMinor == null)
                {
                    _readingsPopUpLatestMinor = new DelegateCommand(() =>
                    {
                        SelectedMeasurement.NewLiveReadingsPopUp(false, typeof(LatestIntervalClockItem), typeof(InactiveClockItem));
                    }, () =>
                    {
                        return SelectedMeasurement != null;
                    }).ObservesProperty(() => SelectedMeasurement);
                }
                return _readingsPopUpLatestMinor;
            }
        }

        private ICommand _readingsPopUpLatestBuildingMajor;
        public ICommand ReadingsPopUpLatestBuildingMajor
        {
            get
            {
                if (_readingsPopUpLatestBuildingMajor == null)
                {
                    _readingsPopUpLatestBuildingMajor = new DelegateCommand(() =>
                    {
                        SelectedMeasurement.NewLiveReadingsPopUp(true, typeof(BuildingReadingClockItem), typeof(InactiveClockItem));
                    }, () =>
                    {
                        return SelectedMeasurement != null;
                    }).ObservesProperty(() => SelectedMeasurement);
                }
                return _readingsPopUpLatestBuildingMajor;
            }
        }

        private ICommand _readingsPopUpLatestBuildingMinor;
        public ICommand ReadingsPopUpLatestBuildingMinor
        {
            get
            {
                if (_readingsPopUpLatestBuildingMinor == null)
                {
                    _readingsPopUpLatestBuildingMinor = new DelegateCommand(() =>
                    {
                        SelectedMeasurement.NewLiveReadingsPopUp(false, typeof(BuildingReadingClockItem), typeof(InactiveClockItem));
                    }, () =>
                    {
                        return SelectedMeasurement != null;
                    }).ObservesProperty(() => SelectedMeasurement);
                }
                return _readingsPopUpLatestBuildingMinor;
            }
        }

        private ICommand _closeMeasurementCommand;
        public ICommand CloseMeasurementCommand
        {
            get
            {
                if (_closeMeasurementCommand == null)
                {
                    _closeMeasurementCommand = new DelegateCommand(() =>
                    {
                        SelectedMeasurement.Close();
                        Measurements.Remove(SelectedMeasurement);
                        SelectedMeasurement = Measurements.FirstOrDefault();
                    });
                }
                return _closeMeasurementCommand;
            }
        }
       
        public void OnMinorGraphSettingsChanged(string value)
        {
            if (SelectedMeasurement == null)
            {
                return;
            }

            SelectedMeasurement.OnMinorGraphSettingsChanged(value);
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            switch (propertyChangedEventArgs.PropertyName)
            {
                case "SelectedMeasurement":
                    if (SelectedMeasurement == null)
                    {
                        return;
                    }
                    foreach (var measurementViewModel in Measurements)
                    {
                        measurementViewModel.IsEnabled = false;
                    }
                    SelectedMeasurement.IsEnabled = true;
                    OnPropertyChanged("ShowDetails");
                    break;
            }
        }

        private bool _showNew;
        public bool ShowNew
        {
            get { return _showNew; }
            set { SetProperty(ref _showNew, value); }
        }

        private bool _showSettings;
        public bool ShowSettings
        {
            get { return _showSettings; }
            set { SetProperty(ref _showSettings, value); }
        }


        public void SettingsChanged(MeasurementViewModel measurementViewModel)
        {
            ShowSettings = false;
            measurementViewModel.SettingsChanged();
        }
    }
}
