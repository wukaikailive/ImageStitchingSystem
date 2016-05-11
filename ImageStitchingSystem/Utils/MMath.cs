using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Emgu.CV;


namespace ImageStitchingSystem.Utils
{
    public static class MMath
    {

        public static double Variance(double[] a)
        {
            double var = 0, n;
            double avrg = a.Average();
            n = a.Count<double>();
            foreach (double x in a)
            {
                var += (x - avrg) * (x - avrg);
            }
            var = var / n;
            return var;
        }

        public static double Variance(double a,double b)
        {
            return Variance(new double[] {a, b});
        }

        public static double Sum(this IEnumerable<double> source, int start, int end)
        {
            var r = 0D;
            int i = -1;
            foreach (var v in source)
            {
                i++;
                if(i<start)continue;
                if (i > end) break;
                r += v;
            }
            return r;
        }
    }


}
