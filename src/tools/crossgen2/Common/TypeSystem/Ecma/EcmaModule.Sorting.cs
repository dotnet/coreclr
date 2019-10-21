// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

using Debug = System.Diagnostics.Debug;

namespace Internal.TypeSystem.Ecma
{
    partial class EcmaModule
    {
        protected internal override int ClassCode => 50698845;

        protected internal override int CompareToImpl(ModuleDesc other, TypeSystemComparer comparer)
        {
            if (this == other)
                return 0;

            EcmaModule otherModule = (EcmaModule)other;
            Guid thisMvid = _metadataReader.GetGuid(_metadataReader.GetModuleDefinition().Mvid);
            Guid otherMvid = otherModule._metadataReader.GetGuid(otherModule.MetadataReader.GetModuleDefinition().Mvid);

            Debug.Assert(thisMvid.CompareTo(otherMvid) != 0, "Different instance of EcmaModule but same MVID?");
            return thisMvid.CompareTo(otherMvid);
        }
    }
}
