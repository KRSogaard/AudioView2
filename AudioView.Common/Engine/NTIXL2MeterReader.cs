using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AudioView.Common.Data;
using System.IO.Ports;
using System.Threading;
using NLog;
using RJCP.IO.Ports;

namespace AudioView.Common.Engine
{
    public class NTIXL2Commands
    {
        // Meter Commands
        public const string Query = "*IDN?";
        public const string ResetSLM = "*RST";
        public const string StartLog = "INIT START";
        public const string InitiateMeasurement = "MEAS:INIT";
        public const string Measurement = "MEAS:SLM:123:dt?";
        public const string LAEQ = "MEAS:SLM:123:dt? LAEQ";
        public const string LAMIN = "MEAS:SLM:123:dt? LAFMIN";
        public const string LAMAX = "MEAS:SLM:123:dt? LAFMAX";
        public const string LZMIN = "MEAS:SLM:123:dt? LZFMIN";
        public const string LZMAX = "MEAS:SLM:123:dt? LZFMAX";
        public const string LC_PEAK = "MEAS:SLM:123:dt? LCPKMAX";
        public const string Octive = "MEAS:SLM:RTA:DT? EQ";
        public const string ExistingSensitivity = "CALIB:MIC:SENSU:VALU?";
        public const string Stop = "INIT STOP";
        public const string CalibrateMic = "CALIB:MIC:SENS:VALU [{0}]";
        public const string Lock = "SYST:KLOCK ON";
        public const string UnLock = "SYST:KLOCK OFF";
    }

    public class NTIXL2MeterReader : IMeterReader
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private TimeSpan minorInterval;
        private TimeSpan majorInterval;
        private bool lastConnectionStatus;
        private string portName;
        private object deviceLock = new object();
        private int TimeToTimeOut = 1500;
        private SerialPortStream serialPort;
        private readonly LinkedList<Tuple<DateTime,ReadingData>> previousReadings;
        private int timeouts = 0;

        public NTIXL2MeterReader(string portName, int timeout)
        {
            logger.Info("Starting NTI XL2 reading on port {0} with a timeout of {1} ms.", portName, timeout);
            this.portName = portName;
            TimeToTimeOut = timeout;
            previousReadings = new LinkedList<Tuple<DateTime, ReadingData>>();
        }

        public Task<ReadingData> GetSecondReading()
        {
            return Task.Run(() =>
            {
                ReadingData reading = new ReadingData();
                lock (deviceLock)
                {
                    logger.Trace("Performing seconds reading from NTI XL2 on port {0}", portName);
                    if (!DeviceAction((serialPort, token) =>
                    {
                        token.ThrowIfCancellationRequested();

                        // Initiate first measurement
                        this.WriteLine(serialPort, NTIXL2Commands.InitiateMeasurement);
                        if(token.IsCancellationRequested)
                            return;
                        // ask for leq
                        this.WriteLine(serialPort, NTIXL2Commands.Measurement + " LAEQ LAFMAX LAFMIN LZFMAX LZFMIN");
                        if (token.IsCancellationRequested)
                            return;

                        // getting LEQ
                        reading.LAeq = NTIXL2Utilities.ParseMeasurement(ReadToDB(serialPort));
                        reading.LAMax = NTIXL2Utilities.ParseMeasurement(ReadToDB(serialPort));
                        reading.LAMin = NTIXL2Utilities.ParseMeasurement(ReadToDB(serialPort));
                        reading.LZMax = NTIXL2Utilities.ParseMeasurement(ReadToDB(serialPort));
                        reading.LZMin = NTIXL2Utilities.ParseMeasurement(ReadToDB(serialPort));

                        // Getting EQ
                        this.WriteLine(serialPort, NTIXL2Commands.Octive);
                        reading.LAeqOctaveBand = NTIXL2Utilities.ParseOctive(ReadToDB(serialPort));
                        
                        previousReadings.AddLast(new Tuple<DateTime, ReadingData>(DateTime.Now, reading));
                        logger.Trace("LAeq response {0} from NTI LX2 port {1}.", reading.LAeq, portName);
                    }))
                    {
                        return null;
                    }
                }
                // Null reading
                if (reading.LAeq < 0)
                {
                    return null;
                }
                return reading;
            });
        }

