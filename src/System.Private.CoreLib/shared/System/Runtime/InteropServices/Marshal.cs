// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Security;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Win32;

using Internal.Runtime.CompilerServices;

namespace System.Runtime.InteropServices
{
    /// <summary>
    /// This class contains methods that are mainly used to marshal between unmanaged
    /// and managed types.
    /// </summary>
    public static partial class Marshal
    {
        /// <summary>
        /// The default character size for the system. This is always 2 because
        /// the framework only runs on UTF-16 systems.
        /// </summary>
        public static readonly int SystemDefaultCharSize = 2;

        /// <summary>
        /// The max DBCS character size for the system.
        /// </summary>
        public static readonly int SystemMaxDBCSCharSize = GetSystemMaxDBCSCharSize();

        public static unsafe string PtrToStringAnsi(IntPtr ptr)
        {
            if (IntPtr.Zero == ptr)
            {
                return null;
            }
            else if (IsWin32Atom(ptr))
            {
                return null;
            }

            return new string((sbyte*)ptr);
        }

        public static unsafe string PtrToStringAnsi(IntPtr ptr, int len)
        {
            if (ptr == IntPtr.Zero)
            {
                throw new ArgumentNullException(nameof(ptr));
            }
            if (len < 0)
            {
                throw new ArgumentException(null, nameof(len));
            }

            return new string((sbyte*)ptr, 0, len);
        }

        public static unsafe string PtrToStringUni(IntPtr ptr, int len)
        {
            if (ptr == IntPtr.Zero)
            {
                throw new ArgumentNullException(nameof(ptr));
            }
            if (len < 0)
            {
                throw new ArgumentException(SR.ArgumentOutOfRange_NeedNonNegNum, nameof(len));
            }

            return new string((char*)ptr, 0, len);
        }

        public static string PtrToStringAuto(IntPtr ptr, int len)
        {
            // Ansi platforms are no longer supported
            return PtrToStringUni(ptr, len);
        }

        public static unsafe string PtrToStringUni(IntPtr ptr)
        {
            if (IntPtr.Zero == ptr)
            {
                return null;
            }
            else if (IsWin32Atom(ptr))
            {
                return null;
            }

            return new string((char*)ptr);
        }

        public static string PtrToStringAuto(IntPtr ptr)
        {
            // Ansi platforms are no longer supported
            return PtrToStringUni(ptr);
        }

        public static unsafe string PtrToStringUTF8(IntPtr ptr)
        {
            if (IntPtr.Zero == ptr)
            {
                return null;
            }

            int nbBytes = string.strlen((byte*)ptr);
            return PtrToStringUTF8(ptr, nbBytes);
        }

        public static unsafe string PtrToStringUTF8(IntPtr ptr, int byteLen)
        {
            if (byteLen < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(byteLen), SR.ArgumentOutOfRange_NeedNonNegNum);
            }
            else if (IntPtr.Zero == ptr)
            {
                return null;
            }
            else if (IsWin32Atom(ptr))
            {
                return null;
            }
            else if (byteLen == 0)
            {
                return string.Empty;
            }

            return Encoding.UTF8.GetString((byte*)ptr, byteLen);
        }

        public static int SizeOf(object structure)
        {
            if (structure == null)
            {
                throw new ArgumentNullException(nameof(structure));
            }

            return SizeOfHelper(structure.GetType(), throwIfNotMarshalable: true);
        }

        public static int SizeOf<T>(T structure)
        {
            if (structure == null)
            {
                throw new ArgumentNullException(nameof(structure));
            }

            return SizeOfHelper(structure.GetType(), throwIfNotMarshalable: true);
        }

        public static int SizeOf(Type t)
        {
            if (t == null)
            {
                throw new ArgumentNullException(nameof(t));
            }
            if (!(t is RuntimeType))
            {
                throw new ArgumentException(SR.Argument_MustBeRuntimeType, nameof(t));
            }
            if (t.IsGenericType)
            {
                throw new ArgumentException(SR.Argument_NeedNonGenericType, nameof(t));
            }

            return SizeOfHelper(t, throwIfNotMarshalable: true);
        }

        public static int SizeOf<T>() => SizeOf(typeof(T));        

        public static IntPtr OffsetOf<T>(string fieldName) => OffsetOf(typeof(T), fieldName);

        public static void Copy(int[] source, int startIndex, IntPtr destination, int length)
        {
            CopyToNative(source, startIndex, destination, length);
        }

        public static void Copy(char[] source, int startIndex, IntPtr destination, int length)
        {
            CopyToNative(source, startIndex, destination, length);
        }

