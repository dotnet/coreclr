// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Internal.Runtime.CompilerServices;

namespace System.Runtime.Intrinsics
{
    [Intrinsic]
    [DebuggerDisplay("{DisplayString,nq}")]
    [DebuggerTypeProxy(typeof(Vector256DebugView<>))]
    [StructLayout(LayoutKind.Sequential, Size = Vector256.Size)]
    public readonly struct Vector256<T> where T : struct
    {
        // These fields exist to ensure the alignment is 8, rather than 1.
        // This also allows the debug view to work https://github.com/dotnet/coreclr/issues/15694)
        private readonly ulong _00;
        private readonly ulong _01;
        private readonly ulong _02;
        private readonly ulong _03;

        public static Vector256<T> Zero
        {
            get
            {
                ThrowIfUnsupportedType();
                return default;
            }
        }

        internal unsafe string DisplayString
        {
            get
            {
                if (IsSupported)
                {
                    var items = new T[ElementCount];
                    Unsafe.WriteUnaligned(ref Unsafe.As<T, byte>(ref items[0]), this);
                    return $"({string.Join(", ", items)})";
                }
                else
                {
                    return SR.NotSupported_Type;
                }
            }
        }

        internal static int ElementCount
        {
            get
            {
                ThrowIfUnsupportedType();
                return Vector256.Size / Unsafe.SizeOf<T>();
            }
        }

        internal static bool IsSupported
        {
            get
            {
                return (typeof(T) == typeof(byte)) ||
                       (typeof(T) == typeof(sbyte)) ||
                       (typeof(T) == typeof(short)) ||
                       (typeof(T) == typeof(ushort)) ||
                       (typeof(T) == typeof(int)) ||
                       (typeof(T) == typeof(uint)) ||
                       (typeof(T) == typeof(long)) ||
                       (typeof(T) == typeof(ulong)) ||
                       (typeof(T) == typeof(float)) ||
                       (typeof(T) == typeof(double));
            }
        }

        internal static void ThrowIfUnsupportedType()
        {
            if (!IsSupported)
            {
                throw new NotSupportedException(SR.Arg_TypeNotSupported);
            }
        }

        public Vector256<U> As<U>() where U : struct
        {
            ThrowIfUnsupportedType();
            Vector256<U>.ThrowIfUnsupportedType();
            return Unsafe.As<Vector256<T>, Vector256<U>>(ref Unsafe.AsRef(in this));
        }

        public Vector256<byte> AsByte() => As<byte>();

        public Vector256<double> AsDouble() => As<double>();

        public Vector256<short> AsInt16() => As<short>();

        public Vector256<int> AsInt32() => As<int>();

        public Vector256<long> AsInt64() => As<long>();

        [CLSCompliant(false)]
        public Vector256<sbyte> AsSByte() => As<sbyte>();

        public Vector256<float> AsSingle() => As<float>();

        [CLSCompliant(false)]
        public Vector256<ushort> AsUInt16() => As<ushort>();

        [CLSCompliant(false)]
        public Vector256<uint> AsUInt32() => As<uint>();

        [CLSCompliant(false)]
        public Vector256<ulong> AsUInt64() => As<ulong>();

        public T GetElement(int index)
        {
            ThrowIfUnsupportedType();

            if ((uint)(index) >= (uint)(ElementCount))
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            ref T e0 = ref Unsafe.As<Vector256<T>, T>(ref Unsafe.AsRef(in this));
            return Unsafe.Add(ref e0, index);
        }

        public Vector256<T> SetElement(int index, T value)
        {
            ThrowIfUnsupportedType();

            if ((uint)(index) >= (uint)(ElementCount))
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            Vector256<T> result = this;
            ref T e0 = ref Unsafe.As<Vector256<T>, T>(ref result);
            Unsafe.Add(ref e0, index) = value;
            return result;
        }

        public Vector128<T> GetLower()
        {
            ThrowIfUnsupportedType();
            Vector128<T>.ThrowIfUnsupportedType();
            return Unsafe.As<Vector256<T>, Vector128<T>>(ref Unsafe.AsRef(in this));
        }

        public Vector256<T> SetLower(Vector128<T> value)
        {
            ThrowIfUnsupportedType();
            Vector128<T>.ThrowIfUnsupportedType();

            Vector256<T> result = this;
            Unsafe.As<Vector256<T>, Vector128<T>>(ref result) = value;
            return result;
        }

        public Vector128<T> GetUpper()
        {
            ThrowIfUnsupportedType();
            Vector128<T>.ThrowIfUnsupportedType();

            ref Vector128<T> lower = ref Unsafe.As<Vector256<T>, Vector128<T>>(ref Unsafe.AsRef(in this));
            return Unsafe.Add(ref lower, 1);
        }

        public Vector256<T> SetUpper(Vector128<T> value)
        {
            ThrowIfUnsupportedType();
            Vector128<T>.ThrowIfUnsupportedType();

            Vector256<T> result = this;
            ref Vector128<T> lower = ref Unsafe.As<Vector256<T>, Vector128<T>>(ref result);
            Unsafe.Add(ref lower, 1) = value;
            return result;
        }

        public T ToScalar()
        {
            ThrowIfUnsupportedType();
            return Unsafe.As<Vector256<T>, T>(ref Unsafe.AsRef(in this));
        }
    }
}
