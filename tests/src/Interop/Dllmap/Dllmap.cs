// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Text;
using System.Security;
using System.Runtime.InteropServices;
using System.Runtime;
using System.Runtime.Loader;
using System.IO;
using System.Reflection;

using Console = Internal.Console;

public class Dllmap
{
    private static int s_failures = 0;

    [DllImport("WrongButMappedLibraryName", EntryPoint = "Sum")]
    public extern static int Sum(int a, int b);

    [DllImport("WrongButMappedLibraryName.dll", EntryPoint = "Multiply")]
    public extern static int Multiply(int a, int b);

    [DllImport("WrongButMappedLibraryName", EntryPoint = "Substract")]
    public extern static int Substract(int a, int b);

    [DllImport("WrongLibraryNameWrongMapping", EntryPoint = "Divide")]
    public extern static int Divide(int a, int b);

    [DllImport("LibraryNoMapping", EntryPoint = "Sum")]
    public extern static int SumNoMapping(int a, int b);

    // Registering the first callback for the assembly
    private static void ExpectSuccessnOnRegistering()
    {
        Assembly callingAssembly = Assembly.GetCallingAssembly();
        try
        {
            NativeLibrary.RegisterNativeLibraryLoadCallback(callingAssembly, TestCallbackHandler);
        }
        catch (Exception e)
        {
            Console.WriteLine("Registering a callback for an assembly throws unexpected exception: " + e.ToString());
            s_failures++;
        }
    }

    // Registering the second callback for the same assembly should fail
    private static void ExpectExceptionOnRegistering()
    {
        Assembly callingAssembly = Assembly.GetCallingAssembly();

        try
        {
            NativeLibrary.RegisterNativeLibraryLoadCallback(callingAssembly, TestCallbackHandler);
            Console.WriteLine("Registering a second callback for the same assembly should fail.");
            s_failures++;
        }
        catch (Exception e)
        {
            if (!(e is InvalidOperationException))
            {
                Console.WriteLine("Unexpected exception: " + e.ToString());
                s_failures++;
            }
        }
    }

    private static void ExpectSuccessPlainName()
    {
        try
        {
            if (5 != Sum(2, 3))
            {
                Console.WriteLine("Dll returns incorrect result for Sum!");
                s_failures++;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("Dll throws unexpected exception: " + e.ToString());
            s_failures++;
        }
    }

    private static void ExpectSuccessNameWithExtension()
    {
        try
        {
            if (6 != Multiply(2, 3))
            {
                Console.WriteLine("Dll returns incorrectly result for Multiply!");
                s_failures++;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("Dll throws unexpected exception: " + e.ToString());
            s_failures++;
        }
    }

    // Dll mapping defined corectely but the entrypoint doesn't exist
    private static void ExpectEntryPointNotFound()
    {
        try
        {
            int result = Substract(2, 3);
            s_failures++;
        }
        catch (Exception e)
        {
            if (!(e is EntryPointNotFoundException))
            {
                Console.WriteLine("Calling an unexisting entrypint in mapped library throws unexpected exception: " + e.ToString());
                s_failures++;
            }
        }
    }

    // Target dll defined in mapping doesn't exist
    private static void ExpectDllNotFound()
    {
        try
        {
            int result = Divide(6, 3);
            s_failures++;
        }
        catch (Exception e)
        {
            if (!(e is DllNotFoundException))
            {
                Console.WriteLine("Load attempt of an unexisting library throws unexpected exception: " + e.ToString());
                s_failures++;
            }
        }
    }

    // There is a callback registered for the assembly but the dll doesn't require mapping 
    private static void ExpectSuccessNoMapping()
    {
        try
        {
            if (5 != SumNoMapping(2, 3))
            {
                Console.WriteLine("Dll returns incorrect result for Sum!");
                s_failures++;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("Dll throws unexpected exception: " + e.ToString());
            s_failures++;
        }
    }

    public static Func<LoadNativeLibraryArgs, NativeLibrary> TestCallbackHandler = TestCallbackHandlerLogic;

    public static NativeLibrary TestCallbackHandlerLogic(LoadNativeLibraryArgs args)
    {
        string libraryName = args.LibraryName;
        DllImportSearchPath dllImportSearchPath = args.DllImportSearchPath;
        Assembly assembly = args.CallingAssembly;

        if (libraryName == "WrongButMappedLibraryName")
        {
            libraryName = "Library";
            NativeLibrary nativeLibrary = NativeLibrary.Load(libraryName, dllImportSearchPath, assembly);
            return nativeLibrary;
        }
        else if (libraryName == "WrongButMappedLibraryName.dll")
        {
            libraryName = "Library";
            NativeLibrary nativeLibrary = NativeLibrary.Load(libraryName, dllImportSearchPath, assembly);
            return nativeLibrary;
        }
        else if (libraryName == "WrongLibraryNameWrongMapping")
        {
            libraryName = "MissingTargetLibrary";
            NativeLibrary nativeLibrary = NativeLibrary.Load(libraryName, dllImportSearchPath, assembly);
            return nativeLibrary;
        }

        return null;
    }


    public static int Main()
    {
        ExpectSuccessnOnRegistering();
        ExpectExceptionOnRegistering();
        ExpectSuccessPlainName();
        ExpectSuccessNameWithExtension();
        ExpectEntryPointNotFound();
        ExpectDllNotFound();
        ExpectSuccessNoMapping();

        if (s_failures > 0)
        {
            Console.WriteLine("Failed! Failures: " + s_failures);
            return 101;
        }
        else
        {
            Console.WriteLine("Succeed!");
            return 100;
        }
    }
}
