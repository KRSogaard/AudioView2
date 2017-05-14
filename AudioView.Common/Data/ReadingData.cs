using System;
using System.Collections.Generic;
using System.Linq;

namespace AudioView.Common.Data
{
    public class ReadingData
    {
        public double LAeq { get; set; }
        public double LCeq { get; set; }
        public double LAMax { get; set; }
        public double LAMin { get; set; }
        public double LZMax { get; set; }
        public double LZMin { get; set; }
        private OctaveBandOneThird lAeqOctaveBandOneThird;
        public OctaveBandOneThird LAeqOctaveBandOneThird
        {
            get {
                return lAeqOctaveBandOneThird;
            }
            set
            {
                lAeqOctaveBandOneThird = value;
                // Update 1:1 band
                calculateOneOneBand();
            }
        }

        public OctaveBandOneOne LAeqOctaveBandOneOne { get; set; }

        public ReadingData()
        {
            LAeqOctaveBandOneThird = new OctaveBandOneThird();
        }

        public string SerializeToOneLine(string splitter)
        {
            return LAeq.ToString();
        }

        private void calculateOneOneBand()
        {
            LAeqOctaveBandOneOne = new OctaveBandOneOne()
            {
                Hz16 = DecibelHelper.CalculateOneOneOctaveBand(lAeqOctaveBandOneThird.Hz12_5,
                                                     lAeqOctaveBandOneThird.Hz16,
                                                     lAeqOctaveBandOneThird.Hz20),
                Hz31_5 = DecibelHelper.CalculateOneOneOctaveBand(lAeqOctaveBandOneThird.Hz25,
                                                     lAeqOctaveBandOneThird.Hz31_5,
                                                     lAeqOctaveBandOneThird.Hz40),
                Hz63 = DecibelHelper.CalculateOneOneOctaveBand(lAeqOctaveBandOneThird.Hz50,
                                                     lAeqOctaveBandOneThird.Hz63,
                                                     lAeqOctaveBandOneThird.Hz80),
                Hz125 = DecibelHelper.CalculateOneOneOctaveBand(lAeqOctaveBandOneThird.Hz100,
                                                     lAeqOctaveBandOneThird.Hz125,
                                                     lAeqOctaveBandOneThird.Hz160),
                Hz250 = DecibelHelper.CalculateOneOneOctaveBand(lAeqOctaveBandOneThird.Hz200,
                                                     lAeqOctaveBandOneThird.Hz250,
                                                     lAeqOctaveBandOneThird.Hz315),
                Hz500 = DecibelHelper.CalculateOneOneOctaveBand(lAeqOctaveBandOneThird.Hz400,
                                                     lAeqOctaveBandOneThird.Hz500,
                                                     lAeqOctaveBandOneThird.Hz630),
                Hz1000 = DecibelHelper.CalculateOneOneOctaveBand(lAeqOctaveBandOneThird.Hz800,
                                                     lAeqOctaveBandOneThird.Hz1000,
                                                     lAeqOctaveBandOneThird.Hz1250),
                Hz2000 = DecibelHelper.CalculateOneOneOctaveBand(lAeqOctaveBandOneThird.Hz1600,
                                                     lAeqOctaveBandOneThird.Hz2000,
                                                     lAeqOctaveBandOneThird.Hz2500),
                Hz4000 = DecibelHelper.CalculateOneOneOctaveBand(lAeqOctaveBandOneThird.Hz3150,
                                                     lAeqOctaveBandOneThird.Hz4000,
                                                     lAeqOctaveBandOneThird.Hz5000),
                Hz8000 = DecibelHelper.CalculateOneOneOctaveBand(lAeqOctaveBandOneThird.Hz6300,
                                                     lAeqOctaveBandOneThird.Hz8000,
                                                     lAeqOctaveBandOneThird.Hz10000),
                Hz16000 = DecibelHelper.CalculateOneOneOctaveBand(lAeqOctaveBandOneThird.Hz12500,
                                                     lAeqOctaveBandOneThird.Hz16000,
                                                     lAeqOctaveBandOneThird.Hz20000)
            };
        }

