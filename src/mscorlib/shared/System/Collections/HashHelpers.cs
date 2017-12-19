// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Threading;
using Internal.Runtime.CompilerServices;

namespace System.Collections
{
    internal static class HashHelpers
    {
        public const int HashCollisionThreshold = 100;

        public const int HashPrime = 101;

        // Table of prime numbers to use as hash table sizes. 
        // A typical resize algorithm would pick the smallest prime number in this array
        // that is larger than twice the previous capacity. 
        // Suppose our Hashtable currently has capacity x and enough elements are added 
        // such that a resize needs to occur. Resizing first computes 2x then finds the 
        // first prime in the table greater than 2x, i.e. if primes are ordered 
        // p_1, p_2, ..., p_i, ..., it finds p_n such that p_n-1 < 2x < p_n. 
        // Doubling is important for preserving the asymptotic complexity of the 
        // hashtable operations such as add.  Having a prime guarantees that double 
        // hashing does not lead to infinite loops.  IE, your hash function will be 
        // h1(key) + i*h2(key), 0 <= i < size.  h2 and the size must be relatively prime.
        public static readonly int[] primes = {
            3, 7, 11, 17, 23, 29, 37, 47, 59, 71, 89, 107, 131, 163, 197, 239, 293, 353, 431, 521, 631, 761, 919,
            1103, 1327, 1597, 1931, 2333, 2801, 3371, 4049, 4861, 5839, 7013, 8419, 10103, 12143, 14591,
            17519, 21023, 25229, 30293, 36353, 43627, 52361, 62851, 75431, 90523, 108631, 130363, 156437,
            187751, 225307, 270371, 324449, 389357, 467237, 560689, 672827, 807403, 968897, 1162687, 1395263,
            1674319, 2009191, 2411033, 2893249, 3471899, 4166287, 4999559, 5999471, 7199369};

        public static readonly uint[] PrimeMagicShift =
        {
            // prime, magic multiplier, magic shift
            3, 0x55555556, 32,
            7, 0x92492493, 34,
            11, 0x2e8ba2e9, 33,
            17, 0x78787879, 35,
            23, 0xb21642c9, 36,
            29, 0x8d3dcb09, 36,
            37, 0xdd67c8a7, 37,
            47, 0xae4c415d, 37,
            59, 0x22b63cbf, 35,
            71, 0xe6c2b449, 38,
            89, 0xb81702e1, 38,
            107, 0x4c8f8d29, 37,
            131, 0x3e88cb3d, 37,
            163, 0x0c907da5, 35,
            197, 0x532ae21d, 38,
            239, 0x891ac73b, 39,
            293, 0xdfac1f75, 40,
            353, 0xb9a7862b, 40,
            431, 0x980e4157, 40,
            521, 0x3ee4f99d, 39,
            631, 0x33ee2623, 39,
            761, 0x561e46a5, 40,
            919, 0x8e9fe543, 41,
            1103, 0x1db54401, 39,
            1327, 0xc58bdd47, 42,
            1597, 0xa425d4b9, 42,
            1931, 0x10f82d9b, 39,
            2333, 0x705d0d0f, 42,
            2801, 0x2ecb7285, 41,
            3371, 0x9b876783, 43,
            4049, 0x817c5d53, 43,
            4861, 0x35ed914d, 42,
            5839, 0x0b394d8f, 40,
            7013, 0x9584d635, 44,
            8419, 0x7c8c7b75, 44,
            10103, 0x33e4f01d, 43,
            12143, 0x565a3073, 44,
            14591, 0x23eeaa5d, 43,
            17519, 0x77b510e9, 45,
            21023, 0x63c14fe5, 45,
            25229, 0x531fe999, 45,
            30293, 0x8a75366b, 46,
            36353, 0xe6c11447, 47,
            43627, 0xc047bac3, 47,
            52361, 0x0a035099, 43,
            62851, 0x42bbed05, 46,
            75431, 0x379ac159, 46,
            90523, 0x05cab127, 43,
            108631, 0x9a713743, 48,
            130363, 0x80b236c9, 48,
            156437, 0x6b3eeec1, 48,
            187751, 0xb2b7bcf9, 49,
            225307, 0x4a76bbc7, 48,
            270371, 0x7c1aeabf, 49,
            324449, 0x676b743d, 49,
            389357, 0x2b16ec6d, 48,
            467237, 0x8fa1117f, 50,
            560689, 0x77b0a38f, 50,
            672827, 0x63bddbb1, 50,
            807403, 0x0531def9, 46,
            968897, 0x8a86bc61, 51,
            1162687, 0x737002ad, 51,
            1395263, 0x180c7f9f, 49,
            1674319, 0x140a67af, 49,
            2009191, 0x42cd47bf, 51,
            2411033, 0x37ab0b5f, 51,
            2893249, 0x5cc7a9dd, 52,
            3471899, 0x4d510d43, 52,
            4166287, 0x080dc5ad, 49,
            4999559, 0x035b11b9, 48,
            5999471, 0xb2f90627, 54,
            7199369, 0x9524d54d, 54,
            14398753, 0x4a92658f, 54,
            28797523, 0x4a9262ad, 55,
            57595063, 0x9524c277, 57,
            115190149, 0x9524c083, 58,
            230380307, 0x4a926011, 58,
            460760623, 0x9524bff1, 60,
            921521257, 0x9524bfd3, 61,
            1843042529, 0x4a925fdf, 61,
            2146435069, 0x20040081, 60
        };

