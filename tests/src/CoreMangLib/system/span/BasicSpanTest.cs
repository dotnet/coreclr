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

        Test(MustNotMoveGcTypesToUnmanagedMemory, "MustNotMoveGcTypesToUnmanagedMemory", ref failedTestsCount);

        Test(TestArrayCoVariance, "TestArrayCoVariance", ref failedTestsCount);
        Test(TestArrayCoVarianceReadOnly, "TestArrayCoVarianceReadOnly", ref failedTestsCount);

        Test(CanCopyValueTypesWithoutPointersToSlice, "CanCopyValueTypesWithoutPointersToSlice", ref failedTestsCount);
        Test(CanCopyValueTypesWithoutPointersToArray, "CanCopyValueTypesWithoutPointersToArray", ref failedTestsCount);

        Test(CanCopyReferenceTypesToSlice, "CanCopyReferenceTypesToSlice", ref failedTestsCount);
        Test(CanCopyReferenceTypesToArray, "CanCopyReferenceTypesToArray", ref failedTestsCount);

        Test(CanCopyValueTypesWithPointersToSlice, "CanCopyValueTypesWithPointersToSlice", ref failedTestsCount);
        Test(CanCopyValueTypesWithPointersToArray, "CanCopyValueTypesWithPointersToArray", ref failedTestsCount);

        Test(CanCopyValueTypesWithoutPointersToUnmanagedMemory, "CanCopyValueTypesWithoutPointersToUnmanagedMemory", ref failedTestsCount);

        Test(CanCopyOverlappingSlicesOfValueTypeWithoutPointers, "CanCopyOverlappingSlicesOfValueTypeWithoutPointers", ref failedTestsCount);
        Test(CanCopyOverlappingSlicesOfValueTypeWithPointers, "CanCopyOverlappingSlicesOfValueTypeWithPointers", ref failedTestsCount);
        Test(CanCopyOverlappingSlicesOfReferenceTypes, "CanCopyOverlappingSlicesOfReferenceTypes", ref failedTestsCount);

        Console.WriteLine(string.Format("{0} tests has failed", failedTestsCount));
        Environment.Exit(failedTestsCount);
    }

    static void CanAccessItemsViaIndexer()
    {
        int[] a = new int[] { 1, 2, 3 };
        Span<int> slice = new Span<int>(a);
        AssertTrue(Sum(slice) == 6, "Failed to sum slice");
        
        Span<int> subslice = slice.Slice(1, 2);
        AssertTrue(Sum(subslice) == 5, "Failed to sum subslice");
    }

    static void ReferenceTypesAreSupported()
    {
        var underlyingArray = new ReferenceType[] { new ReferenceType(0), new ReferenceType(1), new ReferenceType(2) };
        var slice = new Span<ReferenceType>(underlyingArray);

        for (int i = 0; i < underlyingArray.Length; i++)
        {
            AssertTrue(underlyingArray[i].Value == slice[i].Value, "Values are different");
            AssertTrue(object.ReferenceEquals(underlyingArray[i], slice[i]), "References are broken");
        }
    }

    static unsafe void MustNotMoveGcTypesToUnmanagedMemory()
    {
        byte* pointerToStack = stackalloc byte[256];

        try
        {
            new Span<ValueTypeWithPointers>(pointerToStack, 1);
            AssertTrue(false, "Expected exception for value types with references not thrown");
        }
        catch (System.ArgumentException ex)
        {
            AssertTrue(ex.Message == "'ValueTypeWithPointers' is reference type or contains pointers and hence can not be stored in unmanaged memory.",
                "Exception message is incorrect");
        }

        try
        {
            new Span<ReferenceType>(pointerToStack, 1);
            AssertTrue(false, "Expected exception for reference types not thrown");
        }
        catch (System.ArgumentException ex)
        {
            AssertTrue(ex.Message == "'ReferenceType' is reference type or contains pointers and hence can not be stored in unmanaged memory.",
                "Exception message is incorrect");
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

    static void CanUpdateUnderlyingArray()
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

    static void CanCopyValueTypesWithoutPointersToSlice()
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

    static void CanCopyValueTypesWithoutPointersToArray()
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

    static void CanCopyReferenceTypesToSlice()
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

    static void CanCopyReferenceTypesToArray()
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

    static void CanCopyValueTypesWithPointersToSlice()
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

    static void CanCopyValueTypesWithPointersToArray()
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

    static unsafe void CanCopyValueTypesWithoutPointersToUnmanagedMemory()
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

        var result = source.TryCopyTo(new Span<byte>(pointerToStack, 4));

        AssertTrue(result, "Failed to copy value types without pointers to unamanaged memory");
        for (int i = 0; i < 4; i++)
        {
            AssertTrue(source[i] == pointerToStack[i], "Failed to copy value types without pointers to unamanaged memory, values were not equal");
        }
    }

    static void CanCopyOverlappingSlicesOfValueTypeWithoutPointers()
    {
        var sourceArray = new[]
            {
                new ValueTypeWithoutPointers(0),
                new ValueTypeWithoutPointers(1),
                new ValueTypeWithoutPointers(2)
            };
        var firstAndSecondElements = new Span<ValueTypeWithoutPointers>(sourceArray, 0, 2); // 0, 1
        var secondAndThirdElements = new Span<ValueTypeWithoutPointers>(sourceArray, 1, 2); // 1, 2

        // 0 1 2 sourceArray
        // 0 1 - firstAndSecondElements
        // - 1 2 secondAndThirdElements
        var result = firstAndSecondElements.TryCopyTo(secondAndThirdElements); // to avoid overlap we should copy backward now
        // - 0 1 secondAndThirdElements
        // 0 0 - firstAndSecondElements     
        // 0 0 1 sourceArray

        AssertTrue(result, "Failed to copy overlapping value types without pointers");

        AssertTrue(secondAndThirdElements[1].Value == 1, "secondAndThirdElements[1] should get replaced by 1");
        AssertTrue(secondAndThirdElements[0].Value == 0 && firstAndSecondElements[1].Value == 0, "secondAndThirdElements[0] and firstAndSecondElements[1] point to the same element, should get replaced by 0");
        AssertTrue(firstAndSecondElements[0].Value == 0, "firstAndSecondElements[0] should remain the same");

        // let's try the other direction to make sure it works as well!

        sourceArray = new[]
            {
                new ValueTypeWithoutPointers(0),
                new ValueTypeWithoutPointers(1),
                new ValueTypeWithoutPointers(2)
            };
        firstAndSecondElements = new Span<ValueTypeWithoutPointers>(sourceArray, 0, 2); // 0, 1
        secondAndThirdElements = new Span<ValueTypeWithoutPointers>(sourceArray, 1, 2); // 1, 2

        // 0 1 2 sourceArray
        // 0 1 - firstAndSecondElements
        // - 1 2 secondAndThirdElements
        result = secondAndThirdElements.TryCopyTo(firstAndSecondElements); // to avoid overlap we should copy forward now
        // 1 2 - firstAndSecondElements
        // - 2 2 secondAndThirdElements
        // 1 2 2 sourceArray

        AssertTrue(result, "Failed to copy overlapping value types without pointers");

        AssertTrue(secondAndThirdElements[1].Value == 2, "secondAndThirdElements[1] should remain the same");
        AssertTrue(firstAndSecondElements[1].Value == 2 && secondAndThirdElements[0].Value == 2, "secondAndThirdElements[0] && firstAndSecondElements[1] point to the same element, should get replaced by 2");
        AssertTrue(firstAndSecondElements[0].Value == 1, "firstAndSecondElements[0] should get replaced by 1");
    }

    static void CanCopyOverlappingSlicesOfValueTypeWithPointers()
    {
        string zero = "0", one = "1", two = "2";
        var sourceArray = new[]
            {
                new ValueTypeWithPointers(zero),
                new ValueTypeWithPointers(one),
                new ValueTypeWithPointers(two)
            };
        var firstAndSecondElements = new Span<ValueTypeWithPointers>(sourceArray, 0, 2); // 0, 1
        var secondAndThirdElements = new Span<ValueTypeWithPointers>(sourceArray, 1, 2); // 1, 2

        // 0 1 2 sourceArray
        // 0 1 - firstAndSecondElements
        // - 1 2 secondAndThirdElements
        var result = firstAndSecondElements.TryCopyTo(secondAndThirdElements); // to avoid overlap we should copy backward now
        // - 0 1 secondAndThirdElements
        // 0 0 - firstAndSecondElements
        // 0 0 1 sourceArray

        AssertTrue(result, "Failed to copy overlapping value types with pointers");

        AssertTrue(object.ReferenceEquals(secondAndThirdElements[1].Reference, one), "secondAndThirdElements[1] should get replaced by 1");
        AssertTrue(object.ReferenceEquals(secondAndThirdElements[0].Reference, zero) && object.ReferenceEquals(firstAndSecondElements[1].Reference, zero), "secondAndThirdElements[0] and firstAndSecondElements[1] point to the same element, should get replaced by 0");
        AssertTrue(object.ReferenceEquals(firstAndSecondElements[0].Reference, zero), "firstAndSecondElements[0] should remain the same");

        // let's try the other direction to make sure it works as well!

        sourceArray = new[]
            {
                new ValueTypeWithPointers(zero),
                new ValueTypeWithPointers(one),
                new ValueTypeWithPointers(two)
            };
        firstAndSecondElements = new Span<ValueTypeWithPointers>(sourceArray, 0, 2); // 0, 1
        secondAndThirdElements = new Span<ValueTypeWithPointers>(sourceArray, 1, 2); // 1, 2

        // 0 1 2 sourceArray
        // 0 1 - firstAndSecondElements
        // - 1 2 secondAndThirdElements
        result = secondAndThirdElements.TryCopyTo(firstAndSecondElements); // to avoid overlap we should copy forward now
        // 1 2 - firstAndSecondElements
        // - 2 2 secondAndThirdElements
        // 1 2 2 sourceArray

        AssertTrue(result, "Failed to copy overlapping value types with pointers");

        AssertTrue(object.ReferenceEquals(secondAndThirdElements[1].Reference, two), "secondAndThirdElements[1] should remain the same");
        AssertTrue(object.ReferenceEquals(firstAndSecondElements[1].Reference, two) && object.ReferenceEquals(secondAndThirdElements[0].Reference, two), "secondAndThirdElements[0] && firstAndSecondElements[1] point to the same element, should get replaced by 2");
        AssertTrue(object.ReferenceEquals(firstAndSecondElements[0].Reference, one), "firstAndSecondElements[0] should get replaced by 1");
    }

    static void CanCopyOverlappingSlicesOfReferenceTypes()
    {
        var sourceArray = new ReferenceType[] { new ReferenceType(0), new ReferenceType(1), new ReferenceType(2) };

        var firstAndSecondElements = new Span<ReferenceType>(sourceArray, 0, 2); // 0, 1
        var secondAndThirdElements = new Span<ReferenceType>(sourceArray, 1, 2); // 1, 2

        // 0 1 2 sourceArray
        // 0 1 - firstAndSecondElements
        // - 1 2 secondAndThirdElements
        var result = firstAndSecondElements.TryCopyTo(secondAndThirdElements); // to avoid overlap we should copy backward now
        // - 0 1 secondAndThirdElements
        // 0 0 - firstAndSecondElements
        // 0 0 1 sourceArray

        AssertTrue(result, "Failed to copy overlapping reference types");

        AssertTrue(secondAndThirdElements[1].Value == 1, "secondAndThirdElements[1] should get replaced by 1");
        AssertTrue(secondAndThirdElements[0].Value == 0 && firstAndSecondElements[1].Value == 0, "secondAndThirdElements[0] and firstAndSecondElements[1] point to the same element, should get replaced by 0");
        AssertTrue(firstAndSecondElements[0].Value == 0, "firstAndSecondElements[0] should remain the same");

        // let's try the other direction to make sure it works as well!

        sourceArray = new[]
            {
                new ReferenceType(0),
                new ReferenceType(1),
                new ReferenceType(2)
            };
        firstAndSecondElements = new Span<ReferenceType>(sourceArray, 0, 2); // 0, 1
        secondAndThirdElements = new Span<ReferenceType>(sourceArray, 1, 2); // 1, 2

        // 0 1 2 sourceArray
        // 0 1 - firstAndSecondElements
        // - 1 2 secondAndThirdElements
        result = secondAndThirdElements.TryCopyTo(firstAndSecondElements); // to avoid overlap we should copy forward now
        // 1 2 - firstAndSecondElements
        // - 2 2 secondAndThirdElements
        // 1 2 2 sourceArray

        AssertTrue(result, "Failed to copy overlapping reference types");

        AssertTrue(secondAndThirdElements[1].Value == 2, "secondAndThirdElements[1] should remain the same");
        AssertTrue(firstAndSecondElements[1].Value == 2 && secondAndThirdElements[0].Value == 2, "secondAndThirdElements[0] && firstAndSecondElements[1] point to the same element, should get replaced by 2");
        AssertTrue(firstAndSecondElements[0].Value == 1, "firstAndSecondElements[0] should get replaced by 1");
    }

    static void Test(Action test, string testName, ref int failedTestsCount)
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

    static void AssertTrue(bool condition, string errorMessage)
    {
        if (condition == false)
        {
            throw new Exception(errorMessage);
        }
    }
}
