# Enable for UNIX altjit on Windows - set(CLR_CMAKE_PLATFORM_UNIX_TARGET_AMD64 1)
# Enable for UNIX altjit on Windows - add_definitions(-DCLR_CMAKE_PLATFORM_UNIX=1)

# Disable the following line for UNIX altjit on Windows
set(CMAKE_CXX_STANDARD_LIBRARIES "") # do not link against standard win32 libs i.e. kernel32, uuid, user32, etc.

# Include directory directives

# Include the basic prebuilt headers - required for getting fileversion resource details.
include_directories("${CLR_DIR}/src/pal/prebuilt/inc")

if (CLR_CMAKE_PLATFORM_UNIX)
  include_directories("${CLR_DIR}/src/pal/inc")
  include_directories("${CLR_DIR}/src/pal/inc/rt")
  include_directories("${CLR_DIR}/src/pal/src/safecrt")
endif (CLR_CMAKE_PLATFORM_UNIX)

# Libraries

if (WIN32)

    # Define the CRT lib references that link into Desktop imports
    set(STATIC_MT_CRT_LIB  "libcmt$<$<OR:$<CONFIG:Debug>,$<CONFIG:Checked>>:d>.lib")
    set(STATIC_MT_VCRT_LIB  "libvcruntime$<$<OR:$<CONFIG:Debug>,$<CONFIG:Checked>>:d>.lib")
    set(STATIC_MT_CPP_LIB  "libcpmt$<$<OR:$<CONFIG:Debug>,$<CONFIG:Checked>>:d>.lib")

    # ARM64_TODO: Enable this for Windows Arm64
    if (NOT CLR_CMAKE_PLATFORM_ARCH_ARM64)
      # Define the uCRT lib reference
      set(STATIC_MT_UCRT_LIB  "libucrt$<$<OR:$<CONFIG:Debug>,$<CONFIG:Checked>>:d>.lib")
    endif()

endif(WIN32)

# Definition directives

if (CLR_CMAKE_PLATFORM_UNIX)

  if(CLR_CMAKE_PLATFORM_DARWIN)
    add_definitions(-D_XOPEN_SOURCE)
  endif(CLR_CMAKE_PLATFORM_DARWIN)

  if (CLR_CMAKE_PLATFORM_UNIX_TARGET_AMD64)
    add_definitions(-DUNIX_AMD64_ABI)
  elseif (CLR_CMAKE_PLATFORM_UNIX_TARGET_ARM)
    add_definitions(-DUNIX_ARM_ABI)
  endif()

endif(CLR_CMAKE_PLATFORM_UNIX)

add_definitions(-D_BLD_CLR)
add_definitions(-DDEBUGGING_SUPPORTED)
add_definitions(-DPROFILING_SUPPORTED)

if(WIN32)
  add_definitions(-DWIN32)
  add_definitions(-D_WIN32)
  add_definitions(-DWINVER=0x0602)
  add_definitions(-D_WIN32_WINNT=0x0602)
  add_definitions(-DWIN32_LEAN_AND_MEAN=1)
  if(CLR_CMAKE_PLATFORM_ARCH_AMD64 OR CLR_CMAKE_PLATFORM_ARCH_I386)
    add_definitions(-D_CRT_SECURE_NO_WARNINGS)
    # Only enable edit and continue on windows x86 and x64
    # exclude Linux, arm & arm64
    add_definitions(-DEnC_SUPPORTED)
  endif(CLR_CMAKE_PLATFORM_ARCH_AMD64 OR CLR_CMAKE_PLATFORM_ARCH_I386)  
endif(WIN32)

# Features - please keep them alphabetically sorted

add_definitions(-DFEATURE_APPDOMAIN_RESOURCE_MONITORING)
if(WIN32)
    add_definitions(-DFEATURE_APPX)
endif(WIN32)
if(CLR_CMAKE_PLATFORM_ARCH_AMD64 OR CLR_CMAKE_PLATFORM_ARCH_ARM OR CLR_CMAKE_PLATFORM_ARCH_ARM64)
    add_definitions(-DFEATURE_ARRAYSTUB_AS_IL)
endif()

