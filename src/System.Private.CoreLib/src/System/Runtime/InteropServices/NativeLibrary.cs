// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Reflection;
using System.IO;
using System.Runtime.Versioning;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;

namespace System.Runtime.InteropServices
{
    // ToString() must return debugging string that includes source-name, target name that was loaded, and location details.
    public class NativeLibrary
    {
        public IntPtr Handle { get; set; }
        public string Name { get; set; } // Will Return the name referenced by the importing assembly

        // Stores a map of assemblies and on-library-load-callbacks assigned to them.
        // Simulates an additional field of Assembly (which may be added in the future).
        private static ConditionalWeakTable<Assembly, Func<LoadNativeLibraryArgs, NativeLibrary>> AssemblyToCallbackMap { set; get; }

        public NativeLibrary(string libraryName, IntPtr handle)
        {
            Name = libraryName;
            Handle = handle;
        }

        static NativeLibrary()
        {
            AssemblyToCallbackMap = new ConditionalWeakTable<Assembly, Func<LoadNativeLibraryArgs, NativeLibrary>>();
        }

        // It is called by a user per assembly: RegisterNativeLibraryLoadCallback(assembly, customMappingCallback)
        // triggered by an event on AppDomain
        public static bool RegisterNativeLibraryLoadCallback(Assembly assembly, Func<LoadNativeLibraryArgs, NativeLibrary> callback)
        {
            Func<LoadNativeLibraryArgs, NativeLibrary> registeredCallback = null;
            bool callbackAlreadyRegistered = AssemblyToCallbackMap.TryGetValue(assembly, out registeredCallback);

            if (!callbackAlreadyRegistered)
            {
                AssemblyToCallbackMap.Add(assembly, callback);
            }
            else
            {
                AssemblyToCallbackMap.Remove(assembly);
                AssemblyToCallbackMap.Add(assembly, callback);
            }

            return true;
        }

        // A wrapper function for Load(string libraryName, DllImportSearchPath dllImportSearchPath, Assembly assembly)
        public static NativeLibrary Load(string libraryName)
        {
            return Load(libraryName, DllImportSearchPath.LegacyBehavior, null);
        }

        public static NativeLibrary Load(string libraryName, DllImportSearchPath dllImportSearchPath, Assembly assembly)
        {
            RuntimeAssembly assemblyAsRuntimeAssembly = null;
            if (assembly != null)
            {
                assemblyAsRuntimeAssembly = assembly as RuntimeAssembly;
            }

            // Todo: Determine when it shouldn't be true
            bool searchAssemblyDirectory = true;

            IntPtr hmodule = LoadLibrary(assemblyAsRuntimeAssembly, libraryName, searchAssemblyDirectory, (int)dllImportSearchPath);

            NativeLibrary loadedLibrary = new NativeLibrary(libraryName, hmodule);

            return loadedLibrary;
        }

        // Calls NativeLibrary::LoadLibrary unmanaged methode
        [DllImport(JitHelpers.QCall, CharSet = CharSet.Unicode)]
        internal static extern IntPtr LoadLibrary(RuntimeAssembly assembly, string libraryName, bool searchAssemblyDirectory, int dllImportSearchPathFlag);

        // Todo: optimize for methods that do not have callbacks
        internal static IntPtr LoadLibraryCallback(string libraryName, uint dllImportSearchPathUint, Assembly assembly)
        {
            DllImportSearchPath dllImportSearchPath = (DllImportSearchPath)dllImportSearchPathUint;

            Func<LoadNativeLibraryArgs, NativeLibrary> callback = null;
            AssemblyToCallbackMap.TryGetValue(assembly, out callback);

            if (callback != null)
            {
                LoadNativeLibraryArgs loadNativeLibraryArgs = new LoadNativeLibraryArgs(libraryName, dllImportSearchPath, assembly);
                NativeLibrary nativeLibrary = callback(loadNativeLibraryArgs);
                if(nativeLibrary != null) return nativeLibrary.Handle;
            }

            return IntPtr.Zero;
        }
    }
}
