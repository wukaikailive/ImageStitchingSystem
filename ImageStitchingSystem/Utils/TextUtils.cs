using System;

namespace ImageStitchingSystem.Utils
{
    public class TextUtils
    {
        public static string GetFileName(string filePath)
        {
            string[] ps = filePath.Split('\\');
            return ps[ps.Length - 1];
        }

        public static double ParseZoomString()
        {
            return 0;
        }
    }
}
