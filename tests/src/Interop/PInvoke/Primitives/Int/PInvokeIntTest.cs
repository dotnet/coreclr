// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Runtime.InteropServices;
using System;
using System.Reflection;
using System.Text;
using CoreFXTestLibrary;

class ClientPInvokeIntNativeTest
{
    [DllImport(@"PInvokeIntNative", CallingConvention = CallingConvention.Winapi)]
    private static extern int Marshal_In([In]int intValue);

    [DllImport(@"PInvokeIntNative", CallingConvention = CallingConvention.Winapi)]
    private static extern int Marshal_InOut([In, Out]int intValue);

    [DllImport(@"PInvokeIntNative", CallingConvention = CallingConvention.Winapi)]
    private static extern int Marshal_Out([Out]int intValue);

    [DllImport(@"PInvokeIntNative", CallingConvention = CallingConvention.Winapi)]
    private static extern int MarshalPointer_In([In]ref int pintValue);

    [DllImport(@"PInvokeIntNative", CallingConvention = CallingConvention.Winapi)]
    private static extern int MarshalPointer_InOut(ref int pintValue);

    [DllImport(@"PInvokeIntNative", CallingConvention = CallingConvention.Winapi)]
    private static extern int MarshalPointer_Out(out int pintValue);


    public static int Main(string[] args)
    {
        try{
            int intManaged = (int)1000;
            int intNative = (int)2000;
            int intReturn = (int)3000;

            int int1 = intManaged;
            Assert.AreEqual(intReturn, Marshal_In(int1), "The return value is wrong");

            int int2 = intManaged;
            Assert.AreEqual(intReturn, Marshal_InOut(int2), "The return value is wrong");
            Assert.AreEqual(intManaged, int2, "The parameter value is changed");

            int int3 = intManaged;
            Assert.AreEqual(intReturn, Marshal_Out(int3), "The return value is wrong");
            Assert.AreEqual(intManaged, int3, "The parameter value is changed");

            int int4 = intManaged;
            Assert.AreEqual(intReturn, MarshalPointer_In(ref int4), "The return value is wrong");
            Assert.AreEqual(intManaged, int4, "The parameter value is changed");

            int int5 = intManaged;
            Assert.AreEqual(intReturn, MarshalPointer_InOut(ref int5), "The return value is wrong");
            Assert.AreEqual(intNative, int5, "The passed value is wrong");

            int int6 = intManaged;
            Assert.AreEqual(intReturn, MarshalPointer_Out(out int6), "The return value is wrong");
            Assert.AreEqual(intNative, int6, "The passed value is wrong");

            return 100;
        } catch (Exception e){
            Console.WriteLine($"Test Failure: {e}"); 
            return 101; 
        }
    }
}