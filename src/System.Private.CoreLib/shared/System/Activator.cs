// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Globalization;
using System.Reflection;
using System.Runtime.Remoting;

namespace System
{
    /// <summary>
    /// Activator contains the Activation (CreateInstance/New) methods for late bound support.
    /// </summary>
    public static partial class Activator
    {
        private const BindingFlags ConstructorDefault = BindingFlags.Instance | BindingFlags.Public | BindingFlags.CreateInstance;

        public static object CreateInstance(Type type, BindingFlags bindingAttr, Binder binder, object[] args, CultureInfo culture) =>
            CreateInstance(type, bindingAttr, binder, args, culture, null);

        public static object CreateInstance(Type type, params object[] args) =>
            CreateInstance(type, ConstructorDefault, null, args, null, null);

        public static object CreateInstance(Type type, object[] args, object[] activationAttributes) =>
            CreateInstance(type, ConstructorDefault, null, args, null, activationAttributes);

        public static object CreateInstance(Type type) =>
            CreateInstance(type, nonPublic: false);

        public static object CreateInstance(Type type, bool nonPublic) =>
            CreateInstance(type, nonPublic, wrapExceptions: true);

        public static ObjectHandle CreateInstanceFrom(string assemblyFile, string typeName)
        {
            return CreateInstanceFrom(assemblyFile, typeName, null);
        }

        public static ObjectHandle CreateInstanceFrom(string assemblyFile, string typeName, bool ignoreCase, BindingFlags bindingAttr, Binder binder, object[] args, CultureInfo culture, object[] activationAttributes)
        {
            return CreateInstanceFromInternal(assemblyFile,
                                              typeName,
                                              ignoreCase,
                                              bindingAttr,
                                              binder,
                                              args,
                                              culture,
                                              activationAttributes);
        }

        public static ObjectHandle CreateInstanceFrom(string assemblyFile, string typeName, object[] activationAttributes)
        {
            return CreateInstanceFromInternal(assemblyFile,
                                              typeName,
                                              false,
                                              ConstructorDefault,
                                              null,
                                              null,
                                              null,
                                              activationAttributes);
        }
    }
}
