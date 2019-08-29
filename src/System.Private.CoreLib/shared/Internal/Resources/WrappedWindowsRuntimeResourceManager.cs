// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Globalization;

namespace Internal.Resources
{
    // This implementation wraps the IWindowsRuntimeResourceManager implementation in System.Runtime.WindowsRuntime to remove our need to
    // have System.Runtime.WindowsRuntime not have to reference System.Private.CoreLib.
    internal class WrappedWindowsRuntimeResourceManager : IWindowsRuntimeResourceManager
    {
        private delegate bool InitializeDelegate(string libpath, string reswFilename, out string? packageSimpleName, out string? encodedResWFilename);

        private InitializeDelegate _initialize;
        private Func<string, string?, string?, string> _getString;
        private Func<CultureInfo?> _get_GlobalResourceContextBestFitCultureInfo;
        private Func<CultureInfo, bool> _setGlobalResourceContextDefaultCulture;

        public WrappedWindowsRuntimeResourceManager(object implementation)
        {
            Type implementationType = implementation.GetType();
            _initialize = (InitializeDelegate)implementationType.GetMethod(nameof(Initialize))!.CreateDelegate(typeof(InitializeDelegate), implementation);
            _getString = (Func<string, string?, string?, string>)implementationType.GetMethod(nameof(GetString))!.CreateDelegate(typeof(Func<string, string?, string?, string>), implementation);
            _get_GlobalResourceContextBestFitCultureInfo = (Func<CultureInfo?>)implementationType.GetProperty(nameof(GlobalResourceContextBestFitCultureInfo))!.GetMethod!.CreateDelegate(typeof(Func<CultureInfo?>), implementation);
            _setGlobalResourceContextDefaultCulture = (Func<CultureInfo, bool>)implementationType.GetMethod(nameof(SetGlobalResourceContextDefaultCulture))!.CreateDelegate(typeof(Func<CultureInfo, bool>), implementation);
        }

        public bool Initialize(string libpath, string reswFilename, out string? packageSimpleName, out string? encodedResWFilename)
        {
            return _initialize(libpath, reswFilename, out packageSimpleName, out encodedResWFilename);
        }

        public string GetString(string stringName, string? startingCulture, string? neutralResourcesCulture)
        {
            return _getString(stringName, startingCulture, neutralResourcesCulture);
        }

        public CultureInfo? GlobalResourceContextBestFitCultureInfo => _get_GlobalResourceContextBestFitCultureInfo();

        public bool SetGlobalResourceContextDefaultCulture(CultureInfo ci) => _setGlobalResourceContextDefaultCulture(ci);
    }
}
