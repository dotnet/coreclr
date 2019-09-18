using System;
using System.Reflection;
using System.Linq;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.Arm.Arm64;
using System.Collections.Generic;

namespace Arm64intrisicsTest
{
    class Program
    {
        static void testUnaryOp<T, V>(Func<T, V> func, V expected, T value)
        {
            testUnaryOp<T, V>(func.Method, func, expected, value);
        }
        static void testUnaryOp<T, V>(Func<T, V> func, IEnumerable<V> expected, IEnumerable<T> value)
        {
            Func<IEnumerable<T>, IEnumerable<V>> operation = x => x.Select (func);
            testUnaryOp<IEnumerable<T>, IEnumerable<V>>(func.Method, operation, expected, value);
        }
        static void testUnaryOp<T>(Func<T, T> func, T expected, T value)
        {
            testUnaryOp<T, T>(func.Method, func, expected, value);
        }
        static void testUnaryOp<T>(Func<T, T> func, IEnumerable<T> expected, IEnumerable<T> value)
        {
            testUnaryOp<T, T>(func, expected, value);
        }
        static void testUnaryOp<T, V>(MethodInfo callInfo, Func<T, V> func, V expected, T value)
        {
            testOp (callInfo, func, expected, value);
        }

        static void testBinOp<T, V>(Func<T, T, V> func, V expected, T a, T b)
        {
            testOp (func.Method, func, expected, a, b);
        }
        static void testBinOp<T, V>(Func<T, T, V> func, V expected, IEnumerable<T> a, IEnumerable<T> b)
        {
            testOp (func.Method, (Func<IEnumerable<T>, IEnumerable<T>, IEnumerable<V>>)((x, y) => x.Zip (y, func)), expected, a, b);
        }
        static void testBinOp<T>(Func<T, T, T> func, T expected, T a, T b)
        {
            testBinOp<T, T> (func, expected, a, b);
        }
        static void testBinOp<T>(Func<T, T, T> func, IEnumerable<T> expected, IEnumerable<T> a, IEnumerable<T> b)
        {
            testBinOp<IEnumerable<T>, IEnumerable<T>> ((x, y) => x.Zip (y, func), expected, a, b);
        }

        static void testTernOp<T, V>(Func<T, T, T, V> func, V expected, T a, T b, T c)
        {
            testOp (func.Method, func, expected, a, b, c);
        }
        static void testTernOp<T, V>(Func<T, T, V> func, IEnumerable<V> expected, IEnumerable<T> a, IEnumerable<T> b, IEnumerable<T> c)
        {
	    Func<T, T, Func<T, V>> fn = (a, b) => (s => func (s, a, b));
            Func<IEnumerable<T>, IEnumerable<T>, IEnumerable<T>, IEnumerable<V>> op
              = (x, y, z) => Enumerable.Zip (x, Enumerable.Zip<T, T, Func<T, V>> (y, z, fn), (e, f) => f (e));
            testOp (func.Method, op, expected, a, b, c);
        }
        static void testTernOp<T>(Func<T, T, T, T> func, T expected, T a, T b, T c)
        {
            testOp (func.Method, func, expected, a, b, c);
        }
        static void testTernOp<T>(Func<T, T, T> func, IEnumerable<T> expected, IEnumerable<T> a, IEnumerable<T> b, IEnumerable<T> c)
        {
            testTernOp<T, T> (func, expected, a, b, c);
        }

        static void testOp<T>(MethodInfo callInfo, Delegate fn, T expected, params object[] args)
        {
            bool failed = false;
            string testCaseDescription = fn.Method.Name;
            bool isSupported
              = Convert.ToBoolean(callInfo.DeclaringType
                                          .GetMethod("get_IsSupported")
                                          .Invoke(null, null));
            string types
             = args.Aggregate (expected.GetType().Name,
                               (s, ty) => String.Format ("{0}, {1}", s,
                                                         ty.GetType ().Name));
            try
            {
                if (!isSupported)
                {
                    testThrows<PlatformNotSupportedException, T> (() => fn.DynamicInvoke (args));
                    return;
                }

                object result = fn.DynamicInvoke (args);
                bool equal;
                if (typeof(System.Collections.IEnumerable).IsAssignableFrom(expected.GetType ()))
                {
                    equal = Enumerable.SequenceEqual ((IEnumerable)result, (IEnumerable)expected);
                }
                else
                {
                    equal = result != (object)expected;
                }

                if (!equal)
                {
                    Console.WriteLine($"testOp<{types}>{testCaseDescription}: Check Failed");
                    Console.WriteLine($"    result = {types}, expected = {expected}");
                    throw new Exception($"testOp<{types}>{testCaseDescription}: Failed");
                }
            }
            catch
            {
                Console.WriteLine($"testOp<{types}>{testCaseDescription}: Unexpected exception");
                throw;
            }
        }

