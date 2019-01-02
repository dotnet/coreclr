// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

using Internal.Runtime.CompilerServices;

namespace System
{
    [ClassInterface(ClassInterfaceType.None)]
    [ComVisible(true)]
    public abstract class MulticastDelegate : Delegate
    {
        // This is set under 3 circumstances
        // 1. Multicast delegate
        // 2. Secure/Wrapper delegate
        // 3. Inner delegate of secure delegate where the secure delegate security context is a collectible method
        private object _invocationList;
        private IntPtr _invocationCount;

        // This constructor is called from the class generated by the
        //    compiler generated code (This must match the constructor
        //    in Delegate
        protected MulticastDelegate(object target, string method) : base(target, method)
        {
        }

        // This constructor is called from a class to generate a 
        // delegate based upon a static method name and the Type object
        // for the class defining the method.
        protected MulticastDelegate(Type target, string method) : base(target, method)
        {
        }

        internal bool IsUnmanagedFunctionPtr()
        {
            return (_invocationCount == (IntPtr)(-1));
        }

        internal bool InvocationListLogicallyNull()
        {
            return (_invocationList == null) || (_invocationList is LoaderAllocator) || (_invocationList is System.Reflection.Emit.DynamicResolver);
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            throw new SerializationException(SR.Serialization_DelegatesNotSupported);
        }

        // equals returns true IIF the delegate is not null and has the
        //    same target, method and invocation list as this object
        public override sealed bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (object.ReferenceEquals(this, obj))
                return true;
            if (!InternalEqualTypes(this, obj))
                return false;

            // Since this is a MulticastDelegate and we know
            // the types are the same, obj should also be a
            // MulticastDelegate
            Debug.Assert(obj is MulticastDelegate, "Shouldn't have failed here since we already checked the types are the same!");
            var d = Unsafe.As<MulticastDelegate>(obj);

            if (_invocationCount != (IntPtr)0)
            {
                // there are 4 kind of delegate kinds that fall into this bucket
                // 1- Multicast (_invocationList is Object[])
                // 2- Secure/Wrapper (_invocationList is Delegate)
                // 3- Unmanaged FntPtr (_invocationList == null)
                // 4- Open virtual (_invocationCount == MethodDesc of target, _invocationList == null, LoaderAllocator, or DynamicResolver)

                if (InvocationListLogicallyNull())
                {
                    if (IsUnmanagedFunctionPtr())
                    {
                        if (!d.IsUnmanagedFunctionPtr())
                            return false;

                        return CompareUnmanagedFunctionPtrs(this, d);
                    }

                    // now we know 'this' is not a special one, so we can work out what the other is
                    if ((d._invocationList as Delegate) != null)
                        // this is a secure/wrapper delegate so we need to unwrap and check the inner one
                        return Equals(d._invocationList);

                    return base.Equals(obj);
                }
                else
                {
                    if ((_invocationList as Delegate) != null)
                    {
                        // this is a secure/wrapper delegate so we need to unwrap and check the inner one
                        return _invocationList.Equals(obj);
                    }
                    else
                    {
                        Debug.Assert((_invocationList as object[]) != null, "empty invocation list on multicast delegate");
                        return InvocationListEquals(d);
                    }
                }
            }
            else
            {
                // among the several kind of delegates falling into this bucket one has got a non
                // empty _invocationList (open static with special sig)
                // to be equals we need to check that _invocationList matches (both null is fine)
                // and call the base.Equals()
                if (!InvocationListLogicallyNull())
                {
                    if (!_invocationList.Equals(d._invocationList))
                        return false;
                    return base.Equals(d);
                }

                // now we know 'this' is not a special one, so we can work out what the other is
                if ((d._invocationList as Delegate) != null)
                    // this is a secure/wrapper delegate so we need to unwrap and check the inner one
                    return Equals(d._invocationList);

                // now we can call on the base
                return base.Equals(d);
            }
        }

