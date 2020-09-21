#!/usr/bin/env bash

# resolve python-version to use
if [ "$PYTHON" == "" ] ; then
    if ! PYTHON=$(command -v python2.7 || command -v python2 || command -v python || command -v python3)
    then
       echo "Unable to locate build-dependency python!" 1>&2
       exit 1
    fi
fi

# validate python-dependency
# useful in case of explicitly set option.
if ! command -v $PYTHON > /dev/null
then
   echo "Unable to locate build-dependency python ($PYTHON)!" 1>&2
   exit 1
fi

export PYTHON

usage()
{
    echo "Usage: sync [-p]"
    echo "Repository syncing script."
    echo "  -p         Restore all NuGet packages for the repository"
    echo "If no option is specified, then \"sync.sh -p\" is implied."
    exit 1
}

working_tree_root="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
unprocessedBuildArgs=

# Parse arguments
if [ $# == 0 ]; then
    buildArgs="-p"
fi

while [[ $# -gt 0 ]]
do
    opt="$1"
    case $opt in
        -h|--help)
        usage
        ;;
        -p)
        buildArgs="-p"
        ;;
        *)
        unprocessedBuildArgs="$unprocessedBuildArgs $1"
    esac
    shift
done

$working_tree_root/run.sh sync $buildArgs $unprocessedBuildArgs
if [ $? -ne 0 ]
then
    echo "ERROR: An error occurred while syncing packages; See $working_tree_root/sync.log for more details. There may have been networking problems, so please try again in a few minutes."
    exit 1
fi

echo "Sync completed successfully."
exit 0
