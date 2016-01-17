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

        public RemoteMeterReader(string ip, int port)
        {
            this.ip = ip;
            this.port = port;
            StartRemoveReader();
        }

        private Task StartRemoveReader()
        {
            return Task.Factory.StartNew(() =>
            {
                try
                {
                    TcpClient client = new TcpClient(ip, port);
                    try
                    {
                        using (StreamReader reader = new StreamReader(client.GetStream(), Encoding.UTF8))
                        {
                            string line;
                            while ((line = reader.ReadLineAsync().Result) != null)
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
                                    this.engine.OnMinorInterval(DateTime.Now);
                                }
                                else if (line.StartsWith(TCPServerListener.TcpMessages.OnMajorResponse.Replace("{0}", "")))
                                {
                                    var json =
                                        line.Substring(
                                            TCPServerListener.TcpMessages.OnMajorResponse.Replace("{0}", "").Length);
                                    this.lastMajorReading = JsonConvert.DeserializeObject<ReadingData>(json);
                                    this.engine.OnMajorInterval(DateTime.Now);
                                }
                                else if (
                                    line.StartsWith(TCPServerListener.TcpMessages.OnSecondResponse.Replace(
                                        "{0}", "")))
                                {
                                    var json =
                                        line.Substring(
                                            TCPServerListener.TcpMessages.OnSecondResponse.Replace("{0}", "")
                                                .Length);
                                    this.lastSecondReading = JsonConvert.DeserializeObject<ReadingData>(json);
                                    this.engine.OnSecond(DateTime.Now);
                                }
                            }
                        }
                    }
                    catch (Exception exp)
                    {
                        logger.Warn("Connection to remote device {0}:{1} lost, trying to reconnect.", ip, port);
                        Task.Delay(250).RunSynchronously();
                        client = new TcpClient(ip, port);
                    }
                }
                catch (Exception exp)
                {
                    logger.Error("Unable to make connection to remote server {0}:{1}.", ip, port);
                }
            });
        }

        public Task<ReadingData> GetSecondReading()
        {
            return Task.FromResult(this.lastSecondReading);
        }

        public Task<ReadingData> GetMinorReading()
        {
            return Task.FromResult(this.lastMinorReading);
        }

        public Task<ReadingData> GetMajorReading()
        {
            return Task.FromResult(this.lastMajorReading);
        }

        public void SetEngine(AudioViewEngine engine)
        {
            this.engine = engine;
        }

        public bool IsTriggerMode()
        {
            return true;
        }

        public static Task<MeasurementSettings> TestConenction(string ip, int port)
        {
            return Task.Factory.StartNew(() =>
            {
                try
                {
                    CancellationTokenSource tokenSource = new CancellationTokenSource();
                    tokenSource.CancelAfter(new TimeSpan(0,0,30));

                    TcpClient client = new TcpClient(ip, port);
                    NetworkStream networkStream = client.GetStream();
                    StreamWriter streamWriter = new StreamWriter(networkStream);
                    streamWriter.WriteLine(TCPServerListener.TcpMessages.GetSettings);
                    streamWriter.Flush();
                    using (StreamReader reader = new StreamReader(client.GetStream(), Encoding.UTF8))
                    {
                        string line;
                        while ((line = reader.ReadLineAsync().WithCancellation(tokenSource.Token).Result) != null)
                        {
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
