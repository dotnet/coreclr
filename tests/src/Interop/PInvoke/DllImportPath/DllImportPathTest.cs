// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Runtime.InteropServices;
using CoreFXTestLibrary;

class Test
{
    [StructLayout(LayoutKind.Sequential)]
    public struct SYSTEM_INFO
    {
        public System.UInt16 wProcessorArchitecture;
        public System.UInt16 wReserved;
        public System.UInt32 dwPageSize;
        public System.IntPtr lpMinimumApplicationAddress;
        public System.IntPtr lpMaximumApplicationAddress;
        public System.UIntPtr dwActiveProcessorMask;
        public System.UInt32 dwNumberOfProcessors;
        public System.UInt32 dwProcessorType;
        public System.UInt32 dwAllocationGranularity;
        public System.UInt16 wProcessorLevel;
        public System.UInt16 wProcessorRevision;
    }

    [DllImport(@"DllImportPath_Local", CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Unicode, EntryPoint = "MarshalStringPointer_InOut")]
    private static extern bool MarshalStringPointer_InOut_Local1([In, Out]ref string strManaged);

    [DllImport(@".\DllImportPath_Local", CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Unicode, EntryPoint = "MarshalStringPointer_InOut")]
    private static extern bool MarshalStringPointer_InOut_Local2([In, Out]ref string strManaged);

    [DllImport(@"DllImportPath.Local.dll", CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Unicode, EntryPoint = "MarshalStringPointer_InOut")]
    private static extern bool MarshalStringPointer_InOut_LocalWithDot1([In, Out]ref string strManaged);

    [DllImport(@".\DllImportPath.Local.dll", CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Unicode, EntryPoint = "MarshalStringPointer_InOut")]
    private static extern bool MarshalStringPointer_InOut_LocalWithDot2([In, Out]ref string strManaged);

    [DllImport(@".\RelativeNative\..\DllImportPath_Relative", CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Unicode, EntryPoint = "MarshalStringPointer_InOut")]
    private static extern bool MarshalStringPointer_InOut_Relative1([In, Out]ref string strManaged);

    [DllImport(@"..\DllImportPathTest\DllImportPath_Relative.dll", CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Unicode, EntryPoint = "MarshalStringPointer_InOut")]
    private static extern bool MarshalStringPointer_InOut_Relative2([In, Out]ref string strManaged);

    [DllImport(@"..\DllImportPathTest\DllImportPath_Relative", CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Unicode, EntryPoint = "MarshalStringPointer_InOut")]
    private static extern bool MarshalStringPointer_InOut_Relative3([In, Out]ref string strManaged);

    [DllImport(@".\..\DllImportPathTest\DllImportPath_Relative.dll", CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Unicode, EntryPoint = "MarshalStringPointer_InOut")]
    private static extern bool MarshalStringPointer_InOut_Relative4([In, Out]ref string strManaged);

    [DllImport(@"DllImportPath_U�n�i�c�o�d�e.dll", CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Unicode, EntryPoint = "MarshalStringPointer_InOut")]
    private static extern bool MarshalStringPointer_InOut_Unicode([In, Out]ref string strManaged);

    [DllImport(@"api-ms-win-core-sysinfo-l1-2-0.dll", CallingConvention = CallingConvention.Winapi, SetLastError = false, PreserveSig = true)]
    public static extern void GetNativeSystemInfo(ref SYSTEM_INFO a);

    [DllImport(@"api-ms-win-core-errorhandling-l1-1-0", CallingConvention = CallingConvention.Winapi, SetLastError = false, PreserveSig = true)]
    public static extern void SetLastError(System.Int32 a);

    [DllImport(@"api-ms-win-core-errorhandling-l1-1-0", CallingConvention = CallingConvention.Winapi, SetLastError = false, PreserveSig = true)]
    public static extern System.Int32 GetLastError();