        public Task<ReadingData> GetMinorReading()
        {
            return Task.Run(() =>
            {
                RemoveOldReadings();

                var reading = new ReadingData();
                lock (previousReadings)
                {
                    var minorReadings = previousReadings.Where(x => x.Item1 >= DateTime.Now - minorInterval);
                    if (!minorReadings.Any())
                    {
                        return null;
                    }

                    foreach (var previousReading in minorReadings)
                    {
                        reading = reading + previousReading.Item2;
                    }

                    reading /= minorReadings.Count();
                }

                return reading;
            });
        }

        public Task<ReadingData> GetMajorReading()
        {
            return Task.Run(() =>
            {
                RemoveOldReadings();

                var reading = new ReadingData();
                lock (previousReadings)
                {
                    if (previousReadings.Count == 0)
                    {
                        return null;
                    }
                    foreach (var previousReading in previousReadings)
                    {
                        reading = reading + previousReading.Item2;
                    }

                    reading /= previousReadings.Count;
                }

                return reading;
            });
        }

        private void RemoveOldReadings()
        {
            lock(previousReadings)
            {
                while (previousReadings.First != null)
                {
                    if (previousReadings.First.Value.Item1 < DateTime.Now - majorInterval)
                    {
                        previousReadings.RemoveFirst();
                    }
                    else
                    {
                        return;
                    }
                }
            }
        }

        private bool DeviceAction(Action<SerialPortStream, CancellationToken> action)
        {
            CancellationToken token = new CancellationToken();
            DateTime start = DateTime.Now;
            var task = Task.Factory.StartNew(() =>
            {
                try
                {
                    if (serialPort == null || !serialPort.IsOpen)
                    {
                        if (serialPort != null && serialPort.IsOpen)
                        {
                            performClose();
                        }
                        
                        serialPort = new SerialPortStream(this.portName);
                        serialPort.Open();
                        Start(serialPort);
                    }

                    
                    // discard the input buffer after
                    if (serialPort.ReadBufferSize > 0)
                    {
                        try
                        {
                            serialPort.DiscardInBuffer();
                        }
                        catch (Exception exp)
                        {
                            logger.Warn(exp, "Failed to clear in buffer for NTI XL2 on port {0}", portName);
                        }
                    }
                    if (serialPort.WriteBufferSize > 0)
                    {
                        try
                        {
                            serialPort.DiscardOutBuffer();
                        }
                        catch (Exception exp)
                        {
                            logger.Warn(exp, "Failed to clear in buffer for NTI XL2 on port {0}", portName);
                        }
                    }

                    action(serialPort, token);
                    OnConnectionStatus(true);
                }
                catch (Exception exp)
                {
                    OnConnectionStatus(false);
                    logger.Error(exp, "Failed to execute NTI XL2 action on port {0}.", portName);
                    performClose();
                    return false;
                }

                return true;
            }, token);

            if (!task.Wait(this.TimeToTimeOut, token))
            {
                timeouts++;
                logger.Error("NTI XL2 device communication timed out after {0} ms.", (DateTime.Now - start).TotalMilliseconds);
                OnConnectionStatus(false);
                token = new CancellationToken();
                if (timeouts >= 2)
                {
                    timeouts = 0;
                    var closeTask = Task.Factory.StartNew(() =>
                    {
                        performClose(true);
                    }, token);
                    if (!closeTask.Wait(this.TimeToTimeOut, token))
                    {
                        logger.Trace("Time out trying to close the connection after a time out.");
                    }
                }
                return false;
            }
            return task.Result;
        }

        private void performClose(bool afterDisconnect = false)
        {
            logger.Trace("Closing connection to NTI Xl2 on port {0}.", portName);
            if (!afterDisconnect)
            {
                try
                {
                    Close(serialPort);
                }
                catch (Exception exp)
                {
                    logger.Warn(exp, "Failed to close commands to NTI XL2 connected on port {0}.", portName);
                }
            }

            try
            {
                serialPort.Close();
            }
            catch (Exception exp)
            {
                logger.Warn(exp, "Failed to close serial port {0} conenction to NTI XL2.", portName);
            }
        }

        private void Start(SerialPortStream serialPort)
        {
            // Rest the meter
            this.WriteLine(serialPort, NTIXL2Commands.ResetSLM);
            // Lock the keyboard
            //this.WriteLine(serialPort, NTIXL2Commands.Lock);
            // Start the loggin
            this.WriteLine(serialPort, NTIXL2Commands.StartLog);
            // Dt is between two measutments, if we don't do this will the first be -999
            this.WriteLine(serialPort, NTIXL2Commands.InitiateMeasurement);
        }

