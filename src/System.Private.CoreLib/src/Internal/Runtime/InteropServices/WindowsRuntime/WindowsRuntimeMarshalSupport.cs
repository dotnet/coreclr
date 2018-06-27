// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;

namespace Internal.Runtime.InteropServices.WindowsRuntime
{
    public static class WindowsRuntimeMarshalSupport
    {
        /// <summary>
        /// Get EventHandler for specified EventRegistrationToken if it exists.
        /// An internal contract between System.Private.Corelib and System.Runtime.WindowsRuntime
        /// </summary>
        public static T GetEventHandlerFromEventRegistrationToken<T>(EventRegistrationTokenTable<T> table, EventRegistrationToken token)  where T : class
        {
            return table.ExtractHandler(token);
        }
    }
}