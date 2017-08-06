using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using AudioView.Common;
using AudioView.Common.Data;
using AudioView.Common.Engine;
using AudioView.UserControls.Graphs;
using GalaSoft.MvvmLight.Threading;
using Prism.Commands;
using Prism.Mvvm;

namespace AudioView.Views.PopOuts
{
    public class OctaveBandWindowViewModel : BindableBase, IMeterListener
    {
        private OctaveBand band;
        private bool building;

        public OctaveBandWindowViewModel(MeasurementSettings settings, OctaveBand band, bool building)
        {
            this.band = band;
            _settings = settings;
            this.building = building;
            OctaveValues = new ObservableCollection<OctaveBandGraphValue>();
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

        public Task OnMinor(DateTime time, DateTime starTime, ReadingData data)
        {
            throw new NotImplementedException();
        }

        public Task OnMajor(DateTime time, DateTime starTime, ReadingData data)
        {
            throw new NotImplementedException();
        }

        public Task OnSecond(DateTime time, DateTime starTime, ReadingData data, ReadingData minorData, ReadingData majorData)
        {
            return Task.Run(() =>
            {
                DispatcherHelper.CheckBeginInvokeOnUI(() =>
                {
                    OctaveValues.Clear();

                    if ((building ? minorData : data) == null)
                    {
                        return;
                    }

                    if (band == OctaveBand.OneOne)
                    {
                        foreach (var obp in DecibelHelper.GetOneOneOctaveBand())
                        {
                            OctaveValues.Add(new OctaveBandGraphValue(obp.Display, (building ? minorData : data).GetValue("1-1-" + obp.Method), obp.LimitAjust, _settings.MinorDBLimit));
                        }
                    }
                    else
                    {
                        foreach (var obp in DecibelHelper.GetOneThirdOctaveBand())
                        {

                            OctaveValues.Add(new OctaveBandGraphValue(obp.Display,
                                (building ? minorData : data).GetValue("1-3-" + obp.Method), obp.LimitAjust,
                                _settings.MinorDBLimit));
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
        
        private ObservableCollection<OctaveBandGraphValue> octaveValues;
        public ObservableCollection<OctaveBandGraphValue> OctaveValues
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
