// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

/*=============================================================================
**
**
** Purpose: The exception class for class loading failures.
**
=============================================================================*/

using System.Runtime.Serialization;

namespace System
{
    public partial class MissingFieldException : MissingMemberException, ISerializable
    {
        public override string Message
        {
            get
            {
                // do any desired fixups to classname here.
                return ClassName == null ? base.Message : SR.Format(SR.MissingField_Name, (Signature != null ? FormatSignature(Signature) + " " : "") + ClassName + "." + MemberName);
            }
        }
    }
}
