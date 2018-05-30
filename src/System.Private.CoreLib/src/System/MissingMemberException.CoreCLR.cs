// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

/*=============================================================================
**
**
**
** Purpose: The exception class for versioning problems with DLLS.
**
**
=============================================================================*/

using System.Runtime.Serialization;
using System.Runtime.CompilerServices;

namespace System
{
    public partial class MissingMemberException : MemberAccessException, ISerializable
    {
        // Called to format signature
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern string FormatSignature(byte[] signature);
    }
}
