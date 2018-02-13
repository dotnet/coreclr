// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.Runtime.InteropServices
{
    // Contains UNIX-specific logic for NativeLibrary class.
    public unsafe sealed partial class NativeLibrary
    {
        // [DllImport] (NDirectMethodDesc::FindEntryPoint) doesn't allow lookup by ordinal on non-Windows.
        private const bool AllowLocatingFunctionsByOrdinal = false;

        // Per PInvokeStaticSigInfo::InitCallConv and GetDefaultCallConv, the default calling convention
        // on Unix is cdecl.
        private const CallingConvention FallbackCallingConvention = CallingConvention.Cdecl;

        // In UNIX, the CLR's HINSTANCE is actually a pointer to _MODSTRUCT.
        // We can extract the underlying OS handle from that structure.
        private IntPtr OperatingSystemHandle => ((_MODSTRUCT*)_hInstance)->dl_handle;

        private static bool IsValidModuleHandle(IntPtr hModule)
        {
            // No validation other than checking for null.

            return (hModule != IntPtr.Zero);
        }

        // This is a *partial* copy of include/pal/module.h so that we can extract the dl_handle.
        [StructLayout(LayoutKind.Sequential)]
        private struct _MODSTRUCT
        {
            internal IntPtr self;
            internal IntPtr dl_handle;
        }
    }
}
