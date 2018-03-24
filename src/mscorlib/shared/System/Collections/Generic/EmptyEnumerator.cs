﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Runtime.CompilerServices;
using System.Threading;

namespace System.Collections.Generic
{
    internal class EmptyEnumerator<T> : IEnumerator<T>, IDictionaryEnumerator
    {
        private static EmptyEnumerator<T> s_shared;

        public static EmptyEnumerator<T> Shared
        {
            [MethodImpl(MethodImplOptions.NoInlining)]
            get
            {
                return s_shared ?? InitalizeShared();
            }
        }

        private EmptyEnumerator() { }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static EmptyEnumerator<T> InitalizeShared()
        {
            var newEnumerator = new EmptyEnumerator<T>();
            Interlocked.CompareExchange(ref s_shared, newEnumerator, null); // failure is benign. Someone else set the value.
            return s_shared;
        }

        public T Current => default(T);

        public bool MoveNext() => false;

        public void Dispose() { }

        DictionaryEntry IDictionaryEnumerator.Entry => default(DictionaryEntry);

        object IDictionaryEnumerator.Key => null;

        object IDictionaryEnumerator.Value => null;

        object IEnumerator.Current => null;

        void IEnumerator.Reset() { }
    }
}
