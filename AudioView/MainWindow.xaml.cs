using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using AudioView.ViewModels;
using MahApps.Metro.Controls.Dialogs;

namespace AudioView
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private int lastValue;
        private Random rnd;

        public MainWindow()
        {
            InitializeComponent();
            var model = new MainViewModel();
            this.DataContext = model;

        }
    }
}
