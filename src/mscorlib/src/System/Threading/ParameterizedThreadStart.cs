// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

//
/*=============================================================================
**
**
**
** Purpose: This class is a Delegate which defines the start method
**  for starting a thread.  That method must match this delegate.
**
**
=============================================================================*/


namespace System.Threading {
    using System.Security.Permissions;
    using System.Threading;
    using System.Runtime.InteropServices;

    [ComVisibleAttribute(false)]
    public delegate void ParameterizedThreadStart(object obj);
}
