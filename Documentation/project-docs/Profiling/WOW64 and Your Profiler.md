*Adapted from an entry that appeared on David Broman's blog*


Has this ever happened to you?

_My profiler loads and runs great on my 32 bit box. But when I try to run it on a 64 bit box, it never loads. Shouldn't the WOW just make this all work?!_

When it comes to running your profilers on 64 bit boxes, there aren't any special rules dictated by the CLR that you must follow to get it to work. But if you're not careful, or you're unaware of how WOW64 works, it's easy to misconfigure your box.

## OS Redirectors

First off, get familiar with WOW64. MSDN has essential information [here](http://msdn.microsoft.com/library/en-us/winprog64/winprog64/running_32_bit_applications.asp) and in child topics. I'm not going to repeat all that information here, but I will mention a couple things to take note of.

The **registry redirector and file system redirector** create a logical view under which a 32 bit app executes, so that the 32 bit app feels like it is executing on a 32 bit machine. The registry redirector will show a subset of the registry to a 32 bit app, with some automatic remapping. An important example (there are more!):

32 bit app asks for "HKEY\_CLASSES\_ROOT\CLSID".  
32 bit app gets "HKEY\_CLASSES\_ROOT\Wow6432Node\CLSID"

This is how the system keeps 32-bit COM objects separate from 64-bit COM objects.

Similarly, file system paths get redirected to make the 32 bit app feel more at home. Example:

32 bit app asks for "%windir%\system32\Blah".  
32 bit app gets "%windir%\SysWow64\Blah"

There are exceptions to this, but that's the general rule. For those learning this for the first time, it's worthwhile to emphasize this point.

The 64 bit system tools and DLLs are in system32.  
The 32 bit system tools and DLLs are in SysWow64.

At first, this might seem a little reversed from what you'd expect, but you get used to it after a while.

When a 32 bit app starts up, its environment variables (particularly its PATH) are adjusted in a natural way so that the DLLs and other executables it wants are generally in the places it expects to find them.

## Register this

Let's put this knowledge into practice. What happens when you run regedit.exe? The answer is, which regedit.exe are you running? Ok, that's just another question. To answer _that_, from what environment are you running regedit.exe? Darn it, did it again, another question. Let's say you spawn cmd.exe from Start.Run. That gives you a 64 bit command prompt, from the _real_ %windir%\system32\cmd.exe (unless you do some wacky things to your path). That will give you the real 64 bit environment and paths. So if you type regedit.exe from that command prompt, you'll get the real 64 bit %windir%\system32\regedit.exe. That guy will show you the whole registry.

But you can also run a 32 bit command prompt from Start.Run via %windir%\SysWow64\cmd.exe. When you do _that_ and type "regedit.exe" you're gonna get the 32 bit %windir%\SysWow64\regedit.exe, which will show you the 32-bit logical view of the registry, with Registry Redirection in place.

Now what if, instead of running regedit.exe, you run regsvr32.exe? Same deal: depending on your environment, you'll either get the 32-bit or the 64-bit version of regsvr32.exe. And let's say you were running regsvr32.exe to register your profiler DLL. Well, you'll need to make sure the regsvr32 you run corresponds to the "bitness" of your profiler DLL. One of the WOW64 rules says that a 64 bit process cannot load 32 bit DLLs, and a 32 bit process cannot load 64 bit DLLs. So if the bitness of regsvr32 doesn't match the bitness of your profiler DLL, then regsvr32 will fail to LoadLibrary your DLL.

## Profilers: The magic ends with you

One of the nice features of the CLR is the option to go for platform independence. You can write a C# app, compile with csc.exe /platform:anycpu, and you get an executable with a pretty cool property. Slap that executable onto an x64 or ia64 machine (with the appropriate CLR installed), and it will run as a native 64-bit app in that machine's 64-bit architecture. Slap that same executable onto an x86 machine, and it will run as a native 32-bit app. For managed applications that don't make assumptions about bitness (e.g., that don't rely on the size of pointers or on native COM objects of a particular bitness being used), this is pretty nifty. See Josh William's informative [blog entry](http://blogs.msdn.com/joshwil/archive/2004/03/11/88280.aspx) for more information.

But profilers don't get to live in this magical world of chocolate fountains, lollipop skyscrapers, and dancing leprechauns. Profilers are native DLLs, and you have to explicitly choose whether to compile your profiler DLL for x86, x64, or ia64. And as you probably know, it's not a simple matter of swapping in a new compiler and saying, "go!". There's a fair amount of platform-specific code you need to write, depending on which features of the CLR Profiling API you use. Using the enter / leave / tailcall hooks is but one example. (See Jonathan Keljo's [blog entry](http://blogs.msdn.com/jkeljo/archive/2005/08/11/450506.aspx) on that to get an idea.) And of course there are the little things, too, like several CLR Profiling API types (e.g., FunctionIDs) becoming 64 bits long on 64 bit architectures. Anyway, it's out of the scope of this topic to talk about all the pitfalls you can fall into. But when you embark on compiling for multiple architectures, it should hopefully become pretty clear what changes you'll need to make.

Once you've decided all the architectures you wish to support, you need to provide a version of your DLL for each architecture, and register it appropriately. As a quick recap, here's an easy way to do the registration:

**x86 Machine**  
Just run regsvr32 like you usually do.

**x64 or ia64 Machine**

| If you want to be able to profile 64 bit apps, | run 64 bit regsvr32 against your 64 bit Profiler DLL |
| If you want to be able to profile 32 bit apps (WOW),   | run 32 bit regsvr32 against your 32 bit Profiler DLL. |
| If you want to support both, | do both! |

Note that, although you may not mix an EXE of one bitness with a DLL of another bitness, you may still _spawn_ child EXEs of any bitness you like from any parent EXE. So you can still author one installation program that copies and registers both 32 bit and 64 bit copies of your profiler DLL.

## COR\_PROFILER

Ok, so your profiler DLL is registered, but what about those pesky environment variables you're supposed to set in order to get your profiler DLL actually _loaded_ when the managed app runs? This part is easy. Microsoft generally recommends that, if you're creating a COM object and compiling for both 32 and 64 bits, just use the same CLSID in both versions. If you register both versions of the COM object on the same machine, then that same CLSID will show up in both branches of the registry:

under HKEY\_CLASSES\_ROOT\CLSID for 64 bit apps  
under HKEY\_CLASSES\_ROOT\Wow6432Node\CLSID for 32 bit apps

So if you just set COR\_PROFILER to your one and only CLSID, then you don't even need to know beforehand the bitness of the managed application that will be spawned from that environment. 64 bit apps will know to look for your CLSID under the 64 bit version of the CLSID hive, and will find the path to the 64 bit version of your profiler DLL. 32 bit apps running under the WOW will get redirected to the 32 bit version of the CLSID hive, and will find the path to the 32 bit version of your profiler DLL.

## Double your bits, double your pleasure

Hopefully this clears things up. Really, there's only one key thing to remember, which is to be aware of the various WOW64 redirectors. Once you understand how they work, it becomes clear how to register your profiler DLL, and where the managed app expects to find what you registered.