        static void testThrows<E, T>(Action func) where E : Exception
        {
            testThrows<E> (typeof(T), func);
        }

        static void testThrows<E>(Type T, Action func) where E : Exception
        {
            try
            {
                func();
            }
            catch (PlatformNotSupportedException)
            {
                return;
            }
            catch
            {
                Console.WriteLine($"testThrows<{typeof(E).Name}, {T.Name}>: Unexpected exception");
                throw;
            }

            throw new Exception($"testThrows<{typeof(E).Name}, {T.Name}>{func.Method.Name}: Failed");
        }

        static int s_SignBit32 = 1 << 31;
        static long s_SignBit64 = 1L << 63;

        static int GenLeadingSignBitsI32(int num)
        {
            Debug.Assert(0 <= num && num < 32);
            return s_SignBit32 >> num;
        }

        static long GenLeadingSignBitsI64(long num)
        {
            Debug.Assert(0 <= num && num < 64);
            return s_SignBit64 >> (int)num;
        }

        static uint GenLeadingZeroBitsU32(int num)
        {
            Debug.Assert(0 <= num && num <= 32);
            return (num < 32) ? (~0U >> num) : 0;
        }

        static int GenLeadingZeroBitsI32(int num)
        {
            return (int)GenLeadingZeroBitsU32(num);
        }

        static ulong GenLeadingZeroBitsU64(int num)
        {
            Debug.Assert(0 <= num && num <= 64);
            return (num < 64) ? (~0UL >> num) : 0;
        }

        static long GenLeadingZeroBitsI64(int num)
        {
            return (long)GenLeadingZeroBitsU64(num);
        }

        static void TestLeadingSignCount()
        {
            String name = "LeadingSignCount";

            var intRange = Enumerable.Range (0, 32);
            testUnaryOp<int>(Base.LeadingSignCount, intRange.Select (GenLeadingSignBitsI32), intRange);

            var longRange = Enumerable.Range (0, 64).Cast<long>();
            testUnaryOp<long>(Base.LeadingSignCount, x => (long)Base.LeadingSignCount (x), longRange.Select (GenLeadingSignBitsI64), longRange);

            Console.WriteLine($"Test{name} passed");
        }

        static void TestLeadingZeroCount()
        {
            String name = "LeadingZeroCount";

            var intRange = Enumerable.Range (0, 32);
            testUnaryOp<int >(Base.LeadingZeroCount, intRange.Select (GenLeadingZeroBitsI32), intRange.Cast< int>());
            testUnaryOp<uint>(Base.LeadingZeroCount, intRange.Select (GenLeadingZeroBitsU32), intRange.Cast<uint>());

            var longRange = Enumerable.Range (0, 64);
            testUnaryOp<long >(Base.LeadingZeroCount, longRange.Select (GenLeadingZeroBitsI64), longRange.Cast< long>());
            testUnaryOp<ulong>(Base.LeadingZeroCount, longRange.Select (GenLeadingZeroBitsU64), longRange.Cast<ulong>());

            Console.WriteLine($"Test{name} passed");
        }


        static void TestReverseBitOrder()
        {
            String name = "ReverseBitOrder";

            var control = (v, shift) => {
              ulong r = v;
              int s = (sizeof (v) * 8) - 1;
              for (v >>= 1; v; v >>= 1)
              {
                r <<= 1;
                r |= v & 1;
                s--;
              }
              return (r << s) >> shift;
            };

            var intRange = Enumerable.Range (0, 32);
            testUnaryOp<int >(Base.ReverseBitOrder, intRange.Select (n => control(n, 32)), intRange.Cast());
            testUnaryOp<uint>(Base.ReverseBitOrder, intRange.Select (n => control(n, 32)), intRange.Cast());

            var longRange = Enumerable.Range (0, 64);
            testUnaryOp<long >(Base.ReverseBitOrder, longRange.Select (n => control (n, 0)), longRange.Cast());
            testUnaryOp<ulong>(Base.ReverseBitOrder, longRange.Select (n => control (n, 0)), longRange.Cast());

            Console.WriteLine($"Test{name} passed");
        }

