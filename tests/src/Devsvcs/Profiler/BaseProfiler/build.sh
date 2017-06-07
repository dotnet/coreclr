#!/bin/sh

[ -z "${CORECLR_PATH:-}" ] && CORECLR_PATH=~/coreclr
[ -z "${PROFILER_TESTS_PATH:-}" ] && PROFILER_TESTS_PATH=~/Profiler/tests
[ -z "${BuildOS:-}"      ] && BuildOS=Linux
[ -z "${BuildArch:-}"    ] && BuildArch=x64
[ -z "${BuildType:-}"    ] && BuildType=Debug
[ -z "${Output:-}"       ] && Output=$PROFILER_TESTS_PATH/CorProfiler.so

printf '  CORECLR_PATH : %s\n' "$CORECLR_PATH"
printf '  BuildOS      : %s\n' "$BuildOS"
printf '  BuildArch    : %s\n' "$BuildArch"
printf '  BuildType    : %s\n' "$BuildType"

printf '  Building %s ... ' "$Output"

CXX_FLAGS="$CXX_FLAGS --no-undefined -Wno-invalid-noreturn -fPIC -fms-extensions -DBIT64 -DPAL_STDCPP_COMPAT -DPLATFORM_UNIX -std=c++11"
INCLUDES="-I $CORECLR_PATH/src/pal/inc/rt -I $CORECLR_PATH/src/pal/prebuilt/inc -I $CORECLR_PATH/src/pal/inc -I $CORECLR_PATH/src/inc -I $CORECLR_PATH/bin/Product/$BuildOS.$BuildArch.$BuildType/inc -I $PROFILER_TESTS_PATH/Common"

clang++ -shared -o $Output $CXX_FLAGS $INCLUDES Common/ClassFactory.cpp Common/CorProfiler.cpp Common/dllmain.cpp Common/ILRewriter.cpp Common/ProfilerCommon.cpp apitests/elt3.cpp

printf 'Done.\n'
