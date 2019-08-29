// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Globalization;

namespace Internal.Resources
{
    // This is implemented in System.Runtime.WindowsRuntime as System.Resources.WindowsRuntimeResourceManager,
    // allowing us to ask for a WinRT-specific ResourceManager.
    internal interface IWindowsRuntimeResourceManager
    {
        bool Initialize(string libpath, string reswFilename, out string? packageSimpleName, out string? encodedResWFilename);

        string GetString(string stringName, string? startingCulture, string? neutralResourcesCulture);

        CultureInfo? GlobalResourceContextBestFitCultureInfo { get; }

        bool SetGlobalResourceContextDefaultCulture(CultureInfo ci);
    }
}
