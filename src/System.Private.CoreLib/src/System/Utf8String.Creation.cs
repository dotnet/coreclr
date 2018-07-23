// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Internal.Runtime.CompilerServices;

#if BIT64
using nint = System.Int64;
using nuint = System.UInt64;
#else // BIT64
using nint = System.Int32;
using nuint = System.UInt32;
#endif // BIT64

namespace System
{
    public sealed partial class Utf8String
    {
        // The constructors of Utf8String are special since the JIT will replace newobj instructions
        // with calls to the corresponding 'Ctor' method. Depending on the CLR in use, the ctor methods
        // may be instance methods (with a null 'this' parameter) or static methods.
        //
        // To add a new ctor overload, make changes to the following files:
        //
        // - src/vm/ecall.cpp, update the definition of "NumberOfUtf8StringConstructors" and add the
        //   appropriate static asserts immediately above the definition.
        //
        // - src/vm/ecall.h, search for "Utf8StringCtor" and add the DYNAMICALLY_ASSIGNED_FCALL_IMPL
        //   definitions corresponding to the new overloads.
        //
        // - src/vm/ecalllist.h, search for "FCFuncStart(gUtf8StringFuncs)" and add the overloads
        //   within that block.
        //
        // - src/vm/metasig.h, add the new Utf8String-returning metasig declarations; and, if necessary,
        //   add any void-returning metasig declarations if they haven't already been defined elsewhere.
        //   search "String_RetUtf8Str" for an example of how to do this.
        //
        // - src/vm/mscorlib.h, search "DEFINE_CLASS(UTF8_STRING" and add the new DEFINE_METHOD
        //   declarations for the Utf8String-returning Ctor methods, referencing the new metasig declarations.
        //
        // The default behavior of each ctor is to validate the input, replacing invalid sequences with the
        // Unicode replacement character U+FFFD. The resulting Utf8String instance will be well-formed but
        // might not have full fidelity with the input data. This behavior can be controlled by calling
        // any of the Create instances and specifying a different action.

        /*
         * Create a string from existing UTF-8 data.
         * When a byte* is provided, we'll assume a pointer to a null-terminated UTF-8 string.
         */

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern Utf8String(ReadOnlySpan<byte> value);

#if PROJECTN
        [DependencyReductionRoot]
#endif
#if !CORECLR
        static
#endif
        private Utf8String Ctor(ReadOnlySpan<byte> value) => Create(value);

        public static Utf8String Create(ReadOnlySpan<byte> value, InvalidSequenceBehavior behavior = InvalidSequenceBehavior.ReplaceInvalidSequence)
        {
            if (!UnicodeHelpers.IsInRangeInclusive((uint)behavior, (uint)InvalidSequenceBehavior.Fail, (uint)InvalidSequenceBehavior.LeaveUnchanged))
            {
                throw new ArgumentOutOfRangeException(paramName: nameof(behavior));
            }

            if (value.IsEmpty)
            {
                return Empty;
            }

            return new UnbakedUtf8String(value).Bake(behavior);
        }

        public static Utf8String Create<TState>(int length, TState state, SpanAction<byte, TState> action, InvalidSequenceBehavior behavior = InvalidSequenceBehavior.ReplaceInvalidSequence)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            if (!UnicodeHelpers.IsInRangeInclusive((uint)behavior, (uint)InvalidSequenceBehavior.Fail, (uint)InvalidSequenceBehavior.LeaveUnchanged))
            {
                throw new ArgumentOutOfRangeException(paramName: nameof(behavior));
            }

            if (length < 0)
            {
                if (length == 0)
                {
                    return Empty;
                }
                else
                {
                    throw new ArgumentOutOfRangeException(paramName: nameof(length));
                }
            }

