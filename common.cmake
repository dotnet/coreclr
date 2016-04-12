#----------------------------------------
# Detect and set platform variable names
#     - for non-windows build platform & architecture is detected using inbuilt CMAKE variables
#     - for windows we use the passed in parameter to CMAKE to determine build arch
#----------------------------------------
if(CMAKE_SYSTEM_NAME STREQUAL Linux)
    set(CLR_CMAKE_PLATFORM_UNIX 1) # variant of unix
    # CMAKE_SYSTEM_PROCESSOR returns the value of `uname -p`.
    # For the AMD/Intel 64bit architecure two different strings are common.
    # Linux and Darwin identify it as "x86_64" while FreeBSD uses the
    # "amd64" string. Accept either of the two here.
    if(CMAKE_SYSTEM_PROCESSOR STREQUAL x86_64 OR CMAKE_SYSTEM_PROCESSOR STREQUAL amd64)
        set(CLR_CMAKE_PLATFORM_UNIX_AMD64 1) # amd64 arch and unix OS
    elseif(CMAKE_SYSTEM_PROCESSOR STREQUAL armv7l)
        set(CLR_CMAKE_PLATFORM_UNIX_ARM 1) # arm arch and unix OS
    elseif(CMAKE_SYSTEM_PROCESSOR STREQUAL aarch64)
        set(CLR_CMAKE_PLATFORM_UNIX_ARM64 1) # arm64 arch and unix OS
    else()
        clr_unknown_arch() # unsupported architecture
    endif()
    set(CLR_CMAKE_PLATFORM_LINUX 1)
endif(CMAKE_SYSTEM_NAME STREQUAL Linux)

if(CMAKE_SYSTEM_NAME STREQUAL Darwin)
  set(CLR_CMAKE_PLATFORM_UNIX 1)
  set(CLR_CMAKE_PLATFORM_UNIX_AMD64 1)
  set(CLR_CMAKE_PLATFORM_DARWIN 1)
  if(CMAKE_VERSION VERSION_LESS "3.4.0")
    set(CMAKE_ASM_COMPILE_OBJECT "${CMAKE_C_COMPILER} <FLAGS> <DEFINES> -o <OBJECT> -c <SOURCE>")
  else()
    set(CMAKE_ASM_COMPILE_OBJECT "${CMAKE_C_COMPILER} <FLAGS> <DEFINES> <INCLUDES> -o <OBJECT> -c <SOURCE>")
  endif(CMAKE_VERSION VERSION_LESS "3.4.0")
endif(CMAKE_SYSTEM_NAME STREQUAL Darwin)

if(CMAKE_SYSTEM_NAME STREQUAL FreeBSD)
  set(CLR_CMAKE_PLATFORM_UNIX 1)
  set(CLR_CMAKE_PLATFORM_UNIX_AMD64 1)
  set(CLR_CMAKE_PLATFORM_FREEBSD 1)
endif(CMAKE_SYSTEM_NAME STREQUAL FreeBSD)

if(CMAKE_SYSTEM_NAME STREQUAL OpenBSD)
  set(CLR_CMAKE_PLATFORM_UNIX 1)
  set(CLR_CMAKE_PLATFORM_UNIX_AMD64 1)
  set(CLR_CMAKE_PLATFORM_OPENBSD 1)
endif(CMAKE_SYSTEM_NAME STREQUAL OpenBSD)

if(CMAKE_SYSTEM_NAME STREQUAL NetBSD)
  set(CLR_CMAKE_PLATFORM_UNIX 1)
  set(CLR_CMAKE_PLATFORM_UNIX_AMD64 1)
  set(CLR_CMAKE_PLATFORM_NETBSD 1)
endif(CMAKE_SYSTEM_NAME STREQUAL NetBSD)

if(CMAKE_SYSTEM_NAME STREQUAL SunOS)
  set(CLR_CMAKE_PLATFORM_UNIX 1)
  EXECUTE_PROCESS(
    COMMAND isainfo -n
    OUTPUT_VARIABLE SUNOS_NATIVE_INSTRUCTION_SET
    )
  if(SUNOS_NATIVE_INSTRUCTION_SET MATCHES "amd64")
    set(CLR_CMAKE_PLATFORM_UNIX_AMD64 1)
    set(CMAKE_SYSTEM_PROCESSOR "amd64")
  else()
    clr_unknown_arch()
  endif()
  set(CLR_CMAKE_PLATFORM_SUNOS 1)