        public static ReadingData Average(ICollection<ReadingData> readings)
        {
            var result = new ReadingData()
            {
                lAeqOctaveBandOneThird = new OctaveBandOneThird(),
                LAeqOctaveBandOneOne = new OctaveBandOneOne()
            };
            foreach (var readingData in readings)
            {
                result.LAeq += DecibelHelper.GetPowerFromDecibel(readingData.LAeq);
                result.LCeq += DecibelHelper.GetPowerFromDecibel(readingData.LCeq);
                result.LAMax += DecibelHelper.GetPowerFromDecibel(readingData.LAMax);
                result.LAMin += DecibelHelper.GetPowerFromDecibel(readingData.LAMin);
                result.LZMax += DecibelHelper.GetPowerFromDecibel(readingData.LZMax);
                result.LZMin += DecibelHelper.GetPowerFromDecibel(readingData.LZMin);

                // OneOne
                result.LAeqOctaveBandOneOne.Hz16 += DecibelHelper.GetPowerFromDecibel(readingData.LAeqOctaveBandOneOne.Hz16);
                result.LAeqOctaveBandOneOne.Hz31_5 += DecibelHelper.GetPowerFromDecibel(readingData.LAeqOctaveBandOneOne.Hz31_5);
                result.LAeqOctaveBandOneOne.Hz63 += DecibelHelper.GetPowerFromDecibel(readingData.LAeqOctaveBandOneOne.Hz63);
                result.LAeqOctaveBandOneOne.Hz125 += DecibelHelper.GetPowerFromDecibel(readingData.LAeqOctaveBandOneOne.Hz125);
                result.LAeqOctaveBandOneOne.Hz250 += DecibelHelper.GetPowerFromDecibel(readingData.LAeqOctaveBandOneOne.Hz250);
                result.LAeqOctaveBandOneOne.Hz500 += DecibelHelper.GetPowerFromDecibel(readingData.LAeqOctaveBandOneOne.Hz500);
                result.LAeqOctaveBandOneOne.Hz1000 += DecibelHelper.GetPowerFromDecibel(readingData.LAeqOctaveBandOneOne.Hz1000);
                result.LAeqOctaveBandOneOne.Hz2000 += DecibelHelper.GetPowerFromDecibel(readingData.LAeqOctaveBandOneOne.Hz2000);
                result.LAeqOctaveBandOneOne.Hz4000 += DecibelHelper.GetPowerFromDecibel(readingData.LAeqOctaveBandOneOne.Hz4000);
                result.LAeqOctaveBandOneOne.Hz8000 += DecibelHelper.GetPowerFromDecibel(readingData.LAeqOctaveBandOneOne.Hz8000);
                result.LAeqOctaveBandOneOne.Hz16000 += DecibelHelper.GetPowerFromDecibel(readingData.LAeqOctaveBandOneOne.Hz16000);

                // OneThird
                result.LAeqOctaveBandOneThird.Hz6_3 += DecibelHelper.GetPowerFromDecibel(readingData.LAeqOctaveBandOneThird.Hz6_3);
                result.LAeqOctaveBandOneThird.Hz8 += DecibelHelper.GetPowerFromDecibel(readingData.LAeqOctaveBandOneThird.Hz8);
                result.LAeqOctaveBandOneThird.Hz10 += DecibelHelper.GetPowerFromDecibel(readingData.LAeqOctaveBandOneThird.Hz10);
                result.LAeqOctaveBandOneThird.Hz12_5 += DecibelHelper.GetPowerFromDecibel(readingData.LAeqOctaveBandOneThird.Hz12_5);
                result.LAeqOctaveBandOneThird.Hz16 += DecibelHelper.GetPowerFromDecibel(readingData.LAeqOctaveBandOneThird.Hz16);
                result.LAeqOctaveBandOneThird.Hz20 += DecibelHelper.GetPowerFromDecibel(readingData.LAeqOctaveBandOneThird.Hz20);
                result.LAeqOctaveBandOneThird.Hz25 += DecibelHelper.GetPowerFromDecibel(readingData.LAeqOctaveBandOneThird.Hz25);
                result.LAeqOctaveBandOneThird.Hz31_5 += DecibelHelper.GetPowerFromDecibel(readingData.LAeqOctaveBandOneThird.Hz31_5);
                result.LAeqOctaveBandOneThird.Hz40 += DecibelHelper.GetPowerFromDecibel(readingData.LAeqOctaveBandOneThird.Hz40);
                result.LAeqOctaveBandOneThird.Hz50 += DecibelHelper.GetPowerFromDecibel(readingData.LAeqOctaveBandOneThird.Hz50);
                result.LAeqOctaveBandOneThird.Hz63 += DecibelHelper.GetPowerFromDecibel(readingData.LAeqOctaveBandOneThird.Hz63);
                result.LAeqOctaveBandOneThird.Hz80 += DecibelHelper.GetPowerFromDecibel(readingData.LAeqOctaveBandOneThird.Hz80);
                result.LAeqOctaveBandOneThird.Hz100 += DecibelHelper.GetPowerFromDecibel(readingData.LAeqOctaveBandOneThird.Hz100);
                result.LAeqOctaveBandOneThird.Hz125 += DecibelHelper.GetPowerFromDecibel(readingData.LAeqOctaveBandOneThird.Hz125);
                result.LAeqOctaveBandOneThird.Hz160 += DecibelHelper.GetPowerFromDecibel(readingData.LAeqOctaveBandOneThird.Hz160);
                result.LAeqOctaveBandOneThird.Hz200 += DecibelHelper.GetPowerFromDecibel(readingData.LAeqOctaveBandOneThird.Hz200);
                result.LAeqOctaveBandOneThird.Hz250 += DecibelHelper.GetPowerFromDecibel(readingData.LAeqOctaveBandOneThird.Hz250);
                result.LAeqOctaveBandOneThird.Hz315 += DecibelHelper.GetPowerFromDecibel(readingData.LAeqOctaveBandOneThird.Hz315);
                result.LAeqOctaveBandOneThird.Hz400 += DecibelHelper.GetPowerFromDecibel(readingData.LAeqOctaveBandOneThird.Hz400);
                result.LAeqOctaveBandOneThird.Hz500 += DecibelHelper.GetPowerFromDecibel(readingData.LAeqOctaveBandOneThird.Hz500);
                result.LAeqOctaveBandOneThird.Hz630 += DecibelHelper.GetPowerFromDecibel(readingData.LAeqOctaveBandOneThird.Hz630);
                result.LAeqOctaveBandOneThird.Hz800 += DecibelHelper.GetPowerFromDecibel(readingData.LAeqOctaveBandOneThird.Hz800);
                result.LAeqOctaveBandOneThird.Hz1000 += DecibelHelper.GetPowerFromDecibel(readingData.LAeqOctaveBandOneThird.Hz1000);
                result.LAeqOctaveBandOneThird.Hz1250 += DecibelHelper.GetPowerFromDecibel(readingData.LAeqOctaveBandOneThird.Hz1250);
                result.LAeqOctaveBandOneThird.Hz1600 += DecibelHelper.GetPowerFromDecibel(readingData.LAeqOctaveBandOneThird.Hz1600);
                result.LAeqOctaveBandOneThird.Hz2000 += DecibelHelper.GetPowerFromDecibel(readingData.LAeqOctaveBandOneThird.Hz2000);
                result.LAeqOctaveBandOneThird.Hz2500 += DecibelHelper.GetPowerFromDecibel(readingData.LAeqOctaveBandOneThird.Hz2500);
                result.LAeqOctaveBandOneThird.Hz3150 += DecibelHelper.GetPowerFromDecibel(readingData.LAeqOctaveBandOneThird.Hz3150);
                result.LAeqOctaveBandOneThird.Hz4000 += DecibelHelper.GetPowerFromDecibel(readingData.LAeqOctaveBandOneThird.Hz4000);
                result.LAeqOctaveBandOneThird.Hz5000 += DecibelHelper.GetPowerFromDecibel(readingData.LAeqOctaveBandOneThird.Hz5000);
                result.LAeqOctaveBandOneThird.Hz6300 += DecibelHelper.GetPowerFromDecibel(readingData.LAeqOctaveBandOneThird.Hz6300);
                result.LAeqOctaveBandOneThird.Hz8000 += DecibelHelper.GetPowerFromDecibel(readingData.LAeqOctaveBandOneThird.Hz8000);
                result.LAeqOctaveBandOneThird.Hz10000 += DecibelHelper.GetPowerFromDecibel(readingData.LAeqOctaveBandOneThird.Hz10000);
                result.LAeqOctaveBandOneThird.Hz12500 += DecibelHelper.GetPowerFromDecibel(readingData.LAeqOctaveBandOneThird.Hz12500);
                result.LAeqOctaveBandOneThird.Hz16000 += DecibelHelper.GetPowerFromDecibel(readingData.LAeqOctaveBandOneThird.Hz16000);
                result.LAeqOctaveBandOneThird.Hz20000 += DecibelHelper.GetPowerFromDecibel(readingData.LAeqOctaveBandOneThird.Hz20000);
            }

            // Avarage the power and convert that to decibel
            result.LAeq = DecibelHelper.GetDecibelFromPower(result.LAeq / readings.Count);
            result.LCeq = DecibelHelper.GetDecibelFromPower(result.LCeq / readings.Count);
            result.LAMax = DecibelHelper.GetDecibelFromPower(result.LAMax / readings.Count);
            result.LAMin = DecibelHelper.GetDecibelFromPower(result.LAMin / readings.Count);
            result.LZMax = DecibelHelper.GetDecibelFromPower(result.LZMax / readings.Count);
            result.LZMin = DecibelHelper.GetDecibelFromPower(result.LZMin / readings.Count);

            // OneOne
            result.LAeqOctaveBandOneOne.Hz16 = DecibelHelper.GetDecibelFromPower(result.LAeqOctaveBandOneOne.Hz16 / readings.Count);
            result.LAeqOctaveBandOneOne.Hz31_5 = DecibelHelper.GetDecibelFromPower(result.LAeqOctaveBandOneOne.Hz31_5 / readings.Count);
            result.LAeqOctaveBandOneOne.Hz63 = DecibelHelper.GetDecibelFromPower(result.LAeqOctaveBandOneOne.Hz63 / readings.Count);
            result.LAeqOctaveBandOneOne.Hz125 = DecibelHelper.GetDecibelFromPower(result.LAeqOctaveBandOneOne.Hz125 / readings.Count);
            result.LAeqOctaveBandOneOne.Hz250 = DecibelHelper.GetDecibelFromPower(result.LAeqOctaveBandOneOne.Hz250 / readings.Count);
            result.LAeqOctaveBandOneOne.Hz500 = DecibelHelper.GetDecibelFromPower(result.LAeqOctaveBandOneOne.Hz500 / readings.Count);
            result.LAeqOctaveBandOneOne.Hz1000 = DecibelHelper.GetDecibelFromPower(result.LAeqOctaveBandOneOne.Hz1000 / readings.Count);
            result.LAeqOctaveBandOneOne.Hz2000 = DecibelHelper.GetDecibelFromPower(result.LAeqOctaveBandOneOne.Hz2000 / readings.Count);
            result.LAeqOctaveBandOneOne.Hz4000 = DecibelHelper.GetDecibelFromPower(result.LAeqOctaveBandOneOne.Hz4000 / readings.Count);
            result.LAeqOctaveBandOneOne.Hz8000 = DecibelHelper.GetDecibelFromPower(result.LAeqOctaveBandOneOne.Hz8000 / readings.Count);
            result.LAeqOctaveBandOneOne.Hz16000 = DecibelHelper.GetDecibelFromPower(result.LAeqOctaveBandOneOne.Hz16000 / readings.Count);

            // OneThird
            result.LAeqOctaveBandOneThird.Hz6_3 = DecibelHelper.GetDecibelFromPower(result.LAeqOctaveBandOneThird.Hz6_3 / readings.Count);
            result.LAeqOctaveBandOneThird.Hz8 = DecibelHelper.GetDecibelFromPower(result.LAeqOctaveBandOneThird.Hz8 / readings.Count);
            result.LAeqOctaveBandOneThird.Hz10 = DecibelHelper.GetDecibelFromPower(result.LAeqOctaveBandOneThird.Hz10 / readings.Count);
            result.LAeqOctaveBandOneThird.Hz12_5 = DecibelHelper.GetDecibelFromPower(result.LAeqOctaveBandOneThird.Hz12_5 / readings.Count);
            result.LAeqOctaveBandOneThird.Hz16 = DecibelHelper.GetDecibelFromPower(result.LAeqOctaveBandOneThird.Hz16 / readings.Count);
            result.LAeqOctaveBandOneThird.Hz20 = DecibelHelper.GetDecibelFromPower(result.LAeqOctaveBandOneThird.Hz20 / readings.Count);
            result.LAeqOctaveBandOneThird.Hz25 = DecibelHelper.GetDecibelFromPower(result.LAeqOctaveBandOneThird.Hz25 / readings.Count);
            result.LAeqOctaveBandOneThird.Hz31_5 = DecibelHelper.GetDecibelFromPower(result.LAeqOctaveBandOneThird.Hz31_5 / readings.Count);
            result.LAeqOctaveBandOneThird.Hz40 = DecibelHelper.GetDecibelFromPower(result.LAeqOctaveBandOneThird.Hz40 / readings.Count);
            result.LAeqOctaveBandOneThird.Hz50 = DecibelHelper.GetDecibelFromPower(result.LAeqOctaveBandOneThird.Hz50 / readings.Count);
            result.LAeqOctaveBandOneThird.Hz63 = DecibelHelper.GetDecibelFromPower(result.LAeqOctaveBandOneThird.Hz63 / readings.Count);
            result.LAeqOctaveBandOneThird.Hz80 = DecibelHelper.GetDecibelFromPower(result.LAeqOctaveBandOneThird.Hz80 / readings.Count);
            result.LAeqOctaveBandOneThird.Hz100 = DecibelHelper.GetDecibelFromPower(result.LAeqOctaveBandOneThird.Hz100 / readings.Count);
            result.LAeqOctaveBandOneThird.Hz125 = DecibelHelper.GetDecibelFromPower(result.LAeqOctaveBandOneThird.Hz125 / readings.Count);
            result.LAeqOctaveBandOneThird.Hz160 = DecibelHelper.GetDecibelFromPower(result.LAeqOctaveBandOneThird.Hz160 / readings.Count);
            result.LAeqOctaveBandOneThird.Hz200 = DecibelHelper.GetDecibelFromPower(result.LAeqOctaveBandOneThird.Hz200 / readings.Count);
            result.LAeqOctaveBandOneThird.Hz250 = DecibelHelper.GetDecibelFromPower(result.LAeqOctaveBandOneThird.Hz250 / readings.Count);
            result.LAeqOctaveBandOneThird.Hz315 = DecibelHelper.GetDecibelFromPower(result.LAeqOctaveBandOneThird.Hz315 / readings.Count);
            result.LAeqOctaveBandOneThird.Hz400 = DecibelHelper.GetDecibelFromPower(result.LAeqOctaveBandOneThird.Hz400 / readings.Count);
            result.LAeqOctaveBandOneThird.Hz500 = DecibelHelper.GetDecibelFromPower(result.LAeqOctaveBandOneThird.Hz500 / readings.Count);
            result.LAeqOctaveBandOneThird.Hz630 = DecibelHelper.GetDecibelFromPower(result.LAeqOctaveBandOneThird.Hz630 / readings.Count);
            result.LAeqOctaveBandOneThird.Hz800 = DecibelHelper.GetDecibelFromPower(result.LAeqOctaveBandOneThird.Hz800 / readings.Count);
            result.LAeqOctaveBandOneThird.Hz1000 = DecibelHelper.GetDecibelFromPower(result.LAeqOctaveBandOneThird.Hz1000 / readings.Count);
            result.LAeqOctaveBandOneThird.Hz1250 = DecibelHelper.GetDecibelFromPower(result.LAeqOctaveBandOneThird.Hz1250 / readings.Count);
            result.LAeqOctaveBandOneThird.Hz1600 = DecibelHelper.GetDecibelFromPower(result.LAeqOctaveBandOneThird.Hz1600 / readings.Count);
            result.LAeqOctaveBandOneThird.Hz2000 = DecibelHelper.GetDecibelFromPower(result.LAeqOctaveBandOneThird.Hz2000 / readings.Count);
            result.LAeqOctaveBandOneThird.Hz2500 = DecibelHelper.GetDecibelFromPower(result.LAeqOctaveBandOneThird.Hz2500 / readings.Count);
            result.LAeqOctaveBandOneThird.Hz3150 = DecibelHelper.GetDecibelFromPower(result.LAeqOctaveBandOneThird.Hz3150 / readings.Count);
            result.LAeqOctaveBandOneThird.Hz4000 = DecibelHelper.GetDecibelFromPower(result.LAeqOctaveBandOneThird.Hz4000 / readings.Count);
            result.LAeqOctaveBandOneThird.Hz5000 = DecibelHelper.GetDecibelFromPower(result.LAeqOctaveBandOneThird.Hz5000 / readings.Count);
            result.LAeqOctaveBandOneThird.Hz6300 = DecibelHelper.GetDecibelFromPower(result.LAeqOctaveBandOneThird.Hz6300 / readings.Count);
            result.LAeqOctaveBandOneThird.Hz8000 = DecibelHelper.GetDecibelFromPower(result.LAeqOctaveBandOneThird.Hz8000 / readings.Count);
            result.LAeqOctaveBandOneThird.Hz10000 = DecibelHelper.GetDecibelFromPower(result.LAeqOctaveBandOneThird.Hz10000 / readings.Count);
            result.LAeqOctaveBandOneThird.Hz12500 = DecibelHelper.GetDecibelFromPower(result.LAeqOctaveBandOneThird.Hz12500 / readings.Count);
            result.LAeqOctaveBandOneThird.Hz16000 = DecibelHelper.GetDecibelFromPower(result.LAeqOctaveBandOneThird.Hz16000 / readings.Count);
            result.LAeqOctaveBandOneThird.Hz20000 = DecibelHelper.GetDecibelFromPower(result.LAeqOctaveBandOneThird.Hz20000 / readings.Count);

            return result;
        }

