// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

// Loosly adapted from n-body C++ g++ #3 program
// https://benchmarksgame-team.pages.debian.net/benchmarksgame/program/nbody-gpp-3.html
// aka (as of 2018-09-07) https://salsa.debian.org/benchmarksgame-team/benchmarksgame/tree/979917ba7f2e4d646321be023513044f507e3497
// Best-scoring C++ version as of 2018-09-07 2nd overall after Fortran Intel #6


using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using static System.Runtime.Intrinsics.X86.Avx;
using static System.Runtime.Intrinsics.X86.Avx2;
using static System.Runtime.Intrinsics.X86.Fma;
using static System.Runtime.Intrinsics.X86.Sse;
using static System.Runtime.Intrinsics.X86.Sse2;
using static System.Runtime.Intrinsics.X86.Sse3;
using Microsoft.Xunit.Performance;

[assembly: OptimizeForBenchmarks]

namespace BenchmarksGame
{
    public static class NBodySimd
    {
        [Benchmark(InnerIterationCount = 1)]
        public static void RunBench()
        {
            Benchmark.Iterate(() => NBodySystem.Main(new string[] { 5_000_000.ToString() }));
        }
    }
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct Body
{
    public const double Pi = 3.141592653589793;
    public const double SolarMass = 4 * Pi * Pi;
    public const double DaysPerYear = 365.24;

    public double x;
    public double y;
    public double z;
    public double filler;
    public double vx;
    public double vy;
    public double vz;
    public double mass;

    public void offsetMomentum(double px, double py, double pz)
    {
        vx = -px / SolarMass;
        vy = -py / SolarMass;
        vz = -pz / SolarMass;
    }
};

[StructLayout(LayoutKind.Sequential)]
public struct R
{
    public double dx;
    public double dy;
    public double dz;
    public double filler;
};

[StructLayout(LayoutKind.Sequential, Pack = 32)]
public unsafe struct NBodySystem
{
    public const int BodyCount = 5;
    public const int N = (BodyCount - 1) * BodyCount / 2;

    private Body* bodies;

    public NBodySystem(Body* ptr)
    {
        bodies = ptr;

        ref Body p = ref bodies[0];
        p.mass = Body.SolarMass;

        p = ref bodies[1];
        p.x = 4.84143144246472090e+00;
        p.y = -1.16032004402742839e+00;
        p.z = -1.03622044471123109e-01;
        p.vx = 1.66007664274403694e-03 * Body.DaysPerYear;
        p.vy = 7.69901118419740425e-03 * Body.DaysPerYear;
        p.vz = -6.90460016972063023e-05 * Body.DaysPerYear;
        p.mass = 9.54791938424326609e-04 * Body.SolarMass;

        p = ref bodies[2];
        p.x = 8.34336671824457987e+00;
        p.y = 4.12479856412430479e+00;
        p.z = -4.03523417114321381e-01;
        p.vx = -2.76742510726862411e-03 * Body.DaysPerYear;
        p.vy = 4.99852801234917238e-03 * Body.DaysPerYear;
        p.vz = 2.30417297573763929e-05 * Body.DaysPerYear;
        p.mass = 2.85885980666130812e-04 * Body.SolarMass;

        p = ref bodies[3];
        p.x = 1.28943695621391310e+01;
        p.y = -1.51111514016986312e+01;
        p.z = -2.23307578892655734e-01;
        p.vx = 2.96460137564761618e-03 * Body.DaysPerYear;
        p.vy = 2.37847173959480950e-03 * Body.DaysPerYear;
        p.vz = -2.96589568540237556e-05 * Body.DaysPerYear;
        p.mass = 4.36624404335156298e-05 * Body.SolarMass;

        p = ref bodies[4];
        p.x = 1.53796971148509165e+01;
        p.y = -2.59193146099879641e+01;
        p.z = 1.79258772950371181e-01;
        p.vx = 2.68067772490389322e-03 * Body.DaysPerYear;
        p.vy = 1.62824170038242295e-03 * Body.DaysPerYear;
        p.vz = -9.51592254519715870e-05 * Body.DaysPerYear;
        p.mass = 5.15138902046611451e-05 * Body.SolarMass;

        double px = 0.0, py = 0.0, pz = 0.0;
        for (int i = 0; i < BodyCount; ++i)
        {
            p = ref bodies[i];
            px += p.vx * p.mass;
            py += p.vy * p.mass;
            pz += p.vz * p.mass;
        }

        bodies[0].offsetMomentum(px, py, pz);
    }

