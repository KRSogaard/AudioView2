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
using GalaSoft.MvvmLight.CommandWpf;

namespace AudioView.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private DispatcherTimer timer;
        public MainViewModel()
        {
            Measurements = new ObservableCollection<MeasurementViewModel>();
            var model = new MeasurementViewModel(Guid.NewGuid(), new MeasurementSettings()
            {
                DBLimit = 90,
                GraphLowerBound = 60,
                GraphUpperBound = 150,
                MajorClockMainItemId = 1,
                MajorClockSecondaryItemId = 2,
                MinorClockMainItemId = 0,
                MinorClockSecondaryItemId = 1,
                MinorInterval = new TimeSpan(0, 1, 0),
                MajorInterval = new TimeSpan(0, 15, 0)
            });
            model.IsEnabled = true;
            Measurements.Add(model);
            SelectedMeasurement = Measurements.FirstOrDefault();
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
                OnPropertyChanged("MeasurementSelected");
            }
        }

        public ICommand NewMeasurementCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    ShowNewFlow = true;
                    NewViewModel = new NewMeasurementViewModel(this);
                });
            }
        }

        public ICommand NewLiveReadingsPopUp
        {
            get
            {
                if (SelectedMeasurement == null)
                    return null;
                return SelectedMeasurement.NewLiveReadingsPopUp;
            }
        }

        public ICommand CloseMeasurementCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    SelectedMeasurement.Close();
                    Measurements.Remove(SelectedMeasurement);
                    SelectedMeasurement = Measurements.FirstOrDefault();
                });
            }
        }

        private bool _showNewFlow;
        public bool ShowNewFlow
        {
            get { return _showNewFlow; }
            set { _showNewFlow = value; OnPropertyChanged(); }
        }

        private int _lagTest;
        public int LagTest
        {
            get { return _lagTest; }
            set { _lagTest = value; OnPropertyChanged(); }
        }

        public bool MeasurementSelected
        {
            get { return SelectedMeasurement != null; }
        }

        public NewMeasurementViewModel _newViewModel;
        public NewMeasurementViewModel NewViewModel
        {
            get { return _newViewModel; }
            set { _newViewModel = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
