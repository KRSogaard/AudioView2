using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioView.Common
{
    public static class DecibelHelper
    {
        public static double GetPowerFromDecibel(double dB)
        {
            return Math.Pow(10, dB / 10);
        }

        public static double GetDecibelFromPower(double power)
        {
            return 10*Math.Log10(power);
        }

        public static double CalculateOneOneOctaveBand(double one, double two, double three)
        {
            return GetDecibelFromPower(
                GetPowerFromDecibel(one) +
                GetPowerFromDecibel(two) +
                GetPowerFromDecibel(three)
                );
        }
    }
}
