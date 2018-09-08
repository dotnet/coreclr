// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Reflection;

namespace System.Runtime.InteropServices
{
    public class LoadNativeLibraryArgs : EventArgs
    {
        public LoadNativeLibraryArgs(string libraryName, DllImportSearchPath dllImportSearchPath, Assembly callingAssembly)
        {
            LibraryName = libraryName;
            DllImportSearchPath = dllImportSearchPath;
            CallingAssembly = callingAssembly;
        }

        public string LibraryName { get; private set; }
        public DllImportSearchPath DllImportSearchPath { get; private set; }
        public Assembly CallingAssembly { get; private set; }
    }
}
