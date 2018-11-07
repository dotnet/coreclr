// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Runtime.InteropServices;
using TestLibrary;

class Test
{
    private const string NativeLibrary = "VariantNative";

    private const byte NumericValue = 15;

    private const char CharValue = 'z';

    private const string StringValue = "Abcdefg";

    private const decimal DecimalValue = 74.25;

    private static readonly DateTime DateValue = new DateTime(2018, 11, 6);

    private struct CustomStruct
    {

    }

    private struct ObjectWrapper
    {
#pragma warning disable CS0612, CS0618
        [MarshalAs(UnmanagedType.Struct)]
        public object value;
#pragma warning restore CS0612, CS0618
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
    [DllImport(NativeLibrary)]
    private static extern bool Marshal_ByValue_String(object obj, String expected);
    [DllImport(NativeLibrary)]
    private static extern bool Marshal_ByValue_Char(object obj, char expected);
    [DllImport(NativeLibrary)]
    private static extern bool Marshal_ByValue_Boolean(object obj, bool expected);
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
    private static extern bool Marshal_ByRef(ref object obj, ExpectedVariantType type);
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
    [DllImport(NativeLibrary)]
    private static extern bool Marshal_ByRef_String(ref object obj, String expected);
    [DllImport(NativeLibrary)]
    private static extern bool Marshal_ByRef_Char(ref object obj, char expected);
    [DllImport(NativeLibrary)]
    private static extern bool Marshal_ByRef_Boolean(ref object obj, bool expected);
    [DllImport(NativeLibrary)]
    private static extern bool Marshal_ByRef_DateTime(ref object obj, DateTime expected);
    [DllImport(NativeLibrary)]
    private static extern bool Marshal_ByRef_Decimal(ref object obj, decimal expected);

    [DllImport(NativeLibrary)]
    private static extern bool Marshal_ChangeVariantType(ref object obj);

    [DllImport(NativeLibrary)]
    private static extern bool Marshal_Out(out object obj);

    [DllImport(NativeLibrary)]
    private static extern bool Marshal_Struct_ByValue(ObjectWrapper wrapper, ExpectedVariantType type);

    [DllImport(NativeLibrary)]
    private static extern bool Marshal_Struct_ByRef(ref ObjectWrapper wrapper, ExpectedVariantType type);

    private unsafe static void TestByValue()
    {
        Assert.IsTrue(Marshal_ByValue_Byte((byte)NumericValue, NumericValue));
        Assert.IsTrue(Marshal_ByValue_SByte((sbyte)NumericValue, NumericValue));
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
        Assert.IsTrue(Marshal_ByValue_Boolean(true));
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
        Assert.IsTrue(Marshal_ByRef(ref obj, ExpectedVariantType.Byte));
        
        obj = (sbyte)NumericValue; 
        Assert.IsTrue(Marshal_ByRef(ref obj, ExpectedVariantType.SByte));
        
        obj = (short)NumericValue; 
        Assert.IsTrue(Marshal_ByRef(ref obj, ExpectedVariantType.Int16));
        
        obj = (ushort)NumericValue; 
        Assert.IsTrue(Marshal_ByRef(ref obj, ExpectedVariantType.UInt16));
        
        obj = (int)NumericValue; 
        Assert.IsTrue(Marshal_ByRef(ref obj, ExpectedVariantType.Int32));
        
        obj = (uint)NumericValue; 
        Assert.IsTrue(Marshal_ByRef(ref obj, ExpectedVariantType.UInt32));
        
        obj = (long)NumericValue; 
        Assert.IsTrue(Marshal_ByRef(ref obj, ExpectedVariantType.Int64));
        
        obj = (ulong)NumericValue; 
        Assert.IsTrue(Marshal_ByRef(ref obj, ExpectedVariantType.UInt64));
        
        obj = (float)NumericValue; 
        Assert.IsTrue(Marshal_ByRef(ref obj, ExpectedVariantType.Single));
        
        obj = (double)NumericValue; 
        Assert.IsTrue(Marshal_ByRef(ref obj, ExpectedVariantType.Double));
        
        obj = StringValue;
        Assert.IsTrue(Marshal_ByRef(ref obj, ExpectedVariantType.String));
        
        obj = CharValue;
        Assert.IsTrue(Marshal_ByRef(ref obj, ExpectedVariantType.Char));
        
        obj = true;
        Assert.IsTrue(Marshal_ByRef(ref obj, ExpectedVariantType.Boolean));
        
        obj = new DateTime(2018, 11, 6);
        Assert.IsTrue(Marshal_ByRef(ref obj, ExpectedVariantType.DateTime));
        
        obj = DecimalValue;
        Assert.IsTrue(Marshal_ByRef(ref obj, ExpectedVariantType.Decimal));
        
        obj = DBNull.Value;
        Assert.IsTrue(Marshal_ByRef(ref obj, ExpectedVariantType.Null));
        
        obj = System.Reflection.Missing.Value;
        Assert.IsTrue(Marshal_ByRef(ref obj, ExpectedVariantType.Missing));
        
        obj = null;
        Assert.IsTrue(Marshal_ByRef(ref obj, ExpectedVariantType.Empty));
        
        obj = new object();
        Assert.IsTrue(Marshal_ByRef(ref obj, ExpectedVariantType.Object));

        obj = DecimalValue;

        Assert.IsTrue(Marshal_ChangeVariantType(ref obj));

        Assert.IsTrue(obj is int);

        Assert.AreEqual(NumericValue, (int)obj);
    }

