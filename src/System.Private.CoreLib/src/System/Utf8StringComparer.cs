// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Globalization;

namespace System
{
    [Serializable]
    public abstract class Utf8StringComparer : IEqualityComparer<Utf8String>
    {
        // Unlike StringComparer, we treat Utf8StringComparer as an abstract factory model. Don't allow anybody
        // except for our internal implementations to subclass this type. This allows us to add abstract methods
        // in the future without breaking anybody. It's not a breaking change to make this ctor public in the future
        // if we really need to, but once we do that we'll have difficulty adding functionality to this type.
        // Consumers should accept IEqualityComparer<Utf8String> instead of Utf8StringComparer for maximum agility.
        private Utf8StringComparer()
        {
        }

        // Not a singleton; creates a new instance on each property access.
        public static Utf8StringComparer CurrentCulture => new CultureAwareComparer(CultureInfo.CurrentCulture.CompareInfo, CompareOptions.None);

        // Not a singleton; creates a new instance on each property access.
        public static Utf8StringComparer CurrentCultureIgnoreCase => new CultureAwareComparer(CultureInfo.CurrentCulture.CompareInfo, CompareOptions.IgnoreCase);

        public static Utf8StringComparer InvariantCulture { get; } = new CultureAwareComparer(CultureInfo.InvariantCulture.CompareInfo, CompareOptions.None);

        public static Utf8StringComparer InvariantCultureIgnoreCase { get; } = new CultureAwareComparer(CultureInfo.InvariantCulture.CompareInfo, CompareOptions.None);

        public static Utf8StringComparer Ordinal { get; } = new OrdinalComparer();

        public static Utf8StringComparer OrdinalIgnoreCase { get; } = new OrdinalIgnoreCaseComparer();

        public static Utf8StringComparer Create(CultureInfo culture, bool ignoreCase)
        {
            if (culture == null)
            {
                throw new ArgumentNullException(nameof(culture));
            }

            return new CultureAwareComparer(culture.CompareInfo, ignoreCase ? CompareOptions.IgnoreCase : CompareOptions.None);
        }

        public static Utf8StringComparer Create(CultureInfo culture, CompareOptions options)
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

        public abstract bool Equals(Utf8String x, Utf8String y);

        // Convert a StringComparison to a Utf8StringComparer
        public static Utf8StringComparer FromComparison(StringComparison comparisonType)
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

        public abstract int GetHashCode(Utf8String obj);

        [Serializable]
        private sealed class CultureAwareComparer : Utf8StringComparer
        {
            // do not rename instance fields - breaks binary serialization
            private readonly CompareInfo _compareInfo;
            private readonly CompareOptions _options;

            public CultureAwareComparer(CompareInfo compareInfo, CompareOptions options)
            {
                _compareInfo = compareInfo;
                _options = options;
            }

            public override bool Equals(Utf8String x, Utf8String y)
            {
                // The default implementation is to transcode parameters into UTF-16 and to
                // call the culture-aware Equals(...) method.

                if (ReferenceEquals(x, y))
                {
                    return true;
                }

                if (ReferenceEquals(x, null) || ReferenceEquals(y, null))
                {
                    return false;
                }

                // Note the call below to the corruption-preserving conversion routine.
                // This prevents distinct invalid UTF-8 strings from being normalized to the
                // same UTF-16 string (with U+FFFD replacement) and ending up being equal.

                // TODO: Make the conversion allocation-free.

                return _compareInfo.Compare(x.ConvertToUtf16PreservingCorruption(), y.ConvertToUtf16PreservingCorruption(), _options) == 0;
            }

            public override int GetHashCode(Utf8String obj)
            {
                // The default implementation is to transcode parameters into UTF-16 and to
                // call the culture-aware GetHashCode(...) method.

                if (obj == null)
                {
                    ThrowHelper.ThrowArgumentNullException(ExceptionArgument.obj);
                }

                // Note the call below to the corruption-preserving conversion routine.
                // This prevents distinct invalid UTF-8 strings from being normalized to the
                // same UTF-16 string (with U+FFFD replacement) and ending up with the same hash code.

                // TODO: Make the conversion allocation-free.

                return _compareInfo.GetHashCodeOfString(obj.ConvertToUtf16PreservingCorruption(), _options);
            }
        }

        /// <summary>
        /// An ordinal case-sensitive <see cref="Utf8StringComparer"/>.
        /// (Really just a byte-by-byte comparison.)
        /// </summary>
        [Serializable]
        private sealed class OrdinalComparer : Utf8StringComparer
        {
            public override bool Equals(Utf8String x, Utf8String y) => Utf8String.Equals(x, y);

            public override int GetHashCode(Utf8String obj)
            {
                if (obj == null)
                {
                    ThrowHelper.ThrowArgumentNullException(ExceptionArgument.obj);
                }

                return obj.GetHashCode();
            }
        }

        /// <summary>
        /// An ordinal case-insensitive <see cref="Utf8StringComparer"/>.
        /// (This performs simple case folding, then an ordinal case-sensitive comparison.)
        /// </summary>
        [Serializable]
        private sealed class OrdinalIgnoreCaseComparer : Utf8StringComparer
        {
            public override bool Equals(Utf8String x, Utf8String y) => Utf8String.EqualsOrdinalIgnoreCase(x, y);

            public override int GetHashCode(Utf8String obj)
            {
                if (obj == null)
                {
                    ThrowHelper.ThrowArgumentNullException(ExceptionArgument.obj);
                }

                return obj.GetHashCode(StringComparison.OrdinalIgnoreCase);
            }
        }
    }
}
