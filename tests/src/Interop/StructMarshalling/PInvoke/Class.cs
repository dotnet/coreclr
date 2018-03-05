using System;
using System.Runtime.InteropServices;

public class Common
{
    public const int NumArrElements = 2;
}
//////////////////////////////class definition///////////////////////////
[StructLayout(LayoutKind.Sequential)]
public class InnerSequential
{
    public int f1;
    public float f2;
    public String f3;
}

[StructLayout(LayoutKind.Sequential)]
public struct InnerSequentialStruct
{
    public int f1;
    public float f2;
    public String f3;
}

[StructLayout(LayoutKind.Sequential)]
public class ComplexStruct
{
    public int i;
    [MarshalAs(UnmanagedType.I1)]
    public bool b;
    [MarshalAs(UnmanagedType.LPStr)]
    public string str;
    public IntPtr pedding;
    public ScriptParamType type;
}

[StructLayout(LayoutKind.Explicit)]
public class ScriptParamType
{
    [FieldOffset(0)]
    public int idata;
    [FieldOffset(8)]
    public bool bdata;
    [FieldOffset(8)]
    public double ddata;
    [FieldOffset(8)]
    public IntPtr ptrdata;
}

[StructLayout(LayoutKind.Explicit)]
public class INNER2
{
    [FieldOffset(0)]
    public int f1;
    [FieldOffset(4)]
    public float f2;
    [FieldOffset(8)] 
    public String f3;
}

[StructLayout(LayoutKind.Explicit)]
public class InnerExplicit
{
    [FieldOffset(0)]
    public int f1;
    [FieldOffset(0)]
    public float f2;
    [FieldOffset(8)]
    public String f3;
}

[StructLayout(LayoutKind.Sequential)]//class containing one field of array type
public class InnerArraySequential
{
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = Common.NumArrElements)]
    public InnerSequentialStruct[] arr;
}

[StructLayout(LayoutKind.Explicit, Pack = 8)]
public class InnerArrayExplicit
{
    [FieldOffset(0)]
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = Common.NumArrElements)]
    public InnerSequentialStruct[] arr;

    [FieldOffset(8)]
    public string f4;
}

[StructLayout(LayoutKind.Explicit)]
public class OUTER3
{
    [FieldOffset(0)]
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = Common.NumArrElements)]
    public InnerSequentialStruct[] arr;

    [FieldOffset(24)]
    public string f4;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
public class CharSetAnsiSequential
{
    public string f1;
    public char f2;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public class CharSetUnicodeSequential
{
    public string f1;
    public char f2;
}

[StructLayout(LayoutKind.Sequential)]
public class NumberSequential
{
    public Int64 i64;
    public UInt64 ui64;
    public Double d;
    public int i32;
    public uint ui32;
    public short s1;
    public ushort us1;
    public Int16 i16;
    public UInt16 ui16;
    public Single sgl;
    public Byte b;
    public SByte sb; 
}

[StructLayout(LayoutKind.Sequential)]
public class S3
{
    public bool flag;
    [MarshalAs(UnmanagedType.LPStr)]
    public string str;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
    public int[] vals;
}

[StructLayout(LayoutKind.Sequential)]
public class S4
{
    public int age;
    public string name;
}

public enum Enum1 { e1 = 1, e2 = 3 };
[StructLayout(LayoutKind.Sequential)]
public class S5
{
    public S4 s4;
    public Enum1 ef;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
public class StringStructSequentialAnsi
{
    public string first;
    public string last;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public class StringStructSequentialUnicode
{
    public string first;
    public string last;
}
[StructLayout(LayoutKind.Sequential)]
public class S8
{
    public string name;
    public bool gender;
    [MarshalAs(UnmanagedType.Error)]
    public int i32;
    [MarshalAs(UnmanagedType.Error)]
    public uint ui32;
    [MarshalAs(UnmanagedType.U2)]
    public UInt16 jobNum;
    [MarshalAs(UnmanagedType.I1)]
    public sbyte mySByte;
}

[StructLayout(LayoutKind.Sequential)]
public class S9
{
    [MarshalAs(UnmanagedType.Error)]
    public int i32;
    public TestDelegate1 myDelegate1;
}

public delegate void TestDelegate1(S9 myStruct);

[StructLayout(LayoutKind.Sequential)]
public class IntergerStructSequential
{
    public int i;
}
[StructLayout(LayoutKind.Sequential)]
public class OuterIntergerStructSequential
{
    public int i;
    public IntergerStructSequential s_int = new IntergerStructSequential();
}
[StructLayout(LayoutKind.Sequential)]
public class IncludeOuterIntergerStructSequential
{
    public OuterIntergerStructSequential s = new OuterIntergerStructSequential();
}
[StructLayout(LayoutKind.Sequential)]
public unsafe class S11
{
    public int* i32;
    public int i;
}

[StructLayout(LayoutKind.Explicit)]
public class U
{
    [FieldOffset(0)]
    public int i32;
    [FieldOffset(0)]
    public uint ui32;
    [FieldOffset(0)]
    public IntPtr iPtr;
    [FieldOffset(0)]
    public UIntPtr uiPtr;
    [FieldOffset(0)]
    public short s;
    [FieldOffset(0)]
    public ushort us;
    [FieldOffset(0)]
    public Byte b;
    [FieldOffset(0)]
    public SByte sb;
    [FieldOffset(0)]
    public long l;
    [FieldOffset(0)]
    public ulong ul;
    [FieldOffset(0)]
    public float f;
    [FieldOffset(0)]
    public Double d;
}

[StructLayout(LayoutKind.Explicit, Size = 2)]
public class ByteStructPack2Explicit
{
    [FieldOffset(0)]
    public byte b1;
    [FieldOffset(1)]
    public byte b2;
}

[StructLayout(LayoutKind.Explicit, Size = 4)]
public class ShortStructPack4Explicit
{
    [FieldOffset(0)]
    public short s1;
    [FieldOffset(2)]
    public short s2;
}

[StructLayout(LayoutKind.Explicit, Size = 8)]
public class IntStructPack8Explicit
{
    [FieldOffset(0)]
    public int i1;
    [FieldOffset(4)]
    public int i2;
}

[StructLayout(LayoutKind.Explicit, Size = 16)]
public class LongStructPack16Explicit
{
    [FieldOffset(0)]
    public long l1;
    [FieldOffset(8)]
    public long l2;
}
