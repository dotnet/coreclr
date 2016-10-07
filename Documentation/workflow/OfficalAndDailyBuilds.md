## Official Releases and Daily Builds of CoreCLR and CoreFX components

Roughly every three months, the .NET Runtime team publishes a new version of .NET Core to Nuget.   .NET Core's
official home is
 
 * <https://www.nuget.org/packages/Microsoft.NETCore.Runtime.CoreCLR/> 
 
and you can expect to see new versions roughly three months.   However it is also the case that the .NET 
Team publishes daily builds of all sorts of packages including those built by the CoreCLR and CoreFX repositories.
You can see what is available from

 * <https://dotnet.myget.org/gallery/dotnet-core>, and in particular you can see the builds of CoreCLR at 
 * <https://dotnet.myget.org/feed/dotnet-core/package/nuget/Microsoft.NETCore.Runtime.CoreCLR>.   
 
Thus if your goal is just to get the latest bug fixes and features, you don't need to build CoreCLR yourself you 
can simply add <https://dotnet.myget.org/F/dotnet-core/api/v3/index.json> to your Nuget Feed list.  The version 
numbers for packages 


Build Status
------------

|   | Debug | Release |
|---|:-----:|:-------:|
|**CentOS 7.1**|[![x64 status](https://img.shields.io/jenkins/s/http/dotnet-ci.cloudapp.net/job/dotnet_coreclr/job/master/debug_centos7.1.svg?label=x64)](http://dotnet-ci.cloudapp.net/job/dotnet_coreclr/job/master/job/debug_centos7.1)|[![x64 status](https://img.shields.io/jenkins/s/http/dotnet-ci.cloudapp.net/job/dotnet_coreclr/job/master/release_centos7.1.svg?label=x64)](http://dotnet-ci.cloudapp.net/job/dotnet_coreclr/job/master/job/release_centos7.1)|
|**Debian 8.4**|[![x64 status](https://img.shields.io/jenkins/s/http/dotnet-ci.cloudapp.net/job/dotnet_coreclr/job/master/debug_debian8.4.svg?label=x64)](http://dotnet-ci.cloudapp.net/job/dotnet_coreclr/job/master/job/debug_debian8.4)|[![x64 status](https://img.shields.io/jenkins/s/http/dotnet-ci.cloudapp.net/job/dotnet_coreclr/job/master/release_debian8.4.svg?label=x64)](http://dotnet-ci.cloudapp.net/job/dotnet_coreclr/job/master/job/release_debian8.4)|
|**FreeBSD 10.1**|[![x64 status](https://img.shields.io/jenkins/s/http/dotnet-ci.cloudapp.net/job/dotnet_coreclr/job/master/debug_freebsd.svg?label=x64)](http://dotnet-ci.cloudapp.net/job/dotnet_coreclr/job/master/job/debug_freebsd)|[![x64 status](https://img.shields.io/jenkins/s/http/dotnet-ci.cloudapp.net/job/dotnet_coreclr/job/master/release_freebsd.svg?label=x64)](http://dotnet-ci.cloudapp.net/job/dotnet_coreclr/job/master/job/release_freebsd)|
|**openSUSE 13.2**|[![x64 status](https://img.shields.io/jenkins/s/http/dotnet-ci.cloudapp.net/job/dotnet_coreclr/job/master/debug_opensuse13.2.svg?label=x64)](http://dotnet-ci.cloudapp.net/job/dotnet_coreclr/job/master/job/debug_opensuse13.2)|[![x64 status](https://img.shields.io/jenkins/s/http/dotnet-ci.cloudapp.net/job/dotnet_coreclr/job/master/release_opensuse13.2.svg?label=x64)](http://dotnet-ci.cloudapp.net/job/dotnet_coreclr/job/master/job/release_opensuse13.2)|
|**openSUSE 42.1**|[![x64 status](https://img.shields.io/jenkins/s/http/dotnet-ci.cloudapp.net/job/dotnet_coreclr/job/master/debug_opensuse42.1.svg?label=x64)](http://dotnet-ci.cloudapp.net/job/dotnet_coreclr/job/master/job/debug_opensuse42.1)|[![x64 status](https://img.shields.io/jenkins/s/http/dotnet-ci.cloudapp.net/job/dotnet_coreclr/job/master/release_opensuse42.1.svg?label=x64)](http://dotnet-ci.cloudapp.net/job/dotnet_coreclr/job/master/job/release_opensuse42.1)|
|**OS X 10.11**|[![x64 status](https://img.shields.io/jenkins/s/http/dotnet-ci.cloudapp.net/job/dotnet_coreclr/job/master/debug_osx.svg?label=x64)](http://dotnet-ci.cloudapp.net/job/dotnet_coreclr/job/master/job/debug_osx)|[![x64 status](https://img.shields.io/jenkins/s/http/dotnet-ci.cloudapp.net/job/dotnet_coreclr/job/master/release_osx.svg?label=x64)](http://dotnet-ci.cloudapp.net/job/dotnet_coreclr/job/master/job/release_osx)|
|**Red Hat 7.2**|[![x64 status](https://img.shields.io/jenkins/s/http/dotnet-ci.cloudapp.net/job/dotnet_coreclr/job/master/debug_rhel7.2.svg?label=x64)](http://dotnet-ci.cloudapp.net/job/dotnet_coreclr/job/master/job/debug_rhel7.2)|[![x64 status](https://img.shields.io/jenkins/s/http/dotnet-ci.cloudapp.net/job/dotnet_coreclr/job/master/release_rhel7.2.svg?label=x64)](http://dotnet-ci.cloudapp.net/job/dotnet_coreclr/job/master/job/release_rhel7.2)|
|**Fedora 23**|[![x64 status](https://img.shields.io/jenkins/s/http/dotnet-ci.cloudapp.net/job/dotnet_coreclr/job/master/debug_fedora23.svg?label=x64)](http://dotnet-ci.cloudapp.net/job/dotnet_coreclr/job/master/job/debug_fedora23)|[![x64 status](https://img.shields.io/jenkins/s/http/dotnet-ci.cloudapp.net/job/dotnet_coreclr/job/master/release_fedora23.svg?label=x64)](http://dotnet-ci.cloudapp.net/job/dotnet_coreclr/job/master/job/release_fedora23)|
|**Ubuntu 14.04**|[![x64 status](https://img.shields.io/jenkins/s/http/dotnet-ci.cloudapp.net/job/dotnet_coreclr/job/master/debug_ubuntu.svg?label=x64)](http://dotnet-ci.cloudapp.net/job/dotnet_coreclr/job/master/job/debug_ubuntu)|[![x64 status](https://img.shields.io/jenkins/s/http/dotnet-ci.cloudapp.net/job/dotnet_coreclr/job/master/release_ubuntu.svg?label=x64)](http://dotnet-ci.cloudapp.net/job/dotnet_coreclr/job/master/job/release_ubuntu)|
|**Ubuntu 16.04**|[![x64 status](https://img.shields.io/jenkins/s/http/dotnet-ci.cloudapp.net/job/dotnet_coreclr/job/master/debug_ubuntu16.04.svg?label=x64)](http://dotnet-ci.cloudapp.net/job/dotnet_coreclr/job/master/job/debug_ubuntu16.04)|[![x64 status](https://img.shields.io/jenkins/s/http/dotnet-ci.cloudapp.net/job/dotnet_coreclr/job/master/release_ubuntu16.04.svg?label=x64)](http://dotnet-ci.cloudapp.net/job/dotnet_coreclr/job/master/job/release_ubuntu16.04)|
|**Ubuntu 16.10**|[![x64 status](https://img.shields.io/jenkins/s/http/dotnet-ci.cloudapp.net/job/dotnet_coreclr/job/master/debug_ubuntu16.10.svg?label=x64)](http://dotnet-ci.cloudapp.net/job/dotnet_coreclr/job/master/job/debug_ubuntu16.10)|[![x64 status](https://img.shields.io/jenkins/s/http/dotnet-ci.cloudapp.net/job/dotnet_coreclr/job/master/release_ubuntu16.10.svg?label=x64)](http://dotnet-ci.cloudapp.net/job/dotnet_coreclr/job/master/job/release_ubuntu16.10)|
|**Windows 8.1**|[![x64 status](https://img.shields.io/jenkins/s/http/dotnet-ci.cloudapp.net/job/dotnet_coreclr/job/master/debug_windows_nt.svg?label=x64)](http://dotnet-ci.cloudapp.net/job/dotnet_coreclr/job/master/job/debug_windows_nt)<br/>[![arm64 status](https://img.shields.io/jenkins/s/http/dotnet-ci.cloudapp.net/job/dotnet_coreclr/job/master/arm64_cross_debug_windows_nt.svg?label=arm64)](http://dotnet-ci.cloudapp.net/job/dotnet_coreclr/job/master/job/arm64_cross_debug_windows_nt)|[![x64 status](https://img.shields.io/jenkins/s/http/dotnet-ci.cloudapp.net/job/dotnet_coreclr/job/master/release_windows_nt.svg?label=x64)](http://dotnet-ci.cloudapp.net/job/dotnet_coreclr/job/master/job/release_windows_nt)<br/>[![arm64 status](https://img.shields.io/jenkins/s/http/dotnet-ci.cloudapp.net/job/dotnet_coreclr/job/master/arm64_cross_release_windows_nt.svg?label=arm64)](http://dotnet-ci.cloudapp.net/job/dotnet_coreclr/job/master/job/arm64_cross_release_windows_nt)|
