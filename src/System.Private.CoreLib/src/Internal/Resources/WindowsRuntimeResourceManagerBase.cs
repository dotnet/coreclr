// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Globalization;

namespace Internal.Resources
{
    //
    // This is implemented in System.Runtime.WindowsRuntime as function System.Resources.WindowsRuntimeResourceManager,
    // allowing us to ask for a WinRT-specific ResourceManager.
    // It is important to have WindowsRuntimeResourceManagerBase as regular class with virtual methods and default implementations. 
    // Defining WindowsRuntimeResourceManagerBase as abstract class or interface will cause issues when adding more methods to it 
    // because it'll create dependency between mscorlib and System.Runtime.WindowsRuntime which will require always shipping both DLLs together. 
    //
    public abstract class WindowsRuntimeResourceManagerBase
    {
        public abstract bool Initialize(string libpath, string reswFilename, out PRIExceptionInfo exceptionInfo);

        public abstract string GetString(string stringName, string startingCulture, string neutralResourcesCulture);

        public abstract CultureInfo GlobalResourceContextBestFitCultureInfo
        {
            get;
        }

        public abstract bool SetGlobalResourceContextDefaultCulture(CultureInfo ci);
    }
}
