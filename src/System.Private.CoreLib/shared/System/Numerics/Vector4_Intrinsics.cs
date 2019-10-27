// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Arm;
using System.Runtime.Intrinsics.X86;

namespace System.Numerics
{
    // This file contains the definitions for all of the JIT intrinsic methods and properties that are recognized by the current x64 JIT compiler.
    // The implementation defined here is used in any circumstance where the JIT fails to recognize these members as intrinsic.
    // The JIT recognizes these methods and properties by name and signature: if either is changed, the JIT will no longer recognize the member.
    // Some methods declared here are not strictly intrinsic, but delegate to an intrinsic method. For example, only one overload of CopyTo()

    public partial struct Vector4
    {
        /// <summary>
        /// The X component of the vector.
        /// </summary>
        public float X;
        /// <summary>
        /// The Y component of the vector.
        /// </summary>
        public float Y;
        /// <summary>
        /// The Z component of the vector.
        /// </summary>
        public float Z;
        /// <summary>
        /// The W component of the vector.
        /// </summary>
        public float W;

        #region Constructors

        /// <summary>
        /// Constructs a vector whose elements are all the single specified value.
        /// </summary>
        /// <param name="value">The element to fill the vector with.</param>
#if ARM64
        [Intrinsic]
#endif
        public Vector4(float value)
        {
            if (Sse.IsSupported)
            {
                this = Vector128.Create(value).AsVector4();
            }
            else
            {
                X = value;
                Y = value;
                Z = value;
                W = value;
            }
        }
        /// <summary>
        /// Constructs a vector with the given individual elements.
        /// </summary>
        /// <param name="w">W component.</param>
        /// <param name="x">X component.</param>
        /// <param name="y">Y component.</param>
        /// <param name="z">Z component.</param>
#if ARM64
        [Intrinsic]
#endif
        public Vector4(float x, float y, float z, float w)
        {
            if (Sse.IsSupported)
            {
                this = Vector128.Create(x, y, z, w).AsVector4();
            }
            else
            {
                X = x;
                Y = y;
                Z = z;
                W = w;
            }
        }

        /// <summary>
        /// Constructs a Vector4 from the given Vector2 and a Z and W component.
        /// </summary>
        /// <param name="value">The vector to use as the X and Y components.</param>
        /// <param name="z">The Z component.</param>
        /// <param name="w">The W component.</param>
        [Intrinsic]
        public Vector4(Vector2 value, float z, float w)
        {
            X = value.X;
            Y = value.Y;
            Z = z;
            W = w;
        }

        /// <summary>
        /// Constructs a Vector4 from the given Vector3 and a W component.
        /// </summary>
        /// <param name="value">The vector to use as the X, Y, and Z components.</param>
        /// <param name="w">The W component.</param>
        [Intrinsic]
        public Vector4(Vector3 value, float w)
        {
            X = value.X;
            Y = value.Y;
            Z = value.Z;
            W = w;
        }
        #endregion Constructors

        #region Public Instance Methods
        /// <summary>
        /// Copies the contents of the vector into the given array.
        /// </summary>
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void CopyTo(float[] array)
        {
            CopyTo(array, 0);
        }

        /// <summary>
        /// Copies the contents of the vector into the given array, starting from index.
        /// </summary>
        /// <exception cref="ArgumentNullException">If array is null.</exception>
        /// <exception cref="RankException">If array is multidimensional.</exception>
        /// <exception cref="ArgumentOutOfRangeException">If index is greater than end of the array or index is less than zero.</exception>
        /// <exception cref="ArgumentException">If number of elements in source vector is greater than those available in destination array.</exception>
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void CopyTo(float[] array, int index)
        {
            if (array == null)
            {
                // Match the JIT's exception type here. For perf, a NullReference is thrown instead of an ArgumentNull.
                throw new NullReferenceException(SR.Arg_NullArgumentNullRef);
            }
            if (index < 0 || index >= array.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index), SR.Format(SR.Arg_ArgumentOutOfRangeException, index));
            }
            if ((array.Length - index) < 4)
            {
                throw new ArgumentException(SR.Format(SR.Arg_ElementsInSourceIsGreaterThanDestination, index));
            }
            array[index] = X;
            array[index + 1] = Y;
            array[index + 2] = Z;
            array[index + 3] = W;
        }

        /// <summary>
        /// Returns a boolean indicating whether the given Vector4 is equal to this Vector4 instance.
        /// </summary>
        /// <param name="other">The Vector4 to compare this instance to.</param>
        /// <returns>True if the other Vector4 is equal to this instance; False otherwise.</returns>
#if ARM64
        [Intrinsic]
#endif
        public readonly bool Equals(Vector4 other)
        {
            if (Sse.IsSupported)
            {
                return Sse.MoveMask(Sse.CompareEqual(this.AsVector128(), other.AsVector128())) == 0b1111;
            }
            return this.X == other.X
                && this.Y == other.Y
                && this.Z == other.Z
                && this.W == other.W;
        }
        #endregion Public Instance Methods

        #region Public Static Methods
        /// <summary>
        /// Returns the dot product of two vectors.
        /// </summary>
        /// <param name="vector1">The first vector.</param>
        /// <param name="vector2">The second vector.</param>
        /// <returns>The dot product.</returns>
