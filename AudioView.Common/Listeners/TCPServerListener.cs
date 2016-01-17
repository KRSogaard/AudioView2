using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AudioView.Common.Data;
using AudioView.Common.Engine;
using Newtonsoft.Json;
using NLog;

namespace AudioView.Common.Listeners
{
    public class TCPServerListener : IMeterListener
    {
        public class TcpMessages
        {
            public const string GetSettings = "Get Settings";
            public const string SettingsResponse = "Settings:{0}";
            public const string OnMinorResponse = "OnMinor:{0}";
            public const string OnMajorResponse = "OnMajor:{0}";
            public const string OnSecondResponse = "OnSecond:{0}";
            public const string StopListenerResponse = "StopListener";
        }

        private static Logger logger = LogManager.GetCurrentClassLogger();
        private MeasurementSettings settings;
        private TcpListener tcpServer;
        private bool runServer;
        private CancellationTokenSource cancellationToken;
        private LinkedList<TcpClient> tcpClients; 

        public TCPServerListener(MeasurementSettings settings)
        {
            this.settings = settings;
            this.runServer = true;
            this.cancellationToken = new CancellationTokenSource();
            this.tcpClients = new LinkedList<TcpClient>();
            RunServerAsync();
        }

        /// <summary>
        /// Method to be used on seperate thread.
        /// </summary>
        private Task RunServerAsync()
        {
            return Task.Factory.StartNew(async () =>
            {
                try
                {
                    logger.Info("Starting tcp server on port {0}", settings.Port);
                    var listener = new TcpListener(IPAddress.Any, settings.Port);
                    listener.Start();
                    while (runServer)
                    {
                        try
                        {
                            var client = await listener.AcceptTcpClientAsync().WithCancellation(cancellationToken.Token);
                            logger.Info("Got new TCP client from {0}", client.Client.RemoteEndPoint as IPEndPoint);
                            tcpClients.AddLast(client);
                            HandleClientAsync(client);
                        }
                        catch (Exception exp)
                        {
                            logger.Error(exp, "Exception while waiting for new TCP client.");
                        }
                    }
                }
                catch (Exception exp)
                {
                    logger.Error(exp, "Exception starting Tcp Server.");
                }
            });
        }

        private Task HandleClientAsync(TcpClient client)
        {
            return Task.Factory.StartNew(async () =>
            {
                while (runServer)
                {
                    try
                    {
                        using (StreamReader reader = new StreamReader(client.GetStream(), Encoding.UTF8))
                        {
                            string line;
                            while ((line = await reader.ReadLineAsync()) != null)
                            {
                                HandelRequest(client, line.Trim());
                            }
                        }
                    }
                    catch (Exception exp)
                    {
                        if (!client.Connected)
                        {
                            logger.Info("Client disconnected.");
                            return;
                        }
                        logger.Error(exp, "Exception while reading from client {0}.", client.Client.RemoteEndPoint);
                    }
                }
            });
        }

        private void HandelRequest(TcpClient client, string message)
        {
            switch (message)
            {
                case TcpMessages.GetSettings:
                        SendMessage(client, string.Format(TcpMessages.SettingsResponse, JsonConvert.SerializeObject(this.settings)));
                    break;
            }
        }

        private void SendMessage(TcpClient client, string message)
        {
            logger.Debug("Sending {0} to {1}.", message, client.Client.RemoteEndPoint);
            if (client.Connected)
            {
                NetworkStream networkStream = client.GetStream();
                StreamWriter streamWriter = new StreamWriter(networkStream);
                streamWriter.WriteLine(message);
                streamWriter.Flush();
            }
            else
            {
                logger.Error("Client is not conencted, can not send message to {0}.", client.Client.RemoteEndPoint);
            }
        }

        private void SendMessageToAll(string message)
        {
            List<TcpClient> toRemove = new List<TcpClient>();
            lock (tcpClients)
            {
                foreach (var tcpClient in tcpClients)
                {
                    if (!tcpClient.Connected)
                    {
                        logger.Info("Client have disconnected, removing the client.");
                        tcpClient.Close();
                        toRemove.Add(tcpClient);
                        continue;
                    }

                    SendMessage(tcpClient, message);
                }

                if (toRemove.Count > 0)
                {
                    logger.Debug("Removing {0} clients.", toRemove.Count);
                    foreach (var tcpClient in toRemove)
                    {
                        tcpClients.Remove(tcpClient);
                    }
                }
            }
        }
        

        public Task OnMinor(DateTime time, ReadingData data)
        {
            return Task.Factory.StartNew(() =>
            {
                SendMessageToAll(string.Format(TcpMessages.OnMinorResponse, JsonConvert.SerializeObject(data)));
            });
        }

        public Task OnMajor(DateTime time, ReadingData data)
        {
            return Task.Factory.StartNew(() =>
            {
                SendMessageToAll(string.Format(TcpMessages.OnMajorResponse, JsonConvert.SerializeObject(data)));
            });
        }

        public Task OnSecond(DateTime time, ReadingData data)
        {
            return Task.Factory.StartNew(() =>
            {
                SendMessageToAll(string.Format(TcpMessages.OnSecondResponse, JsonConvert.SerializeObject(data)));
            });
        }

        public Task NextMinor(DateTime time)
        {
            return Task.FromResult<object>(null);
        }

        public Task NextMajor(DateTime time)
        {
            return Task.FromResult<object>(null);
        }

        public Task StopListener()
        {
            return Task.Factory.StartNew(() =>
            {
                SendMessageToAll(string.Format(TcpMessages.StopListenerResponse));

                this.runServer = false;
                this.cancellationToken.Cancel();
                foreach (var tcpClient in this.tcpClients)
                {
                    try
                    {
                        tcpClient.Close();
                    }
                    catch (Exception exp)
                    {
                        logger.Error(exp, "Failed to close tcp client {0}", tcpClient.Client.RemoteEndPoint);
                    }
                }
            });
        }
    }
}
