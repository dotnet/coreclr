// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Threading;
using System.Text;
using System.Runtime.InteropServices;
using System.Reflection;

struct A {
    public int field_int;
    public char field_char;
}

struct Mix
{
    [MarshalAs(UnmanagedType.AsAny)]
    public object intArray;
}

class AsAnyTests
{
    static int testFailures = 0;

    // sbyte, byte, short, ushort, int, 
    // uint, long, ulong, single, double, 
    // char, bool, IntPtr, UIntPtr
    #region PInvoke functions for the Primitive type arrays
        
    // Sbyte
    [DllImport("MarshalObjectsAsAnyDLL.dll", CallingConvention = CallingConvention.StdCall)]
    public static extern bool PassArraySbyte(
        [MarshalAs(UnmanagedType.AsAny)] object sbyteArray,
        [In, MarshalAs(UnmanagedType.AsAny)] object sbyteArray_In,
        [In, Out, MarshalAs(UnmanagedType.AsAny)] object sbyteArray_InOut,
        [Out, MarshalAs(UnmanagedType.AsAny)] object sbyteArray_Out,
        int len
    );

    //Byte
    [DllImport("MarshalObjectsAsAnyDLL.dll", CallingConvention = CallingConvention.StdCall)]
    public static extern bool PassArrayByte(
        [MarshalAs(UnmanagedType.AsAny)] object byteArray,
        [In, MarshalAs(UnmanagedType.AsAny)] object byteArray_In,
        [In, Out, MarshalAs(UnmanagedType.AsAny)] object byteArray_InOut,
        [Out, MarshalAs(UnmanagedType.AsAny)] object byteArray_Out,
        int len
    );

    //Short
    [DllImport("MarshalObjectsAsAnyDLL.dll", CallingConvention = CallingConvention.StdCall)]
    public static extern bool PassArrayShort(
        [MarshalAs(UnmanagedType.AsAny)] object shortArray,
        [In, MarshalAs(UnmanagedType.AsAny)] object shortArray_In,
        [In, Out, MarshalAs(UnmanagedType.AsAny)] object shortArray_InOut,
        [Out, MarshalAs(UnmanagedType.AsAny)] object shortArray_Out,
        int len
    );

    [DllImport("MarshalObjectsAsAnyDLL.dll", CallingConvention = CallingConvention.StdCall)]
    public static extern bool PassArrayUshort(
        [MarshalAs(UnmanagedType.AsAny)] object ushortArray,
        [In, MarshalAs(UnmanagedType.AsAny)] object ushortArray_In,
        [In, Out, MarshalAs(UnmanagedType.AsAny)] object ushortArray_InOut,
        [Out, MarshalAs(UnmanagedType.AsAny)] object ushortArray_Out,
        int len
    );

    [DllImport("MarshalObjectsAsAnyDLL.dll", CallingConvention = CallingConvention.StdCall)]
    public static extern bool PassArrayInt(
        [MarshalAs(UnmanagedType.AsAny)] object intArray,
        [In, MarshalAs(UnmanagedType.AsAny)] object intArray_In,
        [In, Out, MarshalAs(UnmanagedType.AsAny)] object intArray_InOut,
        [Out, MarshalAs(UnmanagedType.AsAny)] object intArray_Out,
        int len
    );

    [DllImport("MarshalObjectsAsAnyDLL.dll", CallingConvention = CallingConvention.StdCall)]
    public static extern bool PassArrayUint(
        [In, MarshalAs(UnmanagedType.AsAny)] object uintArray,
        [In, MarshalAs(UnmanagedType.AsAny)] object uintArray_In,
        [In, Out, MarshalAs(UnmanagedType.AsAny)] object uintArray_InOut,
        [Out, MarshalAs(UnmanagedType.AsAny)] object uintArray_Out,
        int len
    );

    [DllImport("MarshalObjectsAsAnyDLL.dll", CallingConvention = CallingConvention.StdCall)]
    public static extern bool PassArrayLong(
        [In, MarshalAs(UnmanagedType.AsAny)] object longArray,
        [In, MarshalAs(UnmanagedType.AsAny)] object longArray_In,
        [In, Out, MarshalAs(UnmanagedType.AsAny)] object longArray_InOut,
        [Out, MarshalAs(UnmanagedType.AsAny)] object longArray_Out,
        int len
    );

    [DllImport("MarshalObjectsAsAnyDLL.dll", CallingConvention = CallingConvention.StdCall)]
    public static extern bool PassArrayUlong(
        [In, MarshalAs(UnmanagedType.AsAny)] object ulongArray,
        [In, MarshalAs(UnmanagedType.AsAny)] object ulongArray_In,
        [In, Out, MarshalAs(UnmanagedType.AsAny)] object ulongArray_InOut,
        [Out, MarshalAs(UnmanagedType.AsAny)] object ulongArray_Out,
        int len
    );

    [DllImport("MarshalObjectsAsAnyDLL.dll", CallingConvention = CallingConvention.StdCall)]
    public static extern bool PassArraySingle(
        [In, MarshalAs(UnmanagedType.AsAny)] object singleArray,
        [In, MarshalAs(UnmanagedType.AsAny)] object singleArray_In,
        [In, Out, MarshalAs(UnmanagedType.AsAny)] object singleArray_InOut,
        [Out, MarshalAs(UnmanagedType.AsAny)] object singleArray_Out,
        int len
    );

    [DllImport("MarshalObjectsAsAnyDLL.dll", CallingConvention = CallingConvention.StdCall)]
    public static extern bool PassArrayDouble(
        [In, MarshalAs(UnmanagedType.AsAny)] object doubleArray,
        [In, MarshalAs(UnmanagedType.AsAny)] object doubleArray_In,
        [In, Out, MarshalAs(UnmanagedType.AsAny)] object doubleArray_InOut,
        [Out, MarshalAs(UnmanagedType.AsAny)] object doubleArray_Out,
        int len
    );

    [DllImport("MarshalObjectsAsAnyDLL.dll", CallingConvention = CallingConvention.StdCall)]
    public static extern bool PassArrayChar(
        [In, MarshalAs(UnmanagedType.AsAny)] object charArray,
        [In, MarshalAs(UnmanagedType.AsAny)] object charArray_In,
        [In, Out, MarshalAs(UnmanagedType.AsAny)] object charArray_InOut,
        [Out, MarshalAs(UnmanagedType.AsAny)] object charArray_Out,
        int len
    );