        // Recursive function which will check for equality of the invocation list.
        private bool InvocationListEquals(MulticastDelegate d)
        {
            Debug.Assert(d != null && (_invocationList as object[]) != null, "bogus delegate in multicast list comparison");
            object[] invocationList = _invocationList as object[];
            if (d._invocationCount != _invocationCount)
                return false;

            int invocationCount = (int)_invocationCount;
            for (int i = 0; i < invocationCount; i++)
            {
                Delegate dd = (Delegate)invocationList[i];
                object[] dInvocationList = d._invocationList as object[];
                if (!dd.Equals(dInvocationList[i]))
                    return false;
            }
            return true;
        }

        private bool TrySetSlot(object[] a, int index, object o)
        {
            if (a[index] == null && System.Threading.Interlocked.CompareExchange<object>(ref a[index], o, null) == null)
                return true;

            // The slot may be already set because we have added and removed the same method before.
            // Optimize this case, because it's cheaper than copying the array.
            if (a[index] != null)
            {
                MulticastDelegate d = (MulticastDelegate)o;
                MulticastDelegate dd = (MulticastDelegate)a[index];

                if (dd._methodPtr == d._methodPtr &&
                    dd._target == d._target &&
                    dd._methodPtrAux == d._methodPtrAux)
                {
                    return true;
                }
            }
            return false;
        }

        private MulticastDelegate NewMulticastDelegate(object[] invocationList, int invocationCount, bool thisIsMultiCastAlready)
        {
            // First, allocate a new multicast delegate just like this one, i.e. same type as the this object
            MulticastDelegate result = (MulticastDelegate)InternalAllocLike(this);

            // Performance optimization - if this already points to a true multicast delegate,
            // copy _methodPtr and _methodPtrAux fields rather than calling into the EE to get them
            if (thisIsMultiCastAlready)
            {
                result._methodPtr = this._methodPtr;
                result._methodPtrAux = this._methodPtrAux;
            }
            else
            {
                result._methodPtr = GetMulticastInvoke();
                result._methodPtrAux = GetInvokeMethod();
            }
            result._target = result;
            result._invocationList = invocationList;
            result._invocationCount = (IntPtr)invocationCount;

            return result;
        }

        internal MulticastDelegate NewMulticastDelegate(object[] invocationList, int invocationCount)
        {
            return NewMulticastDelegate(invocationList, invocationCount, false);
        }

        internal void StoreDynamicMethod(MethodInfo dynamicMethod)
        {
            if (_invocationCount != (IntPtr)0)
            {
                Debug.Assert(!IsUnmanagedFunctionPtr(), "dynamic method and unmanaged fntptr delegate combined");
                // must be a secure/wrapper one, unwrap and save
                MulticastDelegate d = (MulticastDelegate)_invocationList;
                d._methodBase = dynamicMethod;
            }
            else
                _methodBase = dynamicMethod;
        }

        // This method will combine this delegate with the passed delegate
        //    to form a new delegate.
        protected override sealed Delegate CombineImpl(Delegate follow)
        {
            if ((object)follow == null) // cast to object for a more efficient test
                return this;

            // Verify that the types are the same...
            if (!InternalEqualTypes(this, follow))
                throw new ArgumentException(SR.Arg_DlgtTypeMis);

            MulticastDelegate dFollow = (MulticastDelegate)follow;
            object[] resultList;
            int followCount = 1;
            object[] followList = dFollow._invocationList as object[];
            if (followList != null)
                followCount = (int)dFollow._invocationCount;

            int resultCount;
            object[] invocationList = _invocationList as object[];
            if (invocationList == null)
            {
                resultCount = 1 + followCount;
                resultList = new object[resultCount];
                resultList[0] = this;
                if (followList == null)
                {
                    resultList[1] = dFollow;
                }
                else
                {
                    for (int i = 0; i < followCount; i++)
                        resultList[1 + i] = followList[i];
                }
                return NewMulticastDelegate(resultList, resultCount);
            }
            else
            {
                int invocationCount = (int)_invocationCount;
                resultCount = invocationCount + followCount;
                resultList = null;
                if (resultCount <= invocationList.Length)
                {
                    resultList = invocationList;
                    if (followList == null)
                    {
                        if (!TrySetSlot(resultList, invocationCount, dFollow))
                            resultList = null;
                    }
                    else
                    {
                        for (int i = 0; i < followCount; i++)
                        {
                            if (!TrySetSlot(resultList, invocationCount + i, followList[i]))
                            {
                                resultList = null;
                                break;
                            }
                        }
                    }
                }

                if (resultList == null)
                {
                    int allocCount = invocationList.Length;
                    while (allocCount < resultCount)
                        allocCount *= 2;

                    resultList = new object[allocCount];

                    for (int i = 0; i < invocationCount; i++)
                        resultList[i] = invocationList[i];

                    if (followList == null)
                    {
                        resultList[invocationCount] = dFollow;
                    }
                    else
                    {
                        for (int i = 0; i < followCount; i++)
                            resultList[invocationCount + i] = followList[i];
                    }
                }
                return NewMulticastDelegate(resultList, resultCount, true);
            }
        }

