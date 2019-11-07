// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using Internal.Runtime.CompilerServices;

namespace System
{
    [Serializable]
    [System.Runtime.CompilerServices.TypeForwardedFrom("mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")]
    public abstract partial class Array : ICloneable, IList, IStructuralComparable, IStructuralEquatable
    {
        // We impose limits on maximum array length in each dimension to allow efficient
        // implementation of advanced range check elimination in future.
        // Keep in sync with vm\gcscan.cpp and HashHelpers.MaxPrimeArrayLength.
        // The constants are defined in this method: inline SIZE_T MaxArrayLength(SIZE_T componentSize) from gcscan
        // We have different max sizes for arrays with elements of size 1 for backwards compatibility
        internal const int MaxArrayLength = 0X7FEFFFFF;
        internal const int MaxByteArrayLength = 0x7FFFFFC7;

        // This ctor exists solely to prevent C# from generating a protected .ctor that violates the surface area.
        private protected Array() { }

        public static ReadOnlyCollection<T> AsReadOnly<T>(T[] array)
        {
            if (array == null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.array);
            }

            // T[] implements IList<T>.
            return new ReadOnlyCollection<T>(array);
        }

        public static void Resize<T>([NotNull] ref T[]? array, int newSize)
        {
            if (newSize < 0)
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.newSize, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);

            T[]? larray = array;
            if (larray == null)
            {
                array = new T[newSize];
                return;
            }

