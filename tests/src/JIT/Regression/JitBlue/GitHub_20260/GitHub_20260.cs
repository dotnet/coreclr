using System;
using System.Globalization;
using System.Numerics;

namespace GitHub_20260
{
    class Program
    {
        static int Main(string[] args)
        {         
            Vector<double> x = new Vector<double>();
            string s = ((IFormattable)x).ToString("G", CultureInfo.InvariantCulture);
            string e = "<0, 0, 0, 0>";

            if (s != e)
            {
                Console.WriteLine($"FAIL: Expected {e}, got {s}");
                return -1;
            }

            return 100;
        }
    }
}
