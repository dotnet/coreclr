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
parser.add_argument("--priority_0_only", dest="priority_0_only", action="store_true", default=False)
parser.add_argument("--priority_1_only", dest="priority_1_only", action="store_true", default=True)

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

    def has_conflict(bucket, item):
        assert ("/" not in item)
        project_name = item.split(".")

        csproj_variant = project_name[0] + ".csproj"
        ilproj_variant = project_name[0] + ".ilproj"

        if ilproj_variant.lower() in bucket or csproj_variant.lower() in bucket:
            return True
        
        return False

    # Max sln_count is max_sln_count - 1 to account for conflicts.
    naive_split_amount = math.floor(len(test_projects) / (max_sln_count - 1))

    # If we have too few tests, we can bump up the amount of tests per sln to
    # 1k.
    if naive_split_amount < 1000:
        naive_split_amount = 1000

    split_test_projects = []
    current_split = defaultdict(lambda: None)
    conflicts = []

    last_root_dir = None
    force_split = False

    forced_gc_performance = False
    forced_performance = False
    for item in test_projects:
        this_root_dir = item.split(os.path.sep)[0].lower()
        project_name = item.split(os.path.sep)[-1]
        force_split_for_gc_perf = False
        force_split_for_perf = False

        if "performance" == this_root_dir and forced_performance is not True:
            force_split_for_perf = True

        if os.path.join("GC", "Performance") in item and forced_gc_performance is not True:
            force_split_for_gc_perf = True

        if force_split_for_perf or force_split_for_perf:
            force_split = True

        if (this_root_dir != last_root_dir and len(current_split) > naive_split_amount) or force_split is True:
            split_test_projects.append(current_split)
            current_split = defaultdict(lambda: None)

            if force_split_for_gc_perf is True:
                forced_gc_performance = True
                force_split = False
            elif force_split_for_perf is True:
                forced_performance = False
                force_split = False

        if last_root_dir is None:
            last_root_dir = item.split(os.path.sep)[0].lower()

        # Projects with the same name need to be pushed to their own sln.
        if not has_conflict(current_split, project_name):
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
        if not has_conflict(current_split, item[0]):
            current_split[item[0].lower()] = item[1]
        else:
            new_conflicts.append(item)

    next_current_split = defaultdict(lambda: None)
    conflicts = new_conflicts

    # Append the conflict list
    split_test_projects.append(current_split)

    current_split = defaultdict(lambda: None)
    new_conflicts = []

    # Try to put all conflicts into their own sln. Note it is very possible this
    # opportunistic approach will not be enough.
    for item in conflicts:
        if not has_conflict(current_split, item[0]):
            current_split[item[0].lower()] = item[1]
        else:
            new_conflicts.append(item)

    next_current_split = defaultdict(lambda: None)
    conflicts = new_conflicts

    # Append the conflict list
    split_test_projects.append(current_split)

    new_conflicts = []

    # For all tests which have conflicts, we will need to try to backfill them. 
    # At this point we can either change the test names or we can keep creating 
    # slns.
    for item in conflicts:
        conflict_handled = False
        for bucket in split_test_projects:
            if not has_conflict(bucket, item[0]):
                bucket[item[0].lower()] = item[1]
                conflict_handled = True
                break

        if not conflict_handled:
            new_conflicts.append(item)

    current_split = defaultdict(lambda: None)

    if len(new_conflicts) > 0:
        # Create a conflict bucket.
        for item in new_conflicts:
            current_split[item[0]] = item[1]

        # Append the conflict list
        split_test_projects.append(current_split)

    if len(current_split) > 0:
        print("""After attempting to add conflicts to different sln files, there
where too many conflicts to generate unconflicted sln files. You can avoid this
problem by either increasing the max_sln_count, or renaming tests such that
there are less conflicting project names.""")

    print(len(current_split))

    assert len(split_test_projects) <= max_sln_count
    
    # Convert to lists instead of dicts.
    buckets = []

    for bucket in split_test_projects:
        buckets.append([key for item, key in bucket.items()])

    return buckets

