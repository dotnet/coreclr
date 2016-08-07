// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using static Internal.Runtime.Architecture;

namespace Internal.Runtime
{
    // Provides helper methods to System.Private.CoreLib that tell
    // things like what architecture it's being built for, what features
    // are available/not available, etc. without using ifdefs.

    internal static class BuildInformation
    {
        public static bool Is(Architecture architecture)
        {
#if ARM
            return architecture == Arm;
#elif ARM64
            return architecture == Arm64;
#elif AMD64
            return architecture == x64;
#else // ARM, ARM64. AMD64
            return architecture == x86;
#endif // ARM, ARM64, AMD64
        }
    }
}