// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

/*============================================================
**
**
**
** Purpose: 
**
**
===========================================================*/
namespace System {

    using System.Diagnostics.Contracts;
    using System.Text;
    using CultureInfo = System.Globalization.CultureInfo;
    using NumberStyles = System.Globalization.NumberStyles;

    // A Version object contains four hierarchical numeric components: major, minor,
    // build and revision.  Build and revision may be unspecified, which is represented 
    // internally as a -1.  By definition, an unspecified component matches anything 
    // (both unspecified and specified), and an unspecified component is "less than" any
    // specified component.

    [Serializable]
[System.Runtime.InteropServices.ComVisible(true)]
    public sealed class Version : ICloneable, IComparable
        , IComparable<Version>, IEquatable<Version>
    {
        // AssemblyName depends on the order staying the same
        private readonly int _Major;
        private readonly int _Minor;
        private readonly int _Build = -1;
        private readonly int _Revision = -1;
        private static readonly char[] SeparatorsArray = new char[] { '.' };
    
        public Version(int major, int minor, int build, int revision) {
            if (major < 0) 
              throw new ArgumentOutOfRangeException("major",Environment.GetResourceString("ArgumentOutOfRange_Version"));

            if (minor < 0) 
              throw new ArgumentOutOfRangeException("minor",Environment.GetResourceString("ArgumentOutOfRange_Version"));

            if (build < 0)
              throw new ArgumentOutOfRangeException("build",Environment.GetResourceString("ArgumentOutOfRange_Version"));
            
            if (revision < 0) 
              throw new ArgumentOutOfRangeException("revision",Environment.GetResourceString("ArgumentOutOfRange_Version"));
            Contract.EndContractBlock();
            
            _Major = major;
            _Minor = minor;
            _Build = build;
            _Revision = revision;
        }

        public Version(int major, int minor, int build) {
            if (major < 0) 
                throw new ArgumentOutOfRangeException("major",Environment.GetResourceString("ArgumentOutOfRange_Version"));

            if (minor < 0) 
              throw new ArgumentOutOfRangeException("minor",Environment.GetResourceString("ArgumentOutOfRange_Version"));

            if (build < 0) 
              throw new ArgumentOutOfRangeException("build",Environment.GetResourceString("ArgumentOutOfRange_Version"));

            Contract.EndContractBlock();
            
            _Major = major;
            _Minor = minor;
            _Build = build;
        }
    
        public Version(int major, int minor) {
            if (major < 0) 
                throw new ArgumentOutOfRangeException("major",Environment.GetResourceString("ArgumentOutOfRange_Version"));

            if (minor < 0) 
                throw new ArgumentOutOfRangeException("minor",Environment.GetResourceString("ArgumentOutOfRange_Version"));
            Contract.EndContractBlock();
            
            _Major = major;
            _Minor = minor;
        }

        public Version(String version) {
            Version v = Version.Parse(version);
            _Major = v.Major;
            _Minor = v.Minor;
            _Build = v.Build;
            _Revision = v.Revision;
        }

        public Version() 
        {
            _Major = 0;
            _Minor = 0;
        }

        private Version(Version version)
        {
            Contract.Assert(version != null);

            _Major = version._Major;
            _Minor = version._Minor;
            _Build = version._Build;
            _Revision = version._Revision;
        }

        // Properties for setting and getting version numbers
        public int Major {
            get { return _Major; }
        }
    
        public int Minor {
            get { return _Minor; }
        }
    
        public int Build {
            get { return _Build; }
        }
    
        public int Revision {
            get { return _Revision; }
        }

        public short MajorRevision {
            get { return (short)(_Revision >> 16); }
        }

        public short MinorRevision {
            get { return (short)(_Revision & 0xFFFF); }
        }
     
        public Object Clone() {
            return new Version(this);
        }

        public int CompareTo(Object version)
        {
            if (version == null)
            {
                return 1;
            }

            Version v = version as Version;
            if (v == null)
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_MustBeVersion"));
            }

            return CompareTo(v);
        }

        public int CompareTo(Version value)
        {
            return
                object.ReferenceEquals(value, this) ? 0 :
                object.ReferenceEquals(value, null) ? 1 :
                _Major != value._Major ? (_Major > value._Major ? 1 : -1) :
                _Minor != value._Minor ? (_Minor > value._Minor ? 1 : -1) :
                _Build != value._Build ? (_Build > value._Build ? 1 : -1) :
                _Revision != value._Revision ? (_Revision > value._Revision ? 1 : -1) :
                0;
        }

        public override bool Equals(Object obj) {
            return Equals(obj as Version);
        }

        public bool Equals(Version obj)
        {
            return object.ReferenceEquals(obj, this) ||
                (!object.ReferenceEquals(obj, null) &&
                _Major == obj._Major &&
                _Minor == obj._Minor &&
                _Build == obj._Build &&
                _Revision == obj._Revision);
        }

