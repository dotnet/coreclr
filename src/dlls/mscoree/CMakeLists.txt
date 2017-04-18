include_directories("../../inc")

if(FEATURE_GDBJIT)
   add_definitions(-DFEATURE_GDBJIT)
endif(FEATURE_GDBJIT)

set(CLR_SOURCES
    mscoree.cpp
    unixinterface.cpp
)

if(WIN32)
list(APPEND CLR_SOURCES
    comcallunmarshal.cpp
    Native.rc
)

set (DEF_SOURCES
  mscorwks_ntdef.src
)
else()
if(FEATURE_STANDALONE_GC_ONLY)
  set (DEF_SOURCES
    mscorwks_unixexports_standalone_gc.src
  )
else()
  set (DEF_SOURCES
    mscorwks_unixexports.src
  )
endif(FEATURE_STANDALONE_GC_ONLY)
endif(WIN32)

convert_to_absolute_path(DEF_SOURCES ${DEF_SOURCES})
convert_to_absolute_path(CLR_SOURCES ${CLR_SOURCES})
add_subdirectory(coreclr)
