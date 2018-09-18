// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;

namespace System
{
    internal unsafe partial class Number
    {
        // precomputed tables with powers of 10. These allows us to do at most
        // two Mul64 during the conversion. This is important not only
        // for speed, but also for precision because of Mul64 computes with 1 bit error.

        private static readonly ulong[] rgval64Power10 = new ulong[]
        {
            // powers of 10
            0XA000000000000000,     // 1
            0XC800000000000000,     // 2
            0XFA00000000000000,     // 3
            0X9C40000000000000,     // 4
            0XC350000000000000,     // 5
            0XF424000000000000,     // 6
            0X9896800000000000,     // 7
            0XBEBC200000000000,     // 8
            0XEE6B280000000000,     // 9
            0X9502F90000000000,     // 10
            0XBA43B74000000000,     // 11
            0XE8D4A51000000000,     // 12
            0X9184E72A00000000,     // 13
            0XB5E620F480000000,     // 14
            0XE35FA931A0000000,     // 15

            // powers of 0.1
            0xCCCCCCCCCCCCCCCD,     // 1
            0xA3D70A3D70A3D70B,     // 2
            0x83126E978D4FDF3C,     // 3
            0xD1B71758E219652E,     // 4
            0xA7C5AC471B478425,     // 5
            0x8637BD05AF6C69B7,     // 6
            0xD6BF94D5E57A42BE,     // 7
            0xABCC77118461CEFF,     // 8
            0x89705F4136B4A599,     // 9
            0xDBE6FECEBDEDD5C2,     // 10
            0xAFEBFF0BCB24AB02,     // 11
            0x8CBCCC096F5088CF,     // 12
            0xE12E13424BB40E18,     // 13
            0xB424DC35095CD813,     // 14
            0x901D7CF73AB0ACDC,     // 15
        };

        private static readonly short[] rgexp64Power10 = new short[]
        {
            // exponents for both powers of 10 and 0.1
            4,      // 1
            7,      // 2
            10,     // 3
            14,     // 4
            17,     // 5
            20,     // 6
            24,     // 7
            27,     // 8
            30,     // 9
            34,     // 10
            37,     // 11
            40,     // 12
            44,     // 13
            47,     // 14
            50,     // 15
        };

        private static readonly ulong[] rgval64Power10By16 = new ulong[]
        {
            // powers of 10^16
            0x8E1BC9BF04000000,     // 1
            0x9DC5ADA82B70B59E,     // 2
            0xAF298D050E4395D6,     // 3
            0xC2781F49FFCFA6D4,     // 4
            0xD7E77A8F87DAF7FA,     // 5
            0xEFB3AB16C59B14A0,     // 6
            0x850FADC09923329C,     // 7
            0x93BA47C980E98CDE,     // 8
            0xA402B9C5A8D3A6E6,     // 9
            0xB616A12B7FE617A8,     // 10
            0xCA28A291859BBF90,     // 11
            0xE070F78D39275566,     // 12
            0xF92E0C3537826140,     // 13
            0x8A5296FFE33CC92C,     // 14
            0x9991A6F3D6BF1762,     // 15
            0xAA7EEBFB9DF9DE8A,     // 16
            0xBD49D14AA79DBC7E,     // 17
            0xD226FC195C6A2F88,     // 18
            0xE950DF20247C83F8,     // 19
            0x81842F29F2CCE373,     // 20
            0x8FCAC257558EE4E2,     // 21

            // powers of 0.1^16
            0xE69594BEC44DE160,     // 1
            0xCFB11EAD453994C3,     // 2
            0xBB127C53B17EC165,     // 3
            0xA87FEA27A539E9B3,     // 4
            0x97C560BA6B0919B5,     // 5
            0x88B402F7FD7553AB,     // 6
            0xF64335BCF065D3A0,     // 7
            0xDDD0467C64BCE4C4,     // 8
            0xC7CABA6E7C5382ED,     // 9
            0xB3F4E093DB73A0B7,     // 10
            0xA21727DB38CB0053,     // 11
            0x91FF83775423CC29,     // 12
            0x8380DEA93DA4BC82,     // 13
            0xECE53CEC4A314F00,     // 14
            0xD5605FCDCF32E217,     // 15
            0xC0314325637A1978,     // 16
            0xAD1C8EAB5EE43BA2,     // 17
            0x9BECCE62836AC5B0,     // 18
            0x8C71DCD9BA0B495C,     // 19
            0xFD00B89747823938,     // 20
            0xE3E27A444D8D991A,     // 21
        };

