using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace AudioView.Common.Data
{
    public class Reading
    {
        public System.Guid Id { get; set; }
        public System.Guid Project { get; set; }
        public System.DateTime Time { get; set; }
        public ReadingData Data { get; set; }
        public bool Major { get; set; }

        private static string CsvHeader(string seperator = ",")
        {
            return string.Format("Time{0}Major{0}LAeq{0}LAMax{0}LAMin{0}LZMax{0}LZMin{0}" +
                                 // 1/3 Octaves 
                                 "1/3 6_3Hz{0}1/3 8Hz{0}1/3 10Hz{0}1/3 12_5Hz{0}1/3 16Hz{0}1/3 20Hz{0}1/3 25Hz{0}1/3 31_5Hz{0}" +
                                 "1/3 40Hz{0}1/3 50Hz{0}1/3 63Hz{0}1/3 80Hz{0}1/3 100Hz{0}1/3 125Hz{0}1/3 160Hz{0}1/3 200Hz{0}" +
                                 "1/3 250Hz{0}1/3 315Hz{0}1/3 400Hz{0}1/3 500Hz{0}1/3 630Hz{0}1/3 800Hz{0}1/3 1000Hz{0}" +
                                 "1/3 1250Hz{0}1/3 1600Hz{0}1/3 2000Hz{0}1/3 2500Hz{0}1/3 3150Hz{0}1/3 4000Hz{0}1/3 5000Hz{0}" +
                                 "1/3 6300Hz{0}1/3 8000Hz{0}1/3 10000Hz{0}1/3 12500Hz{0}1/3 16000Hz{0}1/3 20000Hz" +
                                 // 1/1 Octaves
                                 "1/1 16Hz{0}1/1 31_5Hz{0}1/1 63Hz{0}1/1 125Hz{0}1/1 250Hz{0}1/1 500Hz{0}1/1 1000Hz{0}" +
                                 "1/1 2000Hz{0}1/1 4000Hz{0}1/1 8000Hz{0}1/1 16000Hz{0}",
                                 seperator);
        }

        public string CsvLine(string seperator = ",")
        {
            return string.Format(Time + "{0}" + Major + "{0}" + 
                
                formatToPrecision(Data.LAeq) + "{0}" + formatToPrecision(Data.LAMax) + "{0}" + formatToPrecision(Data.LAMin) + "{0}" + formatToPrecision(Data.LZMax) + "{0}" +
                formatToPrecision(Data.LZMin) + "{0}" +

                // 1/3 Octaves
                formatToPrecision(Data.LAeqOctaveBandOneThird.Hz6_3) + "{0}" + formatToPrecision(Data.LAeqOctaveBandOneThird.Hz8) + "{0}" +
                formatToPrecision(Data.LAeqOctaveBandOneThird.Hz10) + "{0}" + formatToPrecision(Data.LAeqOctaveBandOneThird.Hz12_5) + "{0}" + formatToPrecision(Data.LAeqOctaveBandOneThird.Hz16) + "{0}" +
                formatToPrecision(Data.LAeqOctaveBandOneThird.Hz20) + "{0}" + formatToPrecision(Data.LAeqOctaveBandOneThird.Hz25) + "{0}" + formatToPrecision(Data.LAeqOctaveBandOneThird.Hz31_5) + "{0}" +
                formatToPrecision(Data.LAeqOctaveBandOneThird.Hz40) + "{0}" + formatToPrecision(Data.LAeqOctaveBandOneThird.Hz50) + "{0}" + formatToPrecision(Data.LAeqOctaveBandOneThird.Hz63) + "{0}" +
                formatToPrecision(Data.LAeqOctaveBandOneThird.Hz80) + "{0}" + formatToPrecision(Data.LAeqOctaveBandOneThird.Hz100) + "{0}" + formatToPrecision(Data.LAeqOctaveBandOneThird.Hz125) + "{0}" +
                formatToPrecision(Data.LAeqOctaveBandOneThird.Hz160) + "{0}" + formatToPrecision(Data.LAeqOctaveBandOneThird.Hz200) + "{0}" + formatToPrecision(Data.LAeqOctaveBandOneThird.Hz250) + "{0}" +
                formatToPrecision(Data.LAeqOctaveBandOneThird.Hz315) + "{0}" + formatToPrecision(Data.LAeqOctaveBandOneThird.Hz400) + "{0}" + formatToPrecision(Data.LAeqOctaveBandOneThird.Hz500) + "{0}" +
                formatToPrecision(Data.LAeqOctaveBandOneThird.Hz630) + "{0}" + formatToPrecision(Data.LAeqOctaveBandOneThird.Hz800) + "{0}" + formatToPrecision(Data.LAeqOctaveBandOneThird.Hz1000) + "{0}" +
                formatToPrecision(Data.LAeqOctaveBandOneThird.Hz1250) + "{0}" + formatToPrecision(Data.LAeqOctaveBandOneThird.Hz1600) + "{0}" + formatToPrecision(Data.LAeqOctaveBandOneThird.Hz2000) + "{0}" +
                formatToPrecision(Data.LAeqOctaveBandOneThird.Hz2500) + "{0}" + formatToPrecision(Data.LAeqOctaveBandOneThird.Hz3150) + "{0}" + formatToPrecision(Data.LAeqOctaveBandOneThird.Hz4000) + "{0}" +
                formatToPrecision(Data.LAeqOctaveBandOneThird.Hz5000) + "{0}" + formatToPrecision(Data.LAeqOctaveBandOneThird.Hz6300) + "{0}" + formatToPrecision(Data.LAeqOctaveBandOneThird.Hz8000) + "{0}" +
                formatToPrecision(Data.LAeqOctaveBandOneThird.Hz10000) + "{0}" + formatToPrecision(Data.LAeqOctaveBandOneThird.Hz12500) + "{0}" + formatToPrecision(Data.LAeqOctaveBandOneThird.Hz16000) + "{0}" +
                formatToPrecision(Data.LAeqOctaveBandOneThird.Hz20000) + "{0}" +

                // 1/1 Octaves
                formatToPrecision(Data.LAeqOctaveBandOneOne.Hz16) + "{0}" + formatToPrecision(Data.LAeqOctaveBandOneOne.Hz31_5) + "{0}" + formatToPrecision(Data.LAeqOctaveBandOneOne.Hz63) + "{0}" +
                formatToPrecision(Data.LAeqOctaveBandOneOne.Hz125) + "{0}" + formatToPrecision(Data.LAeqOctaveBandOneOne.Hz250) + "{0}" + formatToPrecision(Data.LAeqOctaveBandOneOne.Hz500) + "{0}" + 
                formatToPrecision(Data.LAeqOctaveBandOneOne.Hz1000) + "{0}" + formatToPrecision(Data.LAeqOctaveBandOneOne.Hz2000) + "{0}" + formatToPrecision(Data.LAeqOctaveBandOneOne.Hz4000) + "{0}" + 
                formatToPrecision(Data.LAeqOctaveBandOneOne.Hz8000) + "{0}" + formatToPrecision(Data.LAeqOctaveBandOneOne.Hz16000),
                seperator);
        }

        public static string CSV(IList<Reading> readings, string seperator = ",")
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine(CsvHeader(seperator));
            foreach (var reading in readings)
            {
                builder.AppendLine(reading.CsvLine(seperator));
            }
            return builder.ToString();
        }

        private string formatToPrecision(double value)
        {
            return Math.Round(value, 1).ToString();
        }
    }
}