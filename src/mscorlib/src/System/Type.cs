// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

//
//
//
// Implements System.Type
//
// ======================================================================================

namespace System
{
    using System;
    using System.Reflection;
    using System.Threading;
    using System.Runtime;
    using System.Runtime.Remoting;
    using System.Runtime.InteropServices;
    using System.Runtime.CompilerServices;
    using System.Security;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.Versioning;
    using CultureInfo = System.Globalization.CultureInfo;
    using StackCrawlMark = System.Threading.StackCrawlMark;
    using DebuggerStepThroughAttribute = System.Diagnostics.DebuggerStepThroughAttribute;

    [Serializable]
    public abstract partial class Type : MemberInfo, IReflect
    {
        public static readonly MemberFilter FilterAttribute;
        public static readonly MemberFilter FilterName;
        public static readonly MemberFilter FilterNameIgnoreCase;

        public static readonly object Missing = System.Reflection.Missing.Value;

        public static readonly char Delimiter = '.';

        public readonly static Type[] EmptyTypes = EmptyArray<Type>.Value;

        protected Type() { }

        public override MemberTypes MemberType
        {
            get { return System.Reflection.MemberTypes.TypeInfo; }
        }

        public override Type DeclaringType
        {
            get { return null; }
        }

        public virtual MethodBase DeclaringMethod { get { return null; } }

        public override Type ReflectedType
        {
            get { return null; }
        }

        [System.Security.DynamicSecurityMethod] // Methods containing StackCrawlMark local var has to be marked DynamicSecurityMethod
        public static Type ReflectionOnlyGetType(string typeName, bool throwIfNotFound, bool ignoreCase)
        {
            if (typeName == null)
                throw new ArgumentNullException(nameof(typeName));
            if (typeName.Length == 0 && throwIfNotFound)
                throw new TypeLoadException(Environment.GetResourceString("Arg_TypeLoadNullStr"));
            throw new NotSupportedException(Environment.GetResourceString("NotSupported_ReflectionOnlyGetType"));
        }

        public virtual Type MakePointerType() { throw new NotSupportedException(); }
        public virtual StructLayoutAttribute StructLayoutAttribute { get { throw new NotSupportedException(); } }
        public virtual Type MakeByRefType() { throw new NotSupportedException(); }
        public virtual Type MakeArrayType() { throw new NotSupportedException(); }
        public virtual Type MakeArrayType(int rank) { throw new NotSupportedException(); }

        public static Type GetTypeFromProgID(string progID)
        {
            return RuntimeType.GetTypeFromProgIDImpl(progID, null, false);
        }

        public static Type GetTypeFromProgID(string progID, bool throwOnError)
        {
            return RuntimeType.GetTypeFromProgIDImpl(progID, null, throwOnError);
        }

        public static Type GetTypeFromProgID(string progID, string server)
        {
            return RuntimeType.GetTypeFromProgIDImpl(progID, server, false);
        }

        public static Type GetTypeFromCLSID(Guid clsid)
        {
            return RuntimeType.GetTypeFromCLSIDImpl(clsid, null, false);
        }

        public static Type GetTypeFromCLSID(Guid clsid, bool throwOnError)
        {
            return RuntimeType.GetTypeFromCLSIDImpl(clsid, null, throwOnError);
        }

        public static Type GetTypeFromCLSID(Guid clsid, string server)
        {
            return RuntimeType.GetTypeFromCLSIDImpl(clsid, server, false);
        }

        public static TypeCode GetTypeCode(Type type)
        {
            if (type == null)
                return TypeCode.Empty;
            return type.GetTypeCodeImpl();
        }

        protected virtual TypeCode GetTypeCodeImpl()
        {
            if (this != UnderlyingSystemType && UnderlyingSystemType != null)
                return Type.GetTypeCode(UnderlyingSystemType);

            return TypeCode.Object;
        }

        public abstract Guid GUID
        {
            get;
        }

