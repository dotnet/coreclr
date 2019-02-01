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
        private delegate int EntryPoint(string[] args);

        public static unsafe int LoadAndExecuteInMemoryAssembly(IntPtr handle, string[] args)
        {
#if FEATURE_PAL
            throw new PlatformNotSupportedException();
#else
            Assembly entryAssembly = AssemblyLoadContext.Default.LoadFromInMemoryModule(handle);
            MethodInfo entryPointMethod = entryAssembly.EntryPoint;
            
            if (entryPointMethod == null)
            {
                throw new EntryPointNotFoundException();
            }

            EntryPoint entryPoint = (EntryPoint)entryPointMethod.CreateDelegate(typeof(EntryPoint));
            return entryPoint(args);
#endif
        }

        public static unsafe void LoadInMemoryAssembly(IntPtr handle)
        {
#if FEATURE_PAL
            throw new PlatformNotSupportedException();
#else
            AssemblyLoadContext context = new IndividualAssemblyLoadContext();
            context.LoadFromInMemoryModule(handle);
#endif
        }
    }
}
