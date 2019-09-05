// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Runtime.CompilerServices;
using Internal.Runtime.CompilerServices;

namespace System.Reflection
{
    public class FieldAccessor<TObject, TField> : ReadOnlyFieldAccessor<TObject, TField>
    {
        public FieldAccessor(FieldInfo fieldInfo) : base(fieldInfo)
        {
            // We perform two additioanl checks above and beyond the base type's ctor:
            // - The field must not be marked readonly
            // - The field's type must exactly match TField

            if (fieldInfo.IsInitOnly)
            {
                // TODO: Use a better resource string for this.
                throw new MissingMemberException(SR.MissingMemberTypeRef);
            }

            if (fieldInfo.FieldType != typeof(TField))
            {
                // TODO: Use a better resource string for this.
                throw new MissingMemberException(SR.MissingMemberTypeRef);
            }
        }

        // Delegate to the optimized base implementation, then reinterpret the returned
        // ref as a mutable ref instead of an immutable ref.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public new ref TField GetRef(ref TObject obj) => ref Unsafe.AsRef(in base.GetRef(in obj));
    }
}
