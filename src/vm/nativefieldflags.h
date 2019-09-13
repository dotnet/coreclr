// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

// Describes specific categories/subcategories of native fields.
enum NativeFieldFlags : short
{
    // The field may be blittable. The other subcategories determine if the field is blittable.
    NATIVE_FIELD_SUBCATEGORY_BLITTABLE = 1 << 7,
    // The native representation of the field is a floating point field.
    NATIVE_FIELD_SUBCATEGORY_FLOAT = 1 << 8,
    // The field has a nested MethodTable* (i.e. a field of a struct, class, or array)
    NATIVE_FIELD_SUBCATEGORY_NESTED = 1 << 9,
    // The field has a type that is only relevant in COM and requires adherence to the COM ABI.
    NATIVE_FIELD_SUBCATEGORY_COM_ONLY = 1 << 10,
    // The native representation of the field can be treated as an integer.
    NATIVE_FIELD_SUBCATEGORY_INTEGER = 1 << 12,
    // The native representation of the field has no conditional rules in calling convention ABIs.
    // Many of the subcategories here are designed to enable checking if types are HFA types or can be enregistered on certain ABIs.
    // This subcategory is used for well-known runtime types that aren't specially marshalled in ABIs and fields that cannot be marshalled.
    NATIVE_FIELD_SUBCATEGORY_OTHER = 1 << 14,
    // This field is a blittable floating point field.
    NATIVE_FIELD_CATEGORY_BLITTABLE_FLOAT = NATIVE_FIELD_SUBCATEGORY_FLOAT | NATIVE_FIELD_SUBCATEGORY_BLITTABLE,
    // This field is a non-blittable field with a floating point native representation
    NATIVE_FIELD_CATEGORY_NONBLITTABLE_FLOAT = NATIVE_FIELD_SUBCATEGORY_FLOAT,
    // The field is a type with a nested method table but is never blittable (ex. a class with non-auto layout or an array).
    NATIVE_FIELD_CATEGORY_NONBLITTABLE_NESTED = NATIVE_FIELD_SUBCATEGORY_NESTED,
    // The field is a nested type that may be blittable, such as a value class (C# struct).
    NATIVE_FIELD_CATEGORY_BLITTABLE_NESTED = NATIVE_FIELD_SUBCATEGORY_NESTED | NATIVE_FIELD_SUBCATEGORY_BLITTABLE,
    // The field is a non-blittable field with an integer-like native representation for ABI purposes.
    NATIVE_FIELD_CATEGORY_NONBLITTABLE_INTEGER = NATIVE_FIELD_SUBCATEGORY_INTEGER,
    // The field is a blittable integer type.
    NATIVE_FIELD_CATEGORY_BLITTABLE_INTEGER = NATIVE_FIELD_SUBCATEGORY_INTEGER | NATIVE_FIELD_SUBCATEGORY_BLITTABLE,
    // The field is being marshaled to a COM interface pointer.
    NATIVE_FIELD_CATEGORY_INTERFACE_TYPE = NATIVE_FIELD_SUBCATEGORY_INTEGER | NATIVE_FIELD_SUBCATEGORY_COM_ONLY,
    // The field is a structure that is only valid in COM scenarios.
    NATIVE_FIELD_CATEGORY_COM_STRUCT = NATIVE_FIELD_SUBCATEGORY_COM_ONLY,
    // The field is a well-known type to the runtime but cannot be treated like an integer when marshalling.
    NATIVE_FIELD_CATEGORY_WELL_KNOWN = NATIVE_FIELD_SUBCATEGORY_OTHER,
    // The field is illegal to marshal.
    NATIVE_FIELD_CATEGORY_ILLEGAL = NATIVE_FIELD_SUBCATEGORY_OTHER | 0x1
};
