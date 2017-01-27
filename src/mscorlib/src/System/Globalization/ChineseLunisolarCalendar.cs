// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.Globalization {
    using System;
    using System.Diagnostics.Contracts;

    ////////////////////////////////////////////////////////////////////////////
    //
    //  Notes about ChineseLunisolarCalendar
    //
    ////////////////////////////////////////////////////////////////////////////
     /*
     **  Calendar support range:
     **      Calendar               Minimum             Maximum
     **      ==========     ==========  ==========
     **      Gregorian              1901/02/19          2101/01/28
     **      ChineseLunisolar   1901/01/01          2100/12/29
     */

    [Serializable]
    public class ChineseLunisolarCalendar : EastAsianLunisolarCalendar {


        //
        // The era value for the current era.
        //

        public const int ChineseEra = 1;
        //internal static Calendar m_defaultInstance;

        internal const int MIN_LUNISOLAR_YEAR = 1901;
        internal const int MAX_LUNISOLAR_YEAR = 2100;

        internal const int MIN_GREGORIAN_YEAR = 1901;
        internal const int MIN_GREGORIAN_MONTH = 2;
        internal const int MIN_GREGORIAN_DAY = 19;

        internal const int MAX_GREGORIAN_YEAR = 2101;
        internal const int MAX_GREGORIAN_MONTH = 1;
        internal const int MAX_GREGORIAN_DAY = 28;

        internal static DateTime minDate = new DateTime(MIN_GREGORIAN_YEAR, MIN_GREGORIAN_MONTH, MIN_GREGORIAN_DAY);
        internal static DateTime maxDate = new DateTime((new DateTime(MAX_GREGORIAN_YEAR, MAX_GREGORIAN_MONTH, MAX_GREGORIAN_DAY, 23, 59, 59, 999)).Ticks + 9999);

        [System.Runtime.InteropServices.ComVisible(false)]
        public override DateTime MinSupportedDateTime  {
            get
            {
                return (minDate);
            }
        }


        [System.Runtime.InteropServices.ComVisible(false)]
        public override DateTime MaxSupportedDateTime {
            get
            {
                return (maxDate);
            }
        }

        protected override int DaysInYearBeforeMinSupportedYear
        {
            get
            {
                // 1900: 1-29 2-30 3-29 4-29 5-30 6-29 7-30 8-30 Leap8-29 9-30 10-30 11-29 12-30 from Calendrical Tabulations
                return 384;
            }
        }


        static readonly int  [,] yinfo =
        {
 /*Y            LM        Lmon    Lday        DaysPerMonth    D1    D2    D3    D4    D5    D6    D7    D8    D9    D10    D11    D12    D13    #Days
1901    */{    0    ,    2    ,    19    ,    19168    },/*    29    30    29    29    30    29    30    29    30    30    30    29    0    354
1902    */{    0    ,    2    ,    8    ,    42352    },/*    30    29    30    29    29    30    29    30    29    30    30    30    0    355
1903    */{    5    ,    1    ,    29    ,    21096    },/*    29    30    29    30    29    29    30    29    29    30    30    29    30    383
1904    */{    0    ,    2    ,    16    ,    53856    },/*    30    30    29    30    29    29    30    29    29    30    30    29    0    354
1905    */{    0    ,    2    ,    4    ,    55632    },/*    30    30    29    30    30    29    29    30    29    30    29    30    0    355
1906    */{    4    ,    1    ,    25    ,    27304    },/*    29    30    30    29    30    29    30    29    30    29    30    29    30    384
1907    */{    0    ,    2    ,    13    ,    22176    },/*    29    30    29    30    29    30    30    29    30    29    30    29    0    354
1908    */{    0    ,    2    ,    2    ,    39632    },/*    30    29    29    30    30    29    30    29    30    30    29    30    0    355
1909    */{    2    ,    1    ,    22    ,    19176    },/*    29    30    29    29    30    29    30    29    30    30    30    29    30    384
1910    */{    0    ,    2    ,    10    ,    19168    },/*    29    30    29    29    30    29    30    29    30    30    30    29    0    354
1911    */{    6    ,    1    ,    30    ,    42200    },/*    30    29    30    29    29    30    29    29    30    30    29    30    30    384
1912    */{    0    ,    2    ,    18    ,    42192    },/*    30    29    30    29    29    30    29    29    30    30    29    30    0    354
1913    */{    0    ,    2    ,    6    ,    53840    },/*    30    30    29    30    29    29    30    29    29    30    29    30    0    354
1914    */{    5    ,    1    ,    26    ,    54568    },/*    30    30    29    30    29    30    29    30    29    29    30    29    30    384
1915    */{    0    ,    2    ,    14    ,    46400    },/*    30    29    30    30    29    30    29    30    29    30    29    29    0    354
1916    */{    0    ,    2    ,    3    ,    54944    },/*    30    30    29    30    29    30    30    29    30    29    30    29    0    355
1917    */{    2    ,    1    ,    23    ,    38608    },/*    30    29    29    30    29    30    30    29    30    30    29    30    29    384
1918    */{    0    ,    2    ,    11    ,    38320    },/*    30    29    29    30    29    30    29    30    30    29    30    30    0    355
1919    */{    7    ,    2    ,    1    ,    18872    },/*    29    30    29    29    30    29    29    30    30    29    30    30    30    384
1920    */{    0    ,    2    ,    20    ,    18800    },/*    29    30    29    29    30    29    29    30    29    30    30    30    0    354
1921    */{    0    ,    2    ,    8    ,    42160    },/*    30    29    30    29    29    30    29    29    30    29    30    30    0    354
1922    */{    5    ,    1    ,    28    ,    45656    },/*    30    29    30    30    29    29    30    29    29    30    29    30    30    384
1923    */{    0    ,    2    ,    16    ,    27216    },/*    29    30    30    29    30    29    30    29    29    30    29    30    0    354
1924    */{    0    ,    2    ,    5    ,    27968    },/*    29    30    30    29    30    30    29    30    29    30    29    29    0    354
1925    */{    4    ,    1    ,    24    ,    44456    },/*    30    29    30    29    30    30    29    30    30    29    30    29    30    385
1926    */{    0    ,    2    ,    13    ,    11104    },/*    29    29    30    29    30    29    30    30    29    30    30    29    0    354
1927    */{    0    ,    2    ,    2    ,    38256    },/*    30    29    29    30    29    30    29    30    29    30    30    30    0    355
1928    */{    2    ,    1    ,    23    ,    18808    },/*    29    30    29    29    30    29    29    30    29    30    30    30    30    384
1929    */{    0    ,    2    ,    10    ,    18800    },/*    29    30    29    29    30    29    29    30    29    30    30    30    0    354
1930    */{    6    ,    1    ,    30    ,    25776    },/*    29    30    30    29    29    30    29    29    30    29    30    30    29    383
1931    */{    0    ,    2    ,    17    ,    54432    },/*    30    30    29    30    29    30    29    29    30    29    30    29    0    354
1932    */{    0    ,    2    ,    6    ,    59984    },/*    30    30    30    29    30    29    30    29    29    30    29    30    0    355
1933    */{    5    ,    1    ,    26    ,    27976    },/*    29    30    30    29    30    30    29    30    29    30    29    29    30    384
1934    */{    0    ,    2    ,    14    ,    23248    },/*    29    30    29    30    30    29    30    29    30    30    29    30    0    355
1935    */{    0    ,    2    ,    4    ,    11104    },/*    29    29    30    29    30    29    30    30    29    30    30    29    0    354
1936    */{    3    ,    1    ,    24    ,    37744    },/*    30    29    29    30    29    29    30    30    29    30    30    30    29    384
1937    */{    0    ,    2    ,    11    ,    37600    },/*    30    29    29    30    29    29    30    29    30    30    30    29    0    354
1938    */{    7    ,    1    ,    31    ,    51560    },/*    30    30    29    29    30    29    29    30    29    30    30    29    30    384
1939    */{    0    ,    2    ,    19    ,    51536    },/*    30    30    29    29    30    29    29    30    29    30    29    30    0    354
1940    */{    0    ,    2    ,    8    ,    54432    },/*    30    30    29    30    29    30    29    29    30    29    30    29    0    354
1941    */{    6    ,    1    ,    27    ,    55888    },/*    30    30    29    30    30    29    30    29    29    30    29    30    29    384
1942    */{    0    ,    2    ,    15    ,    46416    },/*    30    29    30    30    29    30    29    30    29    30    29    30    0    355
1943    */{    0    ,    2    ,    5    ,    22176    },/*    29    30    29    30    29    30    30    29    30    29    30    29    0    354
1944    */{    4    ,    1    ,    25    ,    43736    },/*    30    29    30    29    30    29    30    29    30    30    29    30    30    385
1945    */{    0    ,    2    ,    13    ,    9680    },/*    29    29    30    29    29    30    29    30    30    30    29    30    0    354
1946    */{    0    ,    2    ,    2    ,    37584    },/*    30    29    29    30    29    29    30    29    30    30    29    30    0    354
1947    */{    2    ,    1    ,    22    ,    51544    },/*    30    30    29    29    30    29    29    30    29    30    29    30    30    384
1948    */{    0    ,    2    ,    10    ,    43344    },/*    30    29    30    29    30    29    29    30    29    30    29    30    0    354
1949    */{    7    ,    1    ,    29    ,    46248    },/*    30    29    30    30    29    30    29    29    30    29    30    29    30    384
1950    */{    0    ,    2    ,    17    ,    27808    },/*    29    30    30    29    30    30    29    29    30    29    30    29    0    354
1951    */{    0    ,    2    ,    6    ,    46416    },/*    30    29    30    30    29    30    29    30    29    30    29    30    0    355
1952    */{    5    ,    1    ,    27    ,    21928    },/*    29    30    29    30    29    30    29    30    30    29    30    29    30    384
1953    */{    0    ,    2    ,    14    ,    19872    },/*    29    30    29    29    30    30    29    30    30    29    30    29    0    354
1954    */{    0    ,    2    ,    3    ,    42416    },/*    30    29    30    29    29    30    29    30    30    29    30    30    0    355
1955    */{    3    ,    1    ,    24    ,    21176    },/*    29    30    29    30    29    29    30    29    30    29    30    30    30    384
1956    */{    0    ,    2    ,    12    ,    21168    },/*    29    30    29    30    29    29    30    29    30    29    30    30    0    354
1957    */{    8    ,    1    ,    31    ,    43344    },/*    30    29    30    29    30    29    29    30    29    30    29    30    29    383
1958    */{    0    ,    2    ,    18    ,    59728    },/*    30    30    30    29    30    29    29    30    29    30    29    30    0    355
1959    */{    0    ,    2    ,    8    ,    27296    },/*    29    30    30    29    30    29    30    29    30    29    30    29    0    354
1960    */{    6    ,    1    ,    28    ,    44368    },/*    30    29    30    29    30    30    29    30    29    30    29    30    29    384
1961    */{    0    ,    2    ,    15    ,    43856    },/*    30    29    30    29    30    29    30    30    29    30    29    30    0    355
1962    */{    0    ,    2    ,    5    ,    19296    },/*    29    30    29    29    30    29    30    30    29    30    30    29    0    354
1963    */{    4    ,    1    ,    25    ,    42352    },/*    30    29    30    29    29    30    29    30    29    30    30    30    29    384
1964    */{    0    ,    2    ,    13    ,    42352    },/*    30    29    30    29    29    30    29    30    29    30    30    30    0    355
1965    */{    0    ,    2    ,    2    ,    21088    },/*    29    30    29    30    29    29    30    29    29    30    30    29    0    353
1966    */{    3    ,    1    ,    21    ,    59696    },/*    30    30    30    29    30    29    29    30    29    29    30    30    29    384
1967    */{    0    ,    2    ,    9    ,    55632    },/*    30    30    29    30    30    29    29    30    29    30    29    30    0    355
1968    */{    7    ,    1    ,    30    ,    23208    },/*    29    30    29    30    30    29    30    29    30    29    30    29    30    384
1969    */{    0    ,    2    ,    17    ,    22176    },/*    29    30    29    30    29    30    30    29    30    29    30    29    0    354
1970    */{    0    ,    2    ,    6    ,    38608    },/*    30    29    29    30    29    30    30    29    30    30    29    30    0    355
1971    */{    5    ,    1    ,    27    ,    19176    },/*    29    30    29    29    30    29    30    29    30    30    30    29    30    384
1972    */{    0    ,    2    ,    15    ,    19152    },/*    29    30    29    29    30    29    30    29    30    30    29    30    0    354
1973    */{    0    ,    2    ,    3    ,    42192    },/*    30    29    30    29    29    30    29    29    30    30    29    30    0    354
1974    */{    4    ,    1    ,    23    ,    53864    },/*    30    30    29    30    29    29    30    29    29    30    30    29    30    384
1975    */{    0    ,    2    ,    11    ,    53840    },/*    30    30    29    30    29    29    30    29    29    30    29    30    0    354
1976    */{    8    ,    1    ,    31    ,    54568    },/*    30    30    29    30    29    30    29    30    29    29    30    29    30    384
1977    */{    0    ,    2    ,    18    ,    46400    },/*    30    29    30    30    29    30    29    30    29    30    29    29    0    354
1978    */{    0    ,    2    ,    7    ,    46752    },/*    30    29    30    30    29    30    30    29    30    29    30    29    0    355
1979    */{    6    ,    1    ,    28    ,    38608    },/*    30    29    29    30    29    30    30    29    30    30    29    30    29    384
1980    */{    0    ,    2    ,    16    ,    38320    },/*    30    29    29    30    29    30    29    30    30    29    30    30    0    355
1981    */{    0    ,    2    ,    5    ,    18864    },/*    29    30    29    29    30    29    29    30    30    29    30    30    0    354
1982    */{    4    ,    1    ,    25    ,    42168    },/*    30    29    30    29    29    30    29    29    30    29    30    30    30    384
1983    */{    0    ,    2    ,    13    ,    42160    },/*    30    29    30    29    29    30    29    29    30    29    30    30    0    354
1984    */{    10    ,    2    ,    2    ,    45656    },/*    30    29    30    30    29    29    30    29    29    30    29    30    30    384
1985    */{    0    ,    2    ,    20    ,    27216    },/*    29    30    30    29    30    29    30    29    29    30    29    30    0    354
1986    */{    0    ,    2    ,    9    ,    27968    },/*    29    30    30    29    30    30    29    30    29    30    29    29    0    354
1987    */{    6    ,    1    ,    29    ,    44448    },/*    30    29    30    29    30    30    29    30    30    29    30    29    29    384
1988    */{    0    ,    2    ,    17    ,    43872    },/*    30    29    30    29    30    29    30    30    29    30    30    29    0    355
1989    */{    0    ,    2    ,    6    ,    38256    },/*    30    29    29    30    29    30    29    30    29    30    30    30    0    355
1990    */{    5    ,    1    ,    27    ,    18808    },/*    29    30    29    29    30    29    29    30    29    30    30    30    30    384
1991    */{    0    ,    2    ,    15    ,    18800    },/*    29    30    29    29    30    29    29    30    29    30    30    30    0    354
1992    */{    0    ,    2    ,    4    ,    25776    },/*    29    30    30    29    29    30    29    29    30    29    30    30    0    354
1993    */{    3    ,    1    ,    23    ,    27216    },/*    29    30    30    29    30    29    30    29    29    30    29    30    29    383
1994    */{    0    ,    2    ,    10    ,    59984    },/*    30    30    30    29    30    29    30    29    29    30    29    30    0    355
1995    */{    8    ,    1    ,    31    ,    27432    },/*    29    30    30    29    30    29    30    30    29    29    30    29    30    384
1996    */{    0    ,    2    ,    19    ,    23232    },/*    29    30    29    30    30    29    30    29    30    30    29    29    0    354
1997    */{    0    ,    2    ,    7    ,    43872    },/*    30    29    30    29    30    29    30    30    29    30    30    29    0    355
1998    */{    5    ,    1    ,    28    ,    37736    },/*    30    29    29    30    29    29    30    30    29    30    30    29    30    384
1999    */{    0    ,    2    ,    16    ,    37600    },/*    30    29    29    30    29    29    30    29    30    30    30    29    0    354
2000    */{    0    ,    2    ,    5    ,    51552    },/*    30    30    29    29    30    29    29    30    29    30    30    29    0    354
2001    */{    4    ,    1    ,    24    ,    54440    },/*    30    30    29    30    29    30    29    29    30    29    30    29    30    384
2002    */{    0    ,    2    ,    12    ,    54432    },/*    30    30    29    30    29    30    29    29    30    29    30    29    0    354
2003    */{    0    ,    2    ,    1    ,    55888    },/*    30    30    29    30    30    29    30    29    29    30    29    30    0    355
2004    */{    2    ,    1    ,    22    ,    23208    },/*    29    30    29    30    30    29    30    29    30    29    30    29    30    384
2005    */{    0    ,    2    ,    9    ,    22176    },/*    29    30    29    30    29    30    30    29    30    29    30    29    0    354
2006    */{    7    ,    1    ,    29    ,    43736    },/*    30    29    30    29    30    29    30    29    30    30    29    30    30    385
2007    */{    0    ,    2    ,    18    ,    9680    },/*    29    29    30    29    29    30    29    30    30    30    29    30    0    354
2008    */{    0    ,    2    ,    7    ,    37584    },/*    30    29    29    30    29    29    30    29    30    30    29    30    0    354
2009    */{    5    ,    1    ,    26    ,    51544    },/*    30    30    29    29    30    29    29    30    29    30    29    30    30    384
2010    */{    0    ,    2    ,    14    ,    43344    },/*    30    29    30    29    30    29    29    30    29    30    29    30    0    354
2011    */{    0    ,    2    ,    3    ,    46240    },/*    30    29    30    30    29    30    29    29    30    29    30    29    0    354
2012    */{    4    ,    1    ,    23    ,    46416    },/*    30    29    30    30    29    30    29    30    29    30    29    30    29    384
2013    */{    0    ,    2    ,    10    ,    44368    },/*    30    29    30    29    30    30    29    30    29    30    29    30    0    355
2014    */{    9    ,    1    ,    31    ,    21928    },/*    29    30    29    30    29    30    29    30    30    29    30    29    30    384
2015    */{    0    ,    2    ,    19    ,    19360    },/*    29    30    29    29    30    29    30    30    30    29    30    29    0    354
2016    */{    0    ,    2    ,    8    ,    42416    },/*    30    29    30    29    29    30    29    30    30    29    30    30    0    355
2017    */{    6    ,    1    ,    28    ,    21176    },/*    29    30    29    30    29    29    30    29    30    29    30    30    30    384
2018    */{    0    ,    2    ,    16    ,    21168    },/*    29    30    29    30    29    29    30    29    30    29    30    30    0    354
2019    */{    0    ,    2    ,    5    ,    43312    },/*    30    29    30    29    30    29    29    30    29    29    30    30    0    354
2020    */{    4    ,    1    ,    25    ,    29864    },/*    29    30    30    30    29    30    29    29    30    29    30    29    30    384
2021    */{    0    ,    2    ,    12    ,    27296    },/*    29    30    30    29    30    29    30    29    30    29    30    29    0    354
2022    */{    0    ,    2    ,    1    ,    44368    },/*    30    29    30    29    30    30    29    30    29    30    29    30    0    355
2023    */{    2    ,    1    ,    22    ,    19880    },/*    29    30    29    29    30    30    29    30    30    29    30    29    30    384
2024    */{    0    ,    2    ,    10    ,    19296    },/*    29    30    29    29    30    29    30    30    29    30    30    29    0    354
2025    */{    6    ,    1    ,    29    ,    42352    },/*    30    29    30    29    29    30    29    30    29    30    30    30    29    384
2026    */{    0    ,    2    ,    17    ,    42208    },/*    30    29    30    29    29    30    29    29    30    30    30    29    0    354
2027    */{    0    ,    2    ,    6    ,    53856    },/*    30    30    29    30    29    29    30    29    29    30    30    29    0    354
2028    */{    5    ,    1    ,    26    ,    59696    },/*    30    30    30    29    30    29    29    30    29    29    30    30    29    384
2029    */{    0    ,    2    ,    13    ,    54576    },/*    30    30    29    30    29    30    29    30    29    29    30    30    0    355
2030    */{    0    ,    2    ,    3    ,    23200    },/*    29    30    29    30    30    29    30    29    30    29    30    29    0    354
2031    */{    3    ,    1    ,    23    ,    27472    },/*    29    30    30    29    30    29    30    30    29    30    29    30    29    384
2032    */{    0    ,    2    ,    11    ,    38608    },/*    30    29    29    30    29    30    30    29    30    30    29    30    0    355
2033    */{    11    ,    1    ,    31    ,    19176    },/*    29    30    29    29    30    29    30    29    30    30    30    29    30    384
2034    */{    0    ,    2    ,    19    ,    19152    },/*    29    30    29    29    30    29    30    29    30    30    29    30    0    354
2035    */{    0    ,    2    ,    8    ,    42192    },/*    30    29    30    29    29    30    29    29    30    30    29    30    0    354
2036    */{    6    ,    1    ,    28    ,    53848    },/*    30    30    29    30    29    29    30    29    29    30    29    30    30    384
2037    */{    0    ,    2    ,    15    ,    53840    },/*    30    30    29    30    29    29    30    29    29    30    29    30    0    354
2038    */{    0    ,    2    ,    4    ,    54560    },/*    30    30    29    30    29    30    29    30    29    29    30    29    0    354
2039    */{    5    ,    1    ,    24    ,    55968    },/*    30    30    29    30    30    29    30    29    30    29    30    29    29    384
2040    */{    0    ,    2    ,    12    ,    46496    },/*    30    29    30    30    29    30    29    30    30    29    30    29    0    355
2041    */{    0    ,    2    ,    1    ,    22224    },/*    29    30    29    30    29    30    30    29    30    30    29    30    0    355
2042    */{    2    ,    1    ,    22    ,    19160    },/*    29    30    29    29    30    29    30    29    30    30    29    30    30    384
2043    */{    0    ,    2    ,    10    ,    18864    },/*    29    30    29    29    30    29    29    30    30    29    30    30    0    354
2044    */{    7    ,    1    ,    30    ,    42168    },/*    30    29    30    29    29    30    29    29    30    29    30    30    30    384
2045    */{    0    ,    2    ,    17    ,    42160    },/*    30    29    30    29    29    30    29    29    30    29    30    30    0    354
2046    */{    0    ,    2    ,    6    ,    43600    },/*    30    29    30    29    30    29    30    29    29    30    29    30    0    354
2047    */{    5    ,    1    ,    26    ,    46376    },/*    30    29    30    30    29    30    29    30    29    29    30    29    30    384
2048    */{    0    ,    2    ,    14    ,    27936    },/*    29    30    30    29    30    30    29    30    29    29    30    29    0    354
2049    */{    0    ,    2    ,    2    ,    44448    },/*    30    29    30    29    30    30    29    30    30    29    30    29    0    355
2050    */{    3    ,    1    ,    23    ,    21936    },/*    29    30    29    30    29    30    29    30    30    29    30    30    29    384
2051    */{    0    ,    2    ,    11    ,    37744    },/*    30    29    29    30    29    29    30    30    29    30    30    30    0    355
2052    */{    8    ,    2    ,    1    ,    18808    },/*    29    30    29    29    30    29    29    30    29    30    30    30    30    384
2053    */{    0    ,    2    ,    19    ,    18800    },/*    29    30    29    29    30    29    29    30    29    30    30    30    0    354
2054    */{    0    ,    2    ,    8    ,    25776    },/*    29    30    30    29    29    30    29    29    30    29    30    30    0    354
2055    */{    6    ,    1    ,    28    ,    27216    },/*    29    30    30    29    30    29    30    29    29    30    29    30    29    383
2056    */{    0    ,    2    ,    15    ,    59984    },/*    30    30    30    29    30    29    30    29    29    30    29    30    0    355
2057    */{    0    ,    2    ,    4    ,    27424    },/*    29    30    30    29    30    29    30    30    29    29    30    29    0    354
2058    */{    4    ,    1    ,    24    ,    43872    },/*    30    29    30    29    30    29    30    30    29    30    30    29    29    384
2059    */{    0    ,    2    ,    12    ,    43744    },/*    30    29    30    29    30    29    30    29    30    30    30    29    0    355
2060    */{    0    ,    2    ,    2    ,    37600    },/*    30    29    29    30    29    29    30    29    30    30    30    29    0    354
2061    */{    3    ,    1    ,    21    ,    51568    },/*    30    30    29    29    30    29    29    30    29    30    30    30    29    384
2062    */{    0    ,    2    ,    9    ,    51552    },/*    30    30    29    29    30    29    29    30    29    30    30    29    0    354
2063    */{    7    ,    1    ,    29    ,    54440    },/*    30    30    29    30    29    30    29    29    30    29    30    29    30    384
2064    */{    0    ,    2    ,    17    ,    54432    },/*    30    30    29    30    29    30    29    29    30    29    30    29    0    354
2065    */{    0    ,    2    ,    5    ,    55888    },/*    30    30    29    30    30    29    30    29    29    30    29    30    0    355
2066    */{    5    ,    1    ,    26    ,    23208    },/*    29    30    29    30    30    29    30    29    30    29    30    29    30    384
2067    */{    0    ,    2    ,    14    ,    22176    },/*    29    30    29    30    29    30    30    29    30    29    30    29    0    354
2068    */{    0    ,    2    ,    3    ,    42704    },/*    30    29    30    29    29    30    30    29    30    30    29    30    0    355
2069    */{    4    ,    1    ,    23    ,    21224    },/*    29    30    29    30    29    29    30    29    30    30    30    29    30    384
2070    */{    0    ,    2    ,    11    ,    21200    },/*    29    30    29    30    29    29    30    29    30    30    29    30    0    354
2071    */{    8    ,    1    ,    31    ,    43352    },/*    30    29    30    29    30    29    29    30    29    30    29    30    30    384
2072    */{    0    ,    2    ,    19    ,    43344    },/*    30    29    30    29    30    29    29    30    29    30    29    30    0    354
2073    */{    0    ,    2    ,    7    ,    46240    },/*    30    29    30    30    29    30    29    29    30    29    30    29    0    354
2074    */{    6    ,    1    ,    27    ,    46416    },/*    30    29    30    30    29    30    29    30    29    30    29    30    29    384
2075    */{    0    ,    2    ,    15    ,    44368    },/*    30    29    30    29    30    30    29    30    29    30    29    30    0    355
2076    */{    0    ,    2    ,    5    ,    21920    },/*    29    30    29    30    29    30    29    30    30    29    30    29    0    354
2077    */{    4    ,    1    ,    24    ,    42448    },/*    30    29    30    29    29    30    29    30    30    30    29    30    29    384
2078    */{    0    ,    2    ,    12    ,    42416    },/*    30    29    30    29    29    30    29    30    30    29    30    30    0    355
2079    */{    0    ,    2    ,    2    ,    21168    },/*    29    30    29    30    29    29    30    29    30    29    30    30    0    354
2080    */{    3    ,    1    ,    22    ,    43320    },/*    30    29    30    29    30    29    29    30    29    29    30    30    30    384
2081    */{    0    ,    2    ,    9    ,    26928    },/*    29    30    30    29    30    29    29    30    29    29    30    30    0    354
2082    */{    7    ,    1    ,    29    ,    29336    },/*    29    30    30    30    29    29    30    29    30    29    29    30    30    384
2083    */{    0    ,    2    ,    17    ,    27296    },/*    29    30    30    29    30    29    30    29    30    29    30    29    0    354
2084    */{    0    ,    2    ,    6    ,    44368    },/*    30    29    30    29    30    30    29    30    29    30    29    30    0    355
2085    */{    5    ,    1    ,    26    ,    19880    },/*    29    30    29    29    30    30    29    30    30    29    30    29    30    384
2086    */{    0    ,    2    ,    14    ,    19296    },/*    29    30    29    29    30    29    30    30    29    30    30    29    0    354
2087    */{    0    ,    2    ,    3    ,    42352    },/*    30    29    30    29    29    30    29    30    29    30    30    30    0    355
2088    */{    4    ,    1    ,    24    ,    21104    },/*    29    30    29    30    29    29    30    29    29    30    30    30    29    383
2089    */{    0    ,    2    ,    10    ,    53856    },/*    30    30    29    30    29    29    30    29    29    30    30    29    0    354
2090    */{    8    ,    1    ,    30    ,    59696    },/*    30    30    30    29    30    29    29    30    29    29    30    30    29    384
2091    */{    0    ,    2    ,    18    ,    54560    },/*    30    30    29    30    29    30    29    30    29    29    30    29    0    354
2092    */{    0    ,    2    ,    7    ,    55968    },/*    30    30    29    30    30    29    30    29    30    29    30    29    0    355
2093    */{    6    ,    1    ,    27    ,    27472    },/*    29    30    30    29    30    29    30    30    29    30    29    30    29    384
2094    */{    0    ,    2    ,    15    ,    22224    },/*    29    30    29    30    29    30    30    29    30    30    29    30    0    355
2095    */{    0    ,    2    ,    5    ,    19168    },/*    29    30    29    29    30    29    30    29    30    30    30    29    0    354
2096    */{    4    ,    1    ,    25    ,    42216    },/*    30    29    30    29    29    30    29    29    30    30    30    29    30    384
2097    */{    0    ,    2    ,    12    ,    42192    },/*    30    29    30    29    29    30    29    29    30    30    29    30    0    354
2098    */{    0    ,    2    ,    1    ,    53584    },/*    30    30    29    30    29    29    29    30    29    30    29    30    0    354
2099    */{    2    ,    1    ,    21    ,    55592    },/*    30    30    29    30    30    29    29    30    29    29    30    29    30    384
2100    */{    0    ,    2    ,    9    ,    54560    },/*    30    30    29    30    29    30    29    30    29    29    30    29    0    354
        */};


        internal override int MinCalendarYear {
            get
            {
                return (MIN_LUNISOLAR_YEAR);
            }
        }

        internal override int MaxCalendarYear {
            get
            {
                return (MAX_LUNISOLAR_YEAR);
            }
        }

        internal override DateTime MinDate {
            get
            {
                return (minDate);
            }
        }

        internal override DateTime MaxDate {
            get
            {
                return (maxDate);
            }
        }

        internal override EraInfo[] CalEraInfo {
            get
            {
                return (null);
            }
        }

        internal override int  GetYearInfo(int LunarYear, int Index) {
            if ((LunarYear < MIN_LUNISOLAR_YEAR) || (LunarYear > MAX_LUNISOLAR_YEAR)) {
                throw new ArgumentOutOfRangeException(
                            "year",
                            String.Format(
                                CultureInfo.CurrentCulture,
                                Environment.GetResourceString("ArgumentOutOfRange_Range"), MIN_LUNISOLAR_YEAR, MAX_LUNISOLAR_YEAR ));
            }
            Contract.EndContractBlock();

            return yinfo[LunarYear - MIN_LUNISOLAR_YEAR, Index];
        }

        internal override int GetYear(int year, DateTime time) {
            return year;
        }

        internal override int GetGregorianYear(int year, int era) {
            if (era != CurrentEra && era != ChineseEra) {
                throw new ArgumentOutOfRangeException(nameof(era), Environment.GetResourceString("ArgumentOutOfRange_InvalidEraValue"));
            }

            if (year < MIN_LUNISOLAR_YEAR || year > MAX_LUNISOLAR_YEAR) {
                throw new ArgumentOutOfRangeException(
                            nameof(year),
                            String.Format(
                                CultureInfo.CurrentCulture,
                                Environment.GetResourceString("ArgumentOutOfRange_Range"), MIN_LUNISOLAR_YEAR, MAX_LUNISOLAR_YEAR));
            }
            Contract.EndContractBlock();

            return year;
        }


        /*=================================GetDefaultInstance==========================
        **Action: Internal method to provide a default intance of ChineseLunisolarCalendar.  Used by NLS+ implementation
        **       and other calendars.
        **Returns:
        **Arguments:
        **Exceptions:
        ============================================================================*/

        /*
        internal static Calendar GetDefaultInstance()
        {
            if (m_defaultInstance == null) {
                m_defaultInstance = new ChineseLunisolarCalendar();
            }
            return (m_defaultInstance);
        }
        */

        // Construct an instance of ChineseLunisolar calendar.

        public ChineseLunisolarCalendar() {
        }


        [System.Runtime.InteropServices.ComVisible(false)]
        public override int GetEra(DateTime time) {
            CheckTicksRange(time.Ticks);
            return (ChineseEra);
        }

        internal override int ID {
            get {
                return (CAL_CHINESELUNISOLAR);
            }
        }

        internal override int BaseCalendarID {
            get {
                //Use CAL_GREGORIAN just to get CurrentEraValue as 1 since we do not have data under the ID CAL_ChineseLunisolar yet
                return (CAL_GREGORIAN);
            }
        }


        [System.Runtime.InteropServices.ComVisible(false)]
        public override int[] Eras {
            get {
                return (new int[] {ChineseEra});
            }
        }
    }
}
