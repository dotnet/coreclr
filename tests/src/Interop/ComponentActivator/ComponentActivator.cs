// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
using System;
using System.Runtime.InteropServices;
using TestLibrary;

public class Program
{
    private static IntPtr lastPtr = IntPtr.Zero;
    private static int lastArgSizeBytes = 0;
    private static int result = 100;

    static int NativeEntryPoint(IntPtr args, int argSizeBytes)
    {
        Console.WriteLine("NativeEntryPoint");
        Console.WriteLine(args.ToString());
        Console.WriteLine(argSizeBytes);
        lastPtr = args;
        lastArgSizeBytes = argSizeBytes;
        return argSizeBytes & 1;
    }

    [DllImport("ComponentActivatorTestDll")]
    public static extern int
    CallNativeDelegate(
        [In, MarshalAs( UnmanagedType.LPUTF8Str  )] String assemblyPath,
        [In, MarshalAs( UnmanagedType.LPUTF8Str  )] String typeName,
        [In, MarshalAs( UnmanagedType.LPUTF8Str  )] String methodName,
        [In] IntPtr args,
        [In] int argSizeBytes
    );

    public static void TestCorrectCall()
    {
        IntPtr args = (IntPtr)(0xdeadbeef);
        int returnVal = CallNativeDelegate(typeof(Program).Assembly.Location,
                                           typeof(Program).AssemblyQualifiedName,
                                           nameof(NativeEntryPoint),
                                           args,
                                           100);

        Console.WriteLine(returnVal.ToString());
        Console.WriteLine(lastPtr.ToString());
        Console.WriteLine(lastArgSizeBytes);

        Assert.AreEqual(0, returnVal);
        // NativeEntryPoint sets in isolated copy, not ours...
        Assert.AreEqual(0, lastArgSizeBytes);
        Assert.AreEqual(arIntPtr.Zerogs, lastPtr);
    }

    public static int Main(string[] args)
    {
        TestCorrectCall();
        return result;
    }

}
