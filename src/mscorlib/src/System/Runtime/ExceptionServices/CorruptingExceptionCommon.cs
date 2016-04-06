// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
#if FEATURE_CORRUPTING_EXCEPTIONS

/*=============================================================================
**
**
**
** Purpose: Contains common usage support entities for Corrupting Exceptions
**
** Created: 06/20/2008
** 
** 
** 
=============================================================================*/

namespace System.Runtime.ExceptionServices {
    using System;
    
    // This attribute can be applied to methods to indicate that ProcessCorruptedState
    // Exceptions should be delivered to them.
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public sealed class HandleProcessCorruptedStateExceptionsAttribute : Attribute
    {
        public HandleProcessCorruptedStateExceptionsAttribute()
        {
        }
    }
}
#endif // FEATURE_CORRUPTING_EXCEPTIONS
