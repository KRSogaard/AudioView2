using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AudioView.Common.Data
{
    public class Reading
    {
        public System.Guid Id { get; set; }
        public System.Guid Project { get; set; }
        public System.DateTime Time { get; set; }
        public bool Major { get; set; }
        public double LAeq { get; set; }

        private static string CsvHeader(string seperator = ",")
        {
            return string.Format("Time{0}Major{0}LAeq", seperator);
        }

        public string CsvLine(string seperator = ",")
        {
            return string.Format("{1}{0}{2}{0}{3}", seperator, Time, Major, LAeq);
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
    }
}