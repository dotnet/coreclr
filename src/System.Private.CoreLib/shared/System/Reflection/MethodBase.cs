// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Globalization;
using System.Diagnostics;

namespace System.Reflection
{
    public abstract partial class MethodBase : MemberInfo
    {
        protected MethodBase() { }

        public abstract ParameterInfo[] GetParameters();
        public abstract MethodAttributes Attributes { get; }
        public virtual MethodImplAttributes MethodImplementationFlags => GetMethodImplementationFlags();
        public abstract MethodImplAttributes GetMethodImplementationFlags();
        public virtual MethodBody GetMethodBody() { throw new InvalidOperationException(); }
        public virtual CallingConventions CallingConvention => CallingConventions.Standard;

        public bool IsAbstract => (Attributes & MethodAttributes.Abstract) != 0;
        public bool IsConstructor
        {
            get
            {
                // To be backward compatible we only return true for instance RTSpecialName ctors.
                return (this is ConstructorInfo &&
                        !IsStatic &&
                        ((Attributes & MethodAttributes.RTSpecialName) == MethodAttributes.RTSpecialName));
            }
        }
        public bool IsFinal => (Attributes & MethodAttributes.Final) != 0;
        public bool IsHideBySig => (Attributes & MethodAttributes.HideBySig) != 0;
        public bool IsSpecialName => (Attributes & MethodAttributes.SpecialName) != 0;
        public bool IsStatic => (Attributes & MethodAttributes.Static) != 0;
        public bool IsVirtual => (Attributes & MethodAttributes.Virtual) != 0;

        public bool IsAssembly => (Attributes & MethodAttributes.MemberAccessMask) == MethodAttributes.Assembly;
        public bool IsFamily => (Attributes & MethodAttributes.MemberAccessMask) == MethodAttributes.Family;
        public bool IsFamilyAndAssembly => (Attributes & MethodAttributes.MemberAccessMask) == MethodAttributes.FamANDAssem;
        public bool IsFamilyOrAssembly => (Attributes & MethodAttributes.MemberAccessMask) == MethodAttributes.FamORAssem;
        public bool IsPrivate => (Attributes & MethodAttributes.MemberAccessMask) == MethodAttributes.Private;
        public bool IsPublic => (Attributes & MethodAttributes.MemberAccessMask) == MethodAttributes.Public;

        public virtual bool IsConstructedGenericMethod => IsGenericMethod && !IsGenericMethodDefinition;
        public virtual bool IsGenericMethod => false;
        public virtual bool IsGenericMethodDefinition => false;
        public virtual Type[] GetGenericArguments() { throw new NotSupportedException(SR.NotSupported_SubclassOverride); }
        public virtual bool ContainsGenericParameters => false;

        [DebuggerHidden]
        [DebuggerStepThrough]
        public object Invoke(object obj, object[] parameters) => Invoke(obj, BindingFlags.Default, binder: null, parameters: parameters, culture: null);
        public abstract object Invoke(object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture);

        public abstract RuntimeMethodHandle MethodHandle { get; }

        public virtual bool IsSecurityCritical { get { throw NotImplemented.ByDesign; } }
        public virtual bool IsSecuritySafeCritical { get { throw NotImplemented.ByDesign; } }
        public virtual bool IsSecurityTransparent { get { throw NotImplemented.ByDesign; } }

        public override bool Equals(object obj) => base.Equals(obj);
        public override int GetHashCode() => base.GetHashCode();

        public static bool operator ==(MethodBase left, MethodBase right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (left is null || right is null)
            {
                return false;
            }

            if (left is MethodInfo method1)
            {
                // MethodInfo operator== calls Equals after same reference and null checks above,
                // so just call Equals directly.
                return (right is MethodInfo method2) ? method1.Equals(method2) : false;
            }

            if (left is ConstructorInfo constructor1)
            {
                // ConstructorInfo operator== calls Equals after same reference and null checks above,
                // so just call Equals directly.
                return (right is ConstructorInfo constructor2) ? constructor1.Equals(constructor2) : false;
            }

            return false;
        }

        public static bool operator !=(MethodBase left, MethodBase right) => !(left == right);
    }
}
