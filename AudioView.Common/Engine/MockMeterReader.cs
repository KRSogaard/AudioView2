using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AudioView.Common.Data;

namespace AudioView.Common.Engine
{
    public class MockMeterReader : IMeterReader
    {
        private Random rnd;
        private int lastReading;
        private List<Tuple<DateTime, double>> minor;
        private List<Tuple<DateTime, double>> major;
        private bool lastConnectionStatus;
        private TimeSpan minorInterval;
        private TimeSpan majorInterval;

        public MockMeterReader()
        {
            this.minor = new List<Tuple<DateTime,double>>();
            this.major = new List<Tuple<DateTime, double>>();
            this.rnd = new Random();
            this.lastReading = 50;
        }

        public Task<ReadingData> GetSecondReading()
        {
            return Task.Factory.StartNew(() =>
            {
                OnConnectionStatus(true);
                var newReading = Math.Min(150, Math.Max(60, this.rnd.Next(this.lastReading - 5, this.lastReading + 7)));
                this.lastReading = newReading;

                lock (minor)
                {
                    minor.Add(new Tuple<DateTime, double>(DateTime.Now, newReading));
                    minor.RemoveAll(x => x.Item1 < DateTime.Now - minorInterval);
                }
                lock (major)
                {
                    major.Add(new Tuple<DateTime, double>(DateTime.Now, newReading));
                    major.RemoveAll(x => x.Item1 < DateTime.Now - majorInterval);
                }

                return new ReadingData()
                {
                    LAeq = newReading
                };
            });
        }

        public Task<ReadingData> GetMinorReading()
        {
            return Task.Factory.StartNew(() =>
            {
                OnConnectionStatus(true);
                double reading = 0;
                int count = 0;
                lock (minor)
                {
                    reading += minor.Select(x => x.Item2).Sum();
                    count = minor.Count;
                }
                reading = (double)reading / (double)count;

                return new ReadingData()
                {
                    LAeq = reading
                };
            });
        }

        public Task<ReadingData> GetMajorReading()
        {
            return Task.Factory.StartNew(() =>
            {
                OnConnectionStatus(true);
                double reading = 0;
                int count = 0;
                lock (major)
                {
                    reading += major.Select(x=>x.Item2).Sum();
                    count = major.Count;
                }
                reading = (double)reading / (double)count;

                return new ReadingData()
                {
                    LAeq = reading
                };
            });
        }

        public void SetMinorInterval(TimeSpan interval)
        {
            this.minorInterval = interval;
        }

        public void SetMajorInterval(TimeSpan interval)
        {
            this.majorInterval = interval;
        }

        public void SetEngine(AudioViewEngine engine)
        {
            // we don't need it
        }

        public bool IsTriggerMode()
        {
            return false;
        }

        public void OnConnectionStatus(bool status)
        {
            if (status == lastConnectionStatus)
                return;

            lastConnectionStatus = status;
            if (ConnectionStatusEvent == null)
                return;
            ConnectionStatusEvent(status);
        }
        public event ConnectionStatusUpdateDeligate ConnectionStatusEvent;
        public Task Close()
        {
            return Task.FromResult<object>(null);
        }
    }
}