endif(CMAKE_SYSTEM_NAME STREQUAL SunOS)

#--------------------------------------------
# This repo builds two set of binaries
# 1. binaries which execute on target arch machine
#        - for such binaries host architecture & target architecture are same
# 2. binaries which execute on host machine but target another architecture
#        - host architecture is different from target architecture
#        - eg. crossgen.exe - runs on x64 machine and generates nis targeting arm64
#-------------------------------------------------------------
# Set HOST architecture variables
if(CLR_CMAKE_PLATFORM_UNIX_ARM)
  # host arch is arm
  set(CLR_CMAKE_PLATFORM_ARCH_ARM 1)
  set(CLR_CMAKE_HOST_ARCH "arm")
elseif(CLR_CMAKE_PLATFORM_UNIX_ARM64)
  # host arch is arm64
  set(CLR_CMAKE_PLATFORM_ARCH_ARM64 1) 
  set(CLR_CMAKE_HOST_ARCH "arm64")
elseif(CLR_CMAKE_PLATFORM_UNIX_AMD64)
  # host arch is amd64
  set(CLR_CMAKE_PLATFORM_ARCH_AMD64 1)
  set(CLR_CMAKE_HOST_ARCH "x64")
elseif(WIN32)
  # CLR_CMAKE_HOST_ARCH is passed in as param to cmake
  if (CLR_CMAKE_HOST_ARCH STREQUAL x64)
    # host arch is amd64
    set(CLR_CMAKE_PLATFORM_ARCH_AMD64 1)
  elseif(CLR_CMAKE_HOST_ARCH STREQUAL x86)
    # host arch is x86
    set(CLR_CMAKE_PLATFORM_ARCH_I386 1)
  elseif(CLR_CMAKE_HOST_ARCH STREQUAL arm64)
    # host arch is arm64
    set(CLR_CMAKE_PLATFORM_ARCH_ARM64 1)
  else()
    clr_unknown_arch()
  endif()
endif()

# Set TARGET architecture variables
# Target arch will be a cmake param (optional) for both windows as well as non-windows build
# if target arch is not specified then host & target are same
if(NOT DEFINED CLR_CMAKE_TARGET_ARCH OR CLR_CMAKE_TARGET_ARCH STREQUAL "" )
  set(CLR_CMAKE_TARGET_ARCH ${CLR_CMAKE_HOST_ARCH})
endif()

# Set target architecture variables
if (CLR_CMAKE_TARGET_ARCH STREQUAL x64)
    set(CLR_CMAKE_TARGET_ARCH_AMD64 1)
  elseif(CLR_CMAKE_TARGET_ARCH STREQUAL x86)
    set(CLR_CMAKE_TARGET_ARCH_I386 1)
  elseif(CLR_CMAKE_TARGET_ARCH STREQUAL arm64)
    set(CLR_CMAKE_TARGET_ARCH_ARM64 1)
  elseif(CLR_CMAKE_TARGET_ARCH STREQUAL arm)
    set(CLR_CMAKE_TARGET_ARCH_ARM 1)
  else()
    clr_unknown_arch()
endif()

# check if host & target arch combination are valid
if(NOT(CLR_CMAKE_TARGET_ARCH STREQUAL CLR_CMAKE_HOST_ARCH))
  if(NOT((CLR_CMAKE_PLATFORM_ARCH_AMD64 AND CLR_CMAKE_TARGET_ARCH_ARM64) OR (CLR_CMAKE_PLATFORM_ARCH_I386 AND CLR_CMAKE_TARGET_ARCH_ARM)))
    message(FATAL_ERROR "Invalid host and target arch combination")
  endif()
endif()

#-----------------------------------------------------
# Initialize Cmake compiler flags and other variables
#-----------------------------------------------------

if (CMAKE_CONFIGURATION_TYPES) # multi-configuration generator?
    set(CMAKE_CONFIGURATION_TYPES "Debug;Checked;Release;RelWithDebInfo" CACHE STRING "" FORCE)
