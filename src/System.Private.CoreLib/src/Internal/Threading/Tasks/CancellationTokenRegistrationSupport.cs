// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Threading;

namespace Internal.Threading.Tasks
{
    public static class CancellationTokenRegistrationSupport
    {
        /// <summary>
        /// Disposes of the registration and unregisters the target callback from the associated CancellationToken
        /// This method is used by AsyncInfoToTaskBridge in System.Runtime.WindowsRuntime
        /// </summary>
        public static bool Unregister(CancellationTokenRegistration registration)
        {
            return registration.Unregister();
        }
    }
}

