// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Unicode;

namespace System
{
    public sealed partial class Utf8String
    {
        /*
         * CONSTRUCTORS
         *
         * The constructors of Utf8String are special since the JIT will replace newobj instructions
         * with calls to the corresponding 'Ctor' method. Depending on the CLR in use, the ctor methods
         * may be instance methods (with a null 'this' parameter) or static methods.
         *
         * To add a new ctor overload, make changes to the following files:
         *
         * - src/vm/ecall.cpp, update the definition of "NumberOfUtf8StringConstructors" and add the
         *   appropriate static asserts immediately above the definition.
         *
         * - src/vm/ecall.h, search for "Utf8StringCtor" and add the DYNAMICALLY_ASSIGNED_FCALL_IMPL
         *   definitions corresponding to the new overloads.
         *
         * - src/vm/ecalllist.h, search for "FCFuncStart(gUtf8StringFuncs)" and add the overloads
         *   within that block.
         *
         * - src/vm/metasig.h, add the new Utf8String-returning metasig declarations; and, if necessary,
         *   add any void-returning metasig declarations if they haven't already been defined elsewhere.
         *   search "String_RetUtf8Str" for an example of how to do this.
         *
         * - src/vm/mscorlib.h, search "DEFINE_CLASS(UTF8_STRING" and add the new DEFINE_METHOD
         *   declarations for the Utf8String-returning Ctor methods, referencing the new metasig declarations.
         *
         * The default behavior of each ctor is to validate the input, replacing invalid sequences with the
         * Unicode replacement character U+FFFD. The resulting Utf8String instance will be well-formed but
         * might not have full fidelity with the input data. This behavior can be controlled by calling
         * any of the Create instances and specifying a different action.
         */

        /// <summary>
        /// Creates a <see cref="Utf8String"/> instance from existing UTF-8 data.
        /// </summary>
        /// <remarks>
        /// The UTF-8 data in <paramref name="value"/> is validated for well-formedness upon construction.
        /// Invalid code unit sequences are replaced with U+FFFD in the resulting <see cref="Utf8String"/>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern Utf8String(ReadOnlySpan<byte> value);

#if PROJECTN
        [DependencyReductionRoot]
#endif
#if !CORECLR
        static
#endif
        private Utf8String Ctor(ReadOnlySpan<byte> value)
        {
            if (value.IsEmpty)
            {
                return Empty;
            }

            Utf8String newString = FastAllocate(value.Length);
            Buffer.Memmove(ref newString.DangerousGetMutableReference(), ref MemoryMarshal.GetReference(value), (uint)value.Length);
            return Utf8Utility.ValidateAndFixupUtf8String(newString);
        }

        /// <summary>
        /// Creates a <see cref="Utf8String"/> instance from existing UTF-8 data.
        /// </summary>
        /// <remarks>
        /// The UTF-8 data in <paramref name="value"/> is validated for well-formedness upon construction.
        /// Invalid code unit sequences are replaced with U+FFFD in the resulting <see cref="Utf8String"/>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern Utf8String(byte[] value, int startIndex, int length);

#if PROJECTN
        [DependencyReductionRoot]
#endif
#if !CORECLR
        static
#endif
        private Utf8String Ctor(byte[] value, int startIndex, int length) => Ctor(new ReadOnlySpan<byte>(value, startIndex, length));

        /// <summary>
        /// Creates a <see cref="Utf8String"/> instance from existing null-terminated UTF-8 data.
        /// </summary>
        /// <remarks>
        /// The UTF-8 data in <paramref name="value"/> is validated for well-formedness upon construction.
        /// Invalid code unit sequences are replaced with U+FFFD in the resulting <see cref="Utf8String"/>.
        /// </remarks>
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

            return Ctor(new ReadOnlySpan<byte>(value, string.strlen(value)));
        }

        /// <summary>
        /// Creates a <see cref="Utf8String"/> instance from existing UTF-16 data.
        /// </summary>
        /// <remarks>
        /// The UTF-16 data in <paramref name="value"/> is validated for well-formedness upon construction.
        /// Invalid code unit sequences are replaced with U+FFFD in the resulting <see cref="Utf8String"/>.
        /// </remarks>
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

            // TODO: Replace me with a better implementation.

            Utf8String newString = FastAllocate(Encoding.UTF8.GetByteCount(value));
            Encoding.UTF8.GetBytes(value, new Span<byte>(ref newString.DangerousGetMutableReference(), newString.Length));
            return newString;
        }

        /// <summary>
        /// Creates a <see cref="Utf8String"/> instance from existing UTF-16 data.
        /// </summary>
        /// <remarks>
        /// The UTF-16 data in <paramref name="value"/> is validated for well-formedness upon construction.
        /// Invalid code unit sequences are replaced with U+FFFD in the resulting <see cref="Utf8String"/>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern Utf8String(char[] value, int startIndex, int length);

#if PROJECTN
        [DependencyReductionRoot]
#endif
#if !CORECLR
        static
#endif
        private Utf8String Ctor(char[] value, int startIndex, int length) => Ctor(new ReadOnlySpan<char>(value, startIndex, length));

        /// <summary>
        /// Creates a <see cref="Utf8String"/> instance from existing null-terminated UTF-16 data.
        /// </summary>
        /// <remarks>
        /// The UTF-16 data in <paramref name="value"/> is validated for well-formedness upon construction.
        /// Invalid code unit sequences are replaced with U+FFFD in the resulting <see cref="Utf8String"/>.
        /// </remarks>
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

        /// <summary>
        /// Creates a <see cref="Utf8String"/> instance from existing UTF-16 data.
        /// </summary>
        /// <remarks>
        /// The UTF-16 data in <paramref name="value"/> is validated for well-formedness upon construction.
        /// Invalid code unit sequences are replaced with U+FFFD in the resulting <see cref="Utf8String"/>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern Utf8String(string value);

#if PROJECTN
        [DependencyReductionRoot]
#endif
#if !CORECLR
        static
#endif
        private Utf8String Ctor(string value) => Ctor(value.AsSpan());

        /*
         * HELPER METHODS
         */

        /// <summary>
        /// Creates a <see cref="Utf8String"/> instance from existing data, bypassing validation.
        /// Also allows the caller to set flags dictating various attributes of the data.
        /// </summary>
        internal static Utf8String DangerousCreateWithoutValidation(ReadOnlySpan<byte> utf8Data, bool assumeWellFormed = false, bool assumeAscii = false)
        {
            if (utf8Data.IsEmpty)
            {
                return Empty;
            }

            Utf8String newString = FastAllocate(utf8Data.Length);
            utf8Data.CopyTo(new Span<byte>(ref newString.DangerousGetMutableReference(), newString.Length));
            return newString;
        }

        /// <summary>
        /// Creates a new zero-initialized instance of the specified length. Actual storage allocated is "length + 1" bytes
        /// because instances are null-terminated.
        /// </summary>
        /// <remarks>
        /// The implementation of this method checks its input argument for overflow.
        /// </remarks>
        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern Utf8String FastAllocate(int length);
    }
}
