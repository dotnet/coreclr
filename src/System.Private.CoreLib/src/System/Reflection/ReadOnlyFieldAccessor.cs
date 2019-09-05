// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;
using System.Runtime.CompilerServices;
using Internal.Runtime.CompilerServices;

namespace System.Reflection
{
    public class ReadOnlyFieldAccessor<TObject, TField>
    {
        private readonly IntPtr _fieldOffset;

        public ReadOnlyFieldAccessor(FieldInfo fieldInfo)
        {
            // There are four checks we perform:
            // - The field must be a regular RtFieldInfo (not a manufactured FieldInfo)
            // - The field must be an instance field
            // - The field must be declared on TObject or a superclass
            // - The field's type must be reinterpret_castable to TField

            if (fieldInfo is null)
            {
                throw new ArgumentNullException(nameof(fieldInfo));
            }

            if (!(fieldInfo is RtFieldInfo rtFieldInfo))
            {
                throw new ArgumentException(SR.Argument_MustBeRuntimeFieldInfo, nameof(fieldInfo));
            }

            if (rtFieldInfo.IsStatic)
            {
                // TODO: Use a better resource string for this.
                throw new ArgumentException(SR.Format(SR.Argument_TypedReferenceInvalidField, rtFieldInfo.Name));
            }

            Type fieldDeclaringType = rtFieldInfo.GetDeclaringTypeInternal();
            Debug.Assert(fieldDeclaringType != null);

            if (typeof(TObject) != fieldDeclaringType && !typeof(TObject).IsSubclassOf(fieldDeclaringType))
            {
                // TODO: Use a better resource string for this.
                throw new MissingMemberException(SR.MissingMemberTypeRef);
            }

            Type fieldType = rtFieldInfo.FieldType;
            Debug.Assert(fieldType != null);

            if (fieldType.IsValueType)
            {
                // For value types, TField must exactly match the FieldInfo's actual type
                // since there's no superclass hierarchy we're able to reinterpret_cast to.

                if (typeof(TField) != fieldType)
                {
                    // TODO: Use a better resource string for this.
                    throw new MissingMemberException(SR.MissingMemberTypeRef);
                }
            }
            else
            {
                // For reference types, we only care that the FieldInfo's actual type can
                // be reinterpret_cast to TObject. For example, it'll be valid to reinterpret_cast
                // to 'object' or to an interface that the FieldInfo's actual type implements.

                if (!typeof(TField).IsAssignableFrom(fieldType))
                {
                    // TODO: Use a better resource string for this.
                    throw new MissingMemberException(SR.MissingMemberTypeRef);
                }
            }

            _fieldOffset = rtFieldInfo.GetOffsetInBytes();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref readonly TField GetRef(in TObject obj)
        {
            // Normally, the JIT would perform a null check on "this" before invoking the instance method.
            // By moving the this._fieldOffset dereference to the beginning of the method and storing it
            // as a local copy, the JIT is able to elide the earlier null check because the line immediately
            // following would cause the correct NullReferenceException if the caller called (null).GetRef(...).

            IntPtr fieldOffset = _fieldOffset;

            if (RuntimeHelpers.IsReference<TObject>())
            {
                // For reference types, we need to dereference the 'in' parameter before we can get the
                // address of the first field in the object. The call to GetRawData below will also perform
                // a null check on the 'obj' parameter. This null check can be elided entirely if the JIT
                // has other ways of proving that 'obj' cannot be null at the entry to this method.

#nullable disable // we want to incur the NRE in the line below
                ref byte rawDataRef = ref obj.GetRawData();
#nullable restore

                // The field offset isn't from the start of the object header; it's from the start
                // of the first data field in the object (returned by GetRawData above).

                return ref Unsafe.As<byte, TField>(ref Unsafe.AddByteOffset(ref rawDataRef, fieldOffset));
            }
            else
            {
                // Perform our arithmetic with 'byte' instead of reinterpret_casting straight from
                // TObject to TField, otherwise a debugger stepping through this code could exhibit odd
                // behavior since it might misinterpret the actual type of the underlying refs.

                ref byte rawDataRef = ref Unsafe.As<TObject, byte>(ref Unsafe.AsRef(in obj));

                // For value types, we only dereference the 'in' parameter to check that it's not null,
                // but we otherwise don't care about the data. This check can be elided entirely if the
                // JIT has other ways of proving that 'obj' cannot be a null ref at the entry to this method.

                if (rawDataRef == default)
                {
                    // intentionally left blank - we only care about forcing the null check
                }

                return ref Unsafe.As<byte, TField>(ref Unsafe.AddByteOffset(ref rawDataRef, fieldOffset));
            }
        }
    }
}
