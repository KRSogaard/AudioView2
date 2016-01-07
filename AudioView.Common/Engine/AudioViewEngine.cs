using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Timers;

namespace AudioView.Common.Engine
{
    public class AudioViewEngine
    {
        private Timer secondTimer;
        private Timer minorTimer;
        private Timer majorTimer;
        private TimeSpan minorInterval;
        private TimeSpan majorInterval;
        private IMeterReader reader;
        private List<IMeterListener> listeners;
        private DateTime nextMajor;
        private DateTime nextMinor;

        public AudioViewEngine(IMeterReader reader)
            : this(new TimeSpan(0,1,0), new TimeSpan(0, 15, 0), reader)
        {
        }
        public AudioViewEngine(TimeSpan minorInterval, TimeSpan majorInterval, IMeterReader reader)
        {
            this.listeners = new List<IMeterListener>();
            this.reader = reader;
            this.minorInterval = minorInterval;
            this.majorInterval = majorInterval;

            this.secondTimer = new Timer(new TimeSpan(0,0,1).TotalMilliseconds);
            this.secondTimer.Elapsed += OnSecond;

            this.minorTimer = new Timer(minorInterval.TotalMilliseconds);
            this.minorTimer.Elapsed += OnMinorInterval;
            nextMinor = DateTime.Now + TimeSpan.FromMilliseconds(this.minorTimer.Interval);

            this.majorTimer = new Timer(majorInterval.TotalMilliseconds);
            this.majorTimer.Elapsed += OnMajorInterval;
            nextMajor = DateTime.Now + TimeSpan.FromMilliseconds(this.majorTimer.Interval);
        }

        public AudioViewEngine()
        {
            throw new NotImplementedException();
        }

        public void RegisterListener(IMeterListener listener)
        {
            lock (this.listeners)
            {
                listener.NextMajor(nextMajor);
                listener.NextMinor(nextMinor);
                this.listeners.Add(listener);
            }
        }

        public void UnRegisterListener(IMeterListener listener)
        {
            lock (this.listeners)
            {
                this.listeners.Remove(listener);
            }
        }

        public void Start()
        {
            this.secondTimer.Enabled = true;
            this.minorTimer.Enabled = true;
            this.majorTimer.Enabled = true;
        }

        public void Stop()
        {
            this.secondTimer.Enabled = false;
            this.minorTimer.Enabled = false;
            this.majorTimer.Enabled = false;
            this.secondTimer = null;
            this.minorTimer = null;
            this.majorTimer = null;
        }

        private async void OnMajorInterval(object sender, ElapsedEventArgs e)
        {
            DateTime time = e.SignalTime;
            nextMajor = e.SignalTime + TimeSpan.FromMilliseconds(((Timer)sender).Interval);
            var reading = await this.reader.GetMajorReading();

            lock (this.listeners)
            {
                foreach (var listener in this.listeners)
                {
                    listener.NextMajor(nextMajor);
                    listener.OnMajor(time, reading);
                }
            }
        }

        private async void OnMinorInterval(object sender, ElapsedEventArgs e)
        {
            DateTime time = e.SignalTime;
            nextMinor = e.SignalTime + TimeSpan.FromMilliseconds(((Timer)sender).Interval);
            var reading = await this.reader.GetMinorReading();

            lock (this.listeners)
            {
                foreach (var listener in this.listeners)
                {
                    listener.NextMinor(nextMinor);
                    listener.OnMinor(time, reading);
                }
            }
        }

        private async void OnSecond(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            DateTime time = DateTime.Now;
            var reading = await this.reader.GetSecondReading();

            lock (this.listeners)
            {
                foreach (var listener in this.listeners)
                {
                    listener.OnSecond(time, reading);
                }
            }
        }
    }
}
