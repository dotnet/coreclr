#!/usr/bin/env python
#
## Licensed to the .NET Foundation under one or more agreements.
## The .NET Foundation licenses this file to you under the MIT license.
## See the LICENSE file in the project root for more information.
#
##
# Title               : create_sln_test_files.py
#
# Notes:
#
# As part of the move to building and running SDK style tests. Coreclr uses
# sln files to explicitely reference test project files.
#
# This allows the entire multiproject build to run dotnet build **/*.sln
#
################################################################################
################################################################################

import argparse
import distutils.dir_util
import math
import os
import re
import shutil
import subprocess
import urllib
import sys
import tarfile
import zipfile

# Import coreclr_arguments from the /coreclrdir/scripts
sys.path.append(os.path.join(os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__)))), "scripts"))
from coreclr_arguments import *

################################################################################
# Argument parser
################################################################################

description = ("""
As part of the move to building and running SDK style tests. Coreclr uses
sln files to explicitely reference test project files.

This allows the entire multiproject build to run dotnet build **/*.sln""")

parser = argparse.ArgumentParser(description=description)

parser.add_argument("-arch", dest="arch", nargs='?', default="x64") 
parser.add_argument("-build_type", dest="build_type", nargs='?', default="Checked")
parser.add_argument("-coreclr_repo_location", dest="coreclr_repo_location", default=os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
parser.add_argument("-old_test_build", dest="old_test_build")
parser.add_argument("-max_sln_count", dest="max_sln_count", default="10")
parser.add_argument("--use_subdir_for_sln_creation", dest="use_subdir_for_sln_creation", default=False)

################################################################################
# Helper Methods
################################################################################

def find_tests(path, split_string=None, extension=".exe"):
    """ Find all of the tests recursively for a passed directory.

    Args:
        path (str): path to look

    Returns:
        collections.defaultdict(str[str]): tests.
    """

    def recursive_find_tests(path, data_structure, split_string):
        dir_contents = os.listdir(path)
        for item in dir_contents:
            if (os.path.isdir(os.path.join(path, item))):
                recursive_find_tests(os.path.join(path, item), data_structure, split_string)
        
            elif item.endswith(extension):
                full_path_test = os.path.join(path, item)

                split_string_was_set = True
                modified_split_string = None

                if split_string is None:
                    split_string_was_set = False
                    if "artifacts" in full_path_test:
                        modified_split_string = os.path.join("artifacts", "tests")
                    
                    else:
                        modified_split_string = os.path.join("bin", "tests")

                else:
                    modified_split_string = split_string

                modified_split_string = modified_split_string + os.path.sep if not modified_split_string.endswith(os.path.sep) else modified_split_string
                relative_path = full_path_test.split(modified_split_string)[1]

                # The next sequence is either $(BuildOS).$(BuildArch).$(Config)
                # or $(BuildOs)/$(BuildArch)/$(Config)
                #
                # Either way remove it.

                if not split_string_was_set:
                    relative_path = relative_path[13:]
                    assert(len(relative_path) > 0)

                assert relative_path not in data_structure
                data_structure[relative_path] = full_path_test

    data_structure = defaultdict(lambda: None)

    recursive_find_tests(path, data_structure, split_string)
    assert(len(data_structure) > 0)

    return data_structure

def divide_tests_for_slns(test_projects, max_sln_count):
    """ Create multiple lists of test projects
    
    Args:
        test_projects([str]): list of all test projects
        max_sln_count(int)  : max sln files to split projects into

    Returns:
        test_projects_split: [[str]]
    """

    # Max sln_count is max_sln_count - 1 to account for conflicts.
    naive_split_amount = math.floor(len(test_projects) / (max_sln_count - 1))

    split_test_projects = []
    current_split = defaultdict(lambda: None)
    conflicts = []

    last_root_dir = None
    for item in test_projects:
        this_root_dir = item.split(os.path.sep)[0].lower()
        if this_root_dir != last_root_dir and len(current_split) > naive_split_amount:
            split_test_projects.append(current_split)
            current_split = defaultdict(lambda: None)

        if last_root_dir is None:
            last_root_dir = item.split(os.path.sep)[0].lower()

        project_name = item.split(os.path.sep)[-1]

        # Projects with the same name need to be pushed to their own sln.
        if not project_name in current_split:
            current_split[project_name.lower()] = item
        else:
            conflicts.append((project_name.lower(), item))

    # Append the last list
    split_test_projects.append(current_split)

    current_split = defaultdict(lambda: None)
    new_conflicts = []

    # Try to put all conflicts into their own sln. Note it is very possible this
    # opportunistic approach will not be enough.
    for item in conflicts:
        if item[0] not in current_split:
            current_split[item[0]] = item[1]
        else:
            new_conflicts.append(item)

    next_current_split = defaultdict(lambda: None)
    conflicts = new_conflicts

    # Append the conflict list
    split_test_projects.append(current_split)

    new_conflicts = []

    # For all tests which have conflicts, we will need to try to fit them
    # wherever we can. At this point we can either change the test names
    # or we can keep creating slns.
    for item in conflicts:
        conflict_handled = False
        for bucket in split_test_projects:
            if item[0] not in split_test_projects:
                bucket[item[0]] = item[1]
                conflict_handled = True
                break

        if not conflict_handled:
            new_conflicts.append(item)

    if len(new_conflicts) > 0:
        print("""After attempting to add conflicts to different sln files, there
where too many conflicts to generate unconflicted sln files. You can avoid this
problem by either increasing the max_sln_count, or renaming tests such that
there are less conflicting project names.""")

    assert len(split_test_projects) <= max_sln_count
    
    # Convert to lists instead of dicts.
    buckets = []

    for bucket in split_test_projects:
        buckets.append([key for item, key in bucket.items()])

    return buckets

def create_sln_files(dotnetcli, coreclr_test_dir, sln_buckets):
    """ Create sln files based on a set of sln buckets.
    """

    coreclr_test_src_dir = os.path.join(coreclr_test_dir, "src")

    extension = ".sh" if sys.platform != "win32" else ".cmd"

    if not os.path.isfile(dotnetcli):
        # Run init tools
        command = [os.path.join(coreclr_test_dir, "..", "init-tools{}".format(extension))]

        print(" ".join(command))
        subprocess.check_output(command)

    assert os.path.isfile(dotnetcli)
    assert os.path.isdir(coreclr_test_dir)

    sln_base_name = "tests"
    sln_extension = ".sln"

    old_cwd = os.getcwd()
    os.chdir(coreclr_test_src_dir)

    count = 1
    for bucket in sln_buckets:
        if not os.path.isfile(os.path.join(coreclr_test_src_dir, sln_base_name + str(count) + sln_extension)):
            # Create the sln
            command = [dotnetcli, "new", "sln", "-n", sln_base_name + str(count)]

            print(" ".join(command))
            subprocess.check_output(command)

        for index, item in enumerate(bucket):
            full_path = os.path.join(coreclr_test_dir, "src", item)
            # Add each project.
            command = [dotnetcli, "sln", os.path.join(coreclr_test_src_dir, sln_base_name + str(count) + sln_extension), "add", full_path]

            print("[{}:{}] {}".format(index + 1, len(bucket), " ".join(command)))
            subprocess.check_output(command)

        count += 1

################################################################################
# main
################################################################################

def main(args):
    """ Main method
    """

    coreclr_args = CoreclrArguments(args, require_built_core_root=False, require_built_product_dir=False, require_built_test_dir=False, default_build_type="Checked")

    coreclr_args.verify(args,
                        "old_test_build",
                        lambda old_test_build: os.path.isdir(old_test_build),
                        "Error, old_test_build should be the base path to the old unchanged tests.")
    coreclr_args.verify(args,
                        "max_sln_count",
                        lambda max_sln_count: max_sln_count.isdigit(),
                        "Error, max_sln_count must be a valid number.",
                        modify_arg=lambda max_sln_count: int(max_sln_count),
                        modify_after_validation=True)
    coreclr_args.verify(args,
                        "use_subdir_for_sln_creation",
                        lambda unused: True,
                        "Error, unable to set use_subdir_for_sln_creation.")

    # Find old tests
    print("Finding old tests at {}...".format(coreclr_args.old_test_build))
    old_built_tests = find_tests(coreclr_args.old_test_build)

    coreclr_test_dir = os.path.join(coreclr_args.coreclr_repo_location, "tests")
    coreclr_test_source_dir = os.path.join(coreclr_test_dir, "src")

    print("Finding all .csproj files under {}...".format(coreclr_test_source_dir))
    unbuilt_cs_tests = find_tests(coreclr_test_source_dir, extension=".csproj", split_string=os.path.join("tests", "src"))

    print("Finding all .ilproj files under {}...".format(coreclr_test_source_dir))
    unbuilt_il_tests = find_tests(coreclr_test_source_dir, extension=".ilproj", split_string=os.path.join("tests", "src"))

    test_projects = defaultdict(lambda: None)

    for item in unbuilt_il_tests:
        assert item not in test_projects
        test_projects[item] = unbuilt_il_tests[item]
    
    for item in unbuilt_cs_tests:
        assert item not in test_projects
        test_projects[item] = unbuilt_cs_tests[item]

    print("Found {} old built tests".format(len(old_built_tests)))
    print("Found {} test projects.".format(len(test_projects)))

    test_projects_list = [item for item in test_projects]
    test_projects_list.sort()

    dotnetcli = os.path.join(coreclr_args.coreclr_repo_location, ".dotnet", "dotnet")

    sln_buckets = divide_tests_for_slns(test_projects_list, coreclr_args.max_sln_count)
    create_sln_files(dotnetcli, coreclr_test_dir, sln_buckets)

    return 0

################################################################################
# __main__
################################################################################

if __name__ == "__main__":
    args = parser.parse_args()
    sys.exit(main(args))