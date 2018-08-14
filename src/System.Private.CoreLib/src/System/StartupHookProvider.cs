// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using Internal.IO;

namespace System
{
    internal class StartupHookProvider
    {
        private static string initializeMethodName = "Initialize";

        // Parse a string specifying a list of assemblies and types
        // containing a startup hook, and call each hook in turn.
        private static void ProcessStartupHooks(IntPtr startupHooksVariable)
        {
            Debug.Assert(startupHooksVariable != IntPtr.Zero);
            string[] startupHooks = Marshal.PtrToStringUTF8(startupHooksVariable).Split(new char[] { Path.PathSeparator });

            // Parse startup hook variable
            List<Tuple<string, string>> startupHooksParsed = new List<Tuple<string, string>>();
            foreach (string startupHook in startupHooks)
            {
                string[] assemblyPathAndTypeName = startupHook.Split(new char[] { '!' });
                if (assemblyPathAndTypeName.Length != 2)
                {
                    throw new ArgumentException(SR.Argument_InvalidStartupHookSyntax);
                }
                string assemblyPath = assemblyPathAndTypeName[0];
                string typeName = assemblyPathAndTypeName[1];
                if (String.IsNullOrEmpty(assemblyPath) || String.IsNullOrEmpty(typeName))
                {
                    throw new ArgumentException(SR.Argument_InvalidStartupHookSyntax);
                }
                startupHooksParsed.Add(Tuple.Create(assemblyPath, typeName));
            }

            // Ensure the startup dlls exist
            foreach (var startupHook in startupHooksParsed)
            {
                var assemblyPath = startupHook.Item1;
                if (!File.Exists(assemblyPath))
                {
                    throw new FileNotFoundException(SR.Format(SR.FileNotFound_ResolveAssembly, assemblyPath));
                }
            }

            // Call each hook in turn
            foreach (var startupHook in startupHooksParsed)
            {
                CallStartupHook(startupHook.Item1, startupHook.Item2);
            }
        }

        // Load the specified assembly, and call the specified type's
        // "public static void Initialize()" method.
        private static void CallStartupHook(string assemblyPath, string typeName)
        {
            Debug.Assert(!String.IsNullOrEmpty(assemblyPath));
            Debug.Assert(!String.IsNullOrEmpty(typeName));
            Debug.Assert(File.Exists(assemblyPath));

            string fullAssemblyPath = Path.GetFullPath(assemblyPath);
            Assembly assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(fullAssemblyPath);
            Debug.Assert(assembly != null);
            Type type = assembly.GetType(typeName, throwOnError: true);
            MethodInfo initializeMethod = type.GetMethod(initializeMethodName, Type.EmptyTypes);

            if (initializeMethod == null)
            {
                throw new MissingMethodException(typeName, initializeMethodName);
            }

            Debug.Assert(initializeMethod.IsPublic);
            if (!(initializeMethod.IsStatic && initializeMethod.ReturnType == typeof(void)))
            {
                throw new ArgumentException(SR.Format(SR.Argument_InvalidStartupHookSignature, typeName + "." + initializeMethodName));
            }

            initializeMethod.Invoke(null, null);
        }
    }
}
