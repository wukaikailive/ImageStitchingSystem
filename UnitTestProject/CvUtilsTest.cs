using System;
using Emgu.CV;
using ImageStitchingSystem.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestProject
{
    [TestClass]
    public class CvUtilsTest
    {
        [TestMethod]
        public void TestMatrixToString()
        {
            Matrix<double> data = new Matrix<double>(2, 2) {[0, 0] = 1, [0, 1] = 2, [1, 0] = 3, [1, 1] = 4};
            string result = CvUtils.MatrixToString(data);
            Assert.AreEqual(result, "[[1][2]]\n[[3][4]]\n");
            Assert.AreEqual(CvUtils.MatrixToString(data,"{","}"), "{{1}{2}}\n{{3}{4}}\n");
        }
    }
}
