using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using AudioView.UserControls.CountDown.ClockItems;
using NLog;

namespace AudioView.UserControls.CountDown
{
    /// <summary>
    /// Interaction logic for AudioViewCountDown2.xaml
    /// </summary>
    public partial class AudioViewCountDown : UserControl
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private DispatcherTimer timer;
        
        public SolidColorBrush BarBrush { get; set; }
        public SolidColorBrush BarOverBrush { get; set; }

        public AudioViewCountDown()
        {
            InitializeComponent();

            Draw();

            this.timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(100); // 41 = 25 fps, 100 = 10 fps
            timer.Tick += TimerOnTick;
            timer.Start();
        }

        private void TimerOnTick(object sender, EventArgs eventArgs)
        {
            Draw();
        }

        private void Draw()
        {
            //DateTime start = DateTime.Now;
            var model = (AudioViewCountDownViewModel)this.DataContext;
            if (model == null || !model.IsEnabled)
                return;

            DateTime lastReading = model.LastReadingTime;
            DateTime nextReading = model.NextReadingTime;

            TimeSpan totalSpan = nextReading - lastReading;
            TimeSpan currentSpan = DateTime.Now - lastReading;
            
            var msValue = 360.0 / totalSpan.TotalMilliseconds;
            // Rotate -90 degres to get start at top
            var angle = currentSpan.TotalMilliseconds * msValue;

            model.Angle = angle;
            model.ArcThickness = (int)Math.Max(20, this.ActualWidth * 0.1);

            model.BarBrush = BarBrush;
            model.BarOverBrush = BarOverBrush;
            //var end = DateTime.Now;
            //logger.Warn("Clock Render time: " + (start - end).TotalMilliseconds);
        }

        private void GraphSettingsMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            var model = (AudioViewCountDownViewModel)this.DataContext;
            string tag = ((MenuItem) sender).Tag.ToString();
            model.ChangeMainDisplayItem(new DisplayValueClockItem(tag));
        }
    }
}
