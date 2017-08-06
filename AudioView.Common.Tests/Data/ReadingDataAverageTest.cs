using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.Components.DictionaryAdapter;

namespace AudioView.Common.Data
{
    [TestClass]
    public class ReadingDataAverageTest
    {

        [TestMethod]
        public void TestAverageWith1Reading()
        {
            TestReadings(60, new EditableList<double>()
            {
                60
            });
        }


        [TestMethod]
        public void TestAverageWith2Readings()
        {
            TestReadings(58.183, new EditableList<double>()
            {
                55,
                60
            });
        }


        [TestMethod]
        public void TestAverageWith5Readings()
        {
            TestReadings(62.376, new EditableList<double>()
            {
                55,
                60,
                65,
                60,
                65
            });
        }


        [TestMethod]
        public void TestAverageWith10Readings()
        {
            TestReadings(93.471, new EditableList<double>()
            {
                90,
                92,
                92,
                93,
                94,
                93,
                93,
                95,
                98,
                80
            });
        }


        private void TestReadings(double expected, List<double> readings)
        {
            List<ReadingData> readingsData = new EditableList<ReadingData>();
            foreach (var readingData in readings)
            {
                readingsData.Add(new ReadingData()
                {
                    LAeq = readingData
                });
            }

            var result = ReadingData.Average(readingsData);
            Assert.AreEqual(Math.Round(expected, 2), Math.Round(result.LAeq, 2));
        }
    }
}
