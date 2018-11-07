// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Runtime.InteropServices;
using TestLibrary;

#pragma warning disable CS0612, CS0618
class Test
{
    private const string NativeLibrary = "VariantNative";

    private const byte NumericValue = 15;

    private const char CharValue = 'z';

    private const string StringValue = "Abcdefg";

    private const decimal DecimalValue = 74.25M;

    private static readonly DateTime DateValue = new DateTime(2018, 11, 6);

    private struct CustomStruct
    {

    }

    private struct ObjectWrapper
    {
        [MarshalAs(UnmanagedType.Struct)]
        public object value;
    }

    private enum ExpectedVariantType
    {
        Byte,
        SByte,
        Int16,
        UInt16,
        Int32,
        UInt32,
        Int64,
        UInt64,
        Single,
        Double,
        String,
        Char,
        Boolean,
        DateTime,
        Decimal,
        Missing,
        Object,
        Empty,
        Null
    }

    [DllImport(NativeLibrary)]
    private static extern bool Marshal_ByValue_Byte(object obj, byte expected);
    [DllImport(NativeLibrary)]
    private static extern bool Marshal_ByValue_SByte(object obj, sbyte expected);
    [DllImport(NativeLibrary)]
    private static extern bool Marshal_ByValue_Int16(object obj, short expected);
    [DllImport(NativeLibrary)]
    private static extern bool Marshal_ByValue_UInt16(object obj, ushort expected);
    [DllImport(NativeLibrary)]
    private static extern bool Marshal_ByValue_Int32(object obj, int expected);
    [DllImport(NativeLibrary)]
    private static extern bool Marshal_ByValue_UInt32(object obj, uint expected);
    [DllImport(NativeLibrary)]
    private static extern bool Marshal_ByValue_Int64(object obj, long expected);
    [DllImport(NativeLibrary)]
    private static extern bool Marshal_ByValue_UInt64(object obj, ulong expected);
    [DllImport(NativeLibrary)]
    private static extern bool Marshal_ByValue_Single(object obj, float expected);
    [DllImport(NativeLibrary)]
    private static extern bool Marshal_ByValue_Double(object obj, double expected);
    [DllImport(NativeLibrary, CharSet = CharSet.Unicode)]
    private static extern bool Marshal_ByValue_String(object obj, [MarshalAs(UnmanagedType.BStr)] string expected);
    [DllImport(NativeLibrary)]
    private static extern bool Marshal_ByValue_Char(object obj, char expected);
    [DllImport(NativeLibrary)]
    private static extern bool Marshal_ByValue_Boolean(object obj, [MarshalAs(UnmanagedType.VariantBool)] bool expected);
    [DllImport(NativeLibrary)]
    private static extern bool Marshal_ByValue_DateTime(object obj, DateTime expected);
    [DllImport(NativeLibrary)]
    private static extern bool Marshal_ByValue_Decimal(object obj, decimal expected);
    [DllImport(NativeLibrary)]
    private static extern bool Marshal_ByValue_Missing(object obj);
    [DllImport(NativeLibrary)]
    private static extern bool Marshal_ByValue_Object(object obj);
    [DllImport(NativeLibrary)]
    private static extern bool Marshal_ByValue_Empty(object obj);
    [DllImport(NativeLibrary)]
    private static extern bool Marshal_ByValue_Null(object obj);

    [DllImport(NativeLibrary)]
    private static extern bool Marshal_ByValue_Invalid(object obj);

