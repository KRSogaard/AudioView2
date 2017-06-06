using System;
using System.Collections.Generic;
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
        private TimeSpan minorInterval;
        private TimeSpan majorInterval;
        private IMeterReader reader;
        private List<IMeterListener> listeners;
        private DateTime nextMajor;
        private DateTime nextMinor;
        private DateTime startTime;

        private DateTime minorIntervalStarted;
        private DateTime majorIntervalStarted;

        private IntervalTimer majorIntervalTimer;
        private IntervalTimer minorIntervalTimer;

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

                minorIntervalTimer = new IntervalTimer(minorInterval);
                majorIntervalTimer = new IntervalTimer(majorInterval);
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
                if (nextMinor > DateTime.Now)
                {
                    logger.Debug("Informing " + listener.GetType().Name + " that next minor interval is at " + nextMinor);
                    listener.NextMinor(nextMinor);
                }
                if (nextMajor > DateTime.Now)
                {
                    logger.Debug("Informing " + listener.GetType().Name + " that next major interval is at " + nextMajor);
                    listener.NextMajor(nextMajor);
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
            secondTimer.Start();

            var minorTimerStart = minorIntervalTimer.Start((triggered, nextInterval) =>
            {
                logger.Info("Minor interval triggered. Triggered: " + triggered + " Next: " + nextInterval);
                nextMinor = nextInterval;
                OnMinorInterval(triggered, nextInterval);
            }, (triggered, nextInterval) =>
            {
                logger.Info("Minor interval Started. Triggered: " + triggered + " Next: " + nextInterval);
                nextMinor = nextInterval;
                minorIntervalStarted = triggered;
                lock (this.listeners)
                {
                    foreach (var listener in this.listeners)
                    {
                        logger.Debug("Informing " + listener.GetType().Name + " of next minor interval " + nextInterval);
                        listener.NextMinor(nextInterval);
                    }
                }
            });

            logger.Debug("Minor interval will start at " + minorTimerStart);
            nextMinor = minorTimerStart;
            lock (this.listeners)
            {
                foreach (var listener in this.listeners)
                {
                    logger.Debug("Informing " + listener.GetType().Name + " of minor start time " + minorTimerStart);
                    listener.NextMinor(minorTimerStart);
                }
            }


            var majorTimerStart = majorIntervalTimer.Start((triggered, nextInterval) =>
            {
                logger.Info("Major interval triggered. Triggered: " + triggered + " Next: " + nextInterval);
                nextMajor = nextInterval;
                OnMajorInterval(triggered, nextInterval);
            }, (triggered, nextInterval) =>
            {
                logger.Info("Major interval Started. Triggered: " + triggered + " Next: " + nextInterval);
                nextMajor = nextInterval;
                majorIntervalStarted = triggered;
                lock (this.listeners)
                {
                    foreach (var listener in this.listeners)
                    {
                        logger.Debug("Informing " + listener.GetType().Name + " of next major interval " + nextInterval);
                        listener.NextMajor(nextInterval);
                    }
                }
            });

            logger.Debug("Major interval will start at " + majorTimerStart);
            nextMajor = majorTimerStart;
            lock (this.listeners)
            {
                foreach (var listener in this.listeners)
                {
                    logger.Debug("Informing " + listener.GetType().Name + " of major start time.");
                    listener.NextMajor(majorTimerStart);
                }
            }
        }

        public void Stop()
        {
            logger.Debug("Stopping the engine.");
            if (this.secondTimer != null)
                this.secondTimer.Enabled = false;
            if (this.minorIntervalTimer != null)
                this.minorIntervalTimer.Stop();
            if (this.majorIntervalTimer != null)
                this.majorIntervalTimer.Stop();

            if (reader != null)
                reader.Close();

            this.secondTimer = null;
            this.minorIntervalTimer = null;
            this.majorIntervalTimer = null;
            this.reader = null;
        }
        
        public Task OnMajorInterval(DateTime time, DateTime nextInterval)
        {
            return Task.Run(async () =>
            {
                try
                {
                    DateTime mesurementSpanStart = time - majorInterval;
                    logger.Trace("Fetching major reading, next fetch is at {0}.", nextInterval);
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
                            // Remote will give null
                            if (nextInterval == null)
                                listener.NextMajor(time + majorInterval);
                            else
                                listener.NextMajor(nextInterval);
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

        public Task OnMinorInterval(DateTime time, DateTime nextInterval)
        {
            return Task.Run(async () =>
            {
                try
                {
                    DateTime mesurementSpanStart = time - minorInterval;
                    logger.Trace("Fetching minor reading, next fetch is at {0}.", nextInterval);
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
                            listener.NextMinor(nextInterval);
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
    }
}
