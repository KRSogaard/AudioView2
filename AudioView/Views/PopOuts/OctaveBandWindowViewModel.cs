using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using AudioView.Common;
using AudioView.Common.Data;
using AudioView.Common.Engine;
using GalaSoft.MvvmLight.Threading;
using Prism.Commands;
using Prism.Mvvm;

namespace AudioView.Views.PopOuts
{
    public class OctaveBandWindowViewModel : BindableBase, IMeterListener
    {
        private OctaveBand band;

        public OctaveBandWindowViewModel(MeasurementSettings settings, OctaveBand band)
        {
            this.band = band;
            _settings = settings;
            OctaveValues = new ObservableCollection<double>();
        }

        public string _title;
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        private bool _stayOnTop;
        public bool StayOnTop
        {
            get { return _stayOnTop; }
            set { _stayOnTop = value; OnPropertyChanged(); OnPropertyChanged("IsNotStayOnTop"); }
        }
        public bool IsNotStayOnTop
        {
            get { return !_stayOnTop; }
        }

        public ICommand ToggleOnTop
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    StayOnTop = !StayOnTop;
                });
            }
        }

        private MeasurementSettings _settings;
        public MeasurementSettings Settings
        {
            get { return _settings; }
        }

        public Task OnMinor(DateTime time, ReadingData data)
        {
            throw new NotImplementedException();
        }

        public Task OnMajor(DateTime time, ReadingData data)
        {
            throw new NotImplementedException();
        }

        public Task OnSecond(DateTime time, ReadingData data, ReadingData minorData, ReadingData majorData)
        {
            return Task.Run(() =>
            {
                DispatcherHelper.CheckBeginInvokeOnUI(() =>
                {
                    OctaveValues.Clear();
                    if (band == OctaveBand.OneOne)
                    {
                        OctaveValues.Add(data.LAeqOctaveBandOneOne.Hz16);
                        OctaveValues.Add(data.LAeqOctaveBandOneOne.Hz31_5);
                        OctaveValues.Add(data.LAeqOctaveBandOneOne.Hz63);
                        OctaveValues.Add(data.LAeqOctaveBandOneOne.Hz125);
                        OctaveValues.Add(data.LAeqOctaveBandOneOne.Hz250);
                        OctaveValues.Add(data.LAeqOctaveBandOneOne.Hz500);
                        OctaveValues.Add(data.LAeqOctaveBandOneOne.Hz1000);
                        OctaveValues.Add(data.LAeqOctaveBandOneOne.Hz2000);
                        OctaveValues.Add(data.LAeqOctaveBandOneOne.Hz4000);
                        OctaveValues.Add(data.LAeqOctaveBandOneOne.Hz8000);
                        OctaveValues.Add(data.LAeqOctaveBandOneOne.Hz16000);
                    }
                    else
                    {
                        OctaveValues.Add(data.LAeqOctaveBandOneThird.Hz6_3);
                        OctaveValues.Add(data.LAeqOctaveBandOneThird.Hz8);
                        OctaveValues.Add(data.LAeqOctaveBandOneThird.Hz10);
                        OctaveValues.Add(data.LAeqOctaveBandOneThird.Hz12_5);
                        OctaveValues.Add(data.LAeqOctaveBandOneThird.Hz16);
                        OctaveValues.Add(data.LAeqOctaveBandOneThird.Hz20);
                        OctaveValues.Add(data.LAeqOctaveBandOneThird.Hz25);
                        OctaveValues.Add(data.LAeqOctaveBandOneThird.Hz31_5);
                        OctaveValues.Add(data.LAeqOctaveBandOneThird.Hz40);
                        OctaveValues.Add(data.LAeqOctaveBandOneThird.Hz50);
                        OctaveValues.Add(data.LAeqOctaveBandOneThird.Hz63);
                        OctaveValues.Add(data.LAeqOctaveBandOneThird.Hz80);
                        OctaveValues.Add(data.LAeqOctaveBandOneThird.Hz100);
                        OctaveValues.Add(data.LAeqOctaveBandOneThird.Hz125);
                        OctaveValues.Add(data.LAeqOctaveBandOneThird.Hz160);
                        OctaveValues.Add(data.LAeqOctaveBandOneThird.Hz200);
                        OctaveValues.Add(data.LAeqOctaveBandOneThird.Hz250);
                        OctaveValues.Add(data.LAeqOctaveBandOneThird.Hz315);
                        OctaveValues.Add(data.LAeqOctaveBandOneThird.Hz400);
                        OctaveValues.Add(data.LAeqOctaveBandOneThird.Hz500);
                        OctaveValues.Add(data.LAeqOctaveBandOneThird.Hz630);
                        OctaveValues.Add(data.LAeqOctaveBandOneThird.Hz800);
                        OctaveValues.Add(data.LAeqOctaveBandOneThird.Hz1000);
                        OctaveValues.Add(data.LAeqOctaveBandOneThird.Hz1250);
                        OctaveValues.Add(data.LAeqOctaveBandOneThird.Hz1600);
                        OctaveValues.Add(data.LAeqOctaveBandOneThird.Hz2000);
                        OctaveValues.Add(data.LAeqOctaveBandOneThird.Hz2500);
                        OctaveValues.Add(data.LAeqOctaveBandOneThird.Hz3150);
                        OctaveValues.Add(data.LAeqOctaveBandOneThird.Hz4000);
                        OctaveValues.Add(data.LAeqOctaveBandOneThird.Hz5000);
                        OctaveValues.Add(data.LAeqOctaveBandOneThird.Hz6300);
                        OctaveValues.Add(data.LAeqOctaveBandOneThird.Hz8000);
                        OctaveValues.Add(data.LAeqOctaveBandOneThird.Hz10000);
                        OctaveValues.Add(data.LAeqOctaveBandOneThird.Hz12500);
                        OctaveValues.Add(data.LAeqOctaveBandOneThird.Hz16000);
                        OctaveValues.Add(data.LAeqOctaveBandOneThird.Hz20000);
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
        
        private ObservableCollection<double> octaveValues;
        public ObservableCollection<double> OctaveValues
        {
            get { return octaveValues; }
            set { SetProperty(ref octaveValues, value); }
        }

        public enum OctaveBand
        {
            OneOne,
            OneThird
        }
    }
}
