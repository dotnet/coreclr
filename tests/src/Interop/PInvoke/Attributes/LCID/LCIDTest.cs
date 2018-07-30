// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

//The testcase focus test the BStr with embed null string
using System.Runtime.InteropServices;
using System;
using System.Reflection;
using System.Text;
using CoreFXTestLibrary;

class LCIDTest
{
    [DllImport(@"LCIDNative.dll", EntryPoint = "MarshalStringBuilder_LCID_As_First_Argument")]
    [LCIDConversionAttribute(0)]
    [return: MarshalAs(UnmanagedType.LPStr)]
    private static extern StringBuilder MarshalStringBuilder_LCID_As_First_Argument([In, Out][MarshalAs(UnmanagedType.LPStr)]StringBuilder s);

    [DllImport(@"LCIDNative.dll", EntryPoint = "MarshalStringBuilder_LCID_As_Last_Argument_SetLastError")]
    [LCIDConversionAttribute(1)]
    [return: MarshalAs(UnmanagedType.LPStr)]
    private static extern StringBuilder MarshalStringBuilder_LCID_As_Last_Argument([In, Out][MarshalAs(UnmanagedType.LPStr)]StringBuilder s);

    [DllImport(@"LCIDNative.dll", EntryPoint = "MarshalStringBuilder_LCID_As_Last_Argument_SetLastError", SetLastError = true)]
    [LCIDConversionAttribute(1)]
    [return: MarshalAs(UnmanagedType.LPStr)]
    private static extern StringBuilder MarshalStringBuilder_LCID_As_Last_Argument_SetLastError([In, Out][MarshalAs(UnmanagedType.LPStr)]StringBuilder s);

    [DllImport(@"LCIDNative.dll", EntryPoint = "MarshalStringBuilder_LCID_PreserveSig_SetLastError", PreserveSig = false, SetLastError = true)]
    [LCIDConversionAttribute(1)]
    [return: MarshalAs(UnmanagedType.LPStr)]
    private static extern StringBuilder MarshalStringBuilder_LCID_PreserveSig_SetLastError([In, Out][MarshalAs(UnmanagedType.LPStr)]StringBuilder s);

    //LCID as first argument
    static void Scenario1()
    {
        Console.WriteLine("Scenairo1 started");

        string strManaged = "Managed";
        string strRet = "a";
        StringBuilder expectedStrRet = new StringBuilder("a", 1);
        string strNative = " Native";
        StringBuilder strBNative = new StringBuilder(" Native", 7);

        StringBuilder strPara1 = new StringBuilder(strManaged, strManaged.Length);
        StringBuilder strRet1 = MarshalStringBuilder_LCID_As_First_Argument(strPara1);

        Assert.AreEqual(expectedStrRet.ToString(), strRet1.ToString(), "Method MarshalStringBuilder_LCID_As_First_Argument[Managed Side],The Return string is wrong");
        Assert.AreEqual(strBNative.ToString(), strPara1.ToString(), "Method MarshalStringBuilder_LCID_As_First_Argument[Managed Side],The Passed string is wrong");
        
        Console.WriteLine("Scenairo1 end");
    }

    //LCID as last argument
    static void Scenario2()
    {
        Console.WriteLine("Scenairo2 started");
        
        string strManaged = "Managed";
        string strRet = "a";
        StringBuilder expectedStrRet = new StringBuilder("a", 1);
        string strNative = " Native";
        StringBuilder strBNative = new StringBuilder(" Native", 7);

        StringBuilder strPara2 = new StringBuilder(strManaged, strManaged.Length);
        StringBuilder strRet2 = MarshalStringBuilder_LCID_As_Last_Argument(strPara2);

        Assert.AreEqual(expectedStrRet.ToString(), strRet2.ToString(), "Method MarshalStringBuilder_LCID_As_Last_Argument[Managed Side],The Return string is wrong");
        Assert.AreEqual(strBNative.ToString(), strPara2.ToString(), "Method MarshalStringBuilder_LCID_As_Last_Argument[Managed Side],The Passed string is wrong");

        //Verify that error value is set.
        int result = Marshal.GetLastWin32Error();
        Assert.AreEqual(0, result, "MarshalStringBuilder_LCID_As_Last_Argument: GetLasterror returned wrong error code");
        
        Console.WriteLine("Scenairo2 end");
    }

    //SetLastError =true
    static void Scearnio3()
    {
        Console.WriteLine("Scenairo3 started");
        
        string strManaged = "Managed";
        string strRet = "a";
        StringBuilder expectedStrRet = new StringBuilder("a", 1);
        string strNative = " Native";
        StringBuilder strBNative = new StringBuilder(" Native", 7);

        StringBuilder strPara3 = new StringBuilder(strManaged, strManaged.Length);
        StringBuilder strRet3 = MarshalStringBuilder_LCID_As_Last_Argument_SetLastError(strPara3);

        Assert.AreEqual(expectedStrRet.ToString(), strRet3.ToString(), "Method MarshalStringBuilder_LCID_As_Last_Argument_SetLastError[Managed Side],The Return string is wrong");
        Assert.AreEqual(strBNative.ToString(), strPara3.ToString(), "Method MarshalStringBuilder_LCID_As_Last_Argument_SetLastError[Managed Side],The Passed string is wrong");

        //Verify that error value is set
        int result = Marshal.GetLastWin32Error();
        Assert.AreEqual(1090, result, "MarshalStringBuilder_LCID_As_Last_Argument_SetLastError : GetLasterror returned wrong error code");

        Console.WriteLine("Scenairo3 end");
    }

    //PreserveSig = false, SetLastError = true
    static void Scenario4()
    {
        Console.WriteLine("Scenairo4 started");
        
        string strManaged = "Managed";
        string strRet = "a";
        StringBuilder expectedStrRet = new StringBuilder("a", 1);
        string strNative = " Native";
        StringBuilder strBNative = new StringBuilder(" Native", 7);

        StringBuilder strPara4 = new StringBuilder(strManaged, strManaged.Length);
        StringBuilder strRet4 = MarshalStringBuilder_LCID_PreserveSig_SetLastError(strPara4);

        Assert.AreEqual(expectedStrRet.ToString(), strRet4.ToString(), "Method MarshalStringBuilder_LCID_PreserveSig_SetLastError[Managed Side],The Return string is wrong");
        Assert.AreEqual(strBNative.ToString(), strPara4.ToString(), "Method MarshalStringBuilder_LCID_PreserveSig_SetLastError[Managed Side],The Passed string is wrong");

        //Verify that error value is set
        int result = Marshal.GetLastWin32Error();
        Assert.AreEqual(1090, result, "MarshalStringBuilder_LCID_PreserveSig_SetLastError : GetLasterror returned wrong error code");

        Console.WriteLine("Scenairo4 end");
    }

    public static int Main(string[] args)
    {
        try
        {
            //LCID as first argument
            Scenario1();
            //LCID as last argument
            Scenario2();
            //SetLastError =true
            Scearnio3();
            //PreserveSig = false, SetLastError = true
            Scenario4();

            return 100;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Test Failure: {e}");
            return 101;
        }
    }
}