    [DllImport("MarshalObjectsAsAnyDLL.dll", CallingConvention = CallingConvention.StdCall)]
    public static extern bool PassArrayBool(
        [In, MarshalAs(UnmanagedType.AsAny)] object boolArray,
        [In, MarshalAs(UnmanagedType.AsAny)] object boolArray_In,
        [In, Out, MarshalAs(UnmanagedType.AsAny)] object boolArray_InOut,
        [Out, MarshalAs(UnmanagedType.AsAny)] object boolArray_Out,
        int len
    );

    [DllImport("MarshalObjectsAsAnyDLL.dll", CallingConvention = CallingConvention.StdCall)]
    public static extern bool PassArrayIntPtr(
        [In, MarshalAs(UnmanagedType.AsAny)] object intPtrArray,
        [In, MarshalAs(UnmanagedType.AsAny)] object intPtrArray_In,
        [In, Out, MarshalAs(UnmanagedType.AsAny)] object intPtrArray_InOut,
        [Out, MarshalAs(UnmanagedType.AsAny)] object intPtrArray_Out,
        int len
    );

    [DllImport("MarshalObjectsAsAnyDLL.dll", CallingConvention = CallingConvention.StdCall)]
    public static extern bool PassArrayUIntPtr(
        [In, MarshalAs(UnmanagedType.AsAny)] object uIntPtrArray,
        [In, MarshalAs(UnmanagedType.AsAny)] object uIntPtrArray_In,
        [In, Out, MarshalAs(UnmanagedType.AsAny)] object uIntPtrArray_InOut,
        [Out, MarshalAs(UnmanagedType.AsAny)] object uIntPtrArray_Out,
        int len
    );
    #endregion 

    #region PInvoke functions for string/stringbuilder/char[]
    // since bestfit = true and throwOn..=false is the default behavior, we have it checked in the old test,
    // so currently we will only cover the left combination
    // true true
    // false true
    // false false

    // string
    #region string
    [DllImport("MarshalObjectsAsAnyDLL.dll", EntryPoint = "PassUnicodeStr", CharSet = CharSet.Unicode, 
    BestFitMapping = true, ThrowOnUnmappableChar = true)]
    public static extern bool PassUnicodeStrTT(
    [MarshalAs(UnmanagedType.AsAny)]
    Object i);

    [DllImport("MarshalObjectsAsAnyDLL.dll", EntryPoint = "PassUnicodeStr", CharSet = CharSet.Unicode,
    BestFitMapping = false, ThrowOnUnmappableChar = true)]
    public static extern bool PassUnicodeStrFT(
    [MarshalAs(UnmanagedType.AsAny)]
    Object i);

    [DllImport("MarshalObjectsAsAnyDLL.dll", EntryPoint = "PassUnicodeStr", CharSet = CharSet.Unicode,
   BestFitMapping = false, ThrowOnUnmappableChar = false)]
    public static extern bool PassUnicodeStrFF(                              
    [MarshalAs(UnmanagedType.AsAny)]
    Object i);

    [DllImport("MarshalObjectsAsAnyDLL.dll", EntryPoint = "PassAnsiStr", CharSet = CharSet.Ansi,
    BestFitMapping = true, ThrowOnUnmappableChar = true)]
    public static extern bool PassAnsiStrTT(
    [MarshalAs(UnmanagedType.AsAny)]
    Object i, bool isIncludeUnMappableChar);

    [DllImport("MarshalObjectsAsAnyDLL.dll", EntryPoint = "PassAnsiStr", CharSet = CharSet.Ansi,
    BestFitMapping = false, ThrowOnUnmappableChar = true)]
    public static extern bool PassAnsiStrFT(
    [MarshalAs(UnmanagedType.AsAny)]
    Object i, bool isIncludeUnMappableChar);

    [DllImport("MarshalObjectsAsAnyDLL.dll", EntryPoint = "PassAnsiStr", CharSet = CharSet.Ansi,
    BestFitMapping = false, ThrowOnUnmappableChar = false)]
    public static extern bool PassAnsiStrFF(
    [MarshalAs(UnmanagedType.AsAny)]
    Object i, bool isIncludeUnMappableChar);

    
    [DllImport("MarshalObjectsAsAnyDLL.dll", CallingConvention=CallingConvention.StdCall,EntryPoint="PassLayout",CharSet=CharSet.Unicode)]
    public static extern long PassLayoutW(
    [MarshalAs(UnmanagedType.AsAny)]
    Object i);

    
    [DllImport("MarshalObjectsAsAnyDLL.dll", CallingConvention=CallingConvention.StdCall,EntryPoint="PassLayout",CharSet=CharSet.Ansi)]
    public static extern long PassLayoutA(
    [MarshalAs(UnmanagedType.AsAny)]
    Object i);

#endregion

    // StringBuilder
    #region stringbuilder old

   // [DllImport("MarshalObjectsAsAnyDLL.dll", EntryPoint = "PassUnicodeStrbd", CharSet = CharSet.Unicode,
   // BestFitMapping = true, ThrowOnUnmappableChar = true)]
   // public static extern bool PassUnicodeStrbdTT(
   //     [In, MarshalAs(UnmanagedType.AsAny)] object strbd_In, 
   //     [In, Out, MarshalAs(UnmanagedType.AsAny)] object strbd_InOut,
   //     [Out, MarshalAs(UnmanagedType.AsAny)] object strbd_Out);

   // [DllImport("MarshalObjectsAsAnyDLL.dll", EntryPoint = "PassUnicodeStrbd", CharSet = CharSet.Unicode,
   // BestFitMapping = false, ThrowOnUnmappableChar = true)]
   // public static extern bool PassUnicodeStrbdFT(
   //     [In, MarshalAs(UnmanagedType.AsAny)] object strbd_In,
   //     [In, Out, MarshalAs(UnmanagedType.AsAny)] object strbd_InOut,
   //     [Out, MarshalAs(UnmanagedType.AsAny)] object strbd_Out);

