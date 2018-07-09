#runs curl and exits with exit code when http server errors happen
res=`mktemp`
httpCode=$(curl --silent --output $res --write-out "%{http_code}" "$@")
curlCode=$?

if [ ! $curlCode ]; then
  exit $curlCode
fi

cat $res

if [ $httpCode -gt 299 ] || [ $httpCode -lt 200 ]; then
  exit 1
else
  exit 0
fi
