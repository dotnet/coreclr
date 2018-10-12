// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Globalization;

namespace System
{
    [Serializable]
    public abstract class StringSegmentComparer : IComparer<StringSegment>, IEqualityComparer<StringSegment>
    {
        // Unlike StringComparer, we treat StringSegmentComparer as an abstract factory model. Don't allow anybody
        // except for our internal implementations to subclass this type. This allows us to add abstract methods
        // in the future without breaking anybody. It's not a breaking change to make this ctor public in the future
        // if we really need to, but once we do that we'll have difficulty adding functionality to this type.
        // Consumers should accept IEqualityComparer<StringSegment> instead of StringSegmentComparer for maximum agility.
        private StringSegmentComparer()
        {
        }

        // Not a singleton; creates a new instance on each property access.
        public static StringSegmentComparer CurrentCulture => new CultureAwareComparer(CultureInfo.CurrentCulture.CompareInfo, CompareOptions.None);

        // Not a singleton; creates a new instance on each property access.
        public static StringSegmentComparer CurrentCultureIgnoreCase => new CultureAwareComparer(CultureInfo.CurrentCulture.CompareInfo, CompareOptions.IgnoreCase);

        public static StringSegmentComparer InvariantCulture { get; } = new CultureAwareComparer(CultureInfo.InvariantCulture.CompareInfo, CompareOptions.None);

        public static StringSegmentComparer InvariantCultureIgnoreCase { get; } = new CultureAwareComparer(CultureInfo.InvariantCulture.CompareInfo, CompareOptions.None);

        public static StringSegmentComparer Ordinal { get; } = new OrdinalComparer();

        public static StringSegmentComparer OrdinalIgnoreCase { get; } = new OrdinalIgnoreCaseComparer();

        public static StringSegmentComparer Create(CultureInfo culture, bool ignoreCase)
        {
            if (culture == null)
            {
                throw new ArgumentNullException(nameof(culture));
            }

            return new CultureAwareComparer(culture.CompareInfo, ignoreCase ? CompareOptions.IgnoreCase : CompareOptions.None);
        }

        public static StringSegmentComparer Create(CultureInfo culture, CompareOptions options)
        {
            if (culture == null)
            {
                throw new ArgumentException(nameof(culture));
            }

            if ((options & System.CultureAwareComparer.ValidCompareMaskOffFlags) != 0)
            {
                throw new ArgumentException(SR.Argument_InvalidFlag, nameof(options));
            }

            return new CultureAwareComparer(culture.CompareInfo, options);
        }

        public abstract int Compare(StringSegment x, StringSegment y);

        public abstract bool Equals(StringSegment x, StringSegment y);

        // Convert a StringComparison to a StringSegmentComparer
        public static StringSegmentComparer FromComparison(StringComparison comparisonType)
        {
            switch (comparisonType)
            {
                case StringComparison.CurrentCulture:
                    return CurrentCulture;
                case StringComparison.CurrentCultureIgnoreCase:
                    return CurrentCultureIgnoreCase;
                case StringComparison.InvariantCulture:
                    return InvariantCulture;
                case StringComparison.InvariantCultureIgnoreCase:
                    return InvariantCultureIgnoreCase;
                case StringComparison.Ordinal:
                    return Ordinal;
                case StringComparison.OrdinalIgnoreCase:
                    return OrdinalIgnoreCase;
                default:
                    throw new ArgumentException(SR.NotSupported_StringComparison, nameof(comparisonType));
            }
        }

        public abstract int GetHashCode(StringSegment obj);

        [Serializable]
        private sealed class CultureAwareComparer : StringSegmentComparer
        {
            // do not rename instance fields - breaks binary serialization
            private readonly CompareInfo _compareInfo;
            private readonly CompareOptions _options;

            public CultureAwareComparer(CompareInfo compareInfo, CompareOptions options)
            {
                _compareInfo = compareInfo;
                _options = options;
            }

            public override int Compare(StringSegment x, StringSegment y)
            {
                return _compareInfo.Compare(
                    string1: x.GetBuffer(out int offset1, out int length1), offset1: offset1, length1: length1,
                    string2: y.GetBuffer(out int offset2, out int length2), offset2: offset2, length2: length2,
                    options: _options);
            }

            public override bool Equals(StringSegment x, StringSegment y) => Compare(x, y) == 0;

            public override int GetHashCode(StringSegment obj) => _compareInfo.GetHashCodeOfString(obj.AsSpan(), _options);
        }

        /// <summary>
        /// An ordinal case-sensitive <see cref="Utf8StringComparer"/>.
        /// (Really just a char-by-char comparison.)
        /// </summary>
        [Serializable]
        private sealed class OrdinalComparer : StringSegmentComparer
        {
            public override int Compare(StringSegment x, StringSegment y) => string.CompareOrdinal(x.AsSpan(), y.AsSpan());

            public override bool Equals(StringSegment x, StringSegment y) => (x == y);

            public override int GetHashCode(StringSegment obj) => obj.GetHashCode();
        }

        /// <summary>
        /// An ordinal case-insensitive <see cref="Utf8StringComparer"/>.
        /// (This performs simple case folding, then an ordinal case-sensitive comparison.)
        /// </summary>
        [Serializable]
        private sealed class OrdinalIgnoreCaseComparer : StringSegmentComparer
        {
            public override int Compare(StringSegment x, StringSegment y) => CompareInfo.CompareOrdinalIgnoreCase(x.AsSpan(), y.AsSpan());

            public override bool Equals(StringSegment x, StringSegment y) => MemoryExtensions.EqualsOrdinalIgnoreCase(x.AsSpan(), y.AsSpan());

            public override int GetHashCode(StringSegment obj) => string.GetHashCodeOrdinalIgnoreCase(obj.AsSpan());
        }
    }
}
