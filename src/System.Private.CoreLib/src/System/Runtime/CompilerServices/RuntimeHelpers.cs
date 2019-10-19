// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.InteropServices;
using Internal.Runtime.CompilerServices;

#pragma warning disable SA1121 // explicitly using type aliases instead of built-in types
#if BIT64
using nuint = System.UInt64;
#else
using nuint = System.UInt32;
#endif

namespace System.Runtime.CompilerServices
{
    public static partial class RuntimeHelpers
    {
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void InitializeArray(Array array, RuntimeFieldHandle fldHandle);

        // GetObjectValue is intended to allow value classes to be manipulated as 'Object'
        // but have aliasing behavior of a value class.  The intent is that you would use
        // this function just before an assignment to a variable of type 'Object'.  If the
        // value being assigned is a mutable value class, then a shallow copy is returned
        // (because value classes have copy semantics), but otherwise the object itself
        // is returned.
        //
        // Note: VB calls this method when they're about to assign to an Object
        // or pass it as a parameter.  The goal is to make sure that boxed
        // value types work identical to unboxed value types - ie, they get
        // cloned when you pass them around, and are always passed by value.
        // Of course, reference types are not cloned.
        //
        [MethodImpl(MethodImplOptions.InternalCall)]
        [return: NotNullIfNotNull("obj")]
        public static extern object? GetObjectValue(object? obj);

        // RunClassConstructor causes the class constructor for the given type to be triggered
        // in the current domain.  After this call returns, the class constructor is guaranteed to
        // have at least been started by some thread.  In the absence of class constructor
        // deadlock conditions, the call is further guaranteed to have completed.
        //
        // This call will generate an exception if the specified class constructor threw an
        // exception when it ran.

        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern void _RunClassConstructor(RuntimeType type);

        public static void RunClassConstructor(RuntimeTypeHandle type)
        {
            _RunClassConstructor(type.GetRuntimeType());
        }

        // RunModuleConstructor causes the module constructor for the given type to be triggered
        // in the current domain.  After this call returns, the module constructor is guaranteed to
        // have at least been started by some thread.  In the absence of module constructor
        // deadlock conditions, the call is further guaranteed to have completed.
        //
        // This call will generate an exception if the specified module constructor threw an
        // exception when it ran.

        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern void _RunModuleConstructor(System.Reflection.RuntimeModule module);

        public static void RunModuleConstructor(ModuleHandle module)
        {
            _RunModuleConstructor(module.GetRuntimeModule());
        }


        [DllImport(JitHelpers.QCall, CharSet = CharSet.Unicode)]
        internal static extern void _CompileMethod(RuntimeMethodHandleInternal method);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern unsafe void _PrepareMethod(IRuntimeMethodInfo method, IntPtr* pInstantiation, int cInstantiation);

        public static void PrepareMethod(RuntimeMethodHandle method)
        {
            unsafe
            {
                _PrepareMethod(method.GetMethodInfo(), null, 0);
            }
        }