        private static readonly short[] rgexp64Power10By16 = new short[]
        {
            // exponents for both powers of 10^16 and 0.1^16
            54,     // 1
            107,    // 2
            160,    // 3
            213,    // 4
            266,    // 5
            319,    // 6
            373,    // 7
            426,    // 8
            479,    // 9
            532,    // 10
            585,    // 11
            638,    // 12
            691,    // 13
            745,    // 14
            798,    // 15
            851,    // 16
            904,    // 17
            957,    // 18
            1010,   // 19
            1064,   // 20
            1117,   // 21
        };

#if DEBUG
        private static bool s_CheckedTables = false;

        private static void CheckTables()
        {
            ulong val;
            int exp;

            val = 0xa000000000000000;
            exp = 4; // 10

            CheckPow10MantissaTable(val, exp, new Span<ulong>(rgval64Power10, 0, 15), "rgval64Power10");
            CheckPow10ExponentTable(val, exp, new Span<short>(rgexp64Power10, 0, 15), "rgexp64Power10");

            val = 0x8E1BC9BF04000000;
            exp = 54; //10^16

            CheckPow10MantissaTable(val, exp, new Span<ulong>(rgval64Power10By16, 0, 21), "rgval64Power10By16");
            CheckPow10ExponentTable(val, exp, new Span<short>(rgexp64Power10By16, 0, 21), "rgexp64Power10By16");

            val = 0xCCCCCCCCCCCCCCCD;
            exp = -3; // 0.1

            CheckPow10MantissaTable(val, exp, new Span<ulong>(rgval64Power10, 15, 15), "rgval64Power10 - inv");

            val = 0xE69594BEC44DE160;
            exp = -53; // 0.1^16

            CheckPow10MantissaTable(val, exp, new Span<ulong>(rgval64Power10By16, 21, 21), "rgval64Power10By16 - inv");
        }

        // debug-only verification of the precomputed tables
        private static void CheckPow10MantissaTable(ulong val, int exp, Span<ulong> table, string name)
        {
            ulong multval = val;
            int mulexp = exp;
            bool isBad = false;

            for (int i = 0; i < table.Length; i++)
            {
                if (table[i] != val)
                {
                    if (!isBad)
                    {
                        Debug.WriteLine(name);
                        isBad = true;
                    }
                    Debug.WriteLine($"0x{val:X16},     // {i + 1}");
                }

                exp += mulexp;
                val = Mul64Precise(val, multval, ref exp);
            }

            Debug.Assert(!isBad, "NumberToDouble table not correct. Correct version dumped to debug output.");
        }

        // debug-only verification of the precomputed tables
        private static void CheckPow10ExponentTable(ulong val, int exp, Span<short> table, string name)
        {
            ulong multval = val;
            int mulexp = exp;
            bool isBad = false;

            for (int i = 0; i < table.Length; i++)
            {
                if (table[i] != exp)
                {
                    if (!isBad)
                    {
                        Debug.WriteLine(name);
                        isBad = true;
                    }
                    Debug.WriteLine($"{val}, // {i + 1}");
                }

                exp += mulexp;
                val = Mul64Precise(val, multval, ref exp);
            }

            Debug.Assert(!isBad, "NumberToDouble table not correct. Correct version dumped to debug output.");
        }
#endif

        // get 32-bit integer from at most 9 digits
        private static uint DigitsToInt(char* p, int count)
        {
            Debug.Assert((1 <= count) && (count <= 9));

            char* end = (p + count);
            uint res = (uint)(p[0] - '0');

            for (p++; p < end; p++)
            {
                res = (10 * res) + p[0] - '0';
            }

            return res;
        }

        private static int GetLength(char* src)
        {
            int length = 0;

            while (src[length] != '\0')
            {
                length++;
            }

            return length;
        }

        // helper function to multiply two 32-bit uints
        private static ulong Mul32x32To64(uint a, uint b)
        {
            return (ulong)(a) * b;
        }

        // multiply two numbers in the internal integer representation
        private static ulong Mul64Lossy(ulong a, ulong b, ref int exp)
        {
            // it's ok to lose some precision here - Mul64 will be called
            // at most twice during the conversion, so the error won't propagate
            // to any of the 53 significant bits of the result
            ulong val = Mul32x32To64((uint)(a >> 32), (uint)(b >> 32))
                      + (Mul32x32To64((uint)(a >> 32), (uint)(b)) >> 32)
                      + (Mul32x32To64((uint)(a), (uint)(b >> 32)) >> 32);

            // normalize
            if ((val & 0x8000000000000000) == 0)
            {
                val <<= 1;
                exp--;
            }

            return val;
        }

