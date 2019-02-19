// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using Internal.Runtime.InteropServices;
using TestLibrary;

namespace NativeCallingManaged
{
    class NativeCallingManaged
    {
        unsafe static int Main(string[] args)
        {
            try
            {

                HostPolicyMock.Initialize(Environment.CurrentDirectory, null);

                // Load our fake mscoree to prevent desktop from loading.
                NativeLibrary.Load(Path.Combine(Environment.CurrentDirectory, "mscoree.dll"));

                string ijwModulePath = Path.Combine(Environment.CurrentDirectory, "IjwNativeCallingManagedDll.dll");
                IntPtr ijwNativeHandle = NativeLibrary.Load(ijwModulePath);

                using (HostPolicyMock.Mock_corehost_resolve_component_dependencies(
                    0,
                    ijwModulePath,
                    string.Empty,
                    string.Empty))
                fixed (char* path = ijwModulePath)
                {
                    InMemoryAssemblyLoader.LoadInMemoryAssembly(ijwNativeHandle, (IntPtr)path);
                }
                
                NativeEntryPointDelegate nativeEntryPoint = Marshal.GetDelegateForFunctionPointer<NativeEntryPointDelegate>(NativeLibrary.GetExport(ijwNativeHandle, "NativeEntryPoint"));

                Assert.AreEqual(100, nativeEntryPoint());
                
                return 100;
            }
            catch (Exception ex)
            {
                Internal.Console.WriteLine(ex.ToString());

                return 101;
            }
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        delegate int NativeEntryPointDelegate();

    }
}
