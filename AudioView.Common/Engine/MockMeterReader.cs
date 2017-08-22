using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AudioView.Common.Data;

namespace AudioView.Common.Engine
{
    public class MockMeterReader : IMeterReader
    {
        private Random rnd;
        private ReadingData lastReading;
        private List<Tuple<DateTime, ReadingData>> minor;
        private List<Tuple<DateTime, ReadingData>> major;
        private bool lastConnectionStatus;
        private TimeSpan minorInterval;
        private TimeSpan majorInterval;

        public MockMeterReader()
        {
            this.minor = new List<Tuple<DateTime, ReadingData>>();
            this.major = new List<Tuple<DateTime, ReadingData>>();
            this.rnd = new Random();
            lastReading = new ReadingData()
            {
                LAeq = newRandom(60),
                LCeq = newRandom(60),
                LAMax = newRandom(60),
                LAMin = newRandom(60),
                LZMax = newRandom(60),
                LZMin = newRandom(60)
            };
            foreach (var band in typeof(ReadingData.OctaveBandOneThird).GetProperties()
                                    .Where(x => x.PropertyType == typeof(double)))
            {
                band.SetValue(lastReading.LAeqOctaveBandOneThird, newRandom(60));
            }
            foreach (var band in typeof(ReadingData.OctaveBandOneOne).GetProperties()
                                    .Where(x => x.PropertyType == typeof(double)))
            {
                band.SetValue(lastReading.LAeqOctaveBandOneOne, newRandom(60));
            }
        }

        public Task<ReadingData> GetSecondReading()
        {
            return Task.Run(() =>
            {
                OnConnectionStatus(true);

                var newReading = new ReadingData()
                {
                    LAeq = newRandom((int)this.lastReading.LZMin),
                    LCeq = newRandom((int)this.lastReading.LCeq),
                    LAMax = newRandom((int)this.lastReading.LAMax),
                    LAMin = newRandom((int)this.lastReading.LAMin),
                    LZMax = newRandom((int)this.lastReading.LZMax),
                    LZMin = newRandom((int)this.lastReading.LZMin)
                };
                foreach (var band in typeof(ReadingData.OctaveBandOneThird).GetProperties()
                                    .Where(x => x.PropertyType == typeof(double)))
                {
                    band.SetValue(newReading.LAeqOctaveBandOneThird, newRandom((double)band.GetValue(lastReading.LAeqOctaveBandOneThird)));
                }
                foreach (var band in typeof(ReadingData.OctaveBandOneOne).GetProperties()
                                    .Where(x => x.PropertyType == typeof(double)))
                {
                    band.SetValue(newReading.LAeqOctaveBandOneOne, newRandom((double)band.GetValue(lastReading.LAeqOctaveBandOneOne)));
                }

                //newReading.LAeqOctaveBandOneThird.Hz1000 = 75.0;
                //newReading.LAeqOctaveBandOneOne.Hz1000 = 75.0;

                lock (minor)
                {
                    minor.Add(new Tuple<DateTime, ReadingData>(DateTime.Now, newReading));
                    minor.RemoveAll(x => x.Item1 < DateTime.Now - minorInterval);
                }
                lock (major)
                {
                    major.Add(new Tuple<DateTime, ReadingData>(DateTime.Now, newReading));
                    major.RemoveAll(x => x.Item1 < DateTime.Now - majorInterval);
                }

                lastReading = newReading;
                return newReading;
            });
        }

        private double newRandom(double? currentValue)
        {
            if (currentValue == null)
                return 60;

            double max = Math.Min(150, (double)currentValue + 5.0);
            double min = Math.Max(60, (double)currentValue - 5.0);
            double diff = this.rnd.NextDouble() * (max - min);

            return min + diff;
        }

        public Task<ReadingData> GetMinorReading(DateTime intervalStarted)
        {
            return Task.Run(() =>
            {
                OnConnectionStatus(true);
                return ReadingData.Average(minor.Select(x => x.Item2).ToList());
            });
        }

        public Task<ReadingData> GetMajorReading(DateTime intervalStarted)
        {
            return Task.Run(() =>
            {
                OnConnectionStatus(true);
                return ReadingData.Average(major.Select(x => x.Item2).ToList());
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
