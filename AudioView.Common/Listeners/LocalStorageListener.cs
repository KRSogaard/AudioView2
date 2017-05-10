using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AudioView.Common.Data;
using AudioView.Common.Engine;
using DocumentFormat.OpenXml.Drawing.Diagrams;

namespace AudioView.Common.Listeners
{
    public class LocalStorageListener : IMeterListener
    {
        private object LogFileLock = new object();
        private const int BufferSize = 65536;  // 64 Kilobytes
        private string s = ",";
        private StreamWriter StreamWriter;

        public LocalStorageListener(String folder, string projectName)
        {
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            string fileName = DateTime.Now.ToString("yyyy-MM-dd-hh-mm-ss." + CoerceValidFileName(projectName) + ".csv");
            string fullPath = folder + "/" + fileName;
            StreamWriter = new StreamWriter(fullPath, true, Encoding.UTF8, BufferSize);
            StreamWriter.WriteLineAsync(getHead()).Wait();
            StreamWriter.Flush();
        }

        public static string CoerceValidFileName(string filename)
        {
            var invalidChars = Regex.Escape(new string(Path.GetInvalidFileNameChars()));
            var invalidReStr = string.Format(@"[{0}]+", invalidChars);
            var sanitisedNamePart = Regex.Replace(filename, invalidReStr, "_");
            return sanitisedNamePart;
        }

        public Task OnMinor(DateTime time, ReadingData data)
        {
            return Task.Run(() =>
            {
                lock (LogFileLock)
                {
                    StreamWriter.WriteLineAsync(ToCsv(data, time, false)).Wait();
                    StreamWriter.Flush();
                }
            });
        }

        public Task OnMajor(DateTime time, ReadingData data)
        {
            return Task.Run(() =>
            {
                lock (LogFileLock)
                {
                    StreamWriter.WriteLineAsync(ToCsv(data, time, true)).Wait();
                    StreamWriter.Flush();
                }
            });
        }

        public Task OnSecond(DateTime time, ReadingData data, ReadingData minorData, ReadingData majorData)
        {
            return Task.FromResult<object>(null);
        }

        public Task NextMinor(DateTime time)
        {
            return Task.FromResult<object>(null);
        }

        public Task NextMajor(DateTime time)
        {
            return Task.FromResult<object>(null);
        }

        private string ToCsv(ReadingData d, DateTime time, bool major)
        {
            StringBuilder b = new StringBuilder();
            b.Append(major ? "1" : "0"); b.Append(s);
            b.Append(time.ToString("o")); b.Append(s);

            b.Append(d.LAeq); b.Append(s);
            b.Append(d.LAMax); b.Append(s);
            b.Append(d.LAMin); b.Append(s);
            b.Append(d.LZMax); b.Append(s);

            // One one
            b.Append(d.LAeqOctaveBandOneOne.Hz16); b.Append(s);
            b.Append(d.LAeqOctaveBandOneOne.Hz31_5); b.Append(s);
            b.Append(d.LAeqOctaveBandOneOne.Hz63); b.Append(s);
            b.Append(d.LAeqOctaveBandOneOne.Hz125); b.Append(s);
            b.Append(d.LAeqOctaveBandOneOne.Hz250); b.Append(s);
            b.Append(d.LAeqOctaveBandOneOne.Hz500); b.Append(s);
            b.Append(d.LAeqOctaveBandOneOne.Hz1000); b.Append(s);
            b.Append(d.LAeqOctaveBandOneOne.Hz2000); b.Append(s);
            b.Append(d.LAeqOctaveBandOneOne.Hz4000); b.Append(s);
            b.Append(d.LAeqOctaveBandOneOne.Hz8000); b.Append(s);
            b.Append(d.LAeqOctaveBandOneOne.Hz16000); b.Append(s);

            // One Third
            b.Append(d.LAeqOctaveBandOneThird.Hz6_3); b.Append(s);
            b.Append(d.LAeqOctaveBandOneThird.Hz8); b.Append(s);
            b.Append(d.LAeqOctaveBandOneThird.Hz10); b.Append(s);
            b.Append(d.LAeqOctaveBandOneThird.Hz12_5); b.Append(s);
            b.Append(d.LAeqOctaveBandOneThird.Hz16); b.Append(s);
            b.Append(d.LAeqOctaveBandOneThird.Hz20); b.Append(s);
            b.Append(d.LAeqOctaveBandOneThird.Hz25); b.Append(s);
            b.Append(d.LAeqOctaveBandOneThird.Hz31_5); b.Append(s);
            b.Append(d.LAeqOctaveBandOneThird.Hz40); b.Append(s);
            b.Append(d.LAeqOctaveBandOneThird.Hz50); b.Append(s);
            b.Append(d.LAeqOctaveBandOneThird.Hz63); b.Append(s);
            b.Append(d.LAeqOctaveBandOneThird.Hz80); b.Append(s);
            b.Append(d.LAeqOctaveBandOneThird.Hz100); b.Append(s);
            b.Append(d.LAeqOctaveBandOneThird.Hz125); b.Append(s);
            b.Append(d.LAeqOctaveBandOneThird.Hz160); b.Append(s);
            b.Append(d.LAeqOctaveBandOneThird.Hz200); b.Append(s);
            b.Append(d.LAeqOctaveBandOneThird.Hz250); b.Append(s);
            b.Append(d.LAeqOctaveBandOneThird.Hz315); b.Append(s);
            b.Append(d.LAeqOctaveBandOneThird.Hz400); b.Append(s);
            b.Append(d.LAeqOctaveBandOneThird.Hz500); b.Append(s);
            b.Append(d.LAeqOctaveBandOneThird.Hz630); b.Append(s);
            b.Append(d.LAeqOctaveBandOneThird.Hz800); b.Append(s);
            b.Append(d.LAeqOctaveBandOneThird.Hz1000); b.Append(s);
            b.Append(d.LAeqOctaveBandOneThird.Hz1250); b.Append(s);
            b.Append(d.LAeqOctaveBandOneThird.Hz1600); b.Append(s);
            b.Append(d.LAeqOctaveBandOneThird.Hz2000); b.Append(s);
            b.Append(d.LAeqOctaveBandOneThird.Hz2500); b.Append(s);
            b.Append(d.LAeqOctaveBandOneThird.Hz3150); b.Append(s);
            b.Append(d.LAeqOctaveBandOneThird.Hz4000); b.Append(s);
            b.Append(d.LAeqOctaveBandOneThird.Hz5000); b.Append(s);
            b.Append(d.LAeqOctaveBandOneThird.Hz6300); b.Append(s);
            b.Append(d.LAeqOctaveBandOneThird.Hz8000); b.Append(s);
            b.Append(d.LAeqOctaveBandOneThird.Hz10000); b.Append(s);
            b.Append(d.LAeqOctaveBandOneThird.Hz12500); b.Append(s);
            b.Append(d.LAeqOctaveBandOneThird.Hz16000); b.Append(s);
            b.Append(d.LAeqOctaveBandOneThird.Hz20000); b.Append(s);
            return b.ToString();
        }

