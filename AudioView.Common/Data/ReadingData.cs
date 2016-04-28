using System;

namespace AudioView.Common.Data
{
    public class ReadingData
    {
        public double LAeq { get; set; }
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
                Hz16 = calculateOneOneFromOneThirs(lAeqOctaveBandOneThird.Hz12_5,
                                                     lAeqOctaveBandOneThird.Hz16,
                                                     lAeqOctaveBandOneThird.Hz20),
                Hz31_5 = calculateOneOneFromOneThirs(lAeqOctaveBandOneThird.Hz25,
                                                     lAeqOctaveBandOneThird.Hz31_5,
                                                     lAeqOctaveBandOneThird.Hz40),
                Hz63 = calculateOneOneFromOneThirs(lAeqOctaveBandOneThird.Hz50,
                                                     lAeqOctaveBandOneThird.Hz63,
                                                     lAeqOctaveBandOneThird.Hz80),
                Hz125 = calculateOneOneFromOneThirs(lAeqOctaveBandOneThird.Hz100,
                                                     lAeqOctaveBandOneThird.Hz125,
                                                     lAeqOctaveBandOneThird.Hz160),
                Hz250 = calculateOneOneFromOneThirs(lAeqOctaveBandOneThird.Hz200,
                                                     lAeqOctaveBandOneThird.Hz250,
                                                     lAeqOctaveBandOneThird.Hz315),
                Hz500 = calculateOneOneFromOneThirs(lAeqOctaveBandOneThird.Hz400,
                                                     lAeqOctaveBandOneThird.Hz500,
                                                     lAeqOctaveBandOneThird.Hz630),
                Hz1000 = calculateOneOneFromOneThirs(lAeqOctaveBandOneThird.Hz800,
                                                     lAeqOctaveBandOneThird.Hz1000,
                                                     lAeqOctaveBandOneThird.Hz1250),
                Hz2000 = calculateOneOneFromOneThirs(lAeqOctaveBandOneThird.Hz1600,
                                                     lAeqOctaveBandOneThird.Hz2000,
                                                     lAeqOctaveBandOneThird.Hz2500),
                Hz4000 = calculateOneOneFromOneThirs(lAeqOctaveBandOneThird.Hz3150,
                                                     lAeqOctaveBandOneThird.Hz4000,
                                                     lAeqOctaveBandOneThird.Hz5000),
                Hz8000 = calculateOneOneFromOneThirs(lAeqOctaveBandOneThird.Hz6300,
                                                     lAeqOctaveBandOneThird.Hz8000,
                                                     lAeqOctaveBandOneThird.Hz10000),
                Hz16000 = calculateOneOneFromOneThirs(lAeqOctaveBandOneThird.Hz12500,
                                                     lAeqOctaveBandOneThird.Hz16000,
                                                     lAeqOctaveBandOneThird.Hz20000)
            };
        }

        private double calculateOneOneFromOneThirs(double one, double two, double three)
        {
            return 10*Math.Log(
                Math.Pow(10, (one/10)) +
                Math.Pow(10, (two/10)) +
                Math.Pow(10, (three/10)));
        }

        public static ReadingData operator +(ReadingData c1, ReadingData c2)
        {
            return new ReadingData()
            {
                LAeq = c1.LAeq + c2.LAeq,
                LAMax = c1.LAMax + c2.LAMax,
                LAMin = c1.LAMin + c2.LAMin,
                LZMax = c1.LZMax + c2.LZMax,
                LZMin = c1.LZMin + c2.LZMin,
                LAeqOctaveBandOneThird = c1.LAeqOctaveBandOneThird + c2.LAeqOctaveBandOneThird
            };
        }

        public static ReadingData operator /(ReadingData c1, int n)
        {
            return new ReadingData()
            {
                LAeq = c1.LAeq / n,
                LAMax = c1.LAMax / n,
                LAMin = c1.LAMin / n,
                LZMax = c1.LZMax / n,
                LZMin = c1.LZMin / n,
                LAeqOctaveBandOneThird = c1.LAeqOctaveBandOneThird / n
            };
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

            public static OctaveBandOneThird operator +(OctaveBandOneThird c1, OctaveBandOneThird c2)
            {
                if (c1 == null || c2 == null)
                    return null;
                return new OctaveBandOneThird()
                {
                    Hz6_3 = c1.Hz6_3 + c2.Hz6_3,
                    Hz8 = c1.Hz8 + c2.Hz8,
                    Hz10 = c1.Hz10 + c2.Hz10,
                    Hz12_5 = c1.Hz12_5 + c2.Hz12_5,
                    Hz16 = c1.Hz16 + c2.Hz16,
                    Hz20 = c1.Hz20 + c2.Hz20,
                    Hz25 = c1.Hz25 + c2.Hz25,
                    Hz31_5 = c1.Hz31_5 + c2.Hz31_5,
                    Hz40 = c1.Hz40 + c2.Hz40,
                    Hz50 = c1.Hz50 + c2.Hz50,
                    Hz63 = c1.Hz63 + c2.Hz63,
                    Hz80 = c1.Hz80 + c2.Hz80,
                    Hz100 = c1.Hz100 + c2.Hz100,
                    Hz125 = c1.Hz125 + c2.Hz125,
                    Hz160 = c1.Hz160 + c2.Hz160,
                    Hz200 = c1.Hz200 + c2.Hz200,
                    Hz250 = c1.Hz250 + c2.Hz250,
                    Hz315 = c1.Hz315 + c2.Hz315,
                    Hz400 = c1.Hz400 + c2.Hz400,
                    Hz500 = c1.Hz500 + c2.Hz500,
                    Hz630 = c1.Hz630 + c2.Hz630,
                    Hz800 = c1.Hz800 + c2.Hz800,
                    Hz1000 = c1.Hz1000 + c2.Hz1000,
                    Hz1250 = c1.Hz1250 + c2.Hz1250,
                    Hz1600 = c1.Hz1600 + c2.Hz1600,
                    Hz2000 = c1.Hz2000 + c2.Hz2000,
                    Hz2500 = c1.Hz2500 + c2.Hz2500,
                    Hz3150 = c1.Hz3150 + c2.Hz3150,
                    Hz4000 = c1.Hz4000 + c2.Hz4000,
                    Hz5000 = c1.Hz5000 + c2.Hz5000,
                    Hz6300 = c1.Hz6300 + c2.Hz6300,
                    Hz8000 = c1.Hz8000 + c2.Hz8000,
                    Hz10000 = c1.Hz10000 + c2.Hz10000,
                    Hz12500 = c1.Hz12500 + c2.Hz12500,
                    Hz16000 = c1.Hz16000 + c2.Hz16000,
                    Hz20000 = c1.Hz20000 + c2.Hz20000
                };
            }
            public static OctaveBandOneThird operator /(OctaveBandOneThird c1, int n)
            {

                if (c1 == null)
                    return null;
                return new OctaveBandOneThird()
                {
                    Hz6_3 = c1.Hz6_3 / n,
                    Hz8 = c1.Hz8 / n,
                    Hz10 = c1.Hz10 / n,
                    Hz12_5 = c1.Hz12_5 / n,
                    Hz16 = c1.Hz16 / n,
                    Hz20 = c1.Hz20 / n,
                    Hz25 = c1.Hz25 / n,
                    Hz31_5 = c1.Hz31_5 / n,
                    Hz40 = c1.Hz40 / n,
                    Hz50 = c1.Hz50 / n,
                    Hz63 = c1.Hz63 / n,
                    Hz80 = c1.Hz80 / n,
                    Hz100 = c1.Hz100 / n,
                    Hz125 = c1.Hz125 / n,
                    Hz160 = c1.Hz160 / n,
                    Hz200 = c1.Hz200 / n,
                    Hz250 = c1.Hz250 / n,
                    Hz315 = c1.Hz315 / n,
                    Hz400 = c1.Hz400 / n,
                    Hz500 = c1.Hz500 / n,
                    Hz630 = c1.Hz630 / n,
                    Hz800 = c1.Hz800 / n,
                    Hz1000 = c1.Hz1000 / n,
                    Hz1250 = c1.Hz1250 / n,
                    Hz1600 = c1.Hz1600 / n,
                    Hz2000 = c1.Hz2000 / n,
                    Hz2500 = c1.Hz2500 / n,
                    Hz3150 = c1.Hz3150 / n,
                    Hz4000 = c1.Hz4000 / n,
                    Hz5000 = c1.Hz5000 / n,
                    Hz6300 = c1.Hz6300 / n,
                    Hz8000 = c1.Hz8000 / n,
                    Hz10000 = c1.Hz10000 / n,
                    Hz12500 = c1.Hz12500 / n,
                    Hz16000 = c1.Hz16000 / n,
                    Hz20000 = c1.Hz20000 / n
                };
            }
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
    }
}