endif (CMAKE_CONFIGURATION_TYPES)

set(CMAKE_C_FLAGS_CHECKED ${CLR_C_FLAGS_CHECKED_INIT} CACHE STRING "Flags used by the compiler during checked builds.")
set(CMAKE_CXX_FLAGS_CHECKED ${CLR_CXX_FLAGS_CHECKED_INIT} CACHE STRING "Flags used by the compiler during checked builds.")
set(CMAKE_EXE_LINKER_FLAGS_CHECKED "")
set(CMAKE_SHARED_LINKER_FLAGS_CHECKED "")

# Disable the following line for UNIX altjit on Windows
set(CMAKE_CXX_STANDARD_LIBRARIES "") # do not link against standard win32 libs i.e. kernel32, uuid, user32, etc.

if (WIN32)
  # For multi-configuration toolset (as Visual Studio)
  # set the different configuration defines.
  foreach (Config DEBUG CHECKED RELEASE RELWITHDEBINFO)
    foreach (Definition IN LISTS CLR_DEFINES_${Config}_INIT)
      set_property(DIRECTORY APPEND PROPERTY COMPILE_DEFINITIONS $<$<CONFIG:${Config}>:${Definition}>)
    endforeach (Definition)
  endforeach (Config)

  if(NOT CLR_CMAKE_PLATFORM_ARCH_ARM64)
    set(CMAKE_SHARED_LINKER_FLAGS "${CMAKE_SHARED_LINKER_FLAGS} /guard:cf")
    set(CMAKE_EXE_LINKER_FLAGS "${CMAKE_EXE_LINKER_FLAGS} /guard:cf")
  endif (NOT CLR_CMAKE_PLATFORM_ARCH_ARM64)

  # Incremental linking with CFG is broken until next VS release.
  # This needs to be appended to the last for each build type to override the default flag.
  set(NO_INCREMENTAL_LINKER_FLAGS "/INCREMENTAL:NO")

  # Linker flags
  #
  # Disable the following line for UNIX altjit on Windows
  set(CMAKE_SHARED_LINKER_FLAGS "${CMAKE_SHARED_LINKER_FLAGS} /MANIFEST:NO") #Do not create Side-by-Side Assembly Manifest
  set(CMAKE_SHARED_LINKER_FLAGS "${CMAKE_SHARED_LINKER_FLAGS} /SUBSYSTEM:WINDOWS,6.00") #windows subsystem
  set(CMAKE_SHARED_LINKER_FLAGS "${CMAKE_SHARED_LINKER_FLAGS} /LARGEADDRESSAWARE") # can handle addresses larger than 2 gigabytes
  set(CMAKE_SHARED_LINKER_FLAGS "${CMAKE_SHARED_LINKER_FLAGS} /RELEASE") #sets the checksum in the header
  set(CMAKE_SHARED_LINKER_FLAGS "${CMAKE_SHARED_LINKER_FLAGS} /NXCOMPAT") #Compatible with Data Execution Prevention
  set(CMAKE_SHARED_LINKER_FLAGS "${CMAKE_SHARED_LINKER_FLAGS} /DYNAMICBASE") #Use address space layout randomization
  set(CMAKE_SHARED_LINKER_FLAGS "${CMAKE_SHARED_LINKER_FLAGS} /DEBUGTYPE:cv,fixup") #debugging format
  set(CMAKE_SHARED_LINKER_FLAGS "${CMAKE_SHARED_LINKER_FLAGS} /PDBCOMPRESS") #shrink pdb size
  set(CMAKE_SHARED_LINKER_FLAGS "${CMAKE_SHARED_LINKER_FLAGS} /DEBUG")
  set(CMAKE_SHARED_LINKER_FLAGS "${CMAKE_SHARED_LINKER_FLAGS} /IGNORE:4197,4013,4254,4070,4221")

  set(CMAKE_STATIC_LINKER_FLAGS "${CMAKE_STATIC_LINKER_FLAGS} /IGNORE:4221")

  set(CMAKE_EXE_LINKER_FLAGS "${CMAKE_EXE_LINKER_FLAGS} /DEBUG /PDBCOMPRESS")
  set(CMAKE_EXE_LINKER_FLAGS "${CMAKE_EXE_LINKER_FLAGS} /STACK:1572864")

  # Debug build specific flags
  set(CMAKE_SHARED_LINKER_FLAGS_DEBUG "/NOVCFEATURE ${NO_INCREMENTAL_LINKER_FLAGS}")
  set(CMAKE_EXE_LINKER_FLAGS_DEBUG "${NO_INCREMENTAL_LINKER_FLAGS}")

  # Checked build specific flags
  set(CMAKE_SHARED_LINKER_FLAGS_CHECKED "${CMAKE_SHARED_LINKER_FLAGS_CHECKED} /OPT:REF /OPT:NOICF /NOVCFEATURE ${NO_INCREMENTAL_LINKER_FLAGS}")
  set(CMAKE_STATIC_LINKER_FLAGS_CHECKED "${CMAKE_STATIC_LINKER_FLAGS_CHECKED}")
  set(CMAKE_EXE_LINKER_FLAGS_CHECKED "${CMAKE_EXE_LINKER_FLAGS_CHECKED} /OPT:REF /OPT:NOICF ${NO_INCREMENTAL_LINKER_FLAGS}")

  # Release build specific flags
  set(CMAKE_SHARED_LINKER_FLAGS_RELEASE "${CMAKE_SHARED_LINKER_FLAGS_RELEASE} /LTCG /OPT:REF /OPT:ICF ${NO_INCREMENTAL_LINKER_FLAGS}")
  set(CMAKE_STATIC_LINKER_FLAGS_RELEASE "${CMAKE_STATIC_LINKER_FLAGS_RELEASE} /LTCG")
  set(CMAKE_EXE_LINKER_FLAGS_RELEASE "${CMAKE_EXE_LINKER_FLAGS_RELEASE} /LTCG /OPT:REF /OPT:ICF ${NO_INCREMENTAL_LINKER_FLAGS}")

  # ReleaseWithDebugInfo build specific flags
  set(CMAKE_SHARED_LINKER_FLAGS_RELWITHDEBINFO "${CMAKE_SHARED_LINKER_FLAGS_RELWITHDEBINFO} /LTCG /OPT:REF /OPT:ICF ${NO_INCREMENTAL_LINKER_FLAGS}")
  set(CMAKE_STATIC_LINKER_FLAGS_RELWITHDEBINFO "${CMAKE_STATIC_LINKER_FLAGS_RELWITHDEBINFO} /LTCG")
  set(CMAKE_EXE_LINKER_FLAGS_RELWITHDEBINFO "${CMAKE_EXE_LINKER_FLAGS_RELWITHDEBINFO} /LTCG /OPT:REF /OPT:ICF ${NO_INCREMENTAL_LINKER_FLAGS}")

  # Temporary until cmake has VS generators for arm64
  if(CLR_CMAKE_PLATFORM_ARCH_ARM64)
    set(CMAKE_SHARED_LINKER_FLAGS "${CMAKE_SHARED_LINKER_FLAGS} /machine:arm64")
    set(CMAKE_STATIC_LINKER_FLAGS "${CMAKE_STATIC_LINKER_FLAGS} /machine:arm64")
    set(CMAKE_EXE_LINKER_FLAGS "${CMAKE_EXE_LINKER_FLAGS} /machine:arm64")
  endif(CLR_CMAKE_PLATFORM_ARCH_ARM64)