add_definitions(-DFEATURE_ASYNC_IO)
add_definitions(-DFEATURE_BCL_FORMATTING)
add_definitions(-DFEATURE_COLLECTIBLE_TYPES)

if(WIN32)
    add_definitions(-DFEATURE_CLASSIC_COMINTEROP)
    add_definitions(-DFEATURE_COMINTEROP)
    add_definitions(-DFEATURE_COMINTEROP_APARTMENT_SUPPORT)
    add_definitions(-DFEATURE_COMINTEROP_UNMANAGED_ACTIVATION)
    add_definitions(-DFEATURE_COMINTEROP_WINRT_MANAGED_ACTIVATION)
endif(WIN32)

add_definitions(-DFEATURE_CORECLR)
if (CLR_CMAKE_PLATFORM_UNIX)
  add_definitions(-DFEATURE_COREFX_GLOBALIZATION)
endif(CLR_CMAKE_PLATFORM_UNIX)
add_definitions(-DFEATURE_CORESYSTEM)
add_definitions(-DFEATURE_CORRUPTING_EXCEPTIONS)
if(CLR_CMAKE_PLATFORM_UNIX)
    add_definitions(-DFEATURE_DBGIPC_TRANSPORT_DI)
    add_definitions(-DFEATURE_DBGIPC_TRANSPORT_VM)
endif(CLR_CMAKE_PLATFORM_UNIX)
if(FEATURE_EVENT_TRACE)
    add_definitions(-DFEATURE_EVENT_TRACE=1)
    if(CLR_CMAKE_PLATFORM_UNIX)
        add_definitions(-DFEATURE_EVENTSOURCE_XPLAT=1)
    endif(CLR_CMAKE_PLATFORM_UNIX)
endif(FEATURE_EVENT_TRACE)
add_definitions(-DFEATURE_EXCEPTIONDISPATCHINFO)
add_definitions(-DFEATURE_FRAMEWORK_INTERNAL)
# NetBSD doesn't implement this feature
if(NOT CLR_CMAKE_PLATFORM_UNIX_TARGET_ARM AND NOT CMAKE_SYSTEM_NAME STREQUAL NetBSD)
    add_definitions(-DFEATURE_HIJACK)
endif(NOT CLR_CMAKE_PLATFORM_UNIX_TARGET_ARM AND NOT CMAKE_SYSTEM_NAME STREQUAL NetBSD)
add_definitions(-DFEATURE_HOST_ASSEMBLY_RESOLVER)
add_definitions(-DFEATURE_HOSTED_BINDER)
add_definitions(-DFEATURE_ICASTABLE)
if (CLR_CMAKE_PLATFORM_UNIX OR CLR_CMAKE_PLATFORM_ARCH_ARM64)
  add_definitions(-DFEATURE_IMPLICIT_TLS)
  set(FEATURE_IMPLICIT_TLS 1)
endif(CLR_CMAKE_PLATFORM_UNIX OR CLR_CMAKE_PLATFORM_ARCH_ARM64)
add_definitions(-DFEATURE_ISYM_READER)
add_definitions(-DFEATURE_LEGACYNETCF)
add_definitions(-DFEATURE_LEGACYNETCF_DBG_HOST_CONTROL)
add_definitions(-DFEATURE_LEGACYNETCFFAS)
add_definitions(-DFEATURE_LOADER_OPTIMIZATION)
add_definitions(-DFEATURE_MANAGED_ETW)
add_definitions(-DFEATURE_MANAGED_ETW_CHANNELS)
add_definitions(-DFEATURE_MAIN_CLR_MODULE_USES_CORE_NAME)
add_definitions(-DFEATURE_MERGE_CULTURE_SUPPORT_AND_ENGINE)
if(WIN32)
# Disable the following for UNIX altjit on Windows
add_definitions(-DFEATURE_MERGE_JIT_AND_ENGINE)
endif(WIN32)
add_definitions(-DFEATURE_MULTICOREJIT)
add_definitions(-DFEATURE_NORM_IDNA_ONLY)
if(CLR_CMAKE_PLATFORM_UNIX)
  add_definitions(-DFEATURE_PAL)
  add_definitions(-DFEATURE_PAL_SXS)
  add_definitions(-DFEATURE_PAL_ANSI)
