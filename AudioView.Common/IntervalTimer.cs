using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace AudioView.Common
{
    public class IntervalTimer
    {
        private TimeSpan interval;
        private Timer timer;

        public IntervalTimer(TimeSpan interval)
        {
            this.interval = interval;
        }

        public DateTime Start(
            Action<DateTime, DateTime> onInterval, 
            Action<DateTime, DateTime> onStarted)
        {
            Stop();
            timer = new Timer();
            timer.Elapsed += (sender, args) =>
            {
                var nextInterval = UpdateTimeToNextInterval(timer);
                onInterval(args.SignalTime, nextInterval);
            };

            // Wait ontill next full minute before starting
            var nextFullMinute = GetNextFullMinute();
            WaitUntil(nextFullMinute).ContinueWith((innerTask) =>
            {
                var nextInterval = UpdateTimeToNextInterval(timer);
                onStarted(nextFullMinute, nextInterval);
            });

            return nextFullMinute;
        }

        public void Stop()
        {
            if (timer == null)
                return;

            timer.Stop();
            timer = null;
        }

        private DateTime UpdateTimeToNextInterval(Timer timer)
        {
            var nextInterval = GetNextInterval(interval);
            var spanUntilNextInterval = nextInterval - DateTime.Now;
            if (spanUntilNextInterval.TotalMilliseconds < 100)
            {
                DateTime startFrom = (DateTime.Now + TimeSpan.FromMilliseconds(interval.TotalMilliseconds * 0.2));
                nextInterval = GetNextInterval(interval, startFrom);
                spanUntilNextInterval = nextInterval - DateTime.Now;
            }
            
            // Reset the time.
            timer.Stop();
            timer.Interval = spanUntilNextInterval.TotalMilliseconds;
            timer.Start();

            return nextInterval;
        }

        private DateTime GetNextFullMinute()
        {
            var nextFullMin = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, 0)
                            .AddMinutes(1);
            return nextFullMin;
        }

        private DateTime GetNextInterval(TimeSpan interval, DateTime? start = null)
        {
            if (start == null)
            {
                start = DateTime.Now;
            }

            var next = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour,
                0, 0);
            while (next < start)
            {
                next += interval;
            }
            return next;
        }

        private Task WaitUntil(DateTime dateTime)
        {
            var waitTime = GetSpanUntill(dateTime);
            return Task.Delay(waitTime);
        }

        private TimeSpan GetSpanUntill(DateTime target)
        {
            return target - DateTime.Now;
        }
    }
}
