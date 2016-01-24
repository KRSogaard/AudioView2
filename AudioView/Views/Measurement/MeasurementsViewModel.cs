using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using AudioView.Common;
using AudioView.Common.Engine;
using AudioView.ViewModels;
using GalaSoft.MvvmLight.CommandWpf;
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
            set { measurements = value; OnPropertyChanged(); }
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
                            MajorClockSecondaryItemId = 2,
                            MajorClockMainItemId = 3,
                            MinorInterval = new TimeSpan(0,0,0,5),
                            MinorClockMainItemId = 3,
                            MinorClockSecondaryItemId = 1
                        }, new MockMeterReader());
                        AddNewMeasurement(newModel);
                    });
                }
                return _starNewMeasurementCommand;
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
                        SelectedMeasurement.NewLiveReadingsPopUp(false, 0, 2);
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
                        SelectedMeasurement.NewLiveReadingsPopUp(true, 1, 2);
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
                if (_readingsPopUpLatestMajor == null)
                {
                    _readingsPopUpLatestMajor = new DelegateCommand(() =>
                    {
                        SelectedMeasurement.NewLiveReadingsPopUp(false, 1, 2);
                    }, () =>
                    {
                        return SelectedMeasurement != null;
                    }).ObservesProperty(() => SelectedMeasurement);
                }
                return _readingsPopUpLatestMajor;
            }
        }

        private ICommand _graphPopUpMajor;
        public ICommand GraphPopUpMajor
        {
            get
            {
                if (_graphPopUpMajor == null)
                {
                    _graphPopUpMajor = new DelegateCommand(() =>
                    {
                        SelectedMeasurement.NewGraphReadingsPopUp(true);
                    }, () =>
                    {
                        return SelectedMeasurement != null;
                    }).ObservesProperty(() => SelectedMeasurement);
                }
                return _graphPopUpMajor;
            }
        }

        private ICommand _graphPopUpMinor;
        public ICommand GraphPopUpMinor
        {
            get
            {
                if (_graphPopUpMinor == null)
                {
                    _graphPopUpMinor = new DelegateCommand(() =>
                    {
                        SelectedMeasurement.NewGraphReadingsPopUp(false);
                    }, () =>
                    {
                        return SelectedMeasurement != null;
                    }).ObservesProperty(() => SelectedMeasurement);
                }
                return _graphPopUpMinor;
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
    }
}