   // [DllImport("MarshalObjectsAsAnyDLL.dll", EntryPoint = "PassUnicodeStrbd", CharSet = CharSet.Unicode,
   //BestFitMapping = false, ThrowOnUnmappableChar = false)]
   // public static extern bool PassUnicodeStrbdFF(
   //     [In, MarshalAs(UnmanagedType.AsAny)] object strbd_In,
   //     [In, Out, MarshalAs(UnmanagedType.AsAny)] object strbd_InOut,
   //     [Out, MarshalAs(UnmanagedType.AsAny)] object strbd_Out);

   // [DllImport("MarshalObjectsAsAnyDLL.dll", EntryPoint = "PassAnsiStrbd", CharSet = CharSet.Ansi,
   // BestFitMapping = true, ThrowOnUnmappableChar = true)]
   // public static extern bool PassAnsiStrbdTT(
   //     [In, MarshalAs(UnmanagedType.AsAny)] object strbd_In,
   //     [In, Out, MarshalAs(UnmanagedType.AsAny)] object strbd_InOut,
   //     [Out, MarshalAs(UnmanagedType.AsAny)] object strbd_Out, 
   //      bool isIncludeUnMappableChar);

   // [DllImport("MarshalObjectsAsAnyDLL.dll", EntryPoint = "PassAnsiStrbd", CharSet = CharSet.Ansi,
   // BestFitMapping = false, ThrowOnUnmappableChar = true)]
   // public static extern bool PassAnsiStrbdFT(
   //     [In, MarshalAs(UnmanagedType.AsAny)] object strbd_In,
   //     [In, Out, MarshalAs(UnmanagedType.AsAny)] object strbd_InOut,
   //     [Out, MarshalAs(UnmanagedType.AsAny)] object strbd_Out,
   //      bool isIncludeUnMappableChar);

   // [DllImport("MarshalObjectsAsAnyDLL.dll", EntryPoint = "PassAnsiStrbd", CharSet = CharSet.Ansi,
   // BestFitMapping = false, ThrowOnUnmappableChar = false)]
   // public static extern bool PassAnsiStrbdFF(
   //     [In, MarshalAs(UnmanagedType.AsAny)] object strbd_In,
   //     [In, Out, MarshalAs(UnmanagedType.AsAny)] object strbd_InOut,
   //     [Out, MarshalAs(UnmanagedType.AsAny)] object strbd_Out,
   //      bool isIncludeUnMappableChar);
    #endregion

    #region stringbuilder

    [DllImport("MarshalObjectsAsAnyDLL.dll", EntryPoint = "PassUnicodeStrbd", CharSet = CharSet.Unicode,
    BestFitMapping = true, ThrowOnUnmappableChar = true)]
    public static extern bool PassUnicodeStrbdTT(
        [In, MarshalAs(UnmanagedType.AsAny)] object strbd_In);

    [DllImport("MarshalObjectsAsAnyDLL.dll", EntryPoint = "PassUnicodeStrbd", CharSet = CharSet.Unicode,
    BestFitMapping = false, ThrowOnUnmappableChar = true)]
    public static extern bool PassUnicodeStrbdFT(
        [In, MarshalAs(UnmanagedType.AsAny)] object strbd_In);

    [DllImport("MarshalObjectsAsAnyDLL.dll", EntryPoint = "PassUnicodeStrbd", CharSet = CharSet.Unicode,
   BestFitMapping = false, ThrowOnUnmappableChar = false)]
    public static extern bool PassUnicodeStrbdFF(
        [In, MarshalAs(UnmanagedType.AsAny)] object strbd_In);

    [DllImport("MarshalObjectsAsAnyDLL.dll", EntryPoint = "PassAnsiStrbd", CharSet = CharSet.Ansi,
    BestFitMapping = true, ThrowOnUnmappableChar = true)]
    public static extern bool PassAnsiStrbdTT(
        [In, MarshalAs(UnmanagedType.AsAny)] object strbd_In,
         bool isIncludeUnMappableChar);

    [DllImport("MarshalObjectsAsAnyDLL.dll", EntryPoint = "PassAnsiStrbd", CharSet = CharSet.Ansi,
    BestFitMapping = false, ThrowOnUnmappableChar = true)]
    public static extern bool PassAnsiStrbdFT(
        [In, MarshalAs(UnmanagedType.AsAny)] object strbd_In,
         bool isIncludeUnMappableChar);

    [DllImport("MarshalObjectsAsAnyDLL.dll", EntryPoint = "PassAnsiStrbd", CharSet = CharSet.Ansi,
    BestFitMapping = false, ThrowOnUnmappableChar = false)]
    public static extern bool PassAnsiStrbdFF(
        [In, MarshalAs(UnmanagedType.AsAny)] object strbd_In,
         bool isIncludeUnMappableChar);
    #endregion

    // char []
    #region char[]

    [DllImport("MarshalObjectsAsAnyDLL.dll", EntryPoint = "PassUnicodeCharArray", CharSet = CharSet.Unicode,
    BestFitMapping = true, ThrowOnUnmappableChar = true)]
    public static extern bool PassUnicodeCharArrayTT(
        [In, MarshalAs(UnmanagedType.AsAny)] object CharArray_In,
        [In, Out, MarshalAs(UnmanagedType.AsAny)] object CharArray_InOut,
        [Out, MarshalAs(UnmanagedType.AsAny)] object CharArray_Out);


    [DllImport("MarshalObjectsAsAnyDLL.dll", EntryPoint = "PassUnicodeCharArray", CharSet = CharSet.Unicode,
    BestFitMapping = false, ThrowOnUnmappableChar = true)]
    public static extern bool PassUnicodeCharArrayFT(
        [In, MarshalAs(UnmanagedType.AsAny)] object CharArray_In,
        [In, Out, MarshalAs(UnmanagedType.AsAny)] object CharArray_InOut,
        [Out, MarshalAs(UnmanagedType.AsAny)] object CharArray_Out);

    [DllImport("MarshalObjectsAsAnyDLL.dll", EntryPoint = "PassUnicodeCharArray", CharSet = CharSet.Unicode,
   BestFitMapping = false, ThrowOnUnmappableChar = false)]
    public static extern bool PassUnicodeCharArrayFF(
        [In, MarshalAs(UnmanagedType.AsAny)] object CharArray_In,
        [In, Out, MarshalAs(UnmanagedType.AsAny)] object CharArray_InOut,
        [Out, MarshalAs(UnmanagedType.AsAny)] object CharArray_Out);

