// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace System.Globalization
{
#if INSIDE_CLR
    using LcidToCultureNameDictionary = Dictionary<int, string>;
    using CultureNameToLcidDictionary = Dictionary<string, int>;
#else
    using LcidToCultureNameDictionary = LowLevelDictionary<int, string>;
    using CultureNameToLcidDictionary = LowLevelDictionary<string, int>;
#endif

    internal partial class CultureData
    {
        // ICU constants
        const int ICU_ULOC_KEYWORD_AND_VALUES_CAPACITY = 100; // max size of keyword or value
        const int ICU_ULOC_FULLNAME_CAPACITY = 157;           // max size of locale name
        const string ICU_COLLATION_KEYWORD = "@collation=";
        
        private static LcidToCultureNameDictionary s_lcidToCultureNameDictionary;
        private static CultureNameToLcidDictionary s_cultureNameToLcidDictionary;
        /// <summary>
        /// This method uses the sRealName field (which is initialized by the constructor before this is called) to
        /// initialize the rest of the state of CultureData based on the underlying OS globalization library.
        /// </summary>
        [SecuritySafeCritical]
        private unsafe bool InitCultureData()
        {
            Contract.Assert(_sRealName != null);
            
            string alternateSortName = string.Empty;
            string realNameBuffer = _sRealName;

            // Basic validation
            if (realNameBuffer.Contains("@"))
            {
                return false; // don't allow ICU variants to come in directly
            }

            // Replace _ (alternate sort) with @collation= for ICU
            int index = realNameBuffer.IndexOf('_');
            if (index > 0)
            {
                if (index >= (realNameBuffer.Length - 1) // must have characters after _
                    || realNameBuffer.Substring(index + 1).Contains("_")) // only one _ allowed
                {
                    return false; // fail
                }
                alternateSortName = realNameBuffer.Substring(index + 1);
                realNameBuffer = realNameBuffer.Substring(0, index) + ICU_COLLATION_KEYWORD + alternateSortName;
            }

            // Get the locale name from ICU
            if (!GetLocaleName(realNameBuffer, out _sWindowsName))
            {
                return false; // fail
            }

            // Replace the ICU collation keyword with an _
            index = _sWindowsName.IndexOf(ICU_COLLATION_KEYWORD, StringComparison.Ordinal);
            if (index >= 0)
            {
                _sName = _sWindowsName.Substring(0, index) + "_" + alternateSortName;
            }
            else
            {
                _sName = _sWindowsName;
            }
            _sRealName = _sName;
            _sSpecificCulture = _sRealName; // we don't attempt to find a non-neutral locale if a neutral is passed in (unlike win32)

            _iLanguage = this.ILANGUAGE;
            if (_iLanguage == 0)
            {
                _iLanguage = LOCALE_CUSTOM_UNSPECIFIED;
            }

            _bNeutral = (this.SISO3166CTRYNAME.Length == 0);

            // Remove the sort from sName unless custom culture
            if (index>0 && !_bNeutral && !IsCustomCultureId(_iLanguage))
            {
                _sName = _sWindowsName.Substring(0, index);
            }
            return true;
        }

        [SecuritySafeCritical]
        internal static bool GetLocaleName(string localeName, out string windowsName)
        {
            // Get the locale name from ICU
            StringBuilder sb = StringBuilderCache.Acquire(ICU_ULOC_FULLNAME_CAPACITY);
            if (!Interop.GlobalizationInterop.GetLocaleName(localeName, sb, sb.Capacity))
            {
                StringBuilderCache.Release(sb);
                windowsName = null;
                return false; // fail
            }

            // Success - use the locale name returned which may be different than realNameBuffer (casing)
            windowsName = StringBuilderCache.GetStringAndRelease(sb); // the name passed to subsequent ICU calls
            return true;
        }

        [SecuritySafeCritical]
        internal static bool GetDefaultLocaleName(out string windowsName)
        {
            // Get the default (system) locale name from ICU
            StringBuilder sb = StringBuilderCache.Acquire(ICU_ULOC_FULLNAME_CAPACITY);
            if (!Interop.GlobalizationInterop.GetDefaultLocaleName(sb, sb.Capacity))
            {
                StringBuilderCache.Release(sb);
                windowsName = null;
                return false; // fail
            }

            // Success - use the locale name returned which may be different than realNameBuffer (casing)
            windowsName = StringBuilderCache.GetStringAndRelease(sb); // the name passed to subsequent ICU calls
            return true;
        }
        
        private string GetLocaleInfo(LocaleStringData type)
        {
            Contract.Assert(_sWindowsName != null, "[CultureData.GetLocaleInfo] Expected _sWindowsName to be populated already");
            return GetLocaleInfo(_sWindowsName, type);
        }

        // For LOCALE_SPARENT we need the option of using the "real" name (forcing neutral names) instead of the
        // "windows" name, which can be specific for downlevel (< windows 7) os's.
        [SecuritySafeCritical]
        private string GetLocaleInfo(string localeName, LocaleStringData type)
        {
            Contract.Assert(localeName != null, "[CultureData.GetLocaleInfo] Expected localeName to be not be null");

            switch (type)
            {
                case LocaleStringData.NegativeInfinitySymbol:
                    // not an equivalent in ICU; prefix the PositiveInfinitySymbol with NegativeSign
                    return GetLocaleInfo(localeName, LocaleStringData.NegativeSign) +
                        GetLocaleInfo(localeName, LocaleStringData.PositiveInfinitySymbol);
            }

            StringBuilder sb = StringBuilderCache.Acquire(ICU_ULOC_KEYWORD_AND_VALUES_CAPACITY);

            bool result = Interop.GlobalizationInterop.GetLocaleInfoString(localeName, (uint)type, sb, sb.Capacity);
            if (!result)
            {
                // Failed, just use empty string
                StringBuilderCache.Release(sb);
                Contract.Assert(false, "[CultureData.GetLocaleInfo(LocaleStringData)] Failed");
                return String.Empty;
            }
            return StringBuilderCache.GetStringAndRelease(sb);
        }

        [SecuritySafeCritical]
        private int GetLocaleInfo(LocaleNumberData type)
        {
            Contract.Assert(_sWindowsName != null, "[CultureData.GetLocaleInfo(LocaleNumberData)] Expected _sWindowsName to be populated already");

            switch (type)
            {
                case LocaleNumberData.CalendarType:
                    // returning 0 will cause the first supported calendar to be returned, which is the preferred calendar
                    return 0;
            }
            

            int value = 0;
            bool result = Interop.GlobalizationInterop.GetLocaleInfoInt(_sWindowsName, (uint)type, ref value);
            if (!result)
            {
                // Failed, just use 0
                Contract.Assert(false, "[CultureData.GetLocaleInfo(LocaleNumberData)] failed");
            }

            return value;
        }

        [SecuritySafeCritical]
        private int[] GetLocaleInfo(LocaleGroupingData type)
        {
            Contract.Assert(_sWindowsName != null, "[CultureData.GetLocaleInfo(LocaleGroupingData)] Expected _sWindowsName to be populated already");

            int primaryGroupingSize = 0;
            int secondaryGroupingSize = 0;
            bool result = Interop.GlobalizationInterop.GetLocaleInfoGroupingSizes(_sWindowsName, (uint)type, ref primaryGroupingSize, ref secondaryGroupingSize);
            if (!result)
            {
                Contract.Assert(false, "[CultureData.GetLocaleInfo(LocaleGroupingData type)] failed");
            }

            if (secondaryGroupingSize == 0)
            {
                return new int[] { primaryGroupingSize };
            }

            return new int[] { primaryGroupingSize, secondaryGroupingSize };
        }

        private string GetTimeFormatString()
        {
            return GetTimeFormatString(false);
        }

        [SecuritySafeCritical]
        private string GetTimeFormatString(bool shortFormat)
        {
            Contract.Assert(_sWindowsName != null, "[CultureData.GetTimeFormatString(bool shortFormat)] Expected _sWindowsName to be populated already");

            StringBuilder sb = StringBuilderCache.Acquire(ICU_ULOC_KEYWORD_AND_VALUES_CAPACITY);

            bool result = Interop.GlobalizationInterop.GetLocaleTimeFormat(_sWindowsName, shortFormat, sb, sb.Capacity);
            if (!result)
            {
                // Failed, just use empty string
                StringBuilderCache.Release(sb);
                Contract.Assert(false, "[CultureData.GetTimeFormatString(bool shortFormat)] Failed");
                return String.Empty;
            }

            return ConvertIcuTimeFormatString(StringBuilderCache.GetStringAndRelease(sb));
        }

        private int GetFirstDayOfWeek()
        {
            return this.GetLocaleInfo(LocaleNumberData.FirstDayOfWeek);
        }

        private String[] GetTimeFormats()
        {
            string format = GetTimeFormatString(false);
            return new string[] { format };
        }

        private String[] GetShortTimeFormats()
        {
            string format = GetTimeFormatString(true);
            return new string[] { format };
        }

        private static CultureData GetCultureDataFromRegionName(String regionName)
        {
            // no support to lookup by region name, other than the hard-coded list in CultureData
            return null;
        }

        private static string GetLanguageDisplayName(string cultureName)
        {
            return new CultureInfo(cultureName).m_cultureData.GetLocaleInfo(cultureName, LocaleStringData.LocalizedDisplayName);
        }

        private static string GetRegionDisplayName(string isoCountryCode)
        {
            // use the fallback which is to return NativeName
            return null;
        }

        private static CultureInfo GetUserDefaultCulture()
        {
            return CultureInfo.GetUserDefaultCulture();
        }

        private static string ConvertIcuTimeFormatString(string icuFormatString)
        {
            StringBuilder sb = StringBuilderCache.Acquire(ICU_ULOC_FULLNAME_CAPACITY);
            bool amPmAdded = false;

            for (int i = 0; i < icuFormatString.Length; i++)
            {
                switch(icuFormatString[i])
                {
                    case ':':
                    case '.':
                    case 'H':
                    case 'h':
                    case 'm':
                    case 's':
                        sb.Append(icuFormatString[i]);
                        break;

                    case ' ':
                    case '\u00A0':
                        // Convert nonbreaking spaces into regular spaces
                        sb.Append(' ');
                        break;

                    case 'a': // AM/PM
                        if (!amPmAdded)
                        {
                            amPmAdded = true;
                            sb.Append("tt");
                        }
                        break;

                }
            }

            return StringBuilderCache.GetStringAndRelease(sb);
        }
        
        private static string LCIDToLocaleName(int culture)
        {
            string cultureName;
            if (LcidToCultureNameMapper.TryGetValue(culture, out cultureName))
            {
                return cultureName;
            }
            
            return null;
        }

        private static int LocaleNameToLCID(string cultureName)
        {
            int lcid;
            if (CultureNameToLcidMapper.TryGetValue(AnsiToLower(cultureName), out lcid))
            {
                return lcid;
            }
            
            return LOCALE_CUSTOM_UNSPECIFIED;
        }

        private static CultureNameToLcidDictionary CultureNameToLcidMapper
        {
            get
            {
                if (s_cultureNameToLcidDictionary == null)
                {
                    CultureNameToLcidDictionary tempMapping = new CultureNameToLcidDictionary()
                    {
                        { "", 0x0007f },
                        /* { "aa-dj",  0x01000 },*/
                        /* { "aa-er",  0x01000 },*/
                        /* { "aa-et",  0x01000 },*/
                        /* { "af-na",  0x01000 },*/
                        { "af-za", 0x00436 },
                        /* { "agq-cm",  0x01000 },*/
                        /* { "ak-gh",  0x01000 },*/
                        { "am-et", 0x0045e },
                        /* { "ar-001",  0x01000 },*/
                        { "ar-ae", 0x03801 },
                        { "ar-bh", 0x03c01 },
                        /* { "ar-dj",  0x01000 },*/
                        { "ar-dz", 0x01401 },
                        { "ar-eg", 0x00c01 },
                        /* { "ar-er",  0x01000 },*/
                        /* { "ar-il",  0x01000 },*/
                        { "ar-iq", 0x00801 },
                        { "ar-jo", 0x02c01 },
                        /* { "ar-km",  0x01000 },*/
                        { "ar-kw", 0x03401 },
                        { "ar-lb", 0x03001 },
                        { "ar-ly", 0x01001 },
                        { "ar-ma", 0x01801 },
                        /* { "ar-mr",  0x01000 },*/
                        { "ar-om", 0x02001 },
                        /* { "ar-ps",  0x01000 },*/
                        { "ar-qa", 0x04001 },
                        { "ar-sa", 0x00401 },
                        /* { "ar-sd",  0x01000 },*/
                        /* { "ar-so",  0x01000 },*/
                        /* { "ar-ss",  0x01000 },*/
                        { "ar-sy", 0x02801 },
                        /* { "ar-td",  0x01000 },*/
                        { "ar-tn", 0x01c01 },
                        { "ar-ye", 0x02401 },
                        { "arn-cl", 0x0047a },
                        { "as-in", 0x0044d },
                        /* { "asa-tz",  0x01000 },*/
                        /* { "ast-es",  0x01000 },*/
                        { "az-cyrl-az", 0x0082c },
                        { "az-latn-az", 0x0042c },
                        { "ba-ru", 0x0046d },
                        /* { "bas-cm",  0x01000 },*/
                        { "be-by", 0x00423 },
                        /* { "bem-zm",  0x01000 },*/
                        /* { "bez-tz",  0x01000 },*/
                        { "bg-bg", 0x00402 },
                        { "bin-ng", 0x00466 },
                        /* { "bm-latn-ml",  0x01000 },*/
                        { "bn-bd", 0x00845 },
                        { "bn-in", 0x00445 },
                        { "bo-cn", 0x00451 },
                        /* { "bo-in",  0x01000 },*/
                        { "br-fr", 0x0047e },
                        /* { "brx-in",  0x01000 },*/
                        { "bs-cyrl-ba", 0x0201a },
                        { "bs-latn-ba", 0x0141a },
                        /* { "byn-er",  0x01000 },*/
                        /* { "ca-ad",  0x01000 },*/
                        { "ca-es", 0x00403 },
                        { "ca-es-valencia", 0x00803 },
                        /* { "ca-fr",  0x01000 },*/
                        /* { "ca-it",  0x01000 },*/
                        /* { "ce-ru",  0x01000 },*/
                        /* { "cgg-ug",  0x01000 },*/
                        { "chr-cher-us", 0x0045c },
                        { "co-fr", 0x00483 },
                        { "cs-cz", 0x00405 },
                        /* { "cu-ru",  0x01000 },*/
                        { "cy-gb", 0x00452 },
                        { "da-dk", 0x00406 },
                        /* { "da-gl",  0x01000 },*/
                        /* { "dav-ke",  0x01000 },*/
                        { "de-at", 0x00c07 },
                        /* { "de-be",  0x01000 },*/
                        { "de-ch", 0x00807 },
                        { "de-de", 0x00407 },
                        /* { "de-it",  0x01000 },*/
                        { "de-li", 0x01407 },
                        { "de-lu", 0x01007 },
                        /* { "dje-ne",  0x01000 },*/
                        { "dsb-de", 0x0082e },
                        /* { "dua-cm",  0x01000 },*/
                        { "dv-mv", 0x00465 },
                        /* { "dyo-sn",  0x01000 },*/
                        { "dz-bt", 0x00c51 },
                        /* { "ebu-ke",  0x01000 },*/
                        /* { "ee-gh",  0x01000 },*/
                        /* { "ee-tg",  0x01000 },*/
                        /* { "el-cy",  0x01000 },*/
                        { "el-gr", 0x00408 },
                        /* { "en-001",  0x01000 },*/
                        { "en-029", 0x02409 },
                        /* { "en-150",  0x01000 },*/
                        /* { "en-ag",  0x01000 },*/
                        /* { "en-ai",  0x01000 },*/
                        /* { "en-as",  0x01000 },*/
                        /* { "en-at",  0x01000 },*/
                        { "en-au", 0x00c09 },
                        /* { "en-bb",  0x01000 },*/
                        /* { "en-be",  0x01000 },*/
                        /* { "en-bi",  0x01000 },*/
                        /* { "en-bm",  0x01000 },*/
                        /* { "en-bs",  0x01000 },*/
                        /* { "en-bw",  0x01000 },*/
                        { "en-bz", 0x02809 },
                        { "en-ca", 0x01009 },
                        /* { "en-cc",  0x01000 },*/
                        /* { "en-ch",  0x01000 },*/
                        /* { "en-ck",  0x01000 },*/
                        /* { "en-cm",  0x01000 },*/
                        /* { "en-cx",  0x01000 },*/
                        /* { "en-cy",  0x01000 },*/
                        /* { "en-de",  0x01000 },*/
                        /* { "en-dk",  0x01000 },*/
                        /* { "en-dm",  0x01000 },*/
                        /* { "en-er",  0x01000 },*/
                        /* { "en-fi",  0x01000 },*/
                        /* { "en-fj",  0x01000 },*/
                        /* { "en-fk",  0x01000 },*/
                        /* { "en-fm",  0x01000 },*/
                        { "en-gb", 0x00809 },
                        /* { "en-gd",  0x01000 },*/
                        /* { "en-gg",  0x01000 },*/
                        /* { "en-gh",  0x01000 },*/
                        /* { "en-gi",  0x01000 },*/
                        /* { "en-gm",  0x01000 },*/
                        /* { "en-gu",  0x01000 },*/
                        /* { "en-gy",  0x01000 },*/
                        { "en-hk", 0x03c09 },
                        { "en-id", 0x03809 },
                        { "en-ie", 0x01809 },
                        /* { "en-il",  0x01000 },*/
                        /* { "en-im",  0x01000 },*/
                        { "en-in", 0x04009 },
                        /* { "en-io",  0x01000 },*/
                        /* { "en-je",  0x01000 },*/
                        { "en-jm", 0x02009 },
                        /* { "en-ke",  0x01000 },*/
                        /* { "en-ki",  0x01000 },*/
                        /* { "en-kn",  0x01000 },*/
                        /* { "en-ky",  0x01000 },*/
                        /* { "en-lc",  0x01000 },*/
                        /* { "en-lr",  0x01000 },*/
                        /* { "en-ls",  0x01000 },*/
                        /* { "en-mg",  0x01000 },*/
                        /* { "en-mh",  0x01000 },*/
                        /* { "en-mo",  0x01000 },*/
                        /* { "en-mp",  0x01000 },*/
                        /* { "en-ms",  0x01000 },*/
                        /* { "en-mt",  0x01000 },*/
                        /* { "en-mu",  0x01000 },*/
                        /* { "en-mw",  0x01000 },*/
                        { "en-my", 0x04409 },
                        /* { "en-na",  0x01000 },*/
                        /* { "en-nf",  0x01000 },*/
                        /* { "en-ng",  0x01000 },*/
                        /* { "en-nl",  0x01000 },*/
                        /* { "en-nr",  0x01000 },*/
                        /* { "en-nu",  0x01000 },*/
                        { "en-nz", 0x01409 },
                        /* { "en-pg",  0x01000 },*/
                        { "en-ph", 0x03409 },
                        /* { "en-pk",  0x01000 },*/
                        /* { "en-pn",  0x01000 },*/
                        /* { "en-pr",  0x01000 },*/
                        /* { "en-pw",  0x01000 },*/
                        /* { "en-rw",  0x01000 },*/
                        /* { "en-sb",  0x01000 },*/
                        /* { "en-sc",  0x01000 },*/
                        /* { "en-sd",  0x01000 },*/
                        /* { "en-se",  0x01000 },*/
                        { "en-sg", 0x04809 },
                        /* { "en-sh",  0x01000 },*/
                        /* { "en-si",  0x01000 },*/
                        /* { "en-sl",  0x01000 },*/
                        /* { "en-ss",  0x01000 },*/
                        /* { "en-sx",  0x01000 },*/
                        /* { "en-sz",  0x01000 },*/
                        /* { "en-tc",  0x01000 },*/
                        /* { "en-tk",  0x01000 },*/
                        /* { "en-to",  0x01000 },*/
                        { "en-tt", 0x02c09 },
                        /* { "en-tv",  0x01000 },*/
                        /* { "en-tz",  0x01000 },*/
                        /* { "en-ug",  0x01000 },*/
                        /* { "en-um",  0x01000 },*/
                        { "en-us", 0x00409 },
                        /* { "en-vc",  0x01000 },*/
                        /* { "en-vg",  0x01000 },*/
                        /* { "en-vi",  0x01000 },*/
                        /* { "en-vu",  0x01000 },*/
                        /* { "en-ws",  0x01000 },*/
                        { "en-za", 0x01c09 },
                        /* { "en-zm",  0x01000 },*/
                        { "en-zw", 0x03009 },
                        /* { "eo-001",  0x01000 },*/
                        { "es-419", 0x0580a },
                        { "es-ar", 0x02c0a },
                        { "es-bo", 0x0400a },
                        /* { "es-br",  0x01000 },*/
                        { "es-cl", 0x0340a },
                        { "es-co", 0x0240a },
                        { "es-cr", 0x0140a },
                        { "es-cu", 0x05c0a },
                        { "es-do", 0x01c0a },
                        { "es-ec", 0x0300a },
                        { "es-es", 0x00c0a },
                        { "es-es_tradnl", 0x0040a },
                        /* { "es-gq",  0x01000 },*/
                        { "es-gt", 0x0100a },
                        { "es-hn", 0x0480a },
                        { "es-mx", 0x0080a },
                        { "es-ni", 0x04c0a },
                        { "es-pa", 0x0180a },
                        { "es-pe", 0x0280a },
                        /* { "es-ph",  0x01000 },*/
                        { "es-pr", 0x0500a },
                        { "es-py", 0x03c0a },
                        { "es-sv", 0x0440a },
                        { "es-us", 0x0540a },
                        { "es-uy", 0x0380a },
                        { "es-ve", 0x0200a },
                        { "et-ee", 0x00425 },
                        { "eu-es", 0x0042d },
                        /* { "ewo-cm",  0x01000 },*/
                        { "fa-ir", 0x00429 },
                        /* { "ff-cm",  0x01000 },*/
                        /* { "ff-gn",  0x01000 },*/
                        { "ff-latn-sn", 0x00867 },
                        /* { "ff-mr",  0x01000 },*/
                        { "ff-ng", 0x00467 },
                        { "fi-fi", 0x0040b },
                        { "fil-ph", 0x00464 },
                        /* { "fo-dk",  0x01000 },*/
                        { "fo-fo", 0x00438 },
                        { "fr-029", 0x01c0c },
                        { "fr-be", 0x0080c },
                        /* { "fr-bf",  0x01000 },*/
                        /* { "fr-bi",  0x01000 },*/
                        /* { "fr-bj",  0x01000 },*/
                        /* { "fr-bl",  0x01000 },*/
                        { "fr-ca", 0x00c0c },
                        { "fr-cd", 0x0240c },
                        /* { "fr-cf",  0x01000 },*/
                        /* { "fr-cg",  0x01000 },*/
                        { "fr-ch", 0x0100c },
                        { "fr-ci", 0x0300c },
                        { "fr-cm", 0x02c0c },
                        /* { "fr-dj",  0x01000 },*/
                        /* { "fr-dz",  0x01000 },*/
                        { "fr-fr", 0x0040c },
                        /* { "fr-ga",  0x01000 },*/
                        /* { "fr-gf",  0x01000 },*/
                        /* { "fr-gn",  0x01000 },*/
                        /* { "fr-gp",  0x01000 },*/
                        /* { "fr-gq",  0x01000 },*/
                        { "fr-ht", 0x03c0c },
                        /* { "fr-km",  0x01000 },*/
                        { "fr-lu", 0x0140c },
                        { "fr-ma", 0x0380c },
                        { "fr-mc", 0x0180c },
                        /* { "fr-mf",  0x01000 },*/
                        /* { "fr-mg",  0x01000 },*/
                        { "fr-ml", 0x0340c },
                        /* { "fr-mq",  0x01000 },*/
                        /* { "fr-mr",  0x01000 },*/
                        /* { "fr-mu",  0x01000 },*/
                        /* { "fr-nc",  0x01000 },*/
                        /* { "fr-ne",  0x01000 },*/
                        /* { "fr-pf",  0x01000 },*/
                        /* { "fr-pm",  0x01000 },*/
                        { "fr-re", 0x0200c },
                        /* { "fr-rw",  0x01000 },*/
                        /* { "fr-sc",  0x01000 },*/
                        { "fr-sn", 0x0280c },
                        /* { "fr-sy",  0x01000 },*/
                        /* { "fr-td",  0x01000 },*/
                        /* { "fr-tg",  0x01000 },*/
                        /* { "fr-tn",  0x01000 },*/
                        /* { "fr-vu",  0x01000 },*/
                        /* { "fr-wf",  0x01000 },*/
                        /* { "fr-yt",  0x01000 },*/
                        /* { "fur-it",  0x01000 },*/
                        { "fy-nl", 0x00462 },
                        { "ga-ie", 0x0083c },
                        { "gd-gb", 0x00491 },
                        { "gl-es", 0x00456 },
                        { "gn-py", 0x00474 },
                        /* { "gsw-ch",  0x01000 },*/
                        { "gsw-fr", 0x00484 },
                        /* { "gsw-li",  0x01000 },*/
                        { "gu-in", 0x00447 },
                        /* { "guz-ke",  0x01000 },*/
                        /* { "gv-im",  0x01000 },*/
                        /* { "ha-latn-gh",  0x01000 },*/
                        /* { "ha-latn-ne",  0x01000 },*/
                        { "ha-latn-ng", 0x00468 },
                        { "haw-us", 0x00475 },
                        { "he-il", 0x0040d },
                        { "hi-in", 0x00439 },
                        { "hr-ba", 0x0101a },
                        { "hr-hr", 0x0041a },
                        { "hsb-de", 0x0042e },
                        { "hu-hu", 0x0040e },
                        { "hy-am", 0x0042b },
                        /* { "ia-001",  0x01000 },*/
                        /* { "ia-fr",  0x01000 },*/
                        { "ibb-ng", 0x00469 },
                        { "id-id", 0x00421 },
                        { "ig-ng", 0x00470 },
                        { "ii-cn", 0x00478 },
                        { "is-is", 0x0040f },
                        { "it-ch", 0x00810 },
                        { "it-it", 0x00410 },
                        /* { "it-sm",  0x01000 },*/
                        { "iu-cans-ca", 0x0045d },
                        { "iu-latn-ca", 0x0085d },
                        { "ja-jp", 0x00411 },
                        /* { "jgo-cm",  0x01000 },*/
                        /* { "jmc-tz",  0x01000 },*/
                        /* { "jv-java-id",  0x01000 },*/
                        /* { "jv-latn-id",  0x01000 },*/
                        { "ka-ge", 0x00437 },
                        /* { "kab-dz",  0x01000 },*/
                        /* { "kam-ke",  0x01000 },*/
                        /* { "kde-tz",  0x01000 },*/
                        /* { "kea-cv",  0x01000 },*/
                        /* { "khq-ml",  0x01000 },*/
                        /* { "ki-ke",  0x01000 },*/
                        { "kk-kz", 0x0043f },
                        /* { "kkj-cm",  0x01000 },*/
                        { "kl-gl", 0x0046f },
                        /* { "kln-ke",  0x01000 },*/
                        { "km-kh", 0x00453 },
                        { "kn-in", 0x0044b },
                        /* { "ko-kp",  0x01000 },*/
                        { "ko-kr", 0x00412 },
                        { "kok-in", 0x00457 },
                        { "kr-ng", 0x00471 },
                        /* { "ks-arab-in",  0x01000 },*/
                        { "ks-deva-in", 0x00860 },
                        /* { "ksb-tz",  0x01000 },*/
                        /* { "ksf-cm",  0x01000 },*/
                        /* { "ksh-de",  0x01000 },*/
                        { "ku-arab-iq", 0x00492 },
                        /* { "ku-arab-ir",  0x01000 },*/
                        /* { "kw-gb",  0x01000 },*/
                        { "ky-kg", 0x00440 },
                        { "la-001", 0x00476 },
                        /* { "lag-tz",  0x01000 },*/
                        { "lb-lu", 0x0046e },
                        /* { "lg-ug",  0x01000 },*/
                        /* { "lkt-us",  0x01000 },*/
                        /* { "ln-ao",  0x01000 },*/
                        /* { "ln-cd",  0x01000 },*/
                        /* { "ln-cf",  0x01000 },*/
                        /* { "ln-cg",  0x01000 },*/
                        { "lo-la", 0x00454 },
                        /* { "lrc-iq",  0x01000 },*/
                        /* { "lrc-ir",  0x01000 },*/
                        { "lt-lt", 0x00427 },
                        /* { "lu-cd",  0x01000 },*/
                        /* { "luo-ke",  0x01000 },*/
                        /* { "luy-ke",  0x01000 },*/
                        { "lv-lv", 0x00426 },
                        /* { "mas-ke",  0x01000 },*/
                        /* { "mas-tz",  0x01000 },*/
                        /* { "mer-ke",  0x01000 },*/
                        /* { "mfe-mu",  0x01000 },*/
                        /* { "mg-mg",  0x01000 },*/
                        /* { "mgh-mz",  0x01000 },*/
                        /* { "mgo-cm",  0x01000 },*/
                        { "mi-nz", 0x00481 },
                        { "mk-mk", 0x0042f },
                        { "ml-in", 0x0044c },
                        { "mn-mn", 0x00450 },
                        { "mn-mong-cn", 0x00850 },
                        { "mn-mong-mn", 0x00c50 },
                        { "mni-in", 0x00458 },
                        { "moh-ca", 0x0047c },
                        { "mr-in", 0x0044e },
                        { "ms-bn", 0x0083e },
                        { "ms-my", 0x0043e },
                        /* { "ms-sg",  0x01000 },*/
                        { "mt-mt", 0x0043a },
                        /* { "mua-cm",  0x01000 },*/
                        { "my-mm", 0x00455 },
                        /* { "mzn-ir",  0x01000 },*/
                        /* { "naq-na",  0x01000 },*/
                        { "nb-no", 0x00414 },
                        /* { "nb-sj",  0x01000 },*/
                        /* { "nd-zw",  0x01000 },*/
                        /* { "nds-de",  0x01000 },*/
                        /* { "nds-nl",  0x01000 },*/
                        { "ne-in", 0x00861 },
                        { "ne-np", 0x00461 },
                        /* { "nl-aw",  0x01000 },*/
                        { "nl-be", 0x00813 },
                        /* { "nl-bq",  0x01000 },*/
                        /* { "nl-cw",  0x01000 },*/
                        { "nl-nl", 0x00413 },
                        /* { "nl-sr",  0x01000 },*/
                        /* { "nl-sx",  0x01000 },*/
                        /* { "nmg-cm",  0x01000 },*/
                        { "nn-no", 0x00814 },
                        /* { "nnh-cm",  0x01000 },*/
                        /* { "nqo-gn",  0x01000 },*/
                        /* { "nr-za",  0x01000 },*/
                        { "nso-za", 0x0046c },
                        /* { "nus-ss",  0x01000 },*/
                        /* { "nyn-ug",  0x01000 },*/
                        { "oc-fr", 0x00482 },
                        { "om-et", 0x00472 },
                        /* { "om-ke",  0x01000 },*/
                        { "or-in", 0x00448 },
                        /* { "os-ge",  0x01000 },*/
                        /* { "os-ru",  0x01000 },*/
                        { "pa-arab-pk", 0x00846 },
                        { "pa-in", 0x00446 },
                        { "pap-029", 0x00479 },
                        { "pl-pl", 0x00415 },
                        /* { "prg-001",  0x01000 },*/
                        { "prs-af", 0x0048c },
                        { "ps-af", 0x00463 },
                        /* { "pt-ao",  0x01000 },*/
                        { "pt-br", 0x00416 },
                        /* { "pt-ch",  0x01000 },*/
                        /* { "pt-cv",  0x01000 },*/
                        /* { "pt-gq",  0x01000 },*/
                        /* { "pt-gw",  0x01000 },*/
                        /* { "pt-lu",  0x01000 },*/
                        /* { "pt-mo",  0x01000 },*/
                        /* { "pt-mz",  0x01000 },*/
                        { "pt-pt", 0x00816 },
                        /* { "pt-st",  0x01000 },*/
                        /* { "pt-tl",  0x01000 },*/
                        { "qps-latn-x-sh", 0x00901 },
                        { "qps-ploc", 0x00501 },
                        { "qps-ploca", 0x005fe },
                        { "qps-plocm", 0x009ff },
                        { "quc-latn-gt", 0x00486 },
                        { "quz-bo", 0x0046b },
                        { "quz-ec", 0x0086b },
                        { "quz-pe", 0x00c6b },
                        { "rm-ch", 0x00417 },
                        /* { "rn-bi",  0x01000 },*/
                        { "ro-md", 0x00818 },
                        { "ro-ro", 0x00418 },
                        /* { "rof-tz",  0x01000 },*/
                        /* { "ru-by",  0x01000 },*/
                        /* { "ru-kg",  0x01000 },*/
                        /* { "ru-kz",  0x01000 },*/
                        { "ru-md", 0x00819 },
                        { "ru-ru", 0x00419 },
                        /* { "ru-ua",  0x01000 },*/
                        { "rw-rw", 0x00487 },
                        /* { "rwk-tz",  0x01000 },*/
                        { "sa-in", 0x0044f },
                        { "sah-ru", 0x00485 },
                        /* { "saq-ke",  0x01000 },*/
                        /* { "sbp-tz",  0x01000 },*/
                        { "sd-arab-pk", 0x00859 },
                        { "sd-deva-in", 0x00459 },
                        { "se-fi", 0x00c3b },
                        { "se-no", 0x0043b },
                        { "se-se", 0x0083b },
                        /* { "seh-mz",  0x01000 },*/
                        /* { "ses-ml",  0x01000 },*/
                        /* { "sg-cf",  0x01000 },*/
                        /* { "shi-latn-ma",  0x01000 },*/
                        /* { "shi-tfng-ma",  0x01000 },*/
                        { "si-lk", 0x0045b },
                        { "sk-sk", 0x0041b },
                        { "sl-si", 0x00424 },
                        { "sma-no", 0x0183b },
                        { "sma-se", 0x01c3b },
                        { "smj-no", 0x0103b },
                        { "smj-se", 0x0143b },
                        { "smn-fi", 0x0243b },
                        { "sms-fi", 0x0203b },
                        /* { "sn-latn-zw",  0x01000 },*/
                        /* { "so-dj",  0x01000 },*/
                        /* { "so-et",  0x01000 },*/
                        /* { "so-ke",  0x01000 },*/
                        { "so-so", 0x00477 },
                        { "sq-al", 0x0041c },
                        /* { "sq-mk",  0x01000 },*/
                        /* { "sq-xk",  0x01000 },*/
                        { "sr-cyrl-ba", 0x01c1a },
                        { "sr-cyrl-cs", 0x00c1a },
                        { "sr-cyrl-me", 0x0301a },
                        { "sr-cyrl-rs", 0x0281a },
                        /* { "sr-cyrl-xk",  0x01000 },*/
                        { "sr-latn-ba", 0x0181a },
                        { "sr-latn-cs", 0x0081a },
                        { "sr-latn-me", 0x02c1a },
                        { "sr-latn-rs", 0x0241a },
                        /* { "sr-latn-xk",  0x01000 },*/
                        /* { "ss-sz",  0x01000 },*/
                        /* { "ss-za",  0x01000 },*/
                        /* { "ssy-er",  0x01000 },*/
                        /* { "st-ls",  0x01000 },*/
                        { "st-za", 0x00430 },
                        /* { "sv-ax",  0x01000 },*/
                        { "sv-fi", 0x0081d },
                        { "sv-se", 0x0041d },
                        /* { "sw-cd",  0x01000 },*/
                        { "sw-ke", 0x00441 },
                        /* { "sw-tz",  0x01000 },*/
                        /* { "sw-ug",  0x01000 },*/
                        { "syr-sy", 0x0045a },
                        { "ta-in", 0x00449 },
                        { "ta-lk", 0x00849 },
                        /* { "ta-my",  0x01000 },*/
                        /* { "ta-sg",  0x01000 },*/
                        { "te-in", 0x0044a },
                        /* { "teo-ke",  0x01000 },*/
                        /* { "teo-ug",  0x01000 },*/
                        { "tg-cyrl-tj", 0x00428 },
                        { "th-th", 0x0041e },
                        { "ti-er", 0x00873 },
                        { "ti-et", 0x00473 },
                        /* { "tig-er",  0x01000 },*/
                        { "tk-tm", 0x00442 },
                        { "tn-bw", 0x00832 },
                        { "tn-za", 0x00432 },
                        /* { "to-to",  0x01000 },*/
                        /* { "tr-cy",  0x01000 },*/
                        { "tr-tr", 0x0041f },
                        { "ts-za", 0x00431 },
                        { "tt-ru", 0x00444 },
                        /* { "twq-ne",  0x01000 },*/
                        { "tzm-arab-ma", 0x0045f },
                        { "tzm-latn-dz", 0x0085f },
                        /* { "tzm-latn-ma",  0x01000 },*/
                        { "tzm-tfng-ma", 0x0105f },
                        { "ug-cn", 0x00480 },
                        { "uk-ua", 0x00422 },
                        { "ur-in", 0x00820 },
                        { "ur-pk", 0x00420 },
                        /* { "uz-arab-af",  0x01000 },*/
                        { "uz-cyrl-uz", 0x00843 },
                        { "uz-latn-uz", 0x00443 },
                        /* { "vai-latn-lr",  0x01000 },*/
                        /* { "vai-vaii-lr",  0x01000 },*/
                        { "ve-za", 0x00433 },
                        { "vi-vn", 0x0042a },
                        /* { "vo-001",  0x01000 },*/
                        /* { "vun-tz",  0x01000 },*/
                        /* { "wae-ch",  0x01000 },*/
                        /* { "wal-et",  0x01000 },*/
                        { "wo-sn", 0x00488 },
                        { "xh-za", 0x00434 },
                        /* { "xog-ug",  0x01000 },*/
                        /* { "yav-cm",  0x01000 },*/
                        { "yi-001", 0x0043d },
                        /* { "yo-bj",  0x01000 },*/
                        { "yo-ng", 0x0046a },
                        /* { "yue-hk",  0x01000 },*/
                        /* { "zgh-tfng-ma",  0x01000 },*/
                        { "zh-cn", 0x00804 },
                        { "zh-hk", 0x00c04 },
                        /* { "zh-hans-hk",  0x01000 },*/
                        /* { "zh-hans-mo",  0x01000 },*/
                        { "zh-mo", 0x01404 },
                        { "zh-sg", 0x01004 },
                        { "zh-tw", 0x00404 },
                        { "zu-za", 0x00435 },
                        /* { "aa",  0x01000 },*/
                        { "af", 0x00036 },
                        /* { "agq",  0x01000 },*/
                        /* { "ak",  0x01000 },*/
                        { "am", 0x0005e },
                        { "ar", 0x00001 },
                        { "arn", 0x0007a },
                        { "as", 0x0004d },
                        /* { "asa",  0x01000 },*/
                        /* { "ast",  0x01000 },*/
                        { "az", 0x0002c },
                        { "az-cyrl", 0x0742c },
                        { "az-latn", 0x0782c },
                        { "ba", 0x0006d },
                        /* { "bas",  0x01000 },*/
                        { "be", 0x00023 },
                        /* { "bem",  0x01000 },*/
                        /* { "bez",  0x01000 },*/
                        { "bg", 0x00002 },
                        { "bin", 0x00066 },
                        /* { "bm",  0x01000 },*/
                        /* { "bm-latn",  0x01000 },*/
                        { "bn", 0x00045 },
                        { "bo", 0x00051 },
                        { "br", 0x0007e },
                        /* { "brx",  0x01000 },*/
                        { "bs", 0x0781a },
                        { "bs-cyrl", 0x0641a },
                        { "bs-latn", 0x0681a },
                        /* { "byn",  0x01000 },*/
                        { "ca", 0x00003 },
                        /* { "cgg",  0x01000 },*/
                        { "chr", 0x0005c },
                        { "chr-cher", 0x07c5c },
                        { "co", 0x00083 },
                        { "cs", 0x00005 },
                        { "cy", 0x00052 },
                        { "da", 0x00006 },
                        /* { "dav",  0x01000 },*/
                        { "de", 0x00007 },
                        /* { "dje",  0x01000 },*/
                        { "dsb", 0x07c2e },
                        /* { "dua",  0x01000 },*/
                        { "dv", 0x00065 },
                        /* { "dyo",  0x01000 },*/
                        /* { "dz",  0x01000 },*/
                        /* { "ebu",  0x01000 },*/
                        /* { "ee",  0x01000 },*/
                        { "el", 0x00008 },
                        { "en", 0x00009 },
                        /* { "eo",  0x01000 },*/
                        { "es", 0x0000a },
                        { "et", 0x00025 },
                        { "eu", 0x0002d },
                        /* { "ewo",  0x01000 },*/
                        { "fa", 0x00029 },
                        { "ff", 0x00067 },
                        { "ff-latn", 0x07c67 },
                        { "fi", 0x0000b },
                        { "fil", 0x00064 },
                        { "fo", 0x00038 },
                        { "fr", 0x0000c },
                        /* { "fur",  0x01000 },*/
                        { "fy", 0x00062 },
                        { "ga", 0x0003c },
                        { "gd", 0x00091 },
                        { "gl", 0x00056 },
                        { "gn", 0x00074 },
                        { "gsw", 0x00084 },
                        { "gu", 0x00047 },
                        /* { "guz",  0x01000 },*/
                        /* { "gv",  0x01000 },*/
                        { "ha", 0x00068 },
                        { "ha-latn", 0x07c68 },
                        { "haw", 0x00075 },
                        { "he", 0x0000d },
                        { "hi", 0x00039 },
                        { "hr", 0x0001a },
                        { "hsb", 0x0002e },
                        { "hu", 0x0000e },
                        { "hy", 0x0002b },
                        /* { "ia",  0x01000 },*/
                        { "ibb", 0x00069 },
                        { "id", 0x00021 },
                        { "ig", 0x00070 },
                        { "ii", 0x00078 },
                        { "is", 0x0000f },
                        { "it", 0x00010 },
                        { "iu", 0x0005d },
                        { "iu-cans", 0x0785d },
                        { "iu-latn", 0x07c5d },
                        { "ja", 0x00011 },
                        /* { "jgo",  0x01000 },*/
                        /* { "jmc",  0x01000 },*/
                        /* { "jv",  0x01000 },*/
                        /* { "jv-java",  0x01000 },*/
                        /* { "jv-latn",  0x01000 },*/
                        { "ka", 0x00037 },
                        /* { "kab",  0x01000 },*/
                        /* { "kam",  0x01000 },*/
                        /* { "kde",  0x01000 },*/
                        /* { "kea",  0x01000 },*/
                        /* { "khq",  0x01000 },*/
                        /* { "ki",  0x01000 },*/
                        { "kk", 0x0003f },
                        /* { "kkj",  0x01000 },*/
                        { "kl", 0x0006f },
                        /* { "kln",  0x01000 },*/
                        { "km", 0x00053 },
                        { "kn", 0x0004b },
                        { "ko", 0x00012 },
                        { "kok", 0x00057 },
                        { "kr", 0x00071 },
                        { "ks", 0x00060 },
                        { "ks-arab", 0x00460 },
                        /* { "ks-deva",  0x01000 },*/
                        /* { "ksb",  0x01000 },*/
                        /* { "ksf",  0x01000 },*/
                        /* { "ksh",  0x01000 },*/
                        { "ku", 0x00092 },
                        { "ku-arab", 0x07c92 },
                        /* { "kw",  0x01000 },*/
                        { "ky", 0x00040 },
                        { "la", 0x00076 },
                        /* { "lag",  0x01000 },*/
                        { "lb", 0x0006e },
                        /* { "lg",  0x01000 },*/
                        /* { "lkt",  0x01000 },*/
                        /* { "ln",  0x01000 },*/
                        { "lo", 0x00054 },
                        { "lt", 0x00027 },
                        /* { "lu",  0x01000 },*/
                        /* { "luo",  0x01000 },*/
                        /* { "luy",  0x01000 },*/
                        { "lv", 0x00026 },
                        /* { "mas",  0x01000 },*/
                        /* { "mer",  0x01000 },*/
                        /* { "mfe",  0x01000 },*/
                        /* { "mg",  0x01000 },*/
                        /* { "mgh",  0x01000 },*/
                        /* { "mgo",  0x01000 },*/
                        { "mi", 0x00081 },
                        { "mk", 0x0002f },
                        { "ml", 0x0004c },
                        { "mn", 0x00050 },
                        { "mn-cyrl", 0x07850 },
                        { "mn-mong", 0x07c50 },
                        { "mni", 0x00058 },
                        { "moh", 0x0007c },
                        { "mr", 0x0004e },
                        { "ms", 0x0003e },
                        { "mt", 0x0003a },
                        /* { "mua",  0x01000 },*/
                        { "my", 0x00055 },
                        /* { "naq",  0x01000 },*/
                        { "nb", 0x07c14 },
                        /* { "nd",  0x01000 },*/
                        { "ne", 0x00061 },
                        { "nl", 0x00013 },
                        /* { "nmg",  0x01000 },*/
                        { "nn", 0x07814 },
                        /* { "nnh",  0x01000 },*/
                        { "no", 0x00014 },
                        /* { "nqo",  0x01000 },*/
                        /* { "nr",  0x01000 },*/
                        { "nso", 0x0006c },
                        /* { "nus",  0x01000 },*/
                        /* { "nyn",  0x01000 },*/
                        { "oc", 0x00082 },
                        { "om", 0x00072 },
                        { "or", 0x00048 },
                        /* { "os",  0x01000 },*/
                        { "pa", 0x00046 },
                        { "pa-arab", 0x07c46 },
                        { "pap", 0x00079 },
                        { "pl", 0x00015 },
                        { "prs", 0x0008c },
                        { "ps", 0x00063 },
                        { "pt", 0x00016 },
                        { "quc", 0x00086 },
                        { "quc-latn", 0x07c86 },
                        { "quz", 0x0006b },
                        { "rm", 0x00017 },
                        /* { "rn",  0x01000 },*/
                        { "ro", 0x00018 },
                        /* { "rof",  0x01000 },*/
                        { "ru", 0x00019 },
                        { "rw", 0x00087 },
                        /* { "rwk",  0x01000 },*/
                        { "sa", 0x0004f },
                        { "sah", 0x00085 },
                        /* { "saq",  0x01000 },*/
                        /* { "sbp",  0x01000 },*/
                        { "sd", 0x00059 },
                        { "sd-arab", 0x07c59 },
                        /* { "sd-deva",  0x01000 },*/
                        { "se", 0x0003b },
                        /* { "seh",  0x01000 },*/
                        /* { "ses",  0x01000 },*/
                        /* { "sg",  0x01000 },*/
                        /* { "shi",  0x01000 },*/
                        /* { "shi-latn",  0x01000 },*/
                        /* { "shi-tfng",  0x01000 },*/
                        { "si", 0x0005b },
                        { "sk", 0x0001b },
                        { "sl", 0x00024 },
                        { "sma", 0x0783b },
                        { "smj", 0x07c3b },
                        { "smn", 0x0703b },
                        { "sms", 0x0743b },
                        /* { "sn",  0x01000 },*/
                        /* { "sn-latn",  0x01000 },*/
                        { "so", 0x00077 },
                        { "sq", 0x0001c },
                        { "sr", 0x07c1a },
                        { "sr-cyrl", 0x06c1a },
                        { "sr-latn", 0x0701a },
                        /* { "ss",  0x01000 },*/
                        /* { "ssy",  0x01000 },*/
                        { "st", 0x00030 },
                        { "sv", 0x0001d },
                        { "sw", 0x00041 },
                        /* { "swc",  0x01000 },*/
                        /* { "swc-cd",  0x01000 },*/
                        { "syr", 0x0005a },
                        { "ta", 0x00049 },
                        { "te", 0x0004a },
                        /* { "teo",  0x01000 },*/
                        { "tg", 0x00028 },
                        { "tg-cyrl", 0x07c28 },
                        { "th", 0x0001e },
                        { "ti", 0x00073 },
                        /* { "tig",  0x01000 },*/
                        { "tk", 0x00042 },
                        { "tn", 0x00032 },
                        /* { "to",  0x01000 },*/
                        { "tr", 0x0001f },
                        { "ts", 0x00031 },
                        { "tt", 0x00044 },
                        /* { "twq",  0x01000 },*/
                        { "tzm", 0x0005f },
                        /* { "tzm-arab",  0x01000 },*/
                        { "tzm-latn", 0x07c5f },
                        { "tzm-tfng", 0x0785f },
                        { "ug", 0x00080 },
                        { "uk", 0x00022 },
                        { "ur", 0x00020 },
                        { "uz", 0x00043 },
                        /* { "uz-arab",  0x01000 },*/
                        { "uz-cyrl", 0x07843 },
                        { "uz-latn", 0x07c43 },
                        /* { "vai",  0x01000 },*/
                        /* { "vai-latn",  0x01000 },*/
                        /* { "vai-vaii",  0x01000 },*/
                        { "ve", 0x00033 },
                        { "vi", 0x0002a },
                        /* { "vo",  0x01000 },*/
                        /* { "vun",  0x01000 },*/
                        /* { "wae",  0x01000 },*/
                        /* { "wal",  0x01000 },*/
                        { "wo", 0x00088 },
                        { "xh", 0x00034 },
                        /* { "xog",  0x01000 },*/
                        /* { "yav",  0x01000 },*/
                        { "yi", 0x0003d },
                        { "yo", 0x0006a },
                        /* { "zgh",  0x01000 },*/
                        /* { "zgh-tfng",  0x01000 },*/
                        { "zh", 0x07804 },
                        { "zh-hans", 0x00004 },
                        { "zh-hant", 0x07c04 },
                        { "zu", 0x00035 },
                        { "zh-chs", 0x00004 },
                        { "zh-cht", 0x07c04 },
                        { "de-de_phoneb", 0x10407 },
                        { "hu-hu_technl", 0x1040e },
                        { "ja-jp_radstr", 0x40411 },
                        { "ka-ge_modern", 0x10437 },
                        { "x-iv_mathan", 0x1007f },
                        { "zh-cn_phoneb", 0x50804 },
                        { "zh-cn_stroke", 0x20804 },
                        { "zh-hk_radstr", 0x40c04 },
                        { "zh-mo_radstr", 0x41404 },
                        { "zh-mo_stroke", 0x21404 },
                        { "zh-sg_phoneb", 0x51004 },
                        { "zh-sg_stroke", 0x21004 },
                        { "zh-tw_pronun", 0x30404 },
                        { "zh-tw_radstr", 0x40404 }                        
                    };
                    s_cultureNameToLcidDictionary = tempMapping; 
                }
                return s_cultureNameToLcidDictionary;
            }   
        }
        
        private static LcidToCultureNameDictionary LcidToCultureNameMapper
        {
            get
            {
                if (s_lcidToCultureNameDictionary == null)
                {
                    // lazy initialize it when needed
                    LcidToCultureNameDictionary tempMapping = new LcidToCultureNameDictionary()
                    {
                        { 0x0007f, "" },
                        /* { 0x01000, "aa-DJ" },*/
                        /* { 0x01000, "aa-ER" },*/
                        /* { 0x01000, "aa-ET" },*/
                        /* { 0x01000, "af-NA" },*/
                        { 0x00436, "af-ZA" },
                        /* { 0x01000, "agq-CM" },*/
                        /* { 0x01000, "ak-GH" },*/
                        { 0x0045e, "am-ET" },
                        /* { 0x01000, "ar-001" },*/
                        { 0x03801, "ar-AE" },
                        { 0x03c01, "ar-BH" },
                        /* { 0x01000, "ar-DJ" },*/
                        { 0x01401, "ar-DZ" },
                        { 0x00c01, "ar-EG" },
                        /* { 0x01000, "ar-ER" },*/
                        /* { 0x01000, "ar-IL" },*/
                        { 0x00801, "ar-IQ" },
                        { 0x02c01, "ar-JO" },
                        /* { 0x01000, "ar-KM" },*/
                        { 0x03401, "ar-KW" },
                        { 0x03001, "ar-LB" },
                        { 0x01001, "ar-LY" },
                        { 0x01801, "ar-MA" },
                        /* { 0x01000, "ar-MR" },*/
                        { 0x02001, "ar-OM" },
                        /* { 0x01000, "ar-PS" },*/
                        { 0x04001, "ar-QA" },
                        { 0x00401, "ar-SA" },
                        /* { 0x01000, "ar-SD" },*/
                        /* { 0x01000, "ar-SO" },*/
                        /* { 0x01000, "ar-SS" },*/
                        { 0x02801, "ar-SY" },
                        /* { 0x01000, "ar-TD" },*/
                        { 0x01c01, "ar-TN" },
                        { 0x02401, "ar-YE" },
                        { 0x0047a, "arn-CL" },
                        { 0x0044d, "as-IN" },
                        /* { 0x01000, "asa-TZ" },*/
                        /* { 0x01000, "ast-ES" },*/
                        { 0x0082c, "az-Cyrl-AZ" },
                        { 0x0042c, "az-Latn-AZ" },
                        { 0x0046d, "ba-RU" },
                        /* { 0x01000, "bas-CM" },*/
                        { 0x00423, "be-BY" },
                        /* { 0x01000, "bem-ZM" },*/
                        /* { 0x01000, "bez-TZ" },*/
                        { 0x00402, "bg-BG" },
                        { 0x00466, "bin-NG" },
                        /* { 0x01000, "bm-Latn-ML" },*/
                        { 0x00845, "bn-BD" },
                        { 0x00445, "bn-IN" },
                        { 0x00451, "bo-CN" },
                        /* { 0x01000, "bo-IN" },*/
                        { 0x0047e, "br-FR" },
                        /* { 0x01000, "brx-IN" },*/
                        { 0x0201a, "bs-Cyrl-BA" },
                        { 0x0141a, "bs-Latn-BA" },
                        /* { 0x01000, "byn-ER" },*/
                        /* { 0x01000, "ca-AD" },*/
                        { 0x00403, "ca-ES" },
                        { 0x00803, "ca-ES-valencia" },
                        /* { 0x01000, "ca-FR" },*/
                        /* { 0x01000, "ca-IT" },*/
                        /* { 0x01000, "ce-RU" },*/
                        /* { 0x01000, "cgg-UG" },*/
                        { 0x0045c, "chr-Cher-US" },
                        { 0x00483, "co-FR" },
                        { 0x00405, "cs-CZ" },
                        /* { 0x01000, "cu-RU" },*/
                        { 0x00452, "cy-GB" },
                        { 0x00406, "da-DK" },
                        /* { 0x01000, "da-GL" },*/
                        /* { 0x01000, "dav-KE" },*/
                        { 0x00c07, "de-AT" },
                        /* { 0x01000, "de-BE" },*/
                        { 0x00807, "de-CH" },
                        { 0x00407, "de-DE" },
                        /* { 0x01000, "de-IT" },*/
                        { 0x01407, "de-LI" },
                        { 0x01007, "de-LU" },
                        /* { 0x01000, "dje-NE" },*/
                        { 0x0082e, "dsb-DE" },
                        /* { 0x01000, "dua-CM" },*/
                        { 0x00465, "dv-MV" },
                        /* { 0x01000, "dyo-SN" },*/
                        { 0x00c51, "dz-BT" },
                        /* { 0x01000, "ebu-KE" },*/
                        /* { 0x01000, "ee-GH" },*/
                        /* { 0x01000, "ee-TG" },*/
                        /* { 0x01000, "el-CY" },*/
                        { 0x00408, "el-GR" },
                        /* { 0x01000, "en-001" },*/
                        { 0x02409, "en-029" },
                        /* { 0x01000, "en-150" },*/
                        /* { 0x01000, "en-AG" },*/
                        /* { 0x01000, "en-AI" },*/
                        /* { 0x01000, "en-AS" },*/
                        /* { 0x01000, "en-AT" },*/
                        { 0x00c09, "en-AU" },
                        /* { 0x01000, "en-BB" },*/
                        /* { 0x01000, "en-BE" },*/
                        /* { 0x01000, "en-BI" },*/
                        /* { 0x01000, "en-BM" },*/
                        /* { 0x01000, "en-BS" },*/
                        /* { 0x01000, "en-BW" },*/
                        { 0x02809, "en-BZ" },
                        { 0x01009, "en-CA" },
                        /* { 0x01000, "en-CC" },*/
                        /* { 0x01000, "en-CH" },*/
                        /* { 0x01000, "en-CK" },*/
                        /* { 0x01000, "en-CM" },*/
                        /* { 0x01000, "en-CX" },*/
                        /* { 0x01000, "en-CY" },*/
                        /* { 0x01000, "en-DE" },*/
                        /* { 0x01000, "en-DK" },*/
                        /* { 0x01000, "en-DM" },*/
                        /* { 0x01000, "en-ER" },*/
                        /* { 0x01000, "en-FI" },*/
                        /* { 0x01000, "en-FJ" },*/
                        /* { 0x01000, "en-FK" },*/
                        /* { 0x01000, "en-FM" },*/
                        { 0x00809, "en-GB" },
                        /* { 0x01000, "en-GD" },*/
                        /* { 0x01000, "en-GG" },*/
                        /* { 0x01000, "en-GH" },*/
                        /* { 0x01000, "en-GI" },*/
                        /* { 0x01000, "en-GM" },*/
                        /* { 0x01000, "en-GU" },*/
                        /* { 0x01000, "en-GY" },*/
                        { 0x03c09, "en-HK" },
                        { 0x03809, "en-ID" },
                        { 0x01809, "en-IE" },
                        /* { 0x01000, "en-IL" },*/
                        /* { 0x01000, "en-IM" },*/
                        { 0x04009, "en-IN" },
                        /* { 0x01000, "en-IO" },*/
                        /* { 0x01000, "en-JE" },*/
                        { 0x02009, "en-JM" },
                        /* { 0x01000, "en-KE" },*/
                        /* { 0x01000, "en-KI" },*/
                        /* { 0x01000, "en-KN" },*/
                        /* { 0x01000, "en-KY" },*/
                        /* { 0x01000, "en-LC" },*/
                        /* { 0x01000, "en-LR" },*/
                        /* { 0x01000, "en-LS" },*/
                        /* { 0x01000, "en-MG" },*/
                        /* { 0x01000, "en-MH" },*/
                        /* { 0x01000, "en-MO" },*/
                        /* { 0x01000, "en-MP" },*/
                        /* { 0x01000, "en-MS" },*/
                        /* { 0x01000, "en-MT" },*/
                        /* { 0x01000, "en-MU" },*/
                        /* { 0x01000, "en-MW" },*/
                        { 0x04409, "en-MY" },
                        /* { 0x01000, "en-NA" },*/
                        /* { 0x01000, "en-NF" },*/
                        /* { 0x01000, "en-NG" },*/
                        /* { 0x01000, "en-NL" },*/
                        /* { 0x01000, "en-NR" },*/
                        /* { 0x01000, "en-NU" },*/
                        { 0x01409, "en-NZ" },
                        /* { 0x01000, "en-PG" },*/
                        { 0x03409, "en-PH" },
                        /* { 0x01000, "en-PK" },*/
                        /* { 0x01000, "en-PN" },*/
                        /* { 0x01000, "en-PR" },*/
                        /* { 0x01000, "en-PW" },*/
                        /* { 0x01000, "en-RW" },*/
                        /* { 0x01000, "en-SB" },*/
                        /* { 0x01000, "en-SC" },*/
                        /* { 0x01000, "en-SD" },*/
                        /* { 0x01000, "en-SE" },*/
                        { 0x04809, "en-SG" },
                        /* { 0x01000, "en-SH" },*/
                        /* { 0x01000, "en-SI" },*/
                        /* { 0x01000, "en-SL" },*/
                        /* { 0x01000, "en-SS" },*/
                        /* { 0x01000, "en-SX" },*/
                        /* { 0x01000, "en-SZ" },*/
                        /* { 0x01000, "en-TC" },*/
                        /* { 0x01000, "en-TK" },*/
                        /* { 0x01000, "en-TO" },*/
                        { 0x02c09, "en-TT" },
                        /* { 0x01000, "en-TV" },*/
                        /* { 0x01000, "en-TZ" },*/
                        /* { 0x01000, "en-UG" },*/
                        /* { 0x01000, "en-UM" },*/
                        { 0x00409, "en-US" },
                        /* { 0x01000, "en-VC" },*/
                        /* { 0x01000, "en-VG" },*/
                        /* { 0x01000, "en-VI" },*/
                        /* { 0x01000, "en-VU" },*/
                        /* { 0x01000, "en-WS" },*/
                        { 0x01c09, "en-ZA" },
                        /* { 0x01000, "en-ZM" },*/
                        { 0x03009, "en-ZW" },
                        /* { 0x01000, "eo-001" },*/
                        { 0x0580a, "es-419" },
                        { 0x02c0a, "es-AR" },
                        { 0x0400a, "es-BO" },
                        /* { 0x01000, "es-BR" },*/
                        { 0x0340a, "es-CL" },
                        { 0x0240a, "es-CO" },
                        { 0x0140a, "es-CR" },
                        { 0x05c0a, "es-CU" },
                        { 0x01c0a, "es-DO" },
                        { 0x0300a, "es-EC" },
                        { 0x00c0a, "es-ES" },
                        { 0x0040a, "es-ES_tradnl" },
                        /* { 0x01000, "es-GQ" },*/
                        { 0x0100a, "es-GT" },
                        { 0x0480a, "es-HN" },
                        { 0x0080a, "es-MX" },
                        { 0x04c0a, "es-NI" },
                        { 0x0180a, "es-PA" },
                        { 0x0280a, "es-PE" },
                        /* { 0x01000, "es-PH" },*/
                        { 0x0500a, "es-PR" },
                        { 0x03c0a, "es-PY" },
                        { 0x0440a, "es-SV" },
                        { 0x0540a, "es-US" },
                        { 0x0380a, "es-UY" },
                        { 0x0200a, "es-VE" },
                        { 0x00425, "et-EE" },
                        { 0x0042d, "eu-ES" },
                        /* { 0x01000, "ewo-CM" },*/
                        { 0x00429, "fa-IR" },
                        /* { 0x01000, "ff-CM" },*/
                        /* { 0x01000, "ff-GN" },*/
                        { 0x00867, "ff-Latn-SN" },
                        /* { 0x01000, "ff-MR" },*/
                        { 0x00467, "ff-NG" },
                        { 0x0040b, "fi-FI" },
                        { 0x00464, "fil-PH" },
                        /* { 0x01000, "fo-DK" },*/
                        { 0x00438, "fo-FO" },
                        { 0x01c0c, "fr-029" },
                        { 0x0080c, "fr-BE" },
                        /* { 0x01000, "fr-BF" },*/
                        /* { 0x01000, "fr-BI" },*/
                        /* { 0x01000, "fr-BJ" },*/
                        /* { 0x01000, "fr-BL" },*/
                        { 0x00c0c, "fr-CA" },
                        { 0x0240c, "fr-CD" },
                        /* { 0x01000, "fr-CF" },*/
                        /* { 0x01000, "fr-CG" },*/
                        { 0x0100c, "fr-CH" },
                        { 0x0300c, "fr-CI" },
                        { 0x02c0c, "fr-CM" },
                        /* { 0x01000, "fr-DJ" },*/
                        /* { 0x01000, "fr-DZ" },*/
                        { 0x0040c, "fr-FR" },
                        /* { 0x01000, "fr-GA" },*/
                        /* { 0x01000, "fr-GF" },*/
                        /* { 0x01000, "fr-GN" },*/
                        /* { 0x01000, "fr-GP" },*/
                        /* { 0x01000, "fr-GQ" },*/
                        { 0x03c0c, "fr-HT" },
                        /* { 0x01000, "fr-KM" },*/
                        { 0x0140c, "fr-LU" },
                        { 0x0380c, "fr-MA" },
                        { 0x0180c, "fr-MC" },
                        /* { 0x01000, "fr-MF" },*/
                        /* { 0x01000, "fr-MG" },*/
                        { 0x0340c, "fr-ML" },
                        /* { 0x01000, "fr-MQ" },*/
                        /* { 0x01000, "fr-MR" },*/
                        /* { 0x01000, "fr-MU" },*/
                        /* { 0x01000, "fr-NC" },*/
                        /* { 0x01000, "fr-NE" },*/
                        /* { 0x01000, "fr-PF" },*/
                        /* { 0x01000, "fr-PM" },*/
                        { 0x0200c, "fr-RE" },
                        /* { 0x01000, "fr-RW" },*/
                        /* { 0x01000, "fr-SC" },*/
                        { 0x0280c, "fr-SN" },
                        /* { 0x01000, "fr-SY" },*/
                        /* { 0x01000, "fr-TD" },*/
                        /* { 0x01000, "fr-TG" },*/
                        /* { 0x01000, "fr-TN" },*/
                        /* { 0x01000, "fr-VU" },*/
                        /* { 0x01000, "fr-WF" },*/
                        /* { 0x01000, "fr-YT" },*/
                        /* { 0x01000, "fur-IT" },*/
                        { 0x00462, "fy-NL" },
                        { 0x0083c, "ga-IE" },
                        { 0x00491, "gd-GB" },
                        { 0x00456, "gl-ES" },
                        { 0x00474, "gn-PY" },
                        /* { 0x01000, "gsw-CH" },*/
                        { 0x00484, "gsw-FR" },
                        /* { 0x01000, "gsw-LI" },*/
                        { 0x00447, "gu-IN" },
                        /* { 0x01000, "guz-KE" },*/
                        /* { 0x01000, "gv-IM" },*/
                        /* { 0x01000, "ha-Latn-GH" },*/
                        /* { 0x01000, "ha-Latn-NE" },*/
                        { 0x00468, "ha-Latn-NG" },
                        { 0x00475, "haw-US" },
                        { 0x0040d, "he-IL" },
                        { 0x00439, "hi-IN" },
                        { 0x0101a, "hr-BA" },
                        { 0x0041a, "hr-HR" },
                        { 0x0042e, "hsb-DE" },
                        { 0x0040e, "hu-HU" },
                        { 0x0042b, "hy-AM" },
                        /* { 0x01000, "ia-001" },*/
                        /* { 0x01000, "ia-FR" },*/
                        { 0x00469, "ibb-NG" },
                        { 0x00421, "id-ID" },
                        { 0x00470, "ig-NG" },
                        { 0x00478, "ii-CN" },
                        { 0x0040f, "is-IS" },
                        { 0x00810, "it-CH" },
                        { 0x00410, "it-IT" },
                        /* { 0x01000, "it-SM" },*/
                        { 0x0045d, "iu-Cans-CA" },
                        { 0x0085d, "iu-Latn-CA" },
                        { 0x00411, "ja-JP" },
                        /* { 0x01000, "jgo-CM" },*/
                        /* { 0x01000, "jmc-TZ" },*/
                        /* { 0x01000, "jv-Java-ID" },*/
                        /* { 0x01000, "jv-Latn-ID" },*/
                        { 0x00437, "ka-GE" },
                        /* { 0x01000, "kab-DZ" },*/
                        /* { 0x01000, "kam-KE" },*/
                        /* { 0x01000, "kde-TZ" },*/
                        /* { 0x01000, "kea-CV" },*/
                        /* { 0x01000, "khq-ML" },*/
                        /* { 0x01000, "ki-KE" },*/
                        { 0x0043f, "kk-KZ" },
                        /* { 0x01000, "kkj-CM" },*/
                        { 0x0046f, "kl-GL" },
                        /* { 0x01000, "kln-KE" },*/
                        { 0x00453, "km-KH" },
                        { 0x0044b, "kn-IN" },
                        /* { 0x01000, "ko-KP" },*/
                        { 0x00412, "ko-KR" },
                        { 0x00457, "kok-IN" },
                        { 0x00471, "kr-NG" },
                        /* { 0x01000, "ks-Arab-IN" },*/
                        { 0x00860, "ks-Deva-IN" },
                        /* { 0x01000, "ksb-TZ" },*/
                        /* { 0x01000, "ksf-CM" },*/
                        /* { 0x01000, "ksh-DE" },*/
                        { 0x00492, "ku-Arab-IQ" },
                        /* { 0x01000, "ku-Arab-IR" },*/
                        /* { 0x01000, "kw-GB" },*/
                        { 0x00440, "ky-KG" },
                        { 0x00476, "la-001" },
                        /* { 0x01000, "lag-TZ" },*/
                        { 0x0046e, "lb-LU" },
                        /* { 0x01000, "lg-UG" },*/
                        /* { 0x01000, "lkt-US" },*/
                        /* { 0x01000, "ln-AO" },*/
                        /* { 0x01000, "ln-CD" },*/
                        /* { 0x01000, "ln-CF" },*/
                        /* { 0x01000, "ln-CG" },*/
                        { 0x00454, "lo-LA" },
                        /* { 0x01000, "lrc-IQ" },*/
                        /* { 0x01000, "lrc-IR" },*/
                        { 0x00427, "lt-LT" },
                        /* { 0x01000, "lu-CD" },*/
                        /* { 0x01000, "luo-KE" },*/
                        /* { 0x01000, "luy-KE" },*/
                        { 0x00426, "lv-LV" },
                        /* { 0x01000, "mas-KE" },*/
                        /* { 0x01000, "mas-TZ" },*/
                        /* { 0x01000, "mer-KE" },*/
                        /* { 0x01000, "mfe-MU" },*/
                        /* { 0x01000, "mg-MG" },*/
                        /* { 0x01000, "mgh-MZ" },*/
                        /* { 0x01000, "mgo-CM" },*/
                        { 0x00481, "mi-NZ" },
                        { 0x0042f, "mk-MK" },
                        { 0x0044c, "ml-IN" },
                        { 0x00450, "mn-MN" },
                        { 0x00850, "mn-Mong-CN" },
                        { 0x00c50, "mn-Mong-MN" },
                        { 0x00458, "mni-IN" },
                        { 0x0047c, "moh-CA" },
                        { 0x0044e, "mr-IN" },
                        { 0x0083e, "ms-BN" },
                        { 0x0043e, "ms-MY" },
                        /* { 0x01000, "ms-SG" },*/
                        { 0x0043a, "mt-MT" },
                        /* { 0x01000, "mua-CM" },*/
                        { 0x00455, "my-MM" },
                        /* { 0x01000, "mzn-IR" },*/
                        /* { 0x01000, "naq-NA" },*/
                        { 0x00414, "nb-NO" },
                        /* { 0x01000, "nb-SJ" },*/
                        /* { 0x01000, "nd-ZW" },*/
                        /* { 0x01000, "nds-DE" },*/
                        /* { 0x01000, "nds-NL" },*/
                        { 0x00861, "ne-IN" },
                        { 0x00461, "ne-NP" },
                        /* { 0x01000, "nl-AW" },*/
                        { 0x00813, "nl-BE" },
                        /* { 0x01000, "nl-BQ" },*/
                        /* { 0x01000, "nl-CW" },*/
                        { 0x00413, "nl-NL" },
                        /* { 0x01000, "nl-SR" },*/
                        /* { 0x01000, "nl-SX" },*/
                        /* { 0x01000, "nmg-CM" },*/
                        { 0x00814, "nn-NO" },
                        /* { 0x01000, "nnh-CM" },*/
                        /* { 0x01000, "nqo-GN" },*/
                        /* { 0x01000, "nr-ZA" },*/
                        { 0x0046c, "nso-ZA" },
                        /* { 0x01000, "nus-SS" },*/
                        /* { 0x01000, "nyn-UG" },*/
                        { 0x00482, "oc-FR" },
                        { 0x00472, "om-ET" },
                        /* { 0x01000, "om-KE" },*/
                        { 0x00448, "or-IN" },
                        /* { 0x01000, "os-GE" },*/
                        /* { 0x01000, "os-RU" },*/
                        { 0x00846, "pa-Arab-PK" },
                        { 0x00446, "pa-IN" },
                        { 0x00479, "pap-029" },
                        { 0x00415, "pl-PL" },
                        /* { 0x01000, "prg-001" },*/
                        { 0x0048c, "prs-AF" },
                        { 0x00463, "ps-AF" },
                        /* { 0x01000, "pt-AO" },*/
                        { 0x00416, "pt-BR" },
                        /* { 0x01000, "pt-CH" },*/
                        /* { 0x01000, "pt-CV" },*/
                        /* { 0x01000, "pt-GQ" },*/
                        /* { 0x01000, "pt-GW" },*/
                        /* { 0x01000, "pt-LU" },*/
                        /* { 0x01000, "pt-MO" },*/
                        /* { 0x01000, "pt-MZ" },*/
                        { 0x00816, "pt-PT" },
                        /* { 0x01000, "pt-ST" },*/
                        /* { 0x01000, "pt-TL" },*/
                        { 0x00901, "qps-Latn-x-sh" },
                        { 0x00501, "qps-ploc" },
                        { 0x005fe, "qps-ploca" },
                        { 0x009ff, "qps-plocm" },
                        { 0x00486, "quc-Latn-GT" },
                        { 0x0046b, "quz-BO" },
                        { 0x0086b, "quz-EC" },
                        { 0x00c6b, "quz-PE" },
                        { 0x00417, "rm-CH" },
                        /* { 0x01000, "rn-BI" },*/
                        { 0x00818, "ro-MD" },
                        { 0x00418, "ro-RO" },
                        /* { 0x01000, "rof-TZ" },*/
                        /* { 0x01000, "ru-BY" },*/
                        /* { 0x01000, "ru-KG" },*/
                        /* { 0x01000, "ru-KZ" },*/
                        { 0x00819, "ru-MD" },
                        { 0x00419, "ru-RU" },
                        /* { 0x01000, "ru-UA" },*/
                        { 0x00487, "rw-RW" },
                        /* { 0x01000, "rwk-TZ" },*/
                        { 0x0044f, "sa-IN" },
                        { 0x00485, "sah-RU" },
                        /* { 0x01000, "saq-KE" },*/
                        /* { 0x01000, "sbp-TZ" },*/
                        { 0x00859, "sd-Arab-PK" },
                        { 0x00459, "sd-Deva-IN" },
                        { 0x00c3b, "se-FI" },
                        { 0x0043b, "se-NO" },
                        { 0x0083b, "se-SE" },
                        /* { 0x01000, "seh-MZ" },*/
                        /* { 0x01000, "ses-ML" },*/
                        /* { 0x01000, "sg-CF" },*/
                        /* { 0x01000, "shi-Latn-MA" },*/
                        /* { 0x01000, "shi-Tfng-MA" },*/
                        { 0x0045b, "si-LK" },
                        { 0x0041b, "sk-SK" },
                        { 0x00424, "sl-SI" },
                        { 0x0183b, "sma-NO" },
                        { 0x01c3b, "sma-SE" },
                        { 0x0103b, "smj-NO" },
                        { 0x0143b, "smj-SE" },
                        { 0x0243b, "smn-FI" },
                        { 0x0203b, "sms-FI" },
                        /* { 0x01000, "sn-Latn-ZW" },*/
                        /* { 0x01000, "so-DJ" },*/
                        /* { 0x01000, "so-ET" },*/
                        /* { 0x01000, "so-KE" },*/
                        { 0x00477, "so-SO" },
                        { 0x0041c, "sq-AL" },
                        /* { 0x01000, "sq-MK" },*/
                        /* { 0x01000, "sq-XK" },*/
                        { 0x01c1a, "sr-Cyrl-BA" },
                        { 0x00c1a, "sr-Cyrl-CS" },
                        { 0x0301a, "sr-Cyrl-ME" },
                        { 0x0281a, "sr-Cyrl-RS" },
                        /* { 0x01000, "sr-Cyrl-XK" },*/
                        { 0x0181a, "sr-Latn-BA" },
                        { 0x0081a, "sr-Latn-CS" },
                        { 0x02c1a, "sr-Latn-ME" },
                        { 0x0241a, "sr-Latn-RS" },
                        /* { 0x01000, "sr-Latn-XK" },*/
                        /* { 0x01000, "ss-SZ" },*/
                        /* { 0x01000, "ss-ZA" },*/
                        /* { 0x01000, "ssy-ER" },*/
                        /* { 0x01000, "st-LS" },*/
                        { 0x00430, "st-ZA" },
                        /* { 0x01000, "sv-AX" },*/
                        { 0x0081d, "sv-FI" },
                        { 0x0041d, "sv-SE" },
                        /* { 0x01000, "sw-CD" },*/
                        { 0x00441, "sw-KE" },
                        /* { 0x01000, "sw-TZ" },*/
                        /* { 0x01000, "sw-UG" },*/
                        { 0x0045a, "syr-SY" },
                        { 0x00449, "ta-IN" },
                        { 0x00849, "ta-LK" },
                        /* { 0x01000, "ta-MY" },*/
                        /* { 0x01000, "ta-SG" },*/
                        { 0x0044a, "te-IN" },
                        /* { 0x01000, "teo-KE" },*/
                        /* { 0x01000, "teo-UG" },*/
                        { 0x00428, "tg-Cyrl-TJ" },
                        { 0x0041e, "th-TH" },
                        { 0x00873, "ti-ER" },
                        { 0x00473, "ti-ET" },
                        /* { 0x01000, "tig-ER" },*/
                        { 0x00442, "tk-TM" },
                        { 0x00832, "tn-BW" },
                        { 0x00432, "tn-ZA" },
                        /* { 0x01000, "to-TO" },*/
                        /* { 0x01000, "tr-CY" },*/
                        { 0x0041f, "tr-TR" },
                        { 0x00431, "ts-ZA" },
                        { 0x00444, "tt-RU" },
                        /* { 0x01000, "twq-NE" },*/
                        { 0x0045f, "tzm-Arab-MA" },
                        { 0x0085f, "tzm-Latn-DZ" },
                        /* { 0x01000, "tzm-Latn-MA" },*/
                        { 0x0105f, "tzm-Tfng-MA" },
                        { 0x00480, "ug-CN" },
                        { 0x00422, "uk-UA" },
                        { 0x00820, "ur-IN" },
                        { 0x00420, "ur-PK" },
                        /* { 0x01000, "uz-Arab-AF" },*/
                        { 0x00843, "uz-Cyrl-UZ" },
                        { 0x00443, "uz-Latn-UZ" },
                        /* { 0x01000, "vai-Latn-LR" },*/
                        /* { 0x01000, "vai-Vaii-LR" },*/
                        { 0x00433, "ve-ZA" },
                        { 0x0042a, "vi-VN" },
                        /* { 0x01000, "vo-001" },*/
                        /* { 0x01000, "vun-TZ" },*/
                        /* { 0x01000, "wae-CH" },*/
                        /* { 0x01000, "wal-ET" },*/
                        { 0x00488, "wo-SN" },
                        { 0x00434, "xh-ZA" },
                        /* { 0x01000, "xog-UG" },*/
                        /* { 0x01000, "yav-CM" },*/
                        { 0x0043d, "yi-001" },
                        /* { 0x01000, "yo-BJ" },*/
                        { 0x0046a, "yo-NG" },
                        /* { 0x01000, "yue-HK" },*/
                        /* { 0x01000, "zgh-Tfng-MA" },*/
                        { 0x00804, "zh-CN" },
                        { 0x00c04, "zh-HK" },
                        /* { 0x01000, "zh-Hans-HK" },*/
                        /* { 0x01000, "zh-Hans-MO" },*/
                        { 0x01404, "zh-MO" },
                        { 0x01004, "zh-SG" },
                        { 0x00404, "zh-TW" },
                        { 0x00435, "zu-ZA" },
                        /* { 0x01000, "aa" },*/
                        { 0x00036, "af" },
                        /* { 0x01000, "agq" },*/
                        /* { 0x01000, "ak" },*/
                        { 0x0005e, "am" },
                        { 0x00001, "ar" },
                        { 0x0007a, "arn" },
                        { 0x0004d, "as" },
                        /* { 0x01000, "asa" },*/
                        /* { 0x01000, "ast" },*/
                        { 0x0002c, "az" },
                        { 0x0742c, "az-Cyrl" },
                        { 0x0782c, "az-Latn" },
                        { 0x0006d, "ba" },
                        /* { 0x01000, "bas" },*/
                        { 0x00023, "be" },
                        /* { 0x01000, "bem" },*/
                        /* { 0x01000, "bez" },*/
                        { 0x00002, "bg" },
                        { 0x00066, "bin" },
                        /* { 0x01000, "bm" },*/
                        /* { 0x01000, "bm-Latn" },*/
                        { 0x00045, "bn" },
                        { 0x00051, "bo" },
                        { 0x0007e, "br" },
                        /* { 0x01000, "brx" },*/
                        { 0x0781a, "bs" },
                        { 0x0641a, "bs-Cyrl" },
                        { 0x0681a, "bs-Latn" },
                        /* { 0x01000, "byn" },*/
                        { 0x00003, "ca" },
                        /* { 0x01000, "cgg" },*/
                        { 0x0005c, "chr" },
                        { 0x07c5c, "chr-Cher" },
                        { 0x00083, "co" },
                        { 0x00005, "cs" },
                        { 0x00052, "cy" },
                        { 0x00006, "da" },
                        /* { 0x01000, "dav" },*/
                        { 0x00007, "de" },
                        /* { 0x01000, "dje" },*/
                        { 0x07c2e, "dsb" },
                        /* { 0x01000, "dua" },*/
                        { 0x00065, "dv" },
                        /* { 0x01000, "dyo" },*/
                        /* { 0x01000, "dz" },*/
                        /* { 0x01000, "ebu" },*/
                        /* { 0x01000, "ee" },*/
                        { 0x00008, "el" },
                        { 0x00009, "en" },
                        /* { 0x01000, "eo" },*/
                        { 0x0000a, "es" },
                        { 0x00025, "et" },
                        { 0x0002d, "eu" },
                        /* { 0x01000, "ewo" },*/
                        { 0x00029, "fa" },
                        { 0x00067, "ff" },
                        { 0x07c67, "ff-Latn" },
                        { 0x0000b, "fi" },
                        { 0x00064, "fil" },
                        { 0x00038, "fo" },
                        { 0x0000c, "fr" },
                        /* { 0x01000, "fur" },*/
                        { 0x00062, "fy" },
                        { 0x0003c, "ga" },
                        { 0x00091, "gd" },
                        { 0x00056, "gl" },
                        { 0x00074, "gn" },
                        { 0x00084, "gsw" },
                        { 0x00047, "gu" },
                        /* { 0x01000, "guz" },*/
                        /* { 0x01000, "gv" },*/
                        { 0x00068, "ha" },
                        { 0x07c68, "ha-Latn" },
                        { 0x00075, "haw" },
                        { 0x0000d, "he" },
                        { 0x00039, "hi" },
                        { 0x0001a, "hr" },
                        { 0x0002e, "hsb" },
                        { 0x0000e, "hu" },
                        { 0x0002b, "hy" },
                        /* { 0x01000, "ia" },*/
                        { 0x00069, "ibb" },
                        { 0x00021, "id" },
                        { 0x00070, "ig" },
                        { 0x00078, "ii" },
                        { 0x0000f, "is" },
                        { 0x00010, "it" },
                        { 0x0005d, "iu" },
                        { 0x0785d, "iu-Cans" },
                        { 0x07c5d, "iu-Latn" },
                        { 0x00011, "ja" },
                        /* { 0x01000, "jgo" },*/
                        /* { 0x01000, "jmc" },*/
                        /* { 0x01000, "jv" },*/
                        /* { 0x01000, "jv-Java" },*/
                        /* { 0x01000, "jv-Latn" },*/
                        { 0x00037, "ka" },
                        /* { 0x01000, "kab" },*/
                        /* { 0x01000, "kam" },*/
                        /* { 0x01000, "kde" },*/
                        /* { 0x01000, "kea" },*/
                        /* { 0x01000, "khq" },*/
                        /* { 0x01000, "ki" },*/
                        { 0x0003f, "kk" },
                        /* { 0x01000, "kkj" },*/
                        { 0x0006f, "kl" },
                        /* { 0x01000, "kln" },*/
                        { 0x00053, "km" },
                        { 0x0004b, "kn" },
                        { 0x00012, "ko" },
                        { 0x00057, "kok" },
                        { 0x00071, "kr" },
                        { 0x00060, "ks" },
                        { 0x00460, "ks-Arab" },
                        /* { 0x01000, "ks-Deva" },*/
                        /* { 0x01000, "ksb" },*/
                        /* { 0x01000, "ksf" },*/
                        /* { 0x01000, "ksh" },*/
                        { 0x00092, "ku" },
                        { 0x07c92, "ku-Arab" },
                        /* { 0x01000, "kw" },*/
                        { 0x00040, "ky" },
                        { 0x00076, "la" },
                        /* { 0x01000, "lag" },*/
                        { 0x0006e, "lb" },
                        /* { 0x01000, "lg" },*/
                        /* { 0x01000, "lkt" },*/
                        /* { 0x01000, "ln" },*/
                        { 0x00054, "lo" },
                        { 0x00027, "lt" },
                        /* { 0x01000, "lu" },*/
                        /* { 0x01000, "luo" },*/
                        /* { 0x01000, "luy" },*/
                        { 0x00026, "lv" },
                        /* { 0x01000, "mas" },*/
                        /* { 0x01000, "mer" },*/
                        /* { 0x01000, "mfe" },*/
                        /* { 0x01000, "mg" },*/
                        /* { 0x01000, "mgh" },*/
                        /* { 0x01000, "mgo" },*/
                        { 0x00081, "mi" },
                        { 0x0002f, "mk" },
                        { 0x0004c, "ml" },
                        { 0x00050, "mn" },
                        { 0x07850, "mn-Cyrl" },
                        { 0x07c50, "mn-Mong" },
                        { 0x00058, "mni" },
                        { 0x0007c, "moh" },
                        { 0x0004e, "mr" },
                        { 0x0003e, "ms" },
                        { 0x0003a, "mt" },
                        /* { 0x01000, "mua" },*/
                        { 0x00055, "my" },
                        /* { 0x01000, "naq" },*/
                        { 0x07c14, "nb" },
                        /* { 0x01000, "nd" },*/
                        { 0x00061, "ne" },
                        { 0x00013, "nl" },
                        /* { 0x01000, "nmg" },*/
                        { 0x07814, "nn" },
                        /* { 0x01000, "nnh" },*/
                        { 0x00014, "no" },
                        /* { 0x01000, "nqo" },*/
                        /* { 0x01000, "nr" },*/
                        { 0x0006c, "nso" },
                        /* { 0x01000, "nus" },*/
                        /* { 0x01000, "nyn" },*/
                        { 0x00082, "oc" },
                        { 0x00072, "om" },
                        { 0x00048, "or" },
                        /* { 0x01000, "os" },*/
                        { 0x00046, "pa" },
                        { 0x07c46, "pa-Arab" },
                        { 0x00079, "pap" },
                        { 0x00015, "pl" },
                        { 0x0008c, "prs" },
                        { 0x00063, "ps" },
                        { 0x00016, "pt" },
                        { 0x00086, "quc" },
                        { 0x07c86, "quc-Latn" },
                        { 0x0006b, "quz" },
                        { 0x00017, "rm" },
                        /* { 0x01000, "rn" },*/
                        { 0x00018, "ro" },
                        /* { 0x01000, "rof" },*/
                        { 0x00019, "ru" },
                        { 0x00087, "rw" },
                        /* { 0x01000, "rwk" },*/
                        { 0x0004f, "sa" },
                        { 0x00085, "sah" },
                        /* { 0x01000, "saq" },*/
                        /* { 0x01000, "sbp" },*/
                        { 0x00059, "sd" },
                        { 0x07c59, "sd-Arab" },
                        /* { 0x01000, "sd-Deva" },*/
                        { 0x0003b, "se" },
                        /* { 0x01000, "seh" },*/
                        /* { 0x01000, "ses" },*/
                        /* { 0x01000, "sg" },*/
                        /* { 0x01000, "shi" },*/
                        /* { 0x01000, "shi-Latn" },*/
                        /* { 0x01000, "shi-Tfng" },*/
                        { 0x0005b, "si" },
                        { 0x0001b, "sk" },
                        { 0x00024, "sl" },
                        { 0x0783b, "sma" },
                        { 0x07c3b, "smj" },
                        { 0x0703b, "smn" },
                        { 0x0743b, "sms" },
                        /* { 0x01000, "sn" },*/
                        /* { 0x01000, "sn-Latn" },*/
                        { 0x00077, "so" },
                        { 0x0001c, "sq" },
                        { 0x07c1a, "sr" },
                        { 0x06c1a, "sr-Cyrl" },
                        { 0x0701a, "sr-Latn" },
                        /* { 0x01000, "ss" },*/
                        /* { 0x01000, "ssy" },*/
                        { 0x00030, "st" },
                        { 0x0001d, "sv" },
                        { 0x00041, "sw" },
                        /* { 0x01000, "swc" },*/
                        /* { 0x01000, "swc-CD" },*/
                        { 0x0005a, "syr" },
                        { 0x00049, "ta" },
                        { 0x0004a, "te" },
                        /* { 0x01000, "teo" },*/
                        { 0x00028, "tg" },
                        { 0x07c28, "tg-Cyrl" },
                        { 0x0001e, "th" },
                        { 0x00073, "ti" },
                        /* { 0x01000, "tig" },*/
                        { 0x00042, "tk" },
                        { 0x00032, "tn" },
                        /* { 0x01000, "to" },*/
                        { 0x0001f, "tr" },
                        { 0x00031, "ts" },
                        { 0x00044, "tt" },
                        /* { 0x01000, "twq" },*/
                        { 0x0005f, "tzm" },
                        /* { 0x01000, "tzm-Arab" },*/
                        { 0x07c5f, "tzm-Latn" },
                        { 0x0785f, "tzm-Tfng" },
                        { 0x00080, "ug" },
                        { 0x00022, "uk" },
                        { 0x00020, "ur" },
                        { 0x00043, "uz" },
                        /* { 0x01000, "uz-Arab" },*/
                        { 0x07843, "uz-Cyrl" },
                        { 0x07c43, "uz-Latn" },
                        /* { 0x01000, "vai" },*/
                        /* { 0x01000, "vai-Latn" },*/
                        /* { 0x01000, "vai-Vaii" },*/
                        { 0x00033, "ve" },
                        { 0x0002a, "vi" },
                        /* { 0x01000, "vo" },*/
                        /* { 0x01000, "vun" },*/
                        /* { 0x01000, "wae" },*/
                        /* { 0x01000, "wal" },*/
                        { 0x00088, "wo" },
                        { 0x00034, "xh" },
                        /* { 0x01000, "xog" },*/
                        /* { 0x01000, "yav" },*/
                        { 0x0003d, "yi" },
                        { 0x0006a, "yo" },
                        /* { 0x01000, "zgh" },*/
                        /* { 0x01000, "zgh-Tfng" },*/
                        { 0x07804, "zh" },
                        { 0x00004, "zh-Hans" },
                        { 0x07c04, "zh-Hant" },
                        { 0x00035, "zu" },
                        { 0x10407, "de-DE_phoneb" },
                        { 0x1040e, "hu-HU_technl" },
                        { 0x40411, "ja-JP_radstr" },
                        { 0x10437, "ka-GE_modern" },
                        { 0x1007f, "x-IV_mathan" },
                        { 0x50804, "zh-CN_phoneb" },
                        { 0x20804, "zh-CN_stroke" },
                        { 0x40c04, "zh-HK_radstr" },
                        { 0x41404, "zh-MO_radstr" },
                        { 0x21404, "zh-MO_stroke" },
                        { 0x51004, "zh-SG_phoneb" },
                        { 0x21004, "zh-SG_stroke" },
                        { 0x30404, "zh-TW_pronun" },
                        { 0x40404, "zh-TW_radstr" }
                    };
                    s_lcidToCultureNameDictionary = tempMapping;
                }
                return s_lcidToCultureNameDictionary;
            }
        }
    }
}
