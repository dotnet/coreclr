set(CORECLR_SET_RPATH ON)

if(CORECLR_SET_RPATH)
    # Enable @rpath support for shared libraries.
    set(MACOSX_RPATH ON)
endif(CORECLR_SET_RPATH)

if(CMAKE_VERSION VERSION_EQUAL 3.0 OR CMAKE_VERSION VERSION_GREATER 3.0)
    cmake_policy(SET CMP0042 NEW)
endif()

if(CLR_CMAKE_PLATFORM_UNIX_ARM)
    # Because we don't use CMAKE_C_COMPILER/CMAKE_CXX_COMPILER to use clang
    # we have to set the triple by adding a compiler argument
    add_compile_options(-target armv7-linux-gnueabihf)
    add_compile_options(-mthumb)
    add_compile_options(-mfpu=vfpv3)
endif(CLR_CMAKE_PLATFORM_UNIX_ARM)

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


# Ensure that python is present
find_program(PYTHON python)
if (PYTHON STREQUAL "PYTHON-NOTFOUND")
    message(FATAL_ERROR "PYTHON not found: Please install Python 2.7.9 or later from https://www.python.org/downloads/")
endif()

if(WIN32)
    enable_language(ASM_MASM)

    # Ensure that MC is present
    find_program(MC mc)
    if (MC STREQUAL "MC-NOTFOUND")
        message(FATAL_ERROR "MC not found")
    endif()
else()
    enable_language(ASM)

    # Ensure that awk is present
    find_program(AWK awk)
    if (AWK STREQUAL "AWK-NOTFOUND")
        message(FATAL_ERROR "AWK not found")
    endif()
 
    if (CMAKE_SYSTEM_NAME STREQUAL Darwin)

      # Ensure that dsymutil and strip is present
      find_program(DSYMUTIL dsymutil)
      if (DSYMUTIL STREQUAL "DSYMUTIL-NOTFOUND")
          message(FATAL_ERROR "dsymutil not found")
      endif()
      find_program(STRIP strip)
      if (STRIP STREQUAL "STRIP-NOTFOUND")
          message(FATAL_ERROR "strip not found")
      endif()

    else (CMAKE_SYSTEM_NAME STREQUAL Darwin)

      # Ensure that objcopy is present
      find_program(OBJCOPY objcopy)
      if (OBJCOPY STREQUAL "OBJCOPY-NOTFOUND")
          message(FATAL_ERROR "objcopy not found")
      endif()
    endif (CMAKE_SYSTEM_NAME STREQUAL Darwin)
endif(WIN32)

# Build a list of compiler definitions by putting -D in front of each define.
function(get_compile_definitions DefinitionName)
    # Get the current list of definitions
    get_directory_property(COMPILE_DEFINITIONS_LIST COMPILE_DEFINITIONS)

    foreach(DEFINITION IN LISTS COMPILE_DEFINITIONS_LIST)
        if (${DEFINITION} MATCHES "^\\$<\\$<CONFIG:([^>]+)>:([^>]+)>$")
            # The entries that contain generator expressions must have the -D inside of the
            # expression. So we transform e.g. $<$<CONFIG:Debug>:_DEBUG> to $<$<CONFIG:Debug>:-D_DEBUG>
            set(DEFINITION "$<$<CONFIG:${CMAKE_MATCH_1}>:-D${CMAKE_MATCH_2}>")
        else()
            set(DEFINITION -D${DEFINITION})
        endif()
        list(APPEND DEFINITIONS ${DEFINITION})
    endforeach()
    set(${DefinitionName} ${DEFINITIONS} PARENT_SCOPE)
endfunction(get_compile_definitions)

# Build a list of include directories by putting -I in front of each include dir.
function(get_include_directories IncludeDirectories)
    get_directory_property(dirs INCLUDE_DIRECTORIES)
    foreach(dir IN LISTS dirs)
        list(APPEND INC_DIRECTORIES -I${dir})
    endforeach()
    set(${IncludeDirectories} ${INC_DIRECTORIES} PARENT_SCOPE)
endfunction(get_include_directories)

# Set the passed in RetSources variable to the list of sources with added current source directory
# to form absolute paths.
# The parameters after the RetSources are the input files.
function(convert_to_absolute_path RetSources)
    set(Sources ${ARGN})
    foreach(Source IN LISTS Sources)
        list(APPEND AbsolutePathSources ${CMAKE_CURRENT_SOURCE_DIR}/${Source})
    endforeach()
    set(${RetSources} ${AbsolutePathSources} PARENT_SCOPE)
endfunction(convert_to_absolute_path)

