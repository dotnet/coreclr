// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Reflection;
using System.Globalization;
using System.Runtime.Remoting;
using System.Threading;

namespace System
{
    /// <summary>
    /// Activator contains the Activation (CreateInstance/New) methods for late bound support.
    /// </summary>
    public static class Activator
    {
        internal const BindingFlags ConstructorDefault = BindingFlags.Instance | BindingFlags.Public | BindingFlags.CreateInstance;

        public static object CreateInstance(Type type, BindingFlags bindingAttr, Binder binder, object[] args, CultureInfo culture) =>
            CreateInstance(type, bindingAttr, binder, args, culture, null);

        public static object CreateInstance(Type type, BindingFlags bindingAttr, Binder binder, object[] args, CultureInfo culture, object[] activationAttributes)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));

            if (type is System.Reflection.Emit.TypeBuilder)
                throw new NotSupportedException(SR.NotSupported_CreateInstanceWithTypeBuilder);

            // If they didn't specify a lookup, then we will provide the default lookup.
            const int LookupMask = 0x000000FF;
            if ((bindingAttr & (BindingFlags)LookupMask) == 0)
                bindingAttr |= ConstructorDefault;

            if (activationAttributes?.Length > 0)
                throw new PlatformNotSupportedException(SR.NotSupported_ActivAttr);

            if (type.UnderlyingSystemType is RuntimeType rt)
                return rt.CreateInstanceImpl(bindingAttr, binder, args, culture, activationAttributes);

            throw new ArgumentException(SR.Arg_MustBeType, nameof(type));
        }

        public static object CreateInstance(Type type, params object[] args) =>
            CreateInstance(type, ConstructorDefault, null, args, null, null);

        public static object CreateInstance(Type type, object[] args, object[] activationAttributes) =>
            CreateInstance(type, ConstructorDefault, null, args, null, activationAttributes);

        public static object CreateInstance(Type type) =>
            CreateInstance(type, nonPublic: false);

        public static object CreateInstance(Type type, bool nonPublic) =>
            CreateInstance(type, nonPublic, wrapExceptions: true);

        internal static object CreateInstance(Type type, bool nonPublic, bool wrapExceptions)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));

            if (type.UnderlyingSystemType is RuntimeType rt)
                return rt.CreateInstanceDefaultCtor(!nonPublic, false, true, wrapExceptions);

            throw new ArgumentException(SR.Arg_MustBeType, nameof(type));
        }

        public static ObjectHandle CreateInstance(string assemblyName, string typeName)
        {
            StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
            return CreateInstance(assemblyName,
                                  typeName,
                                  false,
                                  Activator.ConstructorDefault,
                                  null,
                                  null,
                                  null,
                                  null,
                                  ref stackMark);                                  
        }

        public static ObjectHandle CreateInstance(string assemblyName, string typeName, bool ignoreCase, BindingFlags bindingAttr, Binder binder, object[] args, CultureInfo culture, object[] activationAttributes)
        {
            StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
            return CreateInstance(assemblyName,
                                  typeName,
                                  ignoreCase,
                                  bindingAttr,
                                  binder,
                                  args,
                                  culture,
                                  activationAttributes,                                  
                                  ref stackMark);
        }

        public static ObjectHandle CreateInstance(string assemblyName, string typeName, object[] activationAttributes)
        {
            StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
            return CreateInstance(assemblyName,
                                  typeName,
                                  false,
                                  Activator.ConstructorDefault,
                                  null,
                                  null,
                                  null,
                                  activationAttributes,
                                  ref stackMark);
        }

        static internal ObjectHandle CreateInstance(String assemblyString,
                                                    String typeName,
                                                    bool ignoreCase,
                                                    BindingFlags bindingAttr,
                                                    Binder binder,
                                                    Object[] args,
                                                    CultureInfo culture,
                                                    Object[] activationAttributes,                                                    
                                                    ref StackCrawlMark stackMark)
        {
            Type type = null;
            Assembly assembly = null;
            if (assemblyString == null)
            {
                assembly = RuntimeAssembly.GetExecutingAssembly(ref stackMark);
            }
            else
            {
                RuntimeAssembly assemblyFromResolveEvent;
                AssemblyName assemblyName = RuntimeAssembly.CreateAssemblyName(assemblyString, out assemblyFromResolveEvent);
                if (assemblyFromResolveEvent != null)
                {
                    // Assembly was resolved via AssemblyResolve event
                    assembly = assemblyFromResolveEvent;
                }
                else if (assemblyName.ContentType == AssemblyContentType.WindowsRuntime)
                {
                    // WinRT type - we have to use Type.GetType
                    type = Type.GetType(typeName + ", " + assemblyString, true /*throwOnError*/, ignoreCase);
                }
                else
                {
                    // Classic managed type
                    assembly = RuntimeAssembly.InternalLoadAssemblyName(
                        assemblyName, null, ref stackMark,
                        true /*thrownOnFileNotFound*/);
                }
            }

            if (type == null)
            {
                // It's classic managed type (not WinRT type)                
                if (assembly == null)
                    return null;

                type = assembly.GetType(typeName, true /*throwOnError*/, ignoreCase);
            }

            Object o = Activator.CreateInstance(type,
                                                bindingAttr,
                                                binder,
                                                args,
                                                culture,
                                                activationAttributes);
            
            if (o == null)
                return null;
            else
            {
                ObjectHandle Handle = new ObjectHandle(o);
                return Handle;
            }
        }

        public static ObjectHandle CreateInstanceFrom(string assemblyFile, string typeName)
        {
            return CreateInstanceFrom(assemblyFile, typeName, null);
        }

        public static ObjectHandle CreateInstanceFrom(string assemblyFile, string typeName, bool ignoreCase, BindingFlags bindingAttr, Binder binder, object[] args, CultureInfo culture, object[] activationAttributes)
        {
            throw new NotImplementedException();
        }

        public static ObjectHandle CreateInstanceFrom(string assemblyFile, string typeName, object[] activationAttributes)
        {
            return CreateInstanceFrom(assemblyFile,
                                      typeName, 
                                      false,
                                      Activator.ConstructorDefault,
                                      null,
                                      null,
                                      null,
                                      activationAttributes);
        }

        public static T CreateInstance<T>()
        {
            var rt = (RuntimeType)typeof(T);

            // This is a workaround to maintain compatibility with V2. Without this we would throw a NotSupportedException for void[].
            // Array, Ref, and Pointer types don't have default constructors.
            if (rt.HasElementType)
                throw new MissingMethodException(SR.Arg_NoDefCTor);

            // Skip the CreateInstanceCheckThis call to avoid perf cost and to maintain compatibility with V2 (throwing the same exceptions).
            return (T)rt.CreateInstanceDefaultCtor(publicOnly: true, skipCheckThis: true, fillCache: true, wrapExceptions: true);
        }
    }
}
