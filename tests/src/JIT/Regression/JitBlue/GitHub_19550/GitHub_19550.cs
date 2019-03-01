// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Threading;

// Test folding of addressing expressions

public class Program
{
    struct S
    {
        public float f0;
        public float f1;
        public float f2;
        public float f3;
        public float f4;
        public float f5;
        public float f6;
        public float f7;
        public float f8;
        public float f9;
        public float f10;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    static unsafe int Test(ref S s, Vector128<float> v, int offset)
    {
        int returnVal = 100;
        // offset must be a multiple of the vector size in floats.
        offset &= ~3;
        fixed (float* p = &s.f0)
        {
            // We need an aligned address.
            int alignmentOffset = (((int)p & 0xc) >> 2);
            try
            {
                // This is the aligned case.
                Sse2.StoreScalar(p + alignmentOffset + 2, Sse2.Subtract(v, Sse2.LoadAlignedVector128(p + offset + alignmentOffset + 4)));
                // This is the unaligned case.
                Sse2.StoreScalar(p + alignmentOffset + 1, Sse2.Subtract(v, Sse2.LoadVector128(p + offset + alignmentOffset + 1)));
            }
            catch (Exception e)
            {
                Console.WriteLine("Unexpected exception: " + e.Message);
                returnVal = -1;
            }
        }
        return returnVal;
    }

    static int Main()
    {
        S s = new S();
        Vector128<float> v = Vector128.Create(1.0F);
        Test(ref s, v, 0);
        return 100;
    }
}
