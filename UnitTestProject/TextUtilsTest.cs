using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestProject
{
    [TestClass]
    public class TextUtilsTest
    {
        [TestMethod]
        public void TestGetFileName()
        {
            string test1 = "123\\123.txt";
            string result = ImageStitchingSystem.Utils.TextUtils.GetFileName(test1);
            Assert.AreEqual(result,"123.txt");
        }
    }
}
