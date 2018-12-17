#!/usr/bin/env bash

usage()
{
    echo "Publishes the NuGet packages to the specified location."
    echo "For publishing to Azure the following properties are required."
    echo "   -AzureAccount=\"account name\""
    echo "   -AzureToken=\"access token\""
    echo "   -BuildType=\"Configuration\""
    echo "   -BuildArch=\"Architecture\""
    echo "For publishing to Azure, one of the following properties is required."
    echo "   -PublishPackages        Pass this switch to publish product packages" 
    echo "   -PublishSymbols         Pass this switch to publish symbol packages"
    echo "To specify the name of the container to publish into, use the following property:"
    echo "   -Container=\"container name\""
    echo "To specify the OS you're building for, use the following property:"
    echo "   -DistroRiD=\"RID\""	
    echo "Configuration can be Release, Checked, or Debug"
    echo "Architecture can be x64, x86, arm, or arm64"
    exit 1
}

working_tree_root="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"

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
        -AzureAccount=*)
            __Account=$(echo $1| cut -d'=' -f 2)
            buildArgs="$buildArgs /p:CloudDropAccountName=$__Account"
            ;;
        -AzureToken=*)
            __Token=$(echo $1| cut -d'=' -f 2)
            buildArgs="$buildArgs /p:CloudDropAccessToken=$__Token"
            ;;
        -BuildType=*)
            __Type=$(echo $1| cut -d'=' f 2)
            buildArgs="$buildArgs /p:__BuildType=$__Type"
            ;;
        -BuildArch=*)
            __Arch=$(echo $1| cut -d'=' -f 2)
            buildArgs="$buildArgs /p:__BuildArch=$__Arch"
            ;;
        -Container=*)
            __Container=$(echo $1| cut -d'=' -f 2)
            buildArgs="$buildArgs /p:ContainerName=$__Container"
            ;;
        -distroRid=*)
            __Rid=$(echo $1| cut -d'=' -f 2)
            buildArgs="$buildArgs /p:__DistroRid=$__Rid"
            ;;
        -PublishPackages)
            buildArgs="$buildArgs /p:__PublishPackages=true"
            ;;
        -PublishSymbols)
            buildArgs="$buildArgs /p:__PublishSymbols=true"
            ;;
        -PublishTestNativeBins)
            buildArgs="$buildArgs /p:PublishTestNativeBins=true"
            ;;
        --)
            ;;
        *)
            unprocessedBuildArgs="$unprocessedBuildArgs $1"
    esac
    shift
done

$working_tree_root/dotnet.sh msbuild /nologo /verbosity:minimal /clp:Summary /p:__BuildOS=${OSName} ./src/publish.proj /flp:v=detailed\;LogFile=publish-packages.log /clp:v=detailed $buildArgrs $unprocessedBuildArgs
if [ $? -ne 0 ]
then
    echo "ERROR: An error occurred while publishing packages; see $working_tree_root/publish-packages.log for more details. There may have been networking problems, so please try again in a few minutes."
    exit 1
fi

echo "Publish completed successfully."
exit 0