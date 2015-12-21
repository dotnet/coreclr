Get .NET Core SDK on OS X
=========================

These instructions will lead you through acquiring the .NET Core SDK via the [.NET Core CLI toolset](https://github.com/dotnet/cli) and running a "Hello World" sample on Windows. 

These instructions are for .NET Core console apps. If you want to try out ASP.NET 5 on top of .NET Core - which is a great idea - check out the [ASP.NET 5 instructions](https://github.com/aspnet/home).

You can also [build from source](../building/osx-instructions.md). 

Installing .NET CLI toolset
===========================

There are two main ways to install the .NET CLI on OS X:

1. Using the PKG package  
2. Uzing the tarball (tar.gz file) for a "local install"

In this guide we will be using the PKG package. Simply download this file [from the URL](https://dotnetcli.blob.core.windows.net/dotnet/dev/Installers/Latest/dotnet-osx-x64.latest.pkg) and open it up from disk, then follow the prompts. This will install the .NET CLI toolset on your system. 

**Note:** we will be adding a [Homebrew]() package soon, so check out [.NET CLI repo](https://github.com/dotnet/cli) for more details. 

In order to test out if everything worked, you can run the `dotnet` command from the terminal:

``shell
dotnet
```
You should see the following output:

```shell
.NET Command Line Interface
Usage: dotnet [common-options] [command] [arguments]

Arguments:
  [command]     The command to execute
  [arguments]   Arguments to pass to the command

Common Options (passed before the command):
  -v|--verbose  Enable verbose output

Common Commands:
  new           Initialize a basic .NET project
  restore       Restore dependencies specified in the .NET project
  compile       Compiles a .NET project
  publish       Publishes a .NET project for deployment (including the runtime)
  run           Compiles and immediately executes a .NET project
  pack          Creates a NuGet package
```

Write your App
==============

You need a Hello World application to run. You can write your own, if you'd like, or you can use the `dotnet new` command to drop a predefined one that. 

In order to do that, we will first create a directory and then use `dotnet new` inside it:

```shell
mkdir testapp && cd testapp
dotnet new
```

Run your App
============

You need to restore packages for your app, based on your project.json, with `dotnet restore`.

	dotnet restore

You can run your app with the `dotnet run` command.

	dotnet run
    Hello World!

