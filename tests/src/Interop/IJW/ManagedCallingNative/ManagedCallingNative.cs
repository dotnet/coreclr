// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace ManagedCallingNative
{
    class ManagedCallingNative
    {
        static int Main(string[] args)
        {
            // Load a fake mscoree.dll to avoid starting desktop
            LoadLibraryEx("mscoree.dll", IntPtr.Zero, 0);

            // Building with a reference to the IJW dll is difficult, so load via reflection instead
            Assembly ijwNativeDll = Assembly.Load("IjwNativeDll");
            Type testType = ijwNativeDll.GetType("TestClass");
            object testInstance = Activator.CreateInstance(testType);
            MethodInfo testMethod = testType.GetMethod("ManagedEntryPoint");
            int result = (int)testMethod.Invoke(testInstance, null);

            return result;
        }

        [DllImport("kernel32.dll")]
        static extern IntPtr LoadLibraryEx(string lpFileName, IntPtr hReservedNull, int dwFlags);
    }
}