        private object[] DeleteFromInvocationList(object[] invocationList, int invocationCount, int deleteIndex, int deleteCount)
        {
            object[] thisInvocationList = _invocationList as object[];
            int allocCount = thisInvocationList.Length;
            while (allocCount / 2 >= invocationCount - deleteCount)
                allocCount /= 2;

            object[] newInvocationList = new object[allocCount];

            for (int i = 0; i < deleteIndex; i++)
                newInvocationList[i] = invocationList[i];

            for (int i = deleteIndex + deleteCount; i < invocationCount; i++)
                newInvocationList[i - deleteCount] = invocationList[i];

            return newInvocationList;
        }

        private bool EqualInvocationLists(object[] a, object[] b, int start, int count)
        {
            for (int i = 0; i < count; i++)
            {
                if (!(a[start + i].Equals(b[i])))
                    return false;
            }
            return true;
        }

        // This method currently looks backward on the invocation list
        //    for an element that has Delegate based equality with value.  (Doesn't
        //    look at the invocation list.)  If this is found we remove it from
        //    this list and return a new delegate.  If its not found a copy of the
        //    current list is returned.
        protected override sealed Delegate RemoveImpl(Delegate value)
        {
            // There is a special case were we are removing using a delegate as
            //    the value we need to check for this case
            //
            MulticastDelegate v = value as MulticastDelegate;

            if (v == null)
                return this;
            if (v._invocationList as object[] == null)
            {
                object[] invocationList = _invocationList as object[];
                if (invocationList == null)
                {
                    // they are both not real Multicast
                    if (this.Equals(value))
                        return null;
                }
                else
                {
                    int invocationCount = (int)_invocationCount;
                    for (int i = invocationCount; --i >= 0;)
                    {
                        if (value.Equals(invocationList[i]))
                        {
                            if (invocationCount == 2)
                            {
                                // Special case - only one value left, either at the beginning or the end
                                return (Delegate)invocationList[1 - i];
                            }
                            else
                            {
                                object[] list = DeleteFromInvocationList(invocationList, invocationCount, i, 1);
                                return NewMulticastDelegate(list, invocationCount - 1, true);
                            }
                        }
                    }
                }
            }
            else
            {
                object[] invocationList = _invocationList as object[];
                if (invocationList != null)
                {
                    int invocationCount = (int)_invocationCount;
                    int vInvocationCount = (int)v._invocationCount;
                    for (int i = invocationCount - vInvocationCount; i >= 0; i--)
                    {
                        if (EqualInvocationLists(invocationList, v._invocationList as object[], i, vInvocationCount))
                        {
                            if (invocationCount - vInvocationCount == 0)
                            {
                                // Special case - no values left
                                return null;
                            }
                            else if (invocationCount - vInvocationCount == 1)
                            {
                                // Special case - only one value left, either at the beginning or the end
                                return (Delegate)invocationList[i != 0 ? 0 : invocationCount - 1];
                            }
                            else
                            {
                                object[] list = DeleteFromInvocationList(invocationList, invocationCount, i, vInvocationCount);
                                return NewMulticastDelegate(list, invocationCount - vInvocationCount, true);
                            }
                        }
                    }
                }
            }

            return this;
        }

