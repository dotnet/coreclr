if(CLR_CMAKE_TARGET_TIZEN_LINUX)
  set(FEATURE_GDBJIT_LANGID_CS 1)
endif()

if(FEATURE_STANDALONE_GC_ONLY)
  set(FEATURE_EVENT_TRACE 0)
  set(FEATURE_APPDOMAIN_RESOURCE_MONITORING 0)
endif()

if(NOT DEFINED FEATURE_EVENT_TRACE)
  if (WIN32)
    set(FEATURE_EVENT_TRACE 1)
  endif()

  if(CLR_CMAKE_PLATFORM_LINUX)
    if(CLR_CMAKE_TARGET_TIZEN_LINUX)
      set(FEATURE_EVENT_TRACE 1)
    elseif(CLR_CMAKE_TARGET_ARCH_AMD64)
      set(FEATURE_EVENT_TRACE 1)
    elseif(CLR_CMAKE_TARGET_ARCH_ARM)
      set(FEATURE_EVENT_TRACE 1)
    elseif(CLR_CMAKE_TARGET_ARCH_ARM64)
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
if(NOT WIN32)
  if(NOT DEFINED FEATURE_NI_BIND_FALLBACK)
    if(NOT CLR_CMAKE_TARGET_ARCH_AMD64 AND NOT CLR_CMAKE_TARGET_ARCH_ARM64)
      set(FEATURE_NI_BIND_FALLBACK 1)
    endif()
  endif(NOT DEFINED FEATURE_NI_BIND_FALLBACK)
endif(NOT WIN32)

if(NOT DEFINED FEATURE_APPDOMAIN_RESOURCE_MONITORING)
  set(FEATURE_APPDOMAIN_RESOURCE_MONITORING 1)
endif(NOT DEFINED FEATURE_APPDOMAIN_RESOURCE_MONITORING)

