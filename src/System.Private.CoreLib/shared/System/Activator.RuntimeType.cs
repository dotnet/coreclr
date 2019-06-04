// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Reflection;
using System.Globalization;
using System.Runtime.Loader;
using System.Runtime.Remoting;
using System.Threading;

namespace System
{
    public static partial class Activator
    {
        public static object? CreateInstance(Type type, BindingFlags bindingAttr, Binder? binder, object?[]? args, CultureInfo? culture, object?[]? activationAttributes)
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
                return rt.CreateInstanceImpl(bindingAttr, binder, args, culture);

            throw new ArgumentException(SR.Arg_MustBeType, nameof(type));
        }

        [System.Security.DynamicSecurityMethod]
        public static ObjectHandle? CreateInstance(string assemblyName, string typeName)
        {
            StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
            return CreateInstanceInternal(assemblyName,
                                          typeName,
                                          false,
                                          ConstructorDefault,
                                          null,
                                          null,
                                          null,
                                          null,
                                          ref stackMark);
        }

        [System.Security.DynamicSecurityMethod]
        public static ObjectHandle? CreateInstance(string assemblyName, string typeName, bool ignoreCase, BindingFlags bindingAttr, Binder? binder, object?[]? args, CultureInfo? culture, object?[]? activationAttributes)
        {
            StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
            return CreateInstanceInternal(assemblyName,
                                          typeName,
                                          ignoreCase,
                                          bindingAttr,
                                          binder,
                                          args,
                                          culture,
                                          activationAttributes,
                                          ref stackMark);
        }

        [System.Security.DynamicSecurityMethod]
        public static ObjectHandle? CreateInstance(string assemblyName, string typeName, object?[]? activationAttributes)
        {
            StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
            return CreateInstanceInternal(assemblyName,
                                          typeName,
                                          false,
                                          ConstructorDefault,
                                          null,
                                          null,
                                          null,
                                          activationAttributes,
                                          ref stackMark);
        }

        public static object? CreateInstance(Type type, bool nonPublic) =>
            CreateInstance(type, nonPublic, wrapExceptions: true);

        internal static object? CreateInstance(Type type, bool nonPublic, bool wrapExceptions)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));

            if (type.UnderlyingSystemType is RuntimeType rt)
                return rt.CreateInstanceDefaultCtor(publicOnly: !nonPublic, skipCheckThis: false, fillCache: true, wrapExceptions: wrapExceptions);

            throw new ArgumentException(SR.Arg_MustBeType, nameof(type));
        }

        private static ObjectHandle? CreateInstanceInternal(string assemblyString,
                                                           string typeName,
                                                           bool ignoreCase,
                                                           BindingFlags bindingAttr,
                                                           Binder? binder,
                                                           object?[]? args,
                                                           CultureInfo? culture,
                                                           object?[]? activationAttributes,
                                                           ref StackCrawlMark stackMark)
        {
            Type? type = null;
            Assembly? assembly = null;
            if (assemblyString == null)
            {
                assembly = Assembly.GetExecutingAssembly(ref stackMark);
            }
            else
            {
                AssemblyName assemblyName = new AssemblyName(assemblyString);

                if (assemblyName.ContentType == AssemblyContentType.WindowsRuntime)
                {
                    // WinRT type - we have to use Type.GetType
                    type = Type.GetType(typeName + ", " + assemblyString, true /*throwOnError*/, ignoreCase);
                }
                else
                {
                    // Classic managed type
                    assembly = RuntimeAssembly.InternalLoadAssemblyName(
                        assemblyName, ref stackMark, AssemblyLoadContext.CurrentContextualReflectionContext);
                }
            }

            if (type == null)
            {                
                type = assembly!.GetType(typeName, throwOnError: true, ignoreCase);
            }

            object? o = CreateInstance(type, bindingAttr, binder, args, culture, activationAttributes);

            return o != null ? new ObjectHandle(o) : null;
        }

        public static T CreateInstance<T>()
        {
            var entry = ActivatorCache<T>.Entry;
            if (entry.IsNotActivatable)
                throw new MissingMethodException(SR.Format(SR.Arg_NoDefCTor, entry.RuntimeType));

            if (entry.IsValueType)
            {
                T instance = default!;

                if (entry.ConstructStruct != null)
                {
                    try
                    {
                        entry.ConstructStruct(ref instance);
                    }
                    catch (Exception exception)
                    {
                        throw new TargetInvocationException(exception);
                    }
                }

                return instance;
            }
            else
            {
                if (entry.ConstructClass == null)
                    throw new MissingMethodException(SR.Format(SR.Arg_NoDefCTor, entry.RuntimeType));

                T instance = (T)RuntimeTypeHandle.Allocate(entry.RuntimeType);

                try
                {
                    entry.ConstructClass(instance);
                }
                catch (Exception exception)
                {
                    throw new TargetInvocationException(exception);
                }

                return instance;
            }
        }

        private delegate void ClassConstructor<T>(T instance);
        private delegate void StructConstructor<T>(ref T instance);

        private static T CreateDelegate<T>(MethodBase methodInfo)
            where T : Delegate
        {
            return (T)typeof(T)
                .GetConstructor(new[] { typeof(object), typeof(IntPtr) })!
                .Invoke(new object?[] { null, methodInfo.MethodHandle.GetFunctionPointer() });
        }

        private sealed class ActivatorCache<T>
        {
            internal static readonly ActivatorCache<T> Entry = new ActivatorCache<T>();

            internal readonly RuntimeType RuntimeType;

            internal readonly bool IsValueType;
            internal readonly bool IsNotActivatable;

            internal readonly ClassConstructor<T>? ConstructClass;
            internal readonly StructConstructor<T>? ConstructStruct;

            private ActivatorCache()
            {
                RuntimeType = (RuntimeType)typeof(T);
                IsValueType = RuntimeType.IsValueType;

                // The HasElementType check is a workaround to maintain compatibility with V2.
                // Without this we would throw a NotSupportedException for void[].
                // Array, Ref, and Pointer types don't have default constructors.
                IsNotActivatable = RuntimeType.IsAbstract || RuntimeType.HasElementType;

                if (IsNotActivatable)
                    return;

                ConstructorInfo? runtimeCtorInfo = RuntimeType.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null);

                if (runtimeCtorInfo != null && runtimeCtorInfo.IsPublic)
                {
                    if (IsValueType)
                        ConstructStruct = CreateDelegate<StructConstructor<T>>(runtimeCtorInfo);
                    else
                        ConstructClass = CreateDelegate<ClassConstructor<T>>(runtimeCtorInfo);
                }
                else
                {
                    IsNotActivatable = !(IsValueType && runtimeCtorInfo == null);
                }
            }
        }
    }
}
