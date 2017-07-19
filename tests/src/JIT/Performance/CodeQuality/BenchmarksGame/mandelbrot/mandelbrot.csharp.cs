// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
/* The Computer Language Benchmarks Game
   http://benchmarksgame.alioth.debian.org/

   started with Java #2 program (Krause/Whipkey/Bennet/AhnTran/Enotus/Stalcup)
   adapted for C# by Jan de Vaan
   simplified and optimised to use TPL by Anthony Lloyd

   posted to Benchmarks Game as mandelbrot C# .NET Core #4
   (http://benchmarksgame.alioth.debian.org/u64q/program.php?test=mandelbrot&lang=csharpcore&id=4)
   modified to remove concurrency and operate with xunit-performance
*/

using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Xunit.Performance;
using Microsoft.Xunit.Performance.Api;
using Xunit;

namespace BenchmarksGame
{
    public class Mandelbrot
    {
        private const int Iterations = 1;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static byte getByte(double[] Crb, double Ciby, int x, int y)
        {
            int res = 0;
            for (int i = 0; i < 8; i += 2)
            {
                double Crbx = Crb[x + i], Crbx1 = Crb[x + i + 1];
                double Zr1 = Crbx, Zr2 = Crbx1;
                double Zi1 = Ciby, Zi2 = Ciby;

                int b = 0;
                int j = 49;
                do
                {
                    double nZr1 = Zr1 * Zr1 - Zi1 * Zi1 + Crbx;
                    Zi1 = Zr1 * Zi1 + Zr1 * Zi1 + Ciby;
                    Zr1 = nZr1;

                    double nZr2 = Zr2 * Zr2 - Zi2 * Zi2 + Crbx1;
                    Zi2 = Zr2 * Zi2 + Zr2 * Zi2 + Ciby;
                    Zr2 = nZr2;

                    if (Zr1 * Zr1 + Zi1 * Zi1 > 4)
                    {
                        b |= 2;
                        if (b == 3)
                            break;
                    }
                    if (Zr2 * Zr2 + Zi2 * Zi2 > 4)
                    {
                        b |= 1;
                        if (b == 3)
                            break;
                    }
                } while (--j > 0);
                res = (res << 2) + b;
            }
            return (byte)(res ^ -1);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static byte[] Bench(int n, bool verbose = false)
        {
            double invN = 2.0 / n;
            var Crb = new double[n + 7];
            for (int i = 0; i < n; i++)
            {
                Crb[i] = i * invN - 1.5;
            }
            int lineLen = (n - 1) / 8 + 1;
            var data = new byte[n * lineLen];
            for (int i = 0; i < n; i++)
            {
                var Cibi = i * invN - 1.0;
                var offset = i * lineLen;
                for (int x = 0; x < lineLen; x++)
                    data[offset + x] = getByte(Crb, Cibi, x * 8, i);
            };

            if (verbose)
            {
                Console.Out.WriteLine("P4\n{0} {0}", n);
                Console.OpenStandardOutput().Write(data, 0, data.Length);
            }

            return data;
        }

        [Benchmark]
        [InlineData(4000)]
        public static void Test(int n)
        {
            foreach (var iteration in Benchmark.Iterations)
            {
                using (iteration.StartMeasurement())
                {
                    for (int i = 0; i < Iterations; i++)
                    {
                        Bench(n);
                    }
                }
            }
        }

        public static int Main(string[] args)
        {
            using (var ph = new XunitPerformanceHarness(args))
            {
                ph.RunBenchmarks(Assembly.GetEntryAssembly().Location);
            }
            return 100;
        }
    }
}