elseif (CLR_CMAKE_PLATFORM_UNIX)
  # Set the values to display when interactively configuring CMAKE_BUILD_TYPE
  set_property(CACHE CMAKE_BUILD_TYPE PROPERTY STRINGS "DEBUG;CHECKED;RELEASE;RELWITHDEBINFO")

  # Use uppercase CMAKE_BUILD_TYPE for the string comparisons below
  string(TOUPPER ${CMAKE_BUILD_TYPE} UPPERCASE_CMAKE_BUILD_TYPE)

  # For single-configuration toolset
  # set the different configuration defines.
  if (UPPERCASE_CMAKE_BUILD_TYPE STREQUAL DEBUG)
    # First DEBUG
    set_property(DIRECTORY  PROPERTY COMPILE_DEFINITIONS ${CLR_DEFINES_DEBUG_INIT})
  elseif (UPPERCASE_CMAKE_BUILD_TYPE STREQUAL CHECKED)
    # Then CHECKED
    set_property(DIRECTORY PROPERTY COMPILE_DEFINITIONS ${CLR_DEFINES_CHECKED_INIT})
  elseif (UPPERCASE_CMAKE_BUILD_TYPE STREQUAL RELEASE)
    # Then RELEASE
    set_property(DIRECTORY PROPERTY COMPILE_DEFINITIONS ${CLR_DEFINES_RELEASE_INIT})
  elseif (UPPERCASE_CMAKE_BUILD_TYPE STREQUAL RELWITHDEBINFO)
    # And then RELWITHDEBINFO
    set_property(DIRECTORY APPEND PROPERTY COMPILE_DEFINITIONS ${CLR_DEFINES_RELWITHDEBINFO_INIT})
  else ()
    message(FATAL_ERROR "Unknown build type! Set CMAKE_BUILD_TYPE to DEBUG, CHECKED, RELEASE, or RELWITHDEBINFO!")
  endif ()

  # set the CLANG sanitizer flags for debug build
  if(UPPERCASE_CMAKE_BUILD_TYPE STREQUAL DEBUG OR UPPERCASE_CMAKE_BUILD_TYPE STREQUAL CHECKED)
    # obtain settings from running enablesanitizers.sh
    string(FIND "$ENV{DEBUG_SANITIZERS}" "asan" __ASAN_POS)
    string(FIND "$ENV{DEBUG_SANITIZERS}" "ubsan" __UBSAN_POS)
    if ((${__ASAN_POS} GREATER -1) OR (${__UBSAN_POS} GREATER -1))
      set(CLR_SANITIZE_CXX_FLAGS "${CLR_SANITIZE_CXX_FLAGS} -fsanitize-blacklist=${CMAKE_CURRENT_SOURCE_DIR}/sanitizerblacklist.txt -fsanitize=")
      set(CLR_SANITIZE_LINK_FLAGS "${CLR_SANITIZE_LINK_FLAGS} -fsanitize=")
      if (${__ASAN_POS} GREATER -1)
        set(CLR_SANITIZE_CXX_FLAGS "${CLR_SANITIZE_CXX_FLAGS}address,")
        set(CLR_SANITIZE_LINK_FLAGS "${CLR_SANITIZE_LINK_FLAGS}address,")
        message("Address Sanitizer (asan) enabled")
      endif ()
      if (${__UBSAN_POS} GREATER -1)
        # all sanitizier flags are enabled except alignment (due to heavy use of __unaligned modifier)
        set(CLR_SANITIZE_CXX_FLAGS "${CLR_SANITIZE_CXX_FLAGS}bool,bounds,enum,float-cast-overflow,float-divide-by-zero,function,integer,nonnull-attribute,null,object-size,return,returns-nonnull-attribute,shift,unreachable,vla-bound,vptr")
        set(CLR_SANITIZE_LINK_FLAGS "${CLR_SANITIZE_LINK_FLAGS}undefined")
        message("Undefined Behavior Sanitizer (ubsan) enabled")
      endif ()

      # -fdata-sections -ffunction-sections: each function has own section instead of one per .o file (needed for --gc-sections)
      # -fPIC: enable Position Independent Code normally just for shared libraries but required when linking with address sanitizer
      # -O1: optimization level used instead of -O0 to avoid compile error "invalid operand for inline asm constraint"
      set(CMAKE_CXX_FLAGS_DEBUG "${CMAKE_CXX_FLAGS_DEBUG} ${CLR_SANITIZE_CXX_FLAGS} -fdata-sections -ffunction-sections -fPIC -O1")
      set(CMAKE_CXX_FLAGS_CHECKED "${CMAKE_CXX_FLAGS_CHECKED} ${CLR_SANITIZE_CXX_FLAGS} -fdata-sections -ffunction-sections -fPIC -O1")

      set(CMAKE_EXE_LINKER_FLAGS_DEBUG "${CMAKE_EXE_LINKER_FLAGS_DEBUG} ${CLR_SANITIZE_LINK_FLAGS}")
      set(CMAKE_EXE_LINKER_FLAGS_CHECKED "${CMAKE_EXE_LINKER_FLAGS_CHECKED} ${CLR_SANITIZE_LINK_FLAGS}")

      # -Wl and --gc-sections: drop unused sections\functions (similar to Windows /Gy function-level-linking)
      set(CMAKE_SHARED_LINKER_FLAGS_DEBUG "${CMAKE_SHARED_LINKER_FLAGS_DEBUG} ${CLR_SANITIZE_LINK_FLAGS} -Wl,--gc-sections")
      set(CMAKE_SHARED_LINKER_FLAGS_CHECKED "${CMAKE_SHARED_LINKER_FLAGS_CHECKED} ${CLR_SANITIZE_LINK_FLAGS} -Wl,--gc-sections")
    endif ()
  endif(UPPERCASE_CMAKE_BUILD_TYPE STREQUAL DEBUG OR UPPERCASE_CMAKE_BUILD_TYPE STREQUAL CHECKED)

