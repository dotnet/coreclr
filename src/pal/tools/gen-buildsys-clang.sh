#!/usr/bin/env bash
#
# This file invokes cmake and generates the build system for gcc.
#

if [ $# -lt 4 -o $# -gt 6 ]
then
  echo "Usage..."
  echo "gen-buildsys-clang.sh <path to top level CMakeLists.txt> <ClangMajorVersion> <ClangMinorVersion> <Architecture> [build flavor] [coverage]"
  echo "Specify the path to the top level CMake file - <ProjectK>/src/NDP"
  echo "Specify the clang version to use, split into major and minor version"
  echo "Specify the target architecture." 
  echo "Optionally specify the build configuration (flavor.) Defaults to DEBUG." 
  echo "Optionally specify 'coverage' to enable code coverage build."
  exit 1
fi

check_clang()
{
    if which $1 > /dev/null 2>&1
        then
            result="$(which $1)"
    fi
}

# Set up the environment to be used for building with clang.
check_clang "clang-$2.$3" "CC"
CC=$result

check_clang "clang++-$2.$3" "CXX"
CXX=$result

# CC and CXX may be independently named.
# eg. clang-x.x does not imply clang++ to be clang++x.x
if [ "$CC" == "" ]
    then
        check_clang "clang$2$3"
        CC=$result
fi

if [ "$CXX" == "" ]
    then
        check_clang "clang++$2$3"
        CXX=$result
fi

if [ "$CC" == "" ]
    then
        check_clang "clang"
        CC=$result
fi

if [ "$CXX" == "" ]
    then
        check_clang "clang++"
        CXX=$result
fi

if [[ "$CC" == "" || "$CXX" == "" ]]
    then
        echo "Unable to find Clang Compiler"
        exit 1
fi

build_arch="$4"
buildtype=DEBUG
code_coverage=OFF
build_tests=OFF

for i in "${@:5}"; do
    upperI="$(echo $i | awk '{print toupper($0)}')"
    case $upperI in
      # Possible build types are DEBUG, RELEASE, RELWITHDEBINFO, MINSIZEREL.
      DEBUG | RELEASE | RELWITHDEBINFO | MINSIZEREL)
      buildtype=$upperI
      ;;
      COVERAGE)
      echo "Code coverage is turned on for this build."
      code_coverage=ON
      ;;
      INCLUDE_TESTS)
      echo "Including tests directory in build."
      build_tests=ON
      ;;
      *)
      echo "Ignoring unknown arg '$i'"
    esac
done

OS=`uname`

# Locate llvm
# This can be a little complicated, because the common use-case of Ubuntu with
# llvm-3.5 installed uses a rather unusual llvm installation with the version
# number postfixed (i.e. llvm-ar-3.5), so we check for that first.
# On FreeBSD the version number is appended without point and dash (i.e.
# llvm-ar35).
# Additionally, OSX doesn't use the llvm- prefix.
if [ $OS = "Linux" -o $OS = "FreeBSD" -o $OS = "OpenBSD" -o $OS = "NetBSD" ]; then
  llvm_prefix="llvm-"
elif [ $OS = "Darwin" ]; then
  llvm_prefix=""
else
  echo "Unable to determine build platform"
  exit 1
fi

desired_llvm_major_version=$2
desired_llvm_minor_version=$3
if [ $OS = "FreeBSD" ]; then
    desired_llvm_version="$desired_llvm_major_version$desired_llvm_minor_version"
elif [ $OS = "OpenBSD" ]; then
    desired_llvm_version=""
elif [ $OS = "NetBSD" ]; then
    desired_llvm_version=""
else
  desired_llvm_version="-$desired_llvm_major_version.$desired_llvm_minor_version"
fi
locate_llvm_exec() {
  if which "$llvm_prefix$1$desired_llvm_version" > /dev/null 2>&1
  then
    echo "$(which $llvm_prefix$1$desired_llvm_version)"
  elif which "$llvm_prefix$1" > /dev/null 2>&1
  then
    echo "$(which $llvm_prefix$1)"
  else
    exit 1
  fi
}

llvm_ar="$(locate_llvm_exec ar)"
[[ $? -eq 0 ]] || { echo "Unable to locate llvm-ar"; exit 1; }
llvm_link="$(locate_llvm_exec link)"
[[ $? -eq 0 ]] || { echo "Unable to locate llvm-link"; exit 1; }
llvm_nm="$(locate_llvm_exec nm)"
[[ $? -eq 0 ]] || { echo "Unable to locate llvm-nm"; exit 1; }
if [ $OS = "Linux" -o $OS = "FreeBSD" -o $OS = "OpenBSD" -o $OS = "NetBSD" ]; then
  llvm_objdump="$(locate_llvm_exec objdump)"
  [[ $? -eq 0 ]] || { echo "Unable to locate llvm-objdump"; exit 1; }
fi

cmake_extra_defines=
if [[ -n "$LLDB_LIB_DIR" ]]; then
    cmake_extra_defines="$cmake_extra_defines -DWITH_LLDB_LIBS=$LLDB_LIB_DIR"
fi
if [[ -n "$LLDB_INCLUDE_DIR" ]]; then
    cmake_extra_defines="$cmake_extra_defines -DWITH_LLDB_INCLUDES=$LLDB_INCLUDE_DIR"
fi
if [[ -n "$CROSSCOMPILE" ]]; then
    if ! [[ -n "$ROOTFS_DIR" ]]; then
        echo "ROOTFS_DIR not set for crosscompile"
        exit 1
    fi
    cmake_extra_defines="$cmake_extra_defines -C $1/cross/$build_arch/tryrun.cmake"
    cmake_extra_defines="$cmake_extra_defines -DCMAKE_TOOLCHAIN_FILE=$1/cross/$build_arch/toolchain.cmake"
fi

echo "cmake -DCMAKE_USER_MAKE_RULES_OVERRIDE=$1/src/pal/tools/clang-compiler-override.txt -DCMAKE_AR=$llvm_ar -DCMAKE_C_COMPILER=$CC -DCMAKE_CXX_COMPILER=$CXX -DCMAKE_LINKER=$llvm_link -DCMAKE_NM=$llvm_nm -DCMAKE-OBJDUMP=$llvm_objdump -DCMAKE_BUILD_TYPE=$buildtype -DCMAKE_ENABLE_CODE_COVERAGE=$code_coverage $cmake_extra_defines $1"

cmake \
  "-DCMAKE_USER_MAKE_RULES_OVERRIDE=$1/src/pal/tools/clang-compiler-override.txt" \
  "-DCMAKE_AR=$llvm_ar" \
  "-DCMAKE_C_COMPILER=$CC" \
  "-DCMAKE_CXX_COMPILER=$CXX" \
  "-DCMAKE_LINKER=$llvm_link" \
  "-DCMAKE_NM=$llvm_nm" \
  "-DCMAKE_OBJDUMP=$llvm_objdump" \
  "-DCMAKE_BUILD_TYPE=$buildtype" \
  "-DCMAKE_ENABLE_CODE_COVERAGE=$code_coverage" \
  "-DCLR_CMAKE_BUILD_TESTS=$build_tests" \
  $cmake_extra_defines \
  "$1"
