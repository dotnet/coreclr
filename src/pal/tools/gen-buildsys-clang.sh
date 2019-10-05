#!/usr/bin/env bash
#
# This file invokes cmake and generates the build system for Clang.
#

if [ $# -lt 5 ]
then
  echo "Usage..."
  echo "gen-buildsys-clang.sh <path to top level CMakeLists.txt> <ClangMajorVersion> <ClangMinorVersion> <Architecture> <ScriptDirectory> [build flavor] [coverage] [ninja] [scan-build] [cmakeargs]"
  echo "Specify the path to the top level CMake file - <ProjectK>/src/NDP"
  echo "Specify the clang version to use, split into major and minor version"
  echo "Specify the target architecture."
  echo "Specify the script directory."
  echo "Optionally specify the build configuration (flavor.) Defaults to DEBUG." 
  echo "Optionally specify 'coverage' to enable code coverage build."
  echo "Optionally specify 'scan-build' to enable build with clang static analyzer."
  echo "Target ninja instead of make. ninja must be on the PATH."
  echo "Pass additional arguments to CMake call."
  exit 1
fi

# Set up the environment to be used for building with clang.
if command -v "clang-$2.$3" > /dev/null
    then
        desired_llvm_version="-$2.$3"
elif command -v "clang$2$3" > /dev/null
    then
        desired_llvm_version="$2$3"
elif command -v "clang-$2$3" > /dev/null
    then
        desired_llvm_version="-$2$3"
elif command -v clang > /dev/null
    then
        desired_llvm_version=
else
    echo "Unable to find Clang Compiler"
    exit 1
fi

export CC="$(command -v clang$desired_llvm_version)"
export CXX="$(command -v clang++$desired_llvm_version)"

build_arch="$4"
script_dir="$5"
buildtype=DEBUG
code_coverage=OFF
build_tests=OFF
scan_build=OFF
generator="Unix Makefiles"
__UnprocessedCMakeArgs=""

for i in "${@:6}"; do
    upperI="$(echo $i | awk '{print toupper($0)}')"
    case $upperI in
      # Possible build types are DEBUG, CHECKED, RELEASE, RELWITHDEBINFO, MINSIZEREL.
      DEBUG | CHECKED | RELEASE | RELWITHDEBINFO | MINSIZEREL)
      buildtype=$upperI
      ;;
      COVERAGE)
      echo "Code coverage is turned on for this build."
      code_coverage=ON
      ;;
      NINJA)
      generator=Ninja
      ;;
      SCAN-BUILD)
      echo "Static analysis is turned on for this build."
      scan_build=ON
      ;;
      *)
      __UnprocessedCMakeArgs="${__UnprocessedCMakeArgs}${__UnprocessedCMakeArgs:+ }$i"
    esac
done

OS=`uname`

cmake_extra_defines=
if [ "$CROSSCOMPILE" == "1" ]; then
    if ! [[ -n "$ROOTFS_DIR" ]]; then
        echo "ROOTFS_DIR not set for crosscompile"
        exit 1
    fi
    if [[ -z $CONFIG_DIR ]]; then
        CONFIG_DIR="$1/cross"
    fi
    export TARGET_BUILD_ARCH=$build_arch
    cmake_extra_defines="$cmake_extra_defines -C $CONFIG_DIR/tryrun.cmake"
    cmake_extra_defines="$cmake_extra_defines -DCMAKE_TOOLCHAIN_FILE=$CONFIG_DIR/toolchain.cmake"
    cmake_extra_defines="$cmake_extra_defines -DCLR_UNIX_CROSS_BUILD=1"
fi
if [ $OS == "Linux" ]; then
    linux_id_file="/etc/os-release"
    if [[ -n "$CROSSCOMPILE" ]]; then
        linux_id_file="$ROOTFS_DIR/$linux_id_file"
    fi
    if [[ -e $linux_id_file ]]; then
        source $linux_id_file
        cmake_extra_defines="$cmake_extra_defines -DCLR_CMAKE_LINUX_ID=$ID"
    fi
fi
if [ "$build_arch" == "armel" ]; then
    cmake_extra_defines="$cmake_extra_defines -DARM_SOFTFP=1"
fi

__currentScriptDir="$script_dir"

cmake_command=$(command -v cmake3 || command -v cmake)

if [[ "$scan_build" == "ON" ]]; then
    export CCC_CC=$CC
    export CCC_CXX=$CXX
    export SCAN_BUILD_COMMAND=$(command -v scan-build$desired_llvm_version)
    cmake_command="$SCAN_BUILD_COMMAND $cmake_command"
fi

# Include CMAKE_USER_MAKE_RULES_OVERRIDE as uninitialized since it will hold its value in the CMake cache otherwise can cause issues when branch switching
$cmake_command \
  -G "$generator" \
  "-DCMAKE_BUILD_TYPE=$buildtype" \
  "-DCLR_CMAKE_ENABLE_CODE_COVERAGE=$code_coverage" \
  "-DCMAKE_INSTALL_PREFIX=$__CMakeBinDir" \
  "-DCMAKE_USER_MAKE_RULES_OVERRIDE=" \
  $cmake_extra_defines \
  $__UnprocessedCMakeArgs \
  "$1"