        public abstract object InvokeMember(string name, BindingFlags invokeAttr, Binder binder, object target,
                                    object[] args, ParameterModifier[] modifiers, CultureInfo culture, string[] namedParameters);

        [DebuggerStepThroughAttribute]
        [Diagnostics.DebuggerHidden]
        public object InvokeMember(string name, BindingFlags invokeAttr, Binder binder, object target, object[] args, CultureInfo culture)
        {
            return InvokeMember(name, invokeAttr, binder, target, args, null, culture, null);
        }

        [DebuggerStepThroughAttribute]
        [Diagnostics.DebuggerHidden]
        public object InvokeMember(string name, BindingFlags invokeAttr, Binder binder, object target, object[] args)
        {
            return InvokeMember(name, invokeAttr, binder, target, args, null, null, null);
        }


        public new abstract Module Module { get; }

        public abstract Assembly Assembly
        {
            get;
        }

        public virtual RuntimeTypeHandle TypeHandle
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        public static RuntimeTypeHandle GetTypeHandle(object o)
        {
            if (o == null)
                throw new ArgumentNullException(null, SR.Arg_InvalidHandle);
            return new RuntimeTypeHandle((RuntimeType)o.GetType());
        }

        public abstract string FullName
        {
            get;
        }

        public abstract string Namespace
        {
            get;
        }


        public abstract string AssemblyQualifiedName
        {
            get;
        }


        public virtual int GetArrayRank()
        {
            throw new NotSupportedException(SR.NotSupported_SubclassOverride);
        }

        public abstract Type BaseType
        {
            get;
        }


        public ConstructorInfo GetConstructor(BindingFlags bindingAttr,
                                              Binder binder,
                                              CallingConventions callConvention,
                                              Type[] types,
                                              ParameterModifier[] modifiers)
        {
            if (types == null)
                throw new ArgumentNullException(nameof(types));
            for (int i = 0; i < types.Length; i++)
                if (types[i] == null)
                    throw new ArgumentNullException(nameof(types));
            return GetConstructorImpl(bindingAttr, binder, callConvention, types, modifiers);
        }

        public ConstructorInfo GetConstructor(BindingFlags bindingAttr, Binder binder, Type[] types, ParameterModifier[] modifiers)
        {
            if (types == null)
                throw new ArgumentNullException(nameof(types));
            for (int i = 0; i < types.Length; i++)
                if (types[i] == null)
                    throw new ArgumentNullException(nameof(types));
            return GetConstructorImpl(bindingAttr, binder, CallingConventions.Any, types, modifiers);
        }

        public ConstructorInfo GetConstructor(Type[] types)
        {
            return GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, types, null);
        }

        protected abstract ConstructorInfo GetConstructorImpl(BindingFlags bindingAttr,
                                                              Binder binder,
                                                              CallingConventions callConvention,
                                                              Type[] types,
                                                              ParameterModifier[] modifiers);

        public ConstructorInfo[] GetConstructors()
        {
            return GetConstructors(BindingFlags.Public | BindingFlags.Instance);
        }

        public abstract ConstructorInfo[] GetConstructors(BindingFlags bindingAttr);

        public ConstructorInfo TypeInitializer
        {
            get
            {
                return GetConstructorImpl(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic,
                                          null,
                                          CallingConventions.Any,
                                          Type.EmptyTypes,
                                          null);
            }
        }


