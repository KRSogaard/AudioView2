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
using GalaSoft.MvvmLight.Threading;
using MahApps.Metro.Controls;
using NLog;
using Prism.Mvvm;

namespace AudioView.ViewModels
{
    public class MeasurementViewModel : BindableBase
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private LinkedList<MetroWindow> popOutWindows;
        public AudioViewEngine engine { get; set; }
        private MeasurementSettings settings;
        private DataStorageMeterListener dataStorage;
        private DateTime started;
        private TCPServerListener tcpServer;
        private bool ConnectionStatus;

        public MeasurementViewModel(Guid id, MeasurementSettings settings, IMeterReader reader)
        {
            started = DateTime.Now;
            popOutWindows = new LinkedList<MetroWindow>();
            this.engine = new AudioViewEngine(settings.MinorInterval, settings.MajorInterval, reader);
            this.settings = settings;

            this.engine.ConnectionStatusEvent += connected =>
            {
                if(ConnectionStatus == connected)
                    return; // No need

                Task.Run(() =>
                {
                    ConnectionStatus = connected;
                    DispatcherHelper.CheckBeginInvokeOnUI(() =>
                    {
                        OnPropertyChanged(nameof(IsDisconnected));
                    });
                });
            };
            
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
                    settings.MinorDBLimit,
                    settings.MinorClockMainItemId,
                    settings.MinorClockSecondaryItemId);
            MajorClock = new AudioViewCountDownViewModel(true,
                    settings.MajorInterval,
                    settings.MajorDBLimit,
                    settings.MajorClockMainItemId,
                    settings.MajorClockSecondaryItemId);
            MinorGraph = new AudioViewGraphViewModel(false,
                    settings.BarsDisplayed,
                    settings.MinorDBLimit,
                    settings.MinorInterval,
                    settings.GraphLowerBound,
                    settings.GraphUpperBound);
            MajorGraph = new AudioViewGraphViewModel(true,
                    settings.BarsDisplayed,
                    settings.MajorDBLimit,
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
                return new SolidColorBrush(Colors.White);
            }
        }

        public MeasurementSettings Settings
        {
            get { return settings; }
        }

        public string MinorDBLimit
        {
            get { return settings.MinorDBLimit + "dB."; }
        }
        public string MajorDBLimit
        {
            get { return settings.MajorDBLimit + "dB."; }
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
        public bool IsDisconnected
        {
            get { return !ConnectionStatus; }
        }
        public bool IsLocal
        {
            get { return settings.IsLocal; }
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
                BorderThickness = new Thickness(1),
                GlowBrush = null
            };
            window.SetResourceReference(MetroWindow.BorderBrushProperty, "AccentColorBrush");

            popOutWindows.AddLast(window);
            var model = new LiveReadingViewModel(isMajor,
                isMajor ?  settings.MajorInterval : settings.MinorInterval,
                isMajor ? settings.MajorDBLimit : settings.MinorDBLimit,
                mainClockItemId,
                secondayClockItemId);
            model.OnNext(isMajor ? MajorClock.NextReadingTime : MinorClock.NextReadingTime);

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

        public void NewGraphReadingsPopUp(bool isMajor)
        {
            var window = new GraphWindow()
            {
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                BorderThickness = new Thickness(1),
                GlowBrush = null
            };
            window.SetResourceReference(MetroWindow.BorderBrushProperty, "AccentColorBrush");

            popOutWindows.AddLast(window);
            var model = new GraphReadingViewModel(isMajor,
                    settings.BarsDisplayed,
                    isMajor ? settings.MajorDBLimit : settings.MinorDBLimit,
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
            this.engine.Stop();
            if (popOutWindows.Count <= 0)
                return;

            logger.Debug("Closing all {0} pop-up windows.", popOutWindows.Count);
            while (popOutWindows.First != null)
            {
                logger.Trace("Closing pop-op window {0}.", popOutWindows.First);
                try
                {
                    var first = popOutWindows.First.Value;
                    first.Close();
                    // It should be removed by the even, but lets check just to be sure
                    if (popOutWindows.First.Value == first)
                    {
                        popOutWindows.RemoveFirst();
                    }
                }
                catch (Exception exp)
                {
                    logger.Error(exp, "Failed to close pop-op window.");
                }
            }
            // Clear to make sure everyting ise ok
            popOutWindows.Clear();
        }

        private bool _isEnabled;
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set {
                _isEnabled = value;
                OnPropertyChanged();

                if (MinorClock != null)
                    MinorClock.IsEnabled = value;
                if (MajorClock != null)
                    MajorClock.IsEnabled = value;
                if (MinorGraph != null)
                    MinorGraph.IsEnabled = value;
                if (MajorGraph != null)
                    MajorGraph.IsEnabled = value;
            }
        }
    }
}