#!/usr/bin/env python
################################################################################
################################################################################
#
# Module: superpmi-blob.py
#
# Notes:
#
# Python script to facilitate uploading and downloading artifacts from the helix
# build agents.
#
################################################################################
################################################################################

import argparse
import datetime
import json
import math
import multiprocessing
import os
import platform
import shutil
import subprocess
import sys
import tempfile
import time
import re
import string
import zipfile

import xml.etree.ElementTree

from azure.storage.blob import BlockBlobService
from azure.storage.blob import ContentSettings

from collections import defaultdict
from multiprocessing import Process, Queue, Pipe, Lock
from sys import platform as _platform

# Version specific imports

if sys.version_info.major < 3:
     import urllib
else:
     import urllib.request

################################################################################
# Argument Parser
################################################################################

description = ("""Python script to facilitate uploading and downloading artifacts 
from the helix build agents. """)

superpmi_replay_help = """ Location of the mch file to run a replay over. Note
that this may either be a location to a path on disk or a uri to download the
mch file and replay it.
"""

parser = argparse.ArgumentParser(description=description)

subparsers = parser.add_subparsers(dest='mode')

# subparser for uploading
upload_parser = subparsers.add_parser("upload")

# Add required arguments
upload_parser.add_argument("-helix_queue", nargs=1, help="Helix queue being targetted")
upload_parser.add_argument("-arch", nargs=1, help="Architecture")
upload_parser.add_argument("-build_config", nargs=1, help="Build configuration [Debug, Checked, Release]")
upload_parser.add_argument("-scenario", nargs=1, help="Scenario [jitStress, gcStress...]")
upload_parser.add_argument("-build_id", nargs=1, help="Build identifier")
upload_parser.add_argument("-id", nargs=1, help="Unique id for blob name.")
upload_parser.add_argument("-az_storage_connection_key", nargs=1, help="Key for azure storage.")
upload_parser.add_argument("-artifact", nargs="+", help="Artifacts to upload.")

# subparser for uploading
download_parser = subparsers.add_parser("download")

# Add required arguments
download_parser.add_argument("-helix_queue", nargs=1, help="Helix queue being targetted")
download_parser.add_argument("-arch", nargs=1, help="Architecture")
download_parser.add_argument("-build_config", nargs=1, help="Build configuration [Debug, Checked, Release]")
download_parser.add_argument("-scenario", nargs=1, help="Scenario [jitStress, gcStress...]")
download_parser.add_argument("-build_id", nargs=1, help="Build identifier")
download_parser.add_argument("-id", nargs=1, help="Unique id for blob name.")
download_parser.add_argument("-az_storage_connection_key", nargs=1, help="Key for azure storage.")
download_parser.add_argument("-download_location", nargs=1, help="Download location for artifacts.")

################################################################################
# Globals
################################################################################

account = "clrjit"

# ################################################################################
# # Azure Helper Functions
# ################################################################################

def download(block_blob_service, unique_id, build_id, blob_name, download_location):
    base_location = "{}/{}/".format(unique_id, build_id)
    artifact_locations = None

    generator = block_blob_service.list_blobs("artifacts", prefix=base_location)

    if not os.path.isdir(download_location):
        os.makedirs(download_location)

    for item in generator:
        item_name = item.name.split(base_location)[1]
        local_location = "{}/{}".format(download_location, item_name)

        print("Downloading: {} -> {}".format(item.name, local_location))
        result = block_blob_service.get_blob_to_path("artifacts", item.name, local_location)

def upload(block_blob_service, unique_id, build_id, blob_name, artifacts):
    base_location = "{}/{}/".format(unique_id, build_id)
    artifact_locations = None

    for item in artifacts:
        basename = os.path.basename(item)
        artifact_locations = "{}{}".format(base_location, "{}-{}".format(blob_name, basename))
    
        print("Uploading: {} -> {}".format(item, artifact_locations))
        block_blob_service.create_blob_from_path("artifacts", artifact_locations, item)

        print()


def setup(account):
    """Setup the blob storage account with a artifacts container.

    Create the aftifacts container. It will always be attempted to be created.

    Args:
        None
    Returns:
        None
    """

    try:
        container = account.create_container("artifacts")
    except:
        container = None

    if container:
        print("Container created.")
    else:
        print("Container exists, skipping creation.")

################################################################################
# Main
################################################################################

def main(args):
    args = parser.parse_args(args)

    mode = args.mode
    
    artifacts = None
    id = None
    if mode == "upload":
        artifacts = args.artifact   
        id = args.id[0]
        
        for item in artifacts:
            assert os.path.isfile(item)

    helix_queue = args.helix_queue[0]
    arch = args.arch[0]
    build_config = args.build_config[0]
    scenario = args.scenario[0]
    build_id = args.build_id[0]
    az_storage_connection_key = args.az_storage_connection_key[0]

    download_location = None
    if mode == "download":
        download_location = args.download_location[0]

    unique_id = "{}-{}-{}-{}".format(helix_queue, arch, build_config, scenario)

    block_blob_service = BlockBlobService(account_name=account, account_key=az_storage_connection_key)

    setup(block_blob_service)

    if mode == "upload":
        upload(block_blob_service, unique_id, build_id, id, artifacts)

    elif mode == "download":
        download(block_blob_service, unique_id, build_id, id, download_location)

################################################################################
# Entry Point
################################################################################

if __name__ == "__main__":
    main(sys.argv[1:])