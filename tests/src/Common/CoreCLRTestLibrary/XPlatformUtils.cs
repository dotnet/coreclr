// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace TestLibrary
{
    public class XPlatformUtils
    {
        public static string GetStandardNativeLibraryFileName(string simpleName)
        {
            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Win32NT:
                    return simpleName + ".dll";

                case PlatformID.MacOSX:
                    return "lib" + simpleName + ".dylib";

                default:
                    return "lib" + simpleName + ".so";
            }
        }
    }
}