#Preprocess exports definition file
function(preprocess_def_file inputFilename outputFilename)
  get_compile_definitions(PREPROCESS_DEFINITIONS)

  add_custom_command(
    OUTPUT ${outputFilename}
    COMMAND ${CMAKE_CXX_COMPILER} /P /EP /TC ${PREPROCESS_DEFINITIONS}  /Fi${outputFilename}  ${inputFilename}
    DEPENDS ${inputFilename}
    COMMENT "Preprocessing ${inputFilename}"
  )

  set_source_files_properties(${outputFilename}
                              PROPERTIES GENERATED TRUE)
endfunction()

function(generate_exports_file inputFilename outputFilename)

  if(CMAKE_SYSTEM_NAME STREQUAL Darwin)
    set(AWK_SCRIPT generateexportedsymbols.awk)
  else()
    set(AWK_SCRIPT generateversionscript.awk)
  endif(CMAKE_SYSTEM_NAME STREQUAL Darwin)

  add_custom_command(
    OUTPUT ${outputFilename}
    COMMAND ${AWK} -f ${CMAKE_SOURCE_DIR}/${AWK_SCRIPT} ${inputFilename} >${outputFilename}
    DEPENDS ${inputFilename} ${CMAKE_SOURCE_DIR}/${AWK_SCRIPT}
    COMMENT "Generating exports file ${outputFilename}"
  )
  set_source_files_properties(${outputFilename}
                              PROPERTIES GENERATED TRUE)
endfunction()

function(add_precompiled_header header cppFile targetSources)
  if(MSVC)
    set(precompiledBinary "${CMAKE_CURRENT_BINARY_DIR}/${CMAKE_CFG_INTDIR}/stdafx.pch")

    set_source_files_properties(${cppFile}
                                PROPERTIES COMPILE_FLAGS "/Yc\"${header}\" /Fp\"${precompiledBinary}\""
                                           OBJECT_OUTPUTS "${precompiledBinary}")
    set_source_files_properties(${${targetSources}}
                                PROPERTIES COMPILE_FLAGS "/Yu\"${header}\" /Fp\"${precompiledBinary}\""
                                           OBJECT_DEPENDS "${precompiledBinary}")
    # Add cppFile to SourcesVar
    set(${targetSources} ${${targetSources}} ${cppFile} PARENT_SCOPE)
  endif(MSVC)
endfunction()

function(strip_symbols targetName outputFilename)
  if(CLR_CMAKE_PLATFORM_UNIX)
    if(UPPERCASE_CMAKE_BUILD_TYPE STREQUAL RELEASE)

      # On the older version of cmake (2.8.12) used on Ubuntu 14.04 the TARGET_FILE
      # generator expression doesn't work correctly returning the wrong path and on
      # the newer cmake versions the LOCATION property isn't supported anymore.
      if(CMAKE_VERSION VERSION_EQUAL 3.0 OR CMAKE_VERSION VERSION_GREATER 3.0)
          set(strip_source_file $<TARGET_FILE:${targetName}>)
      else()
          get_property(strip_source_file TARGET ${targetName} PROPERTY LOCATION)
      endif()

      if(CMAKE_SYSTEM_NAME STREQUAL Darwin)
        set(strip_destination_file ${strip_source_file}.dwarf)

        add_custom_command(
          TARGET ${targetName}
          POST_BUILD
          VERBATIM 
          COMMAND ${DSYMUTIL} --flat --minimize ${strip_source_file}
          COMMAND ${STRIP} -u -r ${strip_source_file}
          COMMENT Stripping symbols from ${strip_source_file} into file ${strip_destination_file}
        )
      else(CMAKE_SYSTEM_NAME STREQUAL Darwin)
        set(strip_destination_file ${strip_source_file}.dbg)

        add_custom_command(
          TARGET ${targetName}
          POST_BUILD
          VERBATIM 
          COMMAND ${OBJCOPY} --only-keep-debug ${strip_source_file} ${strip_destination_file}
          COMMAND ${OBJCOPY} --strip-unneeded ${strip_source_file}
          COMMAND ${OBJCOPY} --add-gnu-debuglink=${strip_destination_file} ${strip_source_file}
          COMMENT Stripping symbols from ${strip_source_file} into file ${strip_destination_file}
        )
      endif(CMAKE_SYSTEM_NAME STREQUAL Darwin)

      set(${outputFilename} ${strip_destination_file} PARENT_SCOPE)
    endif(UPPERCASE_CMAKE_BUILD_TYPE STREQUAL RELEASE)
  endif(CLR_CMAKE_PLATFORM_UNIX)
endfunction()

