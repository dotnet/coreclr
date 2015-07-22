#!/usr/bin/env bash

# copy of corefx/build.sh from here

__scriptpath=$(cd "$(dirname "$0")"; pwd -P)
__packageroot=$__scriptpath/packages
__sourceroot=$__scriptpath/src
__nugetpath=$__packageroot/NuGet.exe
__nugetconfig=$__sourceroot/NuGet.Config
__msbuildpackageid="Microsoft.Build.Mono.Debug"
__msbuildpackageversion="14.1.0.0-prerelease"
__msbuildpath=$__packageroot/$__msbuildpackageid.$__msbuildpackageversion/lib/MSBuild.exe
__resgenpackageid="Mono.Tools.ResgenModified"
__resgenpackageversion="1.0.0"
__resgenpath=$__packageroot/$__resgenpackageid.$__resgenpackageversion/tools/resgen.exe

if [ $(uname) == "Linux" ]; then
    __monoroot=/usr
elif [ $(uname) == "FreeBSD" ]; then
    __monoroot=/usr/local
else
    __monoroot=/Library/Frameworks/Mono.framework/Versions/Current
fi

__referenceassemblyroot=$__monoroot/lib/mono/xbuild-frameworks


__monoversion=$(mono --version | grep "version 4.[1-9]")

if [ $? -ne 0 ]; then
    # if built from tarball, mono only identifies itself as 4.0.1
    __monoversion=$(mono --version | egrep "version 4.0.[1-9]+(.[0-9]+)?")
    if [ $? -ne 0 ]; then
        echo "Mono 4.0.1.44 or later is required to build corefx. Please see https://github.com/dotnet/corefx/blob/master/Documentation/building/unix-instructions.md for more details."
        exit 1
    else
        echo "WARNING: Mono 4.0.1.44 or later is required to build corefx. Unable to asses if current version is supported."
    fi
fi

if [ ! -e "$__referenceassemblyroot/.NETPortable" ]; then
    echo "PCL reference assemblies not found. Please see https://github.com/dotnet/corefx/blob/master/Documentation/building/unix-instructions.md for more details."
    exit 1
fi


# Pull NuGet.exe down if we don't have it already
if [ ! -e "$__nugetpath" ]; then
    which curl wget > /dev/null 2> /dev/null
    if [ $? -ne 0 -a $? -ne 1 ]; then
        echo "cURL or wget is required to build corefx. Please see https://github.com/dotnet/corefx/blob/master/Documentation/building/unix-instructions.md for more details."
        exit 1
    fi
    echo "Restoring NuGet.exe..."

    # curl has HTTPS CA trust-issues less often than wget, so lets try that first.
    which curl > /dev/null 2> /dev/null
    if [ $? -ne 0 ]; then
       mkdir -p $__packageroot
       wget -q -O $__nugetpath https://api.nuget.org/downloads/nuget.exe
    else
       curl -sSL --create-dirs -o $__nugetpath https://api.nuget.org/downloads/nuget.exe
    fi

    if [ $? -ne 0 ]; then
        echo "Failed to restore NuGet.exe."
        exit 1
    fi
fi

# Grab the MSBuild package if we don't have it already
if [ ! -e "$__msbuildpath" ]; then
    echo "Restoring MSBuild..."
    mono "$__nugetpath" install $__msbuildpackageid -Version $__msbuildpackageversion -source "https://www.myget.org/F/dotnet-buildtools/" -OutputDirectory "$__packageroot"
    if [ $? -ne 0 ]; then
        echo "Failed to restore MSBuild."
        exit 1
    fi
fi

# end of copy of corefx/build.sh


usage()
{
    echo "Usage: $0 [BuildArch] [BuildType] [clean] [verbose] [clangx.y]"
    echo "BuildArch can be: x64"
    echo "BuildType can be: Debug, Release"
    echo "clean - optional argument to force a clean build."
    echo "verbose - optional argument to enable verbose build output."
    echo "clangx.y - optional argument to build using clang version x.y."

    exit 1
}

