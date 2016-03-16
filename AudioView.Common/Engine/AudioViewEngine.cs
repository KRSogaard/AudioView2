using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Timers;
using AudioView.Common.Data;
using NLog;

namespace AudioView.Common.Engine
{
    public class AudioViewEngine
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

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
            logger.Info("Started engine with major: {0} minor: {1}", majorInterval, minorInterval);
            reader.SetMinorInterval(minorInterval);
            reader.SetMajorInterval(majorInterval);

            this.listeners = new List<IMeterListener>();
            this.reader = reader;
            this.reader.SetEngine(this);
            this.minorInterval = minorInterval;
            this.majorInterval = majorInterval;

            this.reader.ConnectionStatusEvent += connected =>
            {
                if (ConnectionStatusEvent != null)
                {
                    ConnectionStatusEvent(connected);
                }
            };

            if (!reader.IsTriggerMode())
            {
                this.secondTimer = new Timer(new TimeSpan(0, 0, 1).TotalMilliseconds);
                this.secondTimer.Elapsed += OnSecond;

                this.minorTimer = new Timer(minorInterval.TotalMilliseconds);
                this.minorTimer.Elapsed += OnMinorInterval;
                nextMinor = DateTime.Now + TimeSpan.FromMilliseconds(this.minorTimer.Interval);

                this.majorTimer = new Timer(majorInterval.TotalMilliseconds);
                this.majorTimer.Elapsed += OnMajorInterval;
                nextMajor = DateTime.Now + TimeSpan.FromMilliseconds(this.majorTimer.Interval);
            }
        }
        
        public void RegisterListener(IMeterListener listener)
        {
            if(listener == null)
                return;
            logger.Debug("Registering new meter listener {0}.", listener);
            lock (this.listeners)
            {
                listener.NextMajor(nextMajor);
                listener.NextMinor(nextMinor);
                this.listeners.Add(listener);
            }
        }

        public void UnRegisterListener(IMeterListener listener)
        {
            if (listener == null)
                return;

            logger.Debug("Removing meter listener {0}.", listener);
            lock (this.listeners)
            {
                this.listeners.Remove(listener);
            }
        }

        public void Start()
        {
            logger.Debug("Staring the engine.");
            if (this.secondTimer != null)
                this.secondTimer.Enabled = true;
            if(this.minorTimer != null)
                this.minorTimer.Enabled = true;
            if (this.majorTimer != null)
                this.majorTimer.Enabled = true;
        }

        public void Stop()
        {
            logger.Debug("Stopping the engine.");
            if (this.secondTimer != null)
                this.secondTimer.Enabled = false;
            if (this.minorTimer != null)
                this.minorTimer.Enabled = false;
            if (this.majorTimer != null)
                this.majorTimer.Enabled = false;

            if (reader != null)
                reader.Close();

            this.secondTimer = null;
            this.minorTimer = null;
            this.majorTimer = null;
            this.reader = null;
        }

        public void OnMajorInterval(object sender, ElapsedEventArgs e)
        {
            DateTime time = e.SignalTime;
            OnMajorInterval(time);
        }
        public Task OnMajorInterval(DateTime time)
        {
            return Task.Run(async () =>
            {
                try
                {
                    nextMajor = time + majorInterval;
                    logger.Trace("Fetching major reading, next fetch is at {0}.", nextMajor);
                    DateTime start = DateTime.Now;
                    var reading = await this.reader.GetMajorReading();
                    if (reading == null || reading.LAeq < 0)
                    {
                        logger.Warn("Got null as major reading.");
                        return;
                    }
                    DateTime end = DateTime.Now;
                    logger.Trace("Got major reading \"{0}\" in {1} ms.", reading.LAeq, (end - start).TotalMilliseconds);

                    lock (this.listeners)
                    {
                        foreach (var listener in this.listeners)
                        {
                            listener.NextMajor(nextMajor);
                            listener.OnMajor(time, reading);
                        }
                    }
                }
                catch (Exception exp)
                {
                    logger.Error(exp, "Failed on Major Interval");
                }
            });
        }

        public void OnMinorInterval(object sender, ElapsedEventArgs e)
        {
            DateTime time = e.SignalTime;
            OnMinorInterval(time);
        }
        public Task OnMinorInterval(DateTime time)
        {
            return Task.Run(async () =>
            {
                try { 
                    nextMinor = time + minorInterval;
                    logger.Trace("Fetching minor reading, next fetch is at {0}.", nextMinor);
                    DateTime start = DateTime.Now;
                    var reading = await this.reader.GetMinorReading();
                    if (reading == null || reading.LAeq < 0)
                    {
                        logger.Warn("Got null as minor reading.");
                        return;
                    }
                    DateTime end = DateTime.Now;
                    logger.Trace("Got minor reading \"{0}\" in {1} ms.", reading.LAeq, (end - start).TotalMilliseconds);

                    lock (this.listeners)
                    {
                        foreach (var listener in this.listeners)
                        {
                            listener.NextMinor(nextMinor);
                            listener.OnMinor(time, reading);
                        }
                    }
                }
                catch (Exception exp)
                {
                    logger.Error(exp, "Failed on Minor Interval");
                }
            });
        }

        public void OnSecond(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            OnSecond(DateTime.Now);
        }
        public Task OnSecond(DateTime time)
        {
            return Task.Run(async () =>
            {
                try
                {
                    logger.Trace("Fetching second reading.");
                    DateTime start = DateTime.Now;
                    var readingSecond = this.reader.GetSecondReading();
                    var readingMinor = this.reader.GetMinorReading();
                    var readingMajor = this.reader.GetMajorReading();
                    await Task.WhenAll(readingSecond, readingMinor, readingMajor).ConfigureAwait(false);

                    if (readingSecond.Result == null || readingSecond.Result.LAeq < 0)
                    {
                        logger.Warn("Got null as second reading.");
                        return;
                    }
                    
                    DateTime end = DateTime.Now;
                    logger.Trace("Got second reading \"{0}\" in {1} ms.", readingSecond.Result.LAeq, (end - start).TotalMilliseconds);

                    lock (this.listeners)
                    {
                        foreach (var listener in this.listeners)
                        {
                            listener.OnSecond(time, readingSecond.Result, readingMinor.Result, readingMajor.Result);
                        }
                    }
                }
                catch (Exception exp)
                {
                    logger.Error(exp, "Failed on second reading");
                }
            });
        }
        public Task OnSecond(DateTime time, ReadingData second, ReadingData minor, ReadingData major)
        {
            return Task.Run(() =>
            {
                try
                {
                    logger.Trace("Triggered second reading \"{0}\".", second.LAeq);
                    lock (this.listeners)
                    {
                        foreach (var listener in this.listeners)
                        {
                            listener.OnSecond(time, second, minor, major);
                        }
                    }
                }
                catch (Exception exp)
                {
                    logger.Error(exp, "Failed on second reading");
                }
            });
        }

        public event ConnectionStatusUpdateDeligate ConnectionStatusEvent;
    }
}
