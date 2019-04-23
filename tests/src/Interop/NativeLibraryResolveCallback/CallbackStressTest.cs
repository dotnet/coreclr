// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
using System;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Threading;

using Console = Internal.Console;

[assembly: DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
public class Test
{
    static volatile bool s_RunGC = true;
    
    static int s_LoopCounter = 100;
    static int s_FinallyCalled = 0;
    static int s_CatchCalled = 0;
    static int s_WrongPInvokesExecuted = 0;
    static int s_PInvokesExecuted = 0;
    
    public static void Collector()
    {
        while(s_RunGC) GC.Collect();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void SetResolve()
    {
        Console.WriteLine("Setting PInvoke Resolver");
        
        DllImportResolver resolver =
            (string libraryName, Assembly asm, DllImportSearchPath? dllImportSearchPath) =>
            {
                if (dllImportSearchPath != DllImportSearchPath.System32)
                {
                    Console.WriteLine($"Unexpected dllImportSearchPath: {dllImportSearchPath.ToString()}");
                    throw new ArgumentException();
                }

                return NativeLibrary.Load("ResolveLib", asm, null);
            };

        NativeLibrary.SetDllImportResolver(
            Assembly.GetExecutingAssembly(), 
            resolver);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void DoCall()
    {
        NativeSum(10, 10);
        s_WrongPInvokesExecuted++;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void DoCallTryFinally()
    {
        try
        {
            NativeSum(10, 10);
            s_WrongPInvokesExecuted++;
        }
        finally { s_FinallyCalled++; }
    }
    
    public static int Main()
    {
        new Thread(Collector).Start();

        for(int i = 0; i < s_LoopCounter; i++)
        {
            try
            {
                NativeSum(10, 10);
                s_WrongPInvokesExecuted++;
            }
            catch (DllNotFoundException) { s_CatchCalled++; }
            
            try { DoCall(); }
            catch (DllNotFoundException) { s_CatchCalled++; }
            
            try { DoCallTryFinally(); }
            catch (DllNotFoundException) { s_CatchCalled++; }
        }
        
        SetResolve();

        for(int i = 0; i < s_LoopCounter; i++)
        {
            var a = NativeSum(10, 10);
            var b = NativeSum(10, 10);
            s_PInvokesExecuted += (a == b && a == 20)? 2 : 0;
        }
        
        s_RunGC = false;

        Console.WriteLine("s_FinallyCalled = " + s_FinallyCalled);
        Console.WriteLine("s_CatchCalled = " + s_CatchCalled);
        Console.WriteLine("s_WrongPInvokesExecuted = " + s_WrongPInvokesExecuted);
        Console.WriteLine("s_PInvokesExecuted = " + s_PInvokesExecuted);
        
        if (s_FinallyCalled == s_LoopCounter && 
            s_CatchCalled == (s_LoopCounter * 3) &&
            s_WrongPInvokesExecuted == 0 &&
            s_PInvokesExecuted == (s_LoopCounter * 2))
        {
            Console.WriteLine("PASS");
            return 100;
        }
            
        return -1;
    }

    [DllImport("NativeLib")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    static extern int NativeSum(int arg1, int arg2);
}
