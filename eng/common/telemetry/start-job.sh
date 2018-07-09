source="${BASH_SOURCE[0]}"
# resolve $source until the file is no longer a symlink
while [[ -h "$source" ]]; do
  scriptroot="$( cd -P "$( dirname "$source" )" && pwd )"
  source="$(readlink "$source")"
  # if $source was a relative symlink, we need to resolve it relative to the path where the
  # symlink file was located
  [[ $source != /* ]] && source="$scriptroot/$source"
done
scriptroot="$( cd -P "$( dirname "$source" )" && pwd )"

properties=()
while (($# > 0)); do
  lowerI="$(echo $1 | awk '{print tolower($0)}')"
  case $lowerI in
    --queue-id)
      queueId=$2
      shift 2
      ;;
    --source)
      source=$2
      shift 2
      ;;
    --type)
      type=$2
      shift 2
      ;;
    --build)
      build=$2
      shift 2
      ;;
    --attempt)
      attempt=$2
      shift 2
      ;;
    -p)
      properties+=($2)
      shift 2
      ;;
    *)
      echo "Unknown Arg '$1'"
      exit 1
      ;;
  esac
done

jobInfo=`mktemp`

cat > $jobInfo <<JobListStuff
{
  "QueueId": "$queueId",
  "Source": "$source",
  "Type": "$type",
  "Build": "$build",
  "Attempt": "$attempt",
  "Properties": {
    $(printf '%s\n' "${properties[@]}" | sed 's/\([^=]*\)=\(.*\)/"\1":"\2"/' | awk -vORS=, '{ print }' | sed 's/,$//')
  }
}
JobListStuff

curlResult=`
  cat $jobInfo |\
  /bin/bash $scriptroot/curl.sh \
    -H 'Content-Type: application/json' \
    -X POST "https://helix.dot.net/api/2018-03-14/telemetry/job?access_token=$HelixApiAccessToken" -d @-`
curlStatus=$?

if [ $curlStatus -ne 0 ]; then
  echo "Failed To Send Job Start information"
  echo $curlResult
  if /bin/bash "$scriptroot/../is-vsts.sh"; then
    echo "##vso[task.logissue type=error;sourcepath=telemetry/start-job.sh;code=1;]Failed to Send Job Start information: $curlResult"
  fi
  exit 1
fi

export Helix_JobToken=`echo $curlResult | xargs echo` # Strip Quotes

if /bin/bash "$scriptroot/../is-vsts.sh"; then
  echo "##vso[task.setvariable variable=Helix_JobToken;issecret=true;]$Helix_JobToken"
else
  echo "export Helix_JobToken=$Helix_JobToken"
fi


