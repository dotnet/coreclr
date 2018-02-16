// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.Runtime.InteropServices
{
    // Contains Windows-specific logic for NativeLibrary class.
    public sealed partial class NativeLibrary
    {
        // The allowed mask values for the DllImportSearchPath.
        // The high three bytes are passed as-is to the OS (which will check them for validity),
        // and the low byte is allowed to contain AssemblyDirectory or LegacyBehavior.
        private const uint AllowedDllImportSearchPathsMask = ~0xFFU | (uint)DllImportSearchPath.AssemblyDirectory;

        // [DllImport] (NDirectMethodDesc::FindEntryPoint) allows lookup by ordinal on Windows.
        private const bool AllowLocatingFunctionsByOrdinal = true;

        // from libloaderapi.h
        private const uint GET_MODULE_HANDLE_EX_FLAG_FROM_ADDRESS = 0x00000004;
        private const uint GET_MODULE_HANDLE_EX_FLAG_PIN = 0x00000001;

        // On Windows, the CLR's HINSTANCE is the underlying OS handle.
        private IntPtr OperatingSystemHandle => _hInstance;

        private static bool IsValidModuleHandle(IntPtr hModule)
        {
            // This method has two purposes: (a) it ensures that the provided module handle is indeed valid,
            // and (b) it pins the module so that it can never be unloaded from the current process. The CLR
            // expects modules to be pinned (see the call to BaseHolder<...>::Extract in NDirect::LoadLibraryModule),
            // and we should enforce this invariant regardless of which code path ended up loading the module.

            if (hModule == IntPtr.Zero)
            {
                return false;
            }

            if (!GetModuleHandleEx(
                dwFlags: GET_MODULE_HANDLE_EX_FLAG_FROM_ADDRESS | GET_MODULE_HANDLE_EX_FLAG_PIN,
                lpModuleName: hModule, // module base address, not module name
                phModule: out IntPtr baseAddress))
            {
                return false;
            }

            if (hModule != baseAddress)
            {
                return false;
            }

            // all checks succeeded
            return true;
        }

        // https://msdn.microsoft.com/en-us/library/windows/desktop/ms683200(v=vs.85).aspx
        [DllImport(Interop.Libraries.Kernel32, EntryPoint = "GetModuleHandleExW", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetModuleHandleEx(
            [In] uint dwFlags,
            [In] IntPtr lpModuleName,
            [Out] out IntPtr phModule);
    }
}
