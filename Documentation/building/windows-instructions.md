Build CoreCLR on Windows
========================

These instructions will lead you through building CoreCLR.

----------------
Environment

You must install several components to build the CoreCLR and CoreFX repos. These instructions were tested on Windows 7+.

## Visual Studio

Visual Studio must be installed. Supported versions:
- [Visual Studio 2015](https://www.visualstudio.com/downloads/visual-studio-2015-downloads-vs) (Community, Professional, Enterprise).  The community version is completely free.  

To debug managed code, ensure you have installed atleast [Visual Studio 2015 Update 3](https://www.visualstudio.com/en-us/news/releasenotes/vs2015-update3-vs).

Make sure that you install "VC++ Tools". By default, they will not be installed.

To build for Arm32, you need to have [Windows SDK for Windows 10](https://developer.microsoft.com/en-us/windows/downloads) installed. 

Visual Studio Express is not supported.

##CMake

The CoreCLR repo build has been validated using CMake 3.5.2. 

- Install [CMake](http://www.cmake.org/download) for Windows.
- Add it location (e.g. C:\Program Files (x86)\CMake\bin) to the PATH environment variable.  
  The installation script has a check box to do this, but you can do it yourself after the fact 
  following the instructions at [Adding to the Default PATH variable](#add-to-the-default-path-variable)
  

##Python

Python is used in the build system. We are currently using python 2.7.9, although
any recent (2.4+) version of Python should work, including Python 3.
- Install [Python](https://www.python.org/downloads/) for Windows.
- Add it location (e.g. AppData\Local\Programs\Python\Python35-32\python.exe) to the PATH environment variable.  
  The installation script has a check box to do this, but you can do it yourself after the fact 
  following the instructions at [Adding to the Default PATH variable](#add-to-the-default-path-variable)

##Git 

It turns out that you can mostly live without the Git command line utility git.exe because Visual Studio 2015
has enough GIT supported built in to handle most things.  The CoreCLR build also does not need git, but
the tests do use the Git command line utility, so in order to run tests you need Git installed.  You can
get it from 

- Install [Git For Windws](https://git-for-windows.github.io/)
- Add it location (e.g. C:\Program Files\Git\cmd\git.exe) to the PATH environment variable.  
  The installation script has a check box to do this, but you can do it yourself after the fact 
  following the instructions at [Adding to the Default PATH variable](#add-to-the-default-path-variable)

##PowerShell
PowerShell is used in the build system. Ensure that it is accessible via the PATH environment variable.
Typically this is %SYSTEMROOT%\System32\WindowsPowerShell\v1.0\.

Powershell version must be 3.0 or higher. This should be the case for Windows 8 and later builds.
- Windows 7 SP1 can install Powershell version 4 [here](https://www.microsoft.com/en-us/download/details.aspx?id=40855).

##DotNet Core SDK
While not strictly needed to build or tests the .NET Core repository, having the .NET Core SDK installed lets 
you use the dotnet.exe command to run .NET Core applications in the 'normal' way.   We use this in the 
[Using Your Build](Documentation/workflow/UsingYourBuild.md) instructions.  Visual Studio 2015 should have
installed the .NET Core SDK, but in case it did not you can get it from the [Intalling the .Net Core SDK](https://www.microsoft.com/net/core) page.  

##Adding to the default PATH variable

The commands above need to be on your command lookup path.   Some installers will automatically add them to 
the path as part of installation, but if not here is how you can do it.  

You can of course add a directory to the PATH enviroment variable with the syntax
```
    set PATH=%PATH%;DIRECTORY_TO_ADD_TO_PATH
```
However the change above will only last until the command windows closes.   You can make your change to
the PATH variable persistent by going to  Control Panel -> System And Security -> System -> Advanced system settings -> Environment Variables, 
and select the 'Path' variable in the 'System variables' (if you want to change it for all users) or 'User variables' (if you only want
to change it for the currnet user).  Simply edit the PATH variable's value and add the directory (with a semicolon separator).

-------------------------------------
#Building 

Once all the necessary tools are in place, building is trivial.  Simply run build build.cmd script that lives at
the base of the repository.   

```bat
    .\build 

	[Lots of build spew]

	Product binaries are available at C:\git\coreclr\bin\Product\Windows_NT.x64.debug
	Test binaries are available at C:\git\coreclr\bin\tests\Windows_NT.x64.debug
```

As shown above the product will be placed in 

- Product binaries will be dropped in `bin\Product\<OS>.<arch>.<flavor>` folder. 
- A NuGet package, Microsoft.Dotnet.CoreCLR, will be created under `bin\Product\<OS>.<arch>.<flavor>\.nuget` folder. 
- Test binaries will be dropped under `bin\Tests\<OS>.<arch>.<flavor>` folder

By default build generates a 'Debug' build type, that has extra checking (assert) compiled into it. You can
also build the 'release' version which does not have these checks

The build places logs in `bin\Logs` and these are useful when the build fails.

The build places all of its output in the `bin` directory, so if you remove that directory you can force a 
full rebuild.    

Build has a number of options that you can learn about using build -?.   Some of the more important options are

 * skiptests - don't build the tests.   This can shorten build times quite a bit, but means you can't run tests.
 * release - build the 'Release' build type that does not have extra development-time checking compiled in.
 * -rebuild - force the build not to be incremental but to recompile everything.   
 You wand this if you are doing to do performance testing on your build. 

See [Using Your Build](../workflow/UsingYourBuild.md) for instructions on running code with your build.  

See [Running Tests](../workflow/RunningTests.md) for instructions on running the tests.  

