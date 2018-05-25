// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Globalization;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using System.Runtime.Versioning;

namespace System.Collections.Generic
{
    internal interface IArraySortHelper<TKey>
    {
        void Sort(TKey[] keys, int index, int length, IComparer<TKey> comparer);
        int BinarySearch(TKey[] keys, int index, int length, TKey value, IComparer<TKey> comparer);
    }

    [TypeDependencyAttribute("System.Collections.Generic.GenericArraySortHelper`1")]
    internal partial class ArraySortHelper<T>
        : IArraySortHelper<T>
    {
        private static volatile IArraySortHelper<T> defaultArraySortHelper;

        public static IArraySortHelper<T> Default
        {
            get
            {
                IArraySortHelper<T> sorter = defaultArraySortHelper;
                if (sorter == null)
                    sorter = CreateArraySortHelper();

                return sorter;
            }
        }

        private static IArraySortHelper<T> CreateArraySortHelper()
        {
            if (typeof(IComparable<T>).IsAssignableFrom(typeof(T)))
            {
                defaultArraySortHelper = (IArraySortHelper<T>)RuntimeTypeHandle.Allocate(typeof(GenericArraySortHelper<string>).TypeHandle.Instantiate(new Type[] { typeof(T) }));
            }
            else
            {
                defaultArraySortHelper = new ArraySortHelper<T>();
            }
            return defaultArraySortHelper;
        }

        void IArraySortHelper<T>.Sort(T[] keys, int index, int length, IComparer<T> comparer)
        {
            Sort(keys, index, length, comparer);
        }

        int IArraySortHelper<T>.BinarySearch(T[] array, int index, int length, T value, IComparer<T> comparer)
        {
            return BinarySearch(array, index, length, value, comparer);
        }
    }

    internal partial class GenericArraySortHelper<T>
        : IArraySortHelper<T>
    {
    }

    internal interface IArraySortHelper<TKey, TValue>
    {
        void Sort(TKey[] keys, TValue[] values, int index, int length, IComparer<TKey> comparer);
    }

    [TypeDependencyAttribute("System.Collections.Generic.GenericArraySortHelper`2")]
    internal partial class ArraySortHelper<TKey, TValue>
        : IArraySortHelper<TKey, TValue>
    {
        private static volatile IArraySortHelper<TKey, TValue> defaultArraySortHelper;

        public static IArraySortHelper<TKey, TValue> Default
        {
            get
            {
                IArraySortHelper<TKey, TValue> sorter = defaultArraySortHelper;
                if (sorter == null)
                    sorter = CreateArraySortHelper();

                return sorter;
            }
        }

        private static IArraySortHelper<TKey, TValue> CreateArraySortHelper()
        {
            if (typeof(IComparable<TKey>).IsAssignableFrom(typeof(TKey)))
            {
                defaultArraySortHelper = (IArraySortHelper<TKey, TValue>)RuntimeTypeHandle.Allocate(typeof(GenericArraySortHelper<string, string>).TypeHandle.Instantiate(new Type[] { typeof(TKey), typeof(TValue) }));
            }
            else
            {
                defaultArraySortHelper = new ArraySortHelper<TKey, TValue>();
            }
            return defaultArraySortHelper;
        }

        void IArraySortHelper<TKey, TValue>.Sort(TKey[] keys, TValue[] values, int index, int length, IComparer<TKey> comparer)
        {
            Sort(keys, values, index, length, comparer);
        }        
    }

    internal partial class GenericArraySortHelper<TKey, TValue>
        : IArraySortHelper<TKey, TValue>
    {
    }
}