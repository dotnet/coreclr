﻿using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace System.Buffers
{
    internal ref struct ValueListBuilder<T>
    {
        private Span<T> _span;
        private T[] _arrayFromPool;
        private int _pos;

        public ValueListBuilder(Span<T> initialSpan)
        {
            _span = initialSpan;
            _arrayFromPool = null;
            _pos = 0;
        }

        public int Length => _pos;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Append(T item)
        {
            int pos = _pos;
            if (pos < _span.Length)
            {
                _span[pos] = item;
                _pos = pos + 1;
            }
            else
            {
                Grow();
                Append(item);
            }
        }

        public ReadOnlySpan<T> AsReadOnlySpan()
        {
            return _span.Slice(0, _pos);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            if (_arrayFromPool != null)
            {
                ArrayPool<T>.Shared.Return(_arrayFromPool);
            }
        }

        private void Grow()
        {
            T[] array = ArrayPool<T>.Shared.Rent(_span.Length * 2);

            bool success = _span.TryCopyTo(array);
            Debug.Assert(success);

            T[] toReturn = _arrayFromPool;
            _span = _arrayFromPool = array;
            if (toReturn != null)
            {
                ArrayPool<T>.Shared.Return(toReturn);
            }
        }
    }
}
