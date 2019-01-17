// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;

namespace System
{
    internal static partial class Number
    {
        // This is a port of the `DiyFp` implementation here: https://github.com/google/double-conversion/blob/a711666ddd063eb1e4b181a6cb981d39a1fc8bac/double-conversion/diy-fp.h
        // The backing structure and how it is used is described in more detail here: http://www.cs.tufts.edu/~nr/cs257/archive/florian-loitsch/printf.pdf

        // This "Do It Yourself Floating Point" class implements a floating-point number with a ulong significand and an int exponent.
        // Normalized DiyFp numbers will have the most significant bit of the significand set.
        // Multiplication and Subtraction do not normalize their results.
        // DiyFp are not designed to contain special doubles (NaN and Infinity).
        internal readonly ref struct DiyFp
        {
            public const int SignificandSize = 64;

            public readonly ulong f;
            public readonly int e;

            public DiyFp(double value)
            {
                Debug.Assert(double.IsFinite(value));
                Debug.Assert(value > 0.0);
                f = ExtractFractionAndBiasedExponent(value, out e);
            }

            public DiyFp(ulong significand, int exponent)
            {
                f = significand;
                e = exponent;
            }

            public DiyFp Multiply(in DiyFp other)
            {
                // Simply "emulates" a 128-bit multiplcation
                //
                // However: the resulting number only contains 64-bits. The least
                // signficant 64-bits are only used for rounding the most significant
                // 64-bits.

                uint a = (uint)(f >> 32);
                uint b = (uint)(f);

                uint c = (uint)(other.f >> 32);
                uint d = (uint)(other.f);

                ulong ac = ((ulong)(a) * c);
                ulong bc = ((ulong)(b) * c);
                ulong ad = ((ulong)(a) * d);
                ulong bd = ((ulong)(b) * d);

                ulong tmp = (bd >> 32) + (uint)(ad) + (uint)(bc);

                // By adding (1UL << 31) to tmp, we round the final result.
                // Halfway cases will be rounded up.

                tmp += (1U << 31);

                return new DiyFp((ac + (ad >> 32) + (bc >> 32) + (tmp >> 32)), (e + other.e + SignificandSize));
            }

            public DiyFp Normalize()
            {
                // This method is mainly called for normalizing boundaries.
                //
                // We deviate from the reference implementation by just using
                // our LeadingZeroCount function so that we only need to shift
                // and subtract once.

                Debug.Assert(f != 0);
                int lzcnt = (int)(BigInteger.LeadingZeroCount(f));
                return new DiyFp((f << lzcnt), (e - lzcnt));
            }

            // The exponents of both numbers must be the same.
            // The significand of this must be bigger than the significand of other.
            // The result will not be normalized.
            public DiyFp Subtract(in DiyFp other)
            {
                Debug.Assert(e == other.e);
                Debug.Assert(f >= other.f);
                return new DiyFp((f - other.f), e);
            }
        }
    }
}
