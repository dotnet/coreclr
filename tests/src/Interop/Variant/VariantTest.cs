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

    private const decimal DecimalValue = decimal.MaxValue;

    private struct CustomStruct
    {

    }

    [DllImport(NativeLibrary)]
    private static extern bool Marshal_ByValue_Byte(object obj);
    [DllImport(NativeLibrary)]
    private static extern bool Marshal_ByValue_SByte(object obj);
    [DllImport(NativeLibrary)]
    private static extern bool Marshal_ByValue_Int16(object obj);
    [DllImport(NativeLibrary)]
    private static extern bool Marshal_ByValue_UInt16(object obj);
    [DllImport(NativeLibrary)]
    private static extern bool Marshal_ByValue_Int32(object obj);
    [DllImport(NativeLibrary)]
    private static extern bool Marshal_ByValue_UInt32(object obj);
    [DllImport(NativeLibrary)]
    private static extern bool Marshal_ByValue_Int64(object obj);
    [DllImport(NativeLibrary)]
    private static extern bool Marshal_ByValue_UInt64(object obj);
    [DllImport(NativeLibrary)]
    private static extern bool Marshal_ByValue_Single(object obj);
    [DllImport(NativeLibrary)]
    private static extern bool Marshal_ByValue_Double(object obj);
    [DllImport(NativeLibrary)]
    private static extern bool Marshal_ByValue_String(object obj);
    [DllImport(NativeLibrary)]
    private static extern bool Marshal_ByValue_Char(object obj);
    [DllImport(NativeLibrary)]
    private static extern bool Marshal_ByValue_Boolean(object obj);
    [DllImport(NativeLibrary)]
    private static extern bool Marshal_ByValue_DateTime(object obj);
    [DllImport(NativeLibrary)]
    private static extern bool Marshal_ByValue_Decimal(object obj);
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

    private unsafe static void TestByValue()
    {
        Assert.IsTrue(Marshal_ByValue_Byte((byte)NumericValue));
        Assert.IsTrue(Marshal_ByValue_SByte((sbyte)NumericValue));
        Assert.IsTrue(Marshal_ByValue_Int16((short)NumericValue));
        Assert.IsTrue(Marshal_ByValue_UInt16((ushort)NumericValue));
        Assert.IsTrue(Marshal_ByValue_Int32((int)NumericValue));
        Assert.IsTrue(Marshal_ByValue_UInt32((uint)NumericValue));
        Assert.IsTrue(Marshal_ByValue_Int64((long)NumericValue));
        Assert.IsTrue(Marshal_ByValue_UInt64((ulong)NumericValue));
        Assert.IsTrue(Marshal_ByValue_Single((float)NumericValue));
        Assert.IsTrue(Marshal_ByValue_Double((double)NumericValue));
        Assert.IsTrue(Marshal_ByValue_String(StringValue));
        Assert.IsTrue(Marshal_ByValue_Char(CharValue));
        Assert.IsTrue(Marshal_ByValue_Boolean(true));
        Assert.IsTrue(Marshal_ByValue_DateTime(new DateTime(2018, 11, 6)));
        Assert.Throws<ArgumentException>(() => Marshal_ByValue_Invalid(TimeSpan.Zero));
        Assert.IsTrue(Marshal_ByValue_Decimal((decimal)DecimalValue));
        Assert.IsTrue(Marshal_ByValue_Null(DBNull.Value));
        Assert.IsTrue(Marshal_ByValue_Missing(System.Reflection.Missing.Value));
        Assert.IsTrue(Marshal_ByValue_Empty(null));
        Assert.IsTrue(Marshal_ByValue_Object(new object()));
        Assert.Throws<NotSupportedException>(() => Marshal_ByValue_Invalid(new CustomStruct()));
    }

    public static int Main()
    {
        try
        {
            TestByValue();
        }
        catch (System.Exception e)
        {
            Console.WriteLine($"Test failed: {e.ToString()}");
            return 101;
        }
        return 100;
    }
}