        // This method returns the Invocation list of this multicast delegate.
        public override sealed Delegate[] GetInvocationList()
        {
            Delegate[] del;
            object[] invocationList = _invocationList as object[];
            if (invocationList == null)
            {
                del = new Delegate[1];
                del[0] = this;
            }
            else
            {
                // Create an array of delegate copies and each
                //    element into the array
                del = new Delegate[(int)_invocationCount];

                for (int i = 0; i < del.Length; i++)
                    del[i] = (Delegate)invocationList[i];
            }
            return del;
        }

        // Force inline as the true/false ternary takes it above ALWAYS_INLINE size even though the asm ends up smaller
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(MulticastDelegate d1, MulticastDelegate d2)
        {
            // Test d2 first to allow branch elimination when inlined for null checks (== null)
            // so it can become a simple test
            if (d2 is null)
            {
                // return true/false not the test result https://github.com/dotnet/coreclr/issues/914
                return (d1 is null) ? true : false;
            }

            return ReferenceEquals(d2, d1) ? true : d2.Equals(d1);
        }

        // Force inline as the true/false ternary takes it above ALWAYS_INLINE size even though the asm ends up smaller
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(MulticastDelegate d1, MulticastDelegate d2)
        {
            // Can't call the == operator as it will call object==

            // Test d2 first to allow branch elimination when inlined for not null checks (!= null)
            // so it can become a simple test
            if (d2 is null)
            {
                // return true/false not the test result https://github.com/dotnet/coreclr/issues/914
                return (d1 is null) ? false : true;
            }

            return ReferenceEquals(d2, d1) ? false : !d2.Equals(d1);
        }

        public override sealed int GetHashCode()
        {
            if (IsUnmanagedFunctionPtr())
                return ValueType.GetHashCodeOfPtr(_methodPtr) ^ ValueType.GetHashCodeOfPtr(_methodPtrAux);

            if (_invocationCount != (IntPtr)0)
            {
                var t = _invocationList as Delegate;

                if (t != null)
                {
                    // this is a secure/wrapper delegate so we need to unwrap and check the inner one
                    return t.GetHashCode();
                }
            }

            object[] invocationList = _invocationList as object[];
            if (invocationList == null)
            {
                return base.GetHashCode();
            }
            else
            {
                int hash = 0;
                for (int i = 0; i < (int)_invocationCount; i++)
                {
                    hash = hash * 33 + invocationList[i].GetHashCode();
                }

                return hash;
            }
        }

        internal override object GetTarget()
        {
            if (_invocationCount != (IntPtr)0)
            {
                // _invocationCount != 0 we are in one of these cases:
                // - Multicast -> return the target of the last delegate in the list
                // - Secure/wrapper delegate -> return the target of the inner delegate
                // - unmanaged function pointer - return null
                // - virtual open delegate - return null
                if (InvocationListLogicallyNull())
                {
                    // both open virtual and ftn pointer return null for the target
                    return null;
                }
                else
                {
                    object[] invocationList = _invocationList as object[];
                    if (invocationList != null)
                    {
                        int invocationCount = (int)_invocationCount;
                        return ((Delegate)invocationList[invocationCount - 1]).GetTarget();
                    }
                    else
                    {
                        Delegate receiver = _invocationList as Delegate;
                        if (receiver != null)
                            return receiver.GetTarget();
                    }
                }
            }
            return base.GetTarget();
        }

