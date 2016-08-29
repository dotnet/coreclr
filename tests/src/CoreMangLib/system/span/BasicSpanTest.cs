// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;

class ReferenceType
{
    internal byte Value;
    public ReferenceType(byte value) { Value = value; }
}

struct ValueTypeWithoutPointers
{
    internal byte Value;
    public ValueTypeWithoutPointers(byte value) { Value = value; }
}

struct ValueTypeWithPointers
{
    internal object Reference;
    public ValueTypeWithPointers(object reference) { Reference = reference; }
}

class My
{
    static int Sum(Span<int> span)
    {
        int sum = 0;
        for (int i = 0; i < span.Length; i++)
            sum += span[i];
        return sum;
    }

    static void Main()
    {
        int failedTestsCount = 0;
        Test(CanAccessItemsViaIndexer, "CanAccessItemsViaIndexer", ref failedTestsCount);

        Test(ReferenceTypesAreSupported, "ReferenceTypesAreSupported", ref failedTestsCount);

        Test(CanUpdateUnderlyingArray, "CanUpdateUnderlyingArray", ref failedTestsCount);

        Test(TestArrayCoVariance, "TestArrayCoVariance", ref failedTestsCount);
        Test(TestArrayCoVarianceReadOnly, "TestArrayCoVarianceReadOnly", ref failedTestsCount);

        Test(CanCopyValueTypesWithoutPointersToSlice, "CanCopyValueTypesWithoutPointersToSlice", ref failedTestsCount);
        Test(CanCopyValueTypesWithoutPointersToArray, "CanCopyValueTypesWithoutPointersToArray", ref failedTestsCount);

        Test(CanCopyReferenceTypesToSlice, "CanCopyReferenceTypesToSlice", ref failedTestsCount);
        Test(CanCopyReferenceTypesToArray, "CanCopyReferenceTypesToArray", ref failedTestsCount);

        Test(CanCopyValueTypesWithPointersToSlice, "CanCopyValueTypesWithPointersToSlice", ref failedTestsCount);
        Test(CanCopyValueTypesWithPointersToArray, "CanCopyValueTypesWithPointersToArray", ref failedTestsCount);

        Test(CanCopyValueTypesWithoutPointersToUnmanagedMemory, "CanCopyValueTypesWithoutPointersToUnmanagedMemory", ref failedTestsCount);
        Test(MustNotCopyValueTypesWithPointersToUnmanagedMemory, "MustNotCopyValueTypesWithPointersToUnmanagedMemory", ref failedTestsCount);

        Console.WriteLine(string.Format("{0} tests has failed", failedTestsCount));
        Environment.Exit(failedTestsCount);
    }

    private static void CanAccessItemsViaIndexer()
    {
        int[] a = new int[] { 1, 2, 3 };
        Span<int> slice = new Span<int>(a);
        AssertTrue(Sum(slice) == 6, "Failed to sum slice");
        
        Span<int> subslice = slice.Slice(1, 2);
        AssertTrue(Sum(subslice) == 5, "Failed to sum subslice");
    }

    private static void ReferenceTypesAreSupported()
    {
        var underlyingArray = new ReferenceType[] { new ReferenceType(0), new ReferenceType(1), new ReferenceType(2) };
        var slice = new Span<ReferenceType>(underlyingArray);

        for (int i = 0; i < underlyingArray.Length; i++)
        {
            AssertTrue(underlyingArray[i].Value == slice[i].Value, "Values are different");
            AssertTrue(object.ReferenceEquals(underlyingArray[i], slice[i]), "References are broken");
        }
    }

    static void TestArrayCoVariance()
    {
        var array = new ReferenceType[1];
        var objArray = (object[])array;
        try
        {
            new Span<object>(objArray);
            AssertTrue(false, "Expected exception not thrown");
        }
        catch (ArrayTypeMismatchException)
        {
        }

        var objEmptyArray = Array.Empty<ReferenceType>();
        try
        {
            new Span<object>(objEmptyArray);
            AssertTrue(false, "Expected exception not thrown");
        }
        catch (ArrayTypeMismatchException)
        {
        }
    }

