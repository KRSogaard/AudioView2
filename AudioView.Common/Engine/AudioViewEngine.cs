using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;
using AudioView.Common.Data;
using NLog;

namespace AudioView.Common.Engine
{
    public delegate void EngineStartDelayedDeligate(TimeSpan delay);
    public delegate void EngineStartedDeligate();

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
        private DateTime startTime;

        private DateTime minorIntervalStarted;
        private DateTime majorIntervalStarted;

        protected AudioViewEngine()
        {
            
        }
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
                
                this.majorTimer = new Timer(majorInterval.TotalMilliseconds);
                this.majorTimer.Elapsed += OnMajorInterval;
            }
        }
        
        public void RegisterListener(IMeterListener listener)
        {
            if(listener == null)
                return;
            logger.Debug("Registering new meter listener {0}.", listener);
            lock (this.listeners)
            {
                // We add this safty as listernes might be registered before
                // we know the next major time
                if (nextMajor > DateTime.Now)
                {
                    listener.NextMajor(nextMajor);
                }
                if (nextMinor > DateTime.Now)
                {
                    listener.NextMinor(nextMinor);
                }
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
            logger.Debug("Preparing engine to start.");
            var nextFullMin = GetNextFullMinute();
            startTime = nextFullMin;
            lock (this.listeners)
            {
                foreach (var listener in this.listeners)
                {
                    listener.NextMinor(nextFullMin);
                }
            }
            WaitUntil(nextFullMin).ContinueWith((task) =>
            {
                logger.Debug("Staring the engine.");
                if (this.secondTimer != null)
                    this.secondTimer.Enabled = true;
                if (this.minorTimer != null)
                    this.minorTimer.Enabled = true;

                EngineStartedEvent?.Invoke();
                
                if (!reader.IsTriggerMode())
                {
                    nextMinor = nextFullMin + TimeSpan.FromMilliseconds(this.minorTimer.Interval);
                    lock (this.listeners)
                    {
                        foreach (var listener in this.listeners)
                        {
                            listener.NextMinor(nextMinor);
                        }
                    }
                }
            });

            var nextMajorInterval = GetNextInterval(majorInterval);
            lock (this.listeners)
            {
                foreach (var listener in this.listeners)
                {
                    listener.NextMajor(nextMajorInterval);
                }
            }
            WaitUntil(nextMajorInterval).ContinueWith((innerTask) =>
            {
                if (this.majorTimer != null)
                    this.majorTimer.Enabled = true;

                nextMajor = nextMajorInterval + TimeSpan.FromMilliseconds(this.majorTimer.Interval);
                lock (this.listeners)
                {
                    foreach (var listener in this.listeners)
                    {
                        listener.NextMajor(nextMajor);
                    }
                }
            });
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
            logger.Debug("Got Major interval");
            // There might be some minor drift, this will snap to the correct interval
            DateTime time = RoundToNearest(startTime, e.SignalTime, majorInterval);
            logger.Debug("Snapping Major interval to {0}", time);
            OnMajorInterval(time);
        }
        public Task OnMajorInterval(DateTime time)
        {
            return Task.Run(async () =>
            {
                try
                {
                    DateTime mesurementSpanStart = time - majorInterval;
                    nextMajor = time + majorInterval;
                    logger.Trace("Fetching major reading, next fetch is at {0}.", nextMajor);
                    DateTime start = DateTime.Now;
                    var reading = await this.reader.GetMajorReading(majorIntervalStarted);
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
                            start = DateTime.Now;
                            listener.NextMajor(nextMajor);
                            listener.OnMajor(time, mesurementSpanStart, reading);
                            end = DateTime.Now;
                            logger.Debug("On Minor listener \"{0}\" took {1} ms.", listener.GetType(),
                                (end - start).TotalMilliseconds);
                        }
                    }
                }
                catch (Exception exp)
                {
                    logger.Error(exp, "Failed on Major Interval");
                }
                finally
                {
                    majorIntervalStarted = DateTime.Now;
                }
            });
        }

        public void OnMinorInterval(object sender, ElapsedEventArgs e)
        {
            logger.Debug("Got Minor interval");
            // There might be some minor drift, this will snap to the correct interval
            DateTime time = RoundToNearest(startTime, e.SignalTime, minorInterval);
            logger.Debug("Snapping Minor interval to {0}", time);
            OnMinorInterval(time);
        }
        public Task OnMinorInterval(DateTime time)
        {
            return Task.Run(async () =>
            {
                try
                {
                    DateTime mesurementSpanStart = time - minorInterval;
                    nextMinor = time + minorInterval;
                    logger.Trace("Fetching minor reading, next fetch is at {0}.", nextMinor);
                    DateTime start = DateTime.Now;
                    var reading = await this.reader.GetMinorReading(minorIntervalStarted);
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
                            start = DateTime.Now;
                            listener.NextMinor(nextMinor);
                            listener.OnMinor(time, mesurementSpanStart, reading);
                            end = DateTime.Now;
                            logger.Debug("On Minor listener \"{0}\" took {1} ms.", listener.GetType(), (end - start).TotalMilliseconds);
                        }
                    }
                }
                catch (Exception exp)
                {
                    logger.Error(exp, "Failed on Minor Interval");
                }
                finally
                {
                    minorIntervalStarted = DateTime.Now;
                }
            });
        }

        public void OnSecond(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            logger.Trace("Second reading triggered at");
            OnSecond(DateTime.Now);
        }
        public Task OnSecond(DateTime time)
        {
            return Task.Run(async () =>
            {
                try
                {
                    DateTime mesurementSpanStart = time - TimeSpan.FromSeconds(1);
                    logger.Trace("Fetching second reading.");
                    DateTime start = DateTime.Now;
                    var readingSecond = this.reader.GetSecondReading();
                    var readingMinor = this.reader.GetMinorReading(minorIntervalStarted);
                    var readingMajor = this.reader.GetMajorReading(majorIntervalStarted);
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
                            start = DateTime.Now;
                            listener.OnSecond(time, mesurementSpanStart, readingSecond.Result, readingMinor.Result, readingMajor.Result);
                            end = DateTime.Now;
                            logger.Trace("On Second listener \"{0}\" took {1} ms.", listener.GetType(), (end - start).TotalMilliseconds);
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
                    DateTime mesurementSpanStart = time - TimeSpan.FromSeconds(1);
                    logger.Trace("Triggered second reading \"{0}\".", second.LAeq);
                    lock (this.listeners)
                    {
                        foreach (var listener in this.listeners)
                        {
                            listener.OnSecond(time, mesurementSpanStart, second, minor, major);
                        }
                    }
                }
                catch (Exception exp)
                {
                    logger.Error(exp, "Failed on second reading");
                }
            });
        }

        private DateTime GetNextFullMinute()
        {
            var nextFullMin = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, 0)
                            .AddMinutes(1);
            return nextFullMin;
        }

        private DateTime GetNextInterval(TimeSpan interval)
        {
            var next = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour,
                0, 0);
            while (next < DateTime.Now)
            {
                next += interval;
            }
            return next;
        }

        private Task WaitUntil(DateTime dateTime)
        {
            var waitTime = dateTime - DateTime.Now;

            logger.Debug("Engine will start in " + waitTime);
            EngineStartDelayedEvent?.Invoke(waitTime);
            return Task.Delay(waitTime);
        }

        protected DateTime RoundToNearest(DateTime start, DateTime dt, TimeSpan span)
        {
            long ticks = dt.Ticks - start.Ticks;
            decimal spans = Decimal.Divide(ticks, span.Ticks);

            long lowTicks = start.Ticks + Convert.ToInt64(span.Ticks * Math.Floor(spans));
            long highTicks = start.Ticks + Convert.ToInt64(span.Ticks * Math.Ceiling(spans));

            long sinceLow = Math.Abs(dt.Ticks - lowTicks);
            long toHihg = Math.Abs(dt.Ticks - highTicks);

            if (sinceLow < toHihg)
            {
                return new DateTime(lowTicks, dt.Kind);
            }
            return new DateTime(highTicks, dt.Kind);
        }

        public event ConnectionStatusUpdateDeligate ConnectionStatusEvent;
        public event EngineStartDelayedDeligate EngineStartDelayedEvent;
        public event EngineStartedDeligate EngineStartedEvent;
    }
}
