using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using AudioView.Common;
using AudioView.Common.Engine;
using AudioView.UserControls.CountDown;
using AudioView.UserControls.Graph;
using GalaSoft.MvvmLight.CommandWpf;

namespace AudioView.ViewModels
{
    public class MeasurementSettings
    {
        public TimeSpan MinorInterval { get; set; }
        public TimeSpan MajorInterval { get; set; }
        public int BarsDisplayed { get; set; }
        public int DBLimit { get; set; }
        public int GraphUpperBound { get; set; }
        public int GraphLowerBound { get; set; }
        public int MinorClockMainItemId { get; set; }
        public int MinorClockSecondaryItemId { get; set; }
        public int MajorClockMainItemId { get; set; }
        public int MajorClockSecondaryItemId { get; set; }

        public void MeasurementViewModel()
        {
            BarsDisplayed = 15;
        }
    }

    public class MeasurementViewModel : INotifyPropertyChanged
    {
        private List<Window> popOutWindows;
        public AudioViewEngine engine { get; set; }
        private MeasurementSettings settings;

        public MeasurementViewModel(Guid id, MeasurementSettings settings)
        {
            popOutWindows = new List<Window>();
            var reader = new MockMeterReader();
            this.engine = new AudioViewEngine(settings.MinorInterval, settings.MajorInterval, reader);
            this.settings = settings;

            MinorClock = new AudioViewCountDownViewModel(false,
                    settings.MinorInterval,
                    settings.DBLimit,
                    settings.MinorClockMainItemId,
                    settings.MinorClockSecondaryItemId);
            MajorClock = new AudioViewCountDownViewModel(true,
                    settings.MajorInterval,
                    settings.DBLimit,
                    settings.MajorClockMainItemId,
                    settings.MajorClockSecondaryItemId);
            MinorGraph = new AudioViewGraphViewModel(false,
                    settings.BarsDisplayed,
                    settings.DBLimit,
                    settings.MinorInterval,
                    settings.GraphLowerBound,
                    settings.GraphUpperBound);
            MajorGraph = new AudioViewGraphViewModel(false,
                    settings.BarsDisplayed,
                    settings.DBLimit,
                    settings.MajorInterval,
                    settings.GraphLowerBound,
                    settings.GraphUpperBound);

            this.engine.RegisterListener(MinorGraph);
            this.engine.RegisterListener(MajorGraph);
            this.engine.RegisterListener(MinorClock);
            this.engine.RegisterListener(MajorClock);
            this.engine.Start();

            //Title = newViewModel.ProjectName;
            Title = "Test Reading";
            // More here
        }

        private string title;
        public string Title
        {
            get { return title; }
            set { title = value; }
        }

        public Brush TextColor
        {
            get
            {
                return new SolidColorBrush(ColorSettings.AxisColor);
            }
        }

        private AudioViewCountDownViewModel minorClockViewModel;
        public AudioViewCountDownViewModel MinorClock
        {
            get { return minorClockViewModel; }
            set { minorClockViewModel = value; OnPropertyChanged(); }
        }

        private AudioViewCountDownViewModel majorClockViewModel;
        public AudioViewCountDownViewModel MajorClock
        {
            get { return majorClockViewModel; }
            set { majorClockViewModel = value; OnPropertyChanged(); }
        }

        private AudioViewGraphViewModel minorGraph;
        public AudioViewGraphViewModel MinorGraph
        {
            get { return minorGraph; }
            set { minorGraph = value; OnPropertyChanged(); }
        }

        private AudioViewGraphViewModel majorGraph;
        public AudioViewGraphViewModel MajorGraph
        {
            get { return majorGraph; }
            set { majorGraph = value; OnPropertyChanged(); }
        }

        public ICommand NewLiveReadingsPopUp
        {
            get
            {
                return new RelayCommand(() =>
                {
                    var window = new LiveReadingWindow()
                    {
                        WindowStartupLocation = WindowStartupLocation.CenterOwner,
                        BorderThickness = new Thickness(1)
                    };
                    popOutWindows.Add(window);
                    var model = new LiveReadingViewModel(false,
                        settings.MinorInterval,
                        settings.DBLimit,
                        settings.MinorClockMainItemId,
                        settings.MinorClockSecondaryItemId);
                    model.Title = Title;
                    this.engine.RegisterListener(model);
                    model.NextReadingTime = MinorClock.NextReadingTime;
                    model.LastReadingTime = MinorClock.LastReadingTime;
                    window.DataContext = model;
                    window.Closed += (sender, args) =>
                    {
                        this.engine.UnRegisterListener(model);
                        window.DataContext = null;
                        popOutWindows.Remove(window);
                        window = null;
                    };
                    window.Show();
                });
            }
        }

        public void Close()
        {
            foreach (var window in popOutWindows)
            {
                window.Close();
            }
            popOutWindows.Clear();
            this.engine.Stop();
        }

        private bool _isEnabled;
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set {
                _isEnabled = value;
                OnPropertyChanged();
                MinorClock.IsEnabled = value;
                MajorClock.IsEnabled = value;
                MinorGraph.IsEnabled = value;
                MajorGraph.IsEnabled = value;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}