    public static void AdvanceAvxFma(double* constVectors, R* rPtr, double* magPtr, ref NBodySystem bodySystem, int count = 50_000_000)
    {
        Body* items = bodySystem.bodies;
        R* r = rPtr;
        double* mag = magPtr;

        double* rD = (double*)r;
        double* itemsPtr = (double*)items;

        if (Avx2.IsSupported && Fma.IsSupported)
        {

            Vector256<double> positionSol = LoadAlignedVector256(itemsPtr);
            Vector256<double> positionJupiter = LoadAlignedVector256(itemsPtr + 8);
            Vector256<double> positionSaturn = LoadAlignedVector256(itemsPtr + 16);
            Vector256<double> positionNeptune = LoadAlignedVector256(itemsPtr + 24);
            Vector256<double> positionUranus = LoadAlignedVector256(itemsPtr + 32);

            while (0 < count--)
            {
                itemsPtr = (double*)items;
                rD = (double*)r;
                mag = magPtr;

                Vector256<double> hor1 = Subtract(positionSol, positionJupiter);
                Vector256<double> hor2 = Subtract(positionSol, positionSaturn);
                Vector256<double> hor3 = Subtract(positionSol, positionNeptune);
                Vector256<double> hor4 = Subtract(positionSol, positionUranus);

                Vector256<double> dtQ = LoadAlignedVector256(constVectors);
                Vector256<double> oneFiveQ = LoadAlignedVector256(constVectors + 4);
                Vector256<double> oFiveQ = LoadAlignedVector256(constVectors + 8);

                StoreAligned((double*)r, hor1);
                StoreAligned((double*)(r + 1), hor2);

                hor1 = Multiply(hor1, hor1);
                hor2 = Multiply(hor2, hor2);

                StoreAligned((double*)(r + 2), hor3);
                StoreAligned((double*)(r + 3), hor4);

                Vector256<double> hor5 = Subtract(positionJupiter, positionSaturn);
                Vector256<double> hor6 = Subtract(positionJupiter, positionNeptune);
                Vector256<double> hor7 = Subtract(positionJupiter, positionUranus);
                Vector256<double> hor8 = Subtract(positionSaturn, positionNeptune);

                hor1 = HorizontalAdd(hor1, hor2);

                hor3 = Multiply(hor3, hor3);
                hor4 = Multiply(hor4, hor4);

                StoreAligned((double*)(r + 4), hor5);
                StoreAligned((double*)(r + 5), hor6);

                hor3 = HorizontalAdd(hor3, hor4);

                hor5 = Multiply(hor5, hor5);
                hor6 = Multiply(hor6, hor6);

                StoreAligned((double*)(r + 6), hor7);
                StoreAligned((double*)(r + 7), hor8);

                hor1 = Permute4x64(hor1, 0b11011000);
                hor3 = Permute4x64(hor3, 0b11011000);

                hor7 = Multiply(hor7, hor7);
                hor8 = Multiply(hor8, hor8);

                Vector256<double> distSquared = HorizontalAdd(hor1, hor3);

                hor5 = HorizontalAdd(hor5, hor6);
                hor7 = HorizontalAdd(hor7, hor8);

                distSquared = Permute4x64(distSquared, 0b11011000);

                hor5 = Permute4x64(hor5, 0b11011000);
                hor7 = Permute4x64(hor7, 0b11011000);

                Vector256<double> dist = ConvertToVector256Double(Avx.ReciprocalSqrt(ConvertToVector128Single(distSquared)));

                Vector256<double> distSquared2 = HorizontalAdd(hor5, hor7);
                distSquared2 = Permute4x64(distSquared2, 0b11011000);

                Vector256<double> distSquared05 = Multiply(oFiveQ, distSquared);
                Vector256<double> distSq = Multiply(dist, dist);
                Vector256<double> distSqSq05 = Multiply(distSquared05, dist);
                Vector256<double> dist15 = Multiply(dist, oneFiveQ);

                Vector256<double> dist2 = ConvertToVector256Double(Avx.ReciprocalSqrt(ConvertToVector128Single(distSquared2)));

                Vector256<double> distSquared052 = Multiply(oFiveQ, distSquared2);
                Vector256<double> distSq2 = Multiply(dist2, dist2);
                Vector256<double> distSqSq052 = Multiply(distSquared052, dist2);
                Vector256<double> dist152 = Multiply(dist2, oneFiveQ);

                dist = MultiplyAddNegated(distSqSq05, distSq, dist15);
                dist2 = MultiplyAddNegated(distSqSq052, distSq2, dist152);

                distSq = Multiply(dist, dist);
                dist15 = Multiply(dist, oneFiveQ);
                distSqSq05 = Multiply(distSquared05, dist);

                distSqSq052 = Multiply(distSquared052, dist2);
                distSq2 = Multiply(dist2, dist2);
                dist152 = Multiply(dist2, oneFiveQ);

                dist = MultiplyAddNegated(distSqSq05, distSq, dist15);
                dist2 = MultiplyAddNegated(distSqSq052, distSq2, dist152);

                hor1 = Subtract(positionSaturn, positionUranus);
                hor2 = Subtract(positionNeptune, positionUranus);

                Vector256<double> dmagQ = Multiply(Divide(dtQ, distSquared), dist);

                StoreAligned((double*)(r + 8), hor1);
                hor1 = Multiply(hor1, hor1);
                StoreAligned((double*)(r + 9), hor2);
                hor2 = Multiply(hor2, hor2);
                hor3 = SetZeroVector256<double>();

                hor1 = HorizontalAdd(hor1, hor2);
                hor1 = Permute4x64(hor1, 0b11011000);
                distSquared = HorizontalAdd(hor1, hor3);
                distSquared = Permute4x64(distSquared, 0b11011000);

                Vector256<double> dmagQ2 = Multiply(Divide(dtQ, distSquared2), dist2);
                StoreAligned(mag, dmagQ);

                var distSquaredFloat = ConvertToVector128Single(distSquared);
                dist = ConvertToVector256Double(Avx.ReciprocalSqrt(distSquaredFloat));

                distSquared05 = Multiply(oFiveQ, distSquared);
                distSq = Multiply(dist, dist);

                distSquaredFloat = Reciprocal(distSquaredFloat, Sse.SetAllVector128(2.0f));
                StoreAligned(&mag[4], dmagQ2);
                distSqSq05 = Multiply(distSquared05, dist);
                dist15 = Multiply(dist, oneFiveQ);

                dist = MultiplyAddNegated(distSqSq05, distSq, dist15);

                distSqSq05 = Multiply(distSquared05, dist);

                distSquared = ConvertToVector256Double(distSquaredFloat);

                distSq = Multiply(dist, dist);
                dist15 = Multiply(dist, oneFiveQ);

                dist = MultiplyAddNegated(distSqSq05, distSq, dist15);
                dmagQ = Multiply(Multiply(dtQ, distSquared), dist);
                StoreAligned(&mag[8], dmagQ);

                rD = (double*)r;
                itemsPtr = (double*)items;
                mag = magPtr;

                Vector256<double> iVec1 = LoadAlignedVector256(itemsPtr + 4);
                Vector256<double> massSol = Permute4x64(iVec1, 0b11111111);
                Vector256<double> iVec2 = LoadAlignedVector256(itemsPtr + 12);
                Vector256<double> massJupiter = Permute4x64(iVec2, 0b11111111);
                Vector256<double> iVec3 = LoadAlignedVector256(itemsPtr + 20);
                Vector256<double> massSaturn = Permute4x64(iVec3, 0b11111111);
                Vector256<double> magV = BroadcastScalarToVector256(mag);

                // 1, 2

                Vector256<double> rVec = LoadAlignedVector256(rD);

                Vector256<double> massVec2 = Multiply(massJupiter, magV);
                Vector256<double> massVec1 = Multiply(massSol, magV);

                magV = BroadcastScalarToVector256(++mag);

                Vector256<double> speedVecSol = MultiplyAddNegated(massVec2, rVec, iVec1);
                Vector256<double> speedVecJupiter = MultiplyAdd(rVec, massVec1, iVec2);

                // 1, 3

                rVec = LoadAlignedVector256(rD += 4);

                massVec2 = Multiply(massSaturn, magV);
                massVec1 = Multiply(massSol, magV);

                magV = BroadcastScalarToVector256(++mag);

                speedVecSol = MultiplyAddNegated(massVec2, rVec, speedVecSol);
                Vector256<double> speedVecSaturn = MultiplyAdd(rVec, massVec1, iVec3);

                Vector256<double> iVec4 = LoadAlignedVector256(itemsPtr + 28);
                Vector256<double> massNeptune = Permute4x64(iVec4, 0b11111111);

                // 1, 4

                rVec = LoadAlignedVector256(rD += 4);

                massVec2 = Multiply(massNeptune, magV);
                massVec1 = Multiply(massSol, magV);

                magV = BroadcastScalarToVector256(++mag);

                speedVecSol = MultiplyAddNegated(massVec2, rVec, speedVecSol);
                Vector256<double> speedVecNeptune = MultiplyAdd(rVec, massVec1, iVec4);

                Vector256<double> iVec5 = LoadAlignedVector256(itemsPtr + 36);
                Vector256<double> massUranus = Permute4x64(iVec5, 0b11111111);

                // 1, 5

                rVec = LoadAlignedVector256(rD += 4);

                massVec2 = Multiply(massUranus, magV);
                massVec1 = Multiply(massSol, magV);

                magV = BroadcastScalarToVector256(++mag);

                speedVecSol = MultiplyAddNegated(massVec2, rVec, speedVecSol);
                Vector256<double> speedVecUranus = MultiplyAdd(rVec, massVec1, iVec5);

                // 2, 3

                rVec = LoadAlignedVector256(rD += 4);

                massVec2 = Multiply(massSaturn, magV);
                massVec1 = Multiply(massJupiter, magV);

                magV = BroadcastScalarToVector256(++mag);

                speedVecJupiter = MultiplyAddNegated(massVec2, rVec, speedVecJupiter);
                speedVecSaturn = MultiplyAdd(rVec, massVec1, speedVecSaturn);

                // 2, 4

                rVec = LoadAlignedVector256(rD += 4);

                massVec2 = Multiply(massNeptune, magV);
                massVec1 = Multiply(massJupiter, magV);

                magV = BroadcastScalarToVector256(++mag);

                speedVecJupiter = MultiplyAddNegated(massVec2, rVec, speedVecJupiter);
                speedVecNeptune = MultiplyAdd(rVec, massVec1, speedVecNeptune);

                // 2, 5

                rVec = LoadAlignedVector256(rD += 4);

                massVec2 = Multiply(massUranus, magV);
                massVec1 = Multiply(massJupiter, magV);

                magV = BroadcastScalarToVector256(++mag);

                speedVecJupiter = MultiplyAddNegated(massVec2, rVec, speedVecJupiter);
                speedVecUranus = MultiplyAdd(rVec, massVec1, speedVecUranus);

                // 3, 4

                rVec = LoadAlignedVector256(rD += 4);

                massVec2 = Multiply(massNeptune, magV);
                massVec1 = Multiply(massSaturn, magV);

                magV = BroadcastScalarToVector256(++mag);

                speedVecSaturn = MultiplyAddNegated(massVec2, rVec, speedVecSaturn);
                speedVecNeptune = MultiplyAdd(rVec, massVec1, speedVecNeptune);

                // 3, 5

                rVec = LoadAlignedVector256(rD += 4);

                massVec2 = Multiply(massUranus, magV);
                massVec1 = Multiply(massSaturn, magV);

                magV = BroadcastScalarToVector256(++mag);

                speedVecSaturn = MultiplyAddNegated(massVec2, rVec, speedVecSaturn);
                speedVecUranus = MultiplyAdd(rVec, massVec1, speedVecUranus);

                // 4, 5

                rVec = LoadAlignedVector256(rD += 4);

                massVec2 = Multiply(massUranus, magV);
                massVec1 = Multiply(massNeptune, magV);

                // Prepare for finalize
                Vector256<double> distV = LoadAlignedVector256(constVectors);
                Vector256<double> mask = LoadAlignedVector256(constVectors + 12);

                // Finish 4, 5
                speedVecNeptune = MultiplyAddNegated(massVec2, rVec, speedVecNeptune);
                speedVecUranus = MultiplyAdd(rVec, massVec1, speedVecUranus);

                // Finalize and save new Hamiltonian

                positionSol = And(mask, MultiplyAdd(speedVecSol, distV, positionSol));
                positionJupiter = And(mask, MultiplyAdd(speedVecJupiter, distV, positionJupiter));
                positionSaturn = And(mask, MultiplyAdd(speedVecSaturn, distV, positionSaturn));
                positionNeptune = And(mask, MultiplyAdd(speedVecNeptune, distV, positionNeptune));
                positionUranus = And(mask, MultiplyAdd(speedVecUranus, distV, positionUranus));

                StoreAligned(itemsPtr + 4, speedVecSol);
                StoreAligned(itemsPtr + 12, speedVecJupiter);
                StoreAligned(itemsPtr + 20, speedVecSaturn);
                StoreAligned(itemsPtr + 28, speedVecNeptune);
                StoreAligned(itemsPtr + 36, speedVecUranus);
            }

            Vector256<double> storeMask = LoadAlignedVector256(constVectors + 12);

            MaskStore(itemsPtr, storeMask, positionSol);
            MaskStore(itemsPtr + 8, storeMask, positionJupiter);
            MaskStore(itemsPtr + 16, storeMask, positionSaturn);
            MaskStore(itemsPtr + 24, storeMask, positionNeptune);
            MaskStore(itemsPtr + 32, storeMask, positionUranus);

        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<float> Reciprocal(Vector128<float> value, Vector128<float> two)
    {
        var result = Avx.Reciprocal(value);
        result = Multiply(result, MultiplyAddNegated(result, value, two));
        return result;
    }

    public double Energy()
    {
        double e = 0.0;

        for (int i = 0; i < BodyCount; ++i)
        {
            ref Body iBody = ref bodies[i];
            double dx, dy, dz, distance;
            e += 0.5 * iBody.mass *
                (iBody.vx * iBody.vx
                    + iBody.vy * iBody.vy
                    + iBody.vz * iBody.vz);

            for (int j = i + 1; j < BodyCount; ++j)
            {
                ref Body jBody = ref bodies[j];
                dx = iBody.x - jBody.x;
                dy = iBody.y - jBody.y;
                dz = iBody.z - jBody.z;

                distance = Math.Sqrt(dx * dx + dy * dy + dz * dz);
                e -= (iBody.mass * jBody.mass) / distance;
            }
        }
        return e;
    }

    public static T* AlignAs<T>(T* pointer, uint alignment) where T : unmanaged
    {
        return (T*)(((ulong)pointer) + (alignment - ((ulong)pointer % alignment)));
    }

    public static int Main(string[] args)
    {
        int count = args.Length >= 1 ? int.Parse(args[0]) : 10_000_000;
        bool noGCRegion = false, success = false;
        if (noGCRegion = GC.TryStartNoGCRegion(2 * 1024 * 1024))
        {
            double* localCache = stackalloc double[40 + 24 + 16 * 4 + 16];

            // Align base pointer to L1 cache line boundary
            localCache = AlignAs<double>(localCache, 64);

            // Cast memory to pointers of used types
            Body* bodies = (Body*)localCache;
            double* constVectors = localCache + 40;
            R* rPtr = (R*)(localCache + 64);
            double* dPtr = (localCache + 112);

            {
                constVectors[0] = constVectors[1] = constVectors[2] = constVectors[3] = 0.01;
                constVectors[4] = constVectors[5] = constVectors[6] = constVectors[7] = 1.5;
                constVectors[8] = constVectors[9] = constVectors[10] = constVectors[11] = 0.5;
                constVectors[12] = constVectors[13] = constVectors[14] = BitConverter.Int64BitsToDouble(-1L);

                NBodySystem bodySystem = new NBodySystem(bodies);
                double startEnergy = bodySystem.Energy();
                Console.Out.WriteLineAsync(startEnergy.ToString("F9"));

                NBodySystem.AdvanceAvxFma(constVectors, rPtr, dPtr, ref bodySystem, count);

                double energy = bodySystem.Energy();
                Console.Out.WriteLine(energy.ToString("F9"));
                success = Math.Abs(startEnergy - energy) < 1e-4;
            }
        }

        if (noGCRegion)
        {
            GC.EndNoGCRegion();
        }

        return success ? 100 : -1;
    }
}
