// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;

#if BIT64

using nuint = System.UInt64;
#if PROJECTN
using nint = System.Int64;
#endif

#else
using nuint = System.UInt32;
#if PROJECTN
using nint = System.Int32;
#endif

#endif

namespace Internal.Runtime.CompilerServices
{
    //
    // Subsetted clone of System.Runtime.CompilerServices.Unsafe for internal runtime use.
    // Keep in sync with https://github.com/dotnet/corefx/tree/master/src/System.Runtime.CompilerServices.Unsafe.
    //

    /// <summary>
    /// For internal use only. Contains generic, low-level functionality for manipulating pointers.
    /// </summary>
    [CLSCompliant(false)]
    public static unsafe class Unsafe
    {
        /// <summary>
        /// Returns a pointer to the given by-ref parameter.
        /// </summary>
        [Intrinsic]
        [NonVersionable]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void* AsPointer<T>(ref T value)
        {
#if PROJECTN
            // This method is implemented by the toolchain
            throw new PlatformNotSupportedException();
#else
            // The body of this function will be replaced by the EE with unsafe code!!!
            // See getILIntrinsicImplementationForUnsafe for how this happens.  
            throw new InvalidOperationException();
#endif

            // ldarg.0
            // conv.u
            // ret
        }

        /// <summary>
        /// Returns the size of an object of the given type parameter.
        /// </summary>
        [Intrinsic]
        [NonVersionable]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int SizeOf<T>()
        {
#if PROJECTN
            // This method is implemented by the toolchain
            throw new PlatformNotSupportedException();
#else
            // The body of this function will be replaced by the EE with unsafe code that just returns sizeof !!T
            // See getILIntrinsicImplementationForUnsafe for how this happens.  
            typeof(T).ToString(); // Type token used by the actual method body
            throw new InvalidOperationException();
#endif

            // sizeof !!0
            // ret
        }

        /// <summary>
        /// Casts the given object to the specified type, performs no dynamic type checking.
        /// </summary>
        [Intrinsic]
        [NonVersionable]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T As<T>(object value) where T : class
        {
#if PROJECTN
            // This method is implemented by the toolchain
            throw new PlatformNotSupportedException();
#else
            // The body of this function will be replaced by the EE with unsafe code!!!
            // See getILIntrinsicImplementationForUnsafe for how this happens.
            throw new InvalidOperationException();
#endif

            // ldarg.0
            // ret
        }

        /// <summary>
        /// Reinterprets the given reference as a reference to a value of type <typeparamref name="TTo"/>.
        /// </summary>
        [Intrinsic]
        [NonVersionable]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref TTo As<TFrom, TTo>(ref TFrom source)
        {
#if PROJECTN
            // This method is implemented by the toolchain
            throw new PlatformNotSupportedException();
#else
            // The body of this function will be replaced by the EE with unsafe code!!!
            // See getILIntrinsicImplementationForUnsafe for how this happens.
            throw new InvalidOperationException();
#endif

            // ldarg.0
            // ret
        }

        /// <summary>
        /// Adds an element offset to the given reference.
        /// </summary>
        [NonVersionable]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T Add<T>(ref T source, int elementOffset)
        {
#if PROJECTN
            return ref AddByteOffset(ref source, (IntPtr)(elementOffset * (nint)SizeOf<T>()));
#else
            // The body of this function will be replaced by the EE with unsafe code!!!
            // See getILIntrinsicImplementationForUnsafe for how this happens.
            typeof(T).ToString(); // Type token used by the actual method body
            throw new InvalidOperationException();
#endif
        }

        /// <summary>
        /// Adds an element offset to the given pointer.
        /// </summary>
        [NonVersionable]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void* Add<T>(void* source, int elementOffset)
        {
#if PROJECTN
            return (byte*)source + (elementOffset * (nint)SizeOf<T>());
#else
            // The body of this function will be replaced by the EE with unsafe code!!!
            // See getILIntrinsicImplementationForUnsafe for how this happens.
            typeof(T).ToString(); // Type token used by the actual method body
            throw new InvalidOperationException();
#endif
        }

        /// <summary>
        /// Adds an element offset to the given reference.
        /// </summary>
        [Intrinsic]
        [NonVersionable]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T AddByteOffset<T>(ref T source, nuint byteOffset)
        {
#if PROJECTN
            return ref AddByteOffset(ref source, (IntPtr)(void*)byteOffset);
#else
            // The body of this function will be replaced by the EE with unsafe code!!!
            // See getILIntrinsicImplementationForUnsafe for how this happens.
            throw new InvalidOperationException();
#endif
        }

        /// <summary>
        /// Determines whether the specified references point to the same location.
        /// </summary>
        [Intrinsic]
        [NonVersionable]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool AreSame<T>(ref T left, ref T right)
        {
#if PROJECTN
            // This method is implemented by the toolchain
            throw new PlatformNotSupportedException();
#else
            // The body of this function will be replaced by the EE with unsafe code!!!
            // See getILIntrinsicImplementationForUnsafe for how this happens.  
            throw new InvalidOperationException();
#endif

            // ldarg.0
            // ldarg.1
            // ceq
            // ret
        }

