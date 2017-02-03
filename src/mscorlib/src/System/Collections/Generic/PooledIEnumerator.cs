// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.Collections.Generic
{
    internal class PooledIEnumerator<TValue, TEnumerator> : IEnumerator<TValue> where TEnumerator : IEnumerator<TValue>
    {
        public TEnumerator Enumerator;
        public bool MoveNext() => Enumerator.MoveNext();
        public TValue Current => Enumerator.Current;
        object IEnumerator.Current => Current;

        public void Dispose()
        {
            Enumerator = default(TEnumerator);
            ObjectPool<PooledIEnumerator<TValue, TEnumerator>>.Shared.Return(this);
        }
        public void Reset()
        {
            Enumerator.Reset();
        }
    }
}
