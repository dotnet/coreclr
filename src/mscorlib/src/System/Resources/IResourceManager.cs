// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace System.Resources
{
    /// <summary>
    /// Private interface between CoreLib and ResourceManager so that CoreLib can access resources
    /// </summary>
    [FriendAccessAllowed]
    public interface IResourceManager
    {
        String GetString(String name);
        String GetString(String name, CultureInfo culture);
    }
}