        static void TestAbsoluteCompareGreatherThanOrEqual()
        {
            String name = "AbsoluteCompareGreatherThanOrEqual";

            var controlF = (a, b) => {
              return Math.Abs (a) >= Math.Abs (b) ? float.MaxValue : 0;
            };

            var controlD = (a, b) => {
              return Math.Abs (a) >= Math.Abs (b) ? double.MaxValue : 0;
            };

            var data = { (1.0 , 3.0 )
                       , (3.0 , 1.0 )
                       , (2.0 , 2.0 )
                       , (1.5 , 2.7 )
                       , (-2.4, 4.5 )
                       , (-5.2, 1.2 )
                       , (-2.4, -4.5)
                       , (-5.2, -1.2)
                       , (2.4 , -4.5)
                       , (5.2 , -1.2)
                       };

            data.Select ((v) => {
              testBinOp<float >(name, (x) => Base.AbsoluteCompareGreatherThanOrEqual(x), controlF(v.Item1, v.Item2), v.Item1, v.Item2);
              testBinOp<double>(name, (x) => Base.AbsoluteCompareGreatherThanOrEqual(x), controlD(v.Item1, v.Item2), v.Item1, v.Item2);
              return true;
            });

            Console.WriteLine($"Test{name} passed");
        }

        static void TestAbsoluteCompareGreatherThan()
        {
            String name = "AbsoluteCompareGreatherThan";

            var controlF = (a, b) => {
                return Math.Abs (a) > Math.Abs (b) ? float.MaxValue : 0;
            };

            var controlD = (a, b) => {
                return Math.Abs (a) > Math.Abs (b) ? double.MaxValue : 0;
            };

            var dA = { 1.0, 3.0, 2.0, 1.5, -2.4, -5.2, -2.4, -5.2, 2.4, 5.2 };
            var dB = { 3.0, 1.0, 2.0, 2.7, 4.5, 1.2, -4.5, -1.2, -4.5, -1.2 };

            testBinOp<float >(Base.AbsoluteCompareGreatherThan, dA.Zip(dB, controlF), dA, dB);
            testBinOp<double>(Base.AbsoluteCompareGreatherThan, dA.Zip(dB, controlD), dA, dB);

            Console.WriteLine($"Test{name} passed");
        }

        static void TestLeftShiftAndInsert()
        {
            String name = "LeftShiftAndInsert";

            var control = (a, b, s) => {
                return ((a << s) & ~b) | (a << s);
            };

            var dA = { 1.0, 3.0, 2.0, 1.5, -2.4, -5.2, -2.4, -5.2, 2.4, 5.2 };
            var dB = { 3.0, 1.0, 2.0, 2.7, 4.5, 1.2, -4.5, -1.2, -4.5, -1.2 };
            var dC = { 5, 23, 8, 8, 1, 0, 15, 29, 29, 31 };
            var mk = (x, y, z) => x.Select (y.Zip (z, s => control (s, y, z)));

            testTernOp<long >(Base.LeftShiftAndInsert, mk (dA, dB, dC), dA, dB, dC);
            testTernOp<ulong>(Base.LeftShiftAndInsert, mk (dA, dB, dC), dA, dB, dC);

            Console.WriteLine($"Test{name} passed");
        }

        static void ExecuteAllTests()
        {
            TestLeadingSignCount();
            TestLeadingZeroCount();
            TestReverseBitOrder();
            TestAbsoluteCompareGreatherThanOrEqual();
            TestAbsoluteCompareGreatherThan();
            TestLeftShiftAndInsert();
        }

        static int Main(string[] args)
        {
            Console.WriteLine($"System.Runtime.Intrinsics.Arm.Arm64.Base.IsSupported = {Base.IsSupported}");

            // Reflection call
            var issupported = "get_IsSupported";
            bool reflectedIsSupported = Convert.ToBoolean(typeof(Base).GetMethod(issupported).Invoke(null, null));

            Debug.Assert(reflectedIsSupported == Base.IsSupported, "Reflection result does not match");

            ExecuteAllTests();

            return 100;
        }
    }
}