        public class OctaveBandOneThird
        {
            public double Hz6_3 { get; set; }
            public double Hz8 { get; set; }
            public double Hz10 { get; set; }
            public double Hz12_5 { get; set; }
            public double Hz16 { get; set; }
            public double Hz20 { get; set; }
            public double Hz25 { get; set; }
            public double Hz31_5 { get; set; }
            public double Hz40 { get; set; }
            public double Hz50 { get; set; }
            public double Hz63 { get; set; }
            public double Hz80 { get; set; }
            public double Hz100 { get; set; }
            public double Hz125 { get; set; }
            public double Hz160 { get; set; }
            public double Hz200 { get; set; }
            public double Hz250 { get; set; }
            public double Hz315 { get; set; }
            public double Hz400 { get; set; }
            public double Hz500 { get; set; }
            public double Hz630 { get; set; }
            public double Hz800 { get; set; }
            public double Hz1000 { get; set; }
            public double Hz1250 { get; set; }
            public double Hz1600 { get; set; }
            public double Hz2000 { get; set; }
            public double Hz2500 { get; set; }
            public double Hz3150 { get; set; }
            public double Hz4000 { get; set; }
            public double Hz5000 { get; set; }
            public double Hz6300 { get; set; }
            public double Hz8000 { get; set; }
            public double Hz10000 { get; set; }
            public double Hz12500 { get; set; }
            public double Hz16000 { get; set; }
            public double Hz20000 { get; set; }
        }