    private unsafe static void TestOut()
    {
        Assert.IsTrue(Marshal_Out(out object obj));

        Assert.IsTrue(obj is int);
        Assert.AreEqual(NumericValue, (int)obj);
    }

    
    private unsafe static void TestFieldByValue()
    {
        ObjectWrapper wrapper = new ObjectWrapper();

        wrapper.value = (byte)NumericValue; 
        Assert.IsTrue(Marshal_Struct_ByValue(wrapper, ExpectedVariantType.Byte));
        
        wrapper.value = (sbyte)NumericValue; 
        Assert.IsTrue(Marshal_Struct_ByValue(wrapper, ExpectedVariantType.SByte));
        
        wrapper.value = (short)NumericValue; 
        Assert.IsTrue(Marshal_Struct_ByValue(wrapper, ExpectedVariantType.Int16));
        
        wrapper.value = (ushort)NumericValue; 
        Assert.IsTrue(Marshal_Struct_ByValue(wrapper, ExpectedVariantType.UInt16));
        
        wrapper.value = (int)NumericValue; 
        Assert.IsTrue(Marshal_Struct_ByValue(wrapper, ExpectedVariantType.Int32));
        
        wrapper.value = (uint)NumericValue; 
        Assert.IsTrue(Marshal_Struct_ByValue(wrapper, ExpectedVariantType.UInt32));
        
        wrapper.value = (long)NumericValue; 
        Assert.IsTrue(Marshal_Struct_ByValue(wrapper, ExpectedVariantType.Int64));
        
        wrapper.value = (ulong)NumericValue; 
        Assert.IsTrue(Marshal_Struct_ByValue(wrapper, ExpectedVariantType.UInt64));
        
        wrapper.value = (float)NumericValue; 
        Assert.IsTrue(Marshal_Struct_ByValue(wrapper, ExpectedVariantType.Single));
        
        wrapper.value = (double)NumericValue; 
        Assert.IsTrue(Marshal_Struct_ByValue(wrapper, ExpectedVariantType.Double));
        
        wrapper.value = StringValue;
        Assert.IsTrue(Marshal_Struct_ByValue(wrapper, ExpectedVariantType.String));
        
        wrapper.value = CharValue;
        Assert.IsTrue(Marshal_Struct_ByValue(wrapper, ExpectedVariantType.Char));
        
        wrapper.value = true;
        Assert.IsTrue(Marshal_Struct_ByValue(wrapper, ExpectedVariantType.Boolean));
        
        wrapper.value = new DateTime(2018, 11, 6);
        Assert.IsTrue(Marshal_Struct_ByValue(wrapper, ExpectedVariantType.DateTime));
        
        wrapper.value = DecimalValue;
        Assert.IsTrue(Marshal_Struct_ByValue(wrapper, ExpectedVariantType.Decimal));
        
        wrapper.value = DBNull.Value;
        Assert.IsTrue(Marshal_Struct_ByValue(wrapper, ExpectedVariantType.Null));
        
        wrapper.value = System.Reflection.Missing.Value;
        Assert.IsTrue(Marshal_Struct_ByValue(wrapper, ExpectedVariantType.Missing));
        
        wrapper.value = null;
        Assert.IsTrue(Marshal_Struct_ByValue(wrapper, ExpectedVariantType.Empty));
        
        wrapper.value = new object();
        Assert.IsTrue(Marshal_Struct_ByValue(wrapper, ExpectedVariantType.Object));
    }

    
    private unsafe static void TestFieldByRef()
    {
        ObjectWrapper wrapper = new ObjectWrapper();

        wrapper.value = (byte)NumericValue; 
        Assert.IsTrue(Marshal_Struct_ByRef(ref wrapper, ExpectedVariantType.Byte));
        
        wrapper.value = (sbyte)NumericValue; 
        Assert.IsTrue(Marshal_Struct_ByRef(ref wrapper, ExpectedVariantType.SByte));
        
        wrapper.value = (short)NumericValue; 
        Assert.IsTrue(Marshal_Struct_ByRef(ref wrapper, ExpectedVariantType.Int16));
        
        wrapper.value = (ushort)NumericValue; 
        Assert.IsTrue(Marshal_Struct_ByRef(ref wrapper, ExpectedVariantType.UInt16));
        
        wrapper.value = (int)NumericValue; 
        Assert.IsTrue(Marshal_Struct_ByRef(ref wrapper, ExpectedVariantType.Int32));
        
        wrapper.value = (uint)NumericValue; 
        Assert.IsTrue(Marshal_Struct_ByRef(ref wrapper, ExpectedVariantType.UInt32));
        
        wrapper.value = (long)NumericValue; 
        Assert.IsTrue(Marshal_Struct_ByRef(ref wrapper, ExpectedVariantType.Int64));
        
        wrapper.value = (ulong)NumericValue; 
        Assert.IsTrue(Marshal_Struct_ByRef(ref wrapper, ExpectedVariantType.UInt64));
        
        wrapper.value = (float)NumericValue; 
        Assert.IsTrue(Marshal_Struct_ByRef(ref wrapper, ExpectedVariantType.Single));
        
        wrapper.value = (double)NumericValue; 
        Assert.IsTrue(Marshal_Struct_ByRef(ref wrapper, ExpectedVariantType.Double));
        
        wrapper.value = StringValue;
        Assert.IsTrue(Marshal_Struct_ByRef(ref wrapper, ExpectedVariantType.String));
        
        wrapper.value = CharValue;
        Assert.IsTrue(Marshal_Struct_ByRef(ref wrapper, ExpectedVariantType.Char));
        
        wrapper.value = true;
        Assert.IsTrue(Marshal_Struct_ByRef(ref wrapper, ExpectedVariantType.Boolean));
        
        wrapper.value = new DateTime(2018, 11, 6);
        Assert.IsTrue(Marshal_Struct_ByRef(ref wrapper, ExpectedVariantType.DateTime));
        
        wrapper.value = DecimalValue;
        Assert.IsTrue(Marshal_Struct_ByRef(ref wrapper, ExpectedVariantType.Decimal));
        
        wrapper.value = DBNull.Value;
        Assert.IsTrue(Marshal_Struct_ByRef(ref wrapper, ExpectedVariantType.Null));
        
        wrapper.value = System.Reflection.Missing.Value;
        Assert.IsTrue(Marshal_Struct_ByRef(ref wrapper, ExpectedVariantType.Missing));
        
        wrapper.value = null;
        Assert.IsTrue(Marshal_Struct_ByRef(ref wrapper, ExpectedVariantType.Empty));
        
        wrapper.value = new object();
        Assert.IsTrue(Marshal_Struct_ByRef(ref wrapper, ExpectedVariantType.Object));
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
