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
        // Collection of all ALCs used for WinRT activation.
        private static Dictionary<string, AssemblyLoadContext> s_AssemblyLoadContexts = new Dictionary<string, AssemblyLoadContext>(StringComparer.InvariantCultureIgnoreCase);
        
        private static AssemblyLoadContext GetALC(string assemblyPath)
        {
            AssemblyLoadContext alc;

            lock (s_AssemblyLoadContexts)
            {
                if (!s_AssemblyLoadContexts.TryGetValue(assemblyPath, out alc))
                {
                    alc = new IsolatedComponentLoadContext(assemblyPath);
                    s_AssemblyLoadContexts.Add(assemblyPath, alc);
                }
            }

            return alc;
        }

        public static int GetActivationFactory(
            IntPtr componentPath,
            [MarshalAs(UnmanagedType.HString)] string typeName,
            [MarshalAs(UnmanagedType.Interface)] out IActivationFactory activationFactory)
        {
            activationFactory = null;
            try
            {
                if (typeName is null)
                {
                    throw new ArgumentNullException(nameof(typeName));
                }

                AssemblyLoadContext context = GetALC(Marshal.PtrToStringUni(componentPath));
                
                Type winRTType = context.LoadTypeForWinRTTypeNameInContext(typeName);

                if (winRTType is null)
                {
                    throw new TypeLoadException(typeName);
                }
                activationFactory = WindowsRuntimeMarshal.GetManagedActivationFactory(winRTType);
            }
            catch (System.Exception ex)
            {
                return ex.HResult;
            }
            return 0;
        }
    }
}