        public override int GetHashCode()
        {
            // Let's assume that most version numbers will be pretty small and just
            // OR some lower order bits together.

            int accumulator = 0;

            accumulator |= (this._Major & 0x0000000F) << 28;
            accumulator |= (this._Minor & 0x000000FF) << 20;
            accumulator |= (this._Build & 0x000000FF) << 12;
            accumulator |= (this._Revision & 0x00000FFF);

            return accumulator;
        }

        public override String ToString() {
            if (_Build == -1) return(ToString(2));
            if (_Revision == -1) return(ToString(3));
            return(ToString(4));
        }
        
        public String ToString(int fieldCount) {
            StringBuilder sb;
            switch (fieldCount) {
            case 0: 
                return(String.Empty);
            case 1: 
                return(_Major.ToString());
            case 2:
                sb = StringBuilderCache.Acquire();
                AppendPositiveNumber(_Major, sb);
                sb.Append('.');
                AppendPositiveNumber(_Minor, sb);
                return StringBuilderCache.GetStringAndRelease(sb);
            default:
                if (_Build == -1)
                    throw new ArgumentException(Environment.GetResourceString("ArgumentOutOfRange_Bounds_Lower_Upper", "0", "2"), "fieldCount");

                if (fieldCount == 3)
                {
                    sb = StringBuilderCache.Acquire();
                    AppendPositiveNumber(_Major, sb);
                    sb.Append('.');
                    AppendPositiveNumber(_Minor, sb);
                    sb.Append('.');
                    AppendPositiveNumber(_Build, sb);
                    return StringBuilderCache.GetStringAndRelease(sb);
                }

                if (_Revision == -1)
                    throw new ArgumentException(Environment.GetResourceString("ArgumentOutOfRange_Bounds_Lower_Upper", "0", "3"), "fieldCount");

                if (fieldCount == 4)
                {
                    sb = StringBuilderCache.Acquire();
                    AppendPositiveNumber(_Major, sb);
                    sb.Append('.');
                    AppendPositiveNumber(_Minor, sb);
                    sb.Append('.');
                    AppendPositiveNumber(_Build, sb);
                    sb.Append('.');
                    AppendPositiveNumber(_Revision, sb);
                    return StringBuilderCache.GetStringAndRelease(sb);
                }

                throw new ArgumentException(Environment.GetResourceString("ArgumentOutOfRange_Bounds_Lower_Upper", "0", "4"), "fieldCount");
            }
        }

        //
        // AppendPositiveNumber is an optimization to append a number to a StringBuilder object without
        // doing any boxing and not even creating intermediate string.
        // Note: as we always have positive numbers then it is safe to convert the number to string 
        // regardless of the current culture as we’ll not have any punctuation marks in the number
        //
        private const int ZERO_CHAR_VALUE = (int) '0';
        private static void AppendPositiveNumber(int num, StringBuilder sb)
        {
            Contract.Assert(num >= 0, "AppendPositiveNumber expect positive numbers");

            int index = sb.Length;
            int reminder;

            do
            {
                reminder = num % 10;
                num = num / 10;
                sb.Insert(index, (char)(ZERO_CHAR_VALUE + reminder));
            } while (num > 0);
        }

        public static Version Parse(string input) {
            if (input == null) {
                throw new ArgumentNullException("input");
            }
            Contract.EndContractBlock();

            VersionResult r = new VersionResult();
            r.Init("input", true);
            if (!TryParseVersion(input, ref r)) {
                throw r.GetVersionParseException();
            }
            return r.m_parsedVersion;
        }

        public static bool TryParse(string input, out Version result) {
            VersionResult r = new VersionResult();
            r.Init("input", false);
            bool b = TryParseVersion(input, ref r);
            result = r.m_parsedVersion;
            return b;
        }
  
        private static bool TryParseVersion(string version, ref VersionResult result) {
            if ((Object)version == null) {
                result.SetFailure(ParseFailureKind.ArgumentNullException);
                return false;
            }

            int componentsCount = 1;
            for (int i = 0; i < version.Length; i++) {
                if (version[i] == '.') {
                    componentsCount++;
                }
            }

            if (componentsCount < 2 || componentsCount > 4) {
                result.SetFailure(ParseFailureKind.ArgumentException);
                return false;
            }

            result.m_parsedVersion = new Version();
            int numberStart = 0;
            componentsCount = 0;
            for (int i = 0; i < version.Length; i++) {
                if (version[i] == '.' || i == version.Length - 1) {
                    componentsCount++;

                    int numberEnd = i == version.Length - 1 ? i : i - 1;
                    if (componentsCount == 1) {
                        if(!TryParseComponent(version, numberStart, numberEnd, "major", ref result, out result.m_parsedVersion._Major)) {
                        	result.m_parsedVersion = null;
                            return false;
                        }
                    }
                    else if (componentsCount == 2) {
                        if(!TryParseComponent(version, numberStart, numberEnd, "minor", ref result, out result.m_parsedVersion._Minor)) {
                        	result.m_parsedVersion = null;
                            return false;
                        }
                    }
                    else if (componentsCount == 3) {
                        if(!TryParseComponent(version, numberStart, numberEnd, "build", ref result, out result.m_parsedVersion._Build)) {
                        	result.m_parsedVersion = null;
                            return false;
                        }
                    }
                    else if (componentsCount == 4) {
                        if(!TryParseComponent(version, numberStart, numberEnd, "revision", ref result, out result.m_parsedVersion._Revision)) {
                        	result.m_parsedVersion = null;
                            return false;
                        }
                    }
                    else {
                        break;
                    }

                    numberStart = i + 1;
                }
            }

            return true;
        }