        // slower high precision version of Mul64 for computation of the tables
        private static ulong Mul64Precise(ulong a, ulong b, ref int exp)
        {
            ulong hilo = ((Mul32x32To64((uint)(a >> 32), (uint)(b)) >> 1)
                       + (Mul32x32To64((uint)(a), (uint)(b >> 32)) >> 1)
                       + (Mul32x32To64((uint)(a), (uint)(b)) >> 33)) >> 30;

            ulong val = Mul32x32To64((uint)(a >> 32), (uint)(b >> 32))
                      + (hilo >> 1)
                      + (hilo & 1);

            // normalize
            if ((val & 0x8000000000000000) == 0)
            {
                val <<= 1;
                exp--;
            }

            return val;
        }

        private static double NumberToDouble(ref NumberBuffer number)
        {
#if DEBUG
            
            if (!s_CheckedTables)
            {
                CheckTables();
                s_CheckedTables = true;
            }
#endif

            char* src = number.digits;
            int total = GetLength(src);
            int remaining = total;

            // skip the leading zeros
            while (src[0] == '0')
            {
                remaining--;
                src++;
            }

            if (remaining == 0)
            {
                return number.sign ? -0.0 : 0.0;
            }

            int count = Math.Min(remaining, 9);
            remaining -= count;
            ulong val = DigitsToInt(src, count);

            if (remaining > 0)
            {
                count = Math.Min(remaining, 9);
                remaining -= count;

                // get the denormalized power of 10
                uint mult = (uint)(rgval64Power10[count - 1] >> (64 - rgexp64Power10[count - 1]));
                val = Mul32x32To64((uint)(val), mult) + DigitsToInt(src + 9, count);
            }

            int scale = number.scale - (total - remaining);
            int absscale = Math.Abs(scale);
            if (absscale >= 22 * 16)
            {
                // overflow / underflow
                if (scale > 0)
                {
                    return number.sign ? double.NegativeInfinity : double.PositiveInfinity;
                }
                else
                {
                    return number.sign ? -0.0 : 0.0;
                }
            }

            int exp = 64;

            // normalize the mantissa
            if ((val & 0xFFFFFFFF00000000) == 0) { val <<= 32; exp -= 32; }
            if ((val & 0xFFFF000000000000) == 0) { val <<= 16; exp -= 16; }
            if ((val & 0xFF00000000000000) == 0) { val <<= 8; exp -= 8; }
            if ((val & 0xF000000000000000) == 0) { val <<= 4; exp -= 4; }
            if ((val & 0xC000000000000000) == 0) { val <<= 2; exp -= 2; }
            if ((val & 0x8000000000000000) == 0) { val <<= 1; exp -= 1; }

            int index = absscale & 15;
            if (index != 0)
            {
                int multexp = rgexp64Power10[index - 1];
                // the exponents are shared between the inverted and regular table
                exp += (scale < 0) ? (-multexp + 1) : multexp;

                ulong multval = rgval64Power10[index + ((scale < 0) ? 15 : 0) - 1];
                val = Mul64Lossy(val, multval, ref exp);
            }

            index = absscale >> 4;
            if (index != 0)
            {
                int multexp = rgexp64Power10By16[index - 1];
                // the exponents are shared between the inverted and regular table
                exp += (scale < 0) ? (-multexp + 1) : multexp;

                ulong multval = rgval64Power10By16[index + ((scale < 0) ? 21 : 0) - 1];
                val = Mul64Lossy(val, multval, ref exp);
            }


            // round & scale down
            if (((uint)(val) & (1 << 10)) != 0)
            {
                // IEEE round to even
                ulong tmp = val + ((1 << 10) - 1) + (((uint)(val) >> 11) & 1);
                if (tmp < val)
                {
                    // overflow
                    tmp = (tmp >> 1) | 0x8000000000000000;
                    exp += 1;
                }
                val = tmp;
            }

            // return the exponent to a biased state
            exp += 0x3FE;

            // handle overflow, underflow, "Epsilon - 1/2 Epsilon", denormalized, and the normal case
            if (exp <= 0)
            {
                if (exp == -52 && (val >= 0x8000000000000058))
                {
                    // round X where {Epsilon > X >= 2.470328229206232730000000E-324} up to Epsilon (instead of down to zero)
                    val = 0x0000000000000001;
                }
                else if (exp <= -52)
                {
                    // underflow
                    val = 0;
                }
                else
                {
                    // denormalized
                    val >>= (-exp + 11 + 1);
                }
            }
            else if (exp >= 0x7FF)
            {
                // overflow
                val = 0x7FF0000000000000;
            }
            else
            {
                // normal postive exponent case
                val = ((ulong)(exp) << 52) + ((val >> 11) & 0x000FFFFFFFFFFFFF);
            }

            if (number.sign)
            {
                val |= 0x8000000000000000;
            }

            return BitConverter.DoubleToInt64Bits(val);
        }
    }
}