        private string getHead()
        {
            StringBuilder b = new StringBuilder();
            b.Append("Reading"); b.Append(s);
            b.Append("Time"); b.Append(s);
            b.Append("LAeq"); b.Append(s);
            b.Append("LAMax"); b.Append(s);
            b.Append("LAMin"); b.Append(s);
            b.Append("LZMax"); b.Append(s);

            // One one
            b.Append("1/1 Hz16"); b.Append(s);
            b.Append("1/1 Hz31_5"); b.Append(s);
            b.Append("1/1 Hz63"); b.Append(s);
            b.Append("1/1 Hz125"); b.Append(s);
            b.Append("1/1 Hz250"); b.Append(s);
            b.Append("1/1 Hz500"); b.Append(s);
            b.Append("1/1 Hz1000"); b.Append(s);
            b.Append("1/1 Hz2000"); b.Append(s);
            b.Append("1/1 Hz4000"); b.Append(s);
            b.Append("1/1 Hz8000"); b.Append(s);
            b.Append("1/1 Hz16000"); b.Append(s);

            // One Third
            b.Append("1/3 Hz6_3"); b.Append(s);
            b.Append("1/3 Hz8"); b.Append(s);
            b.Append("1/3 Hz10"); b.Append(s);
            b.Append("1/3 Hz12_5"); b.Append(s);
            b.Append("1/3 Hz16"); b.Append(s);
            b.Append("1/3 Hz20"); b.Append(s);
            b.Append("1/3 Hz25"); b.Append(s);
            b.Append("1/3 Hz31_5"); b.Append(s);
            b.Append("1/3 Hz40"); b.Append(s);
            b.Append("1/3 Hz50"); b.Append(s);
            b.Append("1/3 Hz63"); b.Append(s);
            b.Append("1/3 Hz80"); b.Append(s);
            b.Append("1/3 Hz100"); b.Append(s);
            b.Append("1/3 Hz125"); b.Append(s);
            b.Append("1/3 Hz160"); b.Append(s);
            b.Append("1/3 Hz200"); b.Append(s);
            b.Append("1/3 Hz250"); b.Append(s);
            b.Append("1/3 Hz315"); b.Append(s);
            b.Append("1/3 Hz400"); b.Append(s);
            b.Append("1/3 Hz500"); b.Append(s);
            b.Append("1/3 Hz630"); b.Append(s);
            b.Append("1/3 Hz800"); b.Append(s);
            b.Append("1/3 Hz1000"); b.Append(s);
            b.Append("1/3 Hz1250"); b.Append(s);
            b.Append("1/3 Hz1600"); b.Append(s);
            b.Append("1/3 Hz2000"); b.Append(s);
            b.Append("1/3 Hz2500"); b.Append(s);
            b.Append("1/3 Hz3150"); b.Append(s);
            b.Append("1/3 Hz4000"); b.Append(s);
            b.Append("1/3 Hz5000"); b.Append(s);
            b.Append("1/3 Hz6300"); b.Append(s);
            b.Append("1/3 Hz8000"); b.Append(s);
            b.Append("1/3 Hz10000"); b.Append(s);
            b.Append("1/3 Hz12500"); b.Append(s);
            b.Append("1/3 Hz16000"); b.Append(s);
            b.Append("1/3 Hz20000"); b.Append(s);
            return b.ToString();
        }
    }
}
