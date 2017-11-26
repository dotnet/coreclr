#!/usr/bin/env python
################################################################################
################################################################################
#
# Module: ci_job_diff.py
#
# Notes:
#
# Script to download and setup the tests on a machine. There are two main uses
# for this script, either for use in netci or dev use. 
# 
# In the lab, the script requires the ability to automatically determine if 
# there has been a change in the <CoreclrRootDir>/test/ directory.
#
# For dev purposes the script is assumed to be invoked only when the tests are
# needed. Note that the script will overwrite and place the tests in the default
# location under bin/<OS>.<Arch>.<Configruation> even if that requires deleting
# the existing folder to place it there. To avoid destructive behavoir pass
# the -test_location parameter.
#
################################################################################
################################################################################

import argparse
import datetime
import json
import os
import platform
import shutil
import subprocess
import tarfile
import urllib
import urllib2
import sys
import zipfile

from collections import defaultdict

################################################################################
# Globals
################################################################################

os_groups = {
    "Darwin": "OSX", 
    "Linux": "Linux", 
    "Windows": "Windows_NT"
}

arch_groups = {
    "x86_64": "x64",
    "i386": "x86"
}

g_current_os = os_groups[platform.system()]
g_current_arch = arch_groups[platform.machine()]

g_netci_location = "https://ci.dot.net/job/dotnet_coreclr"

script_location = os.path.dirname(os.path.realpath(__file__))
bin_location = os.path.join(script_location, "bin")
g_tests_location = os.path.join(script_location, "bin", "tests", "%s.%s.%s" % (g_current_os, g_current_arch, 'Checked'))

################################################################################
# Argument Parser
################################################################################

description = """Script to download and setup the tests on a machine. 
There are two main uses for this script, either for use in netci or dev use.
 
In the lab, the script requires the ability to automatically determine if 
there has been a change in the <CoreclrRootDir>/test/ directory.

For dev purposes the script is assumed to be invoked only when the tests are
needed. Note that the script will overwrite and place the tests in the default
location under bin/<OS>.<Arch>.<Configruation> even if that requires deleting
the existing folder to place it there. To avoid destructive behavoir pass
the -test_location parameter.
"""

parser = argparse.ArgumentParser(description=description)

parser.add_argument("-test_location", dest="test_location", nargs='?', default=g_tests_location)
parser.add_argument("-configuration", dest="configuration", nargs='?', default="Checked")
parser.add_argument("-priority", dest="priority", nargs='?', default="0")

parser.add_argument("-branch", dest="branch", nargs='?', default="master")

parser.add_argument("--check_for_modifications", dest="check_for_modifications", action="store_true", default=False)
parser.add_argument("--download_product", dest="download_product", action="store_true", default=False)

################################################################################
# Helper methods
################################################################################

def __check_for_modifications__():
    """ Check to see if <CoreclrRootDir>/tests/** has been modified

    Returns:

        modified (Bool): True if <CoreclrRootDir>/tests/** has been modified

    """
    command = "git rev-parse --abbrev-ref HEAD".split(" ")

    current_branch_name = subprocess.check_output(command)
    diff_command = "git diff --name-status master..%s" % current_branch_name.strip()

    diff_output = subprocess.check_output(diff_command.split(" "))

    changed_files = diff_output.split("\t")[1:]
    changed_files = [item.split("\n")[0] for item in changed_files]

    test_dir_modified = False

    for item in changed_files:
        if "tests" in item:
            test_dir_modified = True
            break

    return test_dir_modified

