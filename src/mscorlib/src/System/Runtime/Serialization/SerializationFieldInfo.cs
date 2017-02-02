// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

/*============================================================
**
**
**
** Purpose: Provides a methods of representing imaginary fields
** which are unique to serialization.  In this case, what we're
** representing is the private members of parent classes.  We
** aggregate the RuntimeFieldInfo associated with this member 
** and return a managled form of the name.  The name that we
** return is .parentname.fieldname
**
**
============================================================*/

namespace System.Runtime.Serialization {

    using System;
    using System.Reflection;
    using System.Globalization;
    using System.Diagnostics;
    using System.Diagnostics.Contracts;
    using System.Threading;
}
