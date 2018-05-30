// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

/*=============================================================================
**
**
** Purpose: The exception class for class loading failures.
**
=============================================================================*/


using System;
using System.Runtime.Serialization;
using System.Runtime.CompilerServices;
using System.Globalization;

namespace System
{
    public partial class MissingFieldException : MissingMemberException, ISerializable
    {
        public override string Message
        {
            get
            {
                if (ClassName == null)
                {
                    return base.Message;
                }
                else
                {
                    // do any desired fixups to classname here.
                    return SR.Format(SR.MissingField_Name, (Signature != null ? FormatSignature(Signature) + " " : "") + ClassName + "." + MemberName);
                }
            }
        }
    }
}