        public static bool IsPrime(int candidate)
        {
            if ((candidate & 1) != 0)
            {
                int limit = (int)Math.Sqrt(candidate);
                for (int divisor = 3; divisor <= limit; divisor += 2)
                {
                    if ((candidate % divisor) == 0)
                        return false;
                }
                return true;
            }
            return (candidate == 2);
        }

        public static int GetPrime(int min)
        {
            if (min < 0)
                throw new ArgumentException(SR.Arg_HTCapacityOverflow);

            for (int i = 0; i < primes.Length; i++)
            {
                int prime = primes[i];
                if (prime >= min) return prime;
            }

            //outside of our predefined table. 
            //compute the hard way. 
            for (int i = (min | 1); i < Int32.MaxValue; i += 2)
            {
                if (IsPrime(i) && ((i - 1) % HashPrime != 0))
                    return i;
            }
            return min;
        }

        // Returns size of hashtable to grow to.
        public static int ExpandPrime(int oldSize)
        {
            int newSize = 2 * oldSize;

            // Allow the hashtables to grow to maximum possible size (~2G elements) before encoutering capacity overflow.
            // Note that this check works even when _items.Length overflowed thanks to the (uint) cast
            if ((uint)newSize > MaxPrimeArrayLength && MaxPrimeArrayLength > oldSize)
            {
                Debug.Assert(MaxPrimeArrayLength == GetPrime(MaxPrimeArrayLength), "Invalid MaxPrimeArrayLength");
                return MaxPrimeArrayLength;
            }

            return GetPrime(newSize);
        }


        // This is the maximum prime smaller than Array.MaxArrayLength
        public const int MaxPrimeArrayLength = 0x7FEFFFFD;


        // Used by Hashtable and Dictionary's SeralizationInfo .ctor's to store the SeralizationInfo
        // object until OnDeserialization is called.
        private static ConditionalWeakTable<object, SerializationInfo> s_serializationInfoTable;

        internal static ConditionalWeakTable<object, SerializationInfo> SerializationInfoTable
        {
            get
            {
                if (s_serializationInfoTable == null)
                    Interlocked.CompareExchange(ref s_serializationInfoTable, new ConditionalWeakTable<object, SerializationInfo>(), null);

                return s_serializationInfoTable;
            }
        }

        // To implement magic-number divide with a 32-bit magic number,
        // multiply by the magic number, take the top 64 bits, and shift that
        // by the amount given in the table.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint MagicNumberDivide(uint numerator, uint magic, int shift)
        {
            return (uint)((numerator * (ulong)magic) >> shift);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int MagicNumberRemainder(int numerator, int divisor, uint magic, int shift)
        {
            Debug.Assert(numerator >= 0);
            uint product = MagicNumberDivide((uint)numerator, magic, shift);
            Debug.Assert(product == numerator / divisor);
            int result = (int)(numerator - (product * divisor));
            Debug.Assert(result == numerator % divisor);
            return result;
        }

        [StructLayout(LayoutKind.Sequential)]
        public readonly struct PrimeInfo
        {
            public readonly int Prime;
            public readonly uint Magic;
            public readonly int Shift;
        }

        public static void NearestPrimeInfo(int min, out int prime, out uint magic, out int shift)
        {
            if (min < 0)
            {
                throw new ArgumentException(SR.Arg_HTCapacityOverflow);
            }

            uint[] primeMagicShift = PrimeMagicShift;
            for (int i = 0; i < primeMagicShift.Length; i += 3)
            {
                ref uint primeRef = ref primeMagicShift[i];
                if (primeRef >= min)
                {
                    PrimeInfo primeInfo = Unsafe.As<uint, PrimeInfo>(ref primeRef);
                    prime = primeInfo.Prime;
                    magic = primeInfo.Magic;
                    shift = primeInfo.Shift;
                    return;
                }
            }

            throw new ArgumentException(SR.Arg_HTCapacityOverflow);
        }

        public static void ExpandPrimeInfo(int oldSize, out int prime, out uint magic, out int shift)
        {
            int newSize = 2 * oldSize;
            // Allow the hashtables to grow to maximum possible size (~2G elements) before encoutering capacity overflow.
            // Note that this check works even when _items.Length overflowed thanks to the (uint) cast
            if ((uint)newSize > MaxPrimeArrayLength && MaxPrimeArrayLength > oldSize)
            {
                Debug.Assert(MaxPrimeArrayLength == GetPrime(MaxPrimeArrayLength), "Invalid MaxPrimeArrayLength");

                PrimeInfo primeInfo = Unsafe.As<uint, PrimeInfo>(ref PrimeMagicShift[PrimeMagicShift.Length - 3]);
                prime = primeInfo.Prime;
                magic = primeInfo.Magic;
                shift = primeInfo.Shift;
                return;
            }

            NearestPrimeInfo(newSize, out prime, out magic, out shift);
        }
    }
}
