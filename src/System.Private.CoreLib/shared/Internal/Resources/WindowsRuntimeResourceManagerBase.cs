// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Globalization;

namespace Internal.Resources
{
    // This implementation wraps the WindowsRuntimeResourceManager implementation in ProjectN to adapt it to the new IWindowsRuntimeResourceManager interface
    // without having to make any changes.
    public abstract class WindowsRuntimeResourceManagerBase : IWindowsRuntimeResourceManager
    {
        bool IWindowsRuntimeResourceManager.Initialize(string libpath, string reswFilename, out string? packageSimpleName, out string? encodedResWFilename)
        {
            bool result = Initialize(libpath, reswFilename, out PRIExceptionInfo? exceptionInfo);
            if (exceptionInfo is null)
            {
                packageSimpleName = null;
                encodedResWFilename = null;
            }
            else
            {
                packageSimpleName = exceptionInfo.PackageSimpleName;
                encodedResWFilename = exceptionInfo.ResWFile;
            }
            return result;
        }

        public abstract bool Initialize(string libpath, string reswFilename, out PRIExceptionInfo? exceptionInfo);

        public abstract string GetString(string stringName, string? startingCulture, string? neutralResourcesCulture);

        public abstract CultureInfo? GlobalResourceContextBestFitCultureInfo { get; }

        public abstract bool SetGlobalResourceContextDefaultCulture(CultureInfo ci);
    }
}