        /// <summary>
        /// Initializes a block of memory at the given location with a given initial value 
        /// without assuming architecture dependent alignment of the address.
        /// </summary>
        [Intrinsic]
        [NonVersionable]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void InitBlockUnaligned(ref byte startAddress, byte value, uint byteCount)
        {
#if PROJECTN
            for (uint i = 0; i < byteCount; i++)
                AddByteOffset(ref startAddress, i) = value;
#else
            // The body of this function will be replaced by the EE with unsafe code!!!
            // See getILIntrinsicImplementationForUnsafe for how this happens.  
            throw new InvalidOperationException();
#endif
        }

        /// <summary>
        /// Reads a value of type <typeparamref name="T"/> from the given location.
        /// </summary>
        [Intrinsic]
        [NonVersionable]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T ReadUnaligned<T>(void* source)
        {
#if PROJECTN
            return Unsafe.As<byte, T>(ref *(byte*)source);
#else
            // The body of this function will be replaced by the EE with unsafe code!!!
            // See getILIntrinsicImplementationForUnsafe for how this happens.  
            typeof(T).ToString(); // Type token used by the actual method body
            throw new InvalidOperationException();
#endif
        }

        /// <summary>
        /// Reads a value of type <typeparamref name="T"/> from the given location.
        /// </summary>
        [Intrinsic]
        [NonVersionable]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T ReadUnaligned<T>(ref byte source)
        {
#if PROJECTN
            return Unsafe.As<byte, T>(ref source);
#else
            // The body of this function will be replaced by the EE with unsafe code!!!
            // See getILIntrinsicImplementationForUnsafe for how this happens.  
            typeof(T).ToString(); // Type token used by the actual method body
            throw new InvalidOperationException();
#endif
        }

        /// <summary>
        /// Writes a value of type <typeparamref name="T"/> to the given location.
        /// </summary>
        [Intrinsic]
        [NonVersionable]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteUnaligned<T>(void* destination, T value)
        {
#if PROJECTN
            Unsafe.As<byte, T>(ref *(byte*)destination) = value;
#else
            // The body of this function will be replaced by the EE with unsafe code!!!
            // See getILIntrinsicImplementationForUnsafe for how this happens.  
            typeof(T).ToString(); // Type token used by the actual method body
            throw new InvalidOperationException();
#endif
        }

        /// <summary>
        /// Writes a value of type <typeparamref name="T"/> to the given location.
        /// </summary>
        [Intrinsic]
        [NonVersionable]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteUnaligned<T>(ref byte destination, T value)
        {
#if PROJECTN
            Unsafe.As<byte, T>(ref destination) = value;
#else
            // The body of this function will be replaced by the EE with unsafe code!!!
            // See getILIntrinsicImplementationForUnsafe for how this happens.  
            typeof(T).ToString(); // Type token used by the actual method body
            throw new InvalidOperationException();
#endif
        }

#if PROJECTN

        /// <summary>
        /// Adds an element offset to the given reference.
        /// </summary>
        [Intrinsic]
        [NonVersionable]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T AddByteOffset<T>(ref T source, IntPtr byteOffset)
        {
            // This method is implemented by the toolchain
            throw new PlatformNotSupportedException();

            // ldarg.0
            // ldarg.1
            // add
            // ret
        }

        /// <summary>
        /// Reads a value of type <typeparamref name="T"/> from the given location.
        /// </summary>
        [Intrinsic]
        [NonVersionable]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Read<T>(void* source)
        {
            return Unsafe.As<byte, T>(ref *(byte*)source);
        }

        /// <summary>
        /// Reads a value of type <typeparamref name="T"/> from the given location.
        /// </summary>
        [Intrinsic]
        [NonVersionable]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Read<T>(ref byte source)
        {
            return Unsafe.As<byte, T>(ref source);
        }

        /// <summary>
        /// Writes a value of type <typeparamref name="T"/> to the given location.
        /// </summary>
        [Intrinsic]
        [NonVersionable]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Write<T>(void* destination, T value)
        {
            Unsafe.As<byte, T>(ref *(byte*)destination) = value;
        }

        /// <summary>
        /// Writes a value of type <typeparamref name="T"/> to the given location.
        /// </summary>
        [Intrinsic]
        [NonVersionable]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Write<T>(ref byte destination, T value)
        {
            Unsafe.As<byte, T>(ref destination) = value;
        }

#else

        /// <summary>
        /// Reinterprets the given location as a reference to a value of type <typeparamref name="T"/>.
        /// </summary>
        [NonVersionable]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T AsRef<T>(void* source)
        {
            // The body of this function will be replaced by the EE with unsafe code!!!
            // See getILIntrinsicImplementationForUnsafe for how this happens.  
            throw new InvalidOperationException();
        }


        /// <summary>
        /// Determines the byte offset from origin to target from the given references.
        /// </summary>
        [NonVersionable]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IntPtr ByteOffset<T>(ref T origin, ref T target)
        {
            // The body of this function will be replaced by the EE with unsafe code!!!
            // See getILIntrinsicImplementationForUnsafe for how this happens.
            throw new InvalidOperationException();
        }

#endif
    }
}