            if (larray.Length != newSize)
            {
                T[] newArray = new T[newSize];
                Copy(larray, 0, newArray, 0, larray.Length > newSize ? newSize : larray.Length);
                array = newArray;
            }
        }

        public static Array CreateInstance(Type elementType, params long[] lengths)
        {
            if (lengths == null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.lengths);
            }
            if (lengths.Length == 0)
                ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_NeedAtLeast1Rank);

            int[] intLengths = new int[lengths.Length];

            for (int i = 0; i < lengths.Length; ++i)
            {
                long len = lengths[i];
                int ilen = (int)len;
                if (len != ilen)
                    ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.len, ExceptionResource.ArgumentOutOfRange_HugeArrayNotSupported);
                intLengths[i] = ilen;
            }

            return Array.CreateInstance(elementType, intLengths);
        }

        public static void Copy(Array sourceArray, Array destinationArray, long length)
        {
            int ilength = (int)length;
            if (length != ilength)
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.length, ExceptionResource.ArgumentOutOfRange_HugeArrayNotSupported);

            Copy(sourceArray, destinationArray, ilength);
        }

        public static void Copy(Array sourceArray, long sourceIndex, Array destinationArray, long destinationIndex, long length)
        {
            int isourceIndex = (int)sourceIndex;
            int idestinationIndex = (int)destinationIndex;
            int ilength = (int)length;

            if (sourceIndex != isourceIndex)
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.sourceIndex, ExceptionResource.ArgumentOutOfRange_HugeArrayNotSupported);
            if (destinationIndex != idestinationIndex)
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.destinationIndex, ExceptionResource.ArgumentOutOfRange_HugeArrayNotSupported);
            if (length != ilength)
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.length, ExceptionResource.ArgumentOutOfRange_HugeArrayNotSupported);

            Copy(sourceArray, isourceIndex, destinationArray, idestinationIndex, ilength);
        }

        public object? GetValue(long index)
        {
            int iindex = (int)index;
            if (index != iindex)
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.index, ExceptionResource.ArgumentOutOfRange_HugeArrayNotSupported);

            return this.GetValue(iindex);
        }

        public object? GetValue(long index1, long index2)
        {
            int iindex1 = (int)index1;
            int iindex2 = (int)index2;

            if (index1 != iindex1)
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.index1, ExceptionResource.ArgumentOutOfRange_HugeArrayNotSupported);
            if (index2 != iindex2)
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.index2, ExceptionResource.ArgumentOutOfRange_HugeArrayNotSupported);

            return this.GetValue(iindex1, iindex2);
        }

        public object? GetValue(long index1, long index2, long index3)
        {
            int iindex1 = (int)index1;
            int iindex2 = (int)index2;
            int iindex3 = (int)index3;

            if (index1 != iindex1)
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.index1, ExceptionResource.ArgumentOutOfRange_HugeArrayNotSupported);
            if (index2 != iindex2)
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.index2, ExceptionResource.ArgumentOutOfRange_HugeArrayNotSupported);
            if (index3 != iindex3)
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.index3, ExceptionResource.ArgumentOutOfRange_HugeArrayNotSupported);

            return this.GetValue(iindex1, iindex2, iindex3);
        }

        public object? GetValue(params long[] indices)
        {
            if (indices == null)
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.indices);
            if (Rank != indices.Length)
                ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_RankIndices);

            int[] intIndices = new int[indices.Length];

            for (int i = 0; i < indices.Length; ++i)
            {
                long index = indices[i];
                int iindex = (int)index;
                if (index != iindex)
                    ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.index, ExceptionResource.ArgumentOutOfRange_HugeArrayNotSupported);
                intIndices[i] = iindex;
            }

            return this.GetValue(intIndices);
        }

        public void SetValue(object? value, long index)
        {
            int iindex = (int)index;

            if (index != iindex)
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.index, ExceptionResource.ArgumentOutOfRange_HugeArrayNotSupported);

            this.SetValue(value, iindex);
        }

        public void SetValue(object? value, long index1, long index2)
        {
            int iindex1 = (int)index1;
            int iindex2 = (int)index2;

            if (index1 != iindex1)
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.index1, ExceptionResource.ArgumentOutOfRange_HugeArrayNotSupported);
            if (index2 != iindex2)
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.index2, ExceptionResource.ArgumentOutOfRange_HugeArrayNotSupported);

            this.SetValue(value, iindex1, iindex2);
        }

        public void SetValue(object? value, long index1, long index2, long index3)
        {
            int iindex1 = (int)index1;
            int iindex2 = (int)index2;
            int iindex3 = (int)index3;

            if (index1 != iindex1)
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.index1, ExceptionResource.ArgumentOutOfRange_HugeArrayNotSupported);
            if (index2 != iindex2)
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.index2, ExceptionResource.ArgumentOutOfRange_HugeArrayNotSupported);
            if (index3 != iindex3)
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.index3, ExceptionResource.ArgumentOutOfRange_HugeArrayNotSupported);

            this.SetValue(value, iindex1, iindex2, iindex3);
        }

        public void SetValue(object? value, params long[] indices)
        {
            if (indices == null)
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.indices);
            if (Rank != indices.Length)
                ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_RankIndices);

            int[] intIndices = new int[indices.Length];

            for (int i = 0; i < indices.Length; ++i)
            {
                long index = indices[i];
                int iindex = (int)index;
                if (index != iindex)
                    ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.index, ExceptionResource.ArgumentOutOfRange_HugeArrayNotSupported);
                intIndices[i] = iindex;
            }

            this.SetValue(value, intIndices);
        }

        private static int GetMedian(int low, int hi)
        {
            // Note both may be negative, if we are dealing with arrays w/ negative lower bounds.
            Debug.Assert(low <= hi);
            Debug.Assert(hi - low >= 0, "Length overflow!");
            return low + ((hi - low) >> 1);
        }

        public long GetLongLength(int dimension)
        {
            // This method should throw an IndexOufOfRangeException for compat if dimension < 0 or >= Rank
            return GetLength(dimension);
        }

        // Number of elements in the Array.
        int ICollection.Count => Length;

        // Returns an object appropriate for synchronizing access to this
        // Array.
        public object SyncRoot => this;

        // Is this Array read-only?
        public bool IsReadOnly => false;

        public bool IsFixedSize => true;

        // Is this Array synchronized (i.e., thread-safe)?  If you want a synchronized
        // collection, you can use SyncRoot as an object to synchronize your
        // collection with.  You could also call GetSynchronized()
        // to get a synchronized wrapper around the Array.
        public bool IsSynchronized => false;

        object? IList.this[int index]
        {
            get => GetValue(index);
            set => SetValue(value, index);
        }

        int IList.Add(object? value)
        {
            ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_FixedSizeCollection);
            return default;
        }

        bool IList.Contains(object? value)
        {
            return Array.IndexOf(this, value) >= this.GetLowerBound(0);
        }

        void IList.Clear()
        {
            Array.Clear(this, this.GetLowerBound(0), this.Length);
        }

        int IList.IndexOf(object? value)
        {
            return Array.IndexOf(this, value);
        }

        void IList.Insert(int index, object? value)
        {
            ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_FixedSizeCollection);
        }

        void IList.Remove(object? value)
        {
            ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_FixedSizeCollection);
        }

        void IList.RemoveAt(int index)
        {
            ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_FixedSizeCollection);
        }

        // Make a new array which is a shallow copy of the original array.
        //
        public object Clone()
        {
            return MemberwiseClone();
        }

        int IStructuralComparable.CompareTo(object? other, IComparer comparer)
        {
            if (other == null)
            {
                return 1;
            }

            Array? o = other as Array;

            if (o == null || this.Length != o.Length)
            {
                ThrowHelper.ThrowArgumentException(ExceptionResource.ArgumentException_OtherNotArrayOfCorrectLength, ExceptionArgument.other);
            }

            int i = 0;
            int c = 0;

            while (i < o.Length && c == 0)
            {
                object? left = GetValue(i);
                object? right = o.GetValue(i);

                c = comparer.Compare(left, right);
                i++;
            }

            return c;
        }

        bool IStructuralEquatable.Equals(object? other, IEqualityComparer comparer)
        {
            if (other == null)
            {
                return false;
            }

            if (object.ReferenceEquals(this, other))
            {
                return true;
            }

            if (!(other is Array o) || o.Length != this.Length)
            {
                return false;
            }

            int i = 0;
            while (i < o.Length)
            {
                object? left = GetValue(i);
                object? right = o.GetValue(i);

                if (!comparer.Equals(left, right))
                {
                    return false;
                }
                i++;
            }

            return true;
        }

        int IStructuralEquatable.GetHashCode(IEqualityComparer comparer)
        {
            if (comparer == null)
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.comparer);

            int ret = 0;

            for (int i = (this.Length >= 8 ? this.Length - 8 : 0); i < this.Length; i++)
            {
                ret = HashCode.Combine(ret, comparer.GetHashCode(GetValue(i)!));
            }

            return ret;
        }

        // Searches an array for a given element using a binary search algorithm.
        // Elements of the array are compared to the search value using the
        // IComparable interface, which must be implemented by all elements
        // of the array and the given search value. This method assumes that the
        // array is already sorted according to the IComparable interface;
        // if this is not the case, the result will be incorrect.
        //
        // The method returns the index of the given value in the array. If the
        // array does not contain the given value, the method returns a negative
        // integer. The bitwise complement operator (~) can be applied to a
        // negative result to produce the index of the first element (if any) that
        // is larger than the given search value.
        //
        public static int BinarySearch(Array array, object? value)
        {
            if (array == null)
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.array);
            return BinarySearch(array, array.GetLowerBound(0), array.Length, value, null);
        }

        // Searches a section of an array for a given element using a binary search
        // algorithm. Elements of the array are compared to the search value using
        // the IComparable interface, which must be implemented by all
        // elements of the array and the given search value. This method assumes
        // that the array is already sorted according to the IComparable
        // interface; if this is not the case, the result will be incorrect.
        //
        // The method returns the index of the given value in the array. If the
        // array does not contain the given value, the method returns a negative
        // integer. The bitwise complement operator (~) can be applied to a
        // negative result to produce the index of the first element (if any) that
        // is larger than the given search value.
        //
        public static int BinarySearch(Array array, int index, int length, object? value)
        {
            return BinarySearch(array, index, length, value, null);
        }

        // Searches an array for a given element using a binary search algorithm.
        // Elements of the array are compared to the search value using the given
        // IComparer interface. If comparer is null, elements of the
        // array are compared to the search value using the IComparable
        // interface, which in that case must be implemented by all elements of the
        // array and the given search value. This method assumes that the array is
        // already sorted; if this is not the case, the result will be incorrect.
        //
        // The method returns the index of the given value in the array. If the
        // array does not contain the given value, the method returns a negative
        // integer. The bitwise complement operator (~) can be applied to a
        // negative result to produce the index of the first element (if any) that
        // is larger than the given search value.
        //
        public static int BinarySearch(Array array, object? value, IComparer? comparer)
        {
            if (array == null)
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.array);
            return BinarySearch(array, array.GetLowerBound(0), array.Length, value, comparer);
        }

        // Searches a section of an array for a given element using a binary search
        // algorithm. Elements of the array are compared to the search value using
        // the given IComparer interface. If comparer is null,
        // elements of the array are compared to the search value using the
        // IComparable interface, which in that case must be implemented by
        // all elements of the array and the given search value. This method
        // assumes that the array is already sorted; if this is not the case, the
        // result will be incorrect.
        //
        // The method returns the index of the given value in the array. If the
        // array does not contain the given value, the method returns a negative
        // integer. The bitwise complement operator (~) can be applied to a
        // negative result to produce the index of the first element (if any) that
        // is larger than the given search value.
        //
        public static int BinarySearch(Array array, int index, int length, object? value, IComparer? comparer)
        {
            if (array == null)
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.array);
            int lb = array.GetLowerBound(0);
            if (index < lb)
                ThrowHelper.ThrowIndexArgumentOutOfRange_NeedNonNegNumException();
            if (length < 0)
                ThrowHelper.ThrowLengthArgumentOutOfRange_ArgumentOutOfRange_NeedNonNegNum();
            if (array.Length - (index - lb) < length)
                ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_InvalidOffLen);
            if (array.Rank != 1)
                ThrowHelper.ThrowRankException(ExceptionResource.Rank_MultiDimNotSupported);

            comparer ??= Comparer.Default;

            int lo = index;
            int hi = index + length - 1;
            if (array is object[] objArray)
            {
                while (lo <= hi)
                {
                    // i might overflow if lo and hi are both large positive numbers.
                    int i = GetMedian(lo, hi);

                    int c;
                    try
                    {
                        c = comparer.Compare(objArray[i], value);
                    }
                    catch (Exception e)
                    {
                        ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_IComparerFailed, e);
                        return default;
                    }
                    if (c == 0) return i;
                    if (c < 0)
                    {
                        lo = i + 1;
                    }
                    else
                    {
                        hi = i - 1;
                    }
                }
                return ~lo;
            }

            if (comparer == Comparer.Default)
            {
                int result;
                switch (array.GetCorElementTypeOfElementType())
                {
                    case CorElementType.ELEMENT_TYPE_BOOLEAN:
                        if (TryGenericBinarySearch<bool>(array, index, length, value, out result)) return result;
                        break;
                    case CorElementType.ELEMENT_TYPE_U1:
                        if (TryGenericBinarySearch<byte>(array, index, length, value, out result)) return result;
                        break;
                    case CorElementType.ELEMENT_TYPE_CHAR:
                        if (TryGenericBinarySearch<char>(array, index, length, value, out result)) return result;
                        break;
                    case CorElementType.ELEMENT_TYPE_R8:
                        if (TryGenericBinarySearch<double>(array, index, length, value, out result)) return result;
                        break;
                    case CorElementType.ELEMENT_TYPE_I2:
                        if (TryGenericBinarySearch<short>(array, index, length, value, out result)) return result;
                        break;
                    case CorElementType.ELEMENT_TYPE_I4:
                        if (TryGenericBinarySearch<int>(array, index, length, value, out result)) return result;
                        break;
                    case CorElementType.ELEMENT_TYPE_I8:
                        if (TryGenericBinarySearch<long>(array, index, length, value, out result)) return result;
                        break;
                    case CorElementType.ELEMENT_TYPE_I1:
                        if (TryGenericBinarySearch<sbyte>(array, index, length, value, out result)) return result;
                        break;
                    case CorElementType.ELEMENT_TYPE_R4:
                        if (TryGenericBinarySearch<float>(array, index, length, value, out result)) return result;
                        break;
                    case CorElementType.ELEMENT_TYPE_U2:
                        if (TryGenericBinarySearch<ushort>(array, index, length, value, out result)) return result;
                        break;
                    case CorElementType.ELEMENT_TYPE_U4:
                        if (TryGenericBinarySearch<uint>(array, index, length, value, out result)) return result;
                        break;
                    case CorElementType.ELEMENT_TYPE_U8:
                        if (TryGenericBinarySearch<ulong>(array, index, length, value, out result)) return result;
                        break;
                }

                static bool TryGenericBinarySearch<T>(Array array, int index, int length, object? value, out int result) where T : struct
                {
                    if (array is T[] arrayOfT)
                    {
                        if (value is null)
                        {
                            result = -1;
                            return true;
                        }

                        if (value is T valueOfT)
                        {
                            result = BinarySearch<T>(arrayOfT, index, length, valueOfT);
                            return true;
                        }
                    }

                    result = default;
                    return false;
                }
            }

            while (lo <= hi)
            {
                int i = GetMedian(lo, hi);

                int c;
                try
                {
                    c = comparer.Compare(array.GetValue(i), value);
                }
                catch (Exception e)
                {
                    ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_IComparerFailed, e);
                    return default;
                }
                if (c == 0) return i;
                if (c < 0)
                {
                    lo = i + 1;
                }
                else
                {
                    hi = i - 1;
                }
            }
            return ~lo;
        }

        public static int BinarySearch<T>(T[] array, T value)
        {
            if (array == null)
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.array);
            return BinarySearch<T>(array, 0, array.Length, value, null);
        }

        public static int BinarySearch<T>(T[] array, T value, System.Collections.Generic.IComparer<T>? comparer)
        {
            if (array == null)
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.array);
            return BinarySearch<T>(array, 0, array.Length, value, comparer);
        }

        public static int BinarySearch<T>(T[] array, int index, int length, T value)
        {
            return BinarySearch<T>(array, index, length, value, null);
        }

        public static int BinarySearch<T>(T[] array, int index, int length, T value, System.Collections.Generic.IComparer<T>? comparer)
        {
            if (array == null)
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.array);
            if (index < 0)
                ThrowHelper.ThrowIndexArgumentOutOfRange_NeedNonNegNumException();
            if (length < 0)
                ThrowHelper.ThrowLengthArgumentOutOfRange_ArgumentOutOfRange_NeedNonNegNum();

            if (array.Length - index < length)
                ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_InvalidOffLen);

            return ArraySortHelper<T>.Default.BinarySearch(array, index, length, value, comparer);
        }

        public static TOutput[] ConvertAll<TInput, TOutput>(TInput[] array, Converter<TInput, TOutput> converter)
        {
            if (array == null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.array);
            }

            if (converter == null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.converter);
            }

            TOutput[] newArray = new TOutput[array.Length];
            for (int i = 0; i < array.Length; i++)
            {
                newArray[i] = converter(array[i]);
            }
            return newArray;
        }

        // CopyTo copies a collection into an Array, starting at a particular
        // index into the array.
        //
        // This method is to support the ICollection interface, and calls
        // Array.Copy internally.  If you aren't using ICollection explicitly,
        // call Array.Copy to avoid an extra indirection.
        //
        public void CopyTo(Array array, int index)
        {
            if (array != null && array.Rank != 1)
                ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_RankMultiDimNotSupported);
            // Note: Array.Copy throws a RankException and we want a consistent ArgumentException for all the IList CopyTo methods.
            Array.Copy(this, GetLowerBound(0), array!, index, Length);
        }

        public void CopyTo(Array array, long index)
        {
            int iindex = (int)index;
            if (index != iindex)
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.index, ExceptionResource.ArgumentOutOfRange_HugeArrayNotSupported);

            this.CopyTo(array, iindex);
        }

        private static class EmptyArray<T>
        {
#pragma warning disable CA1825 // this is the implementation of Array.Empty<T>()
            internal static readonly T[] Value = new T[0];
#pragma warning restore CA1825
        }

        public static T[] Empty<T>()
        {
            return EmptyArray<T>.Value;
        }

        public static bool Exists<T>(T[] array, Predicate<T> match)
        {
            return Array.FindIndex(array, match) != -1;
        }

        public static void Fill<T>(T[] array, T value)
        {
            if (array == null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.array);
            }

            for (int i = 0; i < array.Length; i++)
            {
                array[i] = value;
            }
        }

        public static void Fill<T>(T[] array, T value, int startIndex, int count)
        {
            if (array == null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.array);
            }

            if (startIndex < 0 || startIndex > array.Length)
            {
                ThrowHelper.ThrowStartIndexArgumentOutOfRange_ArgumentOutOfRange_Index();
            }

            if (count < 0 || startIndex > array.Length - count)
            {
                ThrowHelper.ThrowCountArgumentOutOfRange_ArgumentOutOfRange_Count();
            }

            for (int i = startIndex; i < startIndex + count; i++)
            {
                array[i] = value;
            }
        }

        [return: MaybeNull]
        public static T Find<T>(T[] array, Predicate<T> match)
        {
            if (array == null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.array);
            }

            if (match == null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.match);
            }

            for (int i = 0; i < array.Length; i++)
            {
                if (match(array[i]))
                {
                    return array[i];
                }
            }
            return default!;
        }

        public static T[] FindAll<T>(T[] array, Predicate<T> match)
        {
            if (array == null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.array);
            }

            if (match == null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.match);
            }

            List<T> list = new List<T>();
            for (int i = 0; i < array.Length; i++)
            {
                if (match(array[i]))
                {
                    list.Add(array[i]);
                }
            }
            return list.ToArray();
        }

        public static int FindIndex<T>(T[] array, Predicate<T> match)
        {
            if (array == null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.array);
            }

            return FindIndex(array, 0, array.Length, match);
        }

        public static int FindIndex<T>(T[] array, int startIndex, Predicate<T> match)
        {
            if (array == null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.array);
            }

            return FindIndex(array, startIndex, array.Length - startIndex, match);
        }

        public static int FindIndex<T>(T[] array, int startIndex, int count, Predicate<T> match)
        {
            if (array == null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.array);
            }

            if (startIndex < 0 || startIndex > array.Length)
            {
                ThrowHelper.ThrowStartIndexArgumentOutOfRange_ArgumentOutOfRange_Index();
            }

            if (count < 0 || startIndex > array.Length - count)
            {
                ThrowHelper.ThrowCountArgumentOutOfRange_ArgumentOutOfRange_Count();
            }

            if (match == null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.match);
            }

            int endIndex = startIndex + count;
            for (int i = startIndex; i < endIndex; i++)
            {
                if (match(array[i]))
                    return i;
            }
            return -1;
        }

        [return: MaybeNull]
        public static T FindLast<T>(T[] array, Predicate<T> match)
        {
            if (array == null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.array);
            }

            if (match == null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.match);
            }

            for (int i = array.Length - 1; i >= 0; i--)
            {
                if (match(array[i]))
                {
                    return array[i];
                }
            }
            return default!;
        }

        public static int FindLastIndex<T>(T[] array, Predicate<T> match)
        {
            if (array == null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.array);
            }

            return FindLastIndex(array, array.Length - 1, array.Length, match);
        }

        public static int FindLastIndex<T>(T[] array, int startIndex, Predicate<T> match)
        {
            if (array == null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.array);
            }

            return FindLastIndex(array, startIndex, startIndex + 1, match);
        }

        public static int FindLastIndex<T>(T[] array, int startIndex, int count, Predicate<T> match)
        {
            if (array == null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.array);
            }

            if (match == null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.match);
            }

            if (array.Length == 0)
            {
                // Special case for 0 length List
                if (startIndex != -1)
                {
                    ThrowHelper.ThrowStartIndexArgumentOutOfRange_ArgumentOutOfRange_Index();
                }
            }
            else
            {
                // Make sure we're not out of range
                if (startIndex < 0 || startIndex >= array.Length)
                {
                    ThrowHelper.ThrowStartIndexArgumentOutOfRange_ArgumentOutOfRange_Index();
                }
            }

            // 2nd have of this also catches when startIndex == MAXINT, so MAXINT - 0 + 1 == -1, which is < 0.
            if (count < 0 || startIndex - count + 1 < 0)
            {
                ThrowHelper.ThrowCountArgumentOutOfRange_ArgumentOutOfRange_Count();
            }

            int endIndex = startIndex - count;
            for (int i = startIndex; i > endIndex; i--)
            {
                if (match(array[i]))
                {
                    return i;
                }
            }
            return -1;
        }

        public static void ForEach<T>(T[] array, Action<T> action)
        {
            if (array == null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.array);
            }

            if (action == null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.action);
            }

            for (int i = 0; i < array.Length; i++)
            {
                action(array[i]);
            }
        }

        // Returns the index of the first occurrence of a given value in an array.
        // The array is searched forwards, and the elements of the array are
        // compared to the given value using the Object.Equals method.
        //
        public static int IndexOf(Array array, object? value)
        {
            if (array == null)
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.array);
            return IndexOf(array, value, array.GetLowerBound(0), array.Length);
        }

        // Returns the index of the first occurrence of a given value in a range of
        // an array. The array is searched forwards, starting at index
        // startIndex and ending at the last element of the array. The
        // elements of the array are compared to the given value using the
        // Object.Equals method.
        //
        public static int IndexOf(Array array, object? value, int startIndex)
        {
            if (array == null)
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.array);
            int lb = array.GetLowerBound(0);
            return IndexOf(array, value, startIndex, array.Length - startIndex + lb);
        }

        // Returns the index of the first occurrence of a given value in a range of
        // an array. The array is searched forwards, starting at index
        // startIndex and upto count elements. The
        // elements of the array are compared to the given value using the
        // Object.Equals method.
        //
        public static int IndexOf(Array array, object? value, int startIndex, int count)
        {
            if (array == null)
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.array);
            if (array.Rank != 1)
                ThrowHelper.ThrowRankException(ExceptionResource.Rank_MultiDimNotSupported);

            int lb = array.GetLowerBound(0);
            if (startIndex < lb || startIndex > array.Length + lb)
                ThrowHelper.ThrowStartIndexArgumentOutOfRange_ArgumentOutOfRange_Index();
            if (count < 0 || count > array.Length - startIndex + lb)
                ThrowHelper.ThrowCountArgumentOutOfRange_ArgumentOutOfRange_Count();

            int endIndex = startIndex + count;
            if (array is object[] objArray)
            {
                if (value == null)
                {
                    for (int i = startIndex; i < endIndex; i++)
                    {
                        if (objArray[i] == null)
                            return i;
                    }
                }
                else
                {
                    for (int i = startIndex; i < endIndex; i++)
                    {
                        object obj = objArray[i];
                        if (obj != null && obj.Equals(value))
                            return i;
                    }
                }
                return -1;
            }

            int result;
            switch (array.GetCorElementTypeOfElementType())
            {
                case CorElementType.ELEMENT_TYPE_BOOLEAN:
                    if (TryGenericIndexOf<bool>(array, value, startIndex, count, out result)) return result;
                    break;
                case CorElementType.ELEMENT_TYPE_U1:
                    if (TryGenericIndexOf<byte>(array, value, startIndex, count, out result)) return result;
                    break;
                case CorElementType.ELEMENT_TYPE_CHAR:
                    if (TryGenericIndexOf<char>(array, value, startIndex, count, out result)) return result;
                    break;
                case CorElementType.ELEMENT_TYPE_R8:
                    if (TryGenericIndexOf<double>(array, value, startIndex, count, out result)) return result;
                    break;
                case CorElementType.ELEMENT_TYPE_I2:
                    if (TryGenericIndexOf<short>(array, value, startIndex, count, out result)) return result;
                    break;
                case CorElementType.ELEMENT_TYPE_I4:
                    if (TryGenericIndexOf<int>(array, value, startIndex, count, out result)) return result;
                    break;
                case CorElementType.ELEMENT_TYPE_I8:
                    if (TryGenericIndexOf<long>(array, value, startIndex, count, out result)) return result;
                    break;
                case CorElementType.ELEMENT_TYPE_I1:
                    if (TryGenericIndexOf<sbyte>(array, value, startIndex, count, out result)) return result;
                    break;
                case CorElementType.ELEMENT_TYPE_R4:
                    if (TryGenericIndexOf<float>(array, value, startIndex, count, out result)) return result;
                    break;
                case CorElementType.ELEMENT_TYPE_U2:
                    if (TryGenericIndexOf<ushort>(array, value, startIndex, count, out result)) return result;
                    break;
                case CorElementType.ELEMENT_TYPE_U4:
                    if (TryGenericIndexOf<uint>(array, value, startIndex, count, out result)) return result;
                    break;
                case CorElementType.ELEMENT_TYPE_U8:
                    if (TryGenericIndexOf<ulong>(array, value, startIndex, count, out result)) return result;
                    break;
                case CorElementType.ELEMENT_TYPE_I:
                    if (TryGenericIndexOf<IntPtr>(array, value, startIndex, count, out result)) return result;
                    break;
                case CorElementType.ELEMENT_TYPE_U:
                    if (TryGenericIndexOf<UIntPtr>(array, value, startIndex, count, out result)) return result;
                    break;
            }

            static bool TryGenericIndexOf<T>(Array array, object? value, int startIndex, int count, out int result) where T: struct
            {
                if (array is T[] arrayOfT)
                {
                    if (value is null)
                    {
                        result = -1;
                        return true;
                    }

                    if (value is T valueOfT)
                    {
                        result = IndexOf<T>(arrayOfT, valueOfT, startIndex, count);
                        return true;
                    }
                }

                result = default;
                return false;
            }

            for (int i = startIndex; i < endIndex; i++)
            {
                object? obj = array.GetValue(i);
                if (obj == null)
                {
                    if (value == null)
                        return i;
                }
                else
                {
                    if (obj.Equals(value))
                        return i;
                }
            }
            // Return one less than the lower bound of the array.  This way,
            // for arrays with a lower bound of -1 we will not return -1 when the
            // item was not found.  And for SZArrays (the vast majority), -1 still
            // works for them.
            return lb - 1;
        }

        public static int IndexOf<T>(T[] array, T value)
        {
            if (array == null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.array);
            }

            return IndexOf(array, value, 0, array.Length);
        }

        public static int IndexOf<T>(T[] array, T value, int startIndex)
        {
            if (array == null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.array);
            }

            return IndexOf(array, value, startIndex, array.Length - startIndex);
        }

        public static int IndexOf<T>(T[] array, T value, int startIndex, int count)
        {
            if (array == null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.array);
            }

            if ((uint)startIndex > (uint)array.Length)
            {
                ThrowHelper.ThrowStartIndexArgumentOutOfRange_ArgumentOutOfRange_Index();
            }

            if ((uint)count > (uint)(array.Length - startIndex))
            {
                ThrowHelper.ThrowCountArgumentOutOfRange_ArgumentOutOfRange_Count();
            }

            if (RuntimeHelpers.IsBitwiseEquatable<T>())
            {
                if (Unsafe.SizeOf<T>() == sizeof(byte))
                {
                    int result = SpanHelpers.IndexOf(
                        ref Unsafe.Add(ref array.GetRawSzArrayData(), startIndex),
                        Unsafe.As<T, byte>(ref value),
                        count);
                    return (result >= 0 ? startIndex : 0) + result;
                }
                else if (Unsafe.SizeOf<T>() == sizeof(char))
                {
                    int result = SpanHelpers.IndexOf(
                        ref Unsafe.Add(ref Unsafe.As<byte, char>(ref array.GetRawSzArrayData()), startIndex),
                        Unsafe.As<T, char>(ref value),
                        count);
                    return (result >= 0 ? startIndex : 0) + result;
                }
                else if (Unsafe.SizeOf<T>() == sizeof(int))
                {
                    int result = SpanHelpers.IndexOf(
                        ref Unsafe.Add(ref Unsafe.As<byte, int>(ref array.GetRawSzArrayData()), startIndex),
                        Unsafe.As<T, int>(ref value),
                        count);
                    return (result >= 0 ? startIndex : 0) + result;
                }
                else if (Unsafe.SizeOf<T>() == sizeof(long))
                {
                    int result = SpanHelpers.IndexOf(
                        ref Unsafe.Add(ref Unsafe.As<byte, long>(ref array.GetRawSzArrayData()), startIndex),
                        Unsafe.As<T, long>(ref value),
                        count);
                    return (result >= 0 ? startIndex : 0) + result;
                }
            }

