// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

/*=============================================================================
**
**
** 
** 
**
**
** Purpose: For Assembly-related stuff.
**
**
=============================================================================*/

namespace System.Reflection
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using CultureInfo = System.Globalization.CultureInfo;
    using System.Security;
    using System.Security.Policy;
    using System.IO;
    using StringBuilder = System.Text.StringBuilder;
    using System.Configuration.Assemblies;
    using StackCrawlMark = System.Threading.StackCrawlMark;
    using System.Runtime.InteropServices;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;
    using Microsoft.Win32;
    using System.Threading;
    using __HResults = System.__HResults;
    using System.Runtime.Versioning;
    using System.Diagnostics.Contracts;
    using System.Runtime.Loader;

    [Serializable]
    public delegate Module ModuleResolveEventHandler(Object sender, ResolveEventArgs e);
}