    [DllImport(NativeLibrary)]
    private static extern bool Marshal_ByRef_Byte(ref object obj, byte expected);
    [DllImport(NativeLibrary)]
    private static extern bool Marshal_ByRef_SByte(ref object obj, sbyte expected);
    [DllImport(NativeLibrary)]
    private static extern bool Marshal_ByRef_Int16(ref object obj, short expected);
    [DllImport(NativeLibrary)]
    private static extern bool Marshal_ByRef_UInt16(ref object obj, ushort expected);
    [DllImport(NativeLibrary)]
    private static extern bool Marshal_ByRef_Int32(ref object obj, int expected);
    [DllImport(NativeLibrary)]
    private static extern bool Marshal_ByRef_UInt32(ref object obj, uint expected);
    [DllImport(NativeLibrary)]
    private static extern bool Marshal_ByRef_Int64(ref object obj, long expected);
    [DllImport(NativeLibrary)]
    private static extern bool Marshal_ByRef_UInt64(ref object obj, ulong expected);
    [DllImport(NativeLibrary)]
    private static extern bool Marshal_ByRef_Single(ref object obj, float expected);
    [DllImport(NativeLibrary)]
    private static extern bool Marshal_ByRef_Double(ref object obj, double expected);
    [DllImport(NativeLibrary, CharSet = CharSet.Unicode)]
    private static extern bool Marshal_ByRef_String(ref object obj, [MarshalAs(UnmanagedType.BStr)] string expected);
    [DllImport(NativeLibrary)]
    private static extern bool Marshal_ByRef_Char(ref object obj, char expected);
    [DllImport(NativeLibrary)]
    private static extern bool Marshal_ByRef_Boolean(ref object obj, [MarshalAs(UnmanagedType.VariantBool)] bool expected);
    [DllImport(NativeLibrary)]
    private static extern bool Marshal_ByRef_DateTime(ref object obj, DateTime expected);
    [DllImport(NativeLibrary)]
    private static extern bool Marshal_ByRef_Decimal(ref object obj, decimal expected);
    [DllImport(NativeLibrary)]
    private static extern bool Marshal_ByRef_Missing(ref object obj);
    [DllImport(NativeLibrary)]
    private static extern bool Marshal_ByRef_Object(ref object obj);
    [DllImport(NativeLibrary)]
    private static extern bool Marshal_ByRef_Empty(ref object obj);
    [DllImport(NativeLibrary)]
    private static extern bool Marshal_ByRef_Null(ref object obj);

    [DllImport(NativeLibrary)]
    private static extern bool Marshal_ByRef_Invalid(ref object obj);

    [DllImport(NativeLibrary)]
    private static extern bool Marshal_ChangeVariantType(ref object obj, int expected);

    [DllImport(NativeLibrary)]
    private static extern bool Marshal_Out(out object obj, int expected);
    [DllImport(NativeLibrary)]
    private static extern bool Marshal_Struct_ByValue_Byte(ObjectWrapper wrapper, byte expected);
    [DllImport(NativeLibrary)]
    private static extern bool Marshal_Struct_ByValue_SByte(ObjectWrapper wrapper, sbyte expected);
    [DllImport(NativeLibrary)]
    private static extern bool Marshal_Struct_ByValue_Int16(ObjectWrapper wrapper, short expected);
    [DllImport(NativeLibrary)]
    private static extern bool Marshal_Struct_ByValue_UInt16(ObjectWrapper wrapper, ushort expected);
    [DllImport(NativeLibrary)]
    private static extern bool Marshal_Struct_ByValue_Int32(ObjectWrapper wrapper, int expected);
    [DllImport(NativeLibrary)]
    private static extern bool Marshal_Struct_ByValue_UInt32(ObjectWrapper wrapper, uint expected);
    [DllImport(NativeLibrary)]
    private static extern bool Marshal_Struct_ByValue_Int64(ObjectWrapper wrapper, long expected);
    [DllImport(NativeLibrary)]
    private static extern bool Marshal_Struct_ByValue_UInt64(ObjectWrapper wrapper, ulong expected);
    [DllImport(NativeLibrary)]
    private static extern bool Marshal_Struct_ByValue_Single(ObjectWrapper wrapper, float expected);
    [DllImport(NativeLibrary)]
    private static extern bool Marshal_Struct_ByValue_Double(ObjectWrapper wrapper, double expected);
    [DllImport(NativeLibrary, CharSet = CharSet.Unicode)]
    private static extern bool Marshal_Struct_ByValue_String(ObjectWrapper wrapper, [MarshalAs(UnmanagedType.BStr)] string expected);
    [DllImport(NativeLibrary)]
    private static extern bool Marshal_Struct_ByValue_Char(ObjectWrapper wrapper, char expected);
    [DllImport(NativeLibrary)]
    private static extern bool Marshal_Struct_ByValue_Boolean(ObjectWrapper wrapper, [MarshalAs(UnmanagedType.VariantBool)] bool expected);
    [DllImport(NativeLibrary)]
    private static extern bool Marshal_Struct_ByValue_DateTime(ObjectWrapper wrapper, DateTime expected);
    [DllImport(NativeLibrary)]
    private static extern bool Marshal_Struct_ByValue_Decimal(ObjectWrapper wrapper, decimal expected);
    [DllImport(NativeLibrary)]
    private static extern bool Marshal_Struct_ByValue_Missing(ObjectWrapper wrapper);
    [DllImport(NativeLibrary)]
    private static extern bool Marshal_Struct_ByValue_Object(ObjectWrapper wrapper);
    [DllImport(NativeLibrary)]
    private static extern bool Marshal_Struct_ByValue_Empty(ObjectWrapper wrapper);
    [DllImport(NativeLibrary)]
    private static extern bool Marshal_Struct_ByValue_Null(ObjectWrapper wrapper);

