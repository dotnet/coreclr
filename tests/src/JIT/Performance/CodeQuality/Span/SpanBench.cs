// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Xunit;
using Microsoft.Xunit.Performance;

[assembly: OptimizeForBenchmarks]
[assembly: MeasureInstructionsRetired]

namespace Span
{
    public class SpanBench
    {

#if DEBUG
        const int BubbleSortIterations = 1;
        const int QuickSortIterations = 1;
        const int FillAllIterations = 1;
#else
        const int BubbleSortIterations = 1000;
        const int QuickSortIterations = 10000;
        const int FillAllIterations = 1000000;
 #endif

        const int Size = 1024;

        const bool OUTPUT = false;
        const int BaseIterations = 100000000;

        [MethodImpl(MethodImplOptions.NoInlining)]
        static void TestFillAllSpan(Span<byte> span)
        {
            for (int i = 0; i < span.Length; ++i) {
                span[i] = unchecked((byte)i);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        static void TestFillAllArray(byte[] data)
        {
            for (int i = 0; i < data.Length; ++i) {
                data[i] = unchecked((byte)i);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        static void TestFillAllReverseSpan(Span<byte> span)
        {
            for (int i = span.Length; --i >= 0;) {
                span[i] = unchecked((byte)i);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        static void TestFillAllReverseArray(byte[] data)
        {
            for (int i = data.Length; --i >= 0;) {
                data[i] = unchecked((byte)i);
            }
        }

        static int[] GetUnsortedData()
        {
            int[] unsortedData = new int[Size];
            Random r = new Random(42);
            for (int i = 0; i < unsortedData.Length; ++i)
            {
                unsortedData[i] = r.Next();
            }
            return unsortedData;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        static void TestBubbleSortSpan(Span<int> span)
        {
            bool swap;
            int temp;
            int n = span.Length - 1;
            do {
                swap = false;
                for (int i = 0; i < n; i++) {
                    if (span[i] > span[i + 1]) {
                        temp = span[i];
                        span[i] = span[i + 1];
                        span[i + 1] = temp;
                        swap = true;
                    }
                }
                --n;
            }
            while (swap);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        static void TestBubbleSortArray(int[] data)
        {
            bool swap;
            int temp;
            int n = data.Length - 1;
            do {
                swap = false;
                for (int i = 0; i < n; i++) {
                    if (data[i] > data[i + 1]) {
                        temp = data[i];
                        data[i] = data[i + 1];
                        data[i + 1] = temp;
                        swap = true;
                    }
                }
                --n;
            }
            while (swap);
        }

        static void TestQuickSortSpan(Span<int> data)
        {
            QuickSortSpan(data);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        static void QuickSortSpan(Span<int> data)
        {
            if (data.Length <= 1) {
                return;
            }

            int lo = 0;
            int hi = data.Length - 1;
            int i, j;
            int pivot, temp;
            for (i = lo, j = hi, pivot = data[hi]; i < j;) {
                while (i < j && data[i] <= pivot) {
                    ++i;
                }
                while (j > i && data[j] >= pivot) {
                    --j;
                }
                if (i < j) {
                    temp = data[i];
                    data[i] = data[j];
                    data[j] = temp;
                }
            }
            if (i != hi) {
                temp = data[i];
                data[i] = pivot;
                data[hi] = temp;
            }

            QuickSortSpan(data.Slice(0, i));
            QuickSortSpan(data.Slice(i + 1));
        }

        static void TestQuickSortArray(int[] data)
        {
            QuickSortArray(data, 0, data.Length - 1);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        static void QuickSortArray(int[] data, int lo, int hi)
        {
            if (lo >= hi) {
                return;
            }

            int i, j;
            int pivot, temp;
            for (i = lo, j = hi, pivot = data[hi]; i < j;) {
                while (i < j && data[i] <= pivot) {
                    ++i;
                }
                while (j > i && data[j] >= pivot) {
                    --j;
                }
                if (i < j) {
                    temp = data[i];
                    data[i] = data[j];
                    data[j] = temp;
                }
            }
            if (i != hi) {
                temp = data[i];
                data[i] = pivot;
                data[hi] = temp;
            }

            QuickSortArray(data, lo, i - 1);
            QuickSortArray(data, i + 1, hi);
        }

        // XUNIT-PERF tests

        /*[Benchmark]
        public static void FillAllSpan()
        {
            byte[] a = new byte[Size];
            Span<byte> s = new Span<byte>(a);
            foreach (var iteration in Benchmark.Iterations)
            {
                using (iteration.StartMeasurement())
                {
                    for (int i = 0; i < FillAllIterations; i++)
                    {
                        TestFillAllSpan(s);
                    }
                }
            }
        }

        [Benchmark]
        public static void FillAllArray()
        {
            byte[] a = new byte[Size];
            foreach (var iteration in Benchmark.Iterations)
            {
                using (iteration.StartMeasurement())
                {
                    for (int i = 0; i < FillAllIterations; i++)
                    {
                        TestFillAllArray(a);
                    }
                }
            }
        }

        [Benchmark]
        public static void FillAllReverseSpan()
        {
            byte[] a = new byte[Size];
            Span<byte> s = new Span<byte>(a);
            foreach (var iteration in Benchmark.Iterations)
            {
                using (iteration.StartMeasurement())
                {
                    for (int i = 0; i < FillAllIterations; i++)
                    {
                        TestFillAllReverseSpan(s);
                    }
                }
            }
        }

        [Benchmark]
        public static void FillAllReverseArray()
        {
            byte[] a = new byte[Size];
            foreach (var iteration in Benchmark.Iterations)
            {
                using (iteration.StartMeasurement())
                {
                    for (int i = 0; i < FillAllIterations; i++)
                    {
                        TestFillAllReverseArray(a);
                    }
                }
            }
        }

        [Benchmark]
        public static void QuickSortSpan()
        {
            int[] data = new int[Size];
            int[] unsortedData = GetUnsortedData();
            Span<int> span = new Span<int>(data);

            foreach (var iteration in Benchmark.Iterations)
            {
                using (iteration.StartMeasurement())
                {
                    for (int i = 0; i < QuickSortIterations; i++)
                    {
                        Array.Copy(unsortedData, data, Size);
                        TestQuickSortSpan(span);
                    }
                }
            }
        }

        [Benchmark]
        public static void BubbleSortSpan()
        {
            int[] data = new int[Size];
            int[] unsortedData = GetUnsortedData();
            Span<int> span = new Span<int>(data);

            foreach (var iteration in Benchmark.Iterations)
            {
                using (iteration.StartMeasurement())
                {
                    for (int i = 0; i < BubbleSortIterations; i++)
                    {
                        Array.Copy(unsortedData, data, Size);
                        TestBubbleSortSpan(span);
                    }
                }
            }
        }

        [Benchmark]
        public static void QuickSortArray()
        {
            int[] data = new int[Size];
            int[] unsortedData = GetUnsortedData();

            foreach (var iteration in Benchmark.Iterations)
            {
                using (iteration.StartMeasurement())
                {
                    for (int i = 0; i < QuickSortIterations; i++)
                    {
                        Array.Copy(unsortedData, data, Size);
                        TestQuickSortArray(data);
                    }
                }
            }
        }

        [Benchmark]
        public static void BubbleSortArray()
        {
            int[] data = new int[Size];
            int[] unsortedData = GetUnsortedData();

            foreach (var iteration in Benchmark.Iterations)
            {
                using (iteration.StartMeasurement())
                {
                    for (int i = 0; i < BubbleSortIterations; i++)
                    {
                        Array.Copy(unsortedData, data, Size);
                        TestBubbleSortArray(data);
                    }
                }
            }
        }*/

        // EXE-based testing

        static void FillAllSpanBase()
        {
            byte[] a = new byte[Size];
            Span<byte> s = new Span<byte>(a);
            for (int i = 0; i < FillAllIterations; i++)
            {
                TestFillAllSpan(s);
            }
        }

        static void FillAllArrayBase()
        {
            byte[] a = new byte[Size];
            for (int i = 0; i < FillAllIterations; i++)
            {
                TestFillAllArray(a);
            }
        }

        static void FillAllReverseSpanBase()
        {
            byte[] a = new byte[Size];
            Span<byte> s = new Span<byte>(a);
            for (int i = 0; i < FillAllIterations; i++)
            {
                TestFillAllReverseSpan(s);
            }
        }

        static void FillAllReverseArrayBase()
        {
            byte[] a = new byte[Size];
            for (int i = 0; i < FillAllIterations; i++)
            {
                TestFillAllReverseArray(a);
            }
        }

        static void QuickSortSpanBase()
        {
            int[] data = new int[Size];
            int[] unsortedData = GetUnsortedData();
            Span<int> span = new Span<int>(data);

            for (int i = 0; i < QuickSortIterations; i++)
            {
                Array.Copy(unsortedData, data, Size);
                TestQuickSortSpan(span);
            }
        }

        static void BubbleSortSpanBase()
        {
            int[] data = new int[Size];
            int[] unsortedData = GetUnsortedData();
            Span<int> span = new Span<int>(data);

            for (int i = 0; i < BubbleSortIterations; i++)
            {
                Array.Copy(unsortedData, data, Size);
                TestBubbleSortSpan(span);
            }
        }

        static void QuickSortArrayBase()
        {
            int[] data = new int[Size];
            int[] unsortedData = GetUnsortedData();

            for (int i = 0; i < QuickSortIterations; i++)
            {
                Array.Copy(unsortedData, data, Size);
                TestQuickSortArray(data);
            }
        }

        static void BubbleSortArrayBase()
        {
            int[] data = new int[Size];
            int[] unsortedData = GetUnsortedData();

            for (int i = 0; i < BubbleSortIterations; i++)
            {
                Array.Copy(unsortedData, data, Size);
                TestBubbleSortArray(data);
            }
        }

        static double Bench(Action f)
        {
            Stopwatch sw = Stopwatch.StartNew();
            f();
            sw.Stop();
            return sw.Elapsed.TotalMilliseconds;
        }

        static IEnumerable<object[]> MakeArgs(params string[] args)
        {
            return args.Select(arg => new object[] { arg });
        }

        static IEnumerable<object[]> TestFuncs = MakeArgs(
            "FillAllSpanBase", "FillAllArrayBase",
            "FillAllReverseSpanBase", "FillAllReverseArrayBase",
            "BubbleSortSpanBase", "BubbleSortArrayBase",
            "QuickSortSpanBase", "QuickSortArrayBase"
        );

        static Action LookupFunc(object o)
        {
            TypeInfo t = typeof(SpanBench).GetTypeInfo();
            MethodInfo m = t.GetDeclaredMethod((string) o);
            return m.CreateDelegate(typeof(Action)) as Action;
        }

        #region TestSpan<X>
        /*
        private static void TestSpanConstructor()
        {
            TestSpanConstructor<byte>(typeof(byte));
            TestSpanConstructor<int>();
            TestSpanConstructor<string>();
            TestSpanConstructor<TestStruct>();
        }

        private static void TestSpanDangerousCreate()
        {
            TestSpanDangerousCreate<byte>();
            TestSpanDangerousCreate<int>();
            TestSpanDangerousCreate<string>();
            TestSpanDangerousCreate<TestStruct>();
        }

        private static void TestSpanDangerousGetPinnableReference()
        {
            TestSpanDangerousGetPinnableReference<byte>();
            TestSpanDangerousGetPinnableReference<int>();
            TestSpanDangerousGetPinnableReference<string>();
            TestSpanDangerousGetPinnableReference<TestStruct>();
        }

        private static void TestSpanIndex()
        {
            TestSpanIndex<byte>();
            TestSpanIndex<int>();
            TestSpanIndex<string>();
            TestSpanIndex<TestStruct>();
        }

        private static void TestSpanSlice()
        {
            TestSpanSlice<byte>();
            TestSpanSlice<int>();
            TestSpanSlice<string>();
            TestSpanSlice<TestStruct>();
        }

        private static void TestSpanToArray()
        {
            TestSpanToArray<byte>();
            TestSpanToArray<int>();
            TestSpanToArray<string>();
            TestSpanToArray<TestStruct>();
        }

        private static void TestSpanCopyTo()
        {
            TestSpanCopyTo<byte>();
            TestSpanCopyTo<int>();
            TestSpanCopyTo<string>();
            TestSpanCopyTo<TestStruct>();
        }

        private static void TestSpanFill()
        {
            TestSpanFill<byte>();
            TestSpanFill<int>();
            TestSpanFill<string>();
            TestSpanFill<TestStruct>();
        }

        private static void TestSpanClear()
        {
            TestSpanClear<byte>();
            TestSpanClear<int>();
            TestSpanClear<string>();
            TestSpanClear<TestStruct>();
        }

        private static void TestSpanAsBytes()
        {
            TestSpanAsBytes<byte>();
            TestSpanAsBytes<int>();
        }

        private static void TestSpanNonPortableCast()
        {
            TestSpanNonPortableCast<byte, int>();
            TestSpanNonPortableCast<int, byte>();
        }

        private static void TestSpanSliceString()
        {
            TestSpanSliceStringChar();
        }*/
        #endregion

        #region TestSpan<X><T>
        [Benchmark(InnerIterationCount = 1000)]
        [InlineData(1, BaseIterations * 10)]
        [InlineData(5, BaseIterations * 10)]
        [InlineData(10, BaseIterations * 10)]
        [InlineData(100, BaseIterations * 10)]
        [InlineData(500, BaseIterations * 10)]
        [InlineData(1000, BaseIterations * 10)]
        [InlineData(10000, BaseIterations * 10)]
        [InlineData(10000000, BaseIterations * 10)]
        public static void TestSpanConstructor(int length, int innerIter)
        {
            var array = new byte[length];
            foreach (var iteration in Benchmark.Iterations)
            {
                using (iteration.StartMeasurement())
                {
                    for (int i = 0; i < Benchmark.InnerIterationCount; i++)
                        PerfTestConstructor<byte>(array, innerIter);
                }
            }
        }

        /*[Benchmark]
        public static void TestSpanDangerousCreate<T>()
        {
            TestClass<T>[] classes = GetClasses<T>();
            int[] iterations = GetIterationsForConstructor<T>();

            for (int i = 0; i < classes.Length; i++)
            {
                TestClass<T> aClass = classes[i];
                int innerIter = iterations[i];

                foreach (var iteration in Benchmark.Iterations)
                {
                    using (iteration.StartMeasurement())
                    {
                        PerfTestDangerousCreate<T>(aClass, innerIter);
                    }
                }
            }
        }

        [Benchmark]
        public static void TestSpanDangerousGetPinnableReference<T>()
        {
            T[][] arrays = GetArrays<T>();
            int[] iterations = GetIterationsForDangerousGetPinnableReference<T>();

            for (int i = 0; i < arrays.Length; i++)
            {
                Span<T> span = new Span<T>(arrays[i]);
                int innerIter = iterations[i];

                foreach (var iteration in Benchmark.Iterations)
                {
                    using (iteration.StartMeasurement())
                    {
                        PerfTestDangerousGetPinnableReference<T>(span, innerIter);
                    }
                }
            }
        }

        [Benchmark]
        public static void TestSpanIndex<T>()
        {
            T[][] arrays = GetArrays<T>();
            int[] iterations = GetIterationsForIndex<T>();

            for (int i = 0; i < arrays.Length; i++)
            {
                Span<T> span = new Span<T>(arrays[i]);
                int innerIter = iterations[i];

                foreach (var iteration in Benchmark.Iterations)
                {
                    using (iteration.StartMeasurement())
                    {
                        PerfTestIndex<T>(span, innerIter);
                    }
                }
            }
        }

        [Benchmark]
        public static void TestArrayIndex<T>()
        {
            T[][] arrays = GetArrays<T>();
            int[] iterations = GetIterationsForIndex<T>();

            for (int i = 0; i < arrays.Length; i++)
            {
                var array = arrays[i];
                int innerIter = iterations[i];
                foreach (var iteration in Benchmark.Iterations)
                {
                    using (iteration.StartMeasurement())
                    {
                        PerfTestIndex<T>(array, innerIter);
                    }
                }
            }
        }

        [Benchmark]
        public static void TestSpanSlice<T>()
        {
            T[][] arrays = GetArrays<T>();
            int[] iterations = GetIterationsForSlice<T>();

            for (int i = 0; i < arrays.Length; i++)
            {
                Span<T> span = new Span<T>(arrays[i]);
                int innerIter = iterations[i];

                foreach (var iteration in Benchmark.Iterations)
                {
                    using (iteration.StartMeasurement())
                    {
                        PerfTestSlice<T>(span, innerIter);
                    }
                }
            }
        }

        [Benchmark]
        public static void TestSpanToArray<T>()
        {
            T[][] arrays = GetArrays<T>();
            int[] iterations = GetIterationsForToArray<T>();

            for (int i = 0; i < arrays.Length; i++)
            {
                Span<T> span = new Span<T>(arrays[i]);
                int innerIter = iterations[i];
                foreach (var iteration in Benchmark.Iterations)
                {
                    using (iteration.StartMeasurement())
                    {
                        PerfTestToArray<T>(span, innerIter);
                    }
                }
            }
        }

        [Benchmark]
        public static void TestSpanCopyTo<T>()
        {
            T[][] arrays = GetArrays<T>();
            int[] iterations = GetIterationsForCopyTo<T>();

            for (int i = 0; i < arrays.Length; i++)
            {
                Span<T> span = new Span<T>(arrays[i]);
                var destArray = new T[arrays[i].Length];
                Span<T> destination = new Span<T>(destArray);
                int innerIter = iterations[i];
                foreach (var iteration in Benchmark.Iterations)
                {
                    using (iteration.StartMeasurement())
                    {
                        PerfTestCopyTo<T>(span, destination, innerIter);
                    }
                }
            }
        }

        [Benchmark]
        public static void TestArrayCopyTo<T>()
        {
            T[][] arrays = GetArrays<T>();
            int[] iterations = GetIterationsForCopyTo<T>();

            for (int i = 0; i < arrays.Length; i++)
            {
                var array = arrays[i];
                var destArray = new T[arrays[i].Length];
                int innerIter = iterations[i];
                foreach (var iteration in Benchmark.Iterations)
                {
                    using (iteration.StartMeasurement())
                    {
                        PerfTestCopyTo<T>(array, destArray, innerIter);
                    }
                }
            }
        }

        [Benchmark]
        public static void TestSpanFill<T>()
        {
            T[][] arrays = GetArrays<T>();
            int[] iterations = GetIterationsForFill<T>();

            for (int i = 0; i < arrays.Length; i++)
            {
                Span<T> span = new Span<T>(arrays[i]);
                int innerIter = iterations[i];
                foreach (var iteration in Benchmark.Iterations)
                {
                    using (iteration.StartMeasurement())
                    {
                        PerfTestFill<T>(span, innerIter);
                    }
                }
            }
        }

        [Benchmark]
        public static void TestSpanClear<T>()
        {
            T[][] arrays = GetArrays<T>();
            int[] iterations = GetIterationsForClear<T>();

            for (int i = 0; i < arrays.Length; i++)
            {
                Span<T> span = new Span<T>(arrays[i]);
                int innerIter = iterations[i];
                foreach (var iteration in Benchmark.Iterations)
                {
                    using (iteration.StartMeasurement())
                    {
                        PerfTestClear<T>(span, innerIter);
                    }
                }
            }
        }

        [Benchmark]
        public static void TestArrayClear<T>()
        {
            T[][] arrays = GetArrays<T>();
            int[] iterations = GetIterationsForClear<T>();

            for (int i = 0; i < arrays.Length; i++)
            {
                var array = arrays[i];
                int innerIter = iterations[i];
                foreach (var iteration in Benchmark.Iterations)
                {
                    using (iteration.StartMeasurement())
                    {
                        PerfTestClear<T>(array, innerIter);
                    }
                }
            }
        }

        [Benchmark]
        public static void TestSpanAsBytes<T>()
            where T : struct
        {
            T[][] arrays = GetArrays<T>();
            int[] iterations = GetIterations();

            for (int i = 0; i < arrays.Length; i++)
            {
                Span<T> span = new Span<T>(arrays[i]);
                int innerIter = iterations[i];
                foreach (var iteration in Benchmark.Iterations)
                {
                    using (iteration.StartMeasurement())
                    {
                        PerfTestAsBytes<T>(span, innerIter);
                    }
                }
            }
        }

        [Benchmark]
        public static void TestSpanNonPortableCast<TFrom, TTo>()
            where TFrom : struct
            where TTo : struct
        {
            TFrom[][] arrays = GetArrays<TFrom>();
            int[] iterations = GetIterations();

            for (int i = 0; i < arrays.Length; i++)
            {
                Span<TFrom> span = new Span<TFrom>(arrays[i]);
                int innerIter = iterations[i];
                foreach (var iteration in Benchmark.Iterations)
                {
                    using (iteration.StartMeasurement())
                    {
                        PerfTestNonPortableCast<TFrom, TTo>(span, innerIter);
                    }
                }
            }
        }

        [Benchmark]
        public static void TestSpanSliceStringChar()
        {
            string[] strings = GetStrings();
            int[] iterations = GetIterations();

            for (int i = 0; i < strings.Length; i++)
            {
                string s = strings[i];
                int innerIter = iterations[i];
                foreach (var iteration in Benchmark.Iterations)
                {
                    using (iteration.StartMeasurement())
                    {
                        PerfTestSliceString(s, innerIter);
                    }
                }
            }
        }*/
        #endregion

        #region PerfTest<X>
        private static void PerfTestConstructor<T>(T[] array, int iterations)
        {
            for (int j = 0; j < iterations; j++)
            {
                Span<T> span = new Span<T>(array);
            }
        }

        private static void PerfTestDangerousCreate<T>(TestClass<T> testClass, int iterations)
        {
            for (int j = 0; j < iterations; j++)
            {
                Span<T> span = Span<T>.DangerousCreate(testClass, ref testClass.C0[0], testClass.C0.Length);
            }
        }

        /*
        private static void PerfTestDangerousGetPinnableReference<T>(Span<T> span, int iterations)
        {
            for (int j = 0; j < iterations; j++)
            {
                ref T temp = ref span.DangerousGetPinnableReference();
            }
        }*/

        private static void PerfTestIndex<T>(Span<T> span, int iterations)
        {
            for (int j = 0; j < iterations; j++)
            {
                for (int i = 0; i < span.Length; i++)
                {
                    var temp = span[i];
                    //var temp = span.GetItem(i);
                }
            }
        }

        private static void PerfTestSlice<T>(Span<T> span, int iterations)
        {
            for (int j = 0; j < iterations; j++)
            {
                for (int i = 0; i < span.Length; i++)
                {
                    var temp = span.Slice(i);
                }
            }
        }

        private static void PerfTestToArray<T>(Span<T> span, int iterations)
        {
            for (int j = 0; j < iterations; j++)
            {
                var temp = span.ToArray();
            }
        }

        private static void PerfTestCopyTo<T>(Span<T> span, Span<T> destination, int iterations)
        {
            for (int j = 0; j < iterations; j++)
            {
                span.CopyTo(destination);
            }
        }

        private static void PerfTestFill<T>(Span<T> span, int iterations)
        {
            for (int j = 0; j < iterations; j++)
            {
                span.Fill(default(T));
            }
        }

        private static void PerfTestClear<T>(Span<T> span, int iterations)
        {
            for (int j = 0; j < iterations; j++)
            {
                span.Clear();
            }
        }

        private static void PerfTestAsBytes<T>(Span<T> span, int iterations) 
            where T : struct
        {
            for (int j = 0; j < iterations; j++)
            {
                Span<byte> temp = span.AsBytes<T>();
            }
        }

        private static void PerfTestNonPortableCast<TFrom, TTo>(Span<TFrom> span, int iterations) 
            where TFrom : struct 
            where TTo : struct
        {
            for (int j = 0; j < iterations; j++)
            {
                Span<TTo> temp = span.NonPortableCast<TFrom, TTo>();
            }
        }

        private static void PerfTestSliceString(string s, int iterations)
        {
            for (int j = 0; j < iterations; j++)
            {
                ReadOnlySpan<char> temp = s.Slice();
            }
        }
        #endregion

        #region PerfTest<X>(Array)
        private static void PerfTestIndex<T>(T[] array, int iterations)
        {
            for (int j = 0; j < iterations; j++)
            {
                for (int i = 0; i < array.Length; i++)
                {
                    var temp = array[i];
                    //var temp = span.GetItem(i);
                }
            }
        }

        private static void PerfTestCopyTo<T>(T[] array, T[] destination, int iterations)
        {
            for (int j = 0; j < iterations; j++)
            {
                array.CopyTo(destination, 0);
            }
        }

        private static void PerfTestClear<T>(T[] array, int iterations)
        {
            int length = array.Length;
            for (int j = 0; j < iterations; j++)
            {
                Array.Clear(array, 0, length);
            }
        }
        #endregion

        #region GetIterationsFor<X><T>
        private static int[] GetIterationsForConstructor<T>()
        {
            int[] iterations = new int[8];

            if (typeof(T) == typeof(string))
            {
                iterations[0] = BaseIterations / 10;
                iterations[1] = BaseIterations / 10;
                iterations[2] = BaseIterations / 10;
                iterations[3] = BaseIterations / 10;
                iterations[4] = BaseIterations / 10;
                iterations[5] = BaseIterations / 10;
                iterations[6] = BaseIterations / 10;
                iterations[7] = BaseIterations / 10;
            }
            else
            {
                iterations[0] = BaseIterations * 10;
                iterations[1] = BaseIterations * 10;
                iterations[2] = BaseIterations * 10;
                iterations[3] = BaseIterations * 10;
                iterations[4] = BaseIterations * 10;
                iterations[5] = BaseIterations * 10;
                iterations[6] = BaseIterations * 10;
                iterations[7] = BaseIterations * 10;
            }

            return iterations;
        }

        private static int[] GetIterationsForDangerousGetPinnableReference<T>()
        {
            int[] iterations = new int[8];
            
            iterations[0] = BaseIterations * 10;
            iterations[1] = BaseIterations * 10;
            iterations[2] = BaseIterations * 10;
            iterations[3] = BaseIterations * 10;
            iterations[4] = BaseIterations * 10;
            iterations[5] = BaseIterations * 10;
            iterations[6] = BaseIterations * 10;
            iterations[7] = BaseIterations * 10;

            return iterations;
        }

        private static int[] GetIterationsForIndex<T>()
        {
            int[] iterations = new int[8];

            iterations[0] = BaseIterations;
            if (typeof(T) == typeof(string))
            {
                iterations[1] = BaseIterations / 10;
                iterations[2] = BaseIterations / 10;
                iterations[3] = BaseIterations / 100;
                iterations[4] = BaseIterations / 1000;
                iterations[5] = BaseIterations / 1000;
                iterations[6] = BaseIterations / 10000;
                iterations[7] = BaseIterations / 10000000;
            }
            else
            {
                iterations[1] = BaseIterations;
                iterations[2] = BaseIterations;
                iterations[3] = BaseIterations / 10;
                iterations[4] = BaseIterations / 100;
                iterations[5] = BaseIterations / 100;
                iterations[6] = BaseIterations / 1000;
                iterations[7] = BaseIterations / 1000000;
            }

            return iterations;
        }

        private static int[] GetIterationsForSlice<T>()
        {
            int[] iterations = new int[8];

            iterations[0] = BaseIterations;
            iterations[1] = BaseIterations / 10;
            iterations[2] = BaseIterations / 10;
            iterations[3] = BaseIterations / 100;
            iterations[4] = BaseIterations / 1000;
            iterations[5] = BaseIterations / 1000;
            iterations[6] = BaseIterations / 10000;
            iterations[7] = BaseIterations / 10000000;

            return iterations;
        }

        private static int[] GetIterationsForToArray<T>()
        {
            int[] iterations = new int[8];

            iterations[0] = BaseIterations;
            if (typeof(T) == typeof(byte))
            {
                iterations[1] = BaseIterations;
                iterations[2] = BaseIterations;
                iterations[3] = BaseIterations / 10;
                iterations[4] = BaseIterations / 10;
                iterations[5] = BaseIterations / 10;
                iterations[6] = BaseIterations / 100;
                iterations[7] = BaseIterations / 1000000;
            }
            else if(typeof(T) == typeof(int))
            {
                iterations[1] = BaseIterations;
                iterations[2] = BaseIterations;
                iterations[3] = BaseIterations / 10;
                iterations[4] = BaseIterations / 10;
                iterations[5] = BaseIterations / 100;
                iterations[6] = BaseIterations / 1000;
                iterations[7] = BaseIterations / 1000000;
            }
            else
            {
                iterations[1] = BaseIterations / 10;
                iterations[2] = BaseIterations / 10;
                iterations[3] = BaseIterations / 100;
                iterations[4] = BaseIterations / 1000;
                iterations[5] = BaseIterations / 1000;
                iterations[6] = BaseIterations / 10000;
                iterations[7] = BaseIterations / 10000000;
            }

            return iterations;
        }

        private static int[] GetIterationsForCopyTo<T>()
        {
            int[] iterations = new int[8];

            iterations[0] = BaseIterations;
            if (typeof(T) == typeof(byte) || (typeof(T) == typeof(int)))
            {
                iterations[1] = BaseIterations;
                iterations[2] = BaseIterations;
                iterations[3] = BaseIterations / 10;
                iterations[4] = BaseIterations / 10;
                iterations[5] = BaseIterations / 10;
                iterations[6] = BaseIterations / 100;
                iterations[7] = BaseIterations / 1000000;
            }
            else
            {
                iterations[1] = BaseIterations / 10;
                iterations[2] = BaseIterations / 10;
                iterations[3] = BaseIterations / 100;
                iterations[4] = BaseIterations / 100;
                iterations[5] = BaseIterations / 1000;
                iterations[6] = BaseIterations / 10000;
                iterations[7] = BaseIterations / 10000000;
            }

            return iterations;
        }

        private static int[] GetIterationsForFill<T>()
        {
            int[] iterations = new int[8];

            iterations[0] = BaseIterations;
            if (typeof(T) == typeof(byte))
            {
                iterations[1] = BaseIterations;
                iterations[2] = BaseIterations;
                iterations[3] = BaseIterations;
                iterations[4] = BaseIterations / 10;
                iterations[5] = BaseIterations / 10;
                iterations[6] = BaseIterations / 100;
                iterations[7] = BaseIterations / 100000;
            }
            else if (typeof(T) == typeof(string))
            {
                iterations[1] = BaseIterations / 10;
                iterations[2] = BaseIterations / 10;
                iterations[3] = BaseIterations / 100;
                iterations[4] = BaseIterations / 1000;
                iterations[5] = BaseIterations / 1000;
                iterations[6] = BaseIterations / 10000;
                iterations[7] = BaseIterations / 10000000;
            }
            else if (typeof(T) == typeof(TestStruct))
            {
                iterations[1] = BaseIterations / 10;
                iterations[2] = BaseIterations / 10;
                iterations[3] = BaseIterations / 100;
                iterations[4] = BaseIterations / 100;
                iterations[5] = BaseIterations / 1000;
                iterations[6] = BaseIterations / 10000;
                iterations[7] = BaseIterations / 10000000;
            }
            else
            {
                iterations[1] = BaseIterations;
                iterations[2] = BaseIterations;
                iterations[3] = BaseIterations / 10;
                iterations[4] = BaseIterations / 100;
                iterations[5] = BaseIterations / 100;
                iterations[6] = BaseIterations / 1000;
                iterations[7] = BaseIterations / 1000000;
            }

            return iterations;
        }

        private static int[] GetIterationsForClear<T>()
        {
            int[] iterations = new int[8];

            iterations[0] = BaseIterations;
            if (typeof(T) == typeof(byte))
            {
                iterations[1] = BaseIterations;
                iterations[2] = BaseIterations;
                iterations[3] = BaseIterations;
                iterations[4] = BaseIterations / 10;
                iterations[5] = BaseIterations / 10;
                iterations[6] = BaseIterations / 100;
                iterations[7] = BaseIterations / 100000;
            }
            else if (typeof(T) == typeof(string))
            {
                iterations[1] = BaseIterations;
                iterations[2] = BaseIterations;
                iterations[3] = BaseIterations / 10;
                iterations[4] = BaseIterations / 100;
                iterations[5] = BaseIterations / 100;
                iterations[6] = BaseIterations / 1000;
                iterations[7] = BaseIterations / 1000000;
            }
            else if (typeof(T) == typeof(TestStruct))
            {
                iterations[1] = BaseIterations;
                iterations[2] = BaseIterations;
                iterations[3] = BaseIterations / 10;
                iterations[4] = BaseIterations / 100;
                iterations[5] = BaseIterations / 100;
                iterations[6] = BaseIterations / 1000;
                iterations[7] = BaseIterations / 1000000;
            }
            else
            {
                iterations[1] = BaseIterations;
                iterations[2] = BaseIterations;
                iterations[3] = BaseIterations / 10;
                iterations[4] = BaseIterations / 10;
                iterations[5] = BaseIterations / 10;
                iterations[6] = BaseIterations / 1000;
                iterations[7] = BaseIterations / 1000000;
            }

            return iterations;
        }

        private static int[] GetIterations()
        {
            int[] iterations = new int[8];

            iterations[0] = BaseIterations;
            iterations[1] = BaseIterations;
            iterations[2] = BaseIterations;
            iterations[3] = BaseIterations;
            iterations[4] = BaseIterations;
            iterations[5] = BaseIterations;
            iterations[6] = BaseIterations;
            iterations[7] = BaseIterations;

            return iterations;
        }
        #endregion

        #region Helpers
        private static T[][] GetArrays<T>()
        {
            T[][] arrays = new T[8][];

            arrays[0] = new T[1];
            arrays[1] = new T[5];
            arrays[2] = new T[10];
            arrays[3] = new T[100];
            arrays[4] = new T[500];
            arrays[5] = new T[1000];
            arrays[6] = new T[10000];
            arrays[7] = new T[10000000];

            return arrays;
        }

        private static string[] GetStrings()
        {
            string[] strings = new string[8];
            Random rand = new Random(42);

            strings[0] = "";
            strings[1] = "";
            strings[2] = "";
            strings[3] = "";
            strings[4] = "";
            strings[5] = "";
            strings[6] = "";
            strings[7] = "";

            StringBuilder sb = new StringBuilder();
            char[] c = new char[1];
            for (int i = 0; i < 10000000; i++)
            {
                c[0] = (char)rand.Next(32, 126);
                sb.Append(new string(c));
                if (i == 1-1)
                {
                    strings[0] = sb.ToString();
                }
                if (i == 5-1)
                {
                    strings[1] = sb.ToString();
                }
                if (i == 10-1)
                {
                    strings[2] = sb.ToString();
                }
                if (i == 100-1)
                {
                    strings[3] = sb.ToString();
                }
                if (i == 500-1)
                {
                    strings[4] = sb.ToString();
                }
                if (i == 1000-1)
                {
                    strings[5] = sb.ToString();
                }
                if (i == 10000-1)
                {
                    strings[6] = sb.ToString();
                }
            }
            strings[7] = sb.ToString();

            return strings;
        }

        private static TestClass<T>[] GetClasses<T>()
        {
            TestClass<T>[] classes = new TestClass<T>[8];

            classes[0] = new TestClass<T>();
            classes[1] = new TestClass<T>();
            classes[2] = new TestClass<T>();
            classes[3] = new TestClass<T>();
            classes[4] = new TestClass<T>();
            classes[5] = new TestClass<T>();
            classes[6] = new TestClass<T>();
            classes[7] = new TestClass<T>();

            classes[0].C0 = new T[1];
            classes[1].C0 = new T[5];
            classes[2].C0 = new T[10];
            classes[3].C0 = new T[100];
            classes[4].C0 = new T[500];
            classes[5].C0 = new T[1000];
            classes[6].C0 = new T[10000];
            classes[7].C0 = new T[10000000];

            return classes;
        }

        [StructLayout(LayoutKind.Sequential)]
        private sealed class TestClass<T>
        {
            private double _d;
            public T[] C0;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct TestStruct
        {
            public int I;
            public string S;
        }
        #endregion

        public static int Main(string[] args)
        {
            bool result = true;

            /*foreach(object[] o in TestFuncs)
            {
                string funcName = (string) o[0];
                Action func = LookupFunc(funcName);
                double timeInMs = Bench(func);
                Console.WriteLine("{0}: {1}ms", funcName, timeInMs);
            }*/

/*#if !DEBUG
            TestSpanConstructor();
            TestSpanDangerousCreate();
            TestSpanDangerousGetPinnableReference();
            TestSpanIndex();
            TestSpanSlice();
            TestSpanToArray();
            TestSpanCopyTo();
            TestSpanFill();
            TestSpanClear();
            TestSpanAsBytes();
            TestSpanNonPortableCast();
            TestSpanSliceString();
#endif*/

            return (result ? 100 : -1);
        }
    }
}