        public static void PrepareMethod(RuntimeMethodHandle method, RuntimeTypeHandle[]? instantiation)
        {
            unsafe
            {
                int length;
                IntPtr[]? instantiationHandles = RuntimeTypeHandle.CopyRuntimeTypeHandles(instantiation, out length);
                fixed (IntPtr* pInstantiation = instantiationHandles)
                {
                    _PrepareMethod(method.GetMethodInfo(), pInstantiation, length);
                    GC.KeepAlive(instantiation);
                }
            }
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void PrepareDelegate(Delegate d);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern int GetHashCode(object o);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern new bool Equals(object? o1, object? o2);

        public static int OffsetToStringData
        {
            // This offset is baked in by string indexer intrinsic, so there is no harm
            // in getting it baked in here as well.
            [System.Runtime.Versioning.NonVersionable]
            get =>
                // Number of bytes from the address pointed to by a reference to
                // a String to the first 16-bit character in the String.  Skip
                // over the MethodTable pointer, & String
                // length.  Of course, the String reference points to the memory
                // after the sync block, so don't count that.
                // This property allows C#'s fixed statement to work on Strings.
                // On 64 bit platforms, this should be 12 (8+4) and on 32 bit 8 (4+4).
#if BIT64
                12;
#else // 32
                8;
#endif // BIT64

        }

        // This method ensures that there is sufficient stack to execute the average Framework function.
        // If there is not enough stack, then it throws System.InsufficientExecutionStackException.
        // Note: this method is not part of the CER support, and is not to be confused with ProbeForSufficientStack
        // below.
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void EnsureSufficientExecutionStack();

        // This method ensures that there is sufficient stack to execute the average Framework function.
        // If there is not enough stack, then it return false.
        // Note: this method is not part of the CER support, and is not to be confused with ProbeForSufficientStack
        // below.
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool TryEnsureSufficientExecutionStack();

        /// <returns>true if given type is reference type or value type that contains references</returns>
        [Intrinsic]
        public static bool IsReferenceOrContainsReferences<T>()
        {
            // The body of this function will be replaced by the EE with unsafe code!!!
            // See getILIntrinsicImplementationForRuntimeHelpers for how this happens.
            throw new InvalidOperationException();
        }

        /// <returns>true if given type is bitwise equatable (memcmp can be used for equality checking)</returns>
        /// <remarks>
        /// Only use the result of this for Equals() comparison, not for CompareTo() comparison.
        /// </remarks>
        [Intrinsic]
        internal static bool IsBitwiseEquatable<T>()
        {
            // The body of this function will be replaced by the EE with unsafe code!!!
            // See getILIntrinsicImplementationForRuntimeHelpers for how this happens.
            throw new InvalidOperationException();
        }

        internal static ref byte GetRawData(this object obj) =>
            ref Unsafe.As<RawData>(obj).Data;

        [Intrinsic]
        internal static ref byte GetRawSzArrayData(this Array array) =>
            ref Unsafe.As<RawArrayData>(array).Data;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static unsafe ref byte GetRawArrayData(this Array array) =>
            ref Unsafe.AddByteOffset(ref Unsafe.As<RawData>(array).Data, (nuint)GetObjectMethodTablePointer(array)->BaseSize - (nuint)(2 * sizeof(IntPtr)));

        internal static unsafe ushort GetElementSize(this Array array)
        {
            Debug.Assert(ObjectHasComponentSize(array));
            return GetObjectMethodTablePointer(array)->ComponentSize;
        }

        // Returns true iff the object has a component size;
        // i.e., is variable length like System.String or Array.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static unsafe bool ObjectHasComponentSize(object obj)
        {
            // CLR objects are laid out in memory as follows.
            // [ pMethodTable || .. object data .. ]
            //   ^-- the object reference points here
            //
            // The m_dwFlags field of the method table class will have its high bit set if the
            // method table has component size info stored somewhere. See member
            // MethodTable:IsStringOrArray in src\vm\methodtable.h for full details.
            //
            // So in effect this method is the equivalent of
            // return ((MethodTable*)(*obj))->IsStringOrArray();
            return (int)GetObjectMethodTablePointer(obj)->Flags < 0;
        }

        // Subset of src\vm\methodtable.h
        [StructLayout(LayoutKind.Explicit)]
        private unsafe struct MethodTable
        {
            // globals
            private static MethodTable* g_pEnumClass = null; // TODO: set

            [FieldOffset(0)]
            public ushort ComponentSize;
            [FieldOffset(0)]
            public WFlagsHighEnum Flags;
            [FieldOffset(4)]
            public uint BaseSize;
            [FieldOffset(24)]
            public MethodTable* ParentMethodTable;
            [FieldOffset(48)]
            public MethodTable* CanonMT;
            [FieldOffset(56)]
            public TypeHandle ElementTypeHnd;

            public WFlagsHighEnum GetFlag(WFlagsHighEnum flag)
            {
                return Flags & flag;
            }

            public TypeHandle GetArrayElementTypeHandle()
            {
                return ElementTypeHnd;
            }

            public bool IsEnum()
            {
                return ParentMethodTable == g_pEnumClass;
            }

            public bool IsTruePrimitive()
            {
                return GetFlag(WFlagsHighEnum.CategoryMask) == WFlagsHighEnum.CategoryTruePrimitive;
            }

            public EEClass* GetClass()
            {
                ulong addr = (ulong)CanonMT;
                if ((addr & 2) == 0)
                {
                    // pointer to EEClass
                    return (EEClass*)CanonMT;
                }
                // pointer to canonical MethodTable.
                return (EEClass*)(addr - 2);
            }

            public CorElementType GetVerifierCorElementType()
            {
                switch (GetFlag(WFlagsHighEnum.CategoryElementTypeMask))
                {
                    case WFlagsHighEnum.CategoryArray:
                        return CorElementType.ELEMENT_TYPE_ARRAY;
                    case WFlagsHighEnum.CategoryArray | WFlagsHighEnum.CategoryIfArrayThenSzArray:
                        return CorElementType.ELEMENT_TYPE_SZARRAY;
                    case WFlagsHighEnum.CategoryValueType:
                        return CorElementType.ELEMENT_TYPE_VALUETYPE;
                    case WFlagsHighEnum.CategoryPrimitiveValueType:
                    {
                        if (IsTruePrimitive() || IsEnum())
                        {
                            return GetClass()->NormType;
                        }
                        else
                        {
                            return CorElementType.ELEMENT_TYPE_VALUETYPE;
                        }
                    }
                    default:
                        return CorElementType.ELEMENT_TYPE_CLASS;
                }
            }
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct EEClass // src/vm/class.h
        {
            [FieldOffset(82)]
            public CorElementType NormType; // BYTE m_NormType
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct TypeDesc // src/vm/typedesc.h
        {
            [FieldOffset(0)]
            public ulong TypeAndFlags; // DWORD m_typeAndFlags

            public CorElementType GetInternalCorElementType()
            {
                return (CorElementType)(TypeAndFlags & 0xFF);
            }
        }

        [Flags]
        private enum WFlagsHighEnum
        {
            CategoryMask               = 0x000F0000,
            CategoryTruePrimitive      = 0x00070000,
            CategoryValueType          = 0x00040000,
            CategoryArray              = 0x00080000,
            CategoryIfArrayThenSzArray = 0x00020000,
            CategoryPrimitiveValueType = 0x00060000,
            CategoryElementTypeMask    = 0x000E0000,
        }

        [StructLayout(LayoutKind.Explicit)]
        private unsafe struct TypeHandle
        {
            [FieldOffset(0)]
            public ulong TAddr;
            [FieldOffset(0)]
            public MethodTable* MT;

            public bool IsTypeDesc()
            {
                return (TAddr & 2) != 0;
            }

            public TypeDesc* AsTypeDesc()
            {
                return (TypeDesc*)(TAddr - 2);
            }

            public CorElementType GetVerifierCorElementType()
            {
                if (IsTypeDesc())
                {
                    return AsTypeDesc()->GetInternalCorElementType();
                }
                else
                {
                    return MT->GetVerifierCorElementType();
                }
            }
        }

        private struct CorTypeInfo
        {
            public static bool IsPrimitiveType_NoThrow(CorElementType type)
            {
                // TODO: native code has a CorTypeInfoEntry table and this check is more efficient
                return (type >= CorElementType.ELEMENT_TYPE_BOOLEAN && type <= CorElementType.ELEMENT_TYPE_R8) ||
                       type == CorElementType.ELEMENT_TYPE_I || type == CorElementType.ELEMENT_TYPE_U;
            }
        }

        // Returns a bool to indicate if the array is of primitive types or not.
        internal static unsafe bool IsPrimitiveTypeArray(Array array)
        {
            TypeHandle elementTh = GetObjectMethodTablePointer(array)->GetArrayElementTypeHandle();
            CorElementType elementEt = elementTh.GetVerifierCorElementType();
            return CorTypeInfo.IsPrimitiveType_NoThrow(elementEt);
        }

        // Returns a bool to indicate if the array is of primitive types or not.
        public static unsafe byte MyTest(Array array)
        {
            TypeHandle elementTh = GetObjectMethodTablePointer(array)->GetArrayElementTypeHandle();
            return (byte)elementTh.GetVerifierCorElementType();
        }

        // Given an object reference, returns its MethodTable* as an IntPtr.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe MethodTable* GetObjectMethodTablePointer(object obj)
        {
            Debug.Assert(obj != null);

            // We know that the first data field in any managed object is immediately after the
            // method table pointer, so just back up one pointer and immediately deref.
            // This is not ideal in terms of minimizing instruction count but is the best we can do at the moment.

            return (MethodTable *)Unsafe.Add(ref Unsafe.As<byte, IntPtr>(ref obj.GetRawData()), -1);

            // The JIT currently implements this as:
            // lea tmp, [rax + 8h] ; assume rax contains the object reference, tmp is type IntPtr&
            // mov tmp, qword ptr [tmp - 8h] ; tmp now contains the MethodTable* pointer
            //
            // Ideally this would just be a single dereference:
            // mov tmp, qword ptr [rax] ; rax = obj ref, tmp = MethodTable* pointer
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern object GetUninitializedObjectInternal(Type type);
    }
}