def download_tests(os_group, arch, configuration, priority, branch, test_location, download_product):
    """ Download the tests to the passed location

    Args:
        os_group        (str): operating system
        arch            (str): architecture
        configuration   (str): configuration
        priority        (str): priority
        branch          (str): branch
        test_location   (str): location to download to

    """

    original_os = os_group
    original_arch = arch
    original_configuration = configuration

    if "/" in branch:
        branch = branch.replace("/", "_")

    netci_location = g_netci_location
    netci_location = "%s/job/%s/job" % (netci_location, branch)

    if arch == "x64":
        arch = ""
    elif "arm" in arch:
        arch = arch + "_cross_"
    else:
        arch = arch + "_"

    arch = arch.lower()

    configuration = configuration + "_"
    configuration = configuration.lower()
    os_group = os_group.lower()

    if os_group == "linux":
        os_group = "ubuntu"
    elif os_group == "osx":
        os_group = "%s10.12" % os_group

    if arch == "arm64" and os_group == "linux":
        os_group = "small_page_size"
    
    product_netci_location = "%s/%s%s%s/lastSuccessfulBuild/artifact/bin/Product/%s.%s.%s/*zip*/archive.zip" % (netci_location, arch, configuration, os_group, original_os, original_arch, original_configuration)
    obj_netci_location = "%s/%s%s%s/lastSuccessfulBuild/artifact/bin/obj/*zip*/archive.zip" % (netci_location, arch, configuration, os_group)
    tests_netci_location = "%s/%s%s%s/lastSuccessfulBuild/artifact/bin/tests/%s.%s.%s.tar.gz" % (netci_location, arch, configuration, os_group, original_os, original_arch, original_configuration)

    product_zip_location = os.path.join(bin_location, "Product", "%s.%s.%s.zip" % (original_os, original_arch, original_configuration))
    obj_zip_location = os.path.join(bin_location, "obj", "%s.%s.%s.zip" % (original_os, original_arch, original_configuration))
    tests_zip_location = os.path.join(bin_location, "tests", "%s.%s.%s.tar.gz" % (original_os, original_arch, original_configuration))

    if os.path.isdir(test_location):
        shutil.rmtree(test_location, ignore_errors=True)

    os.mkdir(test_location)

    def download_and_unzip_file(netci_location, zip_location, location, use_gzip=False):
        try:
            tests = urllib.URLopener()
            tests.retrieve(netci_location, zip_location)
        except Exception, e:
            print e
        
            sys.exit(1)

        assert os.path.isfile(zip_location)

        if not use_gzip:
            zip = zipfile.ZipFile(zip_location)
            zip.extractall(location)
        else:
            tar = tarfile.open(zip_location, "r:gz")
            tar.extractall(location)

        os.remove(zip_location)

    if download_product:
        product_dir = os.path.join(bin_location, "Product")
        obj_dir = os.path.join(bin_location, "obj")

        if not os.path.isdir(product_dir):
            os.mkdir(product_dir)

        if not os.path.isdir(obj_dir):
            os.mkdir(obj_dir)

        download_and_unzip_file(product_netci_location, product_zip_location, os.path.join(bin_location, "Product"))
        download_and_unzip_file(obj_netci_location, obj_zip_location, os.path.join(bin_location, "obj"))

    test_dir = os.path.join(bin_location, "tests")

    if not os.path.isdir(test_dir):
        os.mkdir(test_dir)

    scratch_location = os.path.join(bin_location, "tests", "scratch")
    if os.path.isdir(scratch_location):
        shutil.rmtree(scratch_location, ignore_errors=True)
    
    download_and_unzip_file(tests_netci_location, tests_zip_location, scratch_location, True)

    if os.path.isdir(os.path.join(bin_location, "tests", "%s.%s.%s" % (original_os, original_arch, original_configuration))):
        shutil.rmtree(os.path.join(bin_location, "tests", "%s.%s.%s" % (original_os, original_arch, original_configuration)), ignore_errors=True)

    # Move the test dir
    shutil.move(os.path.join(scratch_location, "bin", "tests", "%s.%s.%s" % (original_os, original_arch, original_configuration)), os.path.join(bin_location, "tests", "%s.%s.%s" % (original_os, original_arch, original_configuration)))

    shutil.rmtree(scratch_location)

    assert os.path.isdir(test_location)
    assert os.path.isdir(os.path.join(bin_location, "Product"))
    assert os.path.isdir(os.path.join(bin_location, "obj"))
    assert os.path.isdir(os.path.join(bin_location, "tests"))

################################################################################
# Main
################################################################################

def main(args):
    download_product = args.download_product
    test_location = args.test_location
    configuration = args.configuration
    priority = args.priority
    branch = args.branch

    check_for_modifications = args.check_for_modifications

    modified = False

    if check_for_modifications:
        modified = __check_for_modifications__()

    if not modified:
        # Download and set up the tests.
        download_tests(g_current_os, g_current_arch, configuration, priority, branch, test_location, download_product)

################################################################################
# __main__ (entry point)
################################################################################

if __name__ == "__main__":
    args = parser.parse_args()

    sys.exit(main(args))