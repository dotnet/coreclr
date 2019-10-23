// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Reflection.Metadata.Ecma335;

namespace Internal.TypeSystem.Ecma
{
    // Functionality related to deterministic ordering of types
    partial class EcmaMethod
    {
        protected internal override int ClassCode => 1419431046;

        protected internal override int CompareToImpl(MethodDesc other, TypeSystemComparer comparer)
        {
            var otherMethod = (EcmaMethod)other;

            EcmaModule module = _type.EcmaModule;
            EcmaModule otherModule = otherMethod._type.EcmaModule;
            
            int result = module.MetadataReader.GetToken(_handle) - otherModule.MetadataReader.GetToken(otherMethod._handle);
            if (result != 0)
                return result;

            return module.CompareTo(otherModule);
        }
    }
}
