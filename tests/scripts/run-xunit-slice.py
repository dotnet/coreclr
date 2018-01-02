import json
import argparse
import os
import shutil
import subprocess
import sys

##########################################################################
# Argument Parser
##########################################################################

description = 'Tool to generate slice<number>.txt files based off of a slice json file (see perf-slices.json for format)'

parser = argparse.ArgumentParser(description=description)

parser.add_argument('-slice', dest='slice_number', default=-1, type=int)
parser.add_argument('-slice-file', dest='slice_file', default=None)
parser.add_argument('-slice-dir', dest='slice_dir', default=None)

##########################################################################
# Helper Functions
##########################################################################

def validate_args(args):
    """ Validate all of the arguments parsed.
    Args:
        args (argparser.ArgumentParser): Args parsed by the argument parser.
    Returns:
        (arch, build_type, clr_root, fx_root, fx_branch, fx_commit, env_script)
            (str, str, str, str, str, str, str)
    Notes:
    If the arguments are valid then return them all in a tuple. If not, raise
    an exception stating x argument is incorrect.
    """

    def validate_arg(arg, check):
        """ Validate an individual arg
        Args:
           arg (str|bool): argument to be validated
           check (lambda: x-> bool): test that returns either True or False
                                   : based on whether the check passes.

        Returns:
           is_valid (bool): Is the argument valid?
        """

        helper = lambda item: item is not None and check(item)

        if not helper(arg):
            raise ValueError('Argument: %s is not valid.' % (arg))

    validate_arg(args.slice_number, lambda item: item >= -1)
    validate_arg(args.slice_file, lambda item: os.path.isfile(item))
    validate_arg(args.slice_dir, lambda item: os.path.isdir(item))

    return (args.slice_number, args.slice_file, args.slice_dir)

def main(args):
    slice_number, slice_file, slice_dir = validate_args(args)

    with open(slice_file) as json_data:
        data = json.load(json_data)
        if slice_number >= len(data["slices"]):
            raise ValueError('Invalid slice number. %s is greater than the max number of slices %s' % (slice_number, len(data["slices"])))
        elif slice_number != -1:
            for count, folder in enumerate(data["slices"][slice_number]["folders"]):
                with open (os.path.join(slice_dir, "slice" + str(count+1) + ".txt"), 'w') as f:
                    f.write(folder)

    return 0

if __name__ == "__main__":
    Args = parser.parse_args(sys.argv[1:])
    sys.exit(main(Args))
