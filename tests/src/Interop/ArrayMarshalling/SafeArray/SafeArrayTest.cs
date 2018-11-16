// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Linq;
using System.Threading;
using System.Runtime.InteropServices;
using TestLibrary;

#pragma warning disable CS0612, CS0618

public class Tester
{
    public static int Main()
    {
        try
        {
            var boolArray = new bool[] { true, false, true, false, false, true };
            Assert.IsTrue(SafeArrayNative.XorBoolArray(boolArray, out var xorResult));
            Assert.AreEqual(XorArray(boolArray), xorResult);

            var decimalArray = new decimal[] { 1.5M, 30.2M, 6432M, 12.5832M };
            Assert.IsTrue(SafeArrayNative.MeanDecimalArray(decimalArray, out var meanDecimalValue));
            Assert.AreEqual(decimalArray.Average(), meanDecimalValue);

            Assert.IsTrue(SafeArrayNative.SumCurrencyArray(decimalArray, out var sumCurrencyValue));
            Assert.AreEqual(decimalArray.Sum(), sumCurrencyValue);

            var strings = new [] {"ABCDE", "12345", "Microsoft"};
            var reversedStrings = strings.Select(str => Reverse(str)).ToArray();

            var ansiTest = strings.ToArray();
            Assert.IsTrue(SafeArrayNative.ReverseStringsAnsi(ansiTest));
            Assert.AreAllEqual(reversedStrings, ansiTest);

            var unicodeTest = strings.ToArray();
            Assert.IsTrue(SafeArrayNative.ReverseStringsUnicode(unicodeTest));
            Assert.AreAllEqual(reversedStrings, unicodeTest);

            var bstrTest = strings.ToArray();
            Assert.IsTrue(SafeArrayNative.ReverseStringsBSTR(bstrTest));
            Assert.AreAllEqual(reversedStrings, bstrTest);

            // var blittableRecords = new SafeArrayNative.BlittableRecord[]
            // {
            //     new SafeArrayNative.BlittableRecord { a = 1 },
            //     new SafeArrayNative.BlittableRecord { a = 5 },
            //     new SafeArrayNative.BlittableRecord { a = 7 },
            //     new SafeArrayNative.BlittableRecord { a = 3 },
            //     new SafeArrayNative.BlittableRecord { a = 9 },
            //     new SafeArrayNative.BlittableRecord { a = 15 },
            // };

            // Assert.IsTrue(SafeArrayNative.MeanBlittableIntRecords(blittableRecords, out var blittableMean));
            // Assert.AreEqual(blittableRecords.Aggregate(0, (sum, record) => sum += record.a) / blittableRecords.Length, blittableMean);

            // var nonBlittableRecords = boolArray.Select(b => new SafeArrayNative.NonBlittableRecord{ b = b }).ToArray();
            // Assert.IsTrue(SafeArrayNative.XorNonBlittableBoolRecords(nonBlittableRecords, out var nonBlittableXor));
            // Assert.AreEqual(XorArray(boolArray), nonBlittableXor);

            var objects = new object[] { new object(), new object(), new object() };
            Assert.IsTrue(SafeArrayNative.VerifyIUnknownArray(objects));
            Assert.IsTrue(SafeArrayNative.VerifyIDispatchArray(objects));

            var variantInts = new object[] {1, 2, 3, 4, 5, 6, 7, 8, 9};
            
            Assert.IsTrue(SafeArrayNative.MeanVariantIntArray(variantInts, out var variantMean));
            Assert.AreEqual(variantInts.OfType<int>().Average(), variantMean);

            var dates = new DateTime[] { new DateTime(2008, 5, 1), new DateTime(2010, 1, 1) };
            Assert.IsTrue(SafeArrayNative.DistanceBetweenDates(dates, out var numDays));
            Assert.AreEqual((dates[1] - dates[0]).TotalDays, numDays);

            Assert.IsTrue(SafeArrayNative.XorBoolArrayInStruct(
                new SafeArrayNative.StructWithSafeArray
                {
                    values = boolArray
                },
                out var structXor));

            Assert.AreEqual(XorArray(boolArray), structXor);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return 101;
        }
        return 100;
    }

    private static bool XorArray(bool[] values)
    {
        bool retVal = false;
        foreach (var item in values)
        {
            retVal ^= item;
        }
        return retVal;
    }

    private static string Reverse(string s)
    {
        var chars = s.ToCharArray();
        Array.Reverse(chars);
        return new string(chars);
    }
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
        [MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_CY)] decimal[] values,
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
        [MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_RECORD)] BlittableRecord[] records,
        out int result
    );

    [DllImport(nameof(SafeArrayNative))]
    public static extern bool XorNonBlittableBoolRecords(
        [MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_RECORD)] NonBlittableRecord[] records,
        out bool result
    );

    [DllImport(nameof(SafeArrayNative), EntryPoint = "VerifyInterfaceArray")]
    private static extern bool VerifyInterfaceArrayIUnknown(
        [MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_UNKNOWN)] object[] objects,
        short expectedVarType
    );

    [DllImport(nameof(SafeArrayNative), EntryPoint = "VerifyInterfaceArray")]
    private static extern bool VerifyInterfaceArrayIDispatch(
        [MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_DISPATCH)] object[] objects,
        short expectedVarType
    );

    public static bool VerifyIUnknownArray(object[] objects)
    {
        return VerifyInterfaceArrayIUnknown(objects, (short)VarEnum.VT_UNKNOWN);
    }

    public static bool VerifyIDispatchArray(object[] objects)
    {
        return VerifyInterfaceArrayIDispatch(objects, (short)VarEnum.VT_DISPATCH);
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
