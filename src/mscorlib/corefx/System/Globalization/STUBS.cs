namespace System.Globalization
{
    public abstract partial class Calendar : System.ICloneable
    {
        [System.Runtime.InteropServices.ComVisibleAttribute(false)]
        public virtual int GetLeapMonth(int year) { throw null; }
    }

    [System.Runtime.InteropServices.ComVisible(true)]
    public enum CalendarAlgorithmType
    {
        Unknown = 0,            // This is the default value to return in the Calendar base class.
        SolarCalendar = 1,      // Solar-base calendar, such as GregorianCalendar, jaoaneseCalendar, JulianCalendar, etc.
                                // Solar calendars are based on the solar year and seasons.
        LunarCalendar = 2,      // Lunar-based calendar, such as Hijri and UmAlQuraCalendar.
                                // Lunar calendars are based on the path of the moon.  The seasons are not accurately represented.
        LunisolarCalendar = 3   // Lunisolar-based calendar which use leap month rule, such as HebrewCalendar and Asian Lunisolar calendars.
                                // Lunisolar calendars are based on the cycle of the moon, but consider the seasons as a secondary consideration,
                                // so they align with the seasons as well as lunar events.
    }

    public static partial class CharUnicodeInfo
    {
        public static int GetDecimalDigitValue(char ch) { throw null; }
        public static int GetDecimalDigitValue(string s, int index) { throw null; }
        public static int GetDigitValue(char ch) { throw null; }
        public static int GetDigitValue(string s, int index) { throw null; }
    }

    public partial class CompareInfo : System.Runtime.Serialization.IDeserializationCallback
    {
        public int LCID { get { throw null; } }
        public static System.Globalization.CompareInfo GetCompareInfo(int culture) { throw null; }
        public static System.Globalization.CompareInfo GetCompareInfo(int culture, System.Reflection.Assembly assembly) { throw null; }
        public static System.Globalization.CompareInfo GetCompareInfo(string name, System.Reflection.Assembly assembly) { throw null; }
        public virtual System.Globalization.SortKey GetSortKey(string source) { throw null; }
        public virtual System.Globalization.SortKey GetSortKey(string source, System.Globalization.CompareOptions options) { throw null; }
        public virtual int IndexOf(string source, char value, int startIndex) { throw null; }
        public virtual int IndexOf(string source, string value, int startIndex) { throw null; }
        [System.Runtime.InteropServices.ComVisibleAttribute(false)]
        public static bool IsSortable(char ch) { throw null; }
        [System.Runtime.InteropServices.ComVisibleAttribute(false)]
        [System.Security.SecuritySafeCriticalAttribute]
        public static bool IsSortable(string text) { throw null; }
        public virtual int LastIndexOf(string source, char value, int startIndex) { throw null; }
        public virtual int LastIndexOf(string source, string value, int startIndex) { throw null; }
    }

    public partial class CultureInfo : System.ICloneable, System.IFormatProvider
    {
        public CultureInfo(int culture) { throw null; }
        public CultureInfo(int culture, bool useUserOverride) { throw null; }
        public static System.Globalization.CultureInfo InstalledUICulture { get { throw null; } }
        public virtual int LCID { get { throw null; } }
        public virtual string ThreeLetterISOLanguageName { get { throw null; } }
        public virtual string ThreeLetterWindowsLanguageName { get { throw null; } }
        public void ClearCachedData() { throw null; }
        public static System.Globalization.CultureInfo CreateSpecificCulture(string name) { throw null; }
        public static System.Globalization.CultureInfo GetCultureInfo(int culture) { throw null; }
        public static System.Globalization.CultureInfo GetCultureInfo(string name, string altName) { throw null; }
        public static System.Globalization.CultureInfo GetCultureInfoByIetfLanguageTag(string name) { throw null; }
        public static System.Globalization.CultureInfo[] GetCultures(System.Globalization.CultureTypes types) { throw null; }
    }

    public partial class CultureNotFoundException : System.ArgumentException, System.Runtime.Serialization.ISerializable
    {
        public CultureNotFoundException(string message, int invalidCultureId, System.Exception innerException) { throw null; }
        public CultureNotFoundException(string paramName, int invalidCultureId, string message) { throw null; }
        public virtual System.Nullable<int> InvalidCultureId { get { throw null; } }
    }

    public enum CultureTypes
    {
        AllCultures = 7,
        [System.ObsoleteAttribute("This value has been deprecated.  Please use other values in CultureTypes.")]
        FrameworkCultures = 64,
        InstalledWin32Cultures = 4,
        NeutralCultures = 1,
        ReplacementCultures = 16,
        SpecificCultures = 2,
        UserCustomCulture = 8,
        [System.ObsoleteAttribute("This value has been deprecated.  Please use other values in CultureTypes.")]
        WindowsOnlyCultures = 32,
    }

    public sealed partial class DateTimeFormatInfo : System.ICloneable, System.IFormatProvider
    {
        // Can't do partial properties so add the setter for DateSeparator and TimeSeparator
        [System.Runtime.InteropServices.ComVisibleAttribute(false)]
        public string NativeCalendarName { get { throw null; } }
        public string[] GetAllDateTimePatterns() { throw null; }
        [System.Runtime.InteropServices.ComVisibleAttribute(false)]
        public string GetShortestDayName(System.DayOfWeek dayOfWeek) { throw null; }
        [System.Runtime.InteropServices.ComVisibleAttribute(false)]
        public void SetAllDateTimePatterns(string[] patterns, char format) { throw null; }
    }

    public enum DigitShapes
    {
        Context = 0,
        NativeNational = 2,
        None = 1,
    }

    public sealed partial class IdnMapping
    {
        public IdnMapping() { }
        public bool AllowUnassigned { get { throw null; } set { throw null; } }
        public bool UseStd3AsciiRules { get { throw null; } set { throw null; } }
        public override bool Equals(object obj) { throw null; }
        public string GetAscii(string unicode) { throw null; }
        public string GetAscii(string unicode, int index) { throw null; }
        public string GetAscii(string unicode, int index, int count) { throw null; }
        public override int GetHashCode() { throw null; }
        public string GetUnicode(string ascii) { throw null; }
        public string GetUnicode(string ascii, int index) { throw null; }
        public string GetUnicode(string ascii, int index, int count) { throw null; }
    }

    public sealed partial class NumberFormatInfo : System.ICloneable, System.IFormatProvider
    {
        [System.Runtime.InteropServices.ComVisibleAttribute(false)]
        public System.Globalization.DigitShapes DigitSubstitution { get { throw null; } set { throw null; } }
        [System.Runtime.InteropServices.ComVisibleAttribute(false)]
        public string[] NativeDigits { get { throw null; } set { throw null; } }
    }

    public partial class RegionInfo
    {
        public RegionInfo(int culture) { throw null; }
        [System.Runtime.InteropServices.ComVisibleAttribute(false)]
        public virtual string CurrencyEnglishName { get { throw null; } }
        [System.Runtime.InteropServices.ComVisibleAttribute(false)]
        public virtual string CurrencyNativeName { get { throw null; } }
        [System.Runtime.InteropServices.ComVisibleAttribute(false)]
        public virtual int GeoId { get { throw null; } }
        public virtual string ThreeLetterISORegionName { get { throw null; } }
        public virtual string ThreeLetterWindowsRegionName { get { throw null; } }
    }

    public partial class SortKey
    {
        internal SortKey() { throw null; }
        public virtual byte[] KeyData { get { throw null; } }
        public virtual string OriginalString { get { throw null; } }
        public static int Compare(System.Globalization.SortKey sortkey1, System.Globalization.SortKey sortkey2) { throw null; }
        public override bool Equals(object value) { throw null; }
        public override int GetHashCode() { throw null; }
        public override string ToString() { throw null; }
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

    public partial class StringInfo 
    {
        public string SubstringByTextElements(int startingTextElement) { throw null; }
        public string SubstringByTextElements(int startingTextElement, int lengthInTextElements) { throw null; }
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