function(install_clr targetName)
  strip_symbols(${targetName} strip_destination_file)

  # On the older version of cmake (2.8.12) used on Ubuntu 14.04 the TARGET_FILE
  # generator expression doesn't work correctly returning the wrong path and on
  # the newer cmake versions the LOCATION property isn't supported anymore.
  if(CMAKE_VERSION VERSION_EQUAL 3.0 OR CMAKE_VERSION VERSION_GREATER 3.0)
      set(install_source_file $<TARGET_FILE:${targetName}>)
  else()
      get_property(install_source_file TARGET ${targetName} PROPERTY LOCATION)
  endif()

  install(PROGRAMS ${install_source_file} DESTINATION .)
  if(WIN32)
      install(FILES ${CMAKE_CURRENT_BINARY_DIR}/$<CONFIG>/${targetName}.pdb DESTINATION PDB)
  else()
      install(FILES ${strip_destination_file} DESTINATION .)
  endif()
endfunction()

# Includes

if (CMAKE_CONFIGURATION_TYPES) # multi-configuration generator?
    set(CMAKE_CONFIGURATION_TYPES "Debug;Checked;Release;RelWithDebInfo" CACHE STRING "" FORCE)
endif (CMAKE_CONFIGURATION_TYPES)

set(CMAKE_C_FLAGS_CHECKED ${CLR_C_FLAGS_CHECKED_INIT} CACHE STRING "Flags used by the compiler during checked builds.")
set(CMAKE_CXX_FLAGS_CHECKED ${CLR_CXX_FLAGS_CHECKED_INIT} CACHE STRING "Flags used by the compiler during checked builds.")
set(CMAKE_EXE_LINKER_FLAGS_CHECKED "")
set(CMAKE_SHARED_LINKER_FLAGS_CHECKED "")

if (WIN32)
  # For multi-configuration toolset (as Visual Studio)
  # set the different configuration defines.
  foreach (Config DEBUG CHECKED RELEASE RELWITHDEBINFO)
    foreach (Definition IN LISTS CLR_DEFINES_${Config}_INIT)
      set_property(DIRECTORY APPEND PROPERTY COMPILE_DEFINITIONS $<$<CONFIG:${Config}>:${Definition}>)
    endforeach (Definition)
  endforeach (Config)

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

endif(WIN32)

if (CLR_CMAKE_PLATFORM_UNIX)
  add_definitions(-DPLATFORM_UNIX=1)

  if(CLR_CMAKE_PLATFORM_DARWIN)
    message("Detected OSX x86_64")
  endif(CLR_CMAKE_PLATFORM_DARWIN)

  if(CLR_CMAKE_PLATFORM_FREEBSD)
    message("Detected FreeBSD amd64")
  endif(CLR_CMAKE_PLATFORM_FREEBSD)

  # Disable frame pointer optimizations so profilers can get better call stacks
  add_compile_options(-fno-omit-frame-pointer)

  # The -fms-extensions enable the stuff like __if_exists, __declspec(uuid()), etc.
  add_compile_options(-fms-extensions )
  #-fms-compatibility      Enable full Microsoft Visual C++ compatibility
  #-fms-extensions         Accept some non-standard constructs supported by the Microsoft compiler

  if(CLR_CMAKE_PLATFORM_DARWIN)
    # We cannot enable "stack-protector-strong" on OS X due to a bug in clang compiler (current version 7.0.2)
    add_compile_options(-fstack-protector)
  else()
    add_compile_options(-fstack-protector-strong)
  endif(CLR_CMAKE_PLATFORM_DARWIN)

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

  add_definitions(-DDISABLE_CONTRACTS)
  # The -ferror-limit is helpful during the porting, it makes sure the compiler doesn't stop
  # after hitting just about 20 errors.
  add_compile_options(-ferror-limit=4096)

  # All warnings that are not explicitly disabled are reported as errors
  add_compile_options(-Werror)

  # Disabled warnings
  add_compile_options(-Wno-unused-private-field)
  add_compile_options(-Wno-unused-variable)
  # Explicit constructor calls are not supported by clang (this->ClassName::ClassName())
  add_compile_options(-Wno-microsoft)
  # This warning is caused by comparing 'this' to NULL
  add_compile_options(-Wno-tautological-compare)
  # There are constants of type BOOL used in a condition. But BOOL is defined as int
  # and so the compiler thinks that there is a mistake.
  add_compile_options(-Wno-constant-logical-operand)

  add_compile_options(-Wno-unknown-warning-option)

  #These seem to indicate real issues
  add_compile_options(-Wno-invalid-offsetof)
  # The following warning indicates that an attribute __attribute__((__ms_struct__)) was applied
  # to a struct or a class that has virtual members or a base class. In that case, clang
  # may not generate the same object layout as MSVC.
  add_compile_options(-Wno-incompatible-ms-struct)

