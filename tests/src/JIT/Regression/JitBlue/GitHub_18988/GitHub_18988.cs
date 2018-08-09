// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
//

using System;

namespace System
{
    class Program
    {
        static int Main(string[] args)
        {
            Decimal lo = 1M;
            Decimal mid = 100000000000M;
            //Decimal hi = 10000000000000000000M;

            double loAsDouble = Convert.ToDouble(lo);
            double loAsMid = Convert.ToDouble(mid);
            //double loAsHi = Convert.ToDouble(hi);
            //const double eps = 0.00001;

            Console.WriteLine(loAsDouble + ", " + loAsMid);

            //Console.WriteLine(loAsDouble + ", " + loAsMid + ", " + loAsHi);
            //System.Diagnostics.Debug.Assert(loAsDouble - 1.0 < eps && loAsDouble - 1.0 > -eps);
            //System.Diagnostics.Debug.Assert(loAsMid - 100000000000.0 < eps && loAsMid - 100000000000.0 > -eps);
            //System.Diagnostics.Debug.Assert(loAsHi - 10000000000000000000.0 < eps && loAsHi - 10000000000000000000.0 > -eps);
            //Console.WriteLine("Passed");
            return 100;
        }
    }
}
