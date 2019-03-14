// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

/*=============================================================================
**
**
**
** Purpose: The exception class for type loading failures.
**
**
=============================================================================*/

using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace System
{
    public partial class TypeLoadException : SystemException
    {
        private void SetMessageField()
        {
            if (_message == null)
            {
                if (_className == null && _resourceId == 0)
                    _message = SR.Arg_TypeLoadException;
                else
                {
                    if (_assemblyName == null)
                        _assemblyName = SR.IO_UnknownFileName;
                    if (_className == null)
                        _className = SR.IO_UnknownFileName;

                    string format = null;
                    GetTypeLoadExceptionMessage(_resourceId, JitHelpers.GetStringHandleOnStack(ref format));
                    _message = string.Format(format, _className, _assemblyName, _messageArg);
                }
            }
        }

        [DllImport(JitHelpers.QCall, CharSet = CharSet.Unicode)]
        private static extern void GetTypeLoadExceptionMessage(int resourceId, StringHandleOnStack retString);
    }
}