endif(CLR_CMAKE_PLATFORM_UNIX)

if (WIN32)
  # Compile options for targeting windows

  # The following options are set by the razzle build
  add_compile_options(/TP) # compile all files as C++
  add_compile_options(/d2Zi+) # make optimized builds debugging easier
  add_compile_options(/nologo) # Suppress Startup Banner
  add_compile_options(/W3) # set warning level to 3
  add_compile_options(/WX) # treat warnings as errors
  add_compile_options(/Oi) # enable intrinsics
  add_compile_options(/Oy-) # disable suppressing of the creation of frame pointers on the call stack for quicker function calls
  add_compile_options(/U_MT) # undefine the predefined _MT macro
  add_compile_options(/GF) # enable read-only string pooling
  add_compile_options(/Gm-) # disable minimal rebuild
  add_compile_options(/EHa) # enable C++ EH (w/ SEH exceptions)
  add_compile_options(/Zp8) # pack structs on 8-byte boundary
  add_compile_options(/Gy) # separate functions for linker
  add_compile_options(/Zc:wchar_t-) # C++ language conformance: wchar_t is NOT the native type, but a typedef
  add_compile_options(/Zc:forScope) # C++ language conformance: enforce Standard C++ for scoping rules
  add_compile_options(/GR-) # disable C++ RTTI
  add_compile_options(/FC) # use full pathnames in diagnostics
  add_compile_options(/MP) # Build with Multiple Processes (number of processes equal to the number of processors)
  add_compile_options(/GS) # Buffer Security Check
  add_compile_options(/Zm200) # Specify Precompiled Header Memory Allocation Limit of 150MB
  add_compile_options(/wd4960 /wd4961 /wd4603 /wd4627 /wd4838 /wd4456 /wd4457 /wd4458 /wd4459 /wd4091 /we4640)
  add_compile_options(/Zi) # enable debugging information

  if (CLR_CMAKE_PLATFORM_ARCH_I386)
    add_compile_options(/Gz)
  endif (CLR_CMAKE_PLATFORM_ARCH_I386)

  add_compile_options($<$<OR:$<CONFIG:Release>,$<CONFIG:Relwithdebinfo>>:/GL>)
  add_compile_options($<$<OR:$<OR:$<CONFIG:Release>,$<CONFIG:Relwithdebinfo>>,$<CONFIG:Checked>>:/O1>)

  if (CLR_CMAKE_PLATFORM_ARCH_AMD64)
  # The generator expression in the following command means that the /homeparams option is added only for debug builds
  add_compile_options($<$<CONFIG:Debug>:/homeparams>) # Force parameters passed in registers to be written to the stack
  endif (CLR_CMAKE_PLATFORM_ARCH_AMD64)

  if(NOT CLR_CMAKE_PLATFORM_ARCH_ARM64)
    # enable control-flow-guard support for native components for non-Arm64 builds
    add_compile_options(/guard:cf) 
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

endif (WIN32)

OPTION(CMAKE_ENABLE_CODE_COVERAGE "Enable code coverage" OFF)

if(CMAKE_ENABLE_CODE_COVERAGE)

  if(CLR_CMAKE_PLATFORM_UNIX)
    string(TOUPPER ${CMAKE_BUILD_TYPE} UPPERCASE_CMAKE_BUILD_TYPE)
    if(NOT UPPERCASE_CMAKE_BUILD_TYPE STREQUAL DEBUG)
      message( WARNING "Code coverage results with an optimised (non-Debug) build may be misleading" )
    endif(NOT UPPERCASE_CMAKE_BUILD_TYPE STREQUAL DEBUG)

    add_compile_options(-fprofile-arcs)
    add_compile_options(-ftest-coverage)
    set(CLANG_COVERAGE_LINK_FLAGS  "--coverage")
    set(CMAKE_SHARED_LINKER_FLAGS  "${CMAKE_SHARED_LINKER_FLAGS} ${CLANG_COVERAGE_LINK_FLAGS}")
    set(CMAKE_EXE_LINKER_FLAGS     "${CMAKE_EXE_LINKER_FLAGS} ${CLANG_COVERAGE_LINK_FLAGS}")
  else()
    message(FATAL_ERROR "Code coverage builds not supported on current platform")
  endif(CLR_CMAKE_PLATFORM_UNIX)

endif(CMAKE_ENABLE_CODE_COVERAGE)

# Start of projects that require usage of platform include files

if (WIN32)
  set(FEATURE_EVENT_TRACE 1)
endif()
if(CLR_CMAKE_PLATFORM_LINUX AND CLR_CMAKE_PLATFORM_ARCH_AMD64)
  set(FEATURE_EVENT_TRACE 1)
endif()

# End of projects that require usage of platform include files