#if ARM64
        [Intrinsic]
#endif
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Dot(Vector4 vector1, Vector4 vector2)
        {
            if (Sse41.IsSupported)
            {
                return Sse41.DotProduct(vector1.AsVector128(), vector2.AsVector128(), 0xFF).ToScalar();
            }
            else if (Sse.IsSupported)
            {
                Vector128<float> tmp = Sse.Multiply(vector1.AsVector128(), vector2.AsVector128());

                if (Sse3.IsSupported)
                {
                    tmp = Sse3.HorizontalAdd(tmp, tmp);
                    return Sse3.HorizontalAdd(tmp, tmp).ToScalar();
                }
                else
                {
                    Vector128<float> tmp2 = Sse.Shuffle(vector2.AsVector128(), tmp, 0x40);
                    tmp2 = Sse.Add(tmp2, tmp);
                    tmp = Sse.Shuffle(tmp, tmp2, 0x30);
                    tmp = Sse.Add(tmp, tmp2);
                    return Sse.Shuffle(tmp, tmp, 0xAA).ToScalar();
                }
            }

            return vector1.X * vector2.X +
                   vector1.Y * vector2.Y +
                   vector1.Z * vector2.Z +
                   vector1.W * vector2.W;
        }

        /// <summary>
        /// Returns a vector whose elements are the minimum of each of the pairs of elements in the two source vectors.
        /// </summary>
        /// <param name="value1">The first source vector.</param>
        /// <param name="value2">The second source vector.</param>
        /// <returns>The minimized vector.</returns>
#if ARM64
        [Intrinsic]
#endif
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 Min(Vector4 value1, Vector4 value2)
        {
            if (Sse.IsSupported)
            {
                return Sse.Min(value1.AsVector128(), value2.AsVector128()).AsVector4();
            }
            return new Vector4(
                (value1.X < value2.X) ? value1.X : value2.X,
                (value1.Y < value2.Y) ? value1.Y : value2.Y,
                (value1.Z < value2.Z) ? value1.Z : value2.Z,
                (value1.W < value2.W) ? value1.W : value2.W);
        }

        /// <summary>
        /// Returns a vector whose elements are the maximum of each of the pairs of elements in the two source vectors.
        /// </summary>
        /// <param name="value1">The first source vector.</param>
        /// <param name="value2">The second source vector.</param>
        /// <returns>The maximized vector.</returns>
#if ARM64
        [Intrinsic]
#endif
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 Max(Vector4 value1, Vector4 value2)
        {
            if (Sse.IsSupported)
            {
                return Sse.Max(value1.AsVector128(), value2.AsVector128()).AsVector4();
            }
            return new Vector4(
                (value1.X > value2.X) ? value1.X : value2.X,
                (value1.Y > value2.Y) ? value1.Y : value2.Y,
                (value1.Z > value2.Z) ? value1.Z : value2.Z,
                (value1.W > value2.W) ? value1.W : value2.W);
        }

        /// <summary>
        /// Returns a vector whose elements are the absolute values of each of the source vector's elements.
        /// </summary>
        /// <param name="value">The source vector.</param>
        /// <returns>The absolute value vector.</returns>
#if ARM64
        [Intrinsic]
#endif
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 Abs(Vector4 value)
        {
            if (Sse.IsSupported)
            {
                return Sse.And(value.AsVector128(), Vector128.Create(0x7FFFFFFF).AsSingle()).AsVector4();
            }
            return new Vector4(MathF.Abs(value.X), MathF.Abs(value.Y), MathF.Abs(value.Z), MathF.Abs(value.W));
        }

        /// <summary>
        /// Returns a vector whose elements are the square root of each of the source vector's elements.
        /// </summary>
        /// <param name="value">The source vector.</param>
        /// <returns>The square root vector.</returns>
#if ARM64
        [Intrinsic]
#endif
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 SquareRoot(Vector4 value)
        {
            if (Sse.IsSupported)
            {
                return Sse.Sqrt(value.AsVector128()).AsVector4();
            }
            return new Vector4(MathF.Sqrt(value.X), MathF.Sqrt(value.Y), MathF.Sqrt(value.Z), MathF.Sqrt(value.W));
        }
        #endregion Public Static Methods

        #region Public static operators
        /// <summary>
        /// Adds two vectors together.
        /// </summary>
        /// <param name="left">The first source vector.</param>
        /// <param name="right">The second source vector.</param>
        /// <returns>The summed vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 operator +(Vector4 left, Vector4 right)
        {
            if (Sse.IsSupported)
            {
                return Sse.Add(left.AsVector128(), right.AsVector128()).AsVector4();
            }
            else if (AdvSimd.IsSupported)
            {
                return AdvSimd.Add(left.AsVector128(), right.AsVector128()).AsVector4();
            }
            return new Vector4(left.X + right.X, left.Y + right.Y, left.Z + right.Z, left.W + right.W);
        }

        /// <summary>
        /// Subtracts the second vector from the first.
        /// </summary>
        /// <param name="left">The first source vector.</param>
        /// <param name="right">The second source vector.</param>
        /// <returns>The difference vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 operator -(Vector4 left, Vector4 right)
        {
            if (Sse.IsSupported)
            {
                return Sse.Subtract(left.AsVector128(), right.AsVector128()).AsVector4();
            }
            else if (AdvSimd.IsSupported)
            {
                return AdvSimd.Subtract(left.AsVector128(), right.AsVector128()).AsVector4();
            }
            return new Vector4(left.X - right.X, left.Y - right.Y, left.Z - right.Z, left.W - right.W);
        }

        /// <summary>
        /// Multiplies two vectors together.
        /// </summary>
        /// <param name="left">The first source vector.</param>
        /// <param name="right">The second source vector.</param>
        /// <returns>The product vector.</returns>