endif(WIN32)

if(CLR_CMAKE_PLATFORM_LINUX)  
  set(CMAKE_ASM_FLAGS "${CMAKE_ASM_FLAGS} -Wa,--noexecstack")  
endif(CLR_CMAKE_PLATFORM_LINUX)  

#------------------------------------
# Definitions
#-----------------------------------
if (CLR_CMAKE_PLATFORM_ARCH_AMD64)
  add_definitions(-D_AMD64_)
  add_definitions(-D_WIN64)
  add_definitions(-DAMD64)
  add_definitions(-DBIT64=1)
elseif (CLR_CMAKE_PLATFORM_ARCH_I386)
  add_definitions(-D_X86_)
elseif (CLR_CMAKE_PLATFORM_ARCH_ARM)
  add_definitions(-D_ARM_)
  add_definitions(-DARM)
elseif (CLR_CMAKE_PLATFORM_ARCH_ARM64)
  add_definitions(-D_ARM64_)
  add_definitions(-DARM64)
  add_definitions(-D_WIN64)
  add_definitions(-DBIT64=1)
else ()
  clr_unknown_arch()
endif ()

if (CLR_CMAKE_PLATFORM_UNIX)
  if(CLR_CMAKE_PLATFORM_LINUX)
    if(CLR_CMAKE_PLATFORM_UNIX_AMD64)
      message("Detected Linux x86_64")
      add_definitions(-DLINUX64)
    elseif(CLR_CMAKE_PLATFORM_UNIX_ARM)
      message("Detected Linux ARM")
      add_definitions(-DLINUX32)
    elseif(CLR_CMAKE_PLATFORM_UNIX_ARM64)
      message("Detected Linux ARM64")
      add_definitions(-DLINUX64)
    else()
      clr_unknown_arch()
    endif()
  endif(CLR_CMAKE_PLATFORM_LINUX)
