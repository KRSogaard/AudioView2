using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using AudioView.Common;
using AudioView.Common.Engine;
using AudioView.Common.Listeners;
using AudioView.UserControls.CountDown;
using AudioView.UserControls.Graph;
using GalaSoft.MvvmLight.CommandWpf;
using Prism.Mvvm;

namespace AudioView.ViewModels
{
    public class MeasurementViewModel : BindableBase
    {
        private List<Window> popOutWindows;
        public AudioViewEngine engine { get; set; }
        private MeasurementSettings settings;
        private DataStorageMeterListener dataStorage;
        private DateTime started;
        private TCPServerListener tcpServer;

        public MeasurementViewModel(Guid id, MeasurementSettings settings, IMeterReader reader)
        {
            started = DateTime.Now;
            popOutWindows = new List<Window>();
            this.engine = new AudioViewEngine(settings.MinorInterval, settings.MajorInterval, reader);
            this.settings = settings;
            
            if (settings.IsLocal)
            {
                // Registering the TCP Server for remote connections
                tcpServer = new TCPServerListener(settings);
                this.engine.RegisterListener(tcpServer);

                // Register the data storange unit
                dataStorage = new DataStorageMeterListener(id, DateTime.Now, settings);
                this.engine.RegisterListener(dataStorage);
            }

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
            MajorGraph = new AudioViewGraphViewModel(true,
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

            Title = settings.ProjectName;

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

        public MeasurementSettings Settings
        {
            get { return settings; }
        }

        public string DBLimit
        {
            get { return settings.DBLimit + "dB."; }
        }
        public string MinorInterval
        {
            get { return settings.MinorInterval.ToString(); }
        }
        public string MajorInterval
        {
            get { return settings.MajorInterval.ToString(); }
        }
        public string Port
        {
            get { return settings.Port.ToString(); }
        }

        public string Started
        {
            get { return started.ToString("g"); }
        }

        public string MyIp
        {
            get
            {
                var host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        return ip.ToString();
                    }
                }
                return "Unknown";
            }
        }

        private AudioViewCountDownViewModel minorClockViewModel;
        public AudioViewCountDownViewModel MinorClock
        {
            get { return minorClockViewModel; }
            set { SetProperty(ref minorClockViewModel, value); }
        }

        private AudioViewCountDownViewModel majorClockViewModel;
        public AudioViewCountDownViewModel MajorClock
        {
            get { return majorClockViewModel; }
            set { SetProperty(ref majorClockViewModel, value); }
        }

        private AudioViewGraphViewModel minorGraph;
        public AudioViewGraphViewModel MinorGraph
        {
            get { return minorGraph; }
            set { SetProperty(ref minorGraph, value); }
        }

        private AudioViewGraphViewModel majorGraph;
        public AudioViewGraphViewModel MajorGraph
        {
            get { return majorGraph; }
            set { SetProperty(ref majorGraph, value); }
        }

        public void NewLiveReadingsPopUp(bool isMajor, int mainClockItemId, int secondayClockItemId)
        {
            var window = new LiveReadingWindow()
            {
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                BorderThickness = new Thickness(1)
            };
            popOutWindows.Add(window);
            var model = new LiveReadingViewModel(isMajor,
                settings.MinorInterval,
                settings.DBLimit,
                mainClockItemId,
                secondayClockItemId);
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
        }

        public void NewGraphReadingsPopUp(bool isMajor)
        {
            var window = new GraphWindow()
            {
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                BorderThickness = new Thickness(1)
            };
            popOutWindows.Add(window);
            var model = new GraphReadingViewModel(isMajor,
                    settings.BarsDisplayed,
                    settings.DBLimit,
                    isMajor ? settings.MajorInterval : settings.MinorInterval,
                    settings.GraphLowerBound,
                    settings.GraphUpperBound);
            model.Title = Title;
            this.engine.RegisterListener(model);
            window.DataContext = model;
            window.Closed += (sender, args) =>
            {
                this.engine.UnRegisterListener(model);
                window.DataContext = null;
                popOutWindows.Remove(window);
                window = null;
            };
            window.Show();
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
    }
}