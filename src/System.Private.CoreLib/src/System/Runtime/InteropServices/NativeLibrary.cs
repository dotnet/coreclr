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
        // Maintains a map of assemblies and their callbacks.
        // Simulates an additional field of Assembly (which may be added in the future).
        public static ConditionalWeakTable<Assembly, Func<LoadNativeLibraryArgs, NativeLibrary>> assemblyToCallbackMap;

        public NativeLibrary(string libraryName, IntPtr handle)
        {
            Name = libraryName;
            Handle = handle;
            assemblyToCallbackMap = new ConditionalWeakTable<Assembly, Func<LoadNativeLibraryArgs, NativeLibrary>>();
        }

        // It is called by a user per assembly: RegisterNativeLibraryLoadCallback(assembly, customMappingCallback)
        // triggered by an event on AppDomain
        public static Func<LoadNativeLibraryArgs, NativeLibrary> RegisterNativeLibraryLoadCallback(Assembly assembly, Func<LoadNativeLibraryArgs, NativeLibrary> callback)
        {
            Func<LoadNativeLibraryArgs, NativeLibrary> registeredCallback = null;
            bool callbackAlreadyRegistered = assemblyToCallbackMap.TryGetValue(assembly, out registeredCallback);

            if (!callbackAlreadyRegistered)
            {
                assemblyToCallbackMap.Add(assembly, callback);
            }

            return callback;
        }

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

            // Todo: how to determine searchAssemblyDirectory? Should it be removed from here?
            bool searchAssemblyDirectory;
            if (dllImportSearchPath != null) searchAssemblyDirectory = true;
            else searchAssemblyDirectory = false;

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

            // Todo: move it from here
            if (assemblyToCallbackMap == null) assemblyToCallbackMap = new ConditionalWeakTable<Assembly, Func<LoadNativeLibraryArgs, NativeLibrary>>();

            assemblyToCallbackMap.Add(assembly, MonoCallbackHandler);
            Func<LoadNativeLibraryArgs, NativeLibrary> callback = null;
            assemblyToCallbackMap.TryGetValue(assembly, out callback);

            if (callback != null)
            {
                LoadNativeLibraryArgs loadNativeLibraryArgs = new LoadNativeLibraryArgs(libraryName, dllImportSearchPath, assembly);
                NativeLibrary nativeLibrary = callback(loadNativeLibraryArgs);
                return nativeLibrary.Handle;
            }

            return IntPtr.Zero;
        }

        public static Func<LoadNativeLibraryArgs, NativeLibrary> MonoCallbackHandler = MonoCallbackHandlerLogic;

        // This needs to be moved from here.
        public static NativeLibrary MonoCallbackHandlerLogic (LoadNativeLibraryArgs args)
        {
            string libraryName = args.LibraryName;
            DllImportSearchPath dllImportSearchPath = args.DllImportSearchPath;
            Assembly assembly = args.CallingAssembly;

            if (libraryName == "WrongLibraryName")
            {
                libraryName = "Library";
                NativeLibrary nativeLibrary = Load(libraryName, dllImportSearchPath, assembly);
                return nativeLibrary;
            }
            return new NativeLibrary("LibraryNotFound",IntPtr.Zero);
        }
    }
}
