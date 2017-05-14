using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using AudioView.Common.Export;
using AudioView.Common.Services;
using AudioView.ViewModels;
using MahApps.Metro.Controls.Dialogs;

namespace AudioView
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private string LoginFile = "auth.bin";

        public MainWindow()
        {
            DateTime uatShutdown = new DateTime(2017, 06, 30, 23, 59, 59);
            if (DateTime.Now > uatShutdown)
            {
                MessageBox.Show("Access to this User Acceptance Testing version of AudioView 2, expired " +
                                uatShutdown + ".", "User Acceptance Testing", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
            }
            else
            {
                MessageBox.Show("This is a special version of AudioView 2 meant for User Acceptance Testing, access will expire after " +
                                uatShutdown + ".", "User Acceptance Testing", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            InitializeComponent();
            var model = new MainViewModel();
            this.DataContext = model;
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            login();
        }

        private void Username_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                login();
            }
        }

        private void Password_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                login();
            }
        }

        private void login()
        {
            ((MainViewModel)DataContext).OnLogIn(username.Text, password.Password);
        }
    }
}
