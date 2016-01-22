using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using AudioView.Annotations;
using AudioView.Common;
using AudioView.Common.Data;
using AudioView.Common.Engine;
using AudioView.ViewModels;

namespace AudioView.UserControls.CountDown
{
    public class AudioViewCountDownViewModel : INotifyPropertyChanged, IMeterListener
    {
        private bool isMajor;
        private int mainItem;
        private int secondItem;
        private int limitDb;
        private bool isPopOut;

        public AudioViewCountDownViewModel(bool isMajor, TimeSpan interval, int limitDb, int mainItem, int secondItem, bool isPopOut = false)
        {
            Interval = interval;
            this.isMajor = isMajor;
            this.limitDb = limitDb;
            this.mainItem = mainItem;
            this.secondItem = secondItem;
            this.isPopOut = isPopOut;
        }
        
        public TimeSpan Interval { get; set; }
        public DateTime NextReadingTime { get; set; }
        public DateTime LastReadingTime { get; set; }

        public ReadingData LastReading { get; set; }
        public ReadingData LastBuildingInterval { get; set; }
        public ReadingData LastInterval { get; set; }
        public string RenderTime { get; set; }

        public SolidColorBrush BarBrush { get; set; }
        public SolidColorBrush BarOverBrush { get; set; }

        private double _angle;
        public double Angle
        {
            get { return 360 - _angle; }
            set {
                _angle = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(MainItemText));
                OnPropertyChanged(nameof(SecondItemText));
                OnPropertyChanged(nameof(TextColor));
                OnPropertyChanged(nameof(RenderTime));
            }
        }

        private int _arcThickness;
        public int ArcThickness
        {
            get
            {
                return _arcThickness;
            }
             set
            {
                _arcThickness = value; OnPropertyChanged();
            }
        }

        private bool _isEnabled;

        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { _isEnabled = value; OnPropertyChanged(); }
        }

        public string MainItemText
        {
            get { return GetDisplayText(mainItem); }
        }

        public string SecondItemText
        {
            get { return GetDisplayText(secondItem); }
        }

        private string GetDisplayText(int displayId)
        {
            switch (displayId)
            {
                case 1: // Latests interval
                    return ((int)Math.Ceiling(LastInterval.LAeq)).ToString();
                case 2: // Time to next interval
                    return (NextReadingTime - DateTime.Now).ToString(@"mm\:ss\.f", null);
                case 3: // Latests building interval
                    return ((int)Math.Ceiling(LastBuildingInterval.LAeq)).ToString();
                case 0: // Lastest reading (live data)
                default:
                    return ((int)Math.Ceiling(LastReading.LAeq)).ToString();
            }
        }

        public Brush TextColor
        {
            get
            {
                return LastReading.LAeq >= limitDb ? BarOverBrush : BarBrush;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public void OnNext(DateTime time)
        {
            LastReadingTime = time.AddMilliseconds(-Interval.TotalMilliseconds);
            NextReadingTime = time;
        }

        #region IMeterListener
        public Task OnMinor(DateTime time, ReadingData data)
        {
            return Task.Factory.StartNew(() =>
            {
                if (!isMajor)
                    LastInterval = data;
            });
        }

        public Task OnMajor(DateTime time, ReadingData data)
        {
            return Task.Factory.StartNew(() =>
            {
                if (isMajor)
                    LastInterval = data;
            });
        }

        public Task OnSecond(DateTime time, ReadingData data, ReadingData minorData, ReadingData majorData)
        {
            return Task.Factory.StartNew(() =>
            {
                LastReading = data;
                LastBuildingInterval = isMajor ? majorData : minorData;
            });
        }

        public Task NextMinor(DateTime time)
        {
            return Task.Factory.StartNew(() =>
            {
                if(!isMajor)
                    OnNext(time);
            });
        }

        public Task NextMajor(DateTime time)
        {
            return Task.Factory.StartNew(() =>
            {
                if (isMajor)
                    OnNext(time);
            });
        }

        public Task StopListener()
        {
            return Task.FromResult<object>(null);
        }

        #endregion
    }
}