endif(CLR_CMAKE_PLATFORM_UNIX)
if(CLR_CMAKE_PLATFORM_LINUX)
    add_definitions(-DFEATURE_PERFMAP)
endif(CLR_CMAKE_PLATFORM_LINUX)
add_definitions(-DFEATURE_PREJIT)
add_definitions(-DFEATURE_RANDOMIZED_STRING_HASHING)
if(NOT DEFINED CLR_CMAKE_PLATFORM_ARCH_ARM64)
  add_definitions(-DFEATURE_READYTORUN)
  set(FEATURE_READYTORUN 1)
endif(NOT DEFINED CLR_CMAKE_PLATFORM_ARCH_ARM64)

if (WIN32)
  if (CLR_CMAKE_PLATFORM_ARCH_AMD64 OR CLR_CMAKE_PLATFORM_ARCH_I386)
    add_definitions(-DFEATURE_REJIT)
  endif(CLR_CMAKE_PLATFORM_ARCH_AMD64 OR CLR_CMAKE_PLATFORM_ARCH_I386)
endif(WIN32)

add_definitions(-DFEATURE_STANDALONE_SN)
add_definitions(-DFEATURE_STRONGNAME_DELAY_SIGNING_ALLOWED)
add_definitions(-DFEATURE_STRONGNAME_MIGRATION)
if(WIN32)
    add_definitions(-DFEATURE_STRONGNAME_TESTKEY_ALLOWED)
endif(WIN32)
if (CLR_CMAKE_PLATFORM_UNIX OR CLR_CMAKE_PLATFORM_ARCH_ARM64)
    add_definitions(-DFEATURE_STUBS_AS_IL)
endif(CLR_CMAKE_PLATFORM_UNIX OR CLR_CMAKE_PLATFORM_ARCH_ARM64)
add_definitions(-DFEATURE_SVR_GC)
add_definitions(-DFEATURE_SYMDIFF)
add_definitions(-DFEATURE_SYNTHETIC_CULTURES)
if(CLR_CMAKE_PLATFORM_UNIX_TARGET_AMD64)
  add_definitions(-DFEATURE_UNIX_AMD64_STRUCT_PASSING)
  add_definitions(-DFEATURE_UNIX_AMD64_STRUCT_PASSING_ITF)
endif (CLR_CMAKE_PLATFORM_UNIX_TARGET_AMD64)
add_definitions(-DFEATURE_USE_ASM_GC_WRITE_BARRIERS)
add_definitions(-DFEATURE_VERSIONING)
if(WIN32)
    add_definitions(-DFEATURE_VERSIONING_LOG)
endif(WIN32)
add_definitions(-DFEATURE_WIN32_REGISTRY)
add_definitions(-DFEATURE_WINDOWSPHONE)
add_definitions(-DFEATURE_WINMD_RESILIENT)

if(CLR_CMAKE_BUILD_TESTS)
  add_subdirectory(tests)
endif(CLR_CMAKE_BUILD_TESTS)

if (CLR_CMAKE_PLATFORM_ARCH_AMD64)
    set(ARCH_SOURCES_DIR amd64)
elseif (CLR_CMAKE_PLATFORM_ARCH_ARM64)
    set(ARCH_SOURCES_DIR arm64)
elseif (CLR_CMAKE_PLATFORM_ARCH_ARM)
    set(ARCH_SOURCES_DIR arm)
elseif (CLR_CMAKE_PLATFORM_ARCH_I386)
    set(ARCH_SOURCES_DIR i386)
else ()
    clr_unknown_arch()
endif ()

add_definitions(-D_SECURE_SCL=0)
add_definitions(-DUNICODE)
add_definitions(-D_UNICODE)

# Compiler options

if(WIN32)
  add_compile_options(/FIWarningControl.h) # force include of WarningControl.h
  add_compile_options(/Zl) # omit default library name in .OBJ
endif(WIN32)

# Microsoft.Dotnet.BuildTools.Coreclr version
set(BuildToolsVersion "1.0.4-prerelease")
set(BuildToolsDir "${CLR_DIR}/packages/Microsoft.DotNet.BuildTools.CoreCLR/${BuildToolsVersion}")