        private void Close(SerialPortStream serialPort)
        {
            // Lock the keyboard
            //this.WriteLine(serialPort, NTIXL2Commands.UnLock);
            // Stop the sound level meter
            this.WriteLine(serialPort, NTIXL2Commands.Stop);
        }

        private void WriteLine(SerialPortStream serialPort, string line)
        {
            if (serialPort == null || !serialPort.IsOpen)
            {
                return;
            }

            logger.Trace("Sending \"{0}\" to NTI XL2 on port {1}", line, portName);
            serialPort.WriteLine(line);
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

        public Task<bool> testDevice()
        {
            return Task.Run(async () =>
            {
                var result = await GetSecondReading() != null;
                performClose();
                return result;
            });
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


        public static Task<List<string>> FindPorts()
        {
            return Task.Run(async () =>
            {
                var testedPorts = new List<string>();

                var portNames = SerialPort.GetPortNames();
                foreach (var portName in portNames)
                {
                    var device = new NTIXL2MeterReader(portName, 1500);
                    if (await device.testDevice())
                    {
                        testedPorts.Add(portName);
                    }
                }
                return testedPorts;
            });
        }

        /// <summary>
        /// Read to the line db, and disgards rest of the buffer
        /// </summary>
        /// <returns></returns>
        private string ReadToDB(SerialPortStream serialPort)
        {
            string result = string.Empty;
            // get the last line
            try
            {
                result = serialPort.ReadLine();
            }
            catch (Exception exp)
            {
                logger.Error(exp, "Failed to read the result fro NTI XL2 on port {0}", portName);
                return "-999";
            }

            logger.Trace("Read \"{0}\" from NTI XL2 on port {1}", result.Trim(), portName);

            string db = "-999";
            try
            {
                int dBindex = result.IndexOf("dB");
                if (dBindex > 0)
                {
                    db = result.Substring(0, dBindex).Trim();
                }
                else
                {
                    logger.Warn("Result did not contain the word \"dB\" as expected.");
                }
            }
            catch (Exception exp)
            {
                Console.WriteLine(exp.Message);
            }
            return db;
        }

        public event ConnectionStatusUpdateDeligate ConnectionStatusEvent;
        public Task Close()
        {
            return Task.Run(() =>
            {
                ReadingData reading = new ReadingData();
                lock (deviceLock)
                {
                    logger.Trace("Telling NTI XL2 on port {0} to stop the measurements.", portName);

                    DeviceAction((serialPort, token) =>
                    {
                        token.ThrowIfCancellationRequested();

                        // Initiate first measurement
                        this.WriteLine(serialPort, NTIXL2Commands.Stop);
                    });
                }
                performClose();
            });
        }
    }

    public class NTIXL2Utilities
    {
        /// <summary>
        /// Parses a line and extracts Octive Band data 
        /// </summary>
        /// <param name="r">Result where octive band data needs to be added to</param>
        /// <param name="line">The line of Octive bands</param>
        public static ReadingData.OctaveBand ParseOctive(String line)
        {
            line = line.Split(new string[] { "dB" }, StringSplitOptions.RemoveEmptyEntries).First().Trim();

            // we need to split the octives by a commer
            String[] octives = line.Split(',');
            
            ReadingData.OctaveBand octaveBand = new ReadingData.OctaveBand();
            // assign the values
            octaveBand.Hz6_3 = ParseMeasurement(octives[0]);
            octaveBand.Hz8 = ParseMeasurement(octives[1]);
            octaveBand.Hz10 = ParseMeasurement(octives[2]);
            octaveBand.Hz12_5 = ParseMeasurement(octives[3]);
            octaveBand.Hz16 = ParseMeasurement(octives[4]);
            octaveBand.Hz20 = ParseMeasurement(octives[5]);
            octaveBand.Hz25 = ParseMeasurement(octives[6]);
            octaveBand.Hz31_5 = ParseMeasurement(octives[7]);
            octaveBand.Hz40 = ParseMeasurement(octives[8]);
            octaveBand.Hz50 = ParseMeasurement(octives[9]);
            octaveBand.Hz63 = ParseMeasurement(octives[10]);
            octaveBand.Hz80 = ParseMeasurement(octives[11]);
            octaveBand.Hz100 = ParseMeasurement(octives[12]);
            octaveBand.Hz125 = ParseMeasurement(octives[13]);
            octaveBand.Hz160 = ParseMeasurement(octives[14]);
            octaveBand.Hz200 = ParseMeasurement(octives[15]);
            octaveBand.Hz250 = ParseMeasurement(octives[16]);
            octaveBand.Hz315 = ParseMeasurement(octives[17]);
            octaveBand.Hz400 = ParseMeasurement(octives[18]);
            octaveBand.Hz500 = ParseMeasurement(octives[19]);
            octaveBand.Hz630 = ParseMeasurement(octives[20]);
            octaveBand.Hz800 = ParseMeasurement(octives[21]);
            octaveBand.Hz1000 = ParseMeasurement(octives[22]);
            octaveBand.Hz1250 = ParseMeasurement(octives[23]);
            octaveBand.Hz1600 = ParseMeasurement(octives[24]);
            octaveBand.Hz2000 = ParseMeasurement(octives[25]);
            octaveBand.Hz2500 = ParseMeasurement(octives[26]);
            octaveBand.Hz3150 = ParseMeasurement(octives[27]);
            octaveBand.Hz4000 = ParseMeasurement(octives[28]);
            octaveBand.Hz5000 = ParseMeasurement(octives[29]);
            octaveBand.Hz6300 = ParseMeasurement(octives[30]);
            octaveBand.Hz8000 = ParseMeasurement(octives[31]);
            octaveBand.Hz10000 = ParseMeasurement(octives[32]);
            octaveBand.Hz12500 = ParseMeasurement(octives[33]);
            octaveBand.Hz16000 = ParseMeasurement(octives[34]);
            octaveBand.Hz20000 = ParseMeasurement(octives[35]);
            return octaveBand;
        }

        /// <summary>
        /// Parse the Calibration Result
        /// </summary>
        /// <param name="line">Line to parse</param>
        /// <returns>Double value reporesentation</returns>
        public static Double ParseCalibration(String line)
        {
            return Parse("e-3 V", line);
        }

        /// <summary>
        /// Parses a measurement
        /// </summary>
        /// <param name="line">Measurement taken from the sound level meter</param>
        /// <returns>A Double representation</returns>
        public static Double ParseMeasurement(String line)
        {
            return Parse("dB", line);
        }

        /// <summary>
        /// Code that parses the lines
        /// </summary>
        /// <param name="pattern"></param>
        /// <param name="line"></param>
        /// <returns></returns>
        private static Double Parse(String pattern, String line)
        {
            try
            {
                // assign an initial result to zero
                Double result = 0;

                // if the line contains a a line break somewhere
                if (line.Contains("\r\n"))
                {
                    // find the index
                    int index = line.IndexOf("\r\n");

                    // replace the new line with an empty string
                    line = line.Replace(Environment.NewLine, String.Empty);

                    // remove dB from that line
                    string removeDb = line.Substring(index, line.Length - index);

                    // try to parse that line if it is sucessfull return the result
                    if (Double.TryParse(removeDb, NumberStyles.Number, CultureInfo.InvariantCulture, out result))
                    {
                        // return the result
                        return result;
                    }
                    else
                    {
                        // just return zero as parse failed
                        return result;
                    }
                }

                // if the line contacts "dB" do the following
                if (line.Contains(pattern))
                {
                    // find the index
                    int index = line.IndexOf(pattern);

                    // remove dB from that line
                    string removeDb = line.Substring(0, index);

                    // try to parse that line if it is sucessfull return the result
                    if (Double.TryParse(removeDb, out result))
                    {
                        // if this is the first few seconds
                        if (result == -999)
                        {
                            return 0;
                        }

                        return result;
                    }
                    else
                    {
                        // just return zero as parse failed
                        return result;
                    }
                }
                // if it is just a line with no dB
                else
                {
                    // try to parse instantly
                    if (Double.TryParse(line, out result))
                    {
                        // if this is the first few seconds
                        if (result == -999)
                        {
                            return 0;
                        }

                        // return that result if it is sucessfull
                        return result;
                    }
                    else
                    {
                        // otherwise just return zero - unparsed result
                        return result;
                    }
                }
            }
            catch (Exception ex)
            {
                // throw an exception with the line number so we have the opportunaty to see what has broken
                // this particular method - this should go straight to the log files
                throw new Exception(String.Format("{0}\n Line: {1}", ex.Message, line), ex);
            }
        }
    }
}
