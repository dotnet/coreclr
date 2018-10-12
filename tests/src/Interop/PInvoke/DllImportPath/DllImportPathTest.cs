// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

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
    
    [DllImport(@"Moved_DllImportPath_PathEnv", CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Unicode, EntryPoint = "MarshalStringPointer_InOut")]
    private static extern bool MarshalStringPointer_InOut_PathEnv([In, Out]ref string strManaged);

    static bool DllExistsOnLocalPath()
    {
        string strManaged = "Managed";
        string native = " Native";

        Console.WriteLine("[Calling MarshalStringPointer_InOut_Local1].");
        string strPara1 = strManaged;
        if (!MarshalStringPointer_InOut_Local1(ref strPara1))
        {
            Console.WriteLine("Return value is wrong");
            return false;
        }

        if (native != strPara1)
        {
            Console.WriteLine("The passed string is wrong");
            return false;
        }

        Console.WriteLine("[Calling MarshalStringPointer_InOut_Local2]");
        string strPara2 = strManaged;
        if (!MarshalStringPointer_InOut_Local2(ref strPara2))
        {
            Console.WriteLine("Return value is wrong");
            return false;
        }

        if (native != strPara2)
        {
            Console.WriteLine("The passed string is wrong");
            return false;
        }

        Console.WriteLine("[Calling MarshalStringPointer_InOut_LocalWithDot1]");
        string strPara3 = strManaged;
        if (!MarshalStringPointer_InOut_LocalWithDot1(ref strPara3))
        {
            Console.WriteLine("Return value is wrong");
            return false;
        }

        if (native != strPara3)
        {
            Console.WriteLine("The passed string is wrong");
            return false;
        }

        Console.WriteLine("[Calling MarshalStringPointer_InOut_LocalWithDot2]");
        string strPara4 = strManaged;
        if (!MarshalStringPointer_InOut_LocalWithDot2(ref strPara4))
        {
            Console.WriteLine("Return value is wrong");
            return false;
        }

        if (native != strPara4)
        {
            Console.WriteLine("The passed string is wrong");
            return false;
        }

        return true;
    }

    static bool DllExistsOnRelativePath()
    {
        string strManaged = "Managed";
        string native = " Native";

        Console.WriteLine("[Calling MarshalStringPointer_InOut_Relative1]");
        string strPara5 = strManaged;
        if (!MarshalStringPointer_InOut_Relative1(ref strPara5))
        {
            Console.WriteLine("Return value is wrong");
            return false;
        }

        if (native != strPara5)
        {
            Console.WriteLine("The passed string is wrong");
            return false;
        }
        
        Console.WriteLine("[Calling MarshalStringPointer_InOut_Relative2]");
        string strPara6 = strManaged;
        if (!MarshalStringPointer_InOut_Relative2(ref strPara6))
        {
            Console.WriteLine("Return value is wrong");
            return false;
        }

        if (native != strPara6)
        {
            Console.WriteLine("The passed string is wrong");
            return false;
        }
        
        Console.WriteLine("[Calling MarshalStringPointer_InOut_Relative3]");
        string strPara7 = strManaged;
        if (!MarshalStringPointer_InOut_Relative3(ref strPara7))
        {
            Console.WriteLine("Return value is wrong");
            return false;
        }

        if (native != strPara7)
        {
            Console.WriteLine("The passed string is wrong");
            return false;
        }

        
        Console.WriteLine("[Calling MarshalStringPointer_InOut_Relative4]");
        string strPara8 = strManaged;
        if (!MarshalStringPointer_InOut_Relative4(ref strPara8))
        {
            Console.WriteLine("Return value is wrong");
            return false;
        }

        if (native != strPara8)
        {
            Console.WriteLine("The passed string is wrong");
            return false;
        }

        return true;
    }

    private static void SetupPathEnvTest()
    {
        string subDirectoryName = "Subdirectory";
        var currentDirectory = Directory.GetCurrentDirectory();
        var info = new DirectoryInfo(currentDirectory);
        var subDirectory = info.CreateSubdirectory(subDirectoryName);

        var file = info.EnumerateFiles("DllImportPath_PathEnv*", SearchOption.TopDirectoryOnly).First();

        var newFileLocation = Path.Combine(subDirectory.FullName, file.Name);

        file.CopyTo(Path.Combine(subDirectory.FullName, $"Moved_{file.Name}"), true);

        Environment.SetEnvironmentVariable("PATH", Environment.GetEnvironmentVariable("PATH") + $";{subDirectory.FullName}");
    }

    static bool DllExistsOnPathEnv()
    {
        SetupPathEnvTest();

        string managed = "Managed";
        string native = " Native";

        Console.WriteLine("[Calling MarshalStringPointer_InOut_PathEnv]");
        if (!MarshalStringPointer_InOut_PathEnv(ref managed))
        {
            Console.WriteLine("Return value is wrong");
            return false;
        }

        if (native != managed)
        {
            Console.WriteLine($"The passed string is wrong. Expected {native} got {managed}.");
            return false;
        }

        return true;
    }

    static bool DllExistsUnicode()
    {
        string managed = "Managed";
        string native = " Native";
        
        Console.WriteLine("[Calling MarshalStringPointer_InOut_Unicode]");
        if (!MarshalStringPointer_InOut_Unicode(ref managed))
        {
            Console.WriteLine("Return value is wrong");
            return false;
        }

        if (native != managed)
        {
            Console.WriteLine("The passed string is wrong");
            return false;
        }

        return true;
    }

    public static int Main(string[] args)
    {
        bool success = true;
        success = success && DllExistsOnLocalPath();
        success = success && DllExistsOnRelativePath();
        success = success && DllExistsUnicode();
        success = success && DllExistsOnPathEnv();
        
        return success ? 100 : 101;
    }
}
