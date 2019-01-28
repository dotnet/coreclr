// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Numerics;
using System.Runtime.Intrinsics.X86;

using Internal.Runtime.CompilerServices;
using System.Runtime.Intrinsics;

namespace System
{
    internal static partial class SpanHelpers // .Char
    {
        public static int IndexOf(ref char searchSpace, int searchSpaceLength, ref char value, int valueLength)
        {
            Debug.Assert(searchSpaceLength >= 0);
            Debug.Assert(valueLength >= 0);

            if (valueLength == 0)
                return 0;  // A zero-length sequence is always treated as "found" at the start of the search space.

            char valueHead = value;
            ref char valueTail = ref Unsafe.Add(ref value, 1);
            int valueTailLength = valueLength - 1;
            int remainingSearchSpaceLength = searchSpaceLength - valueTailLength;

            int offset = 0;
            while (remainingSearchSpaceLength > 0)
            {
                // Do a quick search for the first element of "value".
                int relativeIndex = IndexOf(ref Unsafe.Add(ref searchSpace, offset), valueHead, remainingSearchSpaceLength);
                if (relativeIndex == -1)
                    break;

                remainingSearchSpaceLength -= relativeIndex;
                offset += relativeIndex;

                if (remainingSearchSpaceLength <= 0)
                    break;  // The unsearched portion is now shorter than the sequence we're looking for. So it can't be there.

                // Found the first element of "value". See if the tail matches.
                if (SequenceEqual(
                    ref Unsafe.As<char, byte>(ref Unsafe.Add(ref searchSpace, offset + 1)),
                    ref Unsafe.As<char, byte>(ref valueTail),
                    valueTailLength * 2))
                {
                    return offset;  // The tail matched. Return a successful find.
                }

                remainingSearchSpaceLength--;
                offset++;
            }
            return -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public static unsafe int SequenceCompareTo(ref char first, int firstLength, ref char second, int secondLength)
        {
            Debug.Assert(firstLength >= 0);
            Debug.Assert(secondLength >= 0);

            if (Unsafe.AreSame(ref first, ref second))
                goto Equal;

            int minLength = (firstLength < secondLength) ? firstLength : secondLength;

            int offset = 0;
            int lengthToExamine = minLength;

            if (Avx2.IsSupported)
            {
                if (lengthToExamine >= Vector256<ushort>.Count)
                {
                    lengthToExamine -= Vector256<ushort>.Count;
                    uint matches;
                    while (lengthToExamine > offset)
                    {
                        matches = (uint)Avx2.MoveMask(Avx2.CompareEqual(LoadVector256(ref first, offset), LoadVector256(ref second, offset)).AsByte());
                        // Note that MoveMask has converted the equal vector elements into a set of bit flags,
                        // So the bit position in 'matches' corresponds to the element offset.

                        // 32 elements in Vector256<byte> so we compare to uint.MaxValue to check if everything matched
                        if (matches == uint.MaxValue)
                        {
                            // All matched
                            offset += Vector256<ushort>.Count;
                            continue;
                        }

                        goto Difference;
                    }
                    // Move to Vector length from end for final compare
                    offset = lengthToExamine;
                    // Same as method as above
                    matches = (uint)Avx2.MoveMask(Avx2.CompareEqual(LoadVector256(ref first, offset), LoadVector256(ref second, offset)).AsByte());
                    if (matches == uint.MaxValue)
                    {
                        // All matched
                        goto Equal;
                    }
                Difference:
                    // Invert matches to find differences
                    uint differences = ~matches;
                    // Find bitflag offset of first difference and add to current offset, 
                    // flags are in bytes so divide for chars
                    offset = offset + BitOps.TrailingZeroCount((int)differences) / sizeof(char);

                    int result = Unsafe.Add(ref first, offset).CompareTo(Unsafe.Add(ref second, offset));
                    Debug.Assert(result != 0);

                    return result;
                }

                if (lengthToExamine >= Vector128<ushort>.Count)
                {
                    lengthToExamine -= Vector128<ushort>.Count;
                    uint matches;
                    if (lengthToExamine > offset)
                    {
                        matches = (uint)Sse2.MoveMask(Sse2.CompareEqual(LoadVector128(ref first, offset), LoadVector128(ref second, offset)).AsByte());
                        // Note that MoveMask has converted the equal vector elements into a set of bit flags,
                        // So the bit position in 'matches' corresponds to the element offset.

                        // 16 elements in Vector128<byte> so we compare to ushort.MaxValue to check if everything matched
                        if (matches == ushort.MaxValue)
                        {
                            // All matched
                            offset += Vector128<ushort>.Count;
                        }
                        else
                        {
                            goto Difference;
                        }
                    }
                    // Move to Vector length from end for final compare
                    offset = lengthToExamine;
                    // Same as method as above
                    matches = (uint)Sse2.MoveMask(Sse2.CompareEqual(LoadVector128(ref first, offset), LoadVector128(ref second, offset)).AsByte());
                    if (matches == ushort.MaxValue)
                    {
                        // All matched
                        goto Equal;
                    }
                Difference:
                    // Invert matches to find differences
                    uint differences = ~matches;
                    // Find bitflag offset of first difference and add to current offset, 
                    // flags are in bytes so divide for chars
                    offset = offset + BitOps.TrailingZeroCount((int)differences) / sizeof(char);

                    int result = Unsafe.Add(ref first, offset).CompareTo(Unsafe.Add(ref second, offset));
                    Debug.Assert(result != 0);

                    return result;
                }
            }
            else if (Sse2.IsSupported)
            {
                if (lengthToExamine >= Vector128<ushort>.Count)
                {
                    lengthToExamine -= Vector128<ushort>.Count;
                    uint matches;
                    while (lengthToExamine > offset)
                    {
                        matches = (uint)Sse2.MoveMask(Sse2.CompareEqual(LoadVector128(ref first, offset), LoadVector128(ref second, offset)).AsByte());
                        // Note that MoveMask has converted the equal vector elements into a set of bit flags,
                        // So the bit position in 'matches' corresponds to the element offset.

                        // 16 elements in Vector128<byte> so we compare to ushort.MaxValue to check if everything matched
                        if (matches == ushort.MaxValue)
                        {
                            // All matched
                            offset += Vector128<ushort>.Count;
                            continue;
                        }

                        goto Difference;
                    }
                    // Move to Vector length from end for final compare
                    offset = lengthToExamine;
                    // Same as method as above
                    matches = (uint)Sse2.MoveMask(Sse2.CompareEqual(LoadVector128(ref first, offset), LoadVector128(ref second, offset)).AsByte());
                    if (matches == ushort.MaxValue)
                    {
                        // All matched
                        goto Equal;
                    }
                Difference:
                    // Invert matches to find differences
                    uint differences = ~matches;
                    // Find bitflag offset of first difference and add to current offset, 
                    // flags are in bytes so divide for chars
                    offset = offset + BitOps.TrailingZeroCount((int)differences) / sizeof(char);

                    int result = Unsafe.Add(ref first, offset).CompareTo(Unsafe.Add(ref second, offset));
                    Debug.Assert(result != 0);

                    return result;
                }
            }
            else if (Vector.IsHardwareAccelerated)
            {
                if (lengthToExamine > Vector<ushort>.Count)
                {
                    lengthToExamine -= Vector<ushort>.Count;
                    while (lengthToExamine > offset)
                    {
                        if (LoadVector(ref first, offset) != LoadVector(ref second, offset))
                        {
                            goto CharwiseCheck;
                        }
                        offset += Vector<ushort>.Count;
                    }
                    goto CharwiseCheck;
                }
            }

            if (lengthToExamine > sizeof(UIntPtr) / sizeof(char))
            {
                lengthToExamine -= sizeof(UIntPtr) / sizeof(char);
                while (lengthToExamine > offset)
                {
                    if (LoadUIntPtr(ref first, offset) != LoadUIntPtr(ref second, offset))
                    {
                        goto CharwiseCheck;
                    }
                    offset += sizeof(UIntPtr) / sizeof(char);
                }
            }

        CharwiseCheck:
            while (minLength > offset)
            {
                int result = Unsafe.Add(ref first, offset).CompareTo(Unsafe.Add(ref second, offset));
                if (result != 0)
                    return result;
                offset += 1;
            }

        Equal:
            return firstLength - secondLength;
        }

        // Adapted from IndexOf(...)
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public static bool Contains(ref char searchSpace, char value, int length)
        {
            Debug.Assert(length >= 0);

            int offset = 0;
            int lengthToExamine = length;

            if (Vector.IsHardwareAccelerated)
            {
                if (length >= Vector<ushort>.Count * 2)
                {
                    lengthToExamine = UnalignedCountVector(ref searchSpace);
                }
            }

        SequentialScan:
            while (lengthToExamine >= 4)
            {
                lengthToExamine -= 4;

                if (value == Unsafe.Add(ref searchSpace, offset) ||
                    value == Unsafe.Add(ref searchSpace, offset + 1) || 
                    value == Unsafe.Add(ref searchSpace, offset + 2) || 
                    value == Unsafe.Add(ref searchSpace, offset + 3))
                    goto Found;

                offset += 4;
            }

            while (lengthToExamine > 0)
            {
                lengthToExamine -= 1;

                if (value == Unsafe.Add(ref searchSpace, offset))
                    goto Found;

                offset += 1;
            }

            // We get past SequentialScan only if IsHardwareAccelerated is true. However, we still have the redundant check to allow
            // the JIT to see that the code is unreachable and eliminate it when the platform does not have hardware accelerated.
            if (Vector.IsHardwareAccelerated)
            {
                if (offset < length)
                {
                    lengthToExamine = GetCharVectorSpanLength(offset, length);

                    Vector<ushort> values = new Vector<ushort>(value);

                    while (lengthToExamine > offset)
                    {
                        var matches = Vector.Equals(values, LoadVector(ref searchSpace, offset));
                        if (Vector<ushort>.Zero.Equals(matches))
                        {
                            offset += Vector<ushort>.Count;
                            continue;
                        }

                        goto Found;
                    }

                    if (offset < length)
                    {
                        lengthToExamine = length - offset;
                        goto SequentialScan;
                    }
                }
            }

            return false;
        Found:
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public static int IndexOf(ref char searchSpace, char value, int length)
        {
            Debug.Assert(length >= 0);

            int offset = 0; 
            int lengthToExamine = length;

            if (Avx2.IsSupported || Sse2.IsSupported)
            {
                // Avx2 branch also operates on Sse2 sizes, so check is combined.
                if (length >= Vector128<byte>.Count * 2)
                {
                    lengthToExamine = UnalignedCountVector128(ref searchSpace);
                }
            }
            else if (Vector.IsHardwareAccelerated)
            {
                if (length >= Vector<ushort>.Count * 2)
                {
                    lengthToExamine = UnalignedCountVector(ref searchSpace);
                }
            }

        SequentialScan:
            while (lengthToExamine >= 4)
            {
                lengthToExamine -= 4;

                if (value == Unsafe.Add(ref searchSpace, offset))
                    goto Found;
                if (value == Unsafe.Add(ref searchSpace, offset + 1))
                    goto Found1;
                if (value == Unsafe.Add(ref searchSpace, offset + 2))
                    goto Found2;
                if (value == Unsafe.Add(ref searchSpace, offset + 3))
                    goto Found3;

                offset += 4;
            }

            while (lengthToExamine > 0)
            {
                lengthToExamine -= 1;

                if (value == Unsafe.Add(ref searchSpace, offset))
                    goto Found;

                offset += 1;
            }

            // We get past SequentialScan only if IsHardwareAccelerated or intrinsic .IsSupported is true. However, we still have the redundant check to allow
            // the JIT to see that the code is unreachable and eliminate it when the platform does not have hardware accelerated.
            if (Avx2.IsSupported)
            {
                if (offset < length)
                {
                    lengthToExamine = GetCharVector256SpanLength(offset, length);
                    if (lengthToExamine > offset)
                    {
                        Vector256<ushort> values = Vector256.Create(value);
                        do
                        {
                            Vector256<ushort> search = LoadVector256(ref searchSpace, offset);
                            int matches = Avx2.MoveMask(Avx2.CompareEqual(values, search).AsByte());
                            // Note that MoveMask has converted the equal vector elements into a set of bit flags,
                            // So the bit position in 'matches' corresponds to the element offset.
                            if (matches == 0)
                            {
                                // Zero flags set so no matches
                                offset += Vector256<ushort>.Count;
                                continue;
                            }

                            // Find bitflag offset of first match and add to current offset, 
                            // flags are in bytes so divide for chars
                            return offset + (BitOps.TrailingZeroCount(matches) / sizeof(char));
                        } while (lengthToExamine > offset);
                    }

                    lengthToExamine = GetCharVector128SpanLength(offset, length);
                    if (lengthToExamine > offset)
                    {
                        Vector128<ushort> values = Vector128.Create(value);
                        Vector128<ushort> search = LoadVector128(ref searchSpace, offset);

                        // Same method as above
                        int matches = Sse2.MoveMask(Sse2.CompareEqual(values, search).AsByte());
                        if (matches == 0)
                        {
                            // Zero flags set so no matches
                            offset += Vector128<ushort>.Count;
                        }
                        else
                        {
                            // Find bitflag offset of first match and add to current offset, 
                            // flags are in bytes so divide for chars
                            return offset + (BitOps.TrailingZeroCount(matches) / sizeof(char));
                        }
                    }

                    if (offset < length)
                    {
                        lengthToExamine = length - offset;
                        goto SequentialScan;
                    }
                }
            }
            else if (Sse2.IsSupported)
            {
                if (offset < length)
                {
                    lengthToExamine = GetCharVector128SpanLength(offset, length);

                    Vector128<ushort> values = Vector128.Create(value);
                    while (lengthToExamine > offset)
                    {
                        Vector128<ushort> search = LoadVector128(ref searchSpace, offset);

                        // Same method as above
                        int matches = Sse2.MoveMask(Sse2.CompareEqual(values, search).AsByte());
                        if (matches == 0)
                        {
                            // Zero flags set so no matches
                            offset += Vector128<ushort>.Count;
                            continue;
                        }

                        // Find bitflag offset of first match and add to current offset, 
                        // flags are in bytes so divide for chars
                        return offset + (BitOps.TrailingZeroCount(matches) / sizeof(char));
                    }

                    if (offset < length)
                    {
                        lengthToExamine = length - offset;
                        goto SequentialScan;
                    }
                }
            }
            else if (Vector.IsHardwareAccelerated)
            {
                if (offset < length)
                {
                    lengthToExamine = GetCharVectorSpanLength(offset, length);

                    Vector<ushort> values = new Vector<ushort>(value);

                    while (lengthToExamine > offset)
                    {
                        var matches = Vector.Equals(values, LoadVector(ref searchSpace, offset));
                        if (Vector<ushort>.Zero.Equals(matches))
                        {
                            offset += Vector<ushort>.Count;
                            continue;
                        }

                        // Find offset of first match
                        return offset + LocateFirstFoundChar(matches);
                    }

                    if (offset < length)
                    {
                        lengthToExamine = length - offset;
                        goto SequentialScan;
                    }
                }
            }
            return -1;
        Found3:
            return offset + 3;
        Found2:
            return offset + 2;
        Found1:
            return offset + 1;
        Found:
            return offset;
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public static int IndexOfAny(ref char searchSpace, char value0, char value1, int length)
        {
            Debug.Assert(length >= 0);

            int offset = 0;
            int lengthToExamine = length;

            if (Avx2.IsSupported || Sse2.IsSupported)
            {
                // Avx2 branch also operates on Sse2 sizes, so check is combined.
                if (length >= Vector128<byte>.Count * 2)
                {
                    lengthToExamine = UnalignedCountVector128(ref searchSpace);
                }
            }
            else if (Vector.IsHardwareAccelerated)
            {
                if (length >= Vector<ushort>.Count * 2)
                {
                    lengthToExamine = UnalignedCountVector(ref searchSpace);
                }
            }

        SequentialScan:
            int lookUp;
            while (lengthToExamine >= 4)
            {
                lengthToExamine -= 4;

                lookUp = Unsafe.Add(ref searchSpace, offset);
                if (value0 == lookUp || value1 == lookUp)
                    goto Found;
                lookUp = Unsafe.Add(ref searchSpace, offset + 1);
                if (value0 == lookUp || value1 == lookUp)
                    goto Found1;
                lookUp = Unsafe.Add(ref searchSpace, offset + 2);
                if (value0 == lookUp || value1 == lookUp)
                    goto Found2;
                lookUp = Unsafe.Add(ref searchSpace, offset + 3);
                if (value0 == lookUp || value1 == lookUp)
                    goto Found3;

                offset += 4;
            }

            while (lengthToExamine > 0)
            {
                lengthToExamine -= 1;

                lookUp = Unsafe.Add(ref searchSpace, offset);
                if (value0 == lookUp || value1 == lookUp)
                    goto Found;

                offset += 1;
            }

            // We get past SequentialScan only if IsHardwareAccelerated or intrinsic .IsSupported is true. However, we still have the redundant check to allow
            // the JIT to see that the code is unreachable and eliminate it when the platform does not have hardware accelerated.
            if (Avx2.IsSupported)
            {
                if (offset < length)
                {
                    lengthToExamine = GetCharVector256SpanLength(offset, length);
                    if (lengthToExamine > offset)
                    {
                        Vector256<ushort> values0 = Vector256.Create(value0);
                        Vector256<ushort> values1 = Vector256.Create(value1);
                        do
                        {
                            Vector256<ushort> search = LoadVector256(ref searchSpace, offset);
                            // Bitwise Or to combine the flagged matches for the second value to our match flags
                            int matches = Avx2.MoveMask(
                                            Avx2.Or(
                                                Avx2.CompareEqual(values0, search),
                                                Avx2.CompareEqual(values1, search))
                                            .AsByte());
                            // Note that MoveMask has converted the equal vector elements into a set of bit flags,
                            // So the bit position in 'matches' corresponds to the element offset.
                            if (matches == 0)
                            {
                                // Zero flags set so no matches
                                offset += Vector256<ushort>.Count;
                                continue;
                            }

                            // Find bitflag offset of first match and add to current offset, 
                            // flags are in bytes so divide for chars
                            return offset + (BitOps.TrailingZeroCount(matches) / sizeof(char));
                        } while (lengthToExamine > offset);
                    }

                    lengthToExamine = GetCharVector128SpanLength(offset, length);
                    if (lengthToExamine > offset)
                    {
                        Vector128<ushort> values0 = Vector128.Create(value0);
                        Vector128<ushort> values1 = Vector128.Create(value1);
                        Vector128<ushort> search = LoadVector128(ref searchSpace, offset);

                        // Same method as above
                        int matches = Sse2.MoveMask(
                                        Sse2.Or(
                                            Sse2.CompareEqual(values0, search),
                                            Sse2.CompareEqual(values1, search))
                                        .AsByte());
                        if (matches == 0)
                        {
                            // Zero flags set so no matches
                            offset += Vector128<ushort>.Count;
                        }
                        else
                        {
                            // Find bitflag offset of first match and add to current offset, 
                            // flags are in bytes so divide for chars
                            return offset + (BitOps.TrailingZeroCount(matches) / sizeof(char));
                        }
                    }

                    if (offset < length)
                    {
                        lengthToExamine = length - offset;
                        goto SequentialScan;
                    }
                }
            }
            else if (Sse2.IsSupported)
            {
                if (offset < length)
                {
                    lengthToExamine = GetCharVector128SpanLength(offset, length);

                    Vector128<ushort> values0 = Vector128.Create(value0);
                    Vector128<ushort> values1 = Vector128.Create(value1);
                    while (lengthToExamine > offset)
                    {
                        Vector128<ushort> search = LoadVector128(ref searchSpace, offset);

                        // Same method as above
                        int matches = Sse2.MoveMask(
                                        Sse2.Or(
                                            Sse2.CompareEqual(values0, search),
                                            Sse2.CompareEqual(values1, search))
                                        .AsByte());
                        if (matches == 0)
                        {
                            // Zero flags set so no matches
                            offset += Vector128<ushort>.Count;
                            continue;
                        }

                        // Find bitflag offset of first match and add to current offset, 
                        // flags are in bytes so divide for chars
                        return offset + (BitOps.TrailingZeroCount(matches) / sizeof(char));
                    }

                    if (offset < length)
                    {
                        lengthToExamine = length - offset;
                        goto SequentialScan;
                    }
                }
            }
            else if (Vector.IsHardwareAccelerated)
            {
                if (offset < length)
                {
                    lengthToExamine = GetCharVectorSpanLength(offset, length);

                    Vector<ushort> values0 = new Vector<ushort>(value0);
                    Vector<ushort> values1 = new Vector<ushort>(value1);

                    while (lengthToExamine > offset)
                    {
                        Vector<ushort> search = LoadVector(ref searchSpace, offset);
                        var matches = Vector.BitwiseOr(
                                        Vector.Equals(search, values0),
                                        Vector.Equals(search, values1));
                        if (Vector<ushort>.Zero.Equals(matches))
                        {
                            offset += Vector<ushort>.Count;
                            continue;
                        }

                        // Find offset of first match
                        return offset + LocateFirstFoundChar(matches);
                    }

                    if (offset < length)
                    {
                        lengthToExamine = length - offset;
                        goto SequentialScan;
                    }
                }
            }
            return -1;
        Found3:
            return offset + 3;
        Found2:
            return offset + 2;
        Found1:
            return offset + 1;
        Found:
            return offset;
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public static int IndexOfAny(ref char searchSpace, char value0, char value1, char value2, int length)
        {
            Debug.Assert(length >= 0);

            int offset = 0;
            int lengthToExamine = length;

            if (Avx2.IsSupported || Sse2.IsSupported)
            {
                // Avx2 branch also operates on Sse2 sizes, so check is combined.
                if (length >= Vector128<byte>.Count * 2)
                {
                    lengthToExamine = UnalignedCountVector128(ref searchSpace);
                }
            }
            else if (Vector.IsHardwareAccelerated)
            {
                if (length >= Vector<ushort>.Count * 2)
                {
                    lengthToExamine = UnalignedCountVector(ref searchSpace);
                }
            }

        SequentialScan:
            int lookUp;
            while (lengthToExamine >= 4)
            {
                lengthToExamine -= 4;

                lookUp = Unsafe.Add(ref searchSpace, offset);
                if (value0 == lookUp || value1 == lookUp || value2 == lookUp)
                    goto Found;
                lookUp = Unsafe.Add(ref searchSpace, offset + 1);
                if (value0 == lookUp || value1 == lookUp || value2 == lookUp)
                    goto Found1;
                lookUp = Unsafe.Add(ref searchSpace, offset + 2);
                if (value0 == lookUp || value1 == lookUp || value2 == lookUp)
                    goto Found2;
                lookUp = Unsafe.Add(ref searchSpace, offset + 3);
                if (value0 == lookUp || value1 == lookUp || value2 == lookUp)
                    goto Found3;

                offset += 4;
            }

            while (lengthToExamine > 0)
            {
                lengthToExamine -= 1;
                
                lookUp = Unsafe.Add(ref searchSpace, offset);
                if (value0 == lookUp || value1 == lookUp || value2 == lookUp)
                    goto Found;

                offset += 1;
            }

            // We get past SequentialScan only if IsHardwareAccelerated or intrinsic .IsSupported is true. However, we still have the redundant check to allow
            // the JIT to see that the code is unreachable and eliminate it when the platform does not have hardware accelerated.
            if (Avx2.IsSupported)
            {
                if (offset < length)
                {
                    lengthToExamine = GetCharVector256SpanLength(offset, length);
                    if (lengthToExamine > offset)
                    {
                        Vector256<ushort> values0 = Vector256.Create(value0);
                        Vector256<ushort> values1 = Vector256.Create(value1);
                        Vector256<ushort> values2 = Vector256.Create(value2);
                        do
                        {
                            Vector256<ushort> search = LoadVector256(ref searchSpace, offset);

                            Vector256<ushort> matches0 = Avx2.CompareEqual(values0, search);
                            Vector256<ushort> matches1 = Avx2.CompareEqual(values1, search);
                            Vector256<ushort> matches2 = Avx2.CompareEqual(values2, search);
                            // Bitwise Or to combine the flagged matches for the second and third values to our match flags
                            int matches = Avx2.MoveMask(
                                            Avx2.Or(Avx2.Or(matches0, matches1), matches2)
                                            .AsByte());
                            // Note that MoveMask has converted the equal vector elements into a set of bit flags,
                            // So the bit position in 'matches' corresponds to the element offset.
                            if (matches == 0)
                            {
                                // Zero flags set so no matches
                                offset += Vector256<ushort>.Count;
                                continue;
                            }

                            // Find bitflag offset of first match and add to current offset, 
                            // flags are in bytes so divide for chars
                            return offset + (BitOps.TrailingZeroCount(matches) / sizeof(char));
                        } while (lengthToExamine > offset);
                    }

                    lengthToExamine = GetCharVector128SpanLength(offset, length);
                    if (lengthToExamine > offset)
                    {
                        Vector128<ushort> values0 = Vector128.Create(value0);
                        Vector128<ushort> values1 = Vector128.Create(value1);
                        Vector128<ushort> values2 = Vector128.Create(value2);

                        Vector128<ushort> search = LoadVector128(ref searchSpace, offset);

                        Vector128<ushort> matches0 = Sse2.CompareEqual(values0, search);
                        Vector128<ushort> matches1 = Sse2.CompareEqual(values1, search);
                        Vector128<ushort> matches2 = Sse2.CompareEqual(values2, search);

                        // Same method as above
                        int matches = Sse2.MoveMask(
                                        Sse2.Or(Sse2.Or(matches0, matches1), matches2)
                                        .AsByte());
                        if (matches == 0)
                        {
                            // Zero flags set so no matches
                            offset += Vector128<ushort>.Count;
                        }
                        else
                        {
                            // Find bitflag offset of first match and add to current offset, 
                            // flags are in bytes so divide for chars
                            return offset + (BitOps.TrailingZeroCount(matches) / sizeof(char));
                        }
                    }

                    if (offset < length)
                    {
                        lengthToExamine = length - offset;
                        goto SequentialScan;
                    }
                }
            }
            else if (Sse2.IsSupported)
            {
                if (offset < length)
                {
                    lengthToExamine = GetCharVector128SpanLength(offset, length);

                    Vector128<ushort> values0 = Vector128.Create(value0);
                    Vector128<ushort> values1 = Vector128.Create(value1);
                    Vector128<ushort> values2 = Vector128.Create(value2);
                    while (lengthToExamine > offset)
                    {
                        Vector128<ushort> search = LoadVector128(ref searchSpace, offset);

                        Vector128<ushort> matches0 = Sse2.CompareEqual(values0, search);
                        Vector128<ushort> matches1 = Sse2.CompareEqual(values1, search);
                        Vector128<ushort> matches2 = Sse2.CompareEqual(values2, search);

                        // Same method as above
                        int matches = Sse2.MoveMask(
                                        Sse2.Or(Sse2.Or(matches0, matches1), matches2)
                                        .AsByte());
                        if (matches == 0)
                        {
                            // Zero flags set so no matches
                            offset += Vector128<ushort>.Count;
                            continue;
                        }

                        // Find bitflag offset of first match and add to current offset, 
                        // flags are in bytes so divide for chars
                        return offset + (BitOps.TrailingZeroCount(matches) / sizeof(char));
                    }

                    if (offset < length)
                    {
                        lengthToExamine = length - offset;
                        goto SequentialScan;
                    }
                }
            }
            else if (Vector.IsHardwareAccelerated)
            {
                if (offset < length)
                {
                    lengthToExamine = GetCharVectorSpanLength(offset, length);

                    Vector<ushort> values0 = new Vector<ushort>(value0);
                    Vector<ushort> values1 = new Vector<ushort>(value1);
                    Vector<ushort> values2 = new Vector<ushort>(value2);

                    while (lengthToExamine > offset)
                    {
                        Vector<ushort> search = LoadVector(ref searchSpace, offset);
                        var matches = Vector.BitwiseOr(
                                        Vector.BitwiseOr(
                                            Vector.Equals(search, values0),
                                            Vector.Equals(search, values1)),
                                        Vector.Equals(search, values2));
                        if (Vector<ushort>.Zero.Equals(matches))
                        {
                            offset += Vector<ushort>.Count;
                            continue;
                        }

                        // Find offset of first match
                        return offset + LocateFirstFoundChar(matches);
                    }

                    if (offset < length)
                    {
                        lengthToExamine = length - offset;
                        goto SequentialScan;
                    }
                }
            }
            return -1;
        Found3:
            return offset + 3;
        Found2:
            return offset + 2;
        Found1:
            return offset + 1;
        Found:
            return offset;
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public static int IndexOfAny(ref char searchSpace, char value0, char value1, char value2, char value3, int length)
        {
            Debug.Assert(length >= 0);

            int offset = 0; // Use nint for arithmetic to avoid unnecessary 64->32->64 truncations
            int lengthToExamine = length;

            if (Avx2.IsSupported || Sse2.IsSupported)
            {
                // Avx2 branch also operates on Sse2 sizes, so check is combined.
                if (length >= Vector128<byte>.Count * 2)
                {
                    lengthToExamine = UnalignedCountVector128(ref searchSpace);
                }
            }
            else if (Vector.IsHardwareAccelerated)
            {
                if (length >= Vector<ushort>.Count * 2)
                {
                    lengthToExamine = UnalignedCountVector(ref searchSpace);
                }
            }

        SequentialScan:
            int lookUp;
            while (lengthToExamine >= 4)
            {
                lengthToExamine -= 4;

                lookUp = Unsafe.Add(ref searchSpace, offset);
                if (value0 == lookUp || value1 == lookUp || value2 == lookUp || value3 == lookUp)
                    goto Found;
                lookUp = Unsafe.Add(ref searchSpace, offset + 1);
                if (value0 == lookUp || value1 == lookUp || value2 == lookUp || value3 == lookUp)
                    goto Found1;
                lookUp = Unsafe.Add(ref searchSpace, offset + 2);
                if (value0 == lookUp || value1 == lookUp || value2 == lookUp || value3 == lookUp)
                    goto Found2;
                lookUp = Unsafe.Add(ref searchSpace, offset + 3);
                if (value0 == lookUp || value1 == lookUp || value2 == lookUp || value3 == lookUp)
                    goto Found3;

                offset += 4;
            }

            while (lengthToExamine > 0)
            {
                lengthToExamine -= 1;

                lookUp = Unsafe.Add(ref searchSpace, offset);
                if (value0 == lookUp || value1 == lookUp || value2 == lookUp || value3 == lookUp)
                    goto Found;

                offset += 1;
            }

            // We get past SequentialScan only if IsHardwareAccelerated or intrinsic .IsSupported is true. However, we still have the redundant check to allow
            // the JIT to see that the code is unreachable and eliminate it when the platform does not have hardware accelerated.
            if (Avx2.IsSupported)
            {
                if (offset < length)
                {
                    lengthToExamine = GetCharVector256SpanLength(offset, length);
                    if (lengthToExamine > offset)
                    {
                        Vector256<ushort> values0 = Vector256.Create(value0);
                        Vector256<ushort> values1 = Vector256.Create(value1);
                        Vector256<ushort> values2 = Vector256.Create(value2);
                        Vector256<ushort> values3 = Vector256.Create(value3);
                        do
                        {
                            Vector256<ushort> search = LoadVector256(ref searchSpace, offset);
                            // We preform the Or at non-Vector level as we are using the maximum number of non-preserved registers,
                            // and more causes them first to be pushed to stack and then popped on exit to preseve their values.
                            int matches = Avx2.MoveMask(Avx2.CompareEqual(values0, search).AsByte());
                            // Bitwise Or to combine the flagged matches for the second, third and fourth values to our match flags
                            matches |= Avx2.MoveMask(Avx2.CompareEqual(values1, search).AsByte());
                            matches |= Avx2.MoveMask(Avx2.CompareEqual(values2, search).AsByte());
                            matches |= Avx2.MoveMask(Avx2.CompareEqual(values3, search).AsByte());
                            // Note that MoveMask has converted the equal vector elements into a set of bit flags,
                            // So the bit position in 'matches' corresponds to the element offset.
                            if (matches == 0)
                            {
                                // Zero flags set so no matches
                                offset += Vector256<ushort>.Count;
                                continue;
                            }

                            // Find bitflag offset of first match and add to current offset, 
                            // flags are in bytes so divide for chars
                            return offset + (BitOps.TrailingZeroCount(matches) / sizeof(char));
                        } while (lengthToExamine > offset);
                    }

                    lengthToExamine = GetCharVector128SpanLength(offset, length);
                    if (lengthToExamine > offset)
                    {
                        Vector128<ushort> values0 = Vector128.Create(value0);
                        Vector128<ushort> values1 = Vector128.Create(value1);
                        Vector128<ushort> values2 = Vector128.Create(value2);
                        Vector128<ushort> values3 = Vector128.Create(value3);
                        Vector128<ushort> search = LoadVector128(ref searchSpace, offset);

                        // Same method as above
                        int matches = Sse2.MoveMask(Sse2.CompareEqual(values0, search).AsByte());
                        matches |= Sse2.MoveMask(Sse2.CompareEqual(values1, search).AsByte());
                        matches |= Sse2.MoveMask(Sse2.CompareEqual(values2, search).AsByte());
                        matches |= Sse2.MoveMask(Sse2.CompareEqual(values3, search).AsByte());
                        if (matches == 0)
                        {
                            // Zero flags set so no matches
                            offset += Vector128<ushort>.Count;
                        }
                        else
                        {
                            // Find bitflag offset of first match and add to current offset, 
                            // flags are in bytes so divide for chars
                            return offset + (BitOps.TrailingZeroCount(matches) / sizeof(char));
                        }
                    }

                    if (offset < length)
                    {
                        lengthToExamine = length - offset;
                        goto SequentialScan;
                    }
                }
            }
            else if (Sse2.IsSupported)
            {
                if (offset < length)
                {
                    lengthToExamine = GetCharVector128SpanLength(offset, length);

                    Vector128<ushort> values0 = Vector128.Create(value0);
                    Vector128<ushort> values1 = Vector128.Create(value1);
                    Vector128<ushort> values2 = Vector128.Create(value2);
                    Vector128<ushort> values3 = Vector128.Create(value3);
                    while (lengthToExamine > offset)
                    {
                        Vector128<ushort> search = LoadVector128(ref searchSpace, offset);

                        // Same method as above
                        int matches = Sse2.MoveMask(Sse2.CompareEqual(values0, search).AsByte());
                        matches |= Sse2.MoveMask(Sse2.CompareEqual(values1, search).AsByte());
                        matches |= Sse2.MoveMask(Sse2.CompareEqual(values2, search).AsByte());
                        matches |= Sse2.MoveMask(Sse2.CompareEqual(values3, search).AsByte());
                        if (matches == 0)
                        {
                            // Zero flags set so no matches
                            offset += Vector128<ushort>.Count;
                            continue;
                        }

                        // Find bitflag offset of first match and add to current offset, 
                        // flags are in bytes so divide for chars
                        return offset + (BitOps.TrailingZeroCount(matches) / sizeof(char));
                    }

                    if (offset < length)
                    {
                        lengthToExamine = length - offset;
                        goto SequentialScan;
                    }
                }
            }
            else if (Vector.IsHardwareAccelerated)
            {
                if (offset < length)
                {
                    lengthToExamine = GetCharVectorSpanLength(offset, length);

                    Vector<ushort> values0 = new Vector<ushort>(value0);
                    Vector<ushort> values1 = new Vector<ushort>(value1);
                    Vector<ushort> values2 = new Vector<ushort>(value2);
                    Vector<ushort> values3 = new Vector<ushort>(value3);

                    while (lengthToExamine > offset)
                    {
                        Vector<ushort> search = LoadVector(ref searchSpace, offset);
                        var matches = Vector.BitwiseOr(
                                            Vector.BitwiseOr(
                                                Vector.BitwiseOr(Vector.Equals(search, values0), Vector.Equals(search, values1)),
                                                Vector.Equals(search, values2)),
                                            Vector.Equals(search, values3));
                        if (Vector<ushort>.Zero.Equals(matches))
                        {
                            offset += Vector<ushort>.Count;
                            continue;
                        }

                        // Find offset of first match
                        return offset + LocateFirstFoundChar(matches);
                    }

                    if (offset < length)
                    {
                        lengthToExamine = length - offset;
                        goto SequentialScan;
                    }
                }
            }
            return -1;
        Found3:
            return offset + 3;
        Found2:
            return offset + 2;
        Found1:
            return offset + 1;
        Found:
            return offset;
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public static int IndexOfAny(ref char searchSpace, char value0, char value1, char value2, char value3, char value4, int length)
        {
            Debug.Assert(length >= 0);

            int offset = 0; // Use nint for arithmetic to avoid unnecessary 64->32->64 truncations
            int lengthToExamine = length;

            if (Avx2.IsSupported || Sse2.IsSupported)
            {
                // Avx2 branch also operates on Sse2 sizes, so check is combined.
                if (length >= Vector128<byte>.Count * 2)
                {
                    lengthToExamine = UnalignedCountVector128(ref searchSpace);
                }
            }
            else if (Vector.IsHardwareAccelerated)
            {
                if (length >= Vector<ushort>.Count * 2)
                {
                    lengthToExamine = UnalignedCountVector(ref searchSpace);
                }
            }

        SequentialScan:
            int lookUp;
            while (lengthToExamine >= 4)
            {
                lengthToExamine -= 4;

                lookUp = Unsafe.Add(ref searchSpace, offset);
                if (value0 == lookUp || value1 == lookUp || value2 == lookUp || value3 == lookUp || value4 == lookUp)
                    goto Found;
                lookUp = Unsafe.Add(ref searchSpace, offset + 1);
                if (value0 == lookUp || value1 == lookUp || value2 == lookUp || value3 == lookUp || value4 == lookUp)
                    goto Found1;
                lookUp = Unsafe.Add(ref searchSpace, offset + 2);
                if (value0 == lookUp || value1 == lookUp || value2 == lookUp || value3 == lookUp || value4 == lookUp)
                    goto Found2;
                lookUp = Unsafe.Add(ref searchSpace, offset + 3);
                if (value0 == lookUp || value1 == lookUp || value2 == lookUp || value3 == lookUp || value4 == lookUp)
                    goto Found3;

                offset += 4;
            }

            while (lengthToExamine > 0)
            {
                lengthToExamine -= 1;

                lookUp = Unsafe.Add(ref searchSpace, offset);
                if (value0 == lookUp || value1 == lookUp || value2 == lookUp || value3 == lookUp || value4 == lookUp)
                    goto Found;

                offset += 1;
            }

            // We get past SequentialScan only if IsHardwareAccelerated or intrinsic .IsSupported is true. However, we still have the redundant check to allow
            // the JIT to see that the code is unreachable and eliminate it when the platform does not have hardware accelerated.
            if (Avx2.IsSupported)
            {
                if (offset < length)
                {
                    lengthToExamine = GetCharVector256SpanLength(offset, length);
                    if (lengthToExamine > offset)
                    {
                        Vector256<ushort> values0 = Vector256.Create(value0);
                        Vector256<ushort> values1 = Vector256.Create(value1);
                        Vector256<ushort> values2 = Vector256.Create(value2);
                        Vector256<ushort> values3 = Vector256.Create(value3);
                        Vector256<ushort> values4 = Vector256.Create(value4);
                        do
                        {
                            Vector256<ushort> search = LoadVector256(ref searchSpace, offset);
                            // We preform the Or at non-Vector level as we are using the maximum number of non-preserved registers (+ 1),
                            // and more causes them first to be pushed to stack and then popped on exit to preseve their values.
                            int matches = Avx2.MoveMask(Avx2.CompareEqual(values0, search).AsByte());
                            // Bitwise Or to combine the flagged matches for the second, third, fourth and fifth values to our match flags
                            matches |= Avx2.MoveMask(Avx2.CompareEqual(values1, search).AsByte());
                            matches |= Avx2.MoveMask(Avx2.CompareEqual(values2, search).AsByte());
                            matches |= Avx2.MoveMask(Avx2.CompareEqual(values3, search).AsByte());
                            matches |= Avx2.MoveMask(Avx2.CompareEqual(values4, search).AsByte());
                            // Note that MoveMask has converted the equal vector elements into a set of bit flags,
                            // So the bit position in 'matches' corresponds to the element offset.
                            if (matches == 0)
                            {
                                // Zero flags set so no matches
                                offset += Vector256<ushort>.Count;
                                continue;
                            }

                            // Find bitflag offset of first match and add to current offset, 
                            // flags are in bytes so divide for chars
                            return offset + (BitOps.TrailingZeroCount(matches) / sizeof(char));
                        } while (lengthToExamine > offset);
                    }

                    lengthToExamine = GetCharVector128SpanLength(offset, length);
                    if (lengthToExamine > offset)
                    {
                        Vector128<ushort> values0 = Vector128.Create(value0);
                        Vector128<ushort> values1 = Vector128.Create(value1);
                        Vector128<ushort> values2 = Vector128.Create(value2);
                        Vector128<ushort> values3 = Vector128.Create(value3);
                        Vector128<ushort> values4 = Vector128.Create(value4);
                        Vector128<ushort> search = LoadVector128(ref searchSpace, offset);

                        // Same method as above
                        int matches = Sse2.MoveMask(Sse2.CompareEqual(values0, search).AsByte());
                        matches |= Sse2.MoveMask(Sse2.CompareEqual(values1, search).AsByte());
                        matches |= Sse2.MoveMask(Sse2.CompareEqual(values2, search).AsByte());
                        matches |= Sse2.MoveMask(Sse2.CompareEqual(values3, search).AsByte());
                        matches |= Sse2.MoveMask(Sse2.CompareEqual(values4, search).AsByte());
                        if (matches == 0)
                        {
                            // Zero flags set so no matches
                            offset += Vector128<ushort>.Count;
                        }
                        else
                        {
                            // Find bitflag offset of first match and add to current offset, 
                            // flags are in bytes so divide for chars
                            return offset + (BitOps.TrailingZeroCount(matches) / sizeof(char));
                        }
                    }

                    if (offset < length)
                    {
                        lengthToExamine = length - offset;
                        goto SequentialScan;
                    }
                }
            }
            else if (Sse2.IsSupported)
            {
                if (offset < length)
                {
                    lengthToExamine = GetCharVector128SpanLength(offset, length);

                    Vector128<ushort> values0 = Vector128.Create(value0);
                    Vector128<ushort> values1 = Vector128.Create(value1);
                    Vector128<ushort> values2 = Vector128.Create(value2);
                    Vector128<ushort> values3 = Vector128.Create(value3);
                    Vector128<ushort> values4 = Vector128.Create(value4);
                    while (lengthToExamine > offset)
                    {
                        Vector128<ushort> search = LoadVector128(ref searchSpace, offset);

                        // Same method as above
                        int matches = Sse2.MoveMask(Sse2.CompareEqual(values0, search).AsByte());
                        matches |= Sse2.MoveMask(Sse2.CompareEqual(values1, search).AsByte());
                        matches |= Sse2.MoveMask(Sse2.CompareEqual(values2, search).AsByte());
                        matches |= Sse2.MoveMask(Sse2.CompareEqual(values3, search).AsByte());
                        matches |= Sse2.MoveMask(Sse2.CompareEqual(values4, search).AsByte());
                        if (matches == 0)
                        {
                            // Zero flags set so no matches
                            offset += Vector128<ushort>.Count;
                            continue;
                        }

                        // Find bitflag offset of first match and add to current offset, 
                        // flags are in bytes so divide for chars
                        return offset + (BitOps.TrailingZeroCount(matches) / sizeof(char));
                    }

                    if (offset < length)
                    {
                        lengthToExamine = length - offset;
                        goto SequentialScan;
                    }
                }
            }
            else if (Vector.IsHardwareAccelerated)
            {
                if (offset < length)
                {
                    lengthToExamine = GetCharVectorSpanLength(offset, length);

                    Vector<ushort> values0 = new Vector<ushort>(value0);
                    Vector<ushort> values1 = new Vector<ushort>(value1);
                    Vector<ushort> values2 = new Vector<ushort>(value2);
                    Vector<ushort> values3 = new Vector<ushort>(value3);
                    Vector<ushort> values4 = new Vector<ushort>(value4);

                    while (lengthToExamine > offset)
                    {
                        Vector<ushort> search = LoadVector(ref searchSpace, offset);
                        var matches = Vector.BitwiseOr(
                                            Vector.BitwiseOr(
                                                Vector.BitwiseOr(
                                                    Vector.BitwiseOr(Vector.Equals(search, values0), Vector.Equals(search, values1)),
                                                    Vector.Equals(search, values2)),
                                                Vector.Equals(search, values3)),
                                            Vector.Equals(search, values4));
                        if (Vector<ushort>.Zero.Equals(matches))
                        {
                            offset += Vector<ushort>.Count;
                            continue;
                        }

                        // Find offset of first match
                        return offset + LocateFirstFoundChar(matches);
                    }

                    if (offset < length)
                    {
                        lengthToExamine = length - offset;
                        goto SequentialScan;
                    }
                }
            }
            return -1;
        Found3:
            return offset + 3;
        Found2:
            return offset + 2;
        Found1:
            return offset + 1;
        Found:
            return offset;
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public static unsafe int LastIndexOf(ref char searchSpace, char value, int length)
        {
            Debug.Assert(length >= 0);

            fixed (char* pChars = &searchSpace)
            {
                char* pCh = pChars + length;
                char* pEndCh = pChars;

                if (Vector.IsHardwareAccelerated)
                {
                    if (length >= Vector<ushort>.Count * 2)
                    {
                        length = UnalignedCountVectorFromEnd(ref searchSpace, length);
                    }
                }

            SequentialScan:
                while (length >= 4)
                {
                    length -= 4;
                    pCh -= 4;

                    if (*(pCh + 3) == value)
                        goto Found3;
                    if (*(pCh + 2) == value)
                        goto Found2;
                    if (*(pCh + 1) == value)
                        goto Found1;
                    if (*pCh == value)
                        goto Found;
                }

                while (length > 0)
                {
                    length -= 1;
                    pCh -= 1;

                    if (*pCh == value)
                        goto Found;
                }

                // We get past SequentialScan only if IsHardwareAccelerated is true. However, we still have the redundant check to allow
                // the JIT to see that the code is unreachable and eliminate it when the platform does not have hardware accelerated.
                if (Vector.IsHardwareAccelerated && pCh > pEndCh)
                {
                    // Get the highest multiple of Vector<ushort>.Count that is within the search space.
                    // That will be how many times we iterate in the loop below.
                    // This is equivalent to: length = Vector<ushort>.Count * ((int)(pCh - pEndCh) / Vector<ushort>.Count)
                    length = (int)((pCh - pEndCh) & ~(Vector<ushort>.Count - 1));

                    Vector<ushort> values = new Vector<ushort>(value);

                    while (length > 0)
                    {
                        char* pStart = pCh - Vector<ushort>.Count;
                        // Using Unsafe.Read instead of ReadUnaligned since the search space is pinned and pCh (and hence pSart) is always vector aligned
                        Debug.Assert(((int)pStart & (Unsafe.SizeOf<Vector<ushort>>() - 1)) == 0);
                        Vector<ushort> matches = Vector.Equals(values, Unsafe.Read<Vector<ushort>>(pStart));
                        if (Vector<ushort>.Zero.Equals(matches))
                        {
                            pCh -= Vector<ushort>.Count;
                            length -= Vector<ushort>.Count;
                            continue;
                        }
                        // Find offset of last match
                        return (int)(pStart - pEndCh) + LocateLastFoundChar(matches);
                    }

                    if (pCh > pEndCh)
                    {
                        length = (int)(pCh - pEndCh);
                        goto SequentialScan;
                    }
                }

                return -1;
            Found:
                return (int)(pCh - pEndCh);
            Found1:
                return (int)(pCh - pEndCh) + 1;
            Found2:
                return (int)(pCh - pEndCh) + 2;
            Found3:
                return (int)(pCh - pEndCh) + 3;
            }
        }

        // Vector sub-search adapted from https://github.com/aspnet/KestrelHttpServer/pull/1138
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int LocateFirstFoundChar(Vector<ushort> match)
        {
            var vector64 = Vector.AsVectorUInt64(match);
            ulong candidate = 0;
            int i = 0;
            // Pattern unrolled by jit https://github.com/dotnet/coreclr/pull/8001
            for (; i < Vector<ulong>.Count; i++)
            {
                candidate = vector64[i];
                if (candidate != 0)
                {
                    break;
                }
            }

            // Single LEA instruction with jitted const (using function result)
            return i * 4 + LocateFirstFoundChar(candidate);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int LocateFirstFoundChar(ulong match)
        {
            // TODO: Arm variants
            if (Bmi1.X64.IsSupported)
            {
                return (int)(Bmi1.X64.TrailingZeroCount(match) >> 4);
            }
            else
            {
                unchecked
                {
                    // Flag least significant power of two bit
                    var powerOfTwoFlag = match ^ (match - 1);
                    // Shift all powers of two into the high byte and extract
                    return (int)((powerOfTwoFlag * XorPowerOfTwoToHighChar) >> 49);
                }
            }
        }

        private const ulong XorPowerOfTwoToHighChar = (0x03ul |
                                                       0x02ul << 16 |
                                                       0x01ul << 32) + 1;

        // Vector sub-search adapted from https://github.com/aspnet/KestrelHttpServer/pull/1138
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int LocateLastFoundChar(Vector<ushort> match)
        {
            var vector64 = Vector.AsVectorUInt64(match);
            ulong candidate = 0;
            int i = Vector<ulong>.Count - 1;
            // Pattern unrolled by jit https://github.com/dotnet/coreclr/pull/8001
            for (; i >= 0; i--)
            {
                candidate = vector64[i];
                if (candidate != 0)
                {
                    break;
                }
            }

            // Single LEA instruction with jitted const (using function result)
            return i * 4 + LocateLastFoundChar(candidate);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int LocateLastFoundChar(ulong match)
        {
            // TODO: Arm variants
            if (Lzcnt.X64.IsSupported)
            {
                return 3 - (int)(Lzcnt.X64.LeadingZeroCount(match) >> 4);
            }
            else
            {
                // Find the most significant char that has its highest bit set
                int index = 3;
                while ((long)match > 0)
                {
                    match = match << 16;
                    index--;
                }
                return index;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe Vector<ushort> LoadVector(ref char start, int offset)
            => Unsafe.ReadUnaligned<Vector<ushort>>(ref Unsafe.As<char, byte>(ref Unsafe.Add(ref start, offset)));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe Vector128<ushort> LoadVector128(ref char start, int offset)
            => Unsafe.ReadUnaligned<Vector128<ushort>>(ref Unsafe.As<char, byte>(ref Unsafe.Add(ref start, offset)));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe Vector256<ushort> LoadVector256(ref char start, int offset)
            => Unsafe.ReadUnaligned<Vector256<ushort>>(ref Unsafe.As<char, byte>(ref Unsafe.Add(ref start, offset)));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe UIntPtr LoadUIntPtr(ref char start, int offset)
            => Unsafe.ReadUnaligned<UIntPtr>(ref Unsafe.As<char, byte>(ref Unsafe.Add(ref start, offset)));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe int GetCharVectorSpanLength(int offset, int length)
            => ((length - offset) & ~(Vector<ushort>.Count - 1));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe int GetCharVector128SpanLength(int offset, int length)
            => ((length - offset) & ~(Vector128<ushort>.Count - 1));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe int GetCharVector256SpanLength(int offset, int length)
            => ((length - offset) & ~(Vector256<ushort>.Count - 1));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe int UnalignedCountVector(ref char searchSpace)
        {
            const int elementsPerByte = sizeof(ushort) / sizeof(byte);
            // Figure out how many characters to read sequentially until we are vector aligned
            // This is equivalent to:
            //         unaligned = ((int)pCh % Unsafe.SizeOf<Vector<ushort>>()) / elementsPerByte
            //         length = (Vector<ushort>.Count - unaligned) % Vector<ushort>.Count
            int unaligned = ((int)Unsafe.AsPointer(ref searchSpace) & (Unsafe.SizeOf<Vector<ushort>>() - 1)) / elementsPerByte;
            return ((Vector<ushort>.Count - unaligned) & (Vector<ushort>.Count - 1));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe int UnalignedCountVector128(ref char searchSpace)
        {
            const int elementsPerByte = sizeof(ushort) / sizeof(byte);

            int unaligned = ((int)Unsafe.AsPointer(ref searchSpace) & (Unsafe.SizeOf<Vector128<ushort>>() - 1)) / elementsPerByte;
            return ((Vector128<ushort>.Count - unaligned) & (Vector128<ushort>.Count - 1));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe int UnalignedCountVectorFromEnd(ref char searchSpace, int length)
        {
            const int elementsPerByte = sizeof(ushort) / sizeof(byte);
            // Figure out how many characters to read sequentially from the end until we are vector aligned
            // This is equivalent to: length = ((int)pCh % Unsafe.SizeOf<Vector<ushort>>()) / elementsPerByte
            int unaligned = ((int)Unsafe.AsPointer(ref searchSpace) & (Unsafe.SizeOf<Vector<ushort>>() - 1)) / elementsPerByte;
            return ((length & (Vector<ushort>.Count - 1)) + unaligned) & (Vector<ushort>.Count - 1);
        }
    }
}
