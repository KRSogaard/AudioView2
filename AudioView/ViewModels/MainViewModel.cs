using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using AudioView.Common.DataAccess;
using AudioView.Common.Engine;
using AudioView.Common.Listeners;
using AudioView.Common.Services;
using AudioView.UserControls.CountDown;
using AudioView.Views.Measurement;
using GalaSoft.MvvmLight.CommandWpf;
using MahApps.Metro.Controls.Dialogs;
using NLog;
using Prism.Commands;
using Prism.Mvvm;
using User = AudioView.Common.Data.User;

namespace AudioView.ViewModels
{
    public class MainViewModel : BindableBase
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private string userFile = "auth.bin";

        private UserService service;
        private MainWindow window;
        private DispatcherTimer timer;

        public MainViewModel()
        {
            ShowControls = false;
            logger.Info("Audio View started at {0}", DateTime.Now);

            SettingsViewModel = SettingsViewModel.Instance;
            HistoryViewModel = new HistoryViewModel();
            MeasurementsViewModel = new MeasurementsViewModel(this);

            service = new UserService();
            User user = getAuthData();
            if (user == null)
            {
                NeedLogIn = true;
                LogInFailed = false;
                LogInExpired = false;
                ShowControls = false;
            }
            else
            {
               // Verify expired
                try
                {
                    //var audioViewEntities = new AudioViewEntities();
                    //audioViewEntities.Users.Where(x => x.username == "kasper").First();
                    var userResult = service.GetUserSync(user.UserName);
                    if (userResult == null)
                    {
                        NeedLogIn = true;
                        LogInFailed = true;
                        LogInExpired = false;
                        ShowControls = false;
                        IsLoading = false;
                    }
                    else
                    {
                        if (user.Expires != null && userResult.Expires < DateTime.Now)
                        {
                            NeedLogIn = true;
                            LogInFailed = false;
                            LogInExpired = true;
                            ShowControls = false;
                            IsLoading = false;
                        }
                        else
                        {
                            NeedLogIn = false;
                            LogInFailed = false;
                            LogInExpired = false;
                            ShowControls = true;
                            IsLoading = false;
                        }
                    }
                }
                catch (Exception exp)
                {
                    logger.Error(exp);
                    if (user.Expires != null && user.Expires < DateTime.Now)
                    {
                        NeedLogIn = true;
                        LogInFailed = true;
                        LogInExpired = false;
                        ShowControls = false;
                        IsLoading = false;
                    }
                    else
                    {
                        // Allow user in
                        NeedLogIn = false;
                        LogInFailed = false;
                        LogInExpired = false;
                        ShowControls = true;
                        IsLoading = false;
                    }
                }
            }

            // Debug for me
            MeasurementsViewModel.NewViewModel = new NewMeasurementViewModel(MeasurementsViewModel);

            // Load offline files
            DataStorageMeterListener.UploadLocalFiles();
        }

        private User getAuthData()
        {
            try
            {
                var stream = File.OpenRead(userFile);
                var formatter = new BinaryFormatter();
                var v = (User) formatter.Deserialize(stream);
                stream.Close();
                return v;
            }
            catch (Exception exp)
            {
                return null;
            }
        }

        private void saveAuthData(User user)
        {
            try
            {
                FileStream stream = File.Create(userFile);
                var formatter = new BinaryFormatter();
                formatter.Serialize(stream, user);
                stream.Close();
            }
            catch (Exception exp)
            {
                logger.Error(exp);
            }
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

        private ICommand _skipLogin;
        public ICommand SkipLogin
        {
            get
            {
                if (_skipLogin == null)
                {
                    _skipLogin = new DelegateCommand(() =>
                    {
                        NeedLogIn = false;
                        LogInFailed = false;
                        LogInExpired = false;
                        ShowControls = true;
                    });
                }
                return _skipLogin;
            }
        }
        

        public void OnLogIn(string username, string password)
        {
            logger.Info("Logging in with user: " + username);
            IsLoading = true;
            service.Validate(username, password).ContinueWith(task =>
            {
                logger.Debug("Got respone from database.");
                IsLoading = false;
                var user = task.Result;

                if (user == null)
                {
                    logger.Info("Username and Passowrd combination was not recognized.");
                    NeedLogIn = true;
                    LogInFailed = true;
                }
                else
                {
                    if (user.Expires != null && user.Expires < DateTime.Now)
                    {
                        logger.Info("Login Expired.");
                        NeedLogIn = true;
                        LogInFailed = false;
                        LogInExpired = true;
                        ShowControls = false;
                    }
                    else
                    {
                        logger.Info("Login Ok.");
                        GlobalContainer.CurrentUser = user;
                        saveAuthData(user);
                        NeedLogIn = false;
                        LogInFailed = false;
                        LogInExpired = false;
                        ShowControls = true;
                    }
                }
            });
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

        private bool _showControls;
        public bool ShowControls
        {
            get { return _showControls; }
            set { SetProperty(ref _showControls, value); }
        }
        
        private bool _needLogIn;
        public bool NeedLogIn
        {
            get { return _needLogIn; }
            set { SetProperty(ref _needLogIn, value); }
        }

        private bool _logInFailed;
        public bool LogInFailed
        {
            get { return _logInFailed; }
            set { SetProperty(ref _logInFailed, value); }
        }

        private bool _logInExpired;
        public bool LogInExpired
        {
            get { return _logInExpired; }
            set { SetProperty(ref _logInExpired, value); }
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get { return _isLoading; }
            set { SetProperty(ref _isLoading, value); }
        }

        
    }
}