    [DllImport(NativeLibrary)]
    private static extern bool Marshal_Struct_ByRef_Byte(ref ObjectWrapper wrapper, byte expected);
    [DllImport(NativeLibrary)]
    private static extern bool Marshal_Struct_ByRef_SByte(ref ObjectWrapper wrapper, sbyte expected);
    [DllImport(NativeLibrary)]
    private static extern bool Marshal_Struct_ByRef_Int16(ref ObjectWrapper wrapper, short expected);
    [DllImport(NativeLibrary)]
    private static extern bool Marshal_Struct_ByRef_UInt16(ref ObjectWrapper wrapper, ushort expected);
    [DllImport(NativeLibrary)]
    private static extern bool Marshal_Struct_ByRef_Int32(ref ObjectWrapper wrapper, int expected);
    [DllImport(NativeLibrary)]
    private static extern bool Marshal_Struct_ByRef_UInt32(ref ObjectWrapper wrapper, uint expected);
    [DllImport(NativeLibrary)]
    private static extern bool Marshal_Struct_ByRef_Int64(ref ObjectWrapper wrapper, long expected);
    [DllImport(NativeLibrary)]
    private static extern bool Marshal_Struct_ByRef_UInt64(ref ObjectWrapper wrapper, ulong expected);
    [DllImport(NativeLibrary)]
    private static extern bool Marshal_Struct_ByRef_Single(ref ObjectWrapper wrapper, float expected);
    [DllImport(NativeLibrary)]
    private static extern bool Marshal_Struct_ByRef_Double(ref ObjectWrapper wrapper, double expected);
    [DllImport(NativeLibrary, CharSet = CharSet.Unicode)]
    private static extern bool Marshal_Struct_ByRef_String(ref ObjectWrapper wrapper, [MarshalAs(UnmanagedType.BStr)] string expected);
    [DllImport(NativeLibrary)]
    private static extern bool Marshal_Struct_ByRef_Char(ref ObjectWrapper wrapper, char expected);
    [DllImport(NativeLibrary)]
    private static extern bool Marshal_Struct_ByRef_Boolean(ref ObjectWrapper wrapper, [MarshalAs(UnmanagedType.VariantBool)] bool expected);
    [DllImport(NativeLibrary)]
    private static extern bool Marshal_Struct_ByRef_DateTime(ref ObjectWrapper wrapper, DateTime expected);
    [DllImport(NativeLibrary)]
    private static extern bool Marshal_Struct_ByRef_Decimal(ref ObjectWrapper wrapper, decimal expected);
    [DllImport(NativeLibrary)]
    private static extern bool Marshal_Struct_ByRef_Missing(ref ObjectWrapper wrapper);
    [DllImport(NativeLibrary)]
    private static extern bool Marshal_Struct_ByRef_Object(ref ObjectWrapper wrapper);
    [DllImport(NativeLibrary)]
    private static extern bool Marshal_Struct_ByRef_Empty(ref ObjectWrapper wrapper);
    [DllImport(NativeLibrary)]
    private static extern bool Marshal_Struct_ByRef_Null(ref ObjectWrapper wrapper);

