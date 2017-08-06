using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AudioView.Common.Data;

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


        public class OctaveBandProperty
        {
            public string Display;
            public string Method;
            public double LimitAjust;

            public OctaveBandProperty(string display, string method, double limitAjust)
            {
                Display = display;
                Method = method;
                LimitAjust = limitAjust;
            }
        }

        public static double GetLimitOffSet(string value)
        {
            if (value == null)
                return 0;
            value = value.ToLower();
            if (value.Contains("hz"))
            {
                value = value.Split('-').Last();
            }

            switch (value)
            {
                case "hz6_3":
                    return 85;
                case "hz8":
                    return 78;
                case "hz10":
                    return 70;
                case "hz12_5":
                    return 63;
                case "hz16":
                    return 57;
                case "hz20":
                    return 51;
                case "hz25":
                    return 45;
                case "hz31_5":
                    return 39;
                case "hz40":
                    return 35;
                case "hz50":
                    return 30;
                case "hz63":
                    return 26;
                case "hz80":
                    return 23;
                case "hz100":
                    return 19;
                case "hz125":
                    return 16;
                case "hz160":
                    return 13;
                case "hz200":
                    return 11;
                case "hz250":
                    return 8.6;
                case "hz315":
                    return 6.6;
                case "hz400":
                    return 4.8;
                case "hz500":
                    return 3.2;
                case "hz630":
                    return 1.9;
                case "hz800":
                    return 0.8;
                case "hz1000":
                    return 0;
                case "hz1250":
                    return -0.6;
                case "hz1600":
                    return -1;
                case "hz2000":
                    return -1.2;
                case "hz2500":
                    return -1.3;
                case "hz3150":
                    return -1.2;
                case "hz4000":
                    return -1;
                case "hz5000":
                    return -0.5;
                case "hz6300":
                    return 0.1;
                case "hz8000":
                    return 1.1;
                case "hz10000":
                    return 2.5;
                case "hz12500":
                    return 4.3;
                case "hz16000":
                    return 6.6;
                case "hz20000":
                    return 9.3;
                default:
                    return 0;
            }
        }

        private static object _getOneThirdOctaveBandLock = new object();
        private static List<OctaveBandProperty> _getOneThirdOctaveBand;
        public static List<OctaveBandProperty> GetOneThirdOctaveBand()
        {
            lock (_getOneThirdOctaveBandLock)
            {
                if (_getOneThirdOctaveBand == null)
                {
                    _getOneThirdOctaveBand = new List<OctaveBandProperty>();
                    foreach (var propertyInfo in typeof(ReadingData.OctaveBandOneThird).GetProperties())
                    {
                        _getOneThirdOctaveBand.Add(new OctaveBandProperty(
                            HzToName(propertyInfo.Name),
                            propertyInfo.Name,
                            GetLimitOffSet(propertyInfo.Name)));
                    }
                }
            }
            return _getOneThirdOctaveBand;
        }


        private static object _getOneOneOctaveBandLock = new object();
        private static List<OctaveBandProperty> _getOneOneOctaveBand;
        public static List<OctaveBandProperty> GetOneOneOctaveBand()
        {
            lock (_getOneOneOctaveBandLock)
            {
                if (_getOneOneOctaveBand == null)
                {
                    _getOneOneOctaveBand = new List<OctaveBandProperty>();
                    foreach (var propertyInfo in typeof(ReadingData.OctaveBandOneOne).GetProperties())
                    {
                        _getOneOneOctaveBand.Add(new OctaveBandProperty(
                            HzToName(propertyInfo.Name),
                            propertyInfo.Name,
                            GetLimitOffSet(propertyInfo.Name)));
                    }
                }
            }
            return _getOneOneOctaveBand;
        }

        public static string HzToName(string methodName)
        {
            if (methodName == null || !methodName.ToLower().Contains("hz"))
            {
                return methodName;
            }
            string noHz = methodName.Substring(2, methodName.Length - 2);
            return noHz.Replace("_", ".") + "Hz";
        }
    }
}
