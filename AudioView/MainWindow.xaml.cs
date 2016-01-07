using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using AudioView.ViewModels;

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
            this.DataContext = new MainViewModel();

            //var model = new AudioViewGraphViewModel(10, 150, 90, new TimeSpan(0, 1, 0));
            //AudioViewGraphMinor.DataContext = model;

            //rnd = new Random();
            //DateTime start = DateTime.Now.AddHours(-2).AddMinutes(-3);
            //TimeSpan interval = new TimeSpan(0,1,0);
            //TimeSpan current = new TimeSpan(0,0,0);
            //lastValue = 50;
            //while (start <= DateTime.Now)
            //{
            //    int reading = rnd.Next(Math.Max(lastValue - 5, 50), Math.Min(lastValue + 5, 150));
            //    lastValue = reading;
            //    current = current.Add(new TimeSpan(0, 0, 1));
            //    if (current >= interval)
            //    {
            //        model.AddReading(start, reading);
            //        current = new TimeSpan(0,0,0);
            //    }
            //    model.AddSecondReading(start, reading);
            //    start = start.AddSeconds(1);
            //}

            //model = new AudioViewGraphViewModel(10, 150, 90, new TimeSpan(0,15,0));
            //AudioViewGraphMajor.DataContext = model;
            //start = DateTime.Now.AddHours(-2).AddMinutes(-3);
            //interval = new TimeSpan(0, 15, 0);
            //current = new TimeSpan(0, 0, 0);
            //lastValue = 50;
            //while (start <= DateTime.Now)
            //{
            //    int reading = rnd.Next(Math.Max(lastValue - 5, 50), Math.Min(lastValue + 5, 150));
            //    lastValue = reading;
            //    model.AddReading(start, reading);
            //    start = start.Add(interval);
            //}
        }

        private void MenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            var model = (MainViewModel) this.DataContext;
            model.Measurements.Add(new MeasurementViewModel(Guid.NewGuid(), new MeasurementSettings()
            {
                DBLimit = 90,
                GraphLowerBound = 60,
                GraphUpperBound = 150,
                MajorClockMainItemId = 1,
                MajorClockSecondaryItemId = 2,
                MinorClockMainItemId = 0,
                MinorClockSecondaryItemId = 1,
                MinorInterval = new TimeSpan(0, 1, 0),
                MajorInterval = new TimeSpan(0, 15, 0)
            })
            {
                IsEnabled = true
            });
        }
    }
}
