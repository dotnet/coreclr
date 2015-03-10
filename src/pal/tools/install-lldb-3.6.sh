echo "deb http://llvm.org/apt/trusty/ llvm-toolchain-trusty-3.6 main" >> /etc/apt/sources.list.d/llvm.list
wget -O - http://llvm.org/apt/llvm-snapshot.gpg.key|sudo apt-key add -
apt-get update
apt-get install lldb-3.6 lldb-3.6-dev
