// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;

namespace System.Runtime.InteropServices
{
    public static class InMemoryAssemblyLoader
    {
        public static unsafe int LoadAndExecuteInMemoryAssembly(IntPtr handle, string[] args)
        {
#if FEATURE_PAL
            throw new PlatformNotSupportedException();
#else
            Assembly entryAssembly = AssemblyLoadContext.Default.LoadFromInMemoryModule(handle);
            // Emulate traditional app start behavior that adds back on the path of the entry assembly
            // to the array set for Environment.SetCommandLineArgs.
            string[] environmentArgs = new string[args.Length + 1];
            environmentArgs[0] = entryAssembly.Location;
            Array.Copy(args, 0, environmentArgs, 1, args.Length);

            Environment.SetCommandLineArgs(environmentArgs);

            return ((RuntimeAssembly)entryAssembly).ExecuteMainMethod(args);
#endif
        }

        public static unsafe void LoadInMemoryAssembly(IntPtr handle)
        {
#if FEATURE_PAL
            throw new PlatformNotSupportedException();
#else
            AssemblyLoadContext.Default.LoadFromInMemoryModule(handle);
#endif
        }
    }
}
