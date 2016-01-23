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
using AudioView.Views.Measurement;
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
            PropertyChanged += (sender, args) =>
            {
                if(args.PropertyName != nameof(LagTest))
                    logger.Trace("MainViewModel {0} was change", args.PropertyName);
            };

            logger.Info("Audio View started at {0}", DateTime.Now);

            SettingsViewModel = new SettingsViewModel();
            HistoryViewModel = new HistoryViewModel();
            MeasurementsViewModel = new MeasurementsViewModel(this);

            // Load offline files
            DataStorageMeterListener.UploadLocalFiles();


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

        public MeasurementsViewModel MeasurementsViewModel { get; set; }
        public HistoryViewModel HistoryViewModel { get; set; }
        public SettingsViewModel SettingsViewModel { get; set; }

        private ICommand _showSettingsCommand;
        public ICommand ShowSettingsCommand
        {
            get
            {
                if (_showSettingsCommand == null)
                {
                    _showSettingsCommand = new DelegateCommand(() =>
                    {
                        ShowSettings = true;
                    });
                }
                return _showSettingsCommand;
            }
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

        private bool _showNew;
        public bool ShowNew
        {
            get { return _showNew; }
            set { SetProperty(ref _showNew, value); }
        }
    }
}