    static void TestArrayCoVarianceReadOnly()
    {
        var array = new ReferenceType[1];
        var objArray = (object[])array;
        AssertTrue(new ReadOnlySpan<object>(objArray).Length == 1, "Unexpected length");

        var objEmptyArray = Array.Empty<ReferenceType>();
        AssertTrue(new ReadOnlySpan<object>(objEmptyArray).Length == 0, "Unexpected length");
   }

    private static void CanUpdateUnderlyingArray()
    {
        var underlyingArray = new int[] { 1, 2, 3 };
        var slice = new Span<int>(underlyingArray);

        slice[0] = 0;
        slice[1] = 1;
        slice[2] = 2;

        AssertTrue(underlyingArray[0] == 0, "Failed to update underlying array");
        AssertTrue(underlyingArray[1] == 1, "Failed to update underlying array");
        AssertTrue(underlyingArray[2] == 2, "Failed to update underlying array");
    }

    private static void CanCopyValueTypesWithoutPointersToSlice()
    {
        var source = new Span<ValueTypeWithoutPointers>(
            new[]
            {
                new ValueTypeWithoutPointers(0),
                new ValueTypeWithoutPointers(1),
                new ValueTypeWithoutPointers(2),
                new ValueTypeWithoutPointers(3)
            });
        var underlyingArray = new ValueTypeWithoutPointers[4];
        var slice = new Span<ValueTypeWithoutPointers>(underlyingArray);

        var result = source.TryCopyTo(slice);

        AssertTrue(result, "Failed to copy value types without pointers");
        for (int i = 0; i < 4; i++)
        {
            AssertTrue(source[i].Value == slice[i].Value, "Failed to copy value types without pointers, values were not equal");
            AssertTrue(source[i].Value == underlyingArray[i].Value, "Failed to copy value types without pointers to underlying array, values were not equal");
        }
    }

    private static void CanCopyValueTypesWithoutPointersToArray()
    {
        var source = new Span<ValueTypeWithoutPointers>(
            new[]
            {
                new ValueTypeWithoutPointers(0),
                new ValueTypeWithoutPointers(1),
                new ValueTypeWithoutPointers(2),
                new ValueTypeWithoutPointers(3)
            });
        var array = new ValueTypeWithoutPointers[4];

        var result = source.TryCopyTo(array);

        AssertTrue(result, "Failed to copy value types without pointers");
        for (int i = 0; i < 4; i++)
        {
            AssertTrue(source[i].Value == array[i].Value, "Failed to copy value types without pointers, values were not equal");
        }
    }


    private static void CanCopyReferenceTypesToSlice()
    {
        var source = new Span<ReferenceType>(
            new[]
            {
                    new ReferenceType(0),
                    new ReferenceType(1),
                    new ReferenceType(2),
                    new ReferenceType(3)
            });
        var underlyingArray = new ReferenceType[4];
        var slice = new Span<ReferenceType>(underlyingArray);

        var result = source.TryCopyTo(slice);

        AssertTrue(result, "Failed to copy reference types");
        for (int i = 0; i < 4; i++)
        {
            AssertTrue(source[i] != null && slice[i] != null, "Failed to copy reference types, references were null");
            AssertTrue(object.ReferenceEquals(source[i], slice[i]), "Failed to copy reference types, references were not equal");
            AssertTrue(source[i].Value == slice[i].Value, "Failed to copy reference types, values were not equal");

            AssertTrue(underlyingArray[i] != null, "Failed to copy reference types to underlying array, references were null");
            AssertTrue(object.ReferenceEquals(source[i], underlyingArray[i]), "Failed to copy reference types to underlying array, references were not equal");
            AssertTrue(source[i].Value == underlyingArray[i].Value, "Failed to copy reference types to underlying array, values were not equal");
        }
    }

