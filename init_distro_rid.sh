#!/usr/bin/env bash

# initNonPortableDistroRid
#
# Input:
#   isCrossBuild? (nullable vararg:int)
#
# Return:
#   None
#
# Notes:
#
# initNonPortableDistroRid will attempt to initialize a non portable rid. These
# rids are specific to distros need to build the product/package and consume
# them on the same platform.
#
# Below is the list of current non-portable platforms.
#
# Builds from the following *must* be non-portable:
#
#   |    OS     |           Expected RID            |
#   -------------------------------------------------
#   |   rhel6   |           rhel.6-x64              |
#   |  alpine*  |        linux-musl-(arch)          |
#   |  freeBSD  |        freebsd.(version)-x64      |
#
# It is important to note that the function does not return, but it will set
# __DistroRid if there is a non-portable distro rid to be used.
#
initNonPortableDistroRid()
{
    # Make sure out parameter is cleared.
    export __DistroRid=

    local nonPortableBuildID=""
    local isCrossBuild=0
    local crossBuildLocation=""

    # If there is not an optional argument passed then this will be treated
    # as if there is no crossbuild.
    if [ -z "$1" ]; then
        isCrossBuild=0
    else
        isCrossBuild=$1

        if (( isCrossBuild != 1)) && (( isCrossBuild != 0 )); then
            echo "Type error with argument passed to initNonPortableDistroRid."
            exit 1
        fi
    fi

    if (( ${isCrossBuild} == 1 )); then
        crossBuildLocation=${ROOTFS_DIR}
    fi

    if [ "$__BuildOS" = "Linux" ]; then
        if [ -e "${crossBuildLocation}/etc/redhat-release" ]; then
            local redhatRelease=$(<${crossBuildLocation}/etc/redhat-release)

            if [[ "${redhatRelease}" == "CentOS release 6."* ]] || [[ "$redhatRelease" == "Red Hat Enterprise Linux Server release 6."* ]]; then
                nonPortableBuildID="rhel.6-${__BuildArch}"
            fi

        elif [ -e /etc/os-release ]; then
            source "${crossBuildLocation}/etc/os-release"
            if [ "${ID}" = "rhel" ]; then
                # RHEL should have been caught by the /etc/redhat-release
                echo "Error, please verify that your install of RedHat includes"
                echo "/etd/redhat-release"
                exit 1
            fi

            if [ "${ID}" = "alpine" ]; then
                nonPortableBuildID="linux-musl-${__BuildArch}"
            fi

        elif [ -e $ROOTFS_DIR/android_platform ]; then
            source $ROOTFS_DIR/android_platform
            nonPortableBuildID="$RID"
        fi
    fi

    if [ "$__BuildOS" = "FreeBSD" ]; then
        __freebsd_version=`sysctl -n kern.osrelease | cut -f1 -d'.'`
        nonPortableBuildID="freebsd.$__freebsd_version-$__Arch"
    fi

    if [ "${nonPortableBuildID}" != "" ]; then
        export __DistroRid=${nonPortableBuildID}
    fi
}


# initDistroRidGlobal
#
# Input:
#   os (str)
#   arch (str)
#   ROOTFS_DIR? (nullable vararg:string)
#
# Return:
#   None
#
# Notes:
#
# The following global state is modified:
#
#   __BuildOS
#   __BuildArch
#   ROOTFS_DIR
#
# The following out parameters are returned
#
#   __DistroRid
#
initDistroRidGlobal()
{
    # __DistroRid must be set at the end of the function.
    # Previously we would create a variable __HostDistroRid and/or __DistroRid.
    #
    # __HostDistroRid was used in the case of a non-portable build, it has been
    # deprecated. Now only __DistroRid is supported. It will be used for both
    # portable and non-portable rids and will be used in build-packages.sh

    export __BuildOS=$1
    export __BuildArch=$2
    export ROOTFS_DIR=$3

    # Setup whether this is a crossbuild. We can find this out if ROOTFS_DIR
    # is set. 
    local isCrossBuild=0

    if [ -z "${ROOTFS_DIR}" ]; then
        isCrossBuild=0
    else
        # We may have a cross build. Check for the existance of the ROOTFS_DIR
        if [ -e ${ROOTFS_DIR} ]; then
            isCrossBuild=1
        else
            echo "Error ROOTFS_DIR has been passed, but the location is not valid."
            exit 1
        fi
    fi

    if (( ${isCrossBuild} == 1 )); then
        initNonPortableDistroRid isCrossBuild
    else
        initNonPortableDistroRid
    fi

    if [ -z "${__DistroRid}" ]; then
        # The non-portable build rid was not set. Set the portable rid.

        if [ "$__BuildOS" = "OSX" ]; then
            __PortableBuild=1
        fi

        if (( ${__PortableBuild} != 1 )); then
            echo "Portable build has not been set; however, there was no valid"
            echo "non-portable build RID found."
            exit 1
        fi

        local distroRid=""

        if [ "$__BuildOS" = "Linux" ]; then
            distroRid="linux-$__BuildArch"
        elif [ "$__BuildOS" = "OSX" ]; then
            distroRid="osx-$__BuildArch"
        elif [ "$__BuildOS" = "FreeBSD" ]; then
            distroRid="freebsd-$__BuildArch"
        fi

        export __DistroRid=${distroRid}
    fi

    if [ -z "__DistroRid" ]; then
        echo "DistroRid is not set. This is almost certainly an error"

        exit 1
    else
        echo "__DistroRid: ${__DistroRid}"
    fi
}