    private unsafe static void TestByValue()
    {
        Assert.IsTrue(Marshal_ByValue_Byte((byte)NumericValue, NumericValue));
        Assert.IsTrue(Marshal_ByValue_SByte((sbyte)NumericValue, (sbyte)NumericValue));
        Assert.IsTrue(Marshal_ByValue_Int16((short)NumericValue, NumericValue));
        Assert.IsTrue(Marshal_ByValue_UInt16((ushort)NumericValue, NumericValue));
        Assert.IsTrue(Marshal_ByValue_Int32((int)NumericValue, NumericValue));
        Assert.IsTrue(Marshal_ByValue_UInt32((uint)NumericValue, NumericValue));
        Assert.IsTrue(Marshal_ByValue_Int64((long)NumericValue, NumericValue));
        Assert.IsTrue(Marshal_ByValue_UInt64((ulong)NumericValue, NumericValue));
        Assert.IsTrue(Marshal_ByValue_Single((float)NumericValue, NumericValue));
        Assert.IsTrue(Marshal_ByValue_Double((double)NumericValue, NumericValue));
        Assert.IsTrue(Marshal_ByValue_String(StringValue, StringValue));
        Assert.IsTrue(Marshal_ByValue_Char(CharValue, CharValue));
        Assert.IsTrue(Marshal_ByValue_Boolean(true, true));
        Assert.IsTrue(Marshal_ByValue_DateTime(DateValue, DateValue));
        Assert.IsTrue(Marshal_ByValue_Decimal((decimal)DecimalValue, DecimalValue));
        Assert.IsTrue(Marshal_ByValue_Null(DBNull.Value));
        Assert.IsTrue(Marshal_ByValue_Missing(System.Reflection.Missing.Value));
        Assert.IsTrue(Marshal_ByValue_Empty(null));
        Assert.IsTrue(Marshal_ByValue_Object(new object()));
        Assert.Throws<ArgumentException>(() => Marshal_ByValue_Invalid(TimeSpan.Zero));
        Assert.Throws<NotSupportedException>(() => Marshal_ByValue_Invalid(new CustomStruct()));
    }