    [DllImport("MarshalObjectsAsAnyDLL.dll", EntryPoint = "PassAnsiCharArray", CharSet = CharSet.Ansi,
    BestFitMapping = true, ThrowOnUnmappableChar = true)]
    public static extern bool PassAnsiCharArrayTT(
        [In, MarshalAs(UnmanagedType.AsAny)] object CharArray_In,
        [In, Out, MarshalAs(UnmanagedType.AsAny)] object CharArray_InOut,
        [Out, MarshalAs(UnmanagedType.AsAny)] object CharArray_Out,
        bool isIncludeUnMappableChar);

    [DllImport("MarshalObjectsAsAnyDLL.dll", EntryPoint = "PassAnsiCharArray", CharSet = CharSet.Ansi,
    BestFitMapping = false, ThrowOnUnmappableChar = true)]
    public static extern bool PassAnsiCharArrayFT(
        [In, MarshalAs(UnmanagedType.AsAny)] object CharArray_In,
        [In, Out, MarshalAs(UnmanagedType.AsAny)] object CharArray_InOut,
        [Out, MarshalAs(UnmanagedType.AsAny)] object CharArray_Out,
        bool isIncludeUnMappableChar);

    [DllImport("MarshalObjectsAsAnyDLL.dll", EntryPoint = "PassAnsiCharArray", CharSet = CharSet.Ansi,
    BestFitMapping = false, ThrowOnUnmappableChar = false)]
    public static extern bool PassAnsiCharArrayFF(
        [In, MarshalAs(UnmanagedType.AsAny)] object CharArray_In,
        [In, Out, MarshalAs(UnmanagedType.AsAny)] object CharArray_InOut,
        [Out, MarshalAs(UnmanagedType.AsAny)] object CharArray_Out,
        bool isIncludeUnMappableChar);

    #endregion char[]

    #region Struct
    [DllImport("MarshalObjectsAsAnyDLL.dll", EntryPoint = "PassMixStruct")]
    public static extern bool PassMixStruct(Mix mix);
    #endregion
    #endregion

