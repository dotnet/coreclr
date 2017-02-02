// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

// 

//
// A HostSecurityManager gives a hosting application the chance to 
// participate in the security decisions in the AppDomain.
//

namespace System.Security
{
    using System.Collections;
    using System.Reflection;
    using System.Security;
    using System.Security.Permissions;
    using System.Security.Policy;
    using System.Runtime.Versioning;
    using System.Diagnostics.Contracts;
}
