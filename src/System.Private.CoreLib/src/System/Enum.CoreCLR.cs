// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System
{
    partial class Enum
    {
        [DllImport(JitHelpers.QCall, CharSet = CharSet.Unicode)]
        private static extern void GetEnumValuesAndNames(RuntimeTypeHandle enumType, ObjectHandleOnStack values, ObjectHandleOnStack names, bool getNames);

        private static TypeValuesAndNames GetCachedValuesAndNames(RuntimeType enumType, bool getNames)
        {
            TypeValuesAndNames entry = enumType.GenericCache as TypeValuesAndNames;

            if (entry == null || (getNames && entry.Names == null))
            {
                ulong[] values = null;
                string[] names = null;
                GetEnumValuesAndNames(
                    enumType.GetTypeHandleInternal(),
                    JitHelpers.GetObjectHandleOnStack(ref values),
                    JitHelpers.GetObjectHandleOnStack(ref names),
                    getNames);
                bool isFlags = enumType.IsDefined(typeof(FlagsAttribute), inherit: false);

                entry = new TypeValuesAndNames(isFlags, values, names);
                enumType.GenericCache = entry;
            }

            return entry;
        }

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private extern CorElementType InternalGetCorElementType();

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern override bool Equals(object obj);
    }
}
