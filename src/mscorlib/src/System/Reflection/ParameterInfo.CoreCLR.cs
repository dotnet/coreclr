// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Runtime.Serialization;

namespace System.Reflection
{
    public partial class ParameterInfo : ICustomAttributeProvider, IObjectReference
    {
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
