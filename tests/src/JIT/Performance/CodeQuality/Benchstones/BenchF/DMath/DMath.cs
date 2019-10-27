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

    private const double MAX_ERROR = 1e-8;

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
                } while (diff > MAX_ERROR);

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
        // This class is calculating the sine of each degree from 0 to 90.
        // Expected results were calculated here: https://www.mathsisfun.com/scientific-calculator.html
        bool result = sines[0] == 0;
        result &= Math.Abs(sines[1] - 0.01745240643728351) <= MAX_ERROR;
        result &= Math.Abs(sines[2] - 0.03489949670250097) <= MAX_ERROR;
        result &= Math.Abs(sines[3] - 0.05233595624294383) <= MAX_ERROR;
        result &= Math.Abs(sines[4] - 0.0697564737441253) <= MAX_ERROR;
        result &= Math.Abs(sines[5] - 0.08715574274765817) <= MAX_ERROR;
        result &= Math.Abs(sines[6] - 0.10452846326765346) <= MAX_ERROR;
        result &= Math.Abs(sines[7] - 0.12186934340514748) <= MAX_ERROR;
        result &= Math.Abs(sines[8] - 0.13917310096006544) <= MAX_ERROR;
        result &= Math.Abs(sines[9] - 0.15643446504023087) <= MAX_ERROR;
        result &= Math.Abs(sines[10] - 0.17364817766693033) <= MAX_ERROR;
        result &= Math.Abs(sines[11] - 0.1908089953765448) <= MAX_ERROR;
        result &= Math.Abs(sines[12] - 0.20791169081775931) <= MAX_ERROR;
        result &= Math.Abs(sines[13] - 0.22495105434386498) <= MAX_ERROR;
        result &= Math.Abs(sines[14] - 0.24192189559966773) <= MAX_ERROR;
        result &= Math.Abs(sines[15] - 0.25881904510252074) <= MAX_ERROR;
        result &= Math.Abs(sines[16] - 0.27563735581699916) <= MAX_ERROR;
        result &= Math.Abs(sines[17] - 0.2923717047227367) <= MAX_ERROR;
        result &= Math.Abs(sines[18] - 0.3090169943749474) <= MAX_ERROR;
        result &= Math.Abs(sines[19] - 0.3255681544571567) <= MAX_ERROR;
        result &= Math.Abs(sines[20] - 0.3420201433256687) <= MAX_ERROR;
        result &= Math.Abs(sines[21] - 0.35836794954530027) <= MAX_ERROR;
        result &= Math.Abs(sines[22] - 0.374606593415912) <= MAX_ERROR;
        result &= Math.Abs(sines[23] - 0.39073112848927377) <= MAX_ERROR;
        result &= Math.Abs(sines[24] - 0.40673664307580015) <= MAX_ERROR;
        result &= Math.Abs(sines[25] - 0.42261826174069944) <= MAX_ERROR;
        result &= Math.Abs(sines[26] - 0.4383711467890774) <= MAX_ERROR;
        result &= Math.Abs(sines[27] - 0.45399049973954675) <= MAX_ERROR;
        result &= Math.Abs(sines[28] - 0.4694715627858908) <= MAX_ERROR;
        result &= Math.Abs(sines[29] - 0.48480962024633706) <= MAX_ERROR;
        result &= Math.Abs(sines[30] - 0.49999999999999994) <= MAX_ERROR;
        result &= Math.Abs(sines[31] - 0.5150380749100542) <= MAX_ERROR;
        result &= Math.Abs(sines[32] - 0.5299192642332049) <= MAX_ERROR;
        result &= Math.Abs(sines[33] - 0.5446390350150271) <= MAX_ERROR;
        result &= Math.Abs(sines[34] - 0.5591929034707468) <= MAX_ERROR;
        result &= Math.Abs(sines[35] - 0.573576436351046) <= MAX_ERROR;
        result &= Math.Abs(sines[36] - 0.5877852522924731) <= MAX_ERROR;
        result &= Math.Abs(sines[37] - 0.6018150231520483) <= MAX_ERROR;
        result &= Math.Abs(sines[38] - 0.6156614753256583) <= MAX_ERROR;
        result &= Math.Abs(sines[39] - 0.6293203910498374) <= MAX_ERROR;
        result &= Math.Abs(sines[40] - 0.6427876096865393) <= MAX_ERROR;
        result &= Math.Abs(sines[41] - 0.6560590289905073) <= MAX_ERROR;
        result &= Math.Abs(sines[42] - 0.6691306063588582) <= MAX_ERROR;
        result &= Math.Abs(sines[43] - 0.6819983600624985) <= MAX_ERROR;
        result &= Math.Abs(sines[44] - 0.6946583704589973) <= MAX_ERROR;
        result &= Math.Abs(sines[45] - 0.7071067811865475) <= MAX_ERROR;
        result &= Math.Abs(sines[46] - 0.7193398003386512) <= MAX_ERROR;
        result &= Math.Abs(sines[47] - 0.7313537016191705) <= MAX_ERROR;
        result &= Math.Abs(sines[48] - 0.7431448254773942) <= MAX_ERROR;
        result &= Math.Abs(sines[49] - 0.754709580222772) <= MAX_ERROR;
        result &= Math.Abs(sines[50] - 0.766044443118978) <= MAX_ERROR;
        result &= Math.Abs(sines[51] - 0.7771459614569709) <= MAX_ERROR;
        result &= Math.Abs(sines[52] - 0.7880107536067219) <= MAX_ERROR;
        result &= Math.Abs(sines[53] - 0.7986355100472928) <= MAX_ERROR;
        result &= Math.Abs(sines[54] - 0.8090169943749475) <= MAX_ERROR;
        result &= Math.Abs(sines[55] - 0.8191520442889918) <= MAX_ERROR;
        result &= Math.Abs(sines[56] - 0.8290375725550417) <= MAX_ERROR;
        result &= Math.Abs(sines[57] - 0.8386705679454239) <= MAX_ERROR;
        result &= Math.Abs(sines[58] - 0.8480480961564261) <= MAX_ERROR;
        result &= Math.Abs(sines[59] - 0.8571673007021122) <= MAX_ERROR;
        result &= Math.Abs(sines[60] - 0.8660254037844386) <= MAX_ERROR;
        result &= Math.Abs(sines[61] - 0.8746197071393957) <= MAX_ERROR;
        result &= Math.Abs(sines[62] - 0.8829475928589269) <= MAX_ERROR;
        result &= Math.Abs(sines[63] - 0.8910065241883678) <= MAX_ERROR;
        result &= Math.Abs(sines[64] - 0.898794046299167) <= MAX_ERROR;
        result &= Math.Abs(sines[65] - 0.9063077870366499) <= MAX_ERROR;
        result &= Math.Abs(sines[66] - 0.9135454576426009) <= MAX_ERROR;
        result &= Math.Abs(sines[67] - 0.9205048534524404) <= MAX_ERROR;
        result &= Math.Abs(sines[68] - 0.9271838545667873) <= MAX_ERROR;
        result &= Math.Abs(sines[69] - 0.9335804264972017) <= MAX_ERROR;
        result &= Math.Abs(sines[70] - 0.9396926207859083) <= MAX_ERROR;
        result &= Math.Abs(sines[71] - 0.9455185755993167) <= MAX_ERROR;
        result &= Math.Abs(sines[72] - 0.9510565162951535) <= MAX_ERROR;
        result &= Math.Abs(sines[73] - 0.9563047559630354) <= MAX_ERROR;
        result &= Math.Abs(sines[74] - 0.9612616959383189) <= MAX_ERROR;
        result &= Math.Abs(sines[75] - 0.9659258262890683) <= MAX_ERROR;
        result &= Math.Abs(sines[76] - 0.9702957262759965) <= MAX_ERROR;
        result &= Math.Abs(sines[77] - 0.9743700647852352) <= MAX_ERROR;
        result &= Math.Abs(sines[78] - 0.9781476007338056) <= MAX_ERROR;
        result &= Math.Abs(sines[79] - 0.981627183447664) <= MAX_ERROR;
        result &= Math.Abs(sines[80] - 0.984807753012208) <= MAX_ERROR;
        result &= Math.Abs(sines[81] - 0.9876883405951378) <= MAX_ERROR;
        result &= Math.Abs(sines[82] - 0.9902680687415704) <= MAX_ERROR;
        result &= Math.Abs(sines[83] - 0.992546151641322) <= MAX_ERROR;
        result &= Math.Abs(sines[84] - 0.9945218953682733) <= MAX_ERROR;
        result &= Math.Abs(sines[85] - 0.9961946980917455) <= MAX_ERROR;
        result &= Math.Abs(sines[86] - 0.9975640502598242) <= MAX_ERROR;
        result &= Math.Abs(sines[87] - 0.9986295347545738) <= MAX_ERROR;
        result &= Math.Abs(sines[88] - 0.9993908270190958) <= MAX_ERROR;
        result &= Math.Abs(sines[89] - 0.9998476951563913) <= MAX_ERROR;
        result &= Math.Abs(sines[90] - 1) <= MAX_ERROR;
        
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

