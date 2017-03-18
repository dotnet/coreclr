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
    using System.Diagnostics.Contracts;
    using CultureInfo = System.Globalization.CultureInfo;
    using StackCrawlMark = System.Threading.StackCrawlMark;
    using DebuggerStepThroughAttribute = System.Diagnostics.DebuggerStepThroughAttribute;

    [Serializable]
    public abstract partial class Type : MemberInfo, IReflect
    {
        //
        // System.Type is appdomain agile type. Appdomain agile types cannot have precise static constructors. Make
        // sure to never introduce one here!
        //
        public static readonly MemberFilter FilterAttribute = new MemberFilter(__Filters.Instance.FilterAttribute);
        public static readonly MemberFilter FilterName = new MemberFilter(__Filters.Instance.FilterName);
        public static readonly MemberFilter FilterNameIgnoreCase = new MemberFilter(__Filters.Instance.FilterIgnoreCase);

        public static readonly object Missing = System.Reflection.Missing.Value;

        public static readonly char Delimiter = '.';

        // EmptyTypes is used to indicate that we are looking for someting without any parameters.
        public readonly static Type[] EmptyTypes = EmptyArray<Type>.Value;

        protected Type() { }


        // MemberInfo Methods....
        // The Member type Field.
        public override MemberTypes MemberType
        {
            get { return System.Reflection.MemberTypes.TypeInfo; }
        }

        // Return the class that declared this type.
        public override Type DeclaringType
        {
            get { return null; }
        }

        public virtual MethodBase DeclaringMethod { get { return null; } }

        // Return the class that was used to obtain this type.
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

        ////////////////////////////////////////////////////////////////////////////////
        // This will return a class based upon the progID.  This is provided for 
        // COM classic support.  Program ID's are not used in COM+ because they 
        // have been superceded by namespace.  (This routine is called this instead 
        // of getClass() because of the name conflict with the first method above.)
        //
        //   param progID:     the progID of the class to retrieve
        //   returns:          the class object associated to the progID
        ////
        public static Type GetTypeFromProgID(string progID)
        {
            return RuntimeType.GetTypeFromProgIDImpl(progID, null, false);
        }

        ////////////////////////////////////////////////////////////////////////////////
        // This will return a class based upon the progID.  This is provided for 
        // COM classic support.  Program ID's are not used in COM+ because they 
        // have been superceded by namespace.  (This routine is called this instead 
        // of getClass() because of the name conflict with the first method above.)
        //
        //   param progID:     the progID of the class to retrieve
        //   returns:          the class object associated to the progID
        ////
        public static Type GetTypeFromProgID(string progID, bool throwOnError)
        {
            return RuntimeType.GetTypeFromProgIDImpl(progID, null, throwOnError);
        }

        public static Type GetTypeFromProgID(string progID, string server)
        {
            return RuntimeType.GetTypeFromProgIDImpl(progID, server, false);
        }

        ////////////////////////////////////////////////////////////////////////////////
        // This will return a class based upon the CLSID.  This is provided for 
        // COM classic support.  
        //
        //   param CLSID:      the CLSID of the class to retrieve
        //   returns:          the class object associated to the CLSID
        ////
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

        // GetTypeCode
        // This method will return a TypeCode for the passed
        //  type.
        public static TypeCode GetTypeCode(Type type)
        {
            if (type == null)
                return TypeCode.Empty;
            return type.GetTypeCodeImpl();
        }

        protected virtual TypeCode GetTypeCodeImpl()
        {
            // System.RuntimeType overrides GetTypeCodeInternal
            // so we can assume that this is not a runtime type

            // this is true for EnumBuilder but not the other System.Type subclasses in BCL
            if (this != UnderlyingSystemType && UnderlyingSystemType != null)
                return Type.GetTypeCode(UnderlyingSystemType);

            return TypeCode.Object;
        }

        // Property representing the GUID associated with a class.
        public abstract Guid GUID
        {
            get;
        }

        // Description of the Binding Process.
        // We must invoke a method that is accessable and for which the provided
        // parameters have the most specific match.  A method may be called if
        // 1. The number of parameters in the method declaration equals the number of 
        //      arguments provided to the invocation
        // 2. The type of each argument can be converted by the binder to the
        //      type of the type of the parameter.
        //      
        // The binder will find all of the matching methods.  These method are found based
        // upon the type of binding requested (MethodInvoke, Get/Set Properties).  The set
        // of methods is filtered by the name, number of arguments and a set of search modifiers
        // defined in the Binder.
        // 
        // After the method is selected, it will be invoked.  Accessability is checked
        // at that point.  The search may be control which set of methods are searched based
        // upon the accessibility attribute associated with the method.
        // 
        // The BindToMethod method is responsible for selecting the method to be invoked.
        // For the default binder, the most specific method will be selected.
        // 
        // This will invoke a specific member...

        abstract public object InvokeMember(string name, BindingFlags invokeAttr, Binder binder, object target,
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


        // Module Property associated with a class.
        public new abstract Module Module { get; }

        // Assembly Property associated with a class.
        public abstract Assembly Assembly
        {
            [Pure]
            get;
        }

        // Assembly Property associated with a class.
        // A class handle is a unique integer value associated with
        // each class.  The handle is unique during the process life time.
        public virtual RuntimeTypeHandle TypeHandle
        {
            [Pure]
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

        // Return the fully qualified name.  The name does contain the namespace.
        public abstract string FullName
        {
            [Pure]
            get;
        }

        // Return the name space of the class.  
        public abstract string Namespace
        {
            [Pure]
            get;
        }


        public abstract string AssemblyQualifiedName
        {
            [Pure]
            get;
        }


        [Pure]
        public virtual int GetArrayRank()
        {
            Contract.Ensures(Contract.Result<int>() >= 0);
            throw new NotSupportedException(SR.NotSupported_SubclassOverride);
        }

        // Returns the base class for a class.  If this is an interface or has
        // no base class null is returned.  Object is the only Type that does not 
        // have a base class.  
        public abstract Type BaseType
        {
            [Pure]
            get;
        }


        // GetConstructor
        // This method will search for the specified constructor.  For constructors,
        //  unlike everything else, the default is to not look for static methods.  The
        //  reason is that we don't typically expose the class initializer.
        public ConstructorInfo GetConstructor(BindingFlags bindingAttr,
                                              Binder binder,
                                              CallingConventions callConvention,
                                              Type[] types,
                                              ParameterModifier[] modifiers)
        {
            // Must provide some types (Type[0] for nothing)
            if (types == null)
                throw new ArgumentNullException(nameof(types));
            Contract.EndContractBlock();
            for (int i = 0; i < types.Length; i++)
                if (types[i] == null)
                    throw new ArgumentNullException(nameof(types));
            return GetConstructorImpl(bindingAttr, binder, callConvention, types, modifiers);
        }

        public ConstructorInfo GetConstructor(BindingFlags bindingAttr, Binder binder, Type[] types, ParameterModifier[] modifiers)
        {
            if (types == null)
                throw new ArgumentNullException(nameof(types));
            Contract.EndContractBlock();
            for (int i = 0; i < types.Length; i++)
                if (types[i] == null)
                    throw new ArgumentNullException(nameof(types));
            return GetConstructorImpl(bindingAttr, binder, CallingConventions.Any, types, modifiers);
        }

        public ConstructorInfo GetConstructor(Type[] types)
        {
            // The arguments are checked in the called version of GetConstructor.
            return GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, types, null);
        }

        abstract protected ConstructorInfo GetConstructorImpl(BindingFlags bindingAttr,
                                                              Binder binder,
                                                              CallingConventions callConvention,
                                                              Type[] types,
                                                              ParameterModifier[] modifiers);

        // GetConstructors()
        // This routine will return an array of all constructors supported by the class.
        //  Unlike everything else, the default is to not look for static methods.  The
        //  reason is that we don't typically expose the class initializer.
        public ConstructorInfo[] GetConstructors()
        {
            return GetConstructors(BindingFlags.Public | BindingFlags.Instance);
        }

        abstract public ConstructorInfo[] GetConstructors(BindingFlags bindingAttr);

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


        // Return a method based upon the passed criteria.  The name of the method
        // must be provided, and exception is thrown if it is not.  The bindingAttr
        // parameter indicates if non-public methods should be searched.  The types
        // array indicates the types of the parameters being looked for.
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
            Contract.EndContractBlock();
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
            Contract.EndContractBlock();
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
            Contract.EndContractBlock();
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
            Contract.EndContractBlock();
            for (int i = 0; i < types.Length; i++)
                if (types[i] == null)
                    throw new ArgumentNullException(nameof(types));
            return GetMethodImpl(name, Type.DefaultLookup, null, CallingConventions.Any, types, null);
        }

        public MethodInfo GetMethod(string name, BindingFlags bindingAttr)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            Contract.EndContractBlock();
            return GetMethodImpl(name, bindingAttr, null, CallingConventions.Any, null, null);
        }

        public MethodInfo GetMethod(string name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            Contract.EndContractBlock();
            return GetMethodImpl(name, Type.DefaultLookup, null, CallingConventions.Any, null, null);
        }

        abstract protected MethodInfo GetMethodImpl(string name,
                                                    BindingFlags bindingAttr,
                                                    Binder binder,
                                                    CallingConventions callConvention,
                                                    Type[] types,
                                                    ParameterModifier[] modifiers);


        // GetMethods
        // This routine will return all the methods implemented by the class
        public MethodInfo[] GetMethods()
        {
            return GetMethods(Type.DefaultLookup);
        }

        abstract public MethodInfo[] GetMethods(BindingFlags bindingAttr);

        // GetField
        // Get Field will return a specific field based upon name
        abstract public FieldInfo GetField(string name, BindingFlags bindingAttr);


        public FieldInfo GetField(string name)
        {
            return GetField(name, Type.DefaultLookup);
        }


        // GetFields
        // Get fields will return a full array of fields implemented by a class
        public FieldInfo[] GetFields()
        {
            return GetFields(Type.DefaultLookup);
        }
        abstract public FieldInfo[] GetFields(BindingFlags bindingAttr);

        // GetInterface
        // This method will return an interface (as a class) based upon
        //  the passed in name.
        public Type GetInterface(string name)
        {
            return GetInterface(name, false);
        }
        abstract public Type GetInterface(string name, bool ignoreCase);


        // GetInterfaces
        // This method will return all of the interfaces implemented by a class
        abstract public Type[] GetInterfaces();

        // GetEvent
        // This method will return a event by name if it is found.
        //  null is returned if the event is not found


        public EventInfo GetEvent(string name)
        {
            return GetEvent(name, Type.DefaultLookup);
        }
        abstract public EventInfo GetEvent(string name, BindingFlags bindingAttr);

        // GetEvents
        // This method will return an array of EventInfo.  If there are not Events
        //  an empty array will be returned.         
        virtual public EventInfo[] GetEvents()
        {
            return GetEvents(Type.DefaultLookup);
        }
        abstract public EventInfo[] GetEvents(BindingFlags bindingAttr);


        // Return a property based upon the passed criteria.  The nameof the
        // parameter must be provided.  
        public PropertyInfo GetProperty(string name, BindingFlags bindingAttr, Binder binder,
                        Type returnType, Type[] types, ParameterModifier[] modifiers)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            if (types == null)
                throw new ArgumentNullException(nameof(types));
            Contract.EndContractBlock();
            return GetPropertyImpl(name, bindingAttr, binder, returnType, types, modifiers);
        }

        public PropertyInfo GetProperty(string name, Type returnType, Type[] types, ParameterModifier[] modifiers)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            if (types == null)
                throw new ArgumentNullException(nameof(types));
            Contract.EndContractBlock();
            return GetPropertyImpl(name, Type.DefaultLookup, null, returnType, types, modifiers);
        }

        public PropertyInfo GetProperty(string name, BindingFlags bindingAttr)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            Contract.EndContractBlock();
            return GetPropertyImpl(name, bindingAttr, null, null, null, null);
        }

        public PropertyInfo GetProperty(string name, Type returnType, Type[] types)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            if (types == null)
                throw new ArgumentNullException(nameof(types));
            Contract.EndContractBlock();
            return GetPropertyImpl(name, Type.DefaultLookup, null, returnType, types, null);
        }

        public PropertyInfo GetProperty(string name, Type[] types)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            if (types == null)
                throw new ArgumentNullException(nameof(types));
            Contract.EndContractBlock();
            return GetPropertyImpl(name, Type.DefaultLookup, null, null, types, null);
        }

        public PropertyInfo GetProperty(string name, Type returnType)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            if (returnType == null)
                throw new ArgumentNullException(nameof(returnType));
            Contract.EndContractBlock();
            return GetPropertyImpl(name, Type.DefaultLookup, null, returnType, null, null);
        }

        public PropertyInfo GetProperty(string name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            Contract.EndContractBlock();
            return GetPropertyImpl(name, Type.DefaultLookup, null, null, null, null);
        }

        protected abstract PropertyInfo GetPropertyImpl(string name, BindingFlags bindingAttr, Binder binder,
                        Type returnType, Type[] types, ParameterModifier[] modifiers);


        // GetProperties
        // This method will return an array of all of the properties defined
        //  for a Type.
        abstract public PropertyInfo[] GetProperties(BindingFlags bindingAttr);
        public PropertyInfo[] GetProperties()
        {
            return GetProperties(Type.DefaultLookup);
        }

        // GetNestedTypes()
        // This set of method will return any nested types that are found inside
        //  of the type.
        public Type[] GetNestedTypes()
        {
            return GetNestedTypes(Type.DefaultLookup);
        }

        abstract public Type[] GetNestedTypes(BindingFlags bindingAttr);

        public Type GetNestedType(string name)
        {
            return GetNestedType(name, Type.DefaultLookup);
        }

        abstract public Type GetNestedType(string name, BindingFlags bindingAttr);

        // GetMember
        // This method will return all of the members which match the specified string
        // passed into the method
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


        // GetMembers
        // This will return a Member array of all of the members of a class
        public MemberInfo[] GetMembers()
        {
            return GetMembers(Type.DefaultLookup);
        }
        abstract public MemberInfo[] GetMembers(BindingFlags bindingAttr);

        // GetDefaultMembers
        // This will return a MemberInfo that has been marked with the
        //      DefaultMemberAttribute
        public virtual MemberInfo[] GetDefaultMembers()
        {
            throw new NotImplementedException();
        }

        ////////////////////////////////////////////////////////////////////////////////
        //
        // Attributes
        //
        //   The attributes are all treated as read-only properties on a class.  Most of
        //  these boolean properties have flag values defined in this class and act like
        //  a bit mask of attributes.  There are also a set of boolean properties that
        //  relate to the classes relationship to other classes and to the state of the
        //  class inside the runtime.
        //
        ////////////////////////////////////////////////////////////////////////////////

        public bool IsNested
        {
            [Pure]
            get
            {
                return DeclaringType != null;
            }
        }

        // The attribute property on the Type.
        public TypeAttributes Attributes
        {
            [Pure]
            get { return GetAttributeFlagsImpl(); }
        }

        public virtual GenericParameterAttributes GenericParameterAttributes
        {
            get { throw new NotSupportedException(); }
        }

        public bool IsNotPublic
        {
            [Pure]
            get { return ((GetAttributeFlagsImpl() & TypeAttributes.VisibilityMask) == TypeAttributes.NotPublic); }
        }

        public bool IsPublic
        {
            [Pure]
            get { return ((GetAttributeFlagsImpl() & TypeAttributes.VisibilityMask) == TypeAttributes.Public); }
        }

        public bool IsNestedPublic
        {
            [Pure]
            get { return ((GetAttributeFlagsImpl() & TypeAttributes.VisibilityMask) == TypeAttributes.NestedPublic); }
        }

        public bool IsNestedPrivate
        {
            [Pure]
            get { return ((GetAttributeFlagsImpl() & TypeAttributes.VisibilityMask) == TypeAttributes.NestedPrivate); }
        }
        public bool IsNestedFamily
        {
            [Pure]
            get { return ((GetAttributeFlagsImpl() & TypeAttributes.VisibilityMask) == TypeAttributes.NestedFamily); }
        }
        public bool IsNestedAssembly
        {
            [Pure]
            get { return ((GetAttributeFlagsImpl() & TypeAttributes.VisibilityMask) == TypeAttributes.NestedAssembly); }
        }
        public bool IsNestedFamANDAssem
        {
            [Pure]
            get { return ((GetAttributeFlagsImpl() & TypeAttributes.VisibilityMask) == TypeAttributes.NestedFamANDAssem); }
        }
        public bool IsNestedFamORAssem
        {
            [Pure]
            get { return ((GetAttributeFlagsImpl() & TypeAttributes.VisibilityMask) == TypeAttributes.NestedFamORAssem); }
        }

        public bool IsAutoLayout
        {
            [Pure]
            get { return ((GetAttributeFlagsImpl() & TypeAttributes.LayoutMask) == TypeAttributes.AutoLayout); }
        }
        public bool IsLayoutSequential
        {
            [Pure]
            get { return ((GetAttributeFlagsImpl() & TypeAttributes.LayoutMask) == TypeAttributes.SequentialLayout); }
        }
        public bool IsExplicitLayout
        {
            [Pure]
            get { return ((GetAttributeFlagsImpl() & TypeAttributes.LayoutMask) == TypeAttributes.ExplicitLayout); }
        }

        public bool IsClass
        {
            [Pure]
            get { return ((GetAttributeFlagsImpl() & TypeAttributes.ClassSemanticsMask) == TypeAttributes.Class && !IsValueType); }
        }

        public bool IsInterface
        {
            [Pure]
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
            [Pure]
            get { return IsValueTypeImpl(); }
        }

        public bool IsAbstract
        {
            [Pure]
            get { return ((GetAttributeFlagsImpl() & TypeAttributes.Abstract) != 0); }
        }

        public bool IsSealed
        {
            [Pure]
            get { return ((GetAttributeFlagsImpl() & TypeAttributes.Sealed) != 0); }
        }

        public virtual bool IsEnum
        {
            [Pure]
            get
            {
                // This will return false for a non-runtime Type object unless it overrides IsSubclassOf.
                return IsSubclassOf(RuntimeType.EnumType);
            }
        }

        public bool IsSpecialName
        {
            [Pure]
            get { return ((GetAttributeFlagsImpl() & TypeAttributes.SpecialName) != 0); }
        }

        public bool IsImport
        {
            [Pure]
            get { return ((GetAttributeFlagsImpl() & TypeAttributes.Import) != 0); }
        }

        public virtual bool IsSerializable
        {
            [Pure]
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
            [Pure]
            get { return ((GetAttributeFlagsImpl() & TypeAttributes.StringFormatMask) == TypeAttributes.AnsiClass); }
        }

        public bool IsUnicodeClass
        {
            [Pure]
            get { return ((GetAttributeFlagsImpl() & TypeAttributes.StringFormatMask) == TypeAttributes.UnicodeClass); }
        }

        public bool IsAutoClass
        {
            [Pure]
            get { return ((GetAttributeFlagsImpl() & TypeAttributes.StringFormatMask) == TypeAttributes.AutoClass); }
        }

        // These are not backed up by attributes.  Instead they are implemented
        //      based internally.
        public bool IsArray
        {
            [Pure]
            get { return IsArrayImpl(); }
        }

        public virtual bool IsSZArray
        {
            [Pure]
            get { throw new NotImplementedException(); }
        }

        public virtual bool IsGenericType
        {
            [Pure]
            get { return false; }
        }

        public virtual bool IsGenericTypeDefinition
        {
            [Pure]
            get { return false; }
        }

        public virtual bool IsConstructedGenericType
        {
            [Pure]
            get { throw new NotImplementedException(); }
        }

        public virtual bool IsGenericParameter
        {
            [Pure]
            get { return false; }
        }

        public virtual int GenericParameterPosition
        {
            [Pure]
            get { throw new InvalidOperationException(SR.Arg_NotGenericParameter); }
        }

        [Pure]
        public virtual Type[] GetGenericParameterConstraints()
        {
            if (!IsGenericParameter)
                throw new InvalidOperationException(SR.Arg_NotGenericParameter);
            Contract.EndContractBlock();

            throw new InvalidOperationException();
        }

        public bool IsByRef
        {
            [Pure]
            get { return IsByRefImpl(); }
        }
        public bool IsPointer
        {
            [Pure]
            get { return IsPointerImpl(); }
        }
        public bool IsPrimitive
        {
            [Pure]
            get { return IsPrimitiveImpl(); }
        }
        public bool IsCOMObject
        {
            [Pure]
            get { return IsCOMObjectImpl(); }
        }

        public bool HasElementType
        {
            [Pure]
            get { return HasElementTypeImpl(); }
        }

        public bool IsContextful
        {
            [Pure]
            get { return IsContextfulImpl(); }
        }

        public bool IsMarshalByRef
        {
            [Pure]
            get { return IsMarshalByRefImpl(); }
        }

        // Protected routine to determine if this class represents a value class
        // The default implementation of IsValueTypeImpl never returns true for non-runtime types.
        protected virtual bool IsValueTypeImpl()
        {
            // Note that typeof(Enum) and typeof(ValueType) are not themselves value types.
            // But there is no point excluding them here because customer derived System.Type 
            // (non-runtime type) objects can never be equal to a runtime type, which typeof(XXX) is.
            // Ideally we should throw a NotImplementedException here or just return false because
            // customer implementations of IsSubclassOf should never return true between a non-runtime
            // type and a runtime type. There is no benefits in making that breaking change though.

            return IsSubclassOf(RuntimeType.ValueType);
        }

        // Protected routine to get the attributes.
        abstract protected TypeAttributes GetAttributeFlagsImpl();

        // Protected routine to determine if this class represents an Array
        abstract protected bool IsArrayImpl();

        // Protected routine to determine if this class is a ByRef
        abstract protected bool IsByRefImpl();

        // Protected routine to determine if this class is a Pointer
        abstract protected bool IsPointerImpl();

        // Protected routine to determine if this class represents a primitive type
        abstract protected bool IsPrimitiveImpl();

        // Protected routine to determine if this class represents a COM object
        abstract protected bool IsCOMObjectImpl();

        public virtual Type MakeGenericType(params Type[] typeArguments)
        {
            Contract.Ensures(Contract.Result<Type>() != null);
            throw new NotSupportedException(SR.NotSupported_SubclassOverride);
        }


        // Protected routine to determine if this class is contextful
        protected virtual bool IsContextfulImpl()
        {
            return false;
        }

        // Protected routine to determine if this class is marshaled by ref
        protected virtual bool IsMarshalByRefImpl()
        {
            return false;
        }

        [Pure]
        abstract public Type GetElementType();

        [Pure]
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

        [Pure]
        public virtual Type GetGenericTypeDefinition()
        {
            Contract.Ensures(Contract.Result<Type>() != null);
            throw new NotSupportedException(SR.NotSupported_SubclassOverride);
        }

        [Pure]
        abstract protected bool HasElementTypeImpl();

        // We don't support GetEnumValues in the default implementation because we cannot create an array of
        // a non-runtime type. If there is strong need we can consider returning an object or int64 array.
        public virtual Array GetEnumValues()
        {
            if (!IsEnum)
                throw new ArgumentException(SR.Arg_MustBeEnum, "enumType");
            Contract.Ensures(Contract.Result<Array>() != null);

            throw new NotImplementedException();
        }

        public virtual Type GetEnumUnderlyingType()
        {
            if (!IsEnum)
                throw new ArgumentException(SR.Arg_MustBeEnum, "enumType");
            Contract.Ensures(Contract.Result<Type>() != null);

            FieldInfo[] fields = GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (fields == null || fields.Length != 1)
                throw new ArgumentException(SR.Argument_InvalidEnum, "enumType");

            return fields[0].FieldType;
        }

        public virtual bool IsSecurityCritical { [Pure] get { throw new NotImplementedException(); } }

        public virtual bool IsSecuritySafeCritical { [Pure] get { throw new NotImplementedException(); } }

        public virtual bool IsSecurityTransparent { [Pure] get { throw new NotImplementedException(); } }

        // The behavior of UnderlyingSystemType varies from type to type.
        // For IReflect objects: Return the underlying Type that represents the IReflect Object.
        // For expando object: this is the (Object) IReflectInstance.GetType().  For Type object it is this.
        // It could also return the baked type or the underlying enum type in RefEmit. See the comment in
        // code:TypeBuilder.SetConstantValue.
        public abstract Type UnderlyingSystemType
        {
            get;
        }

        // Returns true if the object passed is assignable to an instance of this class.
        // Everything else returns false. 
        // 
        [Pure]
        public virtual bool IsInstanceOfType(object o)
        {
            if (o == null)
                return false;

            // No need for transparent proxy casting check here
            // because it never returns true for a non-rutnime type.

            return IsAssignableFrom(o.GetType());
        }

        // Base implementation that does only ==.
        [Pure]
        public virtual bool IsEquivalentTo(Type other)
        {
            return (this == other);
        }

        // ToString
        // Print the string Representation of the Type
        public override string ToString()
        {
            // Why do we add the "Type: " prefix? RuntimeType.ToString() doesn't include it.
            return "Type: " + Name;
        }

        // This method will return an array of classes based upon the array of 
        // types.
        public static Type[] GetTypeArray(object[] args)
        {
            if (args == null)
                throw new ArgumentNullException(nameof(args));
            Contract.EndContractBlock();
            Type[] cls = new Type[args.Length];
            for (int i = 0; i < cls.Length; i++)
            {
                if (args[i] == null)
                    throw new ArgumentNullException();
                cls[i] = args[i].GetType();
            }
            return cls;
        }

        [Pure]
        public override bool Equals(object o)
        {
            if (o == null)
                return false;

            return Equals(o as Type);
        }

        [Pure]
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


        // GetInterfaceMap
        // This method will return an interface mapping for the interface
        //  requested.  It will throw an argument exception if the Type doesn't
        //  implemenet the interface.
        public virtual InterfaceMapping GetInterfaceMap(Type interfaceType)
        {
            throw new NotSupportedException(SR.NotSupported_SubclassOverride);
        }

        // this method is required so Object.GetType is not made virtual by the compiler 
        public new Type GetType()
        {
            return base.GetType();
        }

        // private convenience data
        private const BindingFlags DefaultLookup = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public;
    }
}
