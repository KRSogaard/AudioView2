using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioView.Common
{
    public class ReadingData
    {
        public double LAeq { get; set; }

        public string SerializeToOneLine(string splitter)
        {
            return LAeq.ToString();
        }
    }
}
