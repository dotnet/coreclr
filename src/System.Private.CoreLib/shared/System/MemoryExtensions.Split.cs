// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;

namespace System
{
    public static partial class MemoryExtensions
    {
        public static SpanSplitEnumerator<T> Split<T>(this ReadOnlySpan<T> span, T separator,  StringSplitOptions options = StringSplitOptions.None)
            where T : IEquatable<T>
        {
        }

        public ref struct SpanSplitEnumerator<T> where T : IEquatable<T>
        {
            public SpanSplitEnumerator<T> GetEnumerator() { return this; }

            public bool MoveNext();

            public Range Current { get; }
        }
    }
}
