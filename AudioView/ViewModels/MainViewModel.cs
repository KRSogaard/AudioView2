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
            logger.Info("Audio View started at {0}", DateTime.Now);

            SettingsViewModel = SettingsViewModel.Instance;
            HistoryViewModel = new HistoryViewModel();
            MeasurementsViewModel = new MeasurementsViewModel(this);

            // Debug for me
            MeasurementsViewModel.NewViewModel = new NewMeasurementViewModel(MeasurementsViewModel);

            // Load offline files
            DataStorageMeterListener.UploadLocalFiles();
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

        private bool _showNew;
        public bool ShowNew
        {
            get { return _showNew; }
            set { SetProperty(ref _showNew, value); }
        }
    }
}
