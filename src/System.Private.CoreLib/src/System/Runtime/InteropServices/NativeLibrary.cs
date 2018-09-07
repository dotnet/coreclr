// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Reflection;
using System.Runtime.CompilerServices;

namespace System.Runtime.InteropServices
{
    // ToString() must return debugging string that includes source-name, target name that was loaded, and location details.
    public sealed class NativeLibrary
    {
        public IntPtr Handle { get; private set; }
        public string Name { get; private set; } // Will Return the name referenced by the importing assembly

        // Stores a map of assemblies and on-library-load-callbacks assigned to them.
        // Simulates an additional field of Assembly (which may be added in the future).
        private static ConditionalWeakTable<Assembly, Func<LoadNativeLibraryArgs, NativeLibrary>> _assemblyToCallbackMap;

        public NativeLibrary(string libraryName, IntPtr handle)
        {
            Name = libraryName ?? throw new ArgumentException("Name of a NativeLibrary can't be set to null.");
            if(handle == IntPtr.Zero)
                throw new ArgumentException("Handle of a NativeLibrary can't be set to IntPtr.Zero.");
            Handle = handle;
        }

        static NativeLibrary()
        {
            _assemblyToCallbackMap = new ConditionalWeakTable<Assembly, Func<LoadNativeLibraryArgs, NativeLibrary>>();
        }

        /// <exception cref="System.Runtime.InteropServices.CallbackAlreadyRegistered">Thrown when there is already a callback registered for the specified assembly.</exception>
        public static void RegisterNativeLibraryLoadCallback(Assembly assembly, Func<LoadNativeLibraryArgs, NativeLibrary> callback)
        {

            if (_assemblyToCallbackMap.TryGetValue(assembly, out Func<LoadNativeLibraryArgs, NativeLibrary> previousCallback))
            {
                throw new CallbackAlreadyRegisteredException("Callback for " + assembly.GetName().Name + " has already been registered.");
            }
            else
            {
                _assemblyToCallbackMap.Add(assembly, callback);
            }
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
                assemblyAsRuntimeAssembly = (RuntimeAssembly)assembly;
            }

            IntPtr hmodule = LoadLibrary(assemblyAsRuntimeAssembly, libraryName, (int)dllImportSearchPath);

            NativeLibrary loadedLibrary = new NativeLibrary(libraryName, hmodule);

            return loadedLibrary;
        }

        // Calls NativeLibrary::LoadLibrary unmanaged methode
        [DllImport(JitHelpers.QCall, CharSet = CharSet.Unicode)]
        internal static extern IntPtr LoadLibrary(RuntimeAssembly assembly, string libraryName, int dllImportSearchPathFlag);

        // Todo: optimize for methods that do not have callbacks
        internal static IntPtr LoadLibraryCallback(string libraryName, uint dllImportSearchPathUint, Assembly assembly)
        {
            DllImportSearchPath dllImportSearchPath = (DllImportSearchPath)dllImportSearchPathUint;

            bool callbackFound = _assemblyToCallbackMap.TryGetValue(assembly, out Func<LoadNativeLibraryArgs, NativeLibrary> callback);

            if (callbackFound)
            {
                LoadNativeLibraryArgs loadNativeLibraryArgs = new LoadNativeLibraryArgs(libraryName, dllImportSearchPath, assembly);
                NativeLibrary nativeLibrary = callback(loadNativeLibraryArgs);
                if(nativeLibrary != null) return nativeLibrary.Handle;
            }

            return IntPtr.Zero;
        }
    }
}
