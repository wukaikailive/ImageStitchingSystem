using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageStitchingSystem
{
    public class TextUtils
    {
        public static string getFileName(String filePath)
        {
            string[] ps = filePath.Split('\\');
            return ps[ps.Length - 1];
        }

        public static double parseZoomString()
        {
            return 0;
        }
    }
}
