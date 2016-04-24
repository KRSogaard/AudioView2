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
                                 "6_3Hz{0}8Hz{0}10Hz{0}12_5Hz{0}16Hz{0}20Hz{0}25Hz{0}31_5Hz{0}" +
                                 "40Hz{0}50Hz{0}63Hz{0}80Hz{0}100Hz{0}125Hz{0}160Hz{0}200Hz{0}" +
                                 "250Hz{0}315Hz{0}400Hz{0}500Hz{0}630Hz{0}800Hz{0}1000Hz{0}" +
                                 "1250Hz{0}1600Hz{0}2000Hz{0}2500Hz{0}3150Hz{0}4000Hz{0}5000Hz{0}" +
                                 "6300Hz{0}8000Hz{0}10000Hz{0}12500Hz{0}16000Hz{0}20000Hz", seperator);
        }

        public string CsvLine(string seperator = ",")
        {
            return string.Format(Time + "{0}" + Major + "{0}" + 
                
                // 1/3 Octaves
                formatToPrecision(Data.LAeq) + "{0}" + formatToPrecision(Data.LAMax) + "{0}" + formatToPrecision(Data.LAMin) + "{0}" + formatToPrecision(Data.LZMax) + "{0}" +
                formatToPrecision(Data.LZMin) + "{0}" + formatToPrecision(Data.LAeqOctaveBand.Hz6_3) + "{0}" + formatToPrecision(Data.LAeqOctaveBand.Hz8) + "{0}" +
                formatToPrecision(Data.LAeqOctaveBand.Hz10) + "{0}" + formatToPrecision(Data.LAeqOctaveBand.Hz12_5) + "{0}" + formatToPrecision(Data.LAeqOctaveBand.Hz16) + "{0}" +
                formatToPrecision(Data.LAeqOctaveBand.Hz20) + "{0}" + formatToPrecision(Data.LAeqOctaveBand.Hz25) + "{0}" + formatToPrecision(Data.LAeqOctaveBand.Hz31_5) + "{0}" +
                formatToPrecision(Data.LAeqOctaveBand.Hz40) + "{0}" + formatToPrecision(Data.LAeqOctaveBand.Hz50) + "{0}" + formatToPrecision(Data.LAeqOctaveBand.Hz63) + "{0}" +
                formatToPrecision(Data.LAeqOctaveBand.Hz80) + "{0}" + formatToPrecision(Data.LAeqOctaveBand.Hz100) + "{0}" + formatToPrecision(Data.LAeqOctaveBand.Hz125) + "{0}" +
                formatToPrecision(Data.LAeqOctaveBand.Hz160) + "{0}" + formatToPrecision(Data.LAeqOctaveBand.Hz200) + "{0}" + formatToPrecision(Data.LAeqOctaveBand.Hz250) + "{0}" +
                formatToPrecision(Data.LAeqOctaveBand.Hz315) + "{0}" + formatToPrecision(Data.LAeqOctaveBand.Hz400) + "{0}" + formatToPrecision(Data.LAeqOctaveBand.Hz500) + "{0}" +
                formatToPrecision(Data.LAeqOctaveBand.Hz630) + "{0}" + formatToPrecision(Data.LAeqOctaveBand.Hz800) + "{0}" + formatToPrecision(Data.LAeqOctaveBand.Hz1000) + "{0}" +
                formatToPrecision(Data.LAeqOctaveBand.Hz1250) + "{0}" + formatToPrecision(Data.LAeqOctaveBand.Hz1600) + "{0}" + formatToPrecision(Data.LAeqOctaveBand.Hz2000) + "{0}" +
                formatToPrecision(Data.LAeqOctaveBand.Hz2500) + "{0}" + formatToPrecision(Data.LAeqOctaveBand.Hz3150) + "{0}" + formatToPrecision(Data.LAeqOctaveBand.Hz4000) + "{0}" +
                formatToPrecision(Data.LAeqOctaveBand.Hz5000) + "{0}" + formatToPrecision(Data.LAeqOctaveBand.Hz6300) + "{0}" + formatToPrecision(Data.LAeqOctaveBand.Hz8000) + "{0}" +
                formatToPrecision(Data.LAeqOctaveBand.Hz10000) + "{0}" + formatToPrecision(Data.LAeqOctaveBand.Hz12500) + "{0}" + formatToPrecision(Data.LAeqOctaveBand.Hz16000) + "{0}" +
                formatToPrecision(Data.LAeqOctaveBand.Hz20000),
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