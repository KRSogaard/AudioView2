using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NLog;

namespace AudioView.Common.Meter
{
    public class NetworkMeterClient
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private const int UdpPackge = 13123;
        private const string FindDevicesMessage = "Report Back";

        /// <summary>
        /// Perform a reading from the meter
        /// </summary>
        /// <returns>The meter reading</returns>
        public MeterReading GetReading()
        {
            return new MeterReading();
        }

        public static Task<List<Tuple<string, IPEndPoint>>> SearchForDevices(TimeSpan? searchTime = null)
        {
            TimeSpan timeOut = searchTime ?? new TimeSpan(0, 0, 10);
            return Task.Run(async () =>
            {
                List<Tuple<string, IPEndPoint>> result = new List<Tuple<string, IPEndPoint>>();

                IPEndPoint groupEP = new IPEndPoint(IPAddress.Broadcast, UdpPackge);
                var udpClient = new UdpClient();
                string welcome = "Hello, are you there?";
                var data = Encoding.ASCII.GetBytes(welcome);
                udpClient.SendAsync(data, data.Length, groupEP); // TODO await?

                var token = new CancellationTokenSource();
                token.CancelAfter(timeOut);
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        var receiveBytes =
                            await udpClient.ReceiveAsync().WithCancellation(token.Token).ConfigureAwait(false);
                        var message = Encoding.ASCII.GetString(receiveBytes.Buffer);
                        logger.Info("Report back {0}.", message);
                        var messageSplit = message.Split(';');
                        var endPoint = new IPEndPoint(IPAddress.Parse(messageSplit[1]), int.Parse(messageSplit[2]));
                        result.Add(new Tuple<string, IPEndPoint>(messageSplit[0], endPoint));
                    }
                    catch (Exception exp)
                    {
                        logger.Error(exp, "Error while parsing UDP report back.");
                    }
                }

                return result;
            });
        }
    }
}