setup_dirs()
{
    echo Setting up directories for build
    
    mkdir -p "$__RootBinDir"
    mkdir -p "$__BinDir"
    mkdir -p "$__LogsDir"
    mkdir -p "$__IntermediatesDir"
}

# Performs "clean build" type actions (deleting and remaking directories)

clean()
{
    echo Cleaning previous output for the selected configuration
    rm -rf "$__BinDir"
    rm -rf "$__IntermediatesDir"
	
    rm -rf "$__TestWorkingDir"
    rm -rf "$__TestIntermediatesDir"
	
    rm -rf "$__LogsDir/*_$__BuildOS__$__BuildArch__$__BuildType.*"
}

# Check the system to ensure the right pre-reqs are in place

check_prereqs()
{
    echo "Checking pre-requisites..."
    
    # Check presence of CMake on the path
    hash cmake 2>/dev/null || { echo >&2 "Please install cmake before running this script"; exit 1; }
    
    # Check for clang
    hash clang-$__ClangMajorVersion.$__ClangMinorVersion 2>/dev/null ||  hash clang$__ClangMajorVersion$__ClangMinorVersion 2>/dev/null ||  hash clang 2>/dev/null || { echo >&2 "Please install clang before running this script"; exit 1; }
   
}

build_coreclr()
{
    # All set to commence the build
    
    echo "Commencing build of native components for $__BuildOS.$__BuildArch.$__BuildType"
    cd "$__IntermediatesDir"
    
    # Regenerate the CMake solution
    echo "Invoking cmake with arguments: \"$__ProjectRoot\" $__CMakeArgs"
    "$__ProjectRoot/src/pal/tools/gen-buildsys-clang.sh" "$__ProjectRoot" $__ClangMajorVersion $__ClangMinorVersion $__CMakeArgs
    
    # Check that the makefiles were created.
    
    if [ ! -f "$__IntermediatesDir/Makefile" ]; then
        echo "Failed to generate native component build project!"
        exit 1
    fi

    # Get the number of processors available to the scheduler
    # Other techniques such as `nproc` only get the number of
    # processors available to a single process.
    if [ `uname` = "FreeBSD" ]; then
	NumProc=`sysctl hw.ncpu | awk '{ print $2+1 }'`
    else
	NumProc=$(($(getconf _NPROCESSORS_ONLN)+1))
    fi
    
    # Build CoreCLR
    
    echo "Executing make install -j $NumProc $__UnprocessedBuildArgs"

    make install -j $NumProc $__UnprocessedBuildArgs
    if [ $? != 0 ]; then
        echo "Failed to build coreclr components."
        exit 1
    fi
}

build_mscorlib()
{
    echo "Commencing build of mscorlib for $__BuildOS.$__BuildArch.$__BuildType."

    # GenerateNativeVersionInfo=false is a workaround to avoid call to rc.exe.
    MONO29679=1 ReferenceAssemblyRoot=$__referenceassemblyroot mono $__msbuildpath $__ProjectDir/build.proj $__MSBCleanBuildArgs /nologo /verbosity:minimal /fileloggerparameters:Verbosity=normal\;LogFile="$__MScorlibBuildLog" /p:Platform=$__BuildArch /p:OSGroup=$__BuildOS /p:BuildOS=$__BuildOS /p:UseRoslynCompiler=true /p:GenerateNativeVersionInfo=false /p:ResGenCommand=$__resgenpath $__AdditionalMSBuildArgs

    if [ $? != 0 ]; then
        echo "MScorlib build failed. Refer $__MScorlibBuildLog for details."
        exit 1
    fi
}

crossgen()
{
    echo "Generating native image of mscorlib for $__BuildOS.$__BuildArch.$__BuildType."
    echo "$__BinDir/crossgen $__BinDir/mscorlib.dll &> $__CrossGenMScorlibLog
    $__BinDir/crossgen $__BinDir/mscorlib.dll &> "$__CrossGenMScorlibLog
    if [ $? != 0 ]; then
        echo "CrossGen mscorlib failed. Refer $__CrossGenMScorlibLog for details."
        exit 1
    fi
}

echo "Commencing CoreCLR Repo build"

