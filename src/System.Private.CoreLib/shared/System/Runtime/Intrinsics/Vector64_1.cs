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
    [DebuggerTypeProxy(typeof(Vector64DebugView<>))]
    [StructLayout(LayoutKind.Sequential, Size = Vector64.Size)]
    public readonly struct Vector64<T> where T : struct
    {
        // These fields exist to ensure the alignment is 8, rather than 1.
        // This also allows the debug view to work https://github.com/dotnet/coreclr/issues/15694)
        private readonly ulong _00;

        public static Vector64<T> Zero
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
                return Vector64.Size / Unsafe.SizeOf<T>();
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

        public Vector64<U> As<U>() where U : struct
        {
            ThrowIfUnsupportedType();
            Vector64<U>.ThrowIfUnsupportedType();
            return Unsafe.As<Vector64<T>, Vector64<U>>(ref Unsafe.AsRef(in this));
        }

        public Vector64<byte> AsByte() => As<byte>();

        public Vector64<double> AsDouble() => As<double>();

        public Vector64<short> AsInt16() => As<short>();

        public Vector64<int> AsInt32() => As<int>();

        public Vector64<long> AsInt64() => As<long>();

        [CLSCompliant(false)]
        public Vector64<sbyte> AsSByte() => As<sbyte>();

        public Vector64<float> AsSingle() => As<float>();

        [CLSCompliant(false)]
        public Vector64<ushort> AsUInt16() => As<ushort>();

        [CLSCompliant(false)]
        public Vector64<uint> AsUInt32() => As<uint>();

        [CLSCompliant(false)]
        public Vector64<ulong> AsUInt64() => As<ulong>();

        public T GetElement(int index)
        {
            ThrowIfUnsupportedType();

            if ((uint)(index) >= (uint)(ElementCount))
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            ref T e0 = ref Unsafe.As<Vector64<T>, T>(ref Unsafe.AsRef(in this));
            return Unsafe.Add(ref e0, index);
        }

        public Vector64<T> SetElement(int index, T value)
        {
            ThrowIfUnsupportedType();

            if ((uint)(index) >= (uint)(ElementCount))
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            Vector64<T> result = this;
            ref T e0 = ref Unsafe.As<Vector64<T>, T>(ref result);
            Unsafe.Add(ref e0, index) = value;
            return result;
        }

        public T ToScalar()
        {
            ThrowIfUnsupportedType();
            return Unsafe.As<Vector64<T>, T>(ref Unsafe.AsRef(in this));
        }

        public Vector128<T> ToVector128()
        {
            ThrowIfUnsupportedType();
            Vector128<T>.ThrowIfUnsupportedType();

            Vector128<T> result = Vector128<T>.Zero;
            Unsafe.As<Vector128<T>, Vector64<T>>(ref result) = this;
            return result;
        }

        public unsafe Vector128<T> ToVector128Unsafe()
        {
            ThrowIfUnsupportedType();
            Vector128<T>.ThrowIfUnsupportedType();

            // This relies on us stripping the "init" flag from the ".locals"
            // declaration to let the upper bits be uninitialized.

            var pResult = stackalloc byte[Vector128.Size];
            Unsafe.AsRef<Vector64<T>>(pResult) = this;
            return Unsafe.AsRef<Vector128<T>>(pResult);
        }
    }
}
