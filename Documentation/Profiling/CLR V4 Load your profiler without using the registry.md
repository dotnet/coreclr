*Adapted from an entry that appeared on David Broman's blog*


One of the new features in CLR V4 is the ability to load your profiler without needing to register it first.  In V2, we would look at the following environment variables:

COR\_ENABLE\_PROFILING=1

COR\_PROFILER={_CLSID of profiler_}

and look up the CLSID from COR\_PROFILER in the registry to find the full path to your profiler's DLL.  Just like with any COM server DLL, we look for your profiler's CLSID under HKEY\_CLASSES\_ROOT, which merges the classes from HKLM and HKCU.

We mostly follow the same algorithm in V4, so you can continue registering your profiler if you wish.  However, in V4 we look for one more environment variable first:

COR\_PROFILER\_PATH=_full path to your profiler's DLL_

If that environment variable is present, we skip the registry look up altogether, and just use the path from COR\_PROFILER\_PATH to load your DLL.  A couple things to note about this:

- COR\_PROFILER\_PATH is purely optional.  If you don't specify COR\_PROFILER\_PATH, we use the old procedure of looking up your profiler's CLSID in the registry to find its path
- If you specify COR\_PROFILER\_PATH _and_ register your profiler, then COR\_PROFILER\_PATH always wins.  Even if COR\_PROFILER\_PATH points to an invalid path, we will still use COR\_PROFILER\_PATH, and just fail to load your profiler.
- COR\_PROFILER is _always required_.  If you specify COR\_PROFILER\_PATH, we skip the registry look up; however, we still need to know your profiler's CLSID, so we can pass it to your class factory's CreateInstance call.

