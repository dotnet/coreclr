// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
//

using System;
using System.Runtime.CompilerServices;

namespace MathFusedMultiplyAddTest
{
    class Program
    {
        private static int _returnCode = 100;

        static int Main()
        {
            TestFloats();
            TestDoubles();
            return _returnCode;
        }

#region MathF.FusedMultiplyAdd
        static void TestFloats()
        {
            float[] testValues =
                {
                    MathF.PI, MathF.E, 0.0f, -0.0f, float.MinValue, float.MaxValue, 42, -42, 1000, -1000,
                    int.MaxValue, int.MinValue, float.NaN, float.PositiveInfinity, float.NegativeInfinity
                };

            foreach (float a in testValues)
            {
                foreach (float b in testValues)
                {
                    foreach (float c in testValues)
                    {
                        Check1(a, b, c);
                        Check2(a, b, c);
                        Check3(a, b, c);
                        Check4(a, b, c);
                        Check5(a, b, c);
                        Check6(a, b, c);
                        Check7(a, b, c);
                        Check8(a, b, c);
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        static void Check1(float a, float b, float c) =>
            CompareFloats(ReferenceMultiplyAdd( a,  b,  c), 
                        MathF.FusedMultiplyAdd( a,  b,  c));

        [MethodImpl(MethodImplOptions.NoInlining)]
        static void Check2(float a, float b, float c) =>
            CompareFloats(ReferenceMultiplyAdd(-a,  b,  c),
                        MathF.FusedMultiplyAdd(-a,  b,  c));

        [MethodImpl(MethodImplOptions.NoInlining)]
        static void Check3(float a, float b, float c) =>
            CompareFloats(ReferenceMultiplyAdd(-a, -b,  c),
                        MathF.FusedMultiplyAdd(-a, -b,  c));

        [MethodImpl(MethodImplOptions.NoInlining)]
        static void Check4(float a, float b, float c) =>
            CompareFloats(ReferenceMultiplyAdd(-a, -b, -c),
                        MathF.FusedMultiplyAdd(-a, -b, -c));

        [MethodImpl(MethodImplOptions.NoInlining)]
        static void Check5(float a, float b, float c) =>
            CompareFloats(ReferenceMultiplyAdd( a, -b,  c), 
                        MathF.FusedMultiplyAdd( a, -b,  c));

        [MethodImpl(MethodImplOptions.NoInlining)]
        static void Check6(float a, float b, float c) =>
            CompareFloats(ReferenceMultiplyAdd( a, -b, -c), 
                        MathF.FusedMultiplyAdd( a, -b, -c));

        [MethodImpl(MethodImplOptions.NoInlining)]
        static void Check7(float a, float b, float c) =>
            CompareFloats(ReferenceMultiplyAdd(-a,  b, -c), 
                        MathF.FusedMultiplyAdd(-a,  b, -c));

        [MethodImpl(MethodImplOptions.NoInlining)]
        static void Check8(float a, float b, float c) =>
            CompareFloats(ReferenceMultiplyAdd( a,  b, -c), 
                        MathF.FusedMultiplyAdd( a,  b, -c));

        [MethodImpl(MethodImplOptions.NoInlining)]
        static float ReferenceMultiplyAdd(float a, float b, float c) => a * b + c;

        [MethodImpl(MethodImplOptions.NoInlining)]
        static void CompareFloats(float a, float b)
        {
            if (Math.Abs(a - b) > 0.001f)
            {
                Console.WriteLine($"{a} != {b}");
                _returnCode--;
            }
        }
#endregion

#region Math.FusedMultiplyAdd
        static void TestDoubles()
        {
            double[] testValues =
                {
                    Math.PI, Math.E, 0.0, -0.0, double.MinValue, double.MaxValue, 42, -42, 100000, -100000,
                    long.MaxValue, long.MinValue, double.NaN, double.PositiveInfinity, double.NegativeInfinity
                };

            foreach (double a in testValues)
            {
                foreach (double b in testValues)
                {
                    foreach (double c in testValues)
                    {
                        Check1(a, b, c);
                        Check2(a, b, c);
                        Check3(a, b, c);
                        Check4(a, b, c);
                        Check5(a, b, c);
                        Check6(a, b, c);
                        Check7(a, b, c);
                        Check8(a, b, c);
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        static void Check1(double a, double b, double c) =>
            CompareDoubles(ReferenceMultiplyAdd( a,  b,  c), 
                          Math.FusedMultiplyAdd( a,  b,  c));

        [MethodImpl(MethodImplOptions.NoInlining)]
        static void Check2(double a, double b, double c) =>
            CompareDoubles(ReferenceMultiplyAdd(-a,  b,  c),
                          Math.FusedMultiplyAdd(-a,  b,  c));

        [MethodImpl(MethodImplOptions.NoInlining)]
        static void Check3(double a, double b, double c) =>
            CompareDoubles(ReferenceMultiplyAdd(-a, -b,  c),
                          Math.FusedMultiplyAdd(-a, -b,  c));

        [MethodImpl(MethodImplOptions.NoInlining)]
        static void Check4(double a, double b, double c) =>
            CompareDoubles(ReferenceMultiplyAdd(-a, -b, -c),
                          Math.FusedMultiplyAdd(-a, -b, -c));

        [MethodImpl(MethodImplOptions.NoInlining)]
        static void Check5(double a, double b, double c) =>
            CompareDoubles(ReferenceMultiplyAdd( a, -b,  c), 
                          Math.FusedMultiplyAdd( a, -b,  c));

        [MethodImpl(MethodImplOptions.NoInlining)]
        static void Check6(double a, double b, double c) =>
            CompareDoubles(ReferenceMultiplyAdd( a, -b, -c), 
                          Math.FusedMultiplyAdd( a, -b, -c));

        [MethodImpl(MethodImplOptions.NoInlining)]
        static void Check7(double a, double b, double c) =>
            CompareDoubles(ReferenceMultiplyAdd(-a,  b, -c), 
                          Math.FusedMultiplyAdd(-a,  b, -c));

        [MethodImpl(MethodImplOptions.NoInlining)]
        static void Check8(double a, double b, double c) =>
            CompareDoubles(ReferenceMultiplyAdd( a,  b, -c), 
                          Math.FusedMultiplyAdd( a,  b, -c));

        [MethodImpl(MethodImplOptions.NoInlining)]
        static double ReferenceMultiplyAdd(double a, double b, double c) => a * b + c;

        [MethodImpl(MethodImplOptions.NoInlining)]
        static void CompareDoubles(double a, double b)
        {
            if (Math.Abs(a - b) > 0.00001)
            {
                Console.WriteLine($"{a} != {b}");
                _returnCode--;
            }
        }
#endregion
    }
}
