// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Runtime.Serialization;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace System.Reflection
{
    public abstract partial class Module : ISerializable, ICustomAttributeProvider
    {
        static Module()
        {
            __Filters _fltObj;
            _fltObj = new __Filters();
            FilterTypeName = new TypeFilter(_fltObj.FilterTypeName);
            FilterTypeNameIgnoreCase = new TypeFilter(_fltObj.FilterTypeNameIgnoreCase);
        }

        // Used to provide implementation and overriding point for ModuleHandle.
        // To get a module handle inside mscorlib, use GetNativeHandle instead.
        internal virtual ModuleHandle GetModuleHandle()
        {
            return ModuleHandle.EmptyHandle;
        }
    }
}