        protected override MethodInfo GetMethodImpl()
        {
            if (_invocationCount != (IntPtr)0 && _invocationList != null)
            {
                // multicast case
                object[] invocationList = _invocationList as object[];
                if (invocationList != null)
                {
                    int index = (int)_invocationCount - 1;
                    return ((Delegate)invocationList[index]).Method;
                }
                MulticastDelegate innerDelegate = _invocationList as MulticastDelegate;
                if (innerDelegate != null)
                {
                    // must be a secure/wrapper delegate
                    return innerDelegate.GetMethodImpl();
                }
            }
            else if (IsUnmanagedFunctionPtr())
            {
                // we handle unmanaged function pointers here because the generic ones (used for WinRT) would otherwise
                // be treated as open delegates by the base implementation, resulting in failure to get the MethodInfo
                if ((_methodBase == null) || !(_methodBase is MethodInfo))
                {
                    IRuntimeMethodInfo method = FindMethodHandle();
                    RuntimeType declaringType = RuntimeMethodHandle.GetDeclaringType(method);

                    // need a proper declaring type instance method on a generic type
                    if (RuntimeTypeHandle.IsGenericTypeDefinition(declaringType) || RuntimeTypeHandle.HasInstantiation(declaringType))
                    {
                        // we are returning the 'Invoke' method of this delegate so use this.GetType() for the exact type
                        RuntimeType reflectedType = GetType() as RuntimeType;
                        declaringType = reflectedType;
                    }
                    _methodBase = (MethodInfo)RuntimeType.GetMethodBase(declaringType, method);
                }
                return (MethodInfo)_methodBase;
            }

            // Otherwise, must be an inner delegate of a SecureDelegate of an open virtual method. In that case, call base implementation
            return base.GetMethodImpl();
        }

        // this should help inlining
        [System.Diagnostics.DebuggerNonUserCode]
        private void ThrowNullThisInDelegateToInstance()
        {
            throw new ArgumentException(SR.Arg_DlgtNullInst);
        }

        [System.Diagnostics.DebuggerNonUserCode]
        private void CtorClosed(object target, IntPtr methodPtr)
        {
            if (target == null)
                ThrowNullThisInDelegateToInstance();
            this._target = target;
            this._methodPtr = methodPtr;
        }

        [System.Diagnostics.DebuggerNonUserCode]
        private void CtorClosedStatic(object target, IntPtr methodPtr)
        {
            this._target = target;
            this._methodPtr = methodPtr;
        }

        [System.Diagnostics.DebuggerNonUserCode]
        private void CtorRTClosed(object target, IntPtr methodPtr)
        {
            this._target = target;
            this._methodPtr = AdjustTarget(target, methodPtr);
        }

        [System.Diagnostics.DebuggerNonUserCode]
        private void CtorOpened(object target, IntPtr methodPtr, IntPtr shuffleThunk)
        {
            this._target = this;
            this._methodPtr = shuffleThunk;
            this._methodPtrAux = methodPtr;
        }

        [System.Diagnostics.DebuggerNonUserCode]
        private void CtorVirtualDispatch(object target, IntPtr methodPtr, IntPtr shuffleThunk)
        {
            this._target = this;
            this._methodPtr = shuffleThunk;
            this._methodPtrAux = GetCallStub(methodPtr);
        }

        [System.Diagnostics.DebuggerNonUserCode]
        private void CtorCollectibleClosedStatic(object target, IntPtr methodPtr, IntPtr gchandle)
        {
            this._target = target;
            this._methodPtr = methodPtr;
            this._methodBase = System.Runtime.InteropServices.GCHandle.InternalGet(gchandle);
        }

        [System.Diagnostics.DebuggerNonUserCode]
        private void CtorCollectibleOpened(object target, IntPtr methodPtr, IntPtr shuffleThunk, IntPtr gchandle)
        {
            this._target = this;
            this._methodPtr = shuffleThunk;
            this._methodPtrAux = methodPtr;
            this._methodBase = System.Runtime.InteropServices.GCHandle.InternalGet(gchandle);
        }

        [System.Diagnostics.DebuggerNonUserCode]
        private void CtorCollectibleVirtualDispatch(object target, IntPtr methodPtr, IntPtr shuffleThunk, IntPtr gchandle)
        {
            this._target = this;
            this._methodPtr = shuffleThunk;
            this._methodPtrAux = GetCallStub(methodPtr);
            this._methodBase = System.Runtime.InteropServices.GCHandle.InternalGet(gchandle);
        }
    }
}