        public static void Copy(short[] source, int startIndex, IntPtr destination, int length)
        {
            CopyToNative(source, startIndex, destination, length);
        }

        public static void Copy(long[] source, int startIndex, IntPtr destination, int length)
        {
            CopyToNative(source, startIndex, destination, length);
        }

        public static void Copy(float[] source, int startIndex, IntPtr destination, int length)
        {
            CopyToNative(source, startIndex, destination, length);
        }

        public static void Copy(double[] source, int startIndex, IntPtr destination, int length)
        {
            CopyToNative(source, startIndex, destination, length);
        }

        public static void Copy(byte[] source, int startIndex, IntPtr destination, int length)
        {
            CopyToNative(source, startIndex, destination, length);
        }

        public static void Copy(IntPtr[] source, int startIndex, IntPtr destination, int length)
        {
            CopyToNative(source, startIndex, destination, length);
        }

        private static unsafe void CopyToNative<T>(T[] source, int startIndex, IntPtr destination, int length)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (destination == IntPtr.Zero)
                throw new ArgumentNullException(nameof(destination));

            // The rest of the argument validation is done by CopyTo

            new Span<T>(source, startIndex, length).CopyTo(new Span<T>((void*)destination, length));
        }

        public static void Copy(IntPtr source, int[] destination, int startIndex, int length)
        {
            CopyToManaged(source, destination, startIndex, length);
        }

        public static void Copy(IntPtr source, char[] destination, int startIndex, int length)
        {
            CopyToManaged(source, destination, startIndex, length);
        }

        public static void Copy(IntPtr source, short[] destination, int startIndex, int length)
        {
            CopyToManaged(source, destination, startIndex, length);
        }

        public static void Copy(IntPtr source, long[] destination, int startIndex, int length)
        {
            CopyToManaged(source, destination, startIndex, length);
        }

        public static void Copy(IntPtr source, float[] destination, int startIndex, int length)
        {
            CopyToManaged(source, destination, startIndex, length);
        }

        public static void Copy(IntPtr source, double[] destination, int startIndex, int length)
        {
            CopyToManaged(source, destination, startIndex, length);
        }

        public static void Copy(IntPtr source, byte[] destination, int startIndex, int length)
        {
            CopyToManaged(source, destination, startIndex, length);
        }

        public static void Copy(IntPtr source, IntPtr[] destination, int startIndex, int length)
        {
            CopyToManaged(source, destination, startIndex, length);
        }

        private static unsafe void CopyToManaged<T>(IntPtr source, T[] destination, int startIndex, int length)
        {
            if (source == IntPtr.Zero)
                throw new ArgumentNullException(nameof(source));
            if (destination == null)
                throw new ArgumentNullException(nameof(destination));
            if (startIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(startIndex), SR.ArgumentOutOfRange_StartIndex);
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length), SR.ArgumentOutOfRange_NeedNonNegNum);

            // The rest of the argument validation is done by CopyTo

            new Span<T>((void*)source, length).CopyTo(new Span<T>(destination, startIndex, length));
        }

        public static unsafe byte ReadByte(IntPtr ptr, int ofs)
        {
            try
            {
                byte* addr = (byte*)ptr + ofs;
                return *addr;
            }
            catch (NullReferenceException)
            {
                // this method is documented to throw AccessViolationException on any AV
                throw new AccessViolationException();
            }
        }

        public static byte ReadByte(IntPtr ptr) => ReadByte(ptr, 0);

        public static unsafe short ReadInt16(IntPtr ptr, int ofs)
        {
            try
            {
                byte* addr = (byte*)ptr + ofs;
                if ((unchecked((int)addr) & 0x1) == 0)
                {
                    // aligned read
                    return *((short*)addr);
                }
                else
                {
                    return Unsafe.ReadUnaligned<short>(addr);
                }
            }
            catch (NullReferenceException)
            {
                // this method is documented to throw AccessViolationException on any AV
                throw new AccessViolationException();
            }
        }

        public static short ReadInt16(IntPtr ptr) => ReadInt16(ptr, 0);

        public static unsafe int ReadInt32(IntPtr ptr, int ofs)
        {
            try
            {
                byte* addr = (byte*)ptr + ofs;
                if ((unchecked((int)addr) & 0x3) == 0)
                {
                    // aligned read
                    return *((int*)addr);
                }
                else
                {
                    return Unsafe.ReadUnaligned<int>(addr);
                }
            }
            catch (NullReferenceException)
            {
                // this method is documented to throw AccessViolationException on any AV
                throw new AccessViolationException();
            }
        }

        public static int ReadInt32(IntPtr ptr) => ReadInt32(ptr, 0);

        public static IntPtr ReadIntPtr(object ptr, int ofs)
        {
#if BIT64
            return (IntPtr)ReadInt64(ptr, ofs);
#else // 32
            return (IntPtr)ReadInt32(ptr, ofs);
#endif
        }

        public static IntPtr ReadIntPtr(IntPtr ptr, int ofs)
        {
#if BIT64
            return (IntPtr)ReadInt64(ptr, ofs);
#else // 32
            return (IntPtr)ReadInt32(ptr, ofs);
#endif
        }

        public static IntPtr ReadIntPtr(IntPtr ptr) => ReadIntPtr(ptr, 0);

        public static unsafe long ReadInt64(IntPtr ptr, int ofs)
        {
            try
            {
                byte* addr = (byte*)ptr + ofs;
                if ((unchecked((int)addr) & 0x7) == 0)
                {
                    // aligned read
                    return *((long*)addr);
                }
                else
                {
                    return Unsafe.ReadUnaligned<long>(addr);
                }
            }
            catch (NullReferenceException)
            {
                // this method is documented to throw AccessViolationException on any AV
                throw new AccessViolationException();
            }
        }

        public static long ReadInt64(IntPtr ptr) => ReadInt64(ptr, 0);

        public static unsafe void WriteByte(IntPtr ptr, int ofs, byte val)
        {
            try
            {
                byte* addr = (byte*)ptr + ofs;
                *addr = val;
            }
            catch (NullReferenceException)
            {
                // this method is documented to throw AccessViolationException on any AV
                throw new AccessViolationException();
            }
        }

        public static void WriteByte(IntPtr ptr, byte val) => WriteByte(ptr, 0, val);

        public static unsafe void WriteInt16(IntPtr ptr, int ofs, short val)
        {
            try
            {
                byte* addr = (byte*)ptr + ofs;
                if ((unchecked((int)addr) & 0x1) == 0)
                {
                    // aligned write
                    *((short*)addr) = val;
                }
                else
                {
                    Unsafe.WriteUnaligned(addr, val);
                }
            }
            catch (NullReferenceException)
            {
                // this method is documented to throw AccessViolationException on any AV
                throw new AccessViolationException();
            }
        }

        public static void WriteInt16(IntPtr ptr, short val) => WriteInt16(ptr, 0, val);

        public static void WriteInt16(IntPtr ptr, int ofs, char val) => WriteInt16(ptr, ofs, (short)val);

        public static void WriteInt16([In, Out]object ptr, int ofs, char val) => WriteInt16(ptr, ofs, (short)val);

        public static void WriteInt16(IntPtr ptr, char val) => WriteInt16(ptr, 0, (short)val);

        public static unsafe void WriteInt32(IntPtr ptr, int ofs, int val)
        {
            try
            {
                byte* addr = (byte*)ptr + ofs;
                if ((unchecked((int)addr) & 0x3) == 0)
                {
                    // aligned write
                    *((int*)addr) = val;
                }
                else
                {
                    Unsafe.WriteUnaligned(addr, val);
                }
            }
            catch (NullReferenceException)
            {
                // this method is documented to throw AccessViolationException on any AV
                throw new AccessViolationException();
            }
        }

        public static void WriteInt32(IntPtr ptr, int val) => WriteInt32(ptr, 0, val);

        public static void WriteIntPtr(IntPtr ptr, int ofs, IntPtr val)
        {
#if BIT64
            WriteInt64(ptr, ofs, (long)val);
#else // 32
            WriteInt32(ptr, ofs, (int)val);
#endif
        }

        public static void WriteIntPtr(object ptr, int ofs, IntPtr val)
        {
#if BIT64
            WriteInt64(ptr, ofs, (long)val);
#else // 32
            WriteInt32(ptr, ofs, (int)val);
#endif
        }

        public static void WriteIntPtr(IntPtr ptr, IntPtr val) => WriteIntPtr(ptr, 0, val);

        public static unsafe void WriteInt64(IntPtr ptr, int ofs, long val)
        {
            try
            {
                byte* addr = (byte*)ptr + ofs;
                if ((unchecked((int)addr) & 0x7) == 0)
                {
                    // aligned write
                    *((long*)addr) = val;
                }
                else
                {
                    Unsafe.WriteUnaligned(addr, val);
                }
            }
            catch (NullReferenceException)
            {
                // this method is documented to throw AccessViolationException on any AV
                throw new AccessViolationException();
            }
        }

        public static void WriteInt64(IntPtr ptr, long val) => WriteInt64(ptr, 0, val);

        public static void Prelink(MethodInfo m)
        {
            if (m == null)
            {
                throw new ArgumentNullException(nameof(m));
            }
            if (!(m is RuntimeMethodInfo rmi))
            {
                throw new ArgumentException(SR.Argument_MustBeRuntimeMethodInfo, nameof(m));
            }

            InternalPrelink(rmi);
        }

        public static void PrelinkAll(Type c)
        {
            if (c == null)
            {
                throw new ArgumentNullException(nameof(c));
            }

            MethodInfo[] mi = c.GetMethods();
            if (mi != null)
            {
                for (int i = 0; i < mi.Length; i++)
                {
                    Prelink(mi[i]);
                }
            }
        }

        public static void StructureToPtr<T>(T structure, IntPtr ptr, bool fDeleteOld)
        {
            StructureToPtr((object)structure, ptr, fDeleteOld);
        }

        /// <summary>
        /// Marshals data from a native memory block to a preallocated structure class.
        /// </summary>
        public static void PtrToStructure(IntPtr ptr, object structure)
        {
            PtrToStructureHelper(ptr, structure, allowValueClasses: false);
        }

        public static void PtrToStructure<T>(IntPtr ptr, T structure)
        {
            PtrToStructure(ptr, (object)structure);
        }

        public static T PtrToStructure<T>(IntPtr ptr) => (T)PtrToStructure(ptr, typeof(T));

        public static void DestroyStructure<T>(IntPtr ptr) => DestroyStructure(ptr, typeof(T));

        /// <summary>
        /// Converts the HRESULT to a CLR exception.
        /// </summary>
        public static Exception GetExceptionForHR(int errorCode)
        {
            if (errorCode >= 0)
            {
                return null;
            }

            return GetExceptionForHRInternal(errorCode, IntPtr.Zero);
        }
        public static Exception GetExceptionForHR(int errorCode, IntPtr errorInfo)
        {
            if (errorCode >= 0)
            {
                return null;
            }

            return GetExceptionForHRInternal(errorCode, errorInfo);
        }

        public static IntPtr SecureStringToBSTR(SecureString s)
        {
            if (s == null)
            {
                throw new ArgumentNullException(nameof(s));
            }

            return s.MarshalToBSTR();
        }

        public static IntPtr SecureStringToCoTaskMemAnsi(SecureString s)
        {
            if (s == null)
            {
                throw new ArgumentNullException(nameof(s));
            }

            return s.MarshalToString(globalAlloc: false, unicode: false);
        }

        public static IntPtr SecureStringToCoTaskMemUnicode(SecureString s)
        {
            if (s == null)
            {
                throw new ArgumentNullException(nameof(s));
            }

            return s.MarshalToString(globalAlloc: false, unicode: true);
        }

        public static IntPtr SecureStringToGlobalAllocAnsi(SecureString s)
        {
            if (s == null)
            {
                throw new ArgumentNullException(nameof(s));
            }

            return s.MarshalToString(globalAlloc: true, unicode: false);
        }

        public static IntPtr SecureStringToGlobalAllocUnicode(SecureString s)
        {
            if (s == null)
            {
                throw new ArgumentNullException(nameof(s));
            }

            return s.MarshalToString(globalAlloc: true, unicode: true); ;
        }

        public static Delegate GetDelegateForFunctionPointer(IntPtr ptr, Type t)
        {
            if (ptr == IntPtr.Zero)
            {
                throw new ArgumentNullException(nameof(ptr));
            }
            if (t == null)
            {
                throw new ArgumentNullException(nameof(t));
            }
            if (!(t is RuntimeType))
            {
                throw new ArgumentException(SR.Argument_MustBeRuntimeType, nameof(t));
            }
            if (t.IsGenericType)
            {
                throw new ArgumentException(SR.Argument_NeedNonGenericType, nameof(t));
            }

            Type c = t.BaseType;
            if (c == null || (c != typeof(Delegate) && c != typeof(MulticastDelegate)))
            {
                throw new ArgumentException(SR.Arg_MustBeDelegate, nameof(t));
            }

            return GetDelegateForFunctionPointerInternal(ptr, t);
        }

        public static TDelegate GetDelegateForFunctionPointer<TDelegate>(IntPtr ptr)
        {
            return (TDelegate)(object)GetDelegateForFunctionPointer(ptr, typeof(TDelegate));
        }

        public static IntPtr GetFunctionPointerForDelegate(Delegate d)
        {
            if (d == null)
            {
                throw new ArgumentNullException(nameof(d));
            }

            return GetFunctionPointerForDelegateInternal(d);
        }

        public static IntPtr GetFunctionPointerForDelegate<TDelegate>(TDelegate d)
        {
            return GetFunctionPointerForDelegate((Delegate)(object)d);
        }

    }
}