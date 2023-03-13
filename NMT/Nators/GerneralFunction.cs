using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NMT.Nators
{
    public class GerneralFunction
    {
        public void MY_DEBUG(string inf)
        {
#if __ENABLE_DEBUG_OUTPUT_STRING__
            if (string.IsNullOrEmpty(inf) == false)
                System.Diagnostics.Debug.WriteLine(inf);
#endif
        }

        public static void MY_DEBUG(string inf, double[] x)
        {
            bool q = false;
            foreach (var t in x)
                if (t != 0)
                    q = true;
            if (q == false)
                return;

            foreach (var t in x)
                inf += t.ToString("0.000") + ", ";
            MY_DEBUG_Static(inf);
        }

        public static void MY_DEBUG_Static(string inf)
        {
#if __ENABLE_DEBUG_OUTPUT_STRING__
            if (string.IsNullOrEmpty(inf) == false)
                    System.Diagnostics.Debug.WriteLine(inf);
#endif
        }

        public static int LIMIT_MAX_MIN(int v, int UpperLimit, int LowerLimit)
        {
            if (v > UpperLimit) v = UpperLimit;
            if (v < LowerLimit) v = LowerLimit;
            return v;
        }

        public static uint LIMIT_MAX_MIN(uint v, uint UpperLimit, uint LowerLimit)
        {

            if (v > UpperLimit) v = UpperLimit;
            if (v < LowerLimit) v = LowerLimit;
            return v;
        }

        public static double Convert_ToDouble(string s)
        {
            if (string.IsNullOrEmpty(s) == true)
                return 0.0;
            else
                return Convert.ToDouble(s);
        }
    }
}