#if !CORERT
            return EqualityComparer<T>.Default.IndexOf(array, value, startIndex, count);
#else
            return IndexOfImpl(array, value, startIndex, count);
#endif
        }

        // Returns the index of the last occurrence of a given value in an array.
        // The array is searched backwards, and the elements of the array are
        // compared to the given value using the Object.Equals method.
        //
        public static int LastIndexOf(Array array, object? value)
        {
            if (array == null)
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.array);
            int lb = array.GetLowerBound(0);
            return LastIndexOf(array, value, array.Length - 1 + lb, array.Length);
        }

        // Returns the index of the last occurrence of a given value in a range of
        // an array. The array is searched backwards, starting at index
        // startIndex and ending at index 0. The elements of the array are
        // compared to the given value using the Object.Equals method.
        //
        public static int LastIndexOf(Array array, object? value, int startIndex)
        {
            if (array == null)
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.array);
            int lb = array.GetLowerBound(0);
            return LastIndexOf(array, value, startIndex, startIndex + 1 - lb);
        }

        // Returns the index of the last occurrence of a given value in a range of
        // an array. The array is searched backwards, starting at index
        // startIndex and counting uptocount elements. The elements of
        // the array are compared to the given value using the Object.Equals
        // method.
        //
        public static int LastIndexOf(Array array, object? value, int startIndex, int count)
        {
            if (array == null)
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.array);
            int lb = array.GetLowerBound(0);
            if (array.Length == 0)
            {
                return lb - 1;
            }

            if (startIndex < lb || startIndex >= array.Length + lb)
                ThrowHelper.ThrowStartIndexArgumentOutOfRange_ArgumentOutOfRange_Index();
            if (count < 0)
                ThrowHelper.ThrowCountArgumentOutOfRange_ArgumentOutOfRange_Count();
            if (count > startIndex - lb + 1)
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.endIndex, ExceptionResource.ArgumentOutOfRange_EndIndexStartIndex);
            if (array.Rank != 1)
                ThrowHelper.ThrowRankException(ExceptionResource.Rank_MultiDimNotSupported);

            int endIndex = startIndex - count + 1;
            if (array is object[] objArray)
            {
                if (value == null)
                {
                    for (int i = startIndex; i >= endIndex; i--)
                    {
                        if (objArray[i] == null)
                            return i;
                    }
                }
                else
                {
                    for (int i = startIndex; i >= endIndex; i--)
                    {
                        object obj = objArray[i];
                        if (obj != null && obj.Equals(value))
                            return i;
                    }
                }
                return -1;
            }

            int result;
            switch (array.GetCorElementTypeOfElementType())
            {
                case CorElementType.ELEMENT_TYPE_BOOLEAN:
                    if (TryGenericLastIndexOf<bool>(array, value, startIndex, count, out result)) return result;
                    break;
                case CorElementType.ELEMENT_TYPE_U1:
                    if (TryGenericLastIndexOf<byte>(array, value, startIndex, count, out result)) return result;
                    break;
                case CorElementType.ELEMENT_TYPE_CHAR:
                    if (TryGenericLastIndexOf<char>(array, value, startIndex, count, out result)) return result;
                    break;
                case CorElementType.ELEMENT_TYPE_R8:
                    if (TryGenericLastIndexOf<double>(array, value, startIndex, count, out result)) return result;
                    break;
                case CorElementType.ELEMENT_TYPE_I2:
                    if (TryGenericLastIndexOf<short>(array, value, startIndex, count, out result)) return result;
                    break;
                case CorElementType.ELEMENT_TYPE_I4:
                    if (TryGenericLastIndexOf<int>(array, value, startIndex, count, out result)) return result;
                    break;
                case CorElementType.ELEMENT_TYPE_I8:
                    if (TryGenericLastIndexOf<long>(array, value, startIndex, count, out result)) return result;
                    break;
                case CorElementType.ELEMENT_TYPE_I1:
                    if (TryGenericLastIndexOf<sbyte>(array, value, startIndex, count, out result)) return result;
                    break;
                case CorElementType.ELEMENT_TYPE_R4:
                    if (TryGenericLastIndexOf<float>(array, value, startIndex, count, out result)) return result;
                    break;
                case CorElementType.ELEMENT_TYPE_U2:
                    if (TryGenericLastIndexOf<ushort>(array, value, startIndex, count, out result)) return result;
                    break;
                case CorElementType.ELEMENT_TYPE_U4:
                    if (TryGenericLastIndexOf<uint>(array, value, startIndex, count, out result)) return result;
                    break;
                case CorElementType.ELEMENT_TYPE_U8:
                    if (TryGenericLastIndexOf<ulong>(array, value, startIndex, count, out result)) return result;
                    break;
                case CorElementType.ELEMENT_TYPE_I:
                    if (TryGenericLastIndexOf<IntPtr>(array, value, startIndex, count, out result)) return result;
                    break;
                case CorElementType.ELEMENT_TYPE_U:
                    if (TryGenericLastIndexOf<UIntPtr>(array, value, startIndex, count, out result)) return result;
                    break;
            }

            static bool TryGenericLastIndexOf<T>(Array array, object? value, int startIndex, int count, out int result) where T : struct
            {
                if (array is T[] arrayOfT)
                {
                    if (value is null)
                    {
                        result = -1;
                        return true;
                    }

                    if (value is T valueOfT)
                    {
                        result = LastIndexOf<T>(arrayOfT, valueOfT, startIndex, count);
                        return true;
                    }
                }

                result = default;
                return false;
            }

            for (int i = startIndex; i >= endIndex; i--)
            {
                object? obj = array.GetValue(i);
                if (obj == null)
                {
                    if (value == null)
                        return i;
                }
                else
                {
                    if (obj.Equals(value))
                        return i;
                }
            }
            return lb - 1;  // Return lb-1 for arrays with negative lower bounds.
        }

        public static int LastIndexOf<T>(T[] array, T value)
        {
            if (array == null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.array);
            }

            return LastIndexOf(array, value, array.Length - 1, array.Length);
        }

        public static int LastIndexOf<T>(T[] array, T value, int startIndex)
        {
            if (array == null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.array);
            }
            // if array is empty and startIndex is 0, we need to pass 0 as count
            return LastIndexOf(array, value, startIndex, (array.Length == 0) ? 0 : (startIndex + 1));
        }

        public static int LastIndexOf<T>(T[] array, T value, int startIndex, int count)
        {
            if (array == null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.array);
            }

            if (array.Length == 0)
            {
                //
                // Special case for 0 length List
                // accept -1 and 0 as valid startIndex for compablility reason.
                //
                if (startIndex != -1 && startIndex != 0)
                {
                    ThrowHelper.ThrowStartIndexArgumentOutOfRange_ArgumentOutOfRange_Index();
                }

                // only 0 is a valid value for count if array is empty
                if (count != 0)
                {
                    ThrowHelper.ThrowCountArgumentOutOfRange_ArgumentOutOfRange_Count();
                }
                return -1;
            }

            // Make sure we're not out of range
            if ((uint)startIndex >= (uint)array.Length)
            {
                ThrowHelper.ThrowStartIndexArgumentOutOfRange_ArgumentOutOfRange_Index();
            }

            // 2nd have of this also catches when startIndex == MAXINT, so MAXINT - 0 + 1 == -1, which is < 0.
            if (count < 0 || startIndex - count + 1 < 0)
            {
                ThrowHelper.ThrowCountArgumentOutOfRange_ArgumentOutOfRange_Count();
            }

            if (RuntimeHelpers.IsBitwiseEquatable<T>())
            {
                if (Unsafe.SizeOf<T>() == sizeof(byte))
                {
                    int endIndex = startIndex - count + 1;
                    int result = SpanHelpers.LastIndexOf(
                        ref Unsafe.Add(ref array.GetRawSzArrayData(), endIndex),
                        Unsafe.As<T, byte>(ref value),
                        count);

                    return (result >= 0 ? endIndex : 0) + result;
                }
                else if (Unsafe.SizeOf<T>() == sizeof(char))
                {
                    int endIndex = startIndex - count + 1;
                    int result = SpanHelpers.LastIndexOf(
                        ref Unsafe.Add(ref Unsafe.As<byte, char>(ref array.GetRawSzArrayData()), endIndex),
                        Unsafe.As<T, char>(ref value),
                        count);

                    return (result >= 0 ? endIndex : 0) + result;
                }
                else if (Unsafe.SizeOf<T>() == sizeof(int))
                {
                    int endIndex = startIndex - count + 1;
                    int result = SpanHelpers.LastIndexOf(
                        ref Unsafe.Add(ref Unsafe.As<byte, int>(ref array.GetRawSzArrayData()), endIndex),
                        Unsafe.As<T, int>(ref value),
                        count);

                    return (result >= 0 ? endIndex : 0) + result;
                }
                else if (Unsafe.SizeOf<T>() == sizeof(long))
                {
                    int endIndex = startIndex - count + 1;
                    int result = SpanHelpers.LastIndexOf(
                        ref Unsafe.Add(ref Unsafe.As<byte, long>(ref array.GetRawSzArrayData()), endIndex),
                        Unsafe.As<T, long>(ref value),
                        count);

                    return (result >= 0 ? endIndex : 0) + result;
                }
            }

