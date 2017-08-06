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
            
            DateTime nextReading = model.NextReadingTime;
            
            TimeSpan totalSpan = model.Interval;
            TimeSpan currentSpan = nextReading - DateTime.Now;
            
            var msValue = 360.0 / totalSpan.TotalMilliseconds;
            // Rotate -90 degres to get start at top
            var angle = currentSpan.TotalMilliseconds * msValue;
            if (angle > 360)
            {
                angle = 0.0001;
            }
            
            model.Angle = 360 - angle;
            model.ArcThickness = (int)Math.Max(20, this.ActualWidth * 0.1);

            model.BarBrush = BarBrush;
            model.BarOverBrush = BarOverBrush;
        }

        private void GraphSettingsMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            var model = (AudioViewCountDownViewModel)this.DataContext;
            string tag = ((MenuItem) sender).Tag.ToString();
            model.ChangeMainDisplayItem(new DisplayValueClockItem(tag));
        }

        public static readonly DependencyProperty BarBrushProperty =
            DependencyProperty.Register(
                "BarBrush", typeof(SolidColorBrush), typeof(AudioViewCountDown),
                new FrameworkPropertyMetadata(
                    new SolidColorBrush(Colors.White),
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        public static readonly DependencyProperty BarOverBrushProperty =
            DependencyProperty.Register(
                "BarOverBrush", typeof(SolidColorBrush), typeof(AudioViewCountDown),
                new FrameworkPropertyMetadata(
                    new SolidColorBrush(Colors.Red),
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public SolidColorBrush BarBrush
        {
            get { return (SolidColorBrush)GetValue(BarBrushProperty); }
            set { SetValue(BarBrushProperty, value); }
        }

        public SolidColorBrush BarOverBrush
        {
            get { return (SolidColorBrush)GetValue(BarOverBrushProperty); }
            set { SetValue(BarOverBrushProperty, value); }
        }
    }
}
