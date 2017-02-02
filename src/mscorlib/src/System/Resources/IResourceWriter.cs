// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

/*============================================================
**
** 
** 
**
**
** Purpose: Default way to write strings to a COM+ resource 
** file.
**
** 
===========================================================*/
namespace System.Resources {
    using System;
    using System.IO;
    [System.Runtime.InteropServices.ComVisible(true)]
    internal interface IResourceWriter : IDisposable
    {
    }
}
