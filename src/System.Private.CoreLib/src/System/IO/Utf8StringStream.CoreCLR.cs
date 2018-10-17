// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace System.IO
{
    internal sealed class Utf8StringStream : Stream
    {
        private readonly Utf8String _string;
        private int _position;

        internal Utf8StringStream(Utf8String value)
        {
            Debug.Assert(!(value is null));

            _string = value;
            _position = 0;
        }

        public override bool CanRead => true;

        public override bool CanSeek => true;

        public override bool CanWrite => true;

        public override long Length => _string.Length;

        public override long Position
        {
            get => _position;
            set => Seek(value, SeekOrigin.Begin);
        }

        public override void CopyTo(Stream destination, int bufferSize)
        {
            if (destination is null)
            {
                throw new ArgumentNullException(nameof(destination));
            }

            destination.Write(_string.AsSpanFast());
        }

        public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
        {
            if (destination is null)
            {
                throw new ArgumentNullException(nameof(destination));
            }

            return destination.WriteAsync(_string.AsMemoryBytes(), cancellationToken).AsTask();
        }

        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (buffer is null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            return Read(new Span<byte>(buffer, offset, count));
        }

        public override int Read(Span<byte> buffer)
        {
            ReadOnlySpan<byte> remainingBytes = _string.AsSpanFast().Slice(_position);

            int bytesToCopy = Math.Min(remainingBytes.Length, buffer.Length);
            remainingBytes.Slice(0, bytesToCopy).CopyTo(buffer);

            _position += bytesToCopy;
            return bytesToCopy;
        }

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            if (buffer is null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            return Task.FromResult(Read(new Span<byte>(buffer, offset, count)));
        }

        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            return new ValueTask<int>(Read(buffer.Span));
        }

        public override int ReadByte()
        {
            if (_position >= _string.Length)
            {
                return -1;
            }

            return _string.AsSpanFast()[_position++];
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            long newPosition;

            switch (origin)
            {
                case SeekOrigin.Begin:
                    {
                        newPosition = offset;
                        break;
                    }
                case SeekOrigin.Current:
                    {
                        newPosition = _position + offset;
                        break;
                    }
                case SeekOrigin.End:
                    {
                        newPosition = offset + _string.Length;
                        break;
                    }
                default:
                    throw new ArgumentException(SR.Argument_InvalidSeekOrigin);
            }

            if ((ulong)newPosition > (uint)_string.Length)
            {
                throw new IOException(SR.IO_SeekBeforeBegin);
            }

            Debug.Assert(0 <= newPosition && newPosition <= Int32.MaxValue);
            _position = (int)newPosition;
            return newPosition;
        }

        public override void SetLength(long value)
        {
            throw Error.GetWriteNotSupported();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw Error.GetWriteNotSupported();
        }

        public override void Write(ReadOnlySpan<byte> buffer)
        {
            throw Error.GetWriteNotSupported();
        }

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            throw Error.GetWriteNotSupported();
        }

        public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            throw Error.GetWriteNotSupported();
        }

        public override void WriteByte(byte value)
        {
            throw Error.GetWriteNotSupported();
        }
    }
}
