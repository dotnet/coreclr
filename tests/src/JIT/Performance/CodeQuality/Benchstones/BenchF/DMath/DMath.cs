// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
//

using Microsoft.Xunit.Performance;
using System;
using System.Runtime.CompilerServices;
using Xunit;

[assembly: OptimizeForBenchmarks]

namespace Benchstone.BenchF
{
public static class DMath
{
#if DEBUG
    public const int Iterations = 1;
#else
    public const int Iterations = 100000;
#endif

    private const double Deg2Rad = 57.29577951;
    private static volatile object s_volatileObject;

    private static double[] sines;

    private static void Escape(object obj)
    {
        s_volatileObject = obj;
    }

    private static double Fact(double n)
    {
        double res;
        res = 1.0;
        while (n > 0.0)
        {
            res *= n;
            n -= 1.0;
        }

        return res;
    }

    private static double Power(double n, double p)
    {
        double res;
        res = 1.0;
        while (p > 0.0)
        {
            res *= n;
            p -= 1.0;
        }

        return res;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static bool Bench(int loop)
    {
        sines = new double[91];
        double angle, radians, sine, worksine, temp, k;
        double diff;

        for (int iter = 1; iter <= loop; iter++)
        {
            for (angle = 0.0; angle <= 90.0; angle += 1.0)
            {
                radians = angle / Deg2Rad;
                k = 0.0;
                worksine = 0.0;
                do
                {
                    sine = worksine;
                    temp = (2.0 * k) + 1.0;
                    worksine += (Power(-1.0, k) / Fact(temp)) * Power(radians, temp);
                    k += 1.0;
                    diff = Math.Abs(sine - worksine);
                } while (diff > 1E-8);

                sines[(int)angle] = worksine;
            }
        }

        // Escape sines array so that its elements appear live-out
        Escape(sines);

        return true;
    }

    [Benchmark]
    public static void Test()
    {
        foreach (var iteration in Benchmark.Iterations)
        {
            using (iteration.StartMeasurement())
            {
                Bench(Iterations);
            }
        }
    }

    private static bool TestBase()
    {
        bool result = Bench(Iterations);
        return result;
    }

    private static bool VerifyResult()
    {
        const double DIFF = 1E-7;
        bool result = sines[0] == 0;
        result &= Math.Abs(sines[1] - 0.017452406438222396) < DIFF;
        result &= Math.Abs(sines[2] - 0.034899496704390215) < DIFF;
        result &= Math.Abs(sines[3] - 0.05233595624597082) < DIFF;
        result &= Math.Abs(sines[4] - 0.06975647374787176) < DIFF;
        result &= Math.Abs(sines[5] - 0.08715574275233413) < DIFF;
        result &= Math.Abs(sines[6] - 0.104528463273252) < DIFF;
        result &= Math.Abs(sines[7] - 0.12186934341165429) < DIFF;
        result &= Math.Abs(sines[8] - 0.1391731009674482) < DIFF;
        result &= Math.Abs(sines[9] - 0.15643446504841677) < DIFF;
        result &= Math.Abs(sines[10] - 0.17364817767576296) < DIFF;
        result &= Math.Abs(sines[11] - 0.19080899538570711) < DIFF;
        result &= Math.Abs(sines[12] - 0.20791169082664432) < DIFF;
        result &= Math.Abs(sines[13] - 0.22495105435136867) < DIFF;
        result &= Math.Abs(sines[14] - 0.24192189561242688) < DIFF;
        result &= Math.Abs(sines[15] - 0.25881904511613474) < DIFF;
        result &= Math.Abs(sines[16] - 0.27563735583146026) < DIFF;
        result &= Math.Abs(sines[17] - 0.29237170473804036) < DIFF;
        result &= Math.Abs(sines[18] - 0.30901699439109465) < DIFF;
        result &= Math.Abs(sines[19] - 0.3255681544741578) < DIFF;
        result &= Math.Abs(sines[20] - 0.3420201433435495) < DIFF;
        result &= Math.Abs(sines[21] - 0.3583679495641095) < DIFF;
        result &= Math.Abs(sines[22] - 0.37460659343573377) < DIFF;
        result &= Math.Abs(sines[23] - 0.390731128510244) < DIFF;
        result &= Math.Abs(sines[24] - 0.4067366430981294) < DIFF;
        result &= Math.Abs(sines[25] - 0.42261826176470424) < DIFF;
        result &= Math.Abs(sines[26] - 0.43837114681522255) < DIFF;
        result &= Math.Abs(sines[27] - 0.45399049976850087) < DIFF;
        result &= Math.Abs(sines[28] - 0.46947156281860003) < DIFF;
        result &= Math.Abs(sines[29] - 0.48480962028412083) < DIFF;
        result &= Math.Abs(sines[30] - 0.5000000000446739) < DIFF;
        result &= Math.Abs(sines[31] - 0.515038074934949) < DIFF;
        result &= Math.Abs(sines[32] - 0.5299192642586027) < DIFF;
        result &= Math.Abs(sines[33] - 0.5446390350408898) < DIFF;
        result &= Math.Abs(sines[34] - 0.5591929034970311) < DIFF;
        result &= Math.Abs(sines[35] - 0.5735764363777011) < DIFF;
        result &= Math.Abs(sines[36] - 0.5877852523194377) < DIFF;
        result &= Math.Abs(sines[37] - 0.6018150231792486) < DIFF;
        result &= Math.Abs(sines[38] - 0.615661475353004) < DIFF;
        result &= Math.Abs(sines[39] - 0.6293203910772162) < DIFF;
        result &= Math.Abs(sines[40] - 0.6427876097138104) < DIFF;
        result &= Math.Abs(sines[41] - 0.6560590290174939) < DIFF;
        result &= Math.Abs(sines[42] - 0.6691306063853376) < DIFF;
        result &= Math.Abs(sines[43] - 0.6819983600881886) < DIFF;
        result &= Math.Abs(sines[44] - 0.6946583704835415) < DIFF;
        result &= Math.Abs(sines[45] - 0.7071067812094959) < DIFF;
        result &= Math.Abs(sines[46] - 0.7193398003594357) < DIFF;
        result &= Math.Abs(sines[47] - 0.7313537016370768) < DIFF;
        result &= Math.Abs(sines[48] - 0.7431448254915255) < DIFF;
        result &= Math.Abs(sines[49] - 0.7547095802320065) < DIFF;
        result &= Math.Abs(sines[50] - 0.7660444431219174) < DIFF;
        result &= Math.Abs(sines[51] - 0.777145961451877) < DIFF;
        result &= Math.Abs(sines[52] - 0.7880107535914426) < DIFF;
        result &= Math.Abs(sines[53] - 0.7986355100774779) < DIFF;
        result &= Math.Abs(sines[54] - 0.8090169944050627) < DIFF;
        result &= Math.Abs(sines[55] - 0.8191520443190246) < DIFF;
        result &= Math.Abs(sines[56] - 0.8290375725849848) < DIFF;
        result &= Math.Abs(sines[57] - 0.8386705679752776) < DIFF;
        result &= Math.Abs(sines[58] - 0.8480480961861993) < DIFF;
        result &= Math.Abs(sines[59] - 0.857167300731826) < DIFF;
        result &= Math.Abs(sines[60] - 0.8660254038141278) < DIFF;
        result &= Math.Abs(sines[61] - 0.8746197071691121) < DIFF;
        result &= Math.Abs(sines[62] - 0.8829475928887436) < DIFF;
        result &= Math.Abs(sines[63] - 0.8910065242183837) < DIFF;
        result &= Math.Abs(sines[64] - 0.8987940463295122) < DIFF;
        result &= Math.Abs(sines[65] - 0.9063077870674927) < DIFF;
        result &= Math.Abs(sines[66] - 0.913545457674155) < DIFF;
        result &= Math.Abs(sines[67] - 0.9205048534849748) < DIFF;
        result &= Math.Abs(sines[68] - 0.9271838546006372) < DIFF;
        result &= Math.Abs(sines[69] - 0.933580426532781) < DIFF;
        result &= Math.Abs(sines[70] - 0.939692620823725) < DIFF;
        result &= Math.Abs(sines[71] - 0.9455185756399914) < DIFF;
        result &= Math.Abs(sines[72] - 0.9510565163394394) < DIFF;
        result &= Math.Abs(sines[73] - 0.9563047560118436) < DIFF;
        result &= Math.Abs(sines[74] - 0.9612616959927462) < DIFF;
        result &= Math.Abs(sines[75] - 0.9659258263504312) < DIFF;
        result &= Math.Abs(sines[76] - 0.9702957263458689) < DIFF;
        result &= Math.Abs(sines[77] - 0.9743700648654934) < DIFF;
        result &= Math.Abs(sines[78] - 0.9781476008266797) < DIFF;
        result &= Math.Abs(sines[79] - 0.9816271834611594) < DIFF;
        result &= Math.Abs(sines[80] - 0.984807753024437) < DIFF;
        result &= Math.Abs(sines[81] - 0.9876883406060292) < DIFF;
        result &= Math.Abs(sines[82] - 0.9902680687510466) < DIFF;
        result &= Math.Abs(sines[83] - 0.992546151649297) < DIFF;
        result &= Math.Abs(sines[84] - 0.9945218953746517) < DIFF;
        result &= Math.Abs(sines[85] - 0.99619469809642) < DIFF;
        result &= Math.Abs(sines[86] - 0.9975640502626741) < DIFF;
        result &= Math.Abs(sines[87] - 0.9986295347554628) < DIFF;
        result &= Math.Abs(sines[88] - 0.9993908270178673) < DIFF;
        result &= Math.Abs(sines[89] - 0.9998476951528674) < DIFF;
        result &= Math.Abs(sines[90] - 0.9999999999939768) < DIFF;
        
        return result;
    }

    public static int Main()
    {
        TestBase();
        bool result = VerifyResult();
        return (result ? 100 : -1);
    }
}
}

