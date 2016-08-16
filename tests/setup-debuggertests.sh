#!/bin/bash

function print_usage {
    echo ''
    echo 'debuggertests install and deploy script.'
    echo ''
    echo 'Required arguments:'
    echo '  --coreclrBinDir=<path>           : Directory of CoreCLR build'
    echo '  --outputDir=<path>               : Directory of debuggertests will be deployed'
    echo 'Optional arguments:'
    echo '  --mscorlibDir=<path>             : Directory of mscorlib.dll. If not specified, it will be set to coreclrBinDir.'
    echo ''
    echo ''
}

# Argument variables
coreclrBinDir=
outputDir=
mscorlibDir=
for i in "$@"
do
    case $i in
        -h|--help)
            print_usage
            exit $EXIT_CODE_SUCCESS
            ;;
        --coreclrBinDir=*)
            coreclrBinDir=${i#*=}
            ;;
        --outputDir=*)
            outputDir=${i#*=}
            ;;
        --mscorlibDir=*)
            mscorlibDir=${i#*=}
            ;;
        *)
            echo "Unknown switch: $i"
            print_usage
            exit $EXIT_CODE_SUCCESS
            ;;
    esac
done

if [ -z "$coreclrBinDir" ] || [ -z "$outputDir" ]; then
    print_usage
    exit 1
fi

if [ -z "$mscorlibDir" ]; then
    mscorlibDir=$coreclrBinDir
fi

debuggerTestsURL=
OSName=$(uname -s)
case $OSName in
    Darwin)
        debuggerTestsURL=https://dotnetbuilddrops.blob.core.windows.net/debugger-container/OSX.DebuggerTests.tar
        ;;

    Linux)
        if [ ! -e /etc/os-release ]; then
            echo "Cannot determine Linux distribution, using the default debuggertests linux build ."
            debuggerTestsURL=https://dotnetbuilddrops.blob.core.windows.net/debugger-container/Linux.DebuggerTests.tar
        else
            source /etc/os-release
	    if [ "$ID.$VERSION_ID" == "ubuntu.14.04" ] 
	    then
                debuggerTestsURL=https://dotnetbuilddrops.blob.core.windows.net/debugger-container/Linux.DebuggerTests.tar
	    fi
	    	
        fi
        ;;
     *)
        echo "Unsupported OS $OSName detected. Can't download debuggertests for this OS."
        exit 0
        ;;

esac

if [ -z "$debuggerTestsURL" ]
then
    echo "Cannot download debuggertests for this Linux distribution"
    exit 0
fi

installDir=$outputDir/debuggertests
if [ -e "$installDir" ]; then 
    rm -rf $installDir 
fi 

mkdir -p $installDir
debuggertestsZipFilePath=$installDir/debuggertests.tar

which curl > /dev/null 2> /dev/null
echo "Download debuggertests to $debuggertestsZipFilePath"
if [ $? -ne 0 ]; then
    echo "wget -q -O $debuggertestsZipFilePath $debuggerTestsURL"
    wget -q -O $debuggertestsZipFilePath $debuggerTestsURL
else
    echo "curl --retry 10 -sSL --create-dirs -o $debuggertestsZipFilePath $debuggerTestsURL"
    curl --retry 10 -sSL --create-dirs -o $debuggertestsZipFilePath $debuggerTestsURL
fi

echo ""
echo "Deploy $debuggertestsZipFilePath to $installDir"
tar -xvf $debuggertestsZipFilePath -C $installDir 

if [ ! -e "$installDir/Runtests.sh" ]; then 
    echo "The debuggertests package is invalid. Can't deploy it"
    rm -rf $debuggertestsZipFilePath
    exit 1
fi
tr -d '\015' < $installDir/Runtests.sh > $installDir/DebuggerTests.sh
chmod u+x $installDir/DebuggerTests.sh
rm $installDir/Runtests.sh

echo ""
targetRuntimePath=$installDir/Runtimes/Coreclr1
echo "Copy runtime from $coreclrBinDir to $targetRuntimePath"
cp -avR $coreclrBinDir/* $targetRuntimePath
cp -f -v $mscorlibDir/mscorlib.dll $targetRuntimePath
echo "Delete all ni images in $targetRuntimePath"
rm $targetRuntimePath/*.ni.dll
exit 0
