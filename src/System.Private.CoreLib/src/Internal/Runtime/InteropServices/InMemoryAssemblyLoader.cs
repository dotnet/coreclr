// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;

namespace Internal.Runtime.InteropServices
{
    public static class InMemoryAssemblyLoader
    {
        public static unsafe void LoadInMemoryAssembly(IntPtr handle, IntPtr assemblyPath)
        {
            AssemblyLoadContext context = new IsolatedComponentLoadContext(Marshal.PtrToStringUni(assemblyPath));
            context.LoadFromInMemoryModule(handle);
        }
    }
}