            var unbaked = new UnbakedUtf8String(length);
            action(unbaked.GetSpan(), state);
            return unbaked.Bake(behavior);
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern Utf8String(byte[] value, int startIndex, int length);

#if PROJECTN
        [DependencyReductionRoot]
#endif
#if !CORECLR
        static
#endif
        private Utf8String Ctor(byte[] value, int startIndex, int length)
        {
            // TODO: Real parameter validation with friendlier exception messages
            return Ctor(value.AsSpan(startIndex, length));
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        [CLSCompliant(false)]
        public unsafe extern Utf8String(byte* value);

#if PROJECTN
        [DependencyReductionRoot]
#endif
#if !CORECLR
        static
#endif
        private unsafe Utf8String Ctor(byte* value)
        {
            if (value == null)
            {
                return Empty;
            }

            return Ctor(new ReadOnlySpan<byte>(value, checked((int)strlen(value))));
        }

        /*
         * Create a string from existing UTF-16 data.
         * Since this involves transcoding, validation cannot be skipped.
         * When a char* is provided, we'll assume a pointer to a null-terminated UTF-16 string.
         */

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern Utf8String(ReadOnlySpan<char> value);

#if PROJECTN
        [DependencyReductionRoot]
#endif
#if !CORECLR
        static
#endif
        private Utf8String Ctor(ReadOnlySpan<char> value)
        {
            if (value.IsEmpty)
            {
                return Empty;
            }

            // TODO: Use a list of buffers and perform the transcoding piecemeal,
            // which reduces the constant factor on the O(n) operation.

            Characteristics characteristics = Characteristics.IsWellFormed; // transcoding always results in a well-formed output

            Encoding e = Encoding.UTF8;
            int numBytesRequired = e.GetByteCount(value);

            // During UTF-16 to UTF-8 transcoding, the number of code units will stay the same only if the original input is
            // ASCII. Non-ASCII UTF-16 code units always expand into multiple UTF-8 code units (and a UTF-16 surrogate pair
            // expands to 4 UTF-8 code units). Additionally, unpaired surrogate UTF-16 code units will get translated to the
            // 3 UTF-8 code unit sequence U+FFFD by our transcoder.

            if (numBytesRequired == value.Length)
            {
                characteristics |= Characteristics.IsAscii;
            }

            var unbaked = new UnbakedUtf8String(e.GetByteCount(value));

            int numBytesConverted = e.GetBytes(value, unbaked.GetSpan());
            Debug.Assert(numBytesConverted == unbaked.Length, "Incorrect number of bytes converted.");

            unbaked.ApplyCharacteristics(characteristics);
            return unbaked.BakeWithoutValidation();
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern Utf8String(char[] value, int startIndex, int length);

#if PROJECTN
        [DependencyReductionRoot]
#endif
#if !CORECLR
        static
#endif
        private Utf8String Ctor(char[] value, int startIndex, int length)
        {
            // TODO: Real parameter validation with friendlier exception messages
            return Ctor(value.AsSpan(startIndex, length));
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        [CLSCompliant(false)]
        public unsafe extern Utf8String(char* value);

#if PROJECTN
        [DependencyReductionRoot]
#endif
#if !CORECLR
        static
#endif
        private unsafe Utf8String Ctor(char* value)
        {
            if (value == null)
            {
                return Empty;
            }

            return Ctor(new ReadOnlySpan<char>(value, string.wcslen(value)));
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern Utf8String(string value);

#if PROJECTN
        [DependencyReductionRoot]
#endif
#if !CORECLR
        static
#endif
        private Utf8String Ctor(string value)
        {
            // TODO: Null check parameter
            // TODO: Check for interning

            return Ctor(value.AsSpan());
        }

        /*
         * HELPER ROUTINES
         */

        /// <summary>
        /// Creates a new zero-initialized instance of a <see cref="Utf8String"/> whose <see cref="Length"/> property
        /// is equal to the <paramref name="length"/> parameter. Internally, an extra byte is allocated at the very end
        /// of the object to hold the null terminator, just like with the <see cref="string"/> class.
        /// </summary>
        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern Utf8String FastAllocate(int length);

        /// <summary>
        /// Represents a <see cref="Utf8String"/> instance that is "unbaked", i.e., is still mutable.
        /// </summary>
        private readonly ref struct UnbakedUtf8String
        {
            private readonly Utf8String _value;

            /// <summary>
            /// Creates an unbaked UTF-8 string with the specified length (in UTF-8 code units).
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public UnbakedUtf8String(int length)
            {
                _value = FastAllocate(length);
            }

            /// <summary>
            /// Creates an unbaked UTF-8 string from the specified UTF-8 source data.
            /// </summary>
            public UnbakedUtf8String(ReadOnlySpan<byte> source)
                : this(source.Length)
            {
                Debug.Assert(_value.Length == source.Length, "Constructor created Utf8String instance with incorrect size.");

                Buffer.Memmove(
                    destination: ref _value.DangerousGetMutableReference(),
                    source: ref MemoryMarshal.GetReference(source),
                    elementCount: (nuint)source.Length);
            }

            public int Length => _value.Length;

            public void ApplyCharacteristics(Characteristics flags)
            {
                // TODO: Set flags on the Utf8String before returning it
            }

            public Utf8String Bake(InvalidSequenceBehavior behavior)
            {
                Debug.Assert(_value != null, "This method should only be called immediately after construction of a new Utf8String instance.");
                // TODO: Also Debug.Assert that flags haven't been set yet, as that would indicate a call at an inappropriate time

                if ((uint)behavior >= (uint)InvalidSequenceBehavior.LeaveUnchanged)
                {
                    return _value; // no validation or setting of flags
                }

                // Perform validation

                int indexOfFirstInvalidUtf8Sequence = Utf8Utility.GetIndexOfFirstInvalidUtf8Sequence(_value.AsSpan(), out bool isAscii);
                if (indexOfFirstInvalidUtf8Sequence < 0)
                {
                    // TODO: Set IsWellFormed flag on the Utf8String instance.
                    // TODO: Also set IsAscii flag on the Utf8String instance if appropriate.
                    return _value;
                }

                // At this point, we know validation failed, so what action must we take?

                if (behavior == InvalidSequenceBehavior.Fail)
                {
                    // TODO: Use a better exception type and move error message to resource
                    throw new InvalidOperationException("Bad data.");
                }

                Debug.Assert(behavior == InvalidSequenceBehavior.ReplaceInvalidSequence, "The only other behavior possible is U+FFFD replacement.");

                // Replace invalid sequences with U+FFFD.
                // We assume invalid sequences will be very rare, so it's ok for this not to be highly optimized
                // (including even performing allocations) as long as total complexity is still O(n).
                //
                // First, calculate how many total bytes will be needed to store the substitutions.
                // The worst case is an input of all-invalid data [ FF FF FF ... ], which expands to 3x its
                // original size when substitutions are performed.

                int totalBytesRequired = indexOfFirstInvalidUtf8Sequence;
                var inputBuffer = _value.AsSpan().Slice(totalBytesRequired);

                // Then, in a loop, skip over ill-formed subsequences.

                checked // since we're possibly expanding the buffer size and don't want to integer overflow
                {
                    while (true)
                    {
                        var peekResult = UnicodeReader.PeekFirstScalarUtf8(inputBuffer);
                        Debug.Assert(peekResult.status != SequenceValidity.Valid, "Didn't expect to find a valid sequence here.");
                        totalBytesRequired += 3; // we're going to replace this with a U+FFFD, which is 3 UTF-8 code units

                        inputBuffer = inputBuffer.Slice(peekResult.charsConsumed);
                        int numBytesInNextValidRun = Utf8Utility.GetIndexOfFirstInvalidUtf8Sequence(inputBuffer, out _);
                        if (numBytesInNextValidRun < 0)
                        {
                            // Entire remaining run is valid, and we're done computing the required size
                            break;
                        }

                        totalBytesRequired += numBytesInNextValidRun;
                        inputBuffer = inputBuffer.Slice(numBytesInNextValidRun);
                    }

                    // Drain the remainder of the input buffer
                    totalBytesRequired += inputBuffer.Length;
                }

                // We now know how many bytes are required.
                // Create a new Utf8String instance and begin populating it.

                Utf8String newInstance = FastAllocate(totalBytesRequired);
                inputBuffer = _value.AsSpan();
                var destBuffer = newInstance.AsMutableSpan();

                // First, copy over the initial data we know to be valid.

                inputBuffer.Slice(0, indexOfFirstInvalidUtf8Sequence).CopyTo(destBuffer);
                inputBuffer = inputBuffer.Slice(indexOfFirstInvalidUtf8Sequence);
                destBuffer = destBuffer.Slice(indexOfFirstInvalidUtf8Sequence);

                // Then, in a loop, skip over ill-formed subsequences, substituting U+FFFD.

                while (true)
                {
                    var peekResult = UnicodeReader.PeekFirstScalarUtf8(inputBuffer);
                    Debug.Assert(peekResult.status != SequenceValidity.Valid, "Didn't expect to find a valid sequence here.");

                    // Write out U+FFFD, which in UTF-8 is the three-code unit sequence [ EF BF BD ].
                    // We can get away with writing four bytes at a time due to the null terminator at the end of the Utf8String instance.
                    // If we need to write U+FFFD at the end of the string, we'll just end up overwriting the null terminator with another null byte.

                    Debug.Assert(destBuffer.Length >= 3, "Destination buffer too small to receive U+FFFD.");
                    Unsafe.WriteUnaligned<uint>(ref MemoryMarshal.GetReference(destBuffer), (BitConverter.IsLittleEndian) ? 0x00BDBFEFU : 0xEFBFBD00U);

                    inputBuffer = inputBuffer.Slice(peekResult.charsConsumed);
                    destBuffer = destBuffer.Slice(3);

                    int numBytesInNextValidRun = Utf8Utility.GetIndexOfFirstInvalidUtf8Sequence(inputBuffer, out _);
                    if (numBytesInNextValidRun < 0)
                    {
                        // Entire remaining run is valid
                        break;
                    }

                    inputBuffer.Slice(0, numBytesInNextValidRun).CopyTo(destBuffer);
                    inputBuffer = inputBuffer.Slice(numBytesInNextValidRun);
                    destBuffer = destBuffer.Slice(numBytesInNextValidRun);
                }

                // Drain the remainder of the input buffer
                Debug.Assert(inputBuffer.Length == destBuffer.Length, "Input buffer and destination buffer sizes should match for final drain.");
                inputBuffer.CopyTo(destBuffer);

                // And we're finished!
                // The data in the new instance is guaranteed well-formed.

                // TODO: Set IsWellFormed flag on the Utf8String instance.
                return newInstance;
            }

            public Utf8String BakeWithoutValidation()
            {
                return _value;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Span<byte> GetSpan()
            {
                return MemoryMarshal.CreateSpan(ref _value.DangerousGetMutableReference(), _value._length);
            }
        }
    }
}