    private static void CanCopyReferenceTypesToArray()
    {
        var source = new Span<ReferenceType>(
            new[]
            {
                    new ReferenceType(0),
                    new ReferenceType(1),
                    new ReferenceType(2),
                    new ReferenceType(3)
            });
        var array = new ReferenceType[4];

        var result = source.TryCopyTo(array);

        AssertTrue(result, "Failed to copy reference types");
        for (int i = 0; i < 4; i++)
        {
            AssertTrue(source[i] != null && array[i] != null, "Failed to copy reference types, references were null");
            AssertTrue(object.ReferenceEquals(source[i], array[i]), "Failed to copy reference types, references were not equal");
            AssertTrue(source[i].Value == array[i].Value, "Failed to copy reference types, values were not equal");
        }
    }

    private static void CanCopyValueTypesWithPointersToSlice()
    {
        var source = new Span<ValueTypeWithPointers>(
            new[]
            {
                    new ValueTypeWithPointers(new object()),
                    new ValueTypeWithPointers(new object()),
                    new ValueTypeWithPointers(new object()),
                    new ValueTypeWithPointers(new object())
            });
        var underlyingArray = new ValueTypeWithPointers[4];
        var slice = new Span<ValueTypeWithPointers>(underlyingArray);

        var result = source.TryCopyTo(slice);

        AssertTrue(result, "Failed to copy value types with pointers");
        for (int i = 0; i < 4; i++)
        {
            AssertTrue(object.ReferenceEquals(source[i].Reference, slice[i].Reference), "Failed to copy value types with pointers, references were not the same");
            AssertTrue(object.ReferenceEquals(source[i].Reference, underlyingArray[i].Reference), "Failed to copy value types with pointers to underlying array, references were not the same");
        }
    }

    private static void CanCopyValueTypesWithPointersToArray()
    {
        var source = new Span<ValueTypeWithPointers>(
            new[]
            {
                    new ValueTypeWithPointers(new object()),
                    new ValueTypeWithPointers(new object()),
                    new ValueTypeWithPointers(new object()),
                    new ValueTypeWithPointers(new object())
            });
        var array = new ValueTypeWithPointers[4];

        var result = source.TryCopyTo(array);

        AssertTrue(result, "Failed to copy value types with pointers");
        for (int i = 0; i < 4; i++)
        {
            AssertTrue(object.ReferenceEquals(source[i].Reference, array[i].Reference), "Failed to copy value types with pointers, references were not the same");
        }
    }

    private static unsafe void CanCopyValueTypesWithoutPointersToUnmanagedMemory()
    {
        var source = new Span<byte>(
            new byte[]
            {
                    0,
                    1,
                    2,
                    3
            });
        byte* pointerToStack = stackalloc byte[256];

        var result = source.TryCopyTo(pointerToStack, 4);

        AssertTrue(result, "Failed to copy value types without pointers to unamanaged memory");
        for (int i = 0; i < 4; i++)
        {
            AssertTrue(source[i] == pointerToStack[i], "Failed to copy value types without pointers to unamanaged memory, values were not equal");
        }
    }

    private static unsafe void MustNotCopyValueTypesWithPointersToUnmanagedMemory()
    {
        var source = new Span<ValueTypeWithPointers>(
            new[]
            {
                    new ValueTypeWithPointers(new object())
            });
        byte* pointerToStack = stackalloc byte[256];

        var result = source.TryCopyTo(pointerToStack, 1);

        AssertTrue(result == false, "Failed to prevent from copying value types with pointers to unamanaged memory");
    }

    private static void Test(Action test, string testName, ref int failedTestsCount)
    {
        try
        {
            test();

            Console.WriteLine(testName + " test has passed");
        }
        catch (System.Exception ex)
        {
            Console.WriteLine(testName + " test has failed with exception: " + ex.Message);

            ++failedTestsCount;
        }
        finally
        {
            Console.WriteLine("-------------------");
        }
    }

    private static void AssertTrue(bool condition, string errorMessage)
    {
        if (condition == false)
        {
            throw new Exception(errorMessage);
        }
    }
}
