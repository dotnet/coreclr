enum NativeFieldSubcategory
{
    FLOAT_TYPE = 1 << 10,
    NESTED = 1 << 11,
    COM_TYPE = 1 << 12,
    COM_LIKE = 1 << 13,
    INTEGER = 1 << 14,
    ARRAY = 1 << 15 | 1 << 11
};

enum NativeFieldCategory
{
    R4_TYPE = NativeFieldSubcategory::FLOAT_TYPE,
    R8_TYPE = NativeFieldSubcategory::FLOAT_TYPE | 0x1,
    NESTED_TYPE = NativeFieldSubcategory::NESTED,
    DATE_TYPE = NativeFieldCategory::R8_TYPE | NativeFieldSubcategory::COM_LIKE,
    IN_PLACE_ARRAY = NativeFieldSubcategory::ARRAY,
    SAFE_ARRAY = NativeFieldSubcategory::ARRAY | NativeFieldSubcategory::COM_TYPE,
    INTERFACE_TYPE = NativeFieldSubcategory::COM_TYPE,
    INTEGER_LIKE = NativeFieldSubcategory::INTEGER,
    COM_STRUCT = NativeFieldSubcategory::COM_TYPE | NativeFieldSubcategory::INTEGER,
    OTHER = 0
};
