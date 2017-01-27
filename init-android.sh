#!/bin/bash

# Obtain the location of the bash script to figure out where the root of the repo is.
__ProjectRoot="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"

__Android_Cross_Dir="$__ProjectRoot/cross/android"
__NDK_Dir="$__Android_Cross_Dir/android-ndk-r13b"
__libunwind_Dir="$__Android_Cross_Dir/libunwind"
__lldb_Dir="$__Android_Cross_Dir/lldb"

# Download the NDK if required
if [ ! -d $__NDK_Dir ]; then
    echo Downloading the NDK into $__NDK_Dir
    wget -nv -nc https://dl.google.com/android/repository/android-ndk-r13b-linux-x86_64.zip -O $__Android_Cross_Dir/android-ndk-r13b-linux-x86_64.zip
    unzip $__Android_Cross_Dir/android-ndk-r13b-linux-x86_64.zip -d $__Android_Cross_Dir
fi

if [ ! -d $__lldb_Dir ]; then
    mkdir -p $__lldb_Dir
    echo Downloading LLDB into $__lldb_Dir
    wget -nv -nc https://dl.google.com/android/repository/lldb-2.3.3614996-linux-x86_64.zip -O $__Android_Cross_Dir/lldb-2.3.3614996-linux-x86_64.zip
    unzip $__Android_Cross_Dir/lldb-2.3.3614996-linux-x86_64.zip -d $__lldb_Dir
fi

# Create the RootFS for both arm64 as well as aarch
rm -rf $__Android_Cross_Dir/toolchain

echo Generating the arm64 toolchain
$__NDK_Dir/build/tools/make_standalone_toolchain.py --arch arm64 --api 23 --install-dir $__Android_Cross_Dir/toolchain/arm64/

# Install the required packages into the toolchain
rm -rf $__Android_Cross_Dir/deb/
rm -rf $__Android_Cross_Dir/tmp

