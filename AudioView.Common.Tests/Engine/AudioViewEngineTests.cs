using System;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;
using AudioView.Common.Engine;
using AudioView.Common.Export;
using AudioView.Common.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace AudioView.Common.Engine
{
    /// <summary>
    /// Summary description for AudioViewEngineTests
    /// </summary>
    [TestClass]
    public class AudioViewEngineTests : AudioViewEngine
    {
        public AudioViewEngineTests()
        {
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void TestSnapDateTimeNoDrift()
        {
            DateTime start = new DateTime(2017, 03, 24, 20, 00, 00);
            DateTime currenTime = new DateTime(2017, 03, 24, 20, 10, 00);
            TimeSpan span = new TimeSpan(0, 0, 1, 0);

            DateTime result = RoundToNearest(start, currenTime, span);
            Assert.AreEqual(currenTime, result);
        }

        [TestMethod]
        public void TestSnapDateTimeSmallDrift()
        {
            DateTime start = new DateTime(2017, 03, 24, 20, 00, 00);
            DateTime expected = new DateTime(2017, 03, 24, 20, 10, 00);
            TimeSpan drift = new TimeSpan(0, 0, 2);
            DateTime currenTime = expected.Add(drift);
            TimeSpan span = new TimeSpan(0, 0, 1, 0);

            DateTime result = RoundToNearest(start, currenTime, span);
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void TestSnapDateTimeLargeDrift()
        {
            DateTime start = new DateTime(2017, 03, 24, 20, 00, 00);
            DateTime expected = new DateTime(2017, 03, 24, 20, 10, 00);
            TimeSpan drift = new TimeSpan(0, 0, 45);
            DateTime currenTime = expected.Add(drift);
            TimeSpan span = new TimeSpan(0, 0, 1, 0);

            DateTime result = RoundToNearest(start, currenTime, span);
            Assert.AreEqual(new DateTime(2017, 03, 24, 20, 11, 00), result);
        }

        [TestMethod]
        public void TestSnapDateTimeWithNonUniformNumbers()
        {
            DateTime start = new DateTime(2017, 03, 24, 20, 23, 43);
            TimeSpan span = new TimeSpan(0, 0, 15, 0);
            DateTime expected = start + span + span;
            TimeSpan drift = new TimeSpan(Convert.ToInt64(span.Ticks * 0.3d));
            DateTime currenTime = expected.Add(drift);

            DateTime result = RoundToNearest(start, currenTime, span);
            Assert.AreEqual(expected, result);
        }
    }
}
