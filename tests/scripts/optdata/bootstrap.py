#!/usr/bin/env python

"""
    This script prepares the local source tree to be built with
    custom optdata. Simply run this script and follow the
    instructions to inject manually created optdata into the build.
"""

import argparse
import json
import os
from os import path
import shutil
import subprocess
import sys

# Display the docstring if the user passes -h|--help
argparse.ArgumentParser(description=__doc__).parse_args()

SCRIPT_ROOT = path.dirname(path.realpath(__file__))
REPO_ROOT = path.realpath(path.join(SCRIPT_ROOT, '..', '..', '..'))

ORIGIN_FILE = path.join(SCRIPT_ROOT, 'project.json')
TARGET_FILE = path.join(REPO_ROOT, 'src', '.nuget', 'optdata', 'project.json')

ARCH_LIST = ['x64', 'x86']
TOOL_LIST = ['IBC', 'PGO']

def get_buildos():
    """Returns the Build_OS component used by the build system."""
    if os.name == 'nt':
        return 'Windows_NT'
    else:
        sysname = os.uname()[0]
        return 'OSX' if sysname.casefold() == 'Darwin'.casefold() else sysname

def get_optdata_version(tool):
    """Returns the version string specified in project.json for the given tool."""
    package_name = 'optimization.{0}.CoreCLR'.format(tool)
    with open(ORIGIN_FILE) as json_file:
        return json.load(json_file)['dependencies'][package_name]

def get_optdata_dir(tool, arch):
    """Returns an absolute path to the directory that should contain optdata given a tool,arch"""
    package_name = 'optimization.{o}-{a}.{t}.CoreCLR'.format(o=get_buildos(), a=arch, t=tool)
    package_version = get_optdata_version(tool)
    return path.join(REPO_ROOT, 'packages', package_name, package_version, 'data')

def check_for_unstaged_changes(file_path):
    """Returns whether a file in git has untracked changes."""
    if not path.exists(file_path):
        return False
    try:
        subprocess.check_call(['git', 'diff', '--quiet', '--', file_path])
        return False
    except subprocess.CalledProcessError:
        return True

def main():
    """Entry point"""
    if check_for_unstaged_changes(TARGET_FILE):
        print("ERROR: You seem to have unstaged changes to {0} that would be overwritten.".format(
            TARGET_FILE
        ))
        print("Please clean, commit, or stash them before running this script.")
        return 1

    if not path.exists(path.dirname(TARGET_FILE)):
        os.makedirs(path.dirname(TARGET_FILE))
    shutil.copyfile(ORIGIN_FILE, TARGET_FILE)

    print("Bootstrapping optdata is complete.")
    for tool in TOOL_LIST:
        for arch in ARCH_LIST:
            optdata_dir = get_optdata_dir(tool, arch)
            print("  * Copy {a} {t} files into: {d}".format(a=arch, t=tool, d=optdata_dir))
    print("NOTE: Make sure to add 'skiprestoreoptdata' as a switch on the build command line!")

    return 0

if __name__ == '__main__':
    sys.exit(main())
