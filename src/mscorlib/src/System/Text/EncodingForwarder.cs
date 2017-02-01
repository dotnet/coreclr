// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.Text
{
    using System.Diagnostics.Contracts;
    // Shared implementations for commonly overriden Encoding methods

    internal static class EncodingForwarder
    {
        // We normally have to duplicate a lot of code between UTF8Encoding,
        // UTF7Encoding, EncodingNLS, etc. because we want to override many
        // of the methods in all of those classes to just forward to the unsafe
        // version. (e.g. GetBytes(char[]))
        // Ideally, everything would just derive from EncodingNLS, but that's
        // not exposed in the public API, and C# prohibits a public class from
        // inheriting from an internal one. So, we have to override each of the
        // methods in question and repeat the argument validation/logic.

        // These set of methods exist so instead of duplicating code, we can
        // simply have those overriden methods call here to do the actual work.

        // NOTE: This class should ONLY be called from Encodings that override
        // the internal methods which accept an Encoder/DecoderNLS. The reason
        // for this is that by default, those methods just call the same overload
        // except without the encoder/decoder parameter. If an overriden method
        // without that parameter calls this class, which calls the overload with
        // the parameter, it will call the same method again, which will eventually
        // lead to a StackOverflowException.

        public unsafe static int GetByteCount(Encoding encoding, char[] chars, int index, int count)
        {
            // Validate parameters
            Debug.Assert(encoding != null); // this parameter should only be affected internally, so just do a debug check here
            if (chars == null || index < 0 || count < 0 ||
                (chars.Length - index < count))
            {
                ThrowValidationFailedException(chars, index, count);
            }
            Contract.EndContractBlock();

            // If no input, return 0, avoid fixed empty array problem
            if (count == 0)
                return 0;

            // Just call the (internal) pointer version
            fixed (char* pChars = chars)
                return encoding.GetByteCount(pChars + index, count, encoder: null);
        }

        public unsafe static int GetByteCount(Encoding encoding, string s)
        {
            Debug.Assert(encoding != null);
            if (s == null)
            {
                ThrowValidationFailedException(encoding);
            }
            Contract.EndContractBlock();

            // NOTE: The behavior of fixed *is* defined by
            // the spec for empty strings, although not for
            // null strings/empty char arrays. See
            // http://stackoverflow.com/q/37757751/4077294
            // Regardless, we may still want to check
            // for if (s.Length == 0) in the future
            // and short-circuit as an optimization (TODO).

            fixed (char* pChars = s)
                return encoding.GetByteCount(pChars, s.Length, encoder: null);
        }

        public unsafe static int GetByteCount(Encoding encoding, char* chars, int count)
        {
            Debug.Assert(encoding != null);
            if (chars == null || count < 0)
            {
                EncodingForwarder.ThrowValidationFailedException(chars, count);
            }
            Contract.EndContractBlock();

            // Call the internal version, with an empty encoder
            return encoding.GetByteCount(chars, count, encoder: null);
        }

        public unsafe static int GetBytes(Encoding encoding, string s, int charIndex, int charCount, byte[] bytes, int byteIndex)
        {
            Debug.Assert(encoding != null);
            if (s == null || bytes == null ||
                charIndex < 0 || charCount < 0 ||
                (s.Length - charIndex < charCount) ||
                (byteIndex < 0 || byteIndex > bytes.Length))
            {
                ThrowValidationFailedException(encoding, s, charIndex, charCount, bytes);
            }
            Contract.EndContractBlock();

            int byteCount = bytes.Length - byteIndex;

            // Fixed doesn't like empty arrays
            if (bytes.Length == 0)
                bytes = new byte[1];
            
            fixed (char* pChars = s) fixed (byte* pBytes = &bytes[0])
            {
                return encoding.GetBytes(pChars + charIndex, charCount, pBytes + byteIndex, byteCount, encoder: null);
            }
        }

        public unsafe static int GetBytes(Encoding encoding, char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
        {
            Debug.Assert(encoding != null);
            // Validate parameters
            if (chars == null || bytes == null || charIndex < 0 || charCount < 0 ||
                (chars.Length - charIndex < charCount) ||
                (byteIndex < 0 || byteIndex > bytes.Length))
            {
                ThrowValidationFailedException(chars, charIndex, charCount, bytes);
            }
            Contract.EndContractBlock();

            // If nothing to encode return 0, avoid fixed problem
            if (charCount == 0)
                return 0;

            // Note that this is the # of bytes to decode,
            // not the size of the array
            int byteCount = bytes.Length - byteIndex;

            // Fixed doesn't like 0 length arrays.
            if (bytes.Length == 0)
                bytes = new byte[1];
            
            // Just call the (internal) pointer version
            fixed (char* pChars = chars) fixed (byte* pBytes = &bytes[0])
            {
                return encoding.GetBytes(pChars + charIndex, charCount, pBytes + byteIndex, byteCount, encoder: null);
            }
        }

        public unsafe static int GetBytes(Encoding encoding, char* chars, int charCount, byte* bytes, int byteCount)
        {
            Debug.Assert(encoding != null);
            // Validate parameters
            if (bytes == null || chars == null || charCount < 0 || byteCount < 0)
            {
                ThrowValidationFailedException(chars, charCount, bytes);
            }
            Contract.EndContractBlock();

            return encoding.GetBytes(chars, charCount, bytes, byteCount, encoder: null);
        }

        public unsafe static int GetCharCount(Encoding encoding, byte[] bytes, int index, int count)
        {
            Debug.Assert(encoding != null);
            // Validate parameters
            if (bytes == null || index < 0 || count < 0 ||
                (bytes.Length - index < count))
            {
                ThrowValidationFailedException(bytes, index, count);
            }
            Contract.EndContractBlock();

            // If no input just return 0, fixed doesn't like 0 length arrays.
            if (count == 0)
                return 0;

            // Just call pointer version
            fixed (byte* pBytes = bytes)
                return encoding.GetCharCount(pBytes + index, count, decoder: null);
        }

        public unsafe static int GetCharCount(Encoding encoding, byte* bytes, int count)
        {
            Debug.Assert(encoding != null);
            // Validate parameters
            if (bytes == null || count < 0)
            {
                ThrowValidationFailedException(bytes);
            }
            Contract.EndContractBlock();

            return encoding.GetCharCount(bytes, count, decoder: null);
        }

        public unsafe static int GetChars(Encoding encoding, byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
        {
            Debug.Assert(encoding != null);
            // Validate parameters
            if (bytes == null || chars == null || byteIndex < 0 || byteCount < 0 ||
                (bytes.Length - byteIndex < byteCount) ||
                (charIndex < 0 || charIndex > chars.Length))
            {
                ThrowValidationFailedException(bytes, byteIndex, byteCount, chars);
            }
            Contract.EndContractBlock();

            if (byteCount == 0)
                return 0;

            // NOTE: This is the # of chars we can decode,
            // not the size of the array
            int charCount = chars.Length - charIndex;

            // Fixed doesn't like 0 length arrays.
            if (chars.Length == 0)
                chars = new char[1];

            fixed (byte* pBytes = bytes) fixed (char* pChars = &chars[0])
            {
                return encoding.GetChars(pBytes + byteIndex, byteCount, pChars + charIndex, charCount, decoder: null);
            }
        }

        public unsafe static int GetChars(Encoding encoding, byte* bytes, int byteCount, char* chars, int charCount)
        {
            Debug.Assert(encoding != null);
            // Validate parameters
            if (bytes == null || chars == null || charCount < 0 || byteCount < 0)
            {
                ThrowValidationFailedException(bytes, byteCount, chars);
            }
            Contract.EndContractBlock();

            return encoding.GetChars(bytes, byteCount, chars, charCount, decoder: null);
        }

        public unsafe static string GetString(Encoding encoding, byte[] bytes, int index, int count)
        {
            Debug.Assert(encoding != null);
            // Validate parameters
            if (bytes == null || index < 0 ||  count < 0 ||
                (bytes.Length - index < count))
            {
                ThrowValidationFailedException(encoding, bytes, index, count);
            }
            Contract.EndContractBlock();
            
            // Avoid problems with empty input buffer
            if (count == 0)
                return string.Empty;

            // Call string.CreateStringFromEncoding here, which
            // allocates a string and lets the Encoding modify
            // it in place. This way, we don't have to allocate
            // an intermediary char[] to decode into and then
            // call the string constructor; instead we decode
            // directly into the string.

            fixed (byte* pBytes = bytes)
            {
                return string.CreateStringFromEncoding(pBytes + index, count, encoding);
            }
        }

        internal static void ThrowBytesOverflow(Encoding encoding)
        {
            throw GetArgumentException_ThrowBytesOverflow(encoding);
        }

        internal static unsafe void ThrowValidationFailedException(char* chars, int count)
        {
            throw GetValidationFailedException(chars, count);
        }

        internal static void ThrowValidationFailedException(char[] chars, int index, int count)
        {
            throw GetValidationFailedException(chars, index, count);
        }

        internal static void ThrowValidationFailedException(char[] chars, int charIndex, int charCount, byte[] bytes)
        {
            throw GetValidationFailedException(chars, charIndex, charCount, bytes);
        }

        internal static void ThrowValidationFailedException(string s, int index, int count)
        {
            throw GetValidationFailedException(s, index, count);
        }
        
        internal static void ThrowValidationFailedException(Encoding encoding, string s, int charIndex, int charCount, byte[] bytes)
        {
            throw GetValidationFailedException(encoding, s, charIndex, charCount, bytes);
        }

        internal static unsafe void ThrowValidationFailedException(char* chars, int charCount, byte* bytes)
        {
            throw GetValidationFailedException(chars, charCount, bytes);
        }

        private static void ThrowValidationFailedException(Encoding encoding)
        {
            throw GetValidationFailedException(encoding);
        }

        internal static void ThrowValidationFailedException(byte[] bytes, int index, int count)
        {
            throw GetValidationFailedException(bytes, index, count);
        }

        private static void ThrowValidationFailedException(Encoding encoding, byte[] bytes, int index, int count)
        {
            throw GetValidationFailedException(encoding, bytes, index, count);
        }

        internal static void ThrowValidationFailedException(byte[] bytes, int byteIndex, int byteCount, char[] chars)
        {
            throw GetValidationFailedException(bytes, byteIndex, byteCount, chars);
        }

        internal static unsafe void ThrowValidationFailedException(byte* bytes)
        {
            throw GetValidationFailedException(bytes);
        }

        internal static unsafe void ThrowValidationFailedException(byte* bytes, int byteCount, char* chars)
        {
            throw GetValidationFailedException(bytes, byteCount, chars);
        }

        internal static void ThrowValidationFailedException(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex, int byteCount)
        {
            throw GetValidationFailedException(chars, charIndex, charCount, bytes, byteIndex, byteCount);
        }

        internal static unsafe void ThrowValidationFailedException(byte* bytes, int count)
        {
            throw GetValidationFailedException(bytes, count);
        }

        private static ArgumentException GetArgumentException_ThrowBytesOverflow(Encoding encoding)
        {
            throw new ArgumentException(
                Environment.GetResourceString("Argument_EncodingConversionOverflowBytes",
                encoding.EncodingName, encoding.EncoderFallback.GetType()), "bytes");
        }

        private static Exception GetValidationFailedException(Encoding encoding)
        {
            if (encoding is ASCIIEncoding)
                return ThrowHelper.GetArgumentNullException(ExceptionArgument.chars);
            else 
                return ThrowHelper.GetArgumentNullException(ExceptionArgument.s);
        }

        private static unsafe Exception GetValidationFailedException(char* chars, int count)
        {
            if (chars == null)
                return ThrowHelper.GetArgumentNullException(ExceptionArgument.chars, ExceptionResource.ArgumentNull_Array);
            Debug.Assert(count < 0);
            return ThrowHelper.GetArgumentOutOfRangeException(ExceptionArgument.count, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
        }

        private static Exception GetValidationFailedException(char[] chars, int index, int count)
        {
            if (chars == null)
                return ThrowHelper.GetArgumentNullException(ExceptionArgument.chars, ExceptionResource.ArgumentNull_Array);
            if (index < 0)
                return ThrowHelper.GetArgumentOutOfRangeException(ExceptionArgument.index, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
            if (count < 0)
                return ThrowHelper.GetArgumentOutOfRangeException(ExceptionArgument.count, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
            Debug.Assert(chars.Length - index < count);
            return ThrowHelper.GetArgumentOutOfRangeException(ExceptionArgument.chars, ExceptionResource.ArgumentOutOfRange_IndexCountBuffer);
        }

        private static Exception GetValidationFailedException(char[] chars, int charIndex, int charCount, byte[] bytes)
        {
            if (chars == null)
                return ThrowHelper.GetArgumentNullException(ExceptionArgument.chars, ExceptionResource.ArgumentNull_Array);
            if (bytes == null)
                return ThrowHelper.GetArgumentNullException(ExceptionArgument.bytes, ExceptionResource.ArgumentNull_Array);
            if (charIndex < 0)
                return ThrowHelper.GetArgumentOutOfRangeException(ExceptionArgument.charIndex, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
            if (charCount < 0)
                return ThrowHelper.GetArgumentOutOfRangeException(ExceptionArgument.charCount, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
            if (chars.Length - charIndex < charCount)
                return ThrowHelper.GetArgumentOutOfRangeException(ExceptionArgument.chars, ExceptionResource.ArgumentOutOfRange_IndexCountBuffer);
            //if (byteIndex < 0 || byteIndex > bytes.Length)
            return ThrowHelper.GetArgumentOutOfRangeException(ExceptionArgument.byteIndex, ExceptionResource.ArgumentOutOfRange_Index);
        }

        private static Exception GetValidationFailedException(byte[] bytes, int byteIndex, int byteCount, char[] chars)
        {
            if (bytes == null)
                return ThrowHelper.GetArgumentNullException(ExceptionArgument.bytes, ExceptionResource.ArgumentNull_Array);
            if (chars == null)
                return ThrowHelper.GetArgumentNullException(ExceptionArgument.chars, ExceptionResource.ArgumentNull_Array);
            if (byteIndex < 0)
                return ThrowHelper.GetArgumentOutOfRangeException(ExceptionArgument.byteIndex, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
            if (byteCount < 0)
                return ThrowHelper.GetArgumentOutOfRangeException(ExceptionArgument.byteCount, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
            if (bytes.Length - byteIndex < byteCount)
                return ThrowHelper.GetArgumentOutOfRangeException(ExceptionArgument.bytes, ExceptionResource.ArgumentOutOfRange_IndexCountBuffer);
            // if (charIndex < 0 || charIndex > chars.Length);
            return ThrowHelper.GetArgumentOutOfRangeException(ExceptionArgument.charIndex, ExceptionResource.ArgumentOutOfRange_Index);
        }

        private static Exception GetValidationFailedException(string s, int index, int count)
        {
            if (s == null)
                return ThrowHelper.GetArgumentNullException(ExceptionArgument.s, ExceptionResource.ArgumentNull_String);
            if (index < 0)
                return ThrowHelper.GetArgumentOutOfRangeException(ExceptionArgument.index, ExceptionResource.ArgumentOutOfRange_Index);
            if (count < 0)
                return ThrowHelper.GetArgumentOutOfRangeException(ExceptionArgument.count, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
            Debug.Assert(index > s.Length - count);
            return ThrowHelper.GetArgumentOutOfRangeException(ExceptionArgument.index, ExceptionResource.ArgumentOutOfRange_IndexCountBuffer);
        }
        
        private static unsafe Exception GetValidationFailedException(char* chars, int charCount, byte* bytes)
        {
            if (bytes == null)
                return ThrowHelper.GetArgumentNullException(ExceptionArgument.bytes, ExceptionResource.ArgumentNull_Array);
            if (chars == null)
                return ThrowHelper.GetArgumentNullException(ExceptionArgument.chars, ExceptionResource.ArgumentNull_Array);
            if (charCount < 0)
                return ThrowHelper.GetArgumentOutOfRangeException(ExceptionArgument.charCount, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
            // (byteCount < 0)
            return ThrowHelper.GetArgumentOutOfRangeException(ExceptionArgument.byteCount, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
        }

        private static Exception GetValidationFailedException(Encoding encoding, string s, int charIndex, int charCount, byte[] bytes)
        {
            if (s == null)
                return ThrowHelper.GetArgumentNullException(encoding is ASCIIEncoding ? ExceptionArgument.chars : ExceptionArgument.s, ExceptionResource.ArgumentNull_String);
            if (bytes == null)
                return ThrowHelper.GetArgumentNullException(ExceptionArgument.bytes, ExceptionResource.ArgumentNull_Array);
            if (charIndex < 0)
                return ThrowHelper.GetArgumentOutOfRangeException(ExceptionArgument.charIndex, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
            if (charCount < 0)
                return ThrowHelper.GetArgumentOutOfRangeException(ExceptionArgument.charCount, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
            if (s.Length - charIndex < charCount)
                return ThrowHelper.GetArgumentOutOfRangeException(encoding is ASCIIEncoding ? ExceptionArgument.chars : ExceptionArgument.s, ExceptionResource.ArgumentOutOfRange_IndexCountBuffer);
            // (byteIndex < 0 || byteIndex > bytes.Length)
            return ThrowHelper.GetArgumentOutOfRangeException(ExceptionArgument.byteIndex, ExceptionResource.ArgumentOutOfRange_Index);
        }

        private static Exception GetValidationFailedException(byte[] bytes, int index, int count)
        {
            if (bytes == null)
                return ThrowHelper.GetArgumentNullException(ExceptionArgument.bytes, ExceptionResource.ArgumentNull_Array);
            if (index < 0)
                return ThrowHelper.GetArgumentOutOfRangeException(ExceptionArgument.index, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
            if (count < 0)
                return ThrowHelper.GetArgumentOutOfRangeException(ExceptionArgument.count, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
            Debug.Assert(bytes.Length - index < count);
            return ThrowHelper.GetArgumentOutOfRangeException(ExceptionArgument.bytes, ExceptionResource.ArgumentOutOfRange_IndexCountBuffer);
        }

        private static Exception GetValidationFailedException(Encoding encoding, byte[] bytes, int index, int count)
        {
            if (bytes == null)
                return ThrowHelper.GetArgumentNullException(ExceptionArgument.bytes, ExceptionResource.ArgumentNull_Array);
            if (index < 0)
                return ThrowHelper.GetArgumentOutOfRangeException(encoding is ASCIIEncoding ? ExceptionArgument.byteIndex : ExceptionArgument.index, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
            if (count < 0)
                return ThrowHelper.GetArgumentOutOfRangeException(encoding is ASCIIEncoding ? ExceptionArgument.byteCount : ExceptionArgument.count, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
            Debug.Assert(bytes.Length - index < count);
            return ThrowHelper.GetArgumentOutOfRangeException(ExceptionArgument.bytes, ExceptionResource.ArgumentOutOfRange_IndexCountBuffer);
        }

        private static unsafe Exception GetValidationFailedException(byte* bytes)
        {
            if (bytes == null)
                return ThrowHelper.GetArgumentNullException(ExceptionArgument.bytes, ExceptionResource.ArgumentNull_Array);
            // if (count < 0)
            return ThrowHelper.GetArgumentOutOfRangeException(ExceptionArgument.count, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
        }

        private static unsafe Exception GetValidationFailedException(byte* bytes, int byteCount, char* chars)
        {
            if (bytes == null)
                return ThrowHelper.GetArgumentNullException(ExceptionArgument.bytes, ExceptionResource.ArgumentNull_Array);
            if (chars == null)
                return ThrowHelper.GetArgumentNullException(ExceptionArgument.chars, ExceptionResource.ArgumentNull_Array);
            if (byteCount < 0)
                return ThrowHelper.GetArgumentOutOfRangeException(ExceptionArgument.byteCount, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
            // if (charCount < 0)
            return ThrowHelper.GetArgumentOutOfRangeException(ExceptionArgument.charCount, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
        }

        private static Exception GetValidationFailedException(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex, int byteCount)
        {
            if (chars == null)
                return ThrowHelper.GetArgumentNullException(ExceptionArgument.chars, ExceptionResource.ArgumentNull_Array);
            if (bytes == null)
                return ThrowHelper.GetArgumentNullException(ExceptionArgument.bytes, ExceptionResource.ArgumentNull_Array);
            if (charIndex < 0)
                return ThrowHelper.GetArgumentOutOfRangeException(ExceptionArgument.charIndex,
                    ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
            if (charCount < 0)
                return ThrowHelper.GetArgumentOutOfRangeException(ExceptionArgument.charCount,
                    ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
            if (byteIndex < 0)
                return ThrowHelper.GetArgumentOutOfRangeException(ExceptionArgument.byteIndex,
                    ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
            if (byteCount < 0)
                return ThrowHelper.GetArgumentOutOfRangeException(ExceptionArgument.byteCount,
                    ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
            if (chars.Length - charIndex < charCount)
                return ThrowHelper.GetArgumentOutOfRangeException(ExceptionArgument.chars,
                    ExceptionResource.ArgumentOutOfRange_IndexCountBuffer);
            Debug.Assert(bytes.Length - byteIndex < byteCount);
            return ThrowHelper.GetArgumentOutOfRangeException(ExceptionArgument.bytes,
                ExceptionResource.ArgumentOutOfRange_IndexCountBuffer);
        }

        private static unsafe Exception GetValidationFailedException(byte* bytes, int count)
        {
            if (bytes == null)
                return ThrowHelper.GetArgumentNullException(ExceptionArgument.bytes, ExceptionResource.ArgumentNull_Array);
            Debug.Assert(count < 0);
            return ThrowHelper.GetArgumentOutOfRangeException(ExceptionArgument.count, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
        }
    }
}
