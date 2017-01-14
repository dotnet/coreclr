// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.Resources
{
    using System;
    using System.IO;
    using System.Globalization;
    using System.Collections;
    using System.Text;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Security.Permissions;
    using System.Threading;
    using System.Runtime.InteropServices;
    using System.Runtime.CompilerServices;
    using Microsoft.Win32;
    using System.Collections.Generic;
    using System.Runtime.Versioning;
    using System.Diagnostics;
    using System.Diagnostics.Contracts;

#if FEATURE_APPX
    //
    // This is implemented in System.Runtime.WindowsRuntime as function System.Resources.WindowsRuntimeResourceManager,
    // allowing us to ask for a WinRT-specific ResourceManager.
    // It is important to have WindowsRuntimeResourceManagerBase as regular class with virtual methods and default implementations. 
    // Defining WindowsRuntimeResourceManagerBase as abstract class or interface will cause issues when adding more methods to it 
    // because it’ll create dependency between mscorlib and System.Runtime.WindowsRuntime which will require always shipping both DLLs together. 
    // Also using interface or abstract class will not play nice with FriendAccessAllowed.
    //
    [FriendAccessAllowed]
    internal class WindowsRuntimeResourceManagerBase
    {
        public virtual bool Initialize(string libpath, string reswFilename, out PRIExceptionInfo exceptionInfo) { exceptionInfo = null; return false; }

        public virtual String GetString(String stringName, String startingCulture, String neutralResourcesCulture) { return null; }

        public virtual CultureInfo GlobalResourceContextBestFitCultureInfo
        {
            get { return null; }
        }

        public virtual bool SetGlobalResourceContextDefaultCulture(CultureInfo ci) { return false; }
    }

    [FriendAccessAllowed]
    internal class PRIExceptionInfo
    {
        public string _PackageSimpleName;
        public string _ResWFile;
    }
#endif // FEATURE_APPX
}