        private static bool TryParseComponent(string version, int start, int end, string componentName, ref VersionResult result, out int parsedComponent) {
            parsedComponent = 0;
            // Consume any leading or trailing whitespace
            while (start <= end && char.IsWhiteSpace(version[start]))
                start++;
            while (end >= start && char.IsWhiteSpace(version[end]))
                end--;


            int firstChar = version[start];
            if (firstChar == '-') {
                // Don't allow negative integers
                result.SetFailure(ParseFailureKind.ArgumentOutOfRangeException, componentName);
                return false;
            } else if (firstChar  == '+') {
            	start++;
            }

            if (end - start < 0) {
                // Empty string (or all whitespace)
                result.SetFailure(ParseFailureKind.FormatException, string.Empty));
                return false;
            }
            
            for (int i = start; i <= end; i++) {
                int digitValue = 0;
                char c = version[i];
                if (c >= '0' && c <= '9') {
                    digitValue = c - '0';
                }
                else {
                    result.SetFailure(ParseFailureKind.FormatException, version.Substring(start, end - start + 1));
                    return false;
                }

                int previousResult = parsedComponent;
                parsedComponent = (parsedComponent * 10) + digitValue;
                if (parsedComponent < previousResult) {
                    // Overflow occured (>int.MaxValue)
                    result.SetFailure(ParseFailureKind.FormatException, version.Substring(start, end - start + 1));
                    return false;
                }
            }
            return true;
        }

        public static bool operator ==(Version v1, Version v2) {
            if (Object.ReferenceEquals(v1, null)) {
                return Object.ReferenceEquals(v2, null);
            }

            return v1.Equals(v2);
        }

        public static bool operator !=(Version v1, Version v2) {
            return !(v1 == v2);
        }

        public static bool operator <(Version v1, Version v2) {
            if ((Object) v1 == null)
                throw new ArgumentNullException("v1");
            Contract.EndContractBlock();
            return (v1.CompareTo(v2) < 0);
        }
        
        public static bool operator <=(Version v1, Version v2) {
            if ((Object) v1 == null)
                throw new ArgumentNullException("v1");
            Contract.EndContractBlock();
            return (v1.CompareTo(v2) <= 0);
        }
        
        public static bool operator >(Version v1, Version v2) {
            return (v2 < v1);
        }
        
        public static bool operator >=(Version v1, Version v2) {
            return (v2 <= v1);
        }

        internal enum ParseFailureKind { 
            ArgumentNullException, 
            ArgumentException, 
            ArgumentOutOfRangeException, 
            FormatException 
        }

        internal struct VersionResult {
            internal Version m_parsedVersion;
            internal ParseFailureKind m_failure;
            internal string m_exceptionArgument;
            internal string m_argumentName;
            internal bool m_canThrow;

            internal void Init(string argumentName, bool canThrow) {
                m_canThrow = canThrow;
                m_argumentName = argumentName;
            }

            internal void SetFailure(ParseFailureKind failure) {
                SetFailure(failure, String.Empty);
            }

            internal void SetFailure(ParseFailureKind failure, string argument) {
                m_failure = failure;
                m_exceptionArgument = argument;
                if (m_canThrow) {
                    throw GetVersionParseException();
                }
            }

            internal Exception GetVersionParseException() {
                switch (m_failure) {
                    case ParseFailureKind.ArgumentNullException:
                        return new ArgumentNullException(m_argumentName);
                    case ParseFailureKind.ArgumentException:
                        return new ArgumentException(Environment.GetResourceString("Arg_VersionString"));
                    case ParseFailureKind.ArgumentOutOfRangeException:
                        return new ArgumentOutOfRangeException(m_exceptionArgument, Environment.GetResourceString("ArgumentOutOfRange_Version"));
                    case ParseFailureKind.FormatException:
                        // Regenerate the FormatException as would be thrown by Int32.Parse()
                        try {
                            Int32.Parse(m_exceptionArgument, CultureInfo.InvariantCulture);
                        } catch (FormatException e) {
                            return e;
                        } catch (OverflowException e) {
                            return e;
                        }
                        Contract.Assert(false, "Int32.Parse() did not throw exception but TryParse failed: " + m_exceptionArgument);
                        return new FormatException(Environment.GetResourceString("Format_InvalidString"));
                    default:
                        Contract.Assert(false, "Unmatched case in Version.GetVersionParseException() for value: " + m_failure);
                        return new ArgumentException(Environment.GetResourceString("Arg_VersionString"));
                }
            }
 
        }
    }
}
