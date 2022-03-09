#!/bin/bash

set -x
export CrossDacCoreClrVersion=${CrossDacCoreClrVersion:-latest}

# Fetch prebuilt runtime pieces
RUNTIME_URL_PREFIX=$(curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --dry-run --runtime dotnet --channel 3.1 --version ${CrossDacCoreClrVersion} --arch arm64 | grep 'URL #0 - primary:' | sed -e 's/^.*primary: //' -e 's/linux-musl/linux/' -e 's/linux-arm64.tar.gz//')

for rid in linux-arm linux-arm64 linux-musl-arm64 linux-x64 linux-musl-x64
do
  mkdir -p artifacts/crossdac/${rid}
  wget -q ${RUNTIME_URL_PREFIX}${rid}.tar.gz -O artifacts/runtime-${rid}.tar.gz
  tar -f artifacts/runtime-${rid}.tar.gz -C artifacts/crossdac/${rid} --wildcards --xform='s/^.*\(lib[a-zA-Z_0-9]*.so\)/\1/' -x *libcoreclr.so
done

# Build dactablerva.h files
chmod ugo+x src/pal/tools/gen-dactable-rva.sh
for i in $(find artifacts/crossdac -name libcoreclr.so)
do
    src/pal/tools/gen-dactable-rva.sh $i $i.dactablerva.h
done

ls -laR artifacts/crossdac

cd artifacts/crossdac && tar -zcf crossdac-linux-artifacts.tar.gz $(find linux* \( -name libcoreclr.so -o -name \*.h \) ) && cd -

tar -tf artifacts/crossdac/crossdac-linux-artifacts.tar.gz

# Simple error checking.  Ensure the artifacts contain 10 files ((1x -- libcoreclr.so, 1x -- *dactablerva.h) per rid)
if [[ $(tar -tf artifacts/crossdac/crossdac-linux-artifacts.tar.gz | wc -l) -ne 10 ]]
then

  echo "Number of artifacts doesn't match expectations"

  exit 1
fi

exit 0