        public class OctaveBandOneOne
        {
            public double Hz16 { get; set; }
            public double Hz31_5 { get; set; }
            public double Hz63 { get; set; }
            public double Hz125 { get; set; }
            public double Hz250 { get; set; }
            public double Hz500 { get; set; }
            public double Hz1000 { get; set; }
            public double Hz2000 { get; set; }
            public double Hz4000 { get; set; }
            public double Hz8000 { get; set; }
            public double Hz16000 { get; set; }
        }

        public double GetValue(string displayValue)
        {
            switch (displayValue)
            {
                case "LAeq":
                    return LAeq;
                case "LCeq":
                    return LCeq;
                case "LAMax":
                    return LAMax;
                case "LAMin":
                    return LAMin;
                case "LZMax":
                    return LZMax;
                case "LZMin":
                    return LZMin;

                // One One Octave
                case "1-1-Hz16":
                    return LAeqOctaveBandOneOne.Hz16;
                case "1-1-Hz31_5":
                    return LAeqOctaveBandOneOne.Hz31_5;
                case "1-1-Hz63":
                    return LAeqOctaveBandOneOne.Hz63;
                case "1-1-Hz125":
                    return LAeqOctaveBandOneOne.Hz125;
                case "1-1-Hz250":
                    return LAeqOctaveBandOneOne.Hz250;
                case "1-1-Hz500":
                    return LAeqOctaveBandOneOne.Hz500;
                case "1-1-Hz1000":
                    return LAeqOctaveBandOneOne.Hz1000;
                case "1-1-Hz2000":
                    return LAeqOctaveBandOneOne.Hz2000;
                case "1-1-Hz4000":
                    return LAeqOctaveBandOneOne.Hz4000;
                case "1-1-Hz8000":
                    return LAeqOctaveBandOneOne.Hz8000;
                case "1-1-Hz16000":
                    return LAeqOctaveBandOneOne.Hz16000;

                // One Third Octave
                case "1-3-Hz6_3":
                    return LAeqOctaveBandOneThird.Hz6_3;
                case "1-3-Hz8":
                    return LAeqOctaveBandOneThird.Hz8;
                case "1-3-Hz10":
                    return LAeqOctaveBandOneThird.Hz10;
                case "1-3-Hz12_5":
                    return LAeqOctaveBandOneThird.Hz12_5;
                case "1-3-Hz16":
                    return LAeqOctaveBandOneThird.Hz16;
                case "1-3-Hz20":
                    return LAeqOctaveBandOneThird.Hz20;
                case "1-3-Hz25":
                    return LAeqOctaveBandOneThird.Hz25;
                case "1-3-Hz31_5":
                    return LAeqOctaveBandOneThird.Hz31_5;
                case "1-3-Hz40":
                    return LAeqOctaveBandOneThird.Hz40;
                case "1-3-Hz50":
                    return LAeqOctaveBandOneThird.Hz50;
                case "1-3-Hz63":
                    return LAeqOctaveBandOneThird.Hz63;
                case "1-3-Hz80":
                    return LAeqOctaveBandOneThird.Hz80;
                case "1-3-Hz100":
                    return LAeqOctaveBandOneThird.Hz100;
                case "1-3-Hz125":
                    return LAeqOctaveBandOneThird.Hz125;
                case "1-3-Hz160":
                    return LAeqOctaveBandOneThird.Hz160;
                case "1-3-Hz200":
                    return LAeqOctaveBandOneThird.Hz200;
                case "1-3-Hz250":
                    return LAeqOctaveBandOneThird.Hz250;
                case "1-3-Hz315":
                    return LAeqOctaveBandOneThird.Hz315;
                case "1-3-Hz400":
                    return LAeqOctaveBandOneThird.Hz400;
                case "1-3-Hz500":
                    return LAeqOctaveBandOneThird.Hz500;
                case "1-3-Hz630":
                    return LAeqOctaveBandOneThird.Hz630;
                case "1-3-Hz800":
                    return LAeqOctaveBandOneThird.Hz800;
                case "1-3-Hz1000":
                    return LAeqOctaveBandOneThird.Hz1000;
                case "1-3-Hz1250":
                    return LAeqOctaveBandOneThird.Hz1250;
                case "1-3-Hz1600":
                    return LAeqOctaveBandOneThird.Hz1600;
                case "1-3-Hz2000":
                    return LAeqOctaveBandOneThird.Hz2000;
                case "1-3-Hz2500":
                    return LAeqOctaveBandOneThird.Hz2500;
                case "1-3-Hz3150":
                    return LAeqOctaveBandOneThird.Hz3150;
                case "1-3-Hz4000":
                    return LAeqOctaveBandOneThird.Hz4000;
                case "1-3-Hz5000":
                    return LAeqOctaveBandOneThird.Hz5000;
                case "1-3-Hz6300":
                    return LAeqOctaveBandOneThird.Hz6300;
                case "1-3-Hz8000":
                    return LAeqOctaveBandOneThird.Hz8000;
                case "1-3-Hz10000":
                    return LAeqOctaveBandOneThird.Hz10000;
                case "1-3-Hz12500":
                    return LAeqOctaveBandOneThird.Hz12500;
                case "1-3-Hz16000":
                    return LAeqOctaveBandOneThird.Hz16000;
                case "1-3-Hz20000":
                    return LAeqOctaveBandOneThird.Hz20000;
                default:
                    return 0;
            }
        }
    }
}