# Argument types supported by this script:
#
# Build architecture - valid value is: x64.
# Build Type         - valid values are: Debug, Release
#
# Set the default arguments for build

# Obtain the location of the bash script to figure out whether the root of the repo is.
__ProjectRoot="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
__BuildArch=x64
# Use uname to determine what the OS is.
OSName=$(uname -s)
case $OSName in
    Linux)
        __BuildOS=Linux
        ;;

    Darwin)
        __BuildOS=OSX
        ;;

    FreeBSD)
        __BuildOS=FreeBSD
        ;;

    OpenBSD)
        __BuildOS=OpenBSD
        ;;

    NetBSD)
        __BuildOS=NetBSD
        ;;

    *)
        echo "Unsupported OS $OSName detected, configuring as if for Linux"
        __BuildOS=Linux
        ;;
esac
__MSBuildBuildArch=x64
__BuildType=Debug
__CMakeArgs=DEBUG

# Set the various build properties here so that CMake and MSBuild can pick them up
__ProjectDir="$__ProjectRoot"
__SourceDir="$__ProjectDir/src"
__PackagesDir="$__ProjectDir/packages"
__RootBinDir="$__ProjectDir/bin"
__LogsDir="$__RootBinDir/Logs"
__UnprocessedBuildArgs=
__MSBCleanBuildArgs=
__CleanBuild=false
__VerboseBuild=false
__ClangMajorVersion=3
__ClangMinorVersion=5

__MScorlibBuildLog=$__LogsDir/MScorlib_$__BuildOS__$__BuildArch__$__BuildType.log
__CrossGenMScorlibLog=$__LogsDir/CrossgenMScorlib_$__BuildOS__$__BuildArch__$__BuildType.log

for i in "$@"
    do
        lowerI="$(echo $i | awk '{print tolower($0)}')"
        case $lowerI in
        -?|-h|--help)
        usage
        exit 1
        ;;
        x64)
        __BuildArch=x64
        __MSBuildBuildArch=x64
        ;;
        debug)
        __BuildType=Debug
        ;;
        release)
        __BuildType=Release
        __CMakeArgs=RELEASE
        ;;
        clean)
        __CleanBuild=1
        ;;
        verbose)
        __VerboseBuild=1
        ;;
        clang3.5)
        __ClangMajorVersion=3
        __ClangMinorVersion=5
        ;;
        clang3.6)
        __ClangMajorVersion=3
        __ClangMinorVersion=6
        ;;
        clang3.7)
        __ClangMajorVersion=3
        __ClangMinorVersion=7
        ;;
        *)
        __UnprocessedBuildArgs="$__UnprocessedBuildArgs $i"
    esac
done

# Set the remaining variables based upon the determined build configuration
__BinDir="$__RootBinDir/Product/$__BuildOS.$__BuildArch.$__BuildType"
__PackagesBinDir="$__BinDir/.nuget"
__ToolsDir="$__RootBinDir/tools"
__TestWorkingDir="$__RootBinDir/tests/$__BuildOS.$__BuildArch.$__BuildType"
__IntermediatesDir="$__RootBinDir/obj/$__BuildOS.$__BuildArch.$__BuildType"
__TestIntermediatesDir="$__RootBinDir/tests/obj/$__BuildOS.$__BuildArch.$__BuildType"

# Specify path to be set for CMAKE_INSTALL_PREFIX.
# This is where all built CoreClr libraries will copied to.
export __CMakeBinDir="$__BinDir"

# Configure environment if we are doing a clean build.
if [ $__CleanBuild == 1 ]; then
    clean
fi

# Configure environment if we are doing a verbose build
if [ $__VerboseBuild == 1 ]; then
	export VERBOSE=1
fi

# Make the directories necessary for build if they don't exist

setup_dirs

# Check prereqs.

check_prereqs

# Build the coreclr (native) components.

build_coreclr

# Build mscorlib.dll.

build_mscorlib

# Build native parts of mscorlib

crossgen

# Build complete

echo "Repo successfully built."
echo "Product binaries are available at $__BinDir"
exit 0
