
add_library_clr(mscorlib_crossgen ../mscorlib.cpp)
add_dependencies(mscorlib_crossgen eventing_headers)
target_compile_definitions(mscorlib_crossgen
    PRIVATE
        EnC_SUPPORTED
        FEATURE_EVENT_TRACE
        FEATURE_MULTICOREJIT
        FEATURE_VERSIONING_LOG
        CROSSGEN_MSCORLIB)

set_target_properties(mscorlib_crossgen PROPERTIES CROSSGEN_COMPONENT TRUE)
