#!/usr/bin/env bash
#
# This is intended to be a simple script to do the regular merging of the
# origin/release/3.1 branch servicing release tags into the
# origin/release/3.1-crossdac branch.
#
# Usage : ./crossDacMergeLatestTag.sh <upstreamUrl>
#
# Assumes the script is run from within the coreclr repo tree
#
# The script:
# - Gets current upstream tags
# - Finds latest tag by (semantic version 2) numeric patch order
# - Checks to make sure the latest tag hasn't already been merged into origin/release/3.1-crossdac
# - Creates a new branch based on current origin/release/3.1-crossdac.  Names it origin/release/3.1-crossdac-${Latest3_1_tag}
# - Creates the merge commit
# - Pushes the commit upstream to prepare for PR creation and review.

export upstream=$1

git fetch origin

export Latest3_1_tag=$(git tag | grep v3.1.[0-9]*$ | sort -n -k 1.6 | tail -n 1)

echo The latest tag is ${Latest3_1_tag}

# Check to see if the latest tag is already merged into the release/3.1-crossdac tip
git describe --tags origin/release/3.1-crossdac | grep -q "^${Latest3_1_tag}-"
exitCode=$?

if (( exitCode == 0 ))
then
  echo The latest tag is already merged into the origin/release/3.1-crossdac tip
  git describe --tags origin/release/3.1-crossdac
  exit 1
fi

git checkout -b release/3.1-crossdac-${Latest3_1_tag} origin/release/3.1-crossdac
exitCode=$?

if (( exitCode != 0 ))
then
  echo "Couldn't create and checkout the new branch release/3.1-crossdac-${Latest3_1_tag}"
  exit 2
fi


git merge -m "Merge tag ${Latest3_1_tag} into release/3.1-crossdac" ${Latest3_1_tag}
exitCode=$?

if (( exitCode != 0 ))
then
  echo The merge commit failed. The merge requires manual intervention.
  exit 3
fi

if ( "$upstream" == "" )
then
  echo Usage : ./crossDacMergeLatestTag.sh <upstreamUrl>
  echo <upstreamUrl> empty.  Please provide remote name or URL to push merge commit upstream
  echo You can also just execute 'git push <upstreamUrl>' yourself...
  exit 4
fi

git push -f $upstream
