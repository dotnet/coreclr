#!/usr/bin/env bash

usage()
{
    echo "Builds the NuGet packages from the binaries that were built in the Build product binaries step."
    echo "Usage: build-packages -BuildArch -BuildType"
    echo "BuildArch can be x64, x86, arm, arm64 (default is x64)"
    echo "BuildType can be release, checked, debug (default is debug)"
    echo
    exit 1
}

initDistroRid()
{
    source init-distro-rid.sh

    local passedRootfsDir=""

    # Only pass ROOTFS_DIR if __DoCrossArchBuild is specified.
    if [ -z "${__CrossBuild}" ]; then
        passedRootfsDir=${ROOTFS_DIR}
    fi

    initDistroRidGlobal ${__BuildOS} ${__BuildArch} ${__IsPortableBuild} ${passedRootfsDir}
}

__ProjectRoot="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
__IsPortableBuild=1

# Use uname to determine what the OS is.
OSName=$(uname -s)
case $OSName in
    Linux)
        __BuildOS=Linux
        __HostOS=Linux
        ;;

    Darwin)
        __BuildOS=OSX
        __HostOS=OSX
        ;;

    FreeBSD)
        __BuildOS=FreeBSD
        __HostOS=FreeBSD
        ;;

    OpenBSD)
        __BuildOS=OpenBSD
        __HostOS=OpenBSD
        ;;

    NetBSD)
        __BuildOS=NetBSD
        __HostOS=NetBSD
        ;;

    SunOS)
        __BuildOS=SunOS
        __HostOS=SunOS
        ;;

    *)
        echo "Unsupported OS $OSName detected, configuring as if for Linux"
        __BuildOS=Linux
        __HostOS=Linux
        ;;
esac

buildArgs=
unprocessedBuildArgs=

# TODO: get rid of argument processing entirely once we remove the
# uses of -Arg=Value style in buildpipeline.
while :; do
    if [ $# -le 0 ]; then
        break
    fi

    case "$1" in
        -\?|-h|--help)
            usage
            exit 1
            ;;
        -BuildArch=*)
            __BuildArch=$(echo $1| cut -d'=' -f 2)
            buildArgs="$buildArgs /p:__BuildArch=$__BuildArch"
            ;;
        -BuildType=*)
            __Type=$(echo $1| cut -d'=' -f 2)
            buildArgs="$buildArgs /p:__BuildType=$__Type"
            ;;
        -OfficialBuildId=*)
            __Id=$(echo $1| cut -d'=' -f 2)
            buildArgs="$buildArgs /p:OfficialBuildId=$__Id"
            ;;
        -__DoCrossArchBuild=*)
            __CrossBuild=$(echo $1| cut -d'=' -f 2)
            buildArgs="$buildArgs /p:__DoCrossArchBuild=$__CrossBuild"
            ;;
        -portablebuild=false)
            buildArgs="$buildArgs /p:PortableBuild=false"
            __IsPortableBuild=0
            ;;
        --)
            ;;
        *)
            unprocessedBuildArgs="$unprocessedBuildArgs $1"
    esac
    shift
done

initDistroRid

$__ProjectRoot/dotnet.sh msbuild /nologo /verbosity:minimal /clp:Summary \
                         /p:__BuildOS=$__BuildOS /flp:v=detailed\;Append\;LogFile=build-packages.log \
                         /l:BinClashLogger,Tools/Microsoft.DotNet.Build.Tasks.dll\;LogFile=binclash.log \
                         /p:PortableBuild=true src/.nuget/packages.builds \
                         /p:__DistroRid=$__DistroRid /p:BuildNugetPackage=false \
                         $buildArgs $unprocessedBuildArgs
if [ $? -ne 0 ]
then
    echo "ERROR: An error occurred while building packages; See build-packages.log for more details."
    exit 1
fi

echo "Done building packages."
exit 0
