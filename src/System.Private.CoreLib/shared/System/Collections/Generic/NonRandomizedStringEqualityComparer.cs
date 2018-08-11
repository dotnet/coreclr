// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Globalization;
using System.Runtime.Serialization;

namespace System.Collections.Generic
{
    // NonRandomizedStringEqualityComparer is the comparer used by default with the Dictionary<string,...> 
    // We use NonRandomizedStringEqualityComparer as default comparer as it doesnt use the randomized string hashing which 
    // keeps the performance not affected till we hit collision threshold and then we switch to the comparer which is using 
    // randomized string hashing.
    [Serializable] // Required for compatibility with .NET Core 2.0 as we exposed the NonRandomizedStringEqualityComparer inside the serialization blob
    // Needs to be public to support binary serialization compatibility
    public sealed class NonRandomizedStringEqualityComparer : EqualityComparer<string>, ISerializable
    {
        internal static new IEqualityComparer<string> Default { get; } = new NonRandomizedStringEqualityComparer();

        private NonRandomizedStringEqualityComparer() { }

        // This is used by the serialization engine.
        private NonRandomizedStringEqualityComparer(SerializationInfo information, StreamingContext context) { }

        public sealed override bool Equals(string x, string y) => string.Equals(x, y);

        public sealed override int GetHashCode(string obj) => obj?.GetNonRandomizedHashCode() ?? 0;

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            // We are doing this to stay compatible with .NET Framework.
            info.SetType(typeof(GenericEqualityComparer<string>));
        }
    }

    [Serializable]
    internal sealed class NonRandomizedStringComparer : IEqualityComparer<string>, IComparer<string>, ISerializable
    {
        internal static IEqualityComparer<string> Default { get; } = new NonRandomizedStringComparer();

        private NonRandomizedStringComparer() { }

        public bool Equals(string x, string y) => string.Equals(x, y);

        public int GetHashCode(string obj) => obj?.GetNonRandomizedHashCode() ?? 0;

        public int Compare(string x, string y)
        {
            if (ReferenceEquals(x, y))
                return 0;
            if (x == null)
                return -1;
            if (y == null)
                return 1;

            return string.CompareOrdinal(x, y);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.SetType(typeof(OrdinalComparer));
            info.AddValue("_ignoreCase", false); // Do not rename (binary serialization)
        }
    }

    [Serializable]
    internal sealed class NonRandomizedIgnoreCaseStringEqualityComparer : IEqualityComparer<string>, ISerializable
    {
        internal static IEqualityComparer<string> Default { get; } = new NonRandomizedIgnoreCaseStringEqualityComparer();

        private NonRandomizedIgnoreCaseStringEqualityComparer() { }

        public bool Equals(string x, string y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }

            if (x is null || y is null || x.Length != y.Length)
            {
                return false;
            }

            return CompareInfo.EqualsOrdinalIgnoreCase(ref x.GetRawStringData(), ref y.GetRawStringData(), x.Length);
        }

        public int GetHashCode(string obj) => obj?.GetNonRandomizedIgnoreCaseHashCode() ?? 0;

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.SetType(typeof(OrdinalComparer));
            info.AddValue("_ignoreCase", true); // Do not rename (binary serialization)
        }
    }
} 
