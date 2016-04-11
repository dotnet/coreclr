# Require at least version 2.8.12 of CMake
cmake_minimum_required(VERSION 2.8.12)

if(CMAKE_VERSION VERSION_EQUAL 3.0 OR CMAKE_VERSION VERSION_GREATER 3.0)
    cmake_policy(SET CMP0042 NEW)
endif()

# Set the project name
project(CoreCLR)

# Include cmake functions
include(functions.cmake)

# Set commonly used directory names
set(CLR_DIR ${CMAKE_CURRENT_SOURCE_DIR})
set(VM_DIR ${CMAKE_CURRENT_SOURCE_DIR}/src/vm)
set(GENERATED_INCLUDE_DIR ${CMAKE_CURRENT_BINARY_DIR}/src/inc)

set(CORECLR_SET_RPATH ON)
if(CORECLR_SET_RPATH)
    # Enable @rpath support for shared libraries.
    set(MACOSX_RPATH ON)
endif(CORECLR_SET_RPATH)

OPTION(CMAKE_ENABLE_CODE_COVERAGE "Enable code coverage" OFF)

# Ensure that python is present
find_program(PYTHON python)
if (PYTHON STREQUAL "PYTHON-NOTFOUND")
    message(FATAL_ERROR "PYTHON not found: Please install Python 2.7.9 or later from https://www.python.org/downloads/")
endif()

# Ensure other tools are present
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
    elseif (CMAKE_SYSTEM_NAME STREQUAL Linux)
      # Ensure that objcopy is present
      if(DEFINED ENV{CROSSCOMPILE})
        if(CMAKE_SYSTEM_PROCESSOR STREQUAL armv7l)
          find_program(OBJCOPY arm-linux-gnueabihf-objcopy)
        elseif(CMAKE_SYSTEM_PROCESSOR STREQUAL aarch64)
          find_program(OBJCOPY aarch64-linux-gnu-objcopy)
        else()
          clr_unknown_arch()
        endif()
      else()
        find_program(OBJCOPY objcopy)
      endif()
      if (OBJCOPY STREQUAL "OBJCOPY-NOTFOUND")
          message(FATAL_ERROR "objcopy not found")
      endif()
    endif ()
endif(WIN32)

#----------------------------------------------------
# Common cmake configuration
#     - used for build of cross target components
#     - used for non-cross target components
#----------------------------------------------------
include(common.cmake)

#--------------------------------------
# Compile Options
#--------------------------------------
include(CompileOptions.cmake)

#-----------------------------------------
# Add Projects
#     - project which require platform header not clr's
#     - do not depend on clr's compile definitions
#-----------------------------------------
if(CLR_CMAKE_PLATFORM_UNIX)
  add_subdirectory(src/corefx)
endif(CLR_CMAKE_PLATFORM_UNIX)

if(CLR_CMAKE_PLATFORM_UNIX)
  add_subdirectory(src/ToolBox/SOS/lldbplugin)
  add_subdirectory(src/pal)
  add_subdirectory(src/coreclr/hosts)
  add_subdirectory(src/ildasm/unixcoreclrloader)
endif(CLR_CMAKE_PLATFORM_UNIX)

# Add this subdir. We install the headers for the jit.
add_subdirectory(src/pal/prebuilt/inc)

add_subdirectory(src/debug/debug-pal)

if(WIN32)
  add_subdirectory(src/gc/sample)
endif()

# Above projects do not build with these compile options
# All of the compiler options are specified in file CompilerOptions.cmake
# Do not add any new options here. They shoul be added in CompilerOptions.cmake
if(WIN32)
  add_compile_options(/FIWarningControl.h) # force include of WarningControl.h
  add_compile_options(/Zl) # omit default library name in .OBJ
endif(WIN32)

#-------------------------------------
# Include directory directives
#-------------------------------------
# Include the basic prebuilt headers - required for getting fileversion resource details.
include_directories("${CLR_DIR}/src/pal/prebuilt/inc")
include_directories("bin/obj")

if (CLR_CMAKE_PLATFORM_UNIX)
  include_directories("${CLR_DIR}/src/pal/inc")
  include_directories("${CLR_DIR}/src/pal/inc/rt")
  include_directories("${CLR_DIR}/src/pal/src/safecrt")
endif (CLR_CMAKE_PLATFORM_UNIX)

#--------------------------------
# Definition directives
#----------------------------------
include(CompileDefinitions.cmake)

# Microsoft.Dotnet.BuildTools.Coreclr version
set(BuildToolsVersion "1.0.4-prerelease")
set(BuildToolsDir "${CLR_DIR}/packages/Microsoft.DotNet.BuildTools.CoreCLR/${BuildToolsVersion}")

#------------------------------
# Add Product Directory
#------------------------------
add_subdirectory(src)

#------------------------------
# Add Test Directory
#------------------------------
if(CLR_CMAKE_BUILD_TESTS)
  # remove some definitions for test build
  remove_definitions(-D_SECURE_SCL=0)
  remove_definitions(-DUNICODE)
  remove_definitions(-D_UNICODE)

  add_subdirectory(tests)
endif(CLR_CMAKE_BUILD_TESTS)