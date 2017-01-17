// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.Text
{
    using System.Runtime.Serialization;
    using System.Security.Permissions;
    using System.Text;
    using System;
    using System.Diagnostics.Contracts;
    // An Encoder is used to encode a sequence of blocks of characters into
    // a sequence of blocks of bytes. Following instantiation of an encoder,
    // sequential blocks of characters are converted into blocks of bytes through
    // calls to the GetBytes method. The encoder maintains state between the
    // conversions, allowing it to correctly encode character sequences that span
    // adjacent blocks.
    //
    // Instances of specific implementations of the Encoder abstract base
    // class are typically obtained through calls to the GetEncoder method
    // of Encoding objects.
    //

    [Serializable]
    internal class EncoderNLS : Encoder, ISerializable
    {
        // Need a place for the last left over character, most of our encodings use this
        internal char   charLeftOver;
        
        protected Encoding m_encoding;
        
        [NonSerialized] protected   bool     m_mustFlush;
        [NonSerialized] internal    bool     m_throwOnOverflow;
        [NonSerialized] internal    int      m_charsUsed;

#region Serialization

        // Constructor called by serialization. called during deserialization.
        internal EncoderNLS(SerializationInfo info, StreamingContext context)
        {
            throw new NotSupportedException(
                        String.Format(
                            System.Globalization.CultureInfo.CurrentCulture, 
                            Environment.GetResourceString("NotSupported_TypeCannotDeserialized"), this.GetType()));
        }

        // ISerializable implementation. called during serialization.
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            SerializeEncoder(info);
            info.AddValue("encoding", this.m_encoding);
            info.AddValue("charLeftOver", this.charLeftOver);
            info.SetType(typeof(Encoding.DefaultEncoder));
        }

#endregion Serialization 

        internal EncoderNLS(Encoding encoding)
        {
            this.m_encoding = encoding;
            this.m_fallback = this.m_encoding.EncoderFallback;
            this.Reset();
        }

        // This one is used when deserializing (like UTF7Encoding.Encoder)
        internal EncoderNLS()
        {
            this.m_encoding = null;
            this.Reset();
        }

        public override void Reset()
        {
            this.charLeftOver = (char)0;
            if (m_fallbackBuffer != null)
                m_fallbackBuffer.Reset();
        }

        public override unsafe int GetByteCount(char[] chars, int index, int count, bool flush)
        {
            // Validate input parameters
            if (chars == null)
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.chars, ExceptionResource.ArgumentNull_Array);
            if (index < 0)
                ThrowHelper.ThrowIndexArgumentOutOfRange_NeedNonNegNumException();
            if (count < 0)
                ThrowHelper.ThrowCountArgumentOutOfRange_NeedNonNegNumException();
            if (chars.Length - index < count)
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.chars, ExceptionResource.ArgumentOutOfRange_IndexCountBuffer);
            Contract.EndContractBlock();

            // Avoid empty input problem
            if (chars.Length == 0)
                chars = new char[1];

            this.m_mustFlush = flush;
            this.m_throwOnOverflow = true;
            // Just call the pointer version
            int result = -1;
            fixed (char* pChars = &chars[0])
            {
                result = m_encoding.GetByteCount(pChars + index, count, this);
            }
            return result;
        }

        public unsafe override int GetByteCount(char* chars, int count, bool flush)
        {
            // Validate input parameters
            if (chars == null)
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.chars, ExceptionResource.ArgumentNull_Array);
            if (count < 0)
                ThrowHelper.ThrowCountArgumentOutOfRange_NeedNonNegNumException();
            Contract.EndContractBlock();

            this.m_mustFlush = flush;
            this.m_throwOnOverflow = true;
            return m_encoding.GetByteCount(chars, count, this);
        }

        public override unsafe int GetBytes(char[] chars, int charIndex, int charCount,
                                              byte[] bytes, int byteIndex, bool flush)
        {
            // Validate parameters
            if (chars == null)
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.chars, ExceptionResource.ArgumentNull_Array);
            if (bytes == null)
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.bytes, ExceptionResource.ArgumentNull_Array);
            if (charIndex < 0)
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.charIndex, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
            if (charCount < 0)
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.charCount, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
            if (chars.Length - charIndex < charCount)
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.chars, ExceptionResource.ArgumentOutOfRange_IndexCountBuffer);
            if (byteIndex < 0 || byteIndex > bytes.Length)
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.byteIndex, ExceptionResource.ArgumentOutOfRange_Index);
            Contract.EndContractBlock();

            if (chars.Length == 0)
                chars = new char[1];

            int byteCount = bytes.Length - byteIndex;
            if (bytes.Length == 0)
                bytes = new byte[1];

            this.m_mustFlush = flush;
            this.m_throwOnOverflow = true;

            // Just call pointer version
            fixed (char* pChars = &chars[0])
                fixed (byte* pBytes = &bytes[0])

                    // Remember that charCount is # to decode, not size of array.
                    return m_encoding.GetBytes(pChars + charIndex, charCount,
                                    pBytes + byteIndex, byteCount, this);

        }

        public unsafe override int GetBytes(char* chars, int charCount, byte* bytes, int byteCount, bool flush)
        {
            // Validate parameters
            if (chars == null)
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.chars, ExceptionResource.ArgumentNull_Array);
            if (bytes == null)
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.bytes, ExceptionResource.ArgumentNull_Array);
            if (byteCount < 0)
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.byteCount, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
            if (charCount < 0)
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.charCount, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
            Contract.EndContractBlock();

            this.m_mustFlush = flush;
            this.m_throwOnOverflow = true;
            return m_encoding.GetBytes(chars, charCount, bytes, byteCount, this);
        }

        // This method is used when your output buffer might not be large enough for the entire result.
        // Just call the pointer version.  (This gets bytes)
        public override unsafe void Convert(char[] chars, int charIndex, int charCount,
                                              byte[] bytes, int byteIndex, int byteCount, bool flush,
                                              out int charsUsed, out int bytesUsed, out bool completed)
        {
            // Validate parameters
            if (chars == null)
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.chars, ExceptionResource.ArgumentNull_Array);
            if (bytes == null)
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.bytes, ExceptionResource.ArgumentNull_Array);
            if (charIndex < 0)
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.charIndex, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
            if (charCount < 0)
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.charCount, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
            if (byteIndex < 0)
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.byteIndex, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
            if (byteCount < 0)
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.byteCount, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
            if (chars.Length - charIndex < charCount)
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.chars, ExceptionResource.ArgumentOutOfRange_IndexCountBuffer);
            if (bytes.Length - byteIndex < byteCount)
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.bytes, ExceptionResource.ArgumentOutOfRange_IndexCountBuffer);
            Contract.EndContractBlock();

            // We don't want to throw
            this.m_mustFlush = flush;
            this.m_throwOnOverflow = false;
            this.m_charsUsed = 0;

            // Avoid empty input problem
            if (chars.Length == 0)
                chars = new char[1];
            if (bytes.Length == 0)
                bytes = new byte[1];

            // Just call the pointer version (can't do this for non-msft encoders)
            fixed (char* pChars = &chars[0])
            {
                fixed (byte* pBytes = &bytes[0])
                {
                    bytesUsed = this.m_encoding.GetBytes(pChars + charIndex, charCount, pBytes + byteIndex, byteCount, this);
                }
            }

            charsUsed = this.m_charsUsed;

            // Its completed if they've used what they wanted AND if they didn't want flush or if we are flushed
            completed = (charsUsed == charCount) && (!flush || !this.HasState) &&
                (m_fallbackBuffer == null || m_fallbackBuffer.Remaining == 0);
        }

        // This is the version that uses pointers.  We call the base encoding worker function
        // after setting our appropriate internal variables.  This is getting bytes
        public override unsafe void Convert(char* chars, int charCount,
                                              byte* bytes, int byteCount, bool flush,
                                              out int charsUsed, out int bytesUsed, out bool completed)
        {
            // Validate input parameters
            if (chars == null)
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.chars, ExceptionResource.ArgumentNull_Array);
            if (bytes == null)
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.bytes, ExceptionResource.ArgumentNull_Array);
            if (charCount < 0)
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.charCount, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
            if (byteCount < 0)
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.byteCount, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
            Contract.EndContractBlock();

            // We don't want to throw
            this.m_mustFlush = flush;
            this.m_throwOnOverflow = false;
            this.m_charsUsed = 0;

            // Do conversion
            bytesUsed = this.m_encoding.GetBytes(chars, charCount, bytes, byteCount, this);
            charsUsed = this.m_charsUsed;

            // Its completed if they've used what they wanted AND if they didn't want flush or if we are flushed
            completed = (charsUsed == charCount) && (!flush || !this.HasState) &&
                (m_fallbackBuffer == null || m_fallbackBuffer.Remaining == 0);

            // Our data thingys are now full, we can return
        }

        public Encoding Encoding
        {
            get
            {
                return m_encoding;
            }
        }

        public bool MustFlush
        {
            get
            {
                return m_mustFlush;
            }
        }


        // Anything left in our encoder?
        internal virtual bool HasState
        {
            get
            {
                return (this.charLeftOver != (char)0);
            }
        }

        // Allow encoding to clear our must flush instead of throwing (in ThrowBytesOverflow)
        internal void ClearMustFlush()
        {
            m_mustFlush = false;
        }

    }
}