endif(CLR_CMAKE_PLATFORM_UNIX)

if (CLR_CMAKE_PLATFORM_UNIX)
  add_definitions(-DPLATFORM_UNIX=1)

  if(CLR_CMAKE_PLATFORM_DARWIN)
    message("Detected OSX x86_64")
  endif(CLR_CMAKE_PLATFORM_DARWIN)

  if(CLR_CMAKE_PLATFORM_FREEBSD)
    message("Detected FreeBSD amd64")
  endif(CLR_CMAKE_PLATFORM_FREEBSD)
endif(CLR_CMAKE_PLATFORM_UNIX)

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

# Architecture specific files folder name
if (CLR_CMAKE_TARGET_ARCH_AMD64)
    set(ARCH_SOURCES_DIR amd64)
elseif (CLR_CMAKE_TARGET_ARCH_ARM64)
    set(ARCH_SOURCES_DIR arm64)
elseif (CLR_CMAKE_TARGET_ARCH_ARM)
    set(ARCH_SOURCES_DIR arm)
elseif (CLR_CMAKE_TARGET_ARCH_I386)
    set(ARCH_SOURCES_DIR i386)
else ()
    clr_unknown_arch()
endif ()

# Enable for UNIX altjit on Windows - set(CLR_CMAKE_PLATFORM_UNIX_AMD64 1)
# Enable for UNIX altjit on Windows - add_definitions(-DCLR_CMAKE_PLATFORM_UNIX=1)