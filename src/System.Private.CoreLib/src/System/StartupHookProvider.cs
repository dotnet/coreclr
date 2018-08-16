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
            var startupHooksParsed = new List<(string AssemblyPath, string TypeName)>();
            foreach (string startupHook in startupHooks)
            {
                int separatorIndex = startupHook.LastIndexOf('!');
                if (separatorIndex <= 0 || separatorIndex == startupHook.Length - 1)
                {
                    throw new ArgumentException(SR.Argument_InvalidStartupHookSyntax);
                }
                string assemblyPath = startupHook.Substring(0, separatorIndex);
                string typeName = startupHook.Substring(separatorIndex + 1);
                startupHooksParsed.Add((AssemblyPath: assemblyPath, TypeName: typeName));
            }

            // Ensure the startup dlls exist
            foreach (var startupHook in startupHooksParsed)
            {
                var assemblyPath = startupHook.AssemblyPath;
                if (!File.Exists(assemblyPath))
                {
                    throw new FileNotFoundException(SR.Format(SR.FileNotFound_ResolveAssembly, Path.GetFullPath(assemblyPath)));
                }
            }

            // Call each hook in turn
            foreach (var startupHook in startupHooksParsed)
            {
                CallStartupHook(startupHook.AssemblyPath, startupHook.TypeName);
            }
        }

        // Load the specified assembly, and call the specified type's
        // "public static void Initialize()" method.
        private static void CallStartupHook(string assemblyPath, string typeName)
        {
            Debug.Assert(!String.IsNullOrEmpty(assemblyPath));
            Debug.Assert(!String.IsNullOrEmpty(typeName));

            string fullAssemblyPath = Path.GetFullPath(assemblyPath);
            Assembly assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(fullAssemblyPath);
            Debug.Assert(assembly != null);
            Type type = assembly.GetType(typeName, throwOnError: true);

            // Look for a public static method without any parameters
            MethodInfo initializeMethod = type.GetMethod(initializeMethodName,
                                                         BindingFlags.Public | BindingFlags.Static,
                                                         null, // use default binder
                                                         Type.EmptyTypes, // parameters
                                                         null); // no parameter modifiers

            bool wrongSignature = false;
            if (initializeMethod == null)
            {
                // There weren't any public static methods without
                // parameters. Look for any methods with the correct
                // name, to provide precise error handling.
                try
                {
                    // This could find zero, one, or multiple methods
                    // with the correct name.
                    initializeMethod = type.GetMethod(initializeMethodName,
                                                      BindingFlags.Public | BindingFlags.NonPublic |
                                                      BindingFlags.Static | BindingFlags.Instance);
                }
                catch (AmbiguousMatchException)
                {
                    // Found multiple
                    Debug.Assert(initializeMethod == null);
                    wrongSignature = true;
                }
                if (initializeMethod != null)
                {
                    // Found one
                    wrongSignature = true;
                }
                else
                {
                    // Didn't find any
                    throw new MissingMethodException(typeName, initializeMethodName);
                }
            }
            else if (initializeMethod.ReturnType != typeof(void))
            {
                wrongSignature = true;
            }

            if (wrongSignature)
            {
                throw new ArgumentException(SR.Format(SR.Argument_InvalidStartupHookSignature,
                                                      typeName + Type.Delimiter + initializeMethodName));
            }

            Debug.Assert(initializeMethod != null &&
                         initializeMethod.IsPublic &&
                         initializeMethod.IsStatic &&
                         initializeMethod.ReturnType == typeof(void) &&
                         initializeMethod.GetParameters().Length == 0);

            initializeMethod.Invoke(null, null);
        }
    }
}
