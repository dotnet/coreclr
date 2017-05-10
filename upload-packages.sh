#!/usr/bin/env bash

usage()
{
    echo "Upload the NuGet packages to the specified nuget or myget server."
    echo "For uploading to nuget or myget server the following properties are required."
    echo "   -FeedURL=\"NuGet or MyGet feed URL\""
    echo "   -SymbolURL=\"NuGet or MyGet symbol server URL\""
    echo "   -APIKey=\"NuGet or MyGet feed API key\""
    echo "Configuration can be Release, Checked, or Debug"
    echo "Architecture can be x64, x86, arm, armel, or arm64"
    exit 1
}

for arg in "$@"
do
    case $arg in
    -FeedURL=*)
        __FeedURL=${arg#*=}
        ;;
    -SymbolURL=*)
        __SymbolURL=${arg#*=}
        ;;
    -APIKey=*)
        __APIKey=${arg#*=}
        ;;
    Release)
        __BuildConfig=Release
        ;;
    Debug)
        __BuildConfig=Debug
        ;;
    x64)
        __BuildArch=x64
        ;;
    x86)
        __BuildArch=x86
        ;;
    arm)
        __BuildArch=arm
        ;;
    armel)
        __BuildArch=armel
        ;;
    arm64)
        __BuildArch=arm64
        ;;
    *)
        usage
        exit 1
        ;;
    esac
done

CLI_NAME="dotnet-dev-ubuntu-x64.latest.tar.gz"
CLI_URL="https://dotnetcli.blob.core.windows.net/dotnet/Sdk/master/"${CLI_NAME}
CLI_DIR="dotnet-dev"

if [ ! -d ${CLI_DIR} ]; then
    if [ ! -f ${CLI_NAME} ]; then
        wget "${CLI_URL}"
        if [[ $? != 0 ]]; then
            echo "ERROR: Fail to download dotnet cli"
            exit 1
        fi
    fi

    mkdir -p ${CLI_DIR}
    tar xzf "${CLI_NAME}" -C "${CLI_DIR}"
fi

for NUPKG in $( find bin/Product/Linux.${__BuildArch}.${__BuildConfig}/.nuget/pkg -iname "*.nupkg" ); do
    ${CLI_DIR}/dotnet nuget push "${NUPKG}" -s "${__FeedURL}" -k "${__APIKey}"
done

for NUPKG in $( find bin/Product/Linux.${__BuildArch}.${__BuildConfig}/.nuget/symbolpkg -iname "*.nupkg" ); do
    ${CLI_DIR}/dotnet nuget push "${NUPKG}" -s "${__SymbolURL}" -k "${__APIKey}"
done
