// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.X86;

namespace System.Buffers
{
    internal static class Utilities
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int SelectBucketIndex(int bufferSize)
        {
            Debug.Assert(bufferSize >= 0);
            if (Lzcnt.IsSupported)
            {
                uint bits = ((uint)bufferSize - 1) >> 4;
                return 32 - (int)Lzcnt.LeadingZeroCount(bits);
            }

            // bufferSize of 0 will underflow here, causing a huge
            // index which the caller will discard because it is not
            // within the bounds of the bucket array.
            uint bitsRemaining = ((uint)bufferSize - 1) >> 4;

            int poolIndex = 0;
            if (bitsRemaining > 0xFFFF) { bitsRemaining >>= 16; poolIndex = 16; }
            if (bitsRemaining > 0xFF)   { bitsRemaining >>= 8;  poolIndex += 8; }
            if (bitsRemaining > 0xF)    { bitsRemaining >>= 4;  poolIndex += 4; }
            if (bitsRemaining > 0x3)    { bitsRemaining >>= 2;  poolIndex += 2; }
            if (bitsRemaining > 0x1)    { bitsRemaining >>= 1;  poolIndex += 1; }

            return poolIndex + (int)bitsRemaining;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int GetMaxSizeForBucket(int binIndex)
        {
            int maxSize = 16 << binIndex;
            Debug.Assert(maxSize >= 0);
            return maxSize;
        }
    }
}