#if !CORERT
            return EqualityComparer<T>.Default.LastIndexOf(array, value, startIndex, count);
#else
            return LastIndexOfImpl(array, value, startIndex, count);
#endif
        }

        // Reverses all elements of the given array. Following a call to this
        // method, an element previously located at index i will now be
        // located at index length - i - 1, where length is the
        // length of the array.
        //
        public static void Reverse(Array array)
        {
            if (array == null)
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.array);
            Reverse(array, array.GetLowerBound(0), array.Length);
        }

        // Reverses the elements in a range of an array. Following a call to this
        // method, an element in the range given by index and count
        // which was previously located at index i will now be located at
        // index index + (index + count - i - 1).
        // Reliability note: This may fail because it may have to box objects.
        //
        public static void Reverse(Array array, int index, int length)
        {
            if (array == null)
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.array);
            int lowerBound = array.GetLowerBound(0);
            if (index < lowerBound)
                ThrowHelper.ThrowIndexArgumentOutOfRange_NeedNonNegNumException();
            if (length < 0)
                ThrowHelper.ThrowLengthArgumentOutOfRange_ArgumentOutOfRange_NeedNonNegNum();

            if (array.Length - (index - lowerBound) < length)
                ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_InvalidOffLen);
            if (array.Rank != 1)
                ThrowHelper.ThrowRankException(ExceptionResource.Rank_MultiDimNotSupported);

            if (length <= 1)
                return;

            if (array is object[] objArray)
            {
                Array.Reverse<object>(objArray, index, length);
                return;
            }

            switch (array.GetCorElementTypeOfElementType())
            {
                case CorElementType.ELEMENT_TYPE_BOOLEAN:
                    if (TryGenericReverse<bool>(array, index, length)) return;
                    break;
                case CorElementType.ELEMENT_TYPE_U1:
                case CorElementType.ELEMENT_TYPE_I1: // sbyte[] can be cast to byte[]
                    if (TryGenericReverse<byte>(array, index, length)) return;
                    break;
                case CorElementType.ELEMENT_TYPE_CHAR:
                    if (TryGenericReverse<char>(array, index, length)) return;
                    break;
                case CorElementType.ELEMENT_TYPE_R8:
                    if (TryGenericReverse<double>(array, index, length)) return;
                    break;
                case CorElementType.ELEMENT_TYPE_I2:
                case CorElementType.ELEMENT_TYPE_U2: // ushort[] can be cast to short[]
                    if (TryGenericReverse<short>(array, index, length)) return;
                    break;
                case CorElementType.ELEMENT_TYPE_I4:
                case CorElementType.ELEMENT_TYPE_U4: // uint[] can be cast to int[]
                    if (TryGenericReverse<int>(array, index, length)) return;
                    break;
                case CorElementType.ELEMENT_TYPE_I8:
                case CorElementType.ELEMENT_TYPE_U8: // ulong[] can be cast to long[]
                    if (TryGenericReverse<long>(array, index, length)) return;
                    break;
                case CorElementType.ELEMENT_TYPE_R4:
                    if (TryGenericReverse<float>(array, index, length)) return;
                    break;
                case CorElementType.ELEMENT_TYPE_I:
                case CorElementType.ELEMENT_TYPE_U: // UIntPtr[] can be cast to IntPtr[]
                    if (TryGenericReverse<IntPtr>(array, index, length)) return;
                    break;
            }

            static bool TryGenericReverse<T>(Array array, int index, int length) where T : struct
            {
                if (array is T[] arrayOfT)
                {
                    Reverse<T>(arrayOfT, index, length);
                    return true;
                }
                return false;
            }

            int i = index;
            int j = index + length - 1;
            while (i < j)
            {
                object? temp = array.GetValue(i);
                array.SetValue(array.GetValue(j), i);
                array.SetValue(temp, j);
                i++;
                j--;
            }
        }

        public static void Reverse<T>(T[] array)
        {
            if (array == null)
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.array);
            Reverse(array, 0, array.Length);
        }

        public static void Reverse<T>(T[] array, int index, int length)
        {
            if (array == null)
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.array);
            if (index < 0)
                ThrowHelper.ThrowIndexArgumentOutOfRange_NeedNonNegNumException();
            if (length < 0)
                ThrowHelper.ThrowLengthArgumentOutOfRange_ArgumentOutOfRange_NeedNonNegNum();
            if (array.Length - index < length)
                ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_InvalidOffLen);

            if (length <= 1)
                return;

            ref T first = ref Unsafe.Add(ref Unsafe.As<byte, T>(ref array.GetRawSzArrayData()), index);
            ref T last = ref Unsafe.Add(ref Unsafe.Add(ref first, length), -1);
            do
            {
                T temp = first;
                first = last;
                last = temp;
                first = ref Unsafe.Add(ref first, 1);
                last = ref Unsafe.Add(ref last, -1);
            } while (Unsafe.IsAddressLessThan(ref first, ref last));
        }

        // Sorts the elements of an array. The sort compares the elements to each
        // other using the IComparable interface, which must be implemented
        // by all elements of the array.
        //
        public static void Sort(Array array)
        {
            if (array == null)
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.array);
            Sort(array, null, array.GetLowerBound(0), array.Length, null);
        }

        // Sorts the elements of two arrays based on the keys in the first array.
        // Elements in the keys array specify the sort keys for
        // corresponding elements in the items array. The sort compares the
        // keys to each other using the IComparable interface, which must be
        // implemented by all elements of the keys array.
        //
        public static void Sort(Array keys, Array? items)
        {
            if (keys == null)
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.keys);
            Sort(keys, items, keys.GetLowerBound(0), keys.Length, null);
        }

        // Sorts the elements in a section of an array. The sort compares the
        // elements to each other using the IComparable interface, which
        // must be implemented by all elements in the given section of the array.
        //
        public static void Sort(Array array, int index, int length)
        {
            Sort(array, null, index, length, null);
        }

        // Sorts the elements in a section of two arrays based on the keys in the
        // first array. Elements in the keys array specify the sort keys for
        // corresponding elements in the items array. The sort compares the
        // keys to each other using the IComparable interface, which must be
        // implemented by all elements of the keys array.
        //
        public static void Sort(Array keys, Array? items, int index, int length)
        {
            Sort(keys, items, index, length, null);
        }

        // Sorts the elements of an array. The sort compares the elements to each
        // other using the given IComparer interface. If comparer is
        // null, the elements are compared to each other using the
        // IComparable interface, which in that case must be implemented by
        // all elements of the array.
        //
        public static void Sort(Array array, IComparer? comparer)
        {
            if (array == null)
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.array);
            Sort(array, null, array.GetLowerBound(0), array.Length, comparer);
        }

        // Sorts the elements of two arrays based on the keys in the first array.
        // Elements in the keys array specify the sort keys for
        // corresponding elements in the items array. The sort compares the
        // keys to each other using the given IComparer interface. If
        // comparer is null, the elements are compared to each other using
        // the IComparable interface, which in that case must be implemented
        // by all elements of the keys array.
        //
        public static void Sort(Array keys, Array? items, IComparer? comparer)
        {
            if (keys == null)
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.keys);
            Sort(keys, items, keys.GetLowerBound(0), keys.Length, comparer);
        }

        // Sorts the elements in a section of an array. The sort compares the
        // elements to each other using the given IComparer interface. If
        // comparer is null, the elements are compared to each other using
        // the IComparable interface, which in that case must be implemented
        // by all elements in the given section of the array.
        //
        public static void Sort(Array array, int index, int length, IComparer? comparer)
        {
            Sort(array, null, index, length, comparer);
        }

        // Sorts the elements in a section of two arrays based on the keys in the
        // first array. Elements in the keys array specify the sort keys for
        // corresponding elements in the items array. The sort compares the
        // keys to each other using the given IComparer interface. If
        // comparer is null, the elements are compared to each other using
        // the IComparable interface, which in that case must be implemented
        // by all elements of the given section of the keys array.
        //
        public static void Sort(Array keys, Array? items, int index, int length, IComparer? comparer)
        {
            if (keys == null)
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.keys);
            if (keys.Rank != 1 || (items != null && items.Rank != 1))
                ThrowHelper.ThrowRankException(ExceptionResource.Rank_MultiDimNotSupported);
            int keysLowerBound = keys.GetLowerBound(0);
            if (items != null && keysLowerBound != items.GetLowerBound(0))
                ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_LowerBoundsMustMatch);
            if (index < keysLowerBound)
                ThrowHelper.ThrowIndexArgumentOutOfRange_NeedNonNegNumException();
            if (length < 0)
                ThrowHelper.ThrowLengthArgumentOutOfRange_ArgumentOutOfRange_NeedNonNegNum();

            if (keys.Length - (index - keysLowerBound) < length || (items != null && (index - keysLowerBound) > items.Length - length))
                ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_InvalidOffLen);

            if (length <= 1)
                return;

            comparer ??= Comparer.Default;

            if (keys is object[] objKeys)
            {
                object[]? objItems = items as object[];
                if (items == null || objItems != null)
                {
                    new SorterObjectArray(objKeys, objItems, comparer).Sort(index, length);
                    return;
                }
            }

            if (comparer == Comparer.Default)
            {
                switch (keys.GetCorElementTypeOfElementType())
                {
                    case CorElementType.ELEMENT_TYPE_BOOLEAN:
                        if (TryGenericSort<bool>(keys, items, index, length)) return;
                        break;
                    case CorElementType.ELEMENT_TYPE_U1:
                        if (TryGenericSort<byte>(keys, items, index, length)) return;
                        break;
                    case CorElementType.ELEMENT_TYPE_CHAR:
                        if (TryGenericSort<char>(keys, items, index, length)) return;
                        break;
                    case CorElementType.ELEMENT_TYPE_R8:
                        if (TryGenericSort<double>(keys, items, index, length)) return;
                        break;
                    case CorElementType.ELEMENT_TYPE_I2:
                        if (TryGenericSort<short>(keys, items, index, length)) return;
                        break;
                    case CorElementType.ELEMENT_TYPE_I4:
                        if (TryGenericSort<int>(keys, items, index, length)) return;
                        break;
                    case CorElementType.ELEMENT_TYPE_I8:
                        if (TryGenericSort<long>(keys, items, index, length)) return;
                        break;
                    case CorElementType.ELEMENT_TYPE_I1:
                        if (TryGenericSort<sbyte>(keys, items, index, length)) return;
                        break;
                    case CorElementType.ELEMENT_TYPE_R4:
                        if (TryGenericSort<float>(keys, items, index, length)) return;
                        break;
                    case CorElementType.ELEMENT_TYPE_U2:
                        if (TryGenericSort<ushort>(keys, items, index, length)) return;
                        break;
                    case CorElementType.ELEMENT_TYPE_U4:
                        if (TryGenericSort<uint>(keys, items, index, length)) return;
                        break;
                    case CorElementType.ELEMENT_TYPE_U8:
                        if (TryGenericSort<ulong>(keys, items, index, length)) return;
                        break;
                }

                static bool TryGenericSort<T>(Array keys, Array? items, int index, int length) where T : struct
                {
                    if (keys is T[] keysOfT)
                    {
                        if (items is null)
                        {
                            Sort<T>(keysOfT, index, length);
                            return true;
                        }

                        if (items is T[] itemsOfT)
                        {
                            Sort<T, T>(keysOfT, itemsOfT, index, length);
                            return true;
                        }
                    }

                    return false;
                }
            }

            new SorterGenericArray(keys, items, comparer).Sort(index, length);
        }

        public static void Sort<T>(T[] array)
        {
            if (array == null)
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.array);

            if (array.Length > 1)
            {
                var span = new Span<T>(ref Unsafe.As<byte, T>(ref array.GetRawSzArrayData()), array.Length);
                ArraySortHelper<T>.Default.Sort(span, null);
            }
        }

        public static void Sort<TKey, TValue>(TKey[] keys, TValue[]? items)
        {
            if (keys == null)
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.keys);
            Sort<TKey, TValue>(keys, items, 0, keys.Length, null);
        }

        public static void Sort<T>(T[] array, int index, int length)
        {
            Sort<T>(array, index, length, null);
        }

        public static void Sort<TKey, TValue>(TKey[] keys, TValue[]? items, int index, int length)
        {
            Sort<TKey, TValue>(keys, items, index, length, null);
        }

        public static void Sort<T>(T[] array, System.Collections.Generic.IComparer<T>? comparer)
        {
            if (array == null)
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.array);
            Sort<T>(array, 0, array.Length, comparer);
        }

        public static void Sort<TKey, TValue>(TKey[] keys, TValue[]? items, System.Collections.Generic.IComparer<TKey>? comparer)
        {
            if (keys == null)
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.keys);
            Sort<TKey, TValue>(keys, items, 0, keys.Length, comparer);
        }

        public static void Sort<T>(T[] array, int index, int length, System.Collections.Generic.IComparer<T>? comparer)
        {
            if (array == null)
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.array);
            if (index < 0)
                ThrowHelper.ThrowIndexArgumentOutOfRange_NeedNonNegNumException();
            if (length < 0)
                ThrowHelper.ThrowLengthArgumentOutOfRange_ArgumentOutOfRange_NeedNonNegNum();
            if (array.Length - index < length)
                ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_InvalidOffLen);

            if (length > 1)
            {
                var span = new Span<T>(ref Unsafe.Add(ref Unsafe.As<byte, T>(ref array.GetRawSzArrayData()), index), length);
                ArraySortHelper<T>.Default.Sort(span, comparer);
            }
        }

        public static void Sort<TKey, TValue>(TKey[] keys, TValue[]? items, int index, int length, System.Collections.Generic.IComparer<TKey>? comparer)
        {
            if (keys == null)
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.keys);
            if (index < 0)
                ThrowHelper.ThrowIndexArgumentOutOfRange_NeedNonNegNumException();
            if (length < 0)
                ThrowHelper.ThrowLengthArgumentOutOfRange_ArgumentOutOfRange_NeedNonNegNum();
            if (keys.Length - index < length || (items != null && index > items.Length - length))
                ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_InvalidOffLen);

            if (length > 1)
            {
                if (items == null)
                {
                    Sort<TKey>(keys, index, length, comparer);
                    return;
                }

                var spanKeys = new Span<TKey>(ref Unsafe.Add(ref Unsafe.As<byte, TKey>(ref keys.GetRawSzArrayData()), index), length);
                var spanItems = new Span<TValue>(ref Unsafe.Add(ref Unsafe.As<byte, TValue>(ref items.GetRawSzArrayData()), index), length);
                ArraySortHelper<TKey, TValue>.Default.Sort(spanKeys, spanItems, comparer);
            }
        }

        public static void Sort<T>(T[] array, Comparison<T> comparison)
        {
            if (array == null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.array);
            }

            if (comparison == null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.comparison);
            }

            var span = new Span<T>(ref Unsafe.As<byte, T>(ref array.GetRawSzArrayData()), array.Length);
            ArraySortHelper<T>.Sort(span, comparison);
        }

        public static bool TrueForAll<T>(T[] array, Predicate<T> match)
        {
            if (array == null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.array);
            }

            if (match == null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.match);
            }

            for (int i = 0; i < array.Length; i++)
            {
                if (!match(array[i]))
                {
                    return false;
                }
            }
            return true;
        }

        // Private value type used by the Sort methods.
        private readonly struct SorterObjectArray
        {
            private readonly object[] keys;
            private readonly object?[]? items;
            private readonly IComparer comparer;

            internal SorterObjectArray(object[] keys, object?[]? items, IComparer comparer)
            {
                this.keys = keys;
                this.items = items;
                this.comparer = comparer;
            }

            internal void SwapIfGreater(int a, int b)
            {
                if (a != b)
                {
                    if (comparer.Compare(keys[a], keys[b]) > 0)
                    {
                        object temp = keys[a];
                        keys[a] = keys[b];
                        keys[b] = temp;
                        if (items != null)
                        {
                            object? item = items[a];
                            items[a] = items[b];
                            items[b] = item;
                        }
                    }
                }
            }

            private void Swap(int i, int j)
            {
                object t = keys[i];
                keys[i] = keys[j];
                keys[j] = t;

                if (items != null)
                {
                    object? item = items[i];
                    items[i] = items[j];
                    items[j] = item;
                }
            }

            internal void Sort(int left, int length)
            {
                IntrospectiveSort(left, length);
            }

            private void IntrospectiveSort(int left, int length)
            {
                if (length < 2)
                    return;

                try
                {
                    IntroSort(left, length + left - 1, 2 * IntrospectiveSortUtilities.FloorLog2PlusOne(length));
                }
                catch (IndexOutOfRangeException)
                {
                    IntrospectiveSortUtilities.ThrowOrIgnoreBadComparer(comparer);
                }
                catch (Exception e)
                {
                    ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_IComparerFailed, e);
                }
            }

            private void IntroSort(int lo, int hi, int depthLimit)
            {
                while (hi > lo)
                {
                    int partitionSize = hi - lo + 1;
                    if (partitionSize <= IntrospectiveSortUtilities.IntrosortSizeThreshold)
                    {
                        if (partitionSize == 1)
                        {
                            return;
                        }
                        if (partitionSize == 2)
                        {
                            SwapIfGreater(lo, hi);
                            return;
                        }
                        if (partitionSize == 3)
                        {
                            SwapIfGreater(lo, hi - 1);
                            SwapIfGreater(lo, hi);
                            SwapIfGreater(hi - 1, hi);
                            return;
                        }

                        InsertionSort(lo, hi);
                        return;
                    }

                    if (depthLimit == 0)
                    {
                        Heapsort(lo, hi);
                        return;
                    }
                    depthLimit--;

                    int p = PickPivotAndPartition(lo, hi);
                    IntroSort(p + 1, hi, depthLimit);
                    hi = p - 1;
                }
            }

            private int PickPivotAndPartition(int lo, int hi)
            {
                // Compute median-of-three.  But also partition them, since we've done the comparison.
                int mid = lo + (hi - lo) / 2;
                // Sort lo, mid and hi appropriately, then pick mid as the pivot.
                SwapIfGreater(lo, mid);
                SwapIfGreater(lo, hi);
                SwapIfGreater(mid, hi);

                object pivot = keys[mid];
                Swap(mid, hi - 1);
                int left = lo, right = hi - 1;  // We already partitioned lo and hi and put the pivot in hi - 1.  And we pre-increment & decrement below.

                while (left < right)
                {
                    while (comparer.Compare(keys[++left], pivot) < 0) ;
                    while (comparer.Compare(pivot, keys[--right]) < 0) ;

                    if (left >= right)
                        break;

                    Swap(left, right);
                }

                // Put pivot in the right location.
                Swap(left, hi - 1);
                return left;
            }

            private void Heapsort(int lo, int hi)
            {
                int n = hi - lo + 1;
                for (int i = n / 2; i >= 1; i--)
                {
                    DownHeap(i, n, lo);
                }
                for (int i = n; i > 1; i--)
                {
                    Swap(lo, lo + i - 1);

                    DownHeap(1, i - 1, lo);
                }
            }

            private void DownHeap(int i, int n, int lo)
            {
                object d = keys[lo + i - 1];
                object? dt = items?[lo + i - 1];
                int child;
                while (i <= n / 2)
                {
                    child = 2 * i;
                    if (child < n && comparer.Compare(keys[lo + child - 1], keys[lo + child]) < 0)
                    {
                        child++;
                    }
                    if (!(comparer.Compare(d, keys[lo + child - 1]) < 0))
                        break;
                    keys[lo + i - 1] = keys[lo + child - 1];
                    if (items != null)
                        items[lo + i - 1] = items[lo + child - 1];
                    i = child;
                }
                keys[lo + i - 1] = d;
                if (items != null)
                    items[lo + i - 1] = dt;
            }

            private void InsertionSort(int lo, int hi)
            {
                int i, j;
                object t;
                object? ti;
                for (i = lo; i < hi; i++)
                {
                    j = i;
                    t = keys[i + 1];
                    ti = items?[i + 1];
                    while (j >= lo && comparer.Compare(t, keys[j]) < 0)
                    {
                        keys[j + 1] = keys[j];
                        if (items != null)
                            items[j + 1] = items[j];
                        j--;
                    }
                    keys[j + 1] = t;
                    if (items != null)
                        items[j + 1] = ti;
                }
            }
        }

        // Private value used by the Sort methods for instances of Array.
        // This is slower than the one for Object[], since we can't use the JIT helpers
        // to access the elements.  We must use GetValue & SetValue.
        private readonly struct SorterGenericArray
        {
            private readonly Array keys;
            private readonly Array? items;
            private readonly IComparer comparer;

            internal SorterGenericArray(Array keys, Array? items, IComparer comparer)
            {
                this.keys = keys;
                this.items = items;
                this.comparer = comparer;
            }

            internal void SwapIfGreater(int a, int b)
            {
                if (a != b)
                {
                    if (comparer.Compare(keys.GetValue(a), keys.GetValue(b)) > 0)
                    {
                        object? key = keys.GetValue(a);
                        keys.SetValue(keys.GetValue(b), a);
                        keys.SetValue(key, b);
                        if (items != null)
                        {
                            object? item = items.GetValue(a);
                            items.SetValue(items.GetValue(b), a);
                            items.SetValue(item, b);
                        }
                    }
                }
            }

            private void Swap(int i, int j)
            {
                object? t1 = keys.GetValue(i);
                keys.SetValue(keys.GetValue(j), i);
                keys.SetValue(t1, j);

                if (items != null)
                {
                    object? t2 = items.GetValue(i);
                    items.SetValue(items.GetValue(j), i);
                    items.SetValue(t2, j);
                }
            }

            internal void Sort(int left, int length)
            {
                IntrospectiveSort(left, length);
            }

            private void IntrospectiveSort(int left, int length)
            {
                if (length < 2)
                    return;

                try
                {
                    IntroSort(left, length + left - 1, 2 * IntrospectiveSortUtilities.FloorLog2PlusOne(length));
                }
                catch (IndexOutOfRangeException)
                {
                    IntrospectiveSortUtilities.ThrowOrIgnoreBadComparer(comparer);
                }
                catch (Exception e)
                {
                    ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_IComparerFailed, e);
                }
            }

            private void IntroSort(int lo, int hi, int depthLimit)
            {
                while (hi > lo)
                {
                    int partitionSize = hi - lo + 1;
                    if (partitionSize <= IntrospectiveSortUtilities.IntrosortSizeThreshold)
                    {
                        if (partitionSize == 1)
                        {
                            return;
                        }
                        if (partitionSize == 2)
                        {
                            SwapIfGreater(lo, hi);
                            return;
                        }
                        if (partitionSize == 3)
                        {
                            SwapIfGreater(lo, hi - 1);
                            SwapIfGreater(lo, hi);
                            SwapIfGreater(hi - 1, hi);
                            return;
                        }

                        InsertionSort(lo, hi);
                        return;
                    }

                    if (depthLimit == 0)
                    {
                        Heapsort(lo, hi);
                        return;
                    }
                    depthLimit--;

                    int p = PickPivotAndPartition(lo, hi);
                    IntroSort(p + 1, hi, depthLimit);
                    hi = p - 1;
                }
            }

            private int PickPivotAndPartition(int lo, int hi)
            {
                // Compute median-of-three.  But also partition them, since we've done the comparison.
                int mid = lo + (hi - lo) / 2;

                SwapIfGreater(lo, mid);
                SwapIfGreater(lo, hi);
                SwapIfGreater(mid, hi);

                object? pivot = keys.GetValue(mid);
                Swap(mid, hi - 1);
                int left = lo, right = hi - 1;  // We already partitioned lo and hi and put the pivot in hi - 1.  And we pre-increment & decrement below.

                while (left < right)
                {
                    while (comparer.Compare(keys.GetValue(++left), pivot) < 0) ;
                    while (comparer.Compare(pivot, keys.GetValue(--right)) < 0) ;

                    if (left >= right)
                        break;

                    Swap(left, right);
                }

                // Put pivot in the right location.
                Swap(left, hi - 1);
                return left;
            }

            private void Heapsort(int lo, int hi)
            {
                int n = hi - lo + 1;
                for (int i = n / 2; i >= 1; i--)
                {
                    DownHeap(i, n, lo);
                }
                for (int i = n; i > 1; i--)
                {
                    Swap(lo, lo + i - 1);

                    DownHeap(1, i - 1, lo);
                }
            }

            private void DownHeap(int i, int n, int lo)
            {
                object? d = keys.GetValue(lo + i - 1);
                object? dt = items?.GetValue(lo + i - 1);
                int child;
                while (i <= n / 2)
                {
                    child = 2 * i;
                    if (child < n && comparer.Compare(keys.GetValue(lo + child - 1), keys.GetValue(lo + child)) < 0)
                    {
                        child++;
                    }

                    if (!(comparer.Compare(d, keys.GetValue(lo + child - 1)) < 0))
                        break;

                    keys.SetValue(keys.GetValue(lo + child - 1), lo + i - 1);
                    if (items != null)
                        items.SetValue(items.GetValue(lo + child - 1), lo + i - 1);
                    i = child;
                }
                keys.SetValue(d, lo + i - 1);
                if (items != null)
                    items.SetValue(dt, lo + i - 1);
            }

            private void InsertionSort(int lo, int hi)
            {
                int i, j;
                object? t;
                object? dt;
                for (i = lo; i < hi; i++)
                {
                    j = i;
                    t = keys.GetValue(i + 1);
                    dt = items?.GetValue(i + 1);

                    while (j >= lo && comparer.Compare(t, keys.GetValue(j)) < 0)
                    {
                        keys.SetValue(keys.GetValue(j), j + 1);
                        if (items != null)
                            items.SetValue(items.GetValue(j), j + 1);
                        j--;
                    }

                    keys.SetValue(t, j + 1);
                    if (items != null)
                        items.SetValue(dt, j + 1);
                }
            }
        }

#if !CORERT
        public IEnumerator GetEnumerator()
        {
            int lowerBound = GetLowerBound(0);
            if (Rank == 1 && lowerBound == 0)
                return new SZArrayEnumerator(this);
            else
                return new ArrayEnumerator(this, lowerBound, Length);
        }
#endif // !CORERT
    }
}