#if ARM64
        [Intrinsic]
#endif
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 operator *(Vector4 left, Vector4 right)
        {
            if (Sse.IsSupported)
            {
                return Sse.Multiply(left.AsVector128(), right.AsVector128()).AsVector4();
            }
            return new Vector4(left.X * right.X, left.Y * right.Y, left.Z * right.Z, left.W * right.W);
        }

        /// <summary>
        /// Multiplies a vector by the given scalar.
        /// </summary>
        /// <param name="left">The source vector.</param>
        /// <param name="right">The scalar value.</param>
        /// <returns>The scaled vector.</returns>
#if ARM64
        [Intrinsic]
#endif
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 operator *(Vector4 left, float right)
        {
            if (Sse.IsSupported)
            {
                return Sse.Add(left.AsVector128(), Vector128.Create(right)).AsVector4();
            }
            return left * new Vector4(right);
        }

        /// <summary>
        /// Multiplies a vector by the given scalar.
        /// </summary>
        /// <param name="left">The scalar value.</param>
        /// <param name="right">The source vector.</param>
        /// <returns>The scaled vector.</returns>
#if ARM64
        [Intrinsic]
#endif
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 operator *(float left, Vector4 right)
        {
            if (Sse.IsSupported)
            {
                return Sse.Add(Vector128.Create(left), right.AsVector128()).AsVector4();
            }
            return new Vector4(left) * right;
        }

        /// <summary>
        /// Divides the first vector by the second.
        /// </summary>
        /// <param name="left">The first source vector.</param>
        /// <param name="right">The second source vector.</param>
        /// <returns>The vector resulting from the division.</returns>
#if ARM64
        [Intrinsic]
#endif
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 operator /(Vector4 left, Vector4 right)
        {
            if (Sse.IsSupported)
            {
                return Sse.Divide(left.AsVector128(), right.AsVector128()).AsVector4();
            }
            return new Vector4(left.X / right.X, left.Y / right.Y, left.Z / right.Z, left.W / right.W);
        }

        /// <summary>
        /// Divides the vector by the given scalar.
        /// </summary>
        /// <param name="value1">The source vector.</param>
        /// <param name="value2">The scalar value.</param>
        /// <returns>The result of the division.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 operator /(Vector4 value1, float value2)
        {
            if (Sse.IsSupported)
            {
                return Sse.Divide(value1.AsVector128(), Vector128.Create(value2)).AsVector4();
            }
            return value1 / new Vector4(value2);
        }

        /// <summary>
        /// Negates a given vector.
        /// </summary>
        /// <param name="value">The source vector.</param>
        /// <returns>The negated vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 operator -(Vector4 value)
        {
            if (Sse.IsSupported)
            {
                return Sse.Subtract(Vector128<float>.Zero, value.AsVector128()).AsVector4();
            }
            return Zero - value;
        }

        /// <summary>
        /// Returns a boolean indicating whether the two given vectors are equal.
        /// </summary>
        /// <param name="left">The first vector to compare.</param>
        /// <param name="right">The second vector to compare.</param>
        /// <returns>True if the vectors are equal; False otherwise.</returns>
#if ARM64
        [Intrinsic]
#endif
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Vector4 left, Vector4 right)
        {
            if (Sse.IsSupported)
            {
                return Sse.MoveMask(Sse.CompareEqual(left.AsVector128(), right.AsVector128())) == 0b1111;
            }
            return left.Equals(right);
        }

        /// <summary>
        /// Returns a boolean indicating whether the two given vectors are not equal.
        /// </summary>
        /// <param name="left">The first vector to compare.</param>
        /// <param name="right">The second vector to compare.</param>
        /// <returns>True if the vectors are not equal; False if they are equal.</returns>
#if ARM64
        [Intrinsic]
#endif
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Vector4 left, Vector4 right)
        {
            if (Sse.IsSupported)
            {
                return Sse.MoveMask(Sse.CompareNotEqual(left.AsVector128(), right.AsVector128())) == 0b1111;
            }
            return !(left == right);
        }
        #endregion Public static operators
    }
}
