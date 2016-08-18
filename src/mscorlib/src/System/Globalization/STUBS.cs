namespace System.Globalization
{
    public partial class CompareInfo : System.Runtime.Serialization.IDeserializationCallback
    {
        public int LCID { get { throw null; } }
        public static System.Globalization.CompareInfo GetCompareInfo(int culture) { throw null; }
        public static System.Globalization.CompareInfo GetCompareInfo(int culture, System.Reflection.Assembly assembly) { throw null; }
    }

    public partial class CultureInfo : System.ICloneable, System.IFormatProvider
    {
        public CultureInfo(int culture) { throw null; }
        public CultureInfo(int culture, bool useUserOverride) { throw null; }
        public virtual int LCID { get { throw null; } }
        public virtual string ThreeLetterISOLanguageName { get { throw null; } }
        public virtual string ThreeLetterWindowsLanguageName { get { throw null; } }
        public static System.Globalization.CultureInfo CreateSpecificCulture(string name) { throw null; }
        public static System.Globalization.CultureInfo GetCultureInfo(int culture) { throw null; }
        public static System.Globalization.CultureInfo GetCultureInfoByIetfLanguageTag(string name) { throw null; }
        public static System.Globalization.CultureInfo[] GetCultures(System.Globalization.CultureTypes types) { throw null; }
    }

    public partial class CultureNotFoundException : System.ArgumentException, System.Runtime.Serialization.ISerializable
    {
        public CultureNotFoundException(string message, int invalidCultureId, System.Exception innerException) { throw null; }
        public CultureNotFoundException(string paramName, int invalidCultureId, string message) { throw null; }
        public virtual System.Nullable<int> InvalidCultureId { get { throw null; } }
    }

    /*public partial class DateTimeFormatInfo
    {
        Can't do partials here so implement the stub in the main class
        public String DateSeparator { set { throw null; } }
        public String TimeSeparator { set { throw null; } }
    }*/

    public sealed partial class NumberFormatInfo : System.ICloneable, System.IFormatProvider
    {
        [System.Runtime.InteropServices.ComVisibleAttribute(false)]
        public System.Globalization.DigitShapes DigitSubstitution { get { throw null; } set { throw null; } }
    }

    public partial class RegionInfo
    {
        public RegionInfo(int culture) { throw null; }
        public virtual string ThreeLetterISORegionName { get { throw null; } }
        public virtual string ThreeLetterWindowsRegionName { get { throw null; } }
    }

    public partial class SortKey
    {
        internal SortKey() { throw null; }
    }

    public sealed partial class SortVersion : System.IEquatable<System.Globalization.SortVersion>
    {
        public SortVersion(int fullVersion, System.Guid sortId) { throw null; }
        public int FullVersion { get { throw null; } }
        public System.Guid SortId { get { throw null; } }
        public bool Equals(System.Globalization.SortVersion other) { throw null; }
        public override bool Equals(object obj) { throw null; }
        public override int GetHashCode() { throw null; }
        public static bool operator ==(System.Globalization.SortVersion left, System.Globalization.SortVersion right) { throw null; }
        public static bool operator !=(System.Globalization.SortVersion left, System.Globalization.SortVersion right) { throw null; }
    }

    public partial class TextInfo : System.ICloneable, System.Runtime.Serialization.IDeserializationCallback 
    {
        public virtual int ANSICodePage { get { throw null; } }
        public virtual int EBCDICCodePage { get { throw null; } }
        [System.Runtime.InteropServices.ComVisibleAttribute(false)]
        public int LCID { get { throw null; } }
        public virtual int MacCodePage { get { throw null; } }
        public virtual int OEMCodePage { get { throw null; } }
        public string ToTitleCase(string str) { throw null; }
    }
}