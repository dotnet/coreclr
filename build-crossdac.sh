export VERSION_IDENTIFIER=$1

# Install prerequisites
wget https://dotnet.microsoft.com/download/dotnet-core/scripts/v1/dotnet-install.sh

chmod u+x dotnet-install.sh

# Fetch prebuilt runtime pieces

RUNTIME_URL_PREFIX=$(./dotnet-install.sh --dry-run --runtime dotnet --version ${VERSION_IDENTIFIER} --arch arm64 | grep 'Primary named payload URL:' | sed -e 's/^.*URL: //' -e 's/linux-musl/linux/' -e 's/linux-arm64.tar.gz//')

for rid in linux-arm linux-arm64 linux-musl-arm64 linux-x64 linux-musl-x64
do
  mkdir -p artifacts/crossdac/${rid}
  wget ${RUNTIME_URL_PREFIX}${rid}.tar.gz -O artifacts/runtime-${rid}.tar.gz
  tar -f artifacts/runtime-${rid}.tar.gz -C artifacts/crossdac/${rid} --wildcards --xform='s/^.*\(lib[a-zA-Z_0-9]*.so\)/\1/' -x *libcoreclr.so *libmscordaccore.so *libmscordbi.so
done

# Build dactablerva.h files
chmod ugo+x src/pal/tools/gen-dactable-rva.sh
for i in $(find artifacts/crossdac -name libcoreclr.so)
do
    src/pal/tools/gen-dactable-rva.sh $i $i.dactablerva.h
done

cd artifacts/crossdac && tar -zcf crossdac-linux-artifacts.tar.gz $(find linux* \( -name libcoreclr.so -o -name \*.h \) ) && cd -

# Simple error checking.  Ensure the artifacts contain 10 files ((1x -- libcoreclr.so, 1x -- *dactablerva.h) per rid)
if [[ $(tar -tf artifacts/crossdac/crossdac-linux-artifacts.tar.gz | wc -l) -ne 10 ]]
then
  tar -tf artifacts/crossdac/crossdac-linux-artifacts.tar.gz

  echo "Number of artifacts doesn't match expectations"

  exit 1
fi

exit 0
