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


namespace System.Runtime.Loader
{
    public abstract class LoadNative
    {
        [DllImport(JitHelpers.QCall, CharSet = CharSet.Unicode)]
        internal static extern IntPtr LoadLibrary(IntPtr pAssembly, string libraryName, bool searchAssemblyDirectory, ulong dllImportSearchPathFlag);
    }
}
