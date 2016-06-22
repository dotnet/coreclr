#!/usr/bin/env python
#
## Licensed to the .NET Foundation under one or more agreements.
## The .NET Foundation licenses this file to you under the MIT license.
## See the LICENSE file in the project root for more information.
#
##
# Title               :arm32_ci_build.py
#
# Script to create a working list file from the test overlay directory. This 
# will be used by smarty to run tests.
#
################################################################################

import os
import subprocess
import sys

################################################################################
# Helper Functions
################################################################################

def subprocess_run(command):
    print " ".join(command)
    proc = subprocess.Popen(command, stdout=subprocess.PIPE)
    proc.communicate()

def source(filename, args):
    # Emulate the source command from bash, change the parents env to what the
    # child's env is after the source command

    command = ["/bin/bash", "-c", "source %s %s && env" % (filename, " ".join(args))]
    print " ".join(command)
    proc = subprocess.Popen(command, stdout=subprocess.PIPE)

    for line in proc.stdout:
        (key, _, value) = line.partition("=")
        os.environ[key] = value

    proc.communicate()

################################################################################
# Main
################################################################################

if __name__ == "__main__":
    print "Starting arm32_ci"
    print "- - - - - - - - - - - - - - - - - - - - - - - - - - - -"
    print

    if len(sys.argv) < 4:
        print "Error, incorrect number of arguments."
        print "Ex usage: python %s <configuration> <arm32_emulator_path> <armrootfs_mountpath>" % (__file__)
        exit(1)

    configuration = sys.argv[1]
    arm_emulator_path = sys.argv[2]
    rootfs_mount_path = sys.argv[3]

    # /proc/mount will not have trailing /
    if rootfs_mount_path[-1] == "/":
        rootfs_mount_path = rootfs_mount_path[:-1]

    if not os.path.isdir(arm_emulator_path):
        print "Error %s path passed is not a valid directory." % (var_name)
        exit(1)

    # Create the mount path if not present already
    if not os.path.isdir(rootfs_mount_path):
        os.mkdir(rootfs_mount_path)

    contents = None
    with open("/proc/mounts") as file_handle:
        contents = file_handle.read()

    # If the emulator is already mounted, unmount it.
    if rootfs_mount_path in contents:
        command = ["sudo", "umount", rootfs_mount_path]

        subprocess_run(command)

    # Mount the emulator
    command = ["sudo", "mount", os.path.join(arm_emulator_path, "platform", "rootfs-t30.ext4"), rootfs_mount_path]
    subprocess_run(command)

    source(os.path.join(rootfs_mount_path, "dotnet", "setenv", "setenv_incpath.sh"), [rootfs_mount_path])

    command = ["git", "am", "<", os.path.join(rootfs_mount_path, "dotnet", "setenv", "coreclr_cross.patch")]
    subprocess_run(command)

    os.environ["ROOTFS_DIR"] = rootfs_mount_path
    os.environ["CPLUS_INCLUDE_PATH"] = os.environ["LINUX_ARM_INCPATH"]
    os.environ["CXXFLAGS"] = os.environ["LINUX_ARM_CXXFLAGS"]

    # Build
    build_command = [os.path.join(os.getcwd(), "build.sh"), "arm-softfp", "clean", "cross", "verbose", "skipmscorlib", "clang3.5", "%s" % (configuration)]

    print " ".join(build_command)
    proc = subprocess.Popen(build_command)
    proc.communicate()

    # Reset to head
    command = ["git", "reset", "--hard", "HEAD^"]
    subprocess_run(command)

