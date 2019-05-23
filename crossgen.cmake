# Contains the crossgen build specific definitions. Included by the leaf crossgen cmake files.

add_definitions(
    -DCROSSGEN_COMPILE
    -DCROSS_COMPILE
    -DFEATURE_NATIVE_IMAGE_GENERATION
    -DSELF_NO_HOST)

remove_definitions(
    -DFEATURE_CODE_VERSIONING
    -DEnC_SUPPORTED
    -DFEATURE_EVENT_TRACE=1
    -DFEATURE_INTERPRETER
    -DFEATURE_MULTICOREJIT
    -DFEATURE_PERFMAP
    -DFEATURE_REJIT
    -DFEATURE_TIERED_COMPILATION
    -DFEATURE_VERSIONING_LOG
)

if(FEATURE_READYTORUN)
    add_definitions(-DFEATURE_READYTORUN_COMPILER)
endif(FEATURE_READYTORUN)

if(CLR_CMAKE_PLATFORM_LINUX)
    add_definitions(-DFEATURE_PERFMAP)
endif(CLR_CMAKE_PLATFORM_LINUX)