    public static int Main()
    {
        bool funcRet = false;
        bool checkRet_InOut = false;
        bool checkRet_Out = false;

        #region Run test for Primitive type arrays
        Console.WriteLine("\n---------------Run test for Primitive type arrays------------------------");
        // sbyte, byte, short, ushort, int, 
        // uint, long, ulong, single, double, 
        // char, bool, IntPtr, UIntPtr

        // sbyte array
        Console.WriteLine("Scenario : Checking Marshal AsAny for sbyte array ");
        sbyte[] sbyteArray = new sbyte[] { -1, 0, 1 };
        sbyte[] sbyteArray_In = new sbyte[] { -1, 0, 1 };
        sbyte[] sbyteArray_InOut = new sbyte[] { -1, 0, 1 };
        sbyte[] sbyteArray_Out = new sbyte[] { -1, 0, 1 };
        sbyte[] sbyteArray_Back = new sbyte[] { 9, 10, 11 };
        funcRet = PassArraySbyte((object)sbyteArray, (object)sbyteArray_In, (object)sbyteArray_InOut, (object)sbyteArray_Out, 3);
        checkRet_InOut = Helper<sbyte>.CheckArray(sbyteArray_InOut, sbyteArray_Back, "sbyteArray_InOut");
        checkRet_Out = Helper<sbyte>.CheckArray(sbyteArray_Out, sbyteArray_Back, "sbyteArray_Out");
        if (!funcRet || !checkRet_InOut || !checkRet_Out) testFailures++;

        // byte array
        Console.WriteLine("Scenario : Checking Marshal AsAny for byte array ");
        byte[] byteArray = new byte[] { 0, 1, 2 };
        byte[] byteArray_In = new byte[] { 0, 1, 2 };
        byte[] byteArray_InOut = new byte[] { 0, 1, 2 };
        byte[] byteArray_Out = new byte[] { 0, 1, 2 };
        byte[] byteArray_Back = new byte[] { 10, 11, 12 };
        funcRet = PassArrayByte((object)byteArray, (object)byteArray_In, (object)byteArray_InOut, (object)byteArray_Out, 3);
        checkRet_InOut = Helper<byte>.CheckArray(byteArray_InOut, byteArray_Back, "byteArray_InOut");
        checkRet_Out = Helper<byte>.CheckArray(byteArray_Out, byteArray_Back, "byteArray_Out");
        if (!funcRet || !checkRet_InOut || !checkRet_Out) testFailures++;

        // short array
        Console.WriteLine("Scenario : Checking Marshal AsAny for short array ");
        short[] shortArray = new short[] { -1, 0, 1 };
        short[] shortArray_In = new short[] { -1, 0, 1 };
        short[] shortArray_InOut = new short[] { -1, 0, 1 };
        short[] shortArray_Out = new short[] { -1, 0, 1 };
        short[] shortArray_Back = new short[] { 9, 10, 11 };
        funcRet = PassArrayShort((object)shortArray, (object)shortArray_In, (object)shortArray_InOut, (object)shortArray_Out, 3);
        checkRet_InOut = Helper<short>.CheckArray(shortArray_InOut, shortArray_Back, "shortArray_InOut");
        checkRet_Out = Helper<short>.CheckArray(shortArray_Out, shortArray_Back, "shortArray_Out");
        if (!funcRet || !checkRet_InOut || !checkRet_Out) testFailures++;

        // ushort array
        Console.WriteLine("Scenario : Checking Marshal AsAny for ushort array ");
        ushort[] ushortArray = new ushort[] { 0, 1, 2 };
        ushort[] ushortArray_In = new ushort[] { 0, 1, 2 };
        ushort[] ushortArray_InOut = new ushort[] { 0, 1, 2 };
        ushort[] ushortArray_Out = new ushort[] { 0, 1, 2 };
        ushort[] ushortArray_Back = new ushort[] { 10, 11, 12 };
        funcRet = PassArrayUshort((object)ushortArray, (object)ushortArray_In, (object)ushortArray_InOut, (object)ushortArray_Out, 3);
        checkRet_InOut = Helper<ushort>.CheckArray(ushortArray_InOut, ushortArray_Back, "ushortArray_InOut");
        checkRet_Out = Helper<ushort>.CheckArray(ushortArray_Out, ushortArray_Back, "ushortArray_Out");
        if (!funcRet || !checkRet_InOut || !checkRet_Out) testFailures++;

        // int array
        Console.WriteLine("Scenario : Checking Marshal AsAny for Int array ");
        int[] intArray = new int[] { 0, 1, 2 };
        int[] intArray_In = new int[] { 0, 1, 2 };
        int[] intArray_InOut = new int[] { 0, 1, 2 };
        int[] intArray_Out = new int[] { 0, 1, 2 };
        int[] intArray_Back = new int[] { 10, 11, 12 };
        funcRet = PassArrayInt((object)intArray, (object)intArray_In, (object)intArray_InOut, (object)intArray_Out, 3);
        checkRet_InOut = Helper<int>.CheckArray(intArray_InOut, intArray_Back, "IntArray_InOut");
        checkRet_Out = Helper<int>.CheckArray(intArray_Out, intArray_Back, "IntArray_InOut");
        if (!funcRet || !checkRet_InOut || !checkRet_Out) testFailures++;

        // uint array
        Console.WriteLine("Scenario : Checking Marshal AsAny for uint array ");
        uint[] uintArray = new uint[] { 0, 1, 2 };
        uint[] uintArray_In = new uint[] { 0, 1, 2 };
        uint[] uintArray_InOut = new uint[] { 0, 1, 2 };
        uint[] uintArray_Out = new uint[] { 0, 1, 2 };
        uint[] uintArray_Back = new uint[] { 10, 11, 12 };
        funcRet = PassArrayUint((object)uintArray, (object)uintArray_In, (object)uintArray_InOut, (object)uintArray_Out, 3);
        checkRet_InOut = Helper<uint>.CheckArray(uintArray_InOut, uintArray_Back, "uintArray_InOut");
        checkRet_Out = Helper<uint>.CheckArray(uintArray_Out, uintArray_Back, "uintArray_Out");
        if (!funcRet || !checkRet_InOut || !checkRet_Out) testFailures++;

        // long array
        Console.WriteLine("Scenario : Checking Marshal AsAny for long array ");
        long[] longArray = new long[] { 0, 1, 2 };
        long[] longArray_In = new long[] { 0, 1, 2 };
        long[] longArray_InOut = new long[] { 0, 1, 2 };
        long[] longArray_Out = new long[] { 0, 1, 2 };
        long[] longArray_Back = new long[] { 10, 11, 12 };
        funcRet = PassArrayLong((object)longArray, (object)longArray_In, (object)longArray_InOut, (object)longArray_Out, 3);
        checkRet_InOut = Helper<long>.CheckArray(longArray_InOut, longArray_Back, "longArray_InOut");
        checkRet_Out = Helper<long>.CheckArray(longArray_Out, longArray_Back, "longArray_Out");
        if (!funcRet || !checkRet_InOut || !checkRet_Out) testFailures++;

        // ulong array
        Console.WriteLine("Scenario : Checking Marshal AsAny for ulong array ");
        ulong[] ulongArray = new ulong[] { 0, 1, 2 };
        ulong[] ulongArray_In = new ulong[] { 0, 1, 2 };
        ulong[] ulongArray_InOut = new ulong[] { 0, 1, 2 };
        ulong[] ulongArray_Out = new ulong[] { 0, 1, 2 };
        ulong[] ulongArray_Back = new ulong[] { 10, 11, 12 };
        funcRet = PassArrayUlong((object)ulongArray, (object)ulongArray_In, (object)ulongArray_InOut, (object)ulongArray_Out, 3);
        checkRet_InOut = Helper<ulong>.CheckArray(ulongArray_InOut, ulongArray_Back, "ulongArray_InOut");
        checkRet_Out = Helper<ulong>.CheckArray(ulongArray_Out, ulongArray_Back, "ulongArray_Out");
        if (!funcRet || !checkRet_InOut || !checkRet_Out) testFailures++;

        //[DllImport("MarshalObjectsAsAnyDLL.dll", CallingConvention = CallingConvention.StdCall)]
        //public static extern bool PassArraySingle(
        //    [In, MarshalAs(UnmanagedType.AsAny)] object singleArray,
        //    [In, MarshalAs(UnmanagedType.AsAny)] object singleArray_In,
        //    [In, Out, MarshalAs(UnmanagedType.AsAny)] object singleArray_InOut,
        //    [Out, MarshalAs(UnmanagedType.AsAny)] object singleArray_Out,
        //    int len
        //);
        // Single(float) Array
        Console.WriteLine("Scenario : Checking Marshal AsAny for Single(float) array ");
        Single[] singleArray = new Single[] { 0, 1, 2 };
        Single[] singleArray_In = new Single[] { 0, 1, 2 };
        Single[] singleArray_InOut = new Single[] { 0, 1, 2 };
        Single[] singleArray_Out = new Single[] { 0, 1, 2 };
        Single[] singleArray_Back = new Single[] { 10, 11, 12 };
        funcRet = PassArraySingle((object)singleArray, (object)singleArray_In, (object)singleArray_InOut, (object)singleArray_Out, 3);
        checkRet_InOut = Helper<Single>.CheckArray(singleArray_InOut, singleArray_Back, "singleArray_InOut");
        checkRet_Out = Helper<Single>.CheckArray(singleArray_Out, singleArray_Back, "singleArray_Out");
        if (!funcRet || !checkRet_InOut || !checkRet_Out) testFailures++;

    
        // double array
        Console.WriteLine("Scenario : Checking Marshal AsAny for double array ");
        double[] doubleArray = new double[] { 0.0, 1.1, 2.2 };
        double[] doubleArray_In = new double[] { 0.0, 1.1, 2.2 };
        double[] doubleArray_InOut = new double[] { 0.0, 1.1, 2.2 };
        double[] doubleArray_Out = new double[] { 0.0, 1.1, 2.2 };
        double[] doubleArray_Back = new double[] { 10.0, 11.1, 12.2 };
        funcRet = PassArrayDouble((object)doubleArray, (object)doubleArray_In, (object)doubleArray_InOut, (object)doubleArray_Out, 3);
        checkRet_InOut = Helper<double>.CheckArray(doubleArray_InOut, doubleArray_Back, "doubleArray_InOut");
        checkRet_Out = Helper<double>.CheckArray(doubleArray_Out, doubleArray_Back, "doubleArray_Out");
        if (!funcRet || !checkRet_InOut || !checkRet_Out) testFailures++;

        // char array
        Console.WriteLine("Scenario : Checking Marshal AsAny for char array ");
        char[] charArray = new char[] { 'a', 'b', 'c'};
        char[] charArray_In = new char[] { 'a', 'b', 'c' };
        char[] charArray_InOut = new char[] { 'a', 'b', 'c' };
        char[] charArray_Out = new char[] { 'a', 'b', 'c' };
        char[] charArray_Back = new char[] { 'd', 'e', 'f' };
        funcRet = PassArrayChar((object)charArray, (object)charArray_In, (object)charArray_InOut, (object)charArray_Out, 3);
        checkRet_InOut = Helper<char>.CheckArray(charArray_InOut, charArray_Back, "charArray_InOut");
        checkRet_Out = Helper<char>.CheckArray(charArray_Out, charArray_Back, "charArray_Out");
        if (!funcRet || !checkRet_InOut || !checkRet_Out) testFailures++;

        // bool array
        Console.WriteLine("Scenario : Checking Marshal AsAny for bool array ");
        bool[] boolArray = new bool[] { true, false, false };
        bool[] boolArray_In = new bool[] { true, false, false };
        bool[] boolArray_InOut = new bool[] { true, false, false };
        bool[] boolArray_Out = new bool[] { true, false, false };
        bool[] boolArray_Back = new bool[] { false, true, true };
        funcRet = PassArrayBool((object)boolArray, (object)boolArray_In, (object)boolArray_InOut, (object)boolArray_Out, 3);
        checkRet_InOut = Helper<bool>.CheckArray(boolArray_InOut, boolArray_Back, "boolArray_InOut");
        checkRet_Out = Helper<bool>.CheckArray(boolArray_Out, boolArray_Back, "boolArray_Out");
        if (!funcRet || !checkRet_InOut || !checkRet_Out) testFailures++;


        // IntPtr array
        Console.WriteLine("Scenario : Checking Marshal AsAny for IntPtr array ");
        IntPtr[] intPtrArray = new IntPtr[] { new IntPtr(0), new IntPtr(1), new IntPtr(2) };
        IntPtr[] intPtrArray_In = new IntPtr[] { new IntPtr(0), new IntPtr(1), new IntPtr(2) };
        IntPtr[] intPtrArray_InOut = new IntPtr[] { new IntPtr(0), new IntPtr(1), new IntPtr(2) };
        IntPtr[] intPtrArray_Out = new IntPtr[] { new IntPtr(0), new IntPtr(1), new IntPtr(2) };
        IntPtr[] intPtrArray_Back = new IntPtr[] { new IntPtr(10), new IntPtr(11), new IntPtr(12) };
        funcRet = PassArrayIntPtr((object)intPtrArray, (object)intPtrArray_In, (object)intPtrArray_InOut, (object)intPtrArray_Out, 3);
        checkRet_InOut = Helper<IntPtr>.CheckArray(intPtrArray_InOut, intPtrArray_Back, "intPtrArray_InOut");
        checkRet_Out = Helper<IntPtr>.CheckArray(intPtrArray_Out, intPtrArray_Back, "intPtrArray_Out");
        if (!funcRet || !checkRet_InOut || !checkRet_Out) testFailures++;

        // UIntPtr array
        Console.WriteLine("Scenario : Checking Marshal AsAny for UIntPtr array ");
        UIntPtr[] uIntPtrArray = new UIntPtr[] { new UIntPtr(0), new UIntPtr(1), new UIntPtr(2) };
        UIntPtr[] uIntPtrArray_In = new UIntPtr[] { new UIntPtr(0), new UIntPtr(1), new UIntPtr(2) };
        UIntPtr[] uIntPtrArray_InOut = new UIntPtr[] { new UIntPtr(0), new UIntPtr(1), new UIntPtr(2) };
        UIntPtr[] uIntPtrArray_Out = new UIntPtr[] { new UIntPtr(0), new UIntPtr(1), new UIntPtr(2) };
        UIntPtr[] uIntPtrArray_Back = new UIntPtr[] { new UIntPtr(10), new UIntPtr(11), new UIntPtr(12) };
        funcRet = PassArrayUIntPtr((object)uIntPtrArray, (object)uIntPtrArray_In, (object)uIntPtrArray_InOut, (object)uIntPtrArray_Out, 3);
        checkRet_InOut = Helper<UIntPtr>.CheckArray(uIntPtrArray_InOut, uIntPtrArray_Back, "uIntPtrArray_InOut");
        checkRet_Out = Helper<UIntPtr>.CheckArray(uIntPtrArray_Out, uIntPtrArray_Back, "uIntPtrArray_Out");
        if (!funcRet || !checkRet_InOut || !checkRet_Out) testFailures++;


        #endregion


        char mappableChar = (char)0x2075; //int 8039
        char unmappableChar = (char)0x7777; //int 30583
        char NormalChar1 = '0';             // 0x0030 int 48
        char NormalChar2 = '\n';            // 0x 000A int 10
        string mappableStr = "" + NormalChar1 + mappableChar + NormalChar2;
        string unMappableStr = "" + NormalChar1 + mappableChar + unmappableChar;

        #region Run test for string
        Console.WriteLine("\n---------------Run test for string --------------------");

        Helper<object>.RunPositiveMethod("PassUnicodeStrTT", new object[]{unMappableStr});
        Helper<object>.RunPositiveMethod("PassUnicodeStrFT", new object[] { unMappableStr });
        Helper<object>.RunPositiveMethod("PassUnicodeStrFF", new object[] { unMappableStr });

        Helper<object>.RunNegativeMethod("PassAnsiStrTT", new object[] { unMappableStr, true });
        Helper<object>.RunPositiveMethod("PassAnsiStrTT", new object[] { mappableStr, false });
        Helper<object>.RunNegativeMethod("PassAnsiStrFT", new object[] { unMappableStr, true });
        Helper<object>.RunNegativeMethod("PassAnsiStrFT", new object[] { mappableStr, false });
        Helper<object>.RunPositiveMethod("PassAnsiStrFF", new object[] { unMappableStr, true });

        #endregion

        #region Run test for string builder
        Console.WriteLine("\n---------------Run test for string builder------------------------");

        StringBuilder unMappableStrbd = new StringBuilder(unMappableStr);
        StringBuilder mappableStrbd = new StringBuilder(mappableStr);

        Helper<object>.RunPositiveMethod("PassUnicodeStrbdTT", new object[] { unMappableStrbd });
        Helper<object>.RunPositiveMethod("PassUnicodeStrbdFT", new object[] { unMappableStrbd });
        Helper<object>.RunPositiveMethod("PassUnicodeStrbdFF", new object[] { unMappableStrbd });
        Helper<object>.RunNegativeMethod("PassAnsiStrbdTT", new object[] { unMappableStrbd, true });
        Helper<object>.RunPositiveMethod("PassAnsiStrbdTT", new object[] { mappableStrbd, false });
        Helper<object>.RunNegativeMethod("PassAnsiStrbdFT", new object[] { unMappableStrbd, true });
        Helper<object>.RunNegativeMethod("PassAnsiStrbdFT", new object[] { mappableStrbd, false });
        Helper<object>.RunPositiveMethod("PassAnsiStrbdFF", new object[] { unMappableStrbd, true });

        #endregion


        #region Run test for char []
        Console.WriteLine("\n---------------Run test for char []--------------------");
        string unMappableUnicodeStr_back = "" + unmappableChar + mappableChar + NormalChar1;
        string unMappableAnsiStr_back = "" + (char)0x003f+ (char)0x003f + (char)0x0030;
        string mappableAnsiStr_back = "" + (char)0x000A + (char)0x0035 + (char)0x0030;

        char[] unMappableCharArray_In = new char[3] { 'a', 'b', 'c' };
        char[] unMappableCharArray_InOut = new char[3] { 'a', 'b', 'c' };
        char[] unMappableCharArray_Out = new char[3] { 'a', 'b', 'c' };
        char[] mappableCharArray_In = new char[3] { 'a', 'b', 'c' };
        char[] mappableCharArray_InOut = new char[3] { 'a', 'b', 'c' };
        char[] mappableCharArray_Out = new char[3] { 'a', 'b', 'c' };

        // Unicodes
        CharArrayInit(unMappableCharArray_In, unMappableCharArray_InOut, unMappableCharArray_Out,
            mappableCharArray_In, mappableCharArray_InOut, mappableCharArray_Out, unMappableStr, mappableStr);
        Helper<object>.RunPositiveMethod("PassUnicodeCharArrayTT", new object[] { unMappableCharArray_In, unMappableCharArray_InOut, unMappableCharArray_Out });
        Helper<char>.CheckArray(unMappableCharArray_InOut, unMappableUnicodeStr_back.ToCharArray(), "PassUnicodeCharArrayTT");
        Helper<char>.CheckArray(unMappableCharArray_Out, unMappableUnicodeStr_back.ToCharArray(), "PassUnicodeCharArrayTT");

        CharArrayInit(unMappableCharArray_In, unMappableCharArray_InOut, unMappableCharArray_Out,
            mappableCharArray_In, mappableCharArray_InOut, mappableCharArray_Out, unMappableStr, mappableStr);
        Helper<object>.RunPositiveMethod("PassUnicodeCharArrayFT", new object[] { unMappableCharArray_In, unMappableCharArray_InOut, unMappableCharArray_Out });
        Helper<char>.CheckArray(unMappableCharArray_InOut, unMappableUnicodeStr_back.ToCharArray(), "PassUnicodeCharArrayFT");
        Helper<char>.CheckArray(unMappableCharArray_Out, unMappableUnicodeStr_back.ToCharArray(), "PassUnicodeCharArrayFT");

        CharArrayInit(unMappableCharArray_In, unMappableCharArray_InOut, unMappableCharArray_Out,
            mappableCharArray_In, mappableCharArray_InOut, mappableCharArray_Out, unMappableStr, mappableStr);
        Helper<object>.RunPositiveMethod("PassUnicodeCharArrayFF", new object[] { unMappableCharArray_In, unMappableCharArray_InOut, unMappableCharArray_Out });
        Helper<char>.CheckArray(unMappableCharArray_InOut, unMappableUnicodeStr_back.ToCharArray(), "PassUnicodeCharArrayFF");
        Helper<char>.CheckArray(unMappableCharArray_Out, unMappableUnicodeStr_back.ToCharArray(), "PassUnicodeCharArrayFF");

        // Ansi
        CharArrayInit(unMappableCharArray_In, unMappableCharArray_InOut, unMappableCharArray_Out,
           mappableCharArray_In, mappableCharArray_InOut, mappableCharArray_Out, unMappableStr, mappableStr);
        Helper<object>.RunNegativeMethod("PassAnsiCharArrayTT", new object[] { unMappableCharArray_In, unMappableCharArray_InOut, unMappableCharArray_Out, true });

        CharArrayInit(unMappableCharArray_In, unMappableCharArray_InOut, unMappableCharArray_Out,
           mappableCharArray_In, mappableCharArray_InOut, mappableCharArray_Out, unMappableStr, mappableStr);
        Helper<object>.RunPositiveMethod("PassAnsiCharArrayTT", new object[] { mappableCharArray_In, mappableCharArray_InOut, mappableCharArray_Out, false });
        Helper<char>.CheckArray(mappableCharArray_InOut, mappableAnsiStr_back.ToCharArray(), "PassAnsiCharArrayFF");
        Helper<char>.CheckArray(mappableCharArray_Out, mappableAnsiStr_back.ToCharArray(), "PassAnsiCharArrayFF");

        CharArrayInit(unMappableCharArray_In, unMappableCharArray_InOut, unMappableCharArray_Out,
           mappableCharArray_In, mappableCharArray_InOut, mappableCharArray_Out, unMappableStr, mappableStr);
        Helper<object>.RunNegativeMethod("PassAnsiCharArrayFT", new object[] { unMappableCharArray_In, unMappableCharArray_InOut, unMappableCharArray_Out, true });

        CharArrayInit(unMappableCharArray_In, unMappableCharArray_InOut, unMappableCharArray_Out,
           mappableCharArray_In, mappableCharArray_InOut, mappableCharArray_Out, unMappableStr, mappableStr);
        Helper<object>.RunNegativeMethod("PassAnsiCharArrayFT", new object[] { mappableCharArray_In, mappableCharArray_InOut, mappableCharArray_Out, false });

        CharArrayInit(unMappableCharArray_In, unMappableCharArray_InOut, unMappableCharArray_Out,
           mappableCharArray_In, mappableCharArray_InOut, mappableCharArray_Out, unMappableStr, mappableStr);
        Helper<object>.RunPositiveMethod("PassAnsiCharArrayFF", new object[] { unMappableCharArray_In, unMappableCharArray_InOut, unMappableCharArray_Out, true });
        //Console.WriteLine((int)unMappableCharArray_InOut[0] + " " + (int)unMappableCharArray_InOut[1] + " " + (int)unMappableCharArray_InOut[2] + " ");
        //Console.WriteLine("" + 0x0077 + " " + 0x0075 + " " + 0x0030);
        Helper<char>.CheckArray(unMappableCharArray_InOut, unMappableAnsiStr_back.ToCharArray(), "PassAnsiCharArrayFF");
        Helper<char>.CheckArray(unMappableCharArray_Out, unMappableAnsiStr_back.ToCharArray(), "PassAnsiCharArrayFF");

       
        #endregion

        #region Run negative test for struct
        Mix mix = new Mix();
        try
        {
            PassMixStruct(mix);
            //unexpected
            Console.WriteLine("Failed: PassMixStruct does not throw Exception");
            testFailures++;
        }
        catch (TypeLoadException ex)
        {
            if(Thread.CurrentThread.CurrentCulture.Name == "en-US")
            {
                //expected
                string expectedMessage = @"Invalid managed/unmanaged type combination (the Object class must be paired with Interface, IUnknown, IDispatch, or Struct).";
                if (!ex.Message.Contains(expectedMessage))
                {
                    Console.WriteLine("Actual error message :" + ex.Message);
                    Console.WriteLine("Failed: Get TypeLoadException, but not correct error message");
                    testFailures++;
                }
            }

        }
        catch (Exception ex)
        {
            //unexpected
            Console.WriteLine("Failed: PassMixStruct does not throw expected TypeLoadException");
            testFailures++;
        }

        #endregion
        if (testFailures > 0)
        {
            Console.WriteLine("\n==============In total " + testFailures + " tests fails===============");
            return 101;
        }
        else
        {
            Console.WriteLine("\n===================================================");
            Console.WriteLine("All tests passed");
            return 100;
        }

    }

