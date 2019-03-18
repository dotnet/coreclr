// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.Loader;

//
// Types in this file marked as 'public' are done so only to aid in
// testing of functionality and should not be considered publicly consumable.
//
namespace Internal.Runtime.InteropServices.WindowsRuntime
{
    public static class ActivationFactoryLoader
    {
        public static void GetActivationFactory(
            [MarshalAs(UnmanagedType.HString)] string typeName,
            [MarshalAs(UnmanagedType.Interface)] out IActivationFactory activationFactory)
        {
            activationFactory = null;
            if (typeName is null)
            {
                throw new ArgumentNullException(nameof(typeName));
            }
            
            Type winRTType = System.StubHelpers.WinRTTypeNameConverter.GetTypeFromWinRTTypeName(typeName, out bool _);

            activationFactory = new ManagedActivationFactory(winRTType);
        }
    }
}