def split_buckets_between_priorities(coreclr_test_src_dir, buckets):
    """ Given a set of buckets separate them between Pri0/Pri1 and Win32 only
    """

    test_builds_windows_only = []
    priority_0 = []
    priority_1 = []
    test_disabled = []

    for item in buckets:
        if item.split(os.path.sep)[0].lower() == "common":
            continue
        elif os.path.join("JIT", "config").lower() in item.lower():
            continue
        elif os.path.join("Performance", "perfromance.csproj").lower() == item.lower():
            continue
        elif os.path.join("Performance", "Scenario", "JitBench", "unofficial_dotnet", "JitBench.csproj").lower() == item.lower():
            continue
        elif os.path.join("Loader", "classloader", "generics", "regressions", "DD117522", "Test.csproj").lower() == item.lower():
            continue

        with open(os.path.join(coreclr_test_src_dir, item)) as file_handle:
            contents = file_handle.read()

            # Only builds on windows
            if "TestUnsupportedOutsideWindows" in contents:
                test_builds_windows_only.append(item)

            # Disabled due to missing features
            elif "DisableProjectBuild" in contents:
                continue
            
            # Priority 1
            elif "CLRTestPriority>1" in contents:
                priority_1.append(item)

            # We build Exe tests only.
            elif "<OutputType>Library" in contents:
                continue

            # Skip referenced projects.
            elif "BuildOnly" in contents:
                continue
            
            else:
                priority_0.append(item)

    return (priority_0, priority_1, test_builds_windows_only)

def create_sln_files(dotnetcli, coreclr_test_dir, max_sln_count, priority_0, priority_1, win32_tests, coreclr_args):
    """ Create sln files based on a set of sln buckets.
    """

    def create_sln_files_for_group(dotnetcli, coreclr_test_dir, max_sln_count, group_name, sln_buckets):
        buckets = divide_tests_for_slns(sln_buckets, max_sln_count)
        coreclr_test_src_dir = os.path.join(coreclr_test_dir, "src")

        extension = ".sh" if sys.platform != "win32" else ".cmd"

        if not os.path.isfile(dotnetcli):
            # Run init tools
            command = [os.path.join(coreclr_test_dir, "..", "init-tools{}".format(extension))]

            print(" ".join(command))
            subprocess.check_output(command)

        assert os.path.isfile(dotnetcli)
        assert os.path.isdir(coreclr_test_dir)

        sln_base_name = group_name + "-"
        sln_extension = ".sln"

        old_cwd = os.getcwd()
        os.chdir(coreclr_test_src_dir)

        count = 1
        for bucket in buckets:
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

    if not coreclr_args.priority_1_only:
        print("Creating pri0 slns.")
        create_sln_files_for_group(dotnetcli, coreclr_test_dir, max_sln_count, "priority_0", priority_0)
    
    if not coreclr_args.priority_0_only:
        print("Creating pri1 slns.")
        create_sln_files_for_group(dotnetcli, coreclr_test_dir, max_sln_count, "priority_1", priority_1)
        create_sln_files_for_group(dotnetcli, coreclr_test_dir, max_sln_count, "windows_only", win32_tests)

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
    coreclr_args.verify(args,
                        "priority_0_only",
                        lambda unused: True,
                        "Error, unable to set priority_0_only.")
    coreclr_args.verify(args,
                        "priority_1_only",
                        lambda unused: True,
                        "Error, unable to set priority_1_only.")

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

    print("")
    print("Found {} old built tests".format(len(old_built_tests)))
    print("Found {} test projects.".format(len(test_projects)))

    test_projects_list = [item for item in test_projects]
    test_projects_list.sort()

    dotnetcli = os.path.join(coreclr_args.coreclr_repo_location, ".dotnet", "dotnet")

    priority_0, priority_1, win32_tests = split_buckets_between_priorities(coreclr_test_source_dir, test_projects)

    disabled_test_count = len(test_projects) - (len(priority_0) + len(priority_1) + len(win32_tests))

    print("")
    print("Found:")
    print("Priority 0: {} tests.".format(len(priority_0)))
    print("Priority 1: {} tests.".format(len(priority_1)))
    print("Windows Only: {} tests.".format(len(win32_tests)))
    print("")
    print("Disabled: {} tests.".format(disabled_test_count))

    create_sln_files(dotnetcli, coreclr_test_dir, coreclr_args.max_sln_count, priority_0, priority_1, win32_tests, coreclr_args)

    return 0

################################################################################
# __main__
################################################################################

if __name__ == "__main__":
    args = parser.parse_args()
    sys.exit(main(args))