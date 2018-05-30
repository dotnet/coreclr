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
    [Serializable]
    [System.Runtime.CompilerServices.TypeForwardedFrom("mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")]
    public partial class MissingFieldException : MissingMemberException
    {
        public MissingFieldException()
            : base(SR.Arg_MissingFieldException)
        {
            HResult = HResults.COR_E_MISSINGFIELD;
        }

        public MissingFieldException(String message)
            : base(message)
        {
            HResult = HResults.COR_E_MISSINGFIELD;
        }

        public MissingFieldException(String message, Exception inner)
            : base(message, inner)
        {
            HResult = HResults.COR_E_MISSINGFIELD;
        }

        public MissingFieldException(string className, string fieldName)
        {
            ClassName = className;
            MemberName = fieldName;
        }

        protected MissingFieldException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
