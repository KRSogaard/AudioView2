using System.Threading.Tasks;
using System.Windows.Threading;
using AudioView.Common;
using AudioView.Common.Engine;

namespace AudioView.UserControls.Graph
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Windows;
    using System.Windows.Media;
    using AudioView.Annotations;
    using AudioView.ViewModels;
    using GalaSoft.MvvmLight.Threading;

    public class AudioViewGraphViewModel : INotifyPropertyChanged, IMeterListener
    {
        private bool isMajor;
        public AudioViewGraphViewModel(bool isMajor, int intervalsShown, int limitDb, TimeSpan interval, int minHeight, int maxHeight)
        {
            SecondReadings = new List<Tuple<DateTime, double>>();
            Readings = new LinkedList<Tuple<DateTime, double>>();
            IntervalsShown = intervalsShown;
            Interval = interval;
            LimitDb = limitDb;
            this.isMajor = isMajor;
            this.MinHeight = minHeight;
            this.MaxHeight = maxHeight;
        }

        private List<Tuple<DateTime, double>> _secondReadings;
        public List<Tuple<DateTime, double>> SecondReadings
        {
            get { return _secondReadings; }
            set { _secondReadings = value; OnPropertyChanged(); }
        }

        private LinkedList<Tuple<DateTime, double>> _readings;
        public LinkedList<Tuple<DateTime, double>> Readings
        {
            get { return _readings; }
            set { _readings = value; OnPropertyChanged(); }
        }

        private TimeSpan _interval;
        public TimeSpan Interval
        {
            get { return this._interval; }
            set { this._interval = value; OnPropertyChanged(); }
        }

        private int _intervalsShown;
        public int IntervalsShown
        {
            get { return _intervalsShown; }
            set { _intervalsShown = value; OnPropertyChanged(); }
        }

        private int _limitDb;
        public int LimitDb
        {
            get { return _limitDb; }
            set { _limitDb = value; OnPropertyChanged(); }
        }

        private bool _isEnabled;
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { _isEnabled = value; OnPropertyChanged(); }
        }

        private double _minHeight;
        public double MinHeight
        {
            get { return _minHeight; }
            set { _minHeight = value; OnPropertyChanged(); }
        }

        private double _maxHeight;
        public double MaxHeight
        {
            get { return _maxHeight; }
            set { _maxHeight = value; OnPropertyChanged(); }
        }

        #region IMeterListener Members
        public Task OnMinor(DateTime time, ReadingData data)
        {
            if (isMajor)
                return Task.FromResult<object>(null);

            return Task.Factory.StartNew(() =>
            {
                AddReading(time, data);
            });
        }

        public Task OnMajor(DateTime time, ReadingData data)
        {
            if (!isMajor)
                return Task.FromResult<object>(null);

            return Task.Factory.StartNew(() =>
            {
                AddReading(time, data);
            });
        }

        private void AddReading(DateTime time, ReadingData data)
        {
            Readings.AddLast(new Tuple<DateTime, double>(time, data.LAeq));
            if (Readings.Count >= this.IntervalsShown * 2)
            {
                Readings.RemoveFirst();
            }
        }

        public Task OnSecond(DateTime time, ReadingData data)
        {
            if(isMajor)
                return Task.FromResult<object>(null);

            return Task.Factory.StartNew(() =>
            {
                SecondReadings.Add(new Tuple<DateTime, double>(time, data.LAeq));
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

        public event PropertyChangedEventHandler PropertyChanged;
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
