if(CLR_CMAKE_TARGET_TIZEN_LINUX)
  set(FEATURE_EVENT_TRACE 0)
endif()

if(NOT DEFINED FEATURE_EVENT_TRACE)
  if (WIN32)
    set(FEATURE_EVENT_TRACE 1)
  endif()

  if(CLR_CMAKE_PLATFORM_LINUX)
    if(CLR_CMAKE_TARGET_ARCH_AMD64)
      set(FEATURE_EVENT_TRACE 1)
    elseif(CLR_CMAKE_TARGET_ARCH_ARM)
      set(FEATURE_EVENT_TRACE 1)
    endif()
  endif(CLR_CMAKE_PLATFORM_LINUX)
endif(NOT DEFINED FEATURE_EVENT_TRACE)

if(NOT DEFINED FEATURE_DBGIPC)
  if(CLR_CMAKE_PLATFORM_UNIX AND (NOT CLR_CMAKE_PLATFORM_ANDROID))
    set(FEATURE_DBGIPC 1)
  endif()
endif(NOT DEFINED FEATURE_DBGIPC)

if(NOT DEFINED FEATURE_INTERPRETER)
  set(FEATURE_INTERPRETER 0)
endif(NOT DEFINED FEATURE_INTERPRETER)
