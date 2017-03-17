// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.Serialization;

namespace System.Reflection
{
    public partial class ParameterInfo : ICustomAttributeProvider, IObjectReference
    {
        // These are here only for backwards compatibility -- they are not set
        // until this instance is serialized, so don't rely on their values from
        // arbitrary code.
#pragma warning disable 169
        [OptionalField]
        private IntPtr _importer;
        [OptionalField]
        private int _token;
        [OptionalField]
        private bool bExtraConstChecked;
#pragma warning restore 169

        // this is an internal api for DynamicMethod. A better solution is to change the relationship
        // between ParameterInfo and ParameterBuilder so that a ParameterBuilder can be seen as a writer
        // api over a ParameterInfo. However that is a possible breaking change so it needs to go through some process first
        internal void SetName(String name)
        {
            NameImpl = name;
        }

        internal void SetAttributes(ParameterAttributes attributes)
        {
            AttrsImpl = attributes;
        }
    }
}
