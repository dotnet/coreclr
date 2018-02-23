// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.Runtime.InteropServices
{
    // Contains UNIX-specific logic for NativeLibrary class.
    public unsafe sealed partial class NativeLibrary
    {
        // The allowed mask values for the DllImportSearchPath.
        // Non-Windows sytems only allow AssemblyDirectory and LegacyBehavior.
        private const uint AllowedDllImportSearchPathsMask = (uint)(DllImportSearchPath.AssemblyDirectory | DllImportSearchPath.LegacyBehavior);

        // [DllImport] (NDirectMethodDesc::FindEntryPoint) doesn't allow lookup by ordinal on non-Windows.
        private const bool AllowLocatingFunctionsByOrdinal = false;

        private static bool IsValidModuleHandle(IntPtr hModule)
        {
            // No validation other than checking for null.

            return (hModule != IntPtr.Zero);
        }
    }
}
