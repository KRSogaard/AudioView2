using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using AudioView.Common.Data;
using AudioView.Common.Listeners;
using Newtonsoft.Json;
using NLog;

namespace AudioView.Common.Engine
{
    public class RemoteMeterReader : IMeterReader
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private string ip;
        private int port;
        private AudioViewEngine engine;
        private ReadingData lastMinorReading;
        private ReadingData lastMajorReading;
        private ReadingData lastSecondReading;
        private ReadingData lastBuldingMinorReading;
        private ReadingData lastBuldingMajorReading;
        private bool lastConnectionStatus;
        private bool run;

        public RemoteMeterReader(string ip, int port)
        {
            this.ip = ip;
            this.port = port;
            StartRemoveReader();
            lastConnectionStatus = false;
            run = true;
        }

        public Task Close()
        {
            return Task.Run(() =>
            {
                run = false;
            });
        }

        private Task StartRemoveReader()
        {
            return Task.Run(async () =>
            {
                while (run)
                {
                    try
                    {
                        TcpClient client = new TcpClient(ip, port);
                        try
                        {
                            OnConnectionStatus(true);
                            using (StreamReader reader = new StreamReader(client.GetStream(), Encoding.UTF8))
                            {
                                CancellationTokenSource token = new CancellationTokenSource();
                                token.CancelAfter(new TimeSpan(0, 0, 15)); // If no message after 15 sek = disconnected
                                string line;
                                while ((line = await reader.ReadLineAsync().WithCancellation(token.Token).ConfigureAwait(false)) != null)
                                {
                                    if (this.engine == null)
                                    {
                                        continue;
                                    }

                                    if (line.StartsWith(TCPServerListener.TcpMessages.OnMinorResponse.Replace("{0}", "")))
                                    {
                                        var json =
                                            line.Substring(
                                                TCPServerListener.TcpMessages.OnMinorResponse.Replace("{0}", "").Length);
                                        this.lastMinorReading = JsonConvert.DeserializeObject<ReadingData>(json);
                                        // TODO: Fix the next interval time
                                        this.engine.OnMinorInterval(DateTime.Now, DateTime.Now);
                                    }
                                    else if (
                                        line.StartsWith(TCPServerListener.TcpMessages.OnMajorResponse.Replace("{0}", "")))
                                    {
                                        var json =
                                            line.Substring(
                                                TCPServerListener.TcpMessages.OnMajorResponse.Replace("{0}", "")
                                                    .Length);
                                        this.lastMajorReading = JsonConvert.DeserializeObject<ReadingData>(json);
                                        // TODO: Fix the next interval time
                                        this.engine.OnMajorInterval(DateTime.Now, DateTime.Now);
                                    }
                                    else if (
                                        line.StartsWith(TCPServerListener.TcpMessages.OnSecondResponse.Replace("{0}", "")))
                                    {
                                        var json =
                                            line.Substring(
                                                TCPServerListener.TcpMessages.OnSecondResponse.Replace("{0}", "")
                                                    .Length);
                                        var wrapper = JsonConvert.DeserializeObject<TcpWrapperOnSecond>(json);
                                        this.lastSecondReading = wrapper.Second;
                                        this.engine.OnSecond(DateTime.Now, wrapper.Second, wrapper.Minor, wrapper.Major);
                                    }

                                    token.CancelAfter(new TimeSpan(0, 0, 15));
                                    // If no message after 15 sek = disconnected
                                }
                            }
                        }
                        catch (Exception exp)
                        {
                            OnConnectionStatus(false);
                            logger.Warn(exp, "Connection to remote device {0}:{1} lost, trying to reconnect.", ip, port);
                            // Try and close it just to be sure.
                            try
                            {
                                client.Close();
                            }
                            catch
                            {
                            }
                            client = new TcpClient(ip, port);
                        }
                    }
                    catch (Exception exp)
                    {
                        logger.Error(exp, "Unable to make connection to remote server {0}:{1}.", ip, port);
                    }
                }
                OnConnectionStatus(false);
            });
        }

        public Task<ReadingData> GetSecondReading()
        {
            return Task.FromResult(this.lastSecondReading);
        }

        public Task<ReadingData> GetMinorReading(DateTime intervalStarted)
        {
            return Task.FromResult(this.lastMinorReading);
        }

        public Task<ReadingData> GetMajorReading(DateTime intervalStarted)
        {
            return Task.FromResult(this.lastMajorReading);
        }

        public void SetMinorInterval(TimeSpan interval)
        {
        }

        public void SetMajorInterval(TimeSpan interval)
        {
        }

        public void SetEngine(AudioViewEngine engine)
        {
            this.engine = engine;
        }

        public bool IsTriggerMode()
        {
            return true;
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

        public static Task<MeasurementSettings> TestConenction(string ip, int port)
        {
            return Task.Run(async () =>
            {
                try
                {
                    CancellationTokenSource tokenSource = new CancellationTokenSource();
                    tokenSource.CancelAfter(new TimeSpan(0, 0, 10));

                    TcpClient client = new TcpClient(ip, port);
                    NetworkStream networkStream = client.GetStream();
                    StreamWriter streamWriter = new StreamWriter(networkStream);
                    streamWriter.WriteLine(TCPServerListener.TcpMessages.GetSettings);
                    streamWriter.Flush();
                    using (StreamReader reader = new StreamReader(client.GetStream(), Encoding.UTF8))
                    {
                        string line;
                        while ((line = await reader.ReadLineAsync().WithCancellation(tokenSource.Token).ConfigureAwait(false)) != null)
                        {
                            logger.Trace("Got message \"{0}\" while testing connection", line.Trim());
                            if (line.StartsWith(TCPServerListener.TcpMessages.SettingsResponse.Replace("{0}", "")))
                            {
                                var json =
                                    line.Substring(
                                        TCPServerListener.TcpMessages.SettingsResponse.Replace("{0}", "").Length);
                                return JsonConvert.DeserializeObject<MeasurementSettings>(json);
                            }
                        }
                    }

                    return null;
                }
                catch (Exception exp)
                {
                    logger.Info(exp, "Failed to conencted to {0}:{1}", ip, port);
                    return null;
                }
            });
        }
    }
}
