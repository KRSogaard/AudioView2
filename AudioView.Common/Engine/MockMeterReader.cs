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
        private List<double> minor;
        private List<double> major; 

        public MockMeterReader()
        {
            this.minor = new List<double>();
            this.major = new List<double>();
            this.rnd = new Random();
            this.lastReading = 50;
        }

        public Task<ReadingData> GetSecondReading()
        {
            return Task.Factory.StartNew(() =>
            {
                var newReading = Math.Min(150, Math.Max(60, this.rnd.Next(this.lastReading - 5, this.lastReading + 7)));
                this.lastReading = newReading;

                lock (minor)
                {
                    minor.Add(newReading);
                }
                lock (major)
                {
                    major.Add(newReading);
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
                double reading = 0;
                int count = 0;
                lock (minor)
                {
                    reading += minor.Sum();
                    count = minor.Count;
                    minor.Clear();
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
                double reading = 0;
                int count = 0;
                lock (minor)
                {
                    reading += major.Sum();
                    count = major.Count;
                    major.Clear();
                }
                reading = (double)reading / (double)count;

                return new ReadingData()
                {
                    LAeq = reading
                };
            });
        }

        public void SetEngine(AudioViewEngine engine)
        {
            // we don't need it
        }

        public bool IsTriggerMode()
        {
            return false;
        }
    }
}