    public static int TestLayout() {
        Console.WriteLine("\nRunning Layout Tests:");
        Console.WriteLine("------------------------");

        int retVal = 0x0;
        test1 tAway;
        long  Back;
        tAway.a = 12;
        tAway.b = 3;
        
        // PassLayout returns input.b
        Back = PassLayoutW((Object)tAway);
        if( Back == tAway.b ){
            Console.WriteLine("TestLayout W results:  PASS");
        } else {
            Console.WriteLine(tAway.b);
            Console.WriteLine(Back);
            Console.WriteLine("TestLayout W results:   FAIL");
            retVal++;
        }
        
        Back = 0;
        Console.WriteLine();
        
        Back = PassLayoutA((Object)tAway);
        if( Back == tAway.b ){
            Console.WriteLine("TestLayout A results:  PASS");
        } else {
            Console.WriteLine(tAway.b);
            Console.WriteLine(Back);
            Console.WriteLine("TestLayout A results:   FAIL");
            retVal++;
        }        
        
        return retVal;         
    }

    static void CharArrayInit(char[] unMappableCharArray_In, char[] unMappableCharArray_InOut, char[] unMappableCharArray_Out,
        char[] mappableCharArray_In, char[] mappableCharArray_InOut, char[] mappableCharArray_Out,
        string unMappableStr, string mappableStr)
    {
        char[] u = unMappableStr.ToCharArray();
        char[] m = mappableStr.ToCharArray();
        for (int i = 0; i < 3; i++)
        {
            unMappableCharArray_In[i] = u[i];
            unMappableCharArray_InOut[i] = u[i];
            unMappableCharArray_Out[i] = u[i];
            mappableCharArray_In[i] = m[i];
            mappableCharArray_InOut[i] = m[i];
            mappableCharArray_Out[i] = m[i];
        }
        
    }