        public MethodInfo GetMethod(string name,
                                    BindingFlags bindingAttr,
                                    Binder binder,
                                    CallingConventions callConvention,
                                    Type[] types,
                                    ParameterModifier[] modifiers)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            if (types == null)
                throw new ArgumentNullException(nameof(types));
            for (int i = 0; i < types.Length; i++)
                if (types[i] == null)
                    throw new ArgumentNullException(nameof(types));
            return GetMethodImpl(name, bindingAttr, binder, callConvention, types, modifiers);
        }

        public MethodInfo GetMethod(string name,
                                    BindingFlags bindingAttr,
                                    Binder binder,
                                    Type[] types,
                                    ParameterModifier[] modifiers)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            if (types == null)
                throw new ArgumentNullException(nameof(types));
            for (int i = 0; i < types.Length; i++)
                if (types[i] == null)
                    throw new ArgumentNullException(nameof(types));
            return GetMethodImpl(name, bindingAttr, binder, CallingConventions.Any, types, modifiers);
        }

        public MethodInfo GetMethod(string name, Type[] types, ParameterModifier[] modifiers)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            if (types == null)
                throw new ArgumentNullException(nameof(types));
            for (int i = 0; i < types.Length; i++)
                if (types[i] == null)
                    throw new ArgumentNullException(nameof(types));
            return GetMethodImpl(name, Type.DefaultLookup, null, CallingConventions.Any, types, modifiers);
        }

        public MethodInfo GetMethod(string name, Type[] types)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            if (types == null)
                throw new ArgumentNullException(nameof(types));
            for (int i = 0; i < types.Length; i++)
                if (types[i] == null)
                    throw new ArgumentNullException(nameof(types));
            return GetMethodImpl(name, Type.DefaultLookup, null, CallingConventions.Any, types, null);
        }

        public MethodInfo GetMethod(string name, BindingFlags bindingAttr)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            return GetMethodImpl(name, bindingAttr, null, CallingConventions.Any, null, null);
        }

        public MethodInfo GetMethod(string name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            return GetMethodImpl(name, Type.DefaultLookup, null, CallingConventions.Any, null, null);
        }

        protected abstract MethodInfo GetMethodImpl(string name,
                                                    BindingFlags bindingAttr,
                                                    Binder binder,
                                                    CallingConventions callConvention,
                                                    Type[] types,
                                                    ParameterModifier[] modifiers);


        public MethodInfo[] GetMethods()
        {
            return GetMethods(Type.DefaultLookup);
        }

        public abstract MethodInfo[] GetMethods(BindingFlags bindingAttr);

        public abstract FieldInfo GetField(string name, BindingFlags bindingAttr);


        public FieldInfo GetField(string name)
        {
            return GetField(name, Type.DefaultLookup);
        }


        public FieldInfo[] GetFields()
        {
            return GetFields(Type.DefaultLookup);
        }
        public abstract FieldInfo[] GetFields(BindingFlags bindingAttr);

        public Type GetInterface(string name)
        {
            return GetInterface(name, false);
        }
        public abstract Type GetInterface(string name, bool ignoreCase);


        public abstract Type[] GetInterfaces();

        public EventInfo GetEvent(string name)
        {
            return GetEvent(name, Type.DefaultLookup);
        }
        public abstract EventInfo GetEvent(string name, BindingFlags bindingAttr);

        virtual public EventInfo[] GetEvents()
        {
            return GetEvents(Type.DefaultLookup);
        }
        public abstract EventInfo[] GetEvents(BindingFlags bindingAttr);

        public PropertyInfo GetProperty(string name, BindingFlags bindingAttr, Binder binder,
                        Type returnType, Type[] types, ParameterModifier[] modifiers)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            if (types == null)
                throw new ArgumentNullException(nameof(types));
            return GetPropertyImpl(name, bindingAttr, binder, returnType, types, modifiers);
        }

        public PropertyInfo GetProperty(string name, Type returnType, Type[] types, ParameterModifier[] modifiers)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            if (types == null)
                throw new ArgumentNullException(nameof(types));
            return GetPropertyImpl(name, Type.DefaultLookup, null, returnType, types, modifiers);
        }

        public PropertyInfo GetProperty(string name, BindingFlags bindingAttr)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            return GetPropertyImpl(name, bindingAttr, null, null, null, null);
        }

        public PropertyInfo GetProperty(string name, Type returnType, Type[] types)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            if (types == null)
                throw new ArgumentNullException(nameof(types));
            return GetPropertyImpl(name, Type.DefaultLookup, null, returnType, types, null);
        }

        public PropertyInfo GetProperty(string name, Type[] types)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            if (types == null)
                throw new ArgumentNullException(nameof(types));
            return GetPropertyImpl(name, Type.DefaultLookup, null, null, types, null);
        }

        public PropertyInfo GetProperty(string name, Type returnType)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            if (returnType == null)
                throw new ArgumentNullException(nameof(returnType));
            return GetPropertyImpl(name, Type.DefaultLookup, null, returnType, null, null);
        }

        public PropertyInfo GetProperty(string name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            return GetPropertyImpl(name, Type.DefaultLookup, null, null, null, null);
        }

        protected abstract PropertyInfo GetPropertyImpl(string name, BindingFlags bindingAttr, Binder binder,
                        Type returnType, Type[] types, ParameterModifier[] modifiers);


        public abstract PropertyInfo[] GetProperties(BindingFlags bindingAttr);
        public PropertyInfo[] GetProperties()
        {
            return GetProperties(Type.DefaultLookup);
        }

        public Type[] GetNestedTypes()
        {
            return GetNestedTypes(Type.DefaultLookup);
        }

        public abstract Type[] GetNestedTypes(BindingFlags bindingAttr);

        public Type GetNestedType(string name)
        {
            return GetNestedType(name, Type.DefaultLookup);
        }

        public abstract Type GetNestedType(string name, BindingFlags bindingAttr);

        public MemberInfo[] GetMember(string name)
        {
            return GetMember(name, Type.DefaultLookup);
        }

        virtual public MemberInfo[] GetMember(string name, BindingFlags bindingAttr)
        {
            return GetMember(name, MemberTypes.All, bindingAttr);
        }

        virtual public MemberInfo[] GetMember(string name, MemberTypes type, BindingFlags bindingAttr)
        {
            throw new NotSupportedException(SR.NotSupported_SubclassOverride);
        }

        public MemberInfo[] GetMembers()
        {
            return GetMembers(Type.DefaultLookup);
        }
        public abstract MemberInfo[] GetMembers(BindingFlags bindingAttr);

        public virtual MemberInfo[] GetDefaultMembers()
        {
            throw new NotImplementedException();
        }

        public bool IsNested
        {
            get
            {
                return DeclaringType != null;
            }
        }

        public TypeAttributes Attributes
        {
            get { return GetAttributeFlagsImpl(); }
        }

        public virtual GenericParameterAttributes GenericParameterAttributes
        {
            get { throw new NotSupportedException(); }
        }

        public bool IsNotPublic
        {
            get { return ((GetAttributeFlagsImpl() & TypeAttributes.VisibilityMask) == TypeAttributes.NotPublic); }
        }

        public bool IsPublic
        {
            get { return ((GetAttributeFlagsImpl() & TypeAttributes.VisibilityMask) == TypeAttributes.Public); }
        }

        public bool IsNestedPublic
        {
            get { return ((GetAttributeFlagsImpl() & TypeAttributes.VisibilityMask) == TypeAttributes.NestedPublic); }
        }

        public bool IsNestedPrivate
        {
            get { return ((GetAttributeFlagsImpl() & TypeAttributes.VisibilityMask) == TypeAttributes.NestedPrivate); }
        }
        public bool IsNestedFamily
        {
            get { return ((GetAttributeFlagsImpl() & TypeAttributes.VisibilityMask) == TypeAttributes.NestedFamily); }
        }
        public bool IsNestedAssembly
        {
            get { return ((GetAttributeFlagsImpl() & TypeAttributes.VisibilityMask) == TypeAttributes.NestedAssembly); }
        }
        public bool IsNestedFamANDAssem
        {
            get { return ((GetAttributeFlagsImpl() & TypeAttributes.VisibilityMask) == TypeAttributes.NestedFamANDAssem); }
        }
        public bool IsNestedFamORAssem
        {
            get { return ((GetAttributeFlagsImpl() & TypeAttributes.VisibilityMask) == TypeAttributes.NestedFamORAssem); }
        }

        public bool IsAutoLayout
        {
            get { return ((GetAttributeFlagsImpl() & TypeAttributes.LayoutMask) == TypeAttributes.AutoLayout); }
        }
        public bool IsLayoutSequential
        {
            get { return ((GetAttributeFlagsImpl() & TypeAttributes.LayoutMask) == TypeAttributes.SequentialLayout); }
        }
        public bool IsExplicitLayout
        {
            get { return ((GetAttributeFlagsImpl() & TypeAttributes.LayoutMask) == TypeAttributes.ExplicitLayout); }
        }

        public bool IsClass
        {
            get { return ((GetAttributeFlagsImpl() & TypeAttributes.ClassSemanticsMask) == TypeAttributes.Class && !IsValueType); }
        }

        public bool IsInterface
        {
            get
            {
                RuntimeType rt = this as RuntimeType;
                if (rt != null)
                    return RuntimeTypeHandle.IsInterface(rt);

                return ((GetAttributeFlagsImpl() & TypeAttributes.ClassSemanticsMask) == TypeAttributes.Interface);
            }
        }

        public bool IsValueType
        {
            get { return IsValueTypeImpl(); }
        }

        public bool IsAbstract
        {
            get { return ((GetAttributeFlagsImpl() & TypeAttributes.Abstract) != 0); }
        }

        public bool IsSealed
        {
            get { return ((GetAttributeFlagsImpl() & TypeAttributes.Sealed) != 0); }
        }

        public virtual bool IsEnum
        {
            get
            {
                return IsSubclassOf(RuntimeType.EnumType);
            }
        }

        public bool IsSpecialName
        {
            get { return ((GetAttributeFlagsImpl() & TypeAttributes.SpecialName) != 0); }
        }

        public bool IsImport
        {
            get { return ((GetAttributeFlagsImpl() & TypeAttributes.Import) != 0); }
        }

        public virtual bool IsSerializable
        {
            get
            {
                if ((GetAttributeFlagsImpl() & TypeAttributes.Serializable) != 0)
                    return true;

                RuntimeType rt = this.UnderlyingSystemType as RuntimeType;

                if (rt != null)
                    return rt.IsSpecialSerializableType();

                return false;
            }
        }

        public bool IsAnsiClass
        {
            get { return ((GetAttributeFlagsImpl() & TypeAttributes.StringFormatMask) == TypeAttributes.AnsiClass); }
        }

        public bool IsUnicodeClass
        {
            get { return ((GetAttributeFlagsImpl() & TypeAttributes.StringFormatMask) == TypeAttributes.UnicodeClass); }
        }

        public bool IsAutoClass
        {
            get { return ((GetAttributeFlagsImpl() & TypeAttributes.StringFormatMask) == TypeAttributes.AutoClass); }
        }

        public bool IsArray
        {
            get { return IsArrayImpl(); }
        }

        public virtual bool IsSZArray
        {
            get { throw new NotImplementedException(); }
        }

        public virtual bool IsGenericType
        {
            get { return false; }
        }

        public virtual bool IsGenericTypeDefinition
        {
            get { return false; }
        }

        public virtual bool IsConstructedGenericType
        {
            get { throw new NotImplementedException(); }
        }

        public virtual bool IsGenericParameter
        {
            get { return false; }
        }

        public virtual int GenericParameterPosition
        {
            get { throw new InvalidOperationException(SR.Arg_NotGenericParameter); }
        }

        public virtual Type[] GetGenericParameterConstraints()
        {
            if (!IsGenericParameter)
                throw new InvalidOperationException(SR.Arg_NotGenericParameter);

            throw new InvalidOperationException();
        }

        public bool IsByRef
        {
            get { return IsByRefImpl(); }
        }
        public bool IsPointer
        {
            get { return IsPointerImpl(); }
        }
        public bool IsPrimitive
        {
            get { return IsPrimitiveImpl(); }
        }
        public bool IsCOMObject
        {
            get { return IsCOMObjectImpl(); }
        }

        public bool HasElementType
        {
            get { return HasElementTypeImpl(); }
        }

        public bool IsContextful
        {
            get { return IsContextfulImpl(); }
        }

        public bool IsMarshalByRef
        {
            get { return IsMarshalByRefImpl(); }
        }

        protected virtual bool IsValueTypeImpl()
        {
            return IsSubclassOf(RuntimeType.ValueType);
        }

        protected abstract TypeAttributes GetAttributeFlagsImpl();

        protected abstract bool IsArrayImpl();

        protected abstract bool IsByRefImpl();

        protected abstract bool IsPointerImpl();

        protected abstract bool IsPrimitiveImpl();

        protected abstract bool IsCOMObjectImpl();

        public virtual Type MakeGenericType(params Type[] typeArguments)
        {
            throw new NotSupportedException(SR.NotSupported_SubclassOverride);
        }


        protected virtual bool IsContextfulImpl()
        {
            return false;
        }

        protected virtual bool IsMarshalByRefImpl()
        {
            return false;
        }

        public abstract Type GetElementType();

        public virtual Type[] GetGenericArguments()
        {
            throw new NotSupportedException(SR.NotSupported_SubclassOverride);
        }

        public virtual Type[] GenericTypeArguments
        {
            get
            {
                if (IsGenericType && !IsGenericTypeDefinition)
                {
                    return GetGenericArguments();
                }
                else
                {
                    return Type.EmptyTypes;
                }
            }
        }

        public virtual Type GetGenericTypeDefinition()
        {
            throw new NotSupportedException(SR.NotSupported_SubclassOverride);
        }

        protected abstract bool HasElementTypeImpl();

        public virtual Array GetEnumValues()
        {
            if (!IsEnum)
                throw new ArgumentException(SR.Arg_MustBeEnum, "enumType");

            throw new NotImplementedException();
        }

        public virtual Type GetEnumUnderlyingType()
        {
            if (!IsEnum)
                throw new ArgumentException(SR.Arg_MustBeEnum, "enumType");

            FieldInfo[] fields = GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (fields == null || fields.Length != 1)
                throw new ArgumentException(SR.Argument_InvalidEnum, "enumType");

            return fields[0].FieldType;
        }

        public abstract Type UnderlyingSystemType
        {
            get;
        }

        public virtual bool IsInstanceOfType(object o)
        {
            if (o == null)
                return false;

            return IsAssignableFrom(o.GetType());
        }

        public virtual bool IsEquivalentTo(Type other)
        {
            return (this == other);
        }

        public override string ToString()
        {
            return "Type: " + Name;
        }

        public static Type[] GetTypeArray(object[] args)
        {
            if (args == null)
                throw new ArgumentNullException(nameof(args));
            Type[] cls = new Type[args.Length];
            for (int i = 0; i < cls.Length; i++)
            {
                if (args[i] == null)
                    throw new ArgumentNullException();
                cls[i] = args[i].GetType();
            }
            return cls;
        }

        public override bool Equals(object o)
        {
            if (o == null)
                return false;

            return Equals(o as Type);
        }

        public virtual bool Equals(Type o)
        {
            if ((object)o == null)
                return false;

            return (object.ReferenceEquals(this.UnderlyingSystemType, o.UnderlyingSystemType));
        }

        public override int GetHashCode()
        {
            Type SystemType = UnderlyingSystemType;
            if (!object.ReferenceEquals(SystemType, this))
                return SystemType.GetHashCode();
            return base.GetHashCode();
        }


        public virtual InterfaceMapping GetInterfaceMap(Type interfaceType)
        {
            throw new NotSupportedException(SR.NotSupported_SubclassOverride);
        }

        public new Type GetType()
        {
            return base.GetType();
        }

        private const BindingFlags DefaultLookup = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public;
    }
}
