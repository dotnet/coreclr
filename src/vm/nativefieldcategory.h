enum NativeFieldSubcategory
{
    FLOAT_TYPE = 1 << 10,
    NESTED = 1 << 11,
#ifdef FEATURE_COMINTEROP
    COM_TYPE = 1 << 12,
#endif // FEATURE_COMINTEROP
    COM_LIKE = 1 << 13,
    INTEGER = 1 << 14,
    ARRAY = 1 << 15
};

enum NativeFieldCategory
{
    R4_TYPE = NativeFieldSubcategory::FLOAT_TYPE,
    R8_TYPE = NativeFieldSubcategory::FLOAT_TYPE | 0x1,
    NESTED_TYPE = NativeFieldSubcategory::NESTED,
    DATE_TYPE = NativeFieldCategory::R8_TYPE | NativeFieldSubcategory::COM_LIKE,
    IN_PLACE_ARRAY = NativeFieldSubcategory::ARRAY | NativeFieldSubcategory::NESTED,
    INTEGER_LIKE = NativeFieldSubcategory::INTEGER,
#ifdef FEATURE_COMINTEROP
#ifdef FEATURE_CLASSIC_COMINTEROP
    SAFE_ARRAY = NativeFieldSubcategory::COM_TYPE,
#endif // FEATURE_CLASSIC_COMINTEROP
    INTERFACE_TYPE = NativeFieldSubcategory::COM_TYPE | 0x1,
    COM_STRUCT = NativeFieldSubcategory::COM_TYPE | NativeFieldSubcategory::INTEGER,
#endif // FEATURE_COMINTEROP
    OTHER = 0
};
