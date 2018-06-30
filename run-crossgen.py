#!/usr/bin/env python

import argparse
import glob
import json
import hashlib
import os
import re
import sys

platform_assemblies_paths_sep = ";"
core_lib_dll="System.Private.CoreLib.dll"

def run_crossgen(executable, platform_assemblies_paths, il_filename, ni_filename):
    global platform_assemblies_paths_sep

    print("Generating native image for \"{0}\"".format(il_filename))
    command = "{executable} /nologo /Platform_Assemblies_Paths {platform_assemblies_paths} /out {ni_filename} {il_filename}".format(
        executable=executable,
        platform_assemblies_paths=platform_assemblies_paths_sep.join(platform_assemblies_paths), 
        ni_filename=ni_filename,
        il_filename=il_filename)

    return os.system(command)

def compute_filehash(filename, block_size=65536):
    print("Computing SHA256 file hash for \"{0}\"".format(filename))
    algo = hashlib.sha256()
    with open(filename, "rb") as file:
        while True:
            block = file.read(block_size)
            if block:
                algo.update(block)
            else:
                break
    return algo.hexdigest()

def get_framework_assemblies(core_root):
    global core_lib_dll

    framework_assemblies = glob.glob(os.path.join(core_root, "System.*.dll"))
    framework_assemblies = filter(lambda name: not name.endswith(core_lib_dll), framework_assemblies) # Exclude System.Private.CoreLib.dll
    framework_assemblies.extend(glob.glob(os.path.join(core_root, "Microsoft.*.dll")))
    framework_assemblies.extend(glob.glob(os.path.join(core_root, "NuGet.*.dll")))

    return framework_assemblies

def crossgen_one_assembly(executable, platform_assemblies_paths, il_filename, ni_filename, crossgen_result_dict):
    crossgen_result_dict['ILFileName'] = il_filename
    crossgen_result_dict['NIFileName'] = ni_filename

    exit_code = run_crossgen(executable, platform_assemblies_paths, il_filename, ni_filename)
    crossgen_result_dict['ExitCode'] = exit_code

    il_filehash = compute_filehash(il_filename)
    crossgen_result_dict['ILFileHash'] = il_filehash

    if exit_code == 0:
        ni_filehash = compute_filehash(ni_filename)
    else:
        ni_filehash = None

    crossgen_result_dict['NIFileHash'] = ni_filehash

def add_ni_extension(filename):
    filename,ext = os.path.splitext(filename)
    return filename + ".ni" + ext

def main(args):
    global core_lib_dll
    print("crossgen_executable       : {0}".format(args.crossgen_executable))
    print("core_root                 : {0}".format(args.core_root))
    print("product_dir               : {0}".format(args.product_dir))
    print("json_filename             : {0}".format(args.json_filename))

    platform_assemblies_paths=[os.path.join(args.product_dir, "IL")]

    il_filename=os.path.join(args.product_dir, "IL", core_lib_dll)
    ni_filename=os.path.join(args.core_root, core_lib_dll)

    crossgen_result_dict = dict()
    crossgen_one_assembly(args.crossgen_executable, platform_assemblies_paths, il_filename, ni_filename, crossgen_result_dict)

    crossgen_results = [crossgen_result_dict]

    framework_assemblies = get_framework_assemblies(args.core_root)
    platform_assemblies_paths=[args.core_root]

    for il_filename in framework_assemblies:
        ni_filename = add_ni_extension(il_filename)

        crossgen_result_dict = dict()

        crossgen_one_assembly(args.crossgen_executable, platform_assemblies_paths, il_filename, ni_filename, crossgen_result_dict)
        crossgen_results.append(crossgen_result_dict)

    with open(args.json_filename, "wt") as json_file:
        json_file.writelines(json.dumps(crossgen_results, indent=True))

    sys.exit(0)

if __name__ == '__main__':
    parser = argparse.ArgumentParser()

    parser.add_argument('-core_root', dest='core_root')
    parser.add_argument('-product_dir', dest='product_dir')
    parser.add_argument('-crossgen_executable', dest='crossgen_executable')
    parser.add_argument('-json_filename', dest='json_filename')

    args = parser.parse_args()
    main(args)
