// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System {
    
    using System;
    using System.Runtime.InteropServices;
    using System.Runtime.CompilerServices;
    using System.Runtime.Versioning;
    using System.Diagnostics.Contracts;

    // This class will not be marked serializable
    // Note: This type must have the same layout as the CLR's VARARGS type in CLRVarArgs.h.
    // It also contains an inline SigPointer data structure - must keep those fields in sync.
    [StructLayout(LayoutKind.Sequential)]
    public struct ArgIterator
    {
#if VARARGS_ENABLED //The JIT doesn't support Varargs calling convention.
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private extern ArgIterator(IntPtr arglist);

        // create an arg iterator that points at the first argument that
        // is not statically declared (that is the first ... arg)
        // 'arglist' is the value returned by the ARGLIST instruction
        public ArgIterator(RuntimeArgumentHandle arglist) : this(arglist.Value)
        {
        }

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private unsafe extern ArgIterator(IntPtr arglist, void *ptr);
        
        // create an arg iterator that points just past 'firstArg'.  
        // 'arglist' is the value returned by the ARGLIST instruction
        // This is much like the C va_start macro

        [CLSCompliant(false)]
        public unsafe ArgIterator(RuntimeArgumentHandle arglist, void* ptr) : this(arglist.Value, ptr)
        {
        }

        // Fetch an argument as a typed referece, advance the iterator.
        // Throws an exception if past end of argument list
        [CLSCompliant(false)]
        public TypedReference GetNextArg()
        {
            TypedReference result = new TypedReference ();
            // reference to TypedReference is banned, so have to pass result as pointer
            unsafe
            {
                FCallGetNextArg (&result);
            }
            return result;
        }

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        // reference to TypedReference is banned, so have to pass result as void pointer
        private unsafe extern void FCallGetNextArg(void * result);

        // Alternate version of GetNextArg() intended primarily for IJW code
        // generated by VC's "va_arg()" construct. 
        [CLSCompliant(false)]
        public TypedReference GetNextArg(RuntimeTypeHandle rth)
        {
            if (sigPtr != IntPtr.Zero)
            {
                // This is an ordinary ArgIterator capable of determining
                // types from a signature. Just do a regular GetNextArg.
                return GetNextArg();
            }
            else
            {
                // Prevent abuse of this API with a default ArgIterator (it
                // doesn't require permission to create a zero-inited value
                // type). Check that ArgPtr isn't zero or this API will allow a
                // malicious caller to increment the pointer to an arbitrary
                // location in memory and read the contents.
                if (ArgPtr == IntPtr.Zero)
                    throw new ArgumentNullException();

                TypedReference result = new TypedReference ();
                // reference to TypedReference is banned, so have to pass result as pointer
                unsafe
                {
                    InternalGetNextArg(&result, rth.GetRuntimeType());
                }
                return result;
            }
        }


        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        // reference to TypedReference is banned, so have to pass result as void pointer
        private unsafe extern void InternalGetNextArg(void * result, RuntimeType rt);

        // This method should invalidate the iterator (va_end). It is not supported yet.
        public void End()
        {
        }
    
        // How many arguments are left in the list 
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern int GetRemainingCount();
    
        // Gets the type of the current arg, does NOT advance the iterator
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private extern unsafe void* _GetNextArgType();

        public unsafe RuntimeTypeHandle GetNextArgType() 
        {
            return new RuntimeTypeHandle(Type.GetTypeFromHandleUnsafe((IntPtr)_GetNextArgType()));
        }
    
        public override int GetHashCode()
        {
            return ValueType.GetHashCodeOfPtr(ArgCookie);
        }
    
        // Inherited from object
        public override bool Equals(Object o)
        {
            throw new NotSupportedException(Environment.GetResourceString("NotSupported_NYI"));
        }

        private IntPtr ArgCookie;               // Cookie from the EE.

        // The SigPointer structure consists of the following members.  (Note: this is an inline native SigPointer data type)
        private IntPtr sigPtr;                  // Pointer to remaining signature.
        private IntPtr sigPtrLen;               // Remaining length of the pointer

        // Note, sigPtrLen is actually a DWORD, but on 64bit systems this structure becomes
        // 8-byte aligned, which requires us to pad it.
            
        private IntPtr ArgPtr;                  // Pointer to remaining args.
        private int    RemainingArgs;           // # of remaining args.
#else
        public ArgIterator(RuntimeArgumentHandle arglist)
        {
            throw new PlatformNotSupportedException(); //The JIT requires work to enable ArgIterator see: https://github.com/dotnet/standard/issues/20#issuecomment-272775599.
        }

        [CLSCompliant(false)]
        public unsafe ArgIterator(RuntimeArgumentHandle arglist, void* ptr)
        {
            throw new PlatformNotSupportedException(); //The JIT requires work to enable ArgIterator https://github.com/dotnet/standard/issues/20#issuecomment-272775599.
        }

        public void End() 
        { 
            throw new PlatformNotSupportedException(); //The JIT requires work to enable ArgIterator https://github.com/dotnet/standard/issues/20#issuecomment-272775599.
        }

        public override bool Equals(Object o) 
        {  
            throw new PlatformNotSupportedException(); //The JIT requires work to enable ArgIterator https://github.com/dotnet/standard/issues/20#issuecomment-272775599.
        }

        public override int GetHashCode()
        { 
            throw new PlatformNotSupportedException(); //The JIT requires work to enable ArgIterator https://github.com/dotnet/standard/issues/20#issuecomment-272775599.
        }

        [System.CLSCompliantAttribute(false)]
        public System.TypedReference GetNextArg()
        {
            throw new PlatformNotSupportedException(); //The JIT requires work to enable ArgIterator https://github.com/dotnet/standard/issues/20#issuecomment-272775599.
        }

        [System.CLSCompliantAttribute(false)]
        public System.TypedReference GetNextArg(System.RuntimeTypeHandle rth)
        {
            throw new PlatformNotSupportedException(); //The JIT requires work to enable ArgIterator https://github.com/dotnet/standard/issues/20#issuecomment-272775599.
        }

        public unsafe System.RuntimeTypeHandle GetNextArgType()
        {
            throw new PlatformNotSupportedException(); //The JIT requires work to enable ArgIterator https://github.com/dotnet/standard/issues/20#issuecomment-272775599.
        }

        public int GetRemainingCount()
        {  
            throw new PlatformNotSupportedException(); //The JIT requires work to enable ArgIterator https://github.com/dotnet/standard/issues/20#issuecomment-272775599.
        }
#endif //VARARGS_ENABLED
    }
}
