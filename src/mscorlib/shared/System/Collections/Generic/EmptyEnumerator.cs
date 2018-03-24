// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.Collections.Generic
{
    internal class EmptyEnumerator<T> : IEnumerator<T>, IDictionaryEnumerator
    {
        public static EmptyEnumerator<T> Shared { get; } = new EmptyEnumerator<T>();

        private EmptyEnumerator() { }

        public T Current { get; } = default;

        public bool MoveNext() => false;

        public void Dispose() { }

        DictionaryEntry IDictionaryEnumerator.Entry { get; } = default;

        object IDictionaryEnumerator.Key { get; } = null;

        object IDictionaryEnumerator.Value { get; } = null;

        object IEnumerator.Current { get; } = null;

        void IEnumerator.Reset() { }
    }
}
