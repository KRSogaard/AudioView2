using System;
using AudioView.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AudioView.Common.Tests
{
    [TestClass]
    public class DecibelHelperTests
    {
        [TestMethod]
        public void TestHzToName()
        {
            Assert.AreEqual("6.3Hz", DecibelHelper.HzToName("Hz6_3"));
        }
    }
}
