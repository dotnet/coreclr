// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#if BIT64
using nuint = System.UInt64;
#else
using nuint = System.UInt32;
#endif

namespace System
{
    public static partial class Buffer
    {
#if ARM64
        // Determine optimal value for Windows.
        // https://github.com/dotnet/coreclr/issues/13843
        private const nuint MemmoveNativeThreshold = ulong.MaxValue;
        private const nuint MemmoveIntrinsicNativeThreshold = MemmoveNativeThreshold;
#else
        private const nuint MemmoveNativeThreshold = 2048;
        private const nuint MemmoveIntrinsicNativeThreshold = 4096;
#endif
    }
}
