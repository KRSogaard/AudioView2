using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using AudioView.UserControls.CountDown;

namespace AudioView.UserControls.CountDown
{
    /// <summary>
    /// Interaction logic for AudioViewCountDown2.xaml
    /// </summary>
    public partial class AudioViewCountDown : UserControl
    {
        private DispatcherTimer timer;

        public AudioViewCountDown()
        {
            InitializeComponent();

            this.timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(41); // 25 fps
            timer.Tick += TimerOnTick;
            timer.Start();
        }

        private void TimerOnTick(object sender, EventArgs eventArgs)
        {
            Draw();
        }

        private void Draw()
        {
            var model = (AudioViewCountDownViewModel)this.DataContext;
            if (model == null || !model.IsEnabled)
                return;

            DateTime lastReading = model.LastReadingTime;
            DateTime nextReading = model.NextReadingTime;

            TimeSpan totalSpan = nextReading - lastReading;
            TimeSpan currentSpan = DateTime.Now - lastReading;

            var radius = Math.Ceiling(this.ActualWidth / 2);

            var msValue = 360.0 / totalSpan.TotalMilliseconds;
            // Rotate -90 degres to get start at top
            var angle = currentSpan.TotalMilliseconds * msValue;

            model.Angle = angle;
            model.ArcThickness = (int)Math.Max(20, this.ActualWidth * 0.1);
        }
    }
}
