using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Animation;
using AudioView.Common.Data;
using AudioView.Common.Engine;
using AudioView.UserControls.CountDown;
using AudioView.ViewModels;
using GalaSoft.MvvmLight.Threading;
using Prism.Mvvm;

namespace AudioView.Views.PopOuts
{
    public class LiveGraphWindowViewModel : BindableBase, IMeterListener
    {
        private string displayItem;

        public LiveGraphWindowViewModel(string displayItem)
        {
            this.displayItem = displayItem;
            _lineValues = new ObservableCollection<Tuple<DateTime, double>>();
        }

        public string _title;
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        private ObservableCollection<Tuple<DateTime, double>> _lineValues;
        public ObservableCollection<Tuple<DateTime, double>> LineValues
        {
            get { return _lineValues; }
            set { SetProperty(ref _lineValues, value); }
        }

        public Task OnMinor(DateTime time, ReadingData data)
        {
            return Task.FromResult<object>(null);
        }

        public Task OnMajor(DateTime time, ReadingData data)
        {
            return Task.FromResult<object>(null);
        }

        public Task OnSecond(DateTime time, ReadingData data, ReadingData minorData, ReadingData majorData)
        {
            return Task.Run(() =>
            {
                double value = (double)typeof(ReadingData).GetMethod(displayItem).Invoke(data, new object[] { });
                DispatcherHelper.CheckBeginInvokeOnUI(() =>
                {
                    LineValues.Add(new Tuple<DateTime, double>(time, value));
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
    }
}
