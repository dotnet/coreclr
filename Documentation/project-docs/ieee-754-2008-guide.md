# IEEE Std 754-2008: Floating-Point Arithmetic

This document gives an overview of the IEEE 754-2008 standard, but only for the binary floating-point numbers.

**Table of Contents**
* [Format](#format)
  * [Representation](#representation)
  * [Encoding](#encoding)
  * [Parameters and Additional Notes](#parameters-and-additional-notes)
* [Operations](#operations)
* [Recommended Operations](#recommended-operations)
* [Rounding](#rounding)
* [Infinities](#infinities)
* [NaN](#nan)
* [Sign](#sign)
* [Exception Handling](#exception-handling)
* [Expression Evaluation](#expression-evaluation)
* [Reproducible Results](#reproducible-results)

## Format

The set of finite floating-point numbers that can be represented is determined by:
 * A radix (`b`), which is `2`
* The number of digits in the significand, also known as the precision (`p`)
* The maximum exponent (`emax`)
* The minimum exponent (`emin`)
  * `emin` is computed as: `1 - emax`

The largest positive `normal` floating-point number is <code>b<sup>emax</sup> * (b - b<sup>1-p</sup>)</code>.

The smallest positive `normal` floating-point number is <code>b<sup>emin</sup></code>
* Numbers less than this, but greater than zero, are considered `subnormal`.

The smallest positive `subnormal` floating-point number is <code>b<sup>emin</sup> * b<sup>1-p</sup></code>
* Every finite floating-point number is an integral multiple of this value

### Representation

Representations (`r`) of floating-point data comes in the following formats:
* A triplet: `(sign, exponent, signficand)`, where:
  * In a given radix (`b`), the number is computed as: <code>(-1)<sup>sign</sup> * b<sup>exponent</sup> * significand</code>
* A signed zero or non-zero floating point number: <code>(-1)<sup>s</sup> * b<sup>e</sup> * m</code>, where:
  * `s` is `0` or `1`
  * `e` is any integer: `emin <= e <= emax`
  * `m` is a digit string: <code>d<sub>0</sub>.d<sub>1</sub>d<sub>2</sub>...d<sub>p-1</sub></code>
    * <code>d<sub>i</sub></code> is an integer digit: <code>0 <= d<sub>i</sub> <= b</code>; therefore `0 <= m < b`
    * The radix point immediately follows `d0`
* A signed zero or non-zero floating point number: <code>(-1)<sup>s</sup> * b<sup>q</sup> * c</code>, where:
  * `s` is `0` or `1`
  * `q` is any integer: `emin <= (q + p - 1) <= emax`
  * `c` is a digit string: <code>d<sub>0</sub>.d<sub>1</sub>d<sub>2</sub>...d<sub>p-1</sub></code>
    * <code>d<sub>i</sub></code> is an integer digit: <code>0 <= d<sub>i</sub> <= b</code>; therefore `c` is an integer where <code>0 <= c < b<sup>p</sup></code>
  * `e = (q + p - 1)` and <code>m = (c * b<sup>1 - p</sup>)</code>
* Postive Infinity (`+Inf`), Negative Infinity (`−Inf`)
* quiet NaN (`qNaN`), signaling NaN (`sNaN`)

### Encoding

Values (`v`) of binary (`b = 2`) floating-point data have the following encoding:
* 1 bit sign (`S`)
* `w`-bit biased exponent (`E`)
  * `E = (e + bias)` and has the following range:
    * every integer between `1` and <code>2<sup>w</sup> - 2</code>, inclusive, which encodes normal numbers
    * the reserved value `0` which encodes `+0`, `-0`, and `subnormal` numbers
    * the reserved value <code>2<sup>w</sup> - 1</code> which encode `+Inf`, `-Inf`, `qNaN`, and `sNaN`
* `t`-bit trailing significand digit string (`T`)
  * `t = (p - 1)`
  * <code>T = d<sub>1</sub>d<sub>2</sub>...d<sub>p - 1</sub></code>
    * <code>d<sub>0</sub></code> is implicitly encoded in `E`


### Parameters and Additional Notes

| parameter | binary32 | binary64 | compute                                 |
| --------- | -------- | -------- | --------------------------------------- |
| b         | 2        | 2        | radix (2)                               |
| k         | 32       | 64       | storage width, in bits (multiple of 32) |
| p         | 24       | 53       | k - round(4 * log<sub>2</sub>(k)) + 13  |
| t         | 23       | 52       | p - 1                                   |
| w         | 11       | 8        | round(4 * log<sub>2</sub>(k)) - 13      |
| bias      | 127      | 1023     | 2<sup>k– p –1</sup> – 1                 |
| emax      | 127      | 1023     | bias                                    |
| emin      | -126     | -1022    | 1 - emax                                |

`r` and `v` can be inferred as follows:
* If <code>E == 2<sup>w</sup> - 1</code> and `T != 0`
  * `r` is `qNan` or `sNaN`
  * `v` is `NaN` regardless of `S`
* If <code>E == 2<sup>w</sup> - 1</code> and `T == 0`
  * <code>r = ((-1)<sup>S</sup> * +Inf)</code>
  * <code>v = ((-1)<sup>S</sup> * +Inf)</code>
* If <code>1 <= E <= (2<sup>w</sup> - 2)</code>
  * <code>r = (S, (E - bias), (1 + 2<sup>1 - p</sup> * T))</code></code>
  * <code>v = ((-1)^<sup>S</sup> * 2<sup>E - bias</sup> * (1 + 2<sup>1 - p</sup> * T))</code>
    * This causes <code>d<sub>0</sub></code> to be `1`
* If `E == 0` and `T != 0`
  * <code>r = (S, emin, (0 + 2<sup>1 - p</sup> * T))</code>
  * <code>v = ((-1)<sup>S</sup> * 2<sup>emin</sup> * (0 + 2<sup>1 - p</sup> * T))</code>
    * This causes <code>d<sub>0</sub></code> to be `0`
* If `E == 0` and `T == 0`
  * `r = (S, emin, 0)`
  * <code>v = ((-1)<sup>S</sup> * +0.0)</code>

## Rounding

IEEE supports the following rounding directions:
* `roundTiesToEven`: Ties go to the number with an even least signifcant digit (<code>d<sub>p - 1</sub></code>)
  * This is the default
* `roundTiesToAway`: Ties go to the number with a larger magnitude
  * This is not required to be provided
* `roundTowardPositive`: Ties go to the number closest to `+Inf`
* `roundTowardNegative`: Ties go to the number closest to `-Inf`
* `roundTowardZero`: Ties go to the number closest to `0`

For `roundTiesToEven` and `roundTiesToAway`, numbers larger than <code>b<sup>emax</sup> * (b - (0.5 * b<sup>1 - p</sup>))</code> round to `Inf`, with no change in sign

## Operations

TODO

## Recommended Operations

TODO

## Infinities

For all finite numbers: `-Inf < {finiteNumber} < +Inf`

Operations on `Inf` are usually exact and do not signal any exceptions. The exception is when `Inf` is an invalid operand, when `Inf` is created from finite operands by overflow or division by zero, or when doing `remainder(subnormal, Inf)`

## NaN

`sNan` are used to represent things like uninitialized variables and arithmetic-enhancements (such as complex-affine infinities or extremely wide ranges).

`qNaN` are left to the implementer's discretion and can be used to represent things like diagnostic information. To facilitate the use of this diagnostic information, the `NaN` payload should be preserved in the result of an operation as much as possible.

Under the default exception handling, any operation signaling an `invalid operation` exception and that returns a floating-point result should return a `qNaN`.

`sNaN` should be considered a reserved operation that, under default exception handling, signals the `invalid operation` exception.

For an operation with `qNaN` inputs, the result (unless otherwise specified) should be one of the inputs.

An operation that returns a `NaN` and had a single `NaN` as an input, it should return the input `NaN` payload as its result, if the payload is representable in the destination format.

An operation that returns a `NaN` and has more than a single `NaN` as an input should return one of the input `NaN` payloads as its result, if the payload is representable in the destination format. Which of the input payloads is returned is not specified.

Conversion of a `qNaN` to a wider format with the same radix and back should not change the payload.

## Sign

When either input or result is `NaN`, the sign of the `NaN` is not interpreted (unless otherwise specified).

When neither input nor results are `NaN`:
* The sign of a `product` or `quotient` operation is the `xor` of the operands signs
* The sign of a `sum` (`x + y`) or `difference` (`x + (-y)`) operation differs from at mast one of the addends sign
* The sign of a `conversion`, `roundToIntegral`, or `roundToIntegralExact` operation is the sign of the first or only operand
  * This applies to `0` and `Inf`
* The sign of a `sum` with two operands of opposite signs or the `difference` of two operands with the same sign, when the result is exactly 0, shall be `+0`, except under `roundTowardNegative` where it shall be `-0`. However, `(x + x) == (x - (-x))` should retain the sign of `x`
* When `(a * b) + c` is exactly zero, the sign of `fusedMultiplyAdd` shall follow the above rules for the `sum` operands
* When `(a * b) + c` is exactly zero due to rounding, the sign of `fusedMultiplyAdd` shall be the sign of the exact result prior to rounding
* `squareRoot(-0)` should return `-0`; all other `squareRoot` should have a positive sign

## Exception Handling

TODO

## Expression Evaluation

TODO

## Reproducible Results

TODO