    static void StringBuilderInit(StringBuilder unMappableSb1, StringBuilder unMappableSb2, StringBuilder unMappableSb3,
        StringBuilder mappableSb1, StringBuilder mappableSb2, StringBuilder mappableSb3,
        string unMappableStr, string mappableStr)
    {
        unMappableSb1 = new StringBuilder(unMappableStr);
        unMappableSb2 = new StringBuilder(unMappableStr);
        unMappableSb3 = new StringBuilder(unMappableStr);
        mappableSb1 = new StringBuilder(mappableStr);
        mappableSb2 = new StringBuilder(mappableStr);
        mappableSb3 = new StringBuilder(mappableStr);
    }

    class Helper<T>
    { 
        public static bool CheckArray(T [] acualArray, T [] expectedArray, string arrName)
        {
            for (int i = 0; i < expectedArray.Length; i++)
            {
                if (!expectedArray[i].Equals(acualArray[i]))
                {
                    Console.WriteLine("Actual array[" + i + "]:" + acualArray[i]);
                    Console.WriteLine("Expected[" + i + "]:" + expectedArray[i]); 
                    Console.WriteLine("Failed: " + arrName + " does not have expected value after marshal back");               
                    return false;
                }
            }
            return true;
        }

        public static bool RunPositiveMethod(string methodName, object [] paras)
        {
            Console.WriteLine("Running Postive for Method " + methodName);
            object retObj = typeof(AsAnyTests).InvokeMember(methodName, BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Static , null, null, paras);
            bool ret = (bool)retObj;
            if (!ret)
            {
                Console.WriteLine("Failed:" + methodName + " does not pass pamater check!");
                testFailures++;                
            }
            return ret;
        }

        public static void RunNegativeMethod(string methodName, object[] paras)
        {
            Console.WriteLine("Running Negative for Method " + methodName);
            try
            {
                object ret = typeof(AsAnyTests).InvokeMember(methodName, BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Static, null, null, paras);
                Console.WriteLine("Failed: Expect ArgumentException but we get nothing");
                testFailures++;
            }
            catch (TargetInvocationException ex)
            {
                if (!(ex.InnerException is ArgumentException))
                {
                    Console.WriteLine("Failed: Expect ArgumentException but we get " + ex.InnerException.GetType());
                    testFailures++;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed: Expect ArgumentException but we get " + ex.GetType());
                testFailures++;
            }
        }
    }
}
