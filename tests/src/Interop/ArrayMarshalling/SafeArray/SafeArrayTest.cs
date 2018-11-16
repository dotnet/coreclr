// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Threading;
using System.Runtime.InteropServices;

#pragma warning disable CS0612, CS0618

public class Tester
{
}

class SafeArrayNative
{
    public struct StructWithSafeArray
    {
        [MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_BOOL)]
        public bool[] values;
    }

    public struct BlittableRecord
    {
        public int a;
    }

    public struct NonBlittableRecord
    {
        public bool b;
    }

    [DllImport(nameof(SafeArrayNative))]
    public static extern bool XorBoolArray(
        [MarshalAs(UnmanagedType.SafeArray)] bool[] values,
        out bool result
    );

    [DllImport(nameof(SafeArrayNative))]
    public static extern bool MeanDecimalArray(
        [MarshalAs(UnmanagedType.SafeArray)] decimal[] values,
        out decimal result
    );

    [DllImport(nameof(SafeArrayNative))]
    public static extern bool SumCurrencyArray(
        [MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_CURRENCY)] decimal[] values,
        [MarshalAs(UnmanagedType.Currency)] out decimal result
    );

    [DllImport(nameof(SafeArrayNative), EntryPoint = "ReverseStrings")]
    public static extern bool ReverseStringsAnsi(
        [MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_LPSTR), In, Out] string[] strings
    );

    [DllImport(nameof(SafeArrayNative), EntryPoint = "ReverseStrings")]
    public static extern bool ReverseStringsUnicode(
        [MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_LPWSTR), In, Out] string[] strings
    );

    [DllImport(nameof(SafeArrayNative), EntryPoint = "ReverseStrings")]
    public static extern bool ReverseStringsBSTR(
        [MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_BSTR), In, Out] string[] strings
    );

    [DllImport(nameof(SafeArrayNative))]
    public static extern bool MeanBlittableIntRecords(
        [MarshalAs(UnmanagedType.SafeArray)] BlittableRecord[] records,
        out int result
    );

    [DllImport(nameof(SafeArrayNative))]
    public static extern bool XorNonBlittableBoolRecords(
        [MarshalAs(UnmanagedType.SafeArray)] NonBlittableRecord[] records,
        out bool result
    );

    [DllImport(nameof(SafeArrayNative), EntryPoint = "VerifyInterfaceArray")]
    private static extern bool VerifyInterfaceArray(
        [MarshalAs(UnmanagedType.SafeArray)] object[] objects,
        short expectedVarType
    );

    public static bool VerifyIUnknownArray(object[] objects)
    {
        var wrappers = new object[objects.Length];

        for (int i = 0; i < wrappers.Length; i++)
        {
            wrappers[i] = new UnknownWrapper(objects[i]);
        }

        VerifyInterfaceArray(wrappers, (short)VarEnum.VT_UNKNOWN);
    }

    public static bool VerifyIDispatchArray(object[] objects)
    {
        VerifyInterfaceArray(objects, (short)VarEnum.VT_DISPATCH);
    }

    [DllImport(nameof(SafeArrayNative))]
    public static extern bool MeanVariantIntArray(
        [MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_VARIANT)]
        object[] objects,
        out int result
    );

    [DllImport(nameof(SafeArrayNative))]
    public static extern bool DistanceBetweenDates(
        [MarshalAs(UnmanagedType.SafeArray)] DateTime[] dates,
        out double result
    );

    [DllImport(nameof(SafeArrayNative))]
    public static extern bool XorBoolArrayInStruct(StructWithSafeArray str, out bool result);
}
