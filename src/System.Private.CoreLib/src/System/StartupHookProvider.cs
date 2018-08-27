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
        private const string startupHookTypeName = "StartupHook";
        private const string initializeMethodName = "Initialize";

        // Parse a string specifying a list of assemblies and types
        // containing a startup hook, and call each hook in turn.
        private static void ProcessStartupHooks()
        {
            string startupHooksVariable = (string)AppContext.GetData("STARTUP_HOOKS");
            if (startupHooksVariable == null)
            {
                return;
            }

            // Parse startup hooks variable
            string[] startupHooks = startupHooksVariable.Split(Path.PathSeparator);
            foreach (string startupHook in startupHooks)
            {
                if (String.IsNullOrEmpty(startupHook))
                {
                    throw new ArgumentException(SR.Argument_InvalidStartupHookSyntax);
                }
                if (PathInternal.IsPartiallyQualified(startupHook))
                {
                    throw new ArgumentException(SR.Argument_AbsolutePathRequired);
                }
            }

            // Call each hook in turn
            foreach (string startupHook in startupHooks)
            {
                CallStartupHook(startupHook);
            }
        }

        // Load the specified assembly, and call the specified type's
        // "public static void Initialize()" method.
        private static void CallStartupHook(string assemblyPath)
        {
            Debug.Assert(!String.IsNullOrEmpty(assemblyPath));
            Debug.Assert(!PathInternal.IsPartiallyQualified(assemblyPath));

            Assembly assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyPath);
            Debug.Assert(assembly != null);
            Type type = assembly.GetType(startupHookTypeName, throwOnError: true);

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
                    throw new MissingMethodException(startupHookTypeName, initializeMethodName);
                }
            }
            else if (initializeMethod.ReturnType != typeof(void))
            {
                wrongSignature = true;
            }

            if (wrongSignature)
            {
                throw new ArgumentException(SR.Format(SR.Argument_InvalidStartupHookSignature,
                                                      startupHookTypeName + Type.Delimiter + initializeMethodName,
                                                      assemblyPath));
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