for arch in "aarch64"
do
  mkdir -p $__Android_Cross_Dir/deb/
  mkdir -p $__Android_Cross_Dir/tmp/$arch/
  wget -nv -nc http://termux.net/dists/stable/main/binary-$arch/libicu_58.2_$arch.deb -O $__Android_Cross_Dir/deb/libicu_58.2_$arch.deb
  wget -nv -nc http://termux.net/dists/stable/main/binary-$arch/libicu-dev_58.2_$arch.deb -O $__Android_Cross_Dir/deb/libicu-dev_58.2_$arch.deb
  wget -nv -nc http://termux.net/dists/stable/main/binary-$arch/libuuid-dev_1.0.3_$arch.deb -O $__Android_Cross_Dir/deb/libuuid-dev_1.0.3_$arch.deb
  wget -nv -nc http://termux.net/dists/stable/main/binary-$arch/libuuid_1.0.3_$arch.deb -O $__Android_Cross_Dir/deb/libuuid_1.0.3_$arch.deb
  wget -nv -nc http://termux.net/dists/stable/main/binary-$arch/libandroid-glob-dev_0.3_$arch.deb -O $__Android_Cross_Dir/deb/libandroid-glob-dev_0.3_$arch.deb
  wget -nv -nc http://termux.net/dists/stable/main/binary-$arch/libandroid-glob_0.3_$arch.deb -O $__Android_Cross_Dir/deb/libandroid-glob_0.3_$arch.deb
  wget -nv -nc http://termux.net/dists/stable/main/binary-$arch/libandroid-support-dev_13.10_$arch.deb -O $__Android_Cross_Dir/deb/libandroid-support-dev_13.10_$arch.deb
  wget -nv -nc http://termux.net/dists/stable/main/binary-$arch/libandroid-support_13.10_$arch.deb -O $__Android_Cross_Dir/deb/libandroid-support_13.10_$arch.deb
  wget -nv -nc http://termux.net/dists/stable/main/binary-$arch/liblzma-dev_5.2.3_$arch.deb  -O $__Android_Cross_Dir/deb/liblzma-dev_5.2.3_$arch.deb
  wget -nv -nc http://termux.net/dists/stable/main/binary-$arch/liblzma_5.2.3_$arch.deb -O $__Android_Cross_Dir/deb/liblzma_5.2.3_$arch.deb
  wget -nv -nc http://termux.net/dists/stable/main/binary-$arch/libcurl-dev_7.52.1_$arch.deb -O $__Android_Cross_Dir/deb/libcurl-dev_7.52.1_$arch.deb
  wget -nv -nc http://termux.net/dists/stable/main/binary-$arch/libcurl_7.52.1_$arch.deb -O $__Android_Cross_Dir/deb/libcurl_7.52.1_$arch.deb

  echo Unpacking Termux packages
  dpkg -x $__Android_Cross_Dir/deb/libicu_58.2_$arch.deb $__Android_Cross_Dir/tmp/$arch/
  dpkg -x $__Android_Cross_Dir/deb/libicu-dev_58.2_$arch.deb $__Android_Cross_Dir/tmp/$arch/
  dpkg -x $__Android_Cross_Dir/deb/libuuid-dev_1.0.3_$arch.deb $__Android_Cross_Dir/tmp/$arch/
  dpkg -x $__Android_Cross_Dir/deb/libuuid_1.0.3_$arch.deb $__Android_Cross_Dir/tmp/$arch/
  dpkg -x $__Android_Cross_Dir/deb/libandroid-glob-dev_0.3_$arch.deb $__Android_Cross_Dir/tmp/$arch/
  dpkg -x $__Android_Cross_Dir/deb/libandroid-glob_0.3_$arch.deb $__Android_Cross_Dir/tmp/$arch/
  dpkg -x $__Android_Cross_Dir/deb/libandroid-support-dev_13.10_$arch.deb $__Android_Cross_Dir/tmp/$arch/
  dpkg -x $__Android_Cross_Dir/deb/libandroid-support_13.10_$arch.deb $__Android_Cross_Dir/tmp/$arch/
  dpkg -x $__Android_Cross_Dir/deb/liblzma-dev_5.2.3_$arch.deb $__Android_Cross_Dir/tmp/$arch/
  dpkg -x $__Android_Cross_Dir/deb/liblzma_5.2.3_$arch.deb $__Android_Cross_Dir/tmp/$arch/
  dpkg -x $__Android_Cross_Dir/deb/libcurl-dev_7.52.1_$arch.deb $__Android_Cross_Dir/tmp/$arch/
  dpkg -x $__Android_Cross_Dir/deb/libcurl_7.52.1_$arch.deb $__Android_Cross_Dir/tmp/$arch/
done

cp -R $__Android_Cross_Dir/tmp/aarch64/data/data/com.termux/files/usr/* $__Android_Cross_Dir/toolchain/arm64/sysroot/usr/

# Prepare libunwind
if [ ! -d $__libunwind_Dir ]; then
    git clone https://android.googlesource.com/platform/external/libunwind/ $__libunwind_Dir
fi

cd $__libunwind_Dir
git checkout android-6.0.0_r26
git checkout -- .
git clean -xfd

# libunwind is available on Android, but not included in the NDK.
echo Building libunwind
autoreconf --force -v --install 2> /dev/null
./configure CC=$__Android_Cross_Dir/toolchain/arm64/bin/aarch64-linux-android-clang --with-sysroot=$__Android_Cross_Dir/toolchain/arm64/sysroot --host=x86_64 --target=aarch64-eabi --disable-coredump --prefix=$__Android_Cross_Dir/toolchain/arm64/sysroot/usr 2> /dev/null
make > /dev/null
make install > /dev/null

# This header file is missing
cp include/libunwind.h $__Android_Cross_Dir/toolchain/arm64/sysroot/usr/include/

echo Now run:
echo CONFIG_DIR=\`realpath cross/android/arm64\` ROOTFS_DIR=\`realpath cross/android/toolchain/arm64/sysroot\` ./build.sh cross arm64 skipgenerateversion skipmscorlib cmakeargs -DENABLE_LLDBPLUGIN=0

echo To build corefx, assuming it is at ../corefx:
echo cd ../corefx
echo CONFIG_DIR=\`realpath ../coreclr/cross/android/arm64\` ROOTFS_DIR=\`realpath ../coreclr/cross/android/toolchain/arm64/sysroot\` ./build-native.sh -debug -buildArch=arm64 -- verbose cross