    static void DllExistsOnLocalPath()
    {
        string strManaged = "Managed";
        string strNative = " Native";

        Console.WriteLine("[Calling MarshalStringPointer_InOut_Local1].");
        string strPara1 = strManaged;
        Assert.IsTrue(MarshalStringPointer_InOut_Local1(ref strPara1), "the return value is wrong");
        Assert.AreEqual(strNative, strPara1, "the passed string is wrong");

        Console.WriteLine("[Calling MarshalStringPointer_InOut_Local2]");
        string strPara2 = strManaged;
        Assert.IsTrue(MarshalStringPointer_InOut_Local2(ref strPara2), "the return value is wrong");
        Assert.AreEqual(strNative, strPara2, "the passed string is wrong");

        Console.WriteLine("[Calling MarshalStringPointer_InOut_LocalWithDot1]");
        string strPara3 = strManaged;
        Assert.IsTrue(MarshalStringPointer_InOut_LocalWithDot1(ref strPara3), "the return value is wrong");
        Assert.AreEqual(strNative, strPara3, "The passed string is wrong");

        Console.WriteLine("[Calling MarshalStringPointer_InOut_LocalWithDot2]");
        string strPara4 = strManaged;
        Assert.IsTrue(MarshalStringPointer_InOut_LocalWithDot2(ref strPara4), "the return value is wrong");
        Assert.AreEqual(strNative, strPara4, "the passed string is wrong");
    }

    static void DllExistsOnRelativePath()
    {
        string strManaged = "Managed";
        string strNative = " Native";

        Console.WriteLine("[Calling MarshalStringPointer_InOut_Relative1]");
        string strPara5 = strManaged;
        Assert.IsTrue(MarshalStringPointer_InOut_Relative1(ref strPara5), "the return value is wrong");
        Assert.AreEqual(strNative, strPara5, "the passed string is wrong");
        
        Console.WriteLine("[Calling MarshalStringPointer_InOut_Relative2]");
        string strPara6 = strManaged;
        Assert.IsTrue(MarshalStringPointer_InOut_Relative2(ref strPara6), "the return value is wrong");
        Assert.AreEqual(strNative, strPara6, "the passed string is wrong");
        
        Console.WriteLine("[Calling MarshalStringPointer_InOut_Relative3]");
        string strPara7 = strManaged;
        Assert.IsTrue(MarshalStringPointer_InOut_Relative3(ref strPara7), "the return value is wrong");
        Assert.AreEqual(strNative, strPara7, "the passed string is wrong");
        
        Console.WriteLine("[Calling MarshalStringPointer_InOut_Relative4]");
        string strPara8 = strManaged;
        Assert.IsTrue(MarshalStringPointer_InOut_Relative4(ref strPara8), "the return value is wrong");
        Assert.AreEqual(strNative, strPara8, "the passed string is wrong");
    }

    static void DllExistsOnPathEnv()
    {
        Console.WriteLine("[Calling GetNativeSystemInfo]");
        SYSTEM_INFO sysInfo = new SYSTEM_INFO();
        GetNativeSystemInfo(ref sysInfo);
        Assert.AreEqual<uint>((uint)Environment.ProcessorCount, sysInfo.dwNumberOfProcessors, "Method GetNativeSystemInfo, the number of processors reported is different.");

        Console.WriteLine("[Calling SetLastError/GetLastError]");
        int intError10 = 255;
        SetLastError(intError10);
        int intError11 = GetLastError();
        Assert.AreEqual(intError10, intError11, "The value set by SetLastError and the value returned by GetLastError is different.");
    }

    static void DllExistsUnicode()
    {
        string managed = "Managed";
        string native = " Native";
        
        Console.WriteLine("[Calling MarshalStringPointer_InOut_Unicode]");
        Assert.IsTrue(MarshalStringPointer_InOut_Unicode(ref managed), "the return value is wrong");
        Assert.AreEqual(native, managed, "the passed string is wrong");
    }

    public static int Main(string[] args)
    {
        try
        {
            DllExistsOnLocalPath();
            DllExistsOnRelativePath();
            DllExistsUnicode();
            DllExistsOnPathEnv();
            
            return 100;
        } catch (Exception e){
            Console.WriteLine($"Test Failure: {e}"); 
            return 101; 
        }
    }
}