    private unsafe static void TestByRef()
    {
        object obj;

        obj = (byte)NumericValue;
        Assert.IsTrue(Marshal_ByRef_Byte(ref obj, NumericValue));
        
        obj = (sbyte)NumericValue;
        Assert.IsTrue(Marshal_ByRef_SByte(ref obj, (sbyte)NumericValue));
        
        obj = (short)NumericValue;
        Assert.IsTrue(Marshal_ByRef_Int16(ref obj, NumericValue));
        
        obj = (ushort)NumericValue;
        Assert.IsTrue(Marshal_ByRef_UInt16(ref obj, NumericValue));
        
        obj = (int)NumericValue;
        Assert.IsTrue(Marshal_ByRef_Int32(ref obj, NumericValue));
        
        obj = (uint)NumericValue;
        Assert.IsTrue(Marshal_ByRef_UInt32(ref obj, NumericValue));
        
        obj = (long)NumericValue;
        Assert.IsTrue(Marshal_ByRef_Int64(ref obj, NumericValue));
        
        obj = (ulong)NumericValue;
        Assert.IsTrue(Marshal_ByRef_UInt64(ref obj, NumericValue));
        
        obj = (float)NumericValue;
        Assert.IsTrue(Marshal_ByRef_Single(ref obj, NumericValue));
        
        obj = (double)NumericValue;
        Assert.IsTrue(Marshal_ByRef_Double(ref obj, NumericValue));
        
        obj = StringValue;
        Assert.IsTrue(Marshal_ByRef_String(ref obj, StringValue));
        
        obj = CharValue;
        Assert.IsTrue(Marshal_ByRef_Char(ref obj, CharValue));
        
        obj = true;
        Assert.IsTrue(Marshal_ByRef_Boolean(ref obj, true));
        
        obj = DateValue;
        Assert.IsTrue(Marshal_ByRef_DateTime(ref obj, DateValue));
        
        obj = DecimalValue;
        Assert.IsTrue(Marshal_ByRef_Decimal(ref obj, DecimalValue));
        
        obj = DBNull.Value;
        Assert.IsTrue(Marshal_ByRef_Null(ref obj));
        
        obj = System.Reflection.Missing.Value;
        Assert.IsTrue(Marshal_ByRef_Missing(ref obj));
        
        obj = null;
        Assert.IsTrue(Marshal_ByRef_Empty(ref obj));
        
        obj = new object();
        Assert.IsTrue(Marshal_ByRef_Object(ref obj));

        obj = DecimalValue;

        Assert.IsTrue(Marshal_ChangeVariantType(ref obj, NumericValue));

        Assert.IsTrue(obj is int);

        Assert.AreEqual(NumericValue, (int)obj);
    }

    private unsafe static void TestOut()
    {
        Assert.IsTrue(Marshal_Out(out object obj, NumericValue));

        Assert.IsTrue(obj is int);
        Assert.AreEqual(NumericValue, (int)obj);
    }

    
    private unsafe static void TestFieldByValue()
    {
        ObjectWrapper wrapper = new ObjectWrapper();

        wrapper.value = (byte)NumericValue;
        Assert.IsTrue(Marshal_Struct_ByValue_Byte(wrapper, NumericValue));
        
        wrapper.value = (sbyte)NumericValue;
        Assert.IsTrue(Marshal_Struct_ByValue_SByte(wrapper, (sbyte)NumericValue));
        
        wrapper.value = (short)NumericValue;
        Assert.IsTrue(Marshal_Struct_ByValue_Int16(wrapper, NumericValue));
        
        wrapper.value = (ushort)NumericValue;
        Assert.IsTrue(Marshal_Struct_ByValue_UInt16(wrapper, NumericValue));
        
        wrapper.value = (int)NumericValue;
        Assert.IsTrue(Marshal_Struct_ByValue_Int32(wrapper, NumericValue));
        
        wrapper.value = (uint)NumericValue;
        Assert.IsTrue(Marshal_Struct_ByValue_UInt32(wrapper, NumericValue));
        
        wrapper.value = (long)NumericValue;
        Assert.IsTrue(Marshal_Struct_ByValue_Int64(wrapper, NumericValue));
        
        wrapper.value = (ulong)NumericValue;
        Assert.IsTrue(Marshal_Struct_ByValue_UInt64(wrapper, NumericValue));
        
        wrapper.value = (float)NumericValue;
        Assert.IsTrue(Marshal_Struct_ByValue_Single(wrapper, NumericValue));
        
        wrapper.value = (double)NumericValue;
        Assert.IsTrue(Marshal_Struct_ByValue_Double(wrapper, NumericValue));
        
        wrapper.value = StringValue;
        Assert.IsTrue(Marshal_Struct_ByValue_String(wrapper, StringValue));
        
        wrapper.value = CharValue;
        Assert.IsTrue(Marshal_Struct_ByValue_Char(wrapper, CharValue));
        
        wrapper.value = true;
        Assert.IsTrue(Marshal_Struct_ByValue_Boolean(wrapper, true));
        
        wrapper.value = DateValue;
        Assert.IsTrue(Marshal_Struct_ByValue_DateTime(wrapper, DateValue));
        
        wrapper.value = DecimalValue;
        Assert.IsTrue(Marshal_Struct_ByValue_Decimal(wrapper, DecimalValue));
        
        wrapper.value = DBNull.Value;
        Assert.IsTrue(Marshal_Struct_ByValue_Null(wrapper));
        
        wrapper.value = System.Reflection.Missing.Value;
        Assert.IsTrue(Marshal_Struct_ByValue_Missing(wrapper));
        
        wrapper.value = null;
        Assert.IsTrue(Marshal_Struct_ByValue_Empty(wrapper));
        
        wrapper.value = new object();
        Assert.IsTrue(Marshal_Struct_ByValue_Object(wrapper));
    }

    
    private unsafe static void TestFieldByRef()
    {
        ObjectWrapper wrapper = new ObjectWrapper();

        wrapper.value = (byte)NumericValue;
        Assert.IsTrue(Marshal_Struct_ByRef_Byte(ref wrapper, NumericValue));
        
        wrapper.value = (sbyte)NumericValue;
        Assert.IsTrue(Marshal_Struct_ByRef_SByte(ref wrapper, (sbyte)NumericValue));
        
        wrapper.value = (short)NumericValue;
        Assert.IsTrue(Marshal_Struct_ByRef_Int16(ref wrapper, NumericValue));
        
        wrapper.value = (ushort)NumericValue;
        Assert.IsTrue(Marshal_Struct_ByRef_UInt16(ref wrapper, NumericValue));
        
        wrapper.value = (int)NumericValue;
        Assert.IsTrue(Marshal_Struct_ByRef_Int32(ref wrapper, NumericValue));
        
        wrapper.value = (uint)NumericValue;
        Assert.IsTrue(Marshal_Struct_ByRef_UInt32(ref wrapper, NumericValue));
        
        wrapper.value = (long)NumericValue;
        Assert.IsTrue(Marshal_Struct_ByRef_Int64(ref wrapper, NumericValue));
        
        wrapper.value = (ulong)NumericValue;
        Assert.IsTrue(Marshal_Struct_ByRef_UInt64(ref wrapper, NumericValue));
        
        wrapper.value = (float)NumericValue;
        Assert.IsTrue(Marshal_Struct_ByRef_Single(ref wrapper, NumericValue));
        
        wrapper.value = (double)NumericValue;
        Assert.IsTrue(Marshal_Struct_ByRef_Double(ref wrapper, NumericValue));
        
        wrapper.value = StringValue;
        Assert.IsTrue(Marshal_Struct_ByRef_String(ref wrapper, StringValue));
        
        wrapper.value = CharValue;
        Assert.IsTrue(Marshal_Struct_ByRef_Char(ref wrapper, CharValue));
        
        wrapper.value = true;
        Assert.IsTrue(Marshal_Struct_ByRef_Boolean(ref wrapper, true));
        
        wrapper.value = DateValue;
        Assert.IsTrue(Marshal_Struct_ByRef_DateTime(ref wrapper, DateValue));
        
        wrapper.value = DecimalValue;
        Assert.IsTrue(Marshal_Struct_ByRef_Decimal(ref wrapper, DecimalValue));
        
        wrapper.value = DBNull.Value;
        Assert.IsTrue(Marshal_Struct_ByRef_Null(ref wrapper));
        
        wrapper.value = System.Reflection.Missing.Value;
        Assert.IsTrue(Marshal_Struct_ByRef_Missing(ref wrapper));
        
        wrapper.value = null;
        Assert.IsTrue(Marshal_Struct_ByRef_Empty(ref wrapper));
        
        wrapper.value = new object();
        Assert.IsTrue(Marshal_Struct_ByRef_Object(ref wrapper));
    }

    public static int Main()
    {
        try
        {
            TestByValue();
            TestByRef();
            TestOut();
            TestFieldByValue();
            TestFieldByRef();
        }
        catch (System.Exception e)
        {
            Console.WriteLine($"Test failed: {e.ToString()}");
            return 101;
        }
        return 100;
    }
}
