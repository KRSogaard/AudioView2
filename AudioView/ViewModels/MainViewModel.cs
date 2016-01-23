using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using AudioView.Common.Engine;
using AudioView.Common.Listeners;
using AudioView.UserControls.CountDown;
using GalaSoft.MvvmLight.CommandWpf;
using MahApps.Metro.Controls.Dialogs;
using NLog;
using Prism.Commands;
using Prism.Mvvm;

namespace AudioView.ViewModels
{
    public class MainViewModel : BindableBase
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private MainWindow window;
        private DispatcherTimer timer;
        public MainViewModel()
        {
            logger.Info("Audio View started at {0}", DateTime.Now);

            Measurements = new ObservableCollection<MeasurementViewModel>();
            SelectedMeasurement = null;
            NewViewModel = new NewMeasurementViewModel(this);
            SettingsViewModel = new SettingsViewModel();
            PropertyChanged += OnPropertyChanged;

            // Load offline files
            DataStorageMeterListener.UploadLocalFiles();

            HistoryViewModel = new HistoryViewModel();
            OnPropertyChanged("HistoryViewModel");

            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(15);
            timer.IsEnabled = true;
            timer.Tick += (sender, args) =>
            {
                var newValue = LagTest + 1;
                if (newValue > 1000)
                {
                    LagTest = 0;
                }
                else
                {
                    LagTest = newValue;
                }
            };
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

        public HistoryViewModel HistoryViewModel { get; set; }
        public SettingsViewModel SettingsViewModel { get; set; }

        private ICommand _showSettingsCommand;
        public ICommand ShowSettingsCommand
        {
            get
            {
                if (_showSettingsCommand == null)
                {
                    _showSettingsCommand = new RelayCommand(() =>
                    {
                        ShowSettings = true;
                    });
                }
                return _showSettingsCommand;
            }
        }

        private ICommand _newMeasurementCommand;
        public ICommand NewMeasurementCommand
        {
            get
            {
                if (_newMeasurementCommand == null)
                {
                    _newMeasurementCommand = new RelayCommand(() =>
                    {
                        ShowNewFlow = true;
                        NewViewModel = new NewMeasurementViewModel(this);
                    });
                }
                return _newMeasurementCommand;
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
                    _closeMeasurementCommand = new RelayCommand(() =>
                    {
                        SelectedMeasurement.Close();
                        Measurements.Remove(SelectedMeasurement);
                        SelectedMeasurement = Measurements.FirstOrDefault();
                    });
                }
                return _closeMeasurementCommand;
            }
        }

        private bool _showNewFlow;
        public bool ShowNewFlow
        {
            get { return _showNewFlow; }
            set { SetProperty(ref _showNewFlow, value); }
        }

        private bool _showSettings;
        public bool ShowSettings
        {
            get { return _showSettings; }
            set { SetProperty(ref _showSettings, value); }
        }

        private int _lagTest;
        public int LagTest
        {
            get { return _lagTest; }
            set { SetProperty(ref _lagTest, value); }
        }

        public bool MeasurementSelected
        {
            get { return SelectedMeasurement != null; }
        }

        public void AddNewMeasurement(MeasurementViewModel newModel)
        {
            ShowNewFlow = false;
            Measurements.Add(newModel);
            if (SelectedMeasurement == null)
            {
                SelectedMeasurement = newModel;
                SelectedMeasurement.IsEnabled = true;
            }
        }

        public NewMeasurementViewModel _newViewModel;

        public NewMeasurementViewModel NewViewModel
        {
            get { return _newViewModel; }
            set { SetProperty(ref _newViewModel, value); }
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

        public void RegisterWindow(MainWindow mainWindow)
        {
            this.window = mainWindow;
        }
    }
}
