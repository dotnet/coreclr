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
    // The native representation of the field can be treated as an integer.
    NATIVE_FIELD_SUBCATEGORY_INTEGER = 1 << 12,
    // The field is illegal to marshal.
    NATIVE_FIELD_CATEGORY_ILLEGAL = 1 << 14,
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
};
