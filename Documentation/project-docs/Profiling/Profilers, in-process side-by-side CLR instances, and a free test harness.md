*Adapted from an entry that appeared on David Broman's blog*


My previous post on [New stuff in Profiling API for upcoming CLR 4.0](http://blogs.msdn.com/davbr/archive/2008/11/10/new-stuff-in-profiling-api-for-upcoming-clr-4-0.aspx) mentioned that any profiler that implements ICorProfilerCallback3 must be “side-by-side aware”.  This post goes into more detail on how to do this, and how to test it.

# What are in-process side-by-side CLR instances?

To understand this fully, take a look at this CLR Inside Out [article](http://msdn.microsoft.com/en-us/magazine/ee819091.aspx).  It will help you understand the “what” and the “why” around this feature.  The simple summary is that, in order to aid with compatibility in certain scenarios, a single process can now have multiple instances of the CLR loaded simultaneously.  What that means today is that, in one process, you can have a V4-based CLR and [either a V1.1-based or V2.0-based CLR (though not both)].  In the future, the possibilities will likely grow as more major versions of the CLR are released.

The CLR instances are unaware of each other.  If, say, a V2 and V4 CLR are loaded, then any managed code running against V2 CLR will look just like native code to the V4 CLR.  And vice-versa: any managed code running against the V4 CLR will look just like native code to the V2 CLR.  There is no direct communication between these two instances of the CLR.  What is possible is for, say, the V2 managed code to P/Invoke out to native code, which then calls a COM object implemented in V4 managed code.  In that way, one CLR can invoke another, but only in this indirect kind of way with native code in the middle.

To support in-process side-by-side CLR instances, the CLR team has extended the hosting interfaces via the new “metahost” interface.  The metahost interface provides a way to operate over multiple CLRs that may be loaded into a single process, with each CLR represented by an ICLRRuntimeInfo interface.  If you have implemented the “attach” feature for your profiler, then you are already familiar with the metahost interface.  You can find some profiler-specific information about metahost, along with sample code for implementing attach, in [this blog post](http://blogs.msdn.com/davbr/archive/2009/11/04/clr-v4-profiler-attach-basics-with-sample-code.aspx).

Again, I’d encourage you to read through the CLR Inside Out article linked above, as I don’t plan to repeat its content here.  That will give you context on why the feature of in-process side-by-side CLR instances even exists, and what problems it helps to solve.  What I will talk about is how this situation will appear to your profiler, and how your profiler can deal with it.

# Profiler’s Point of View

When multiple CLR instances are loaded into a single process, and those CLRs each load your profiler, then your profiler DLL will be loaded multiple times, once per CLR instance.  This means your DLL gets LoadLibrary’d multiple times and you’ll receive multiple “CreateInstance” calls to your class factory object.  Depending on how you code your “CreateInstance”, that could mean multiple instances of your ICorProfilerCallback implementation would be generated.

As you know, when the same DLL is LoadLibrary’d multiple times, it isn’t really “loaded” multiple times.  Windows just increments a reference count on that DLL (to be released via each FreeLibrary call).  Any global or static state in that DLL is shared across all code that executes in that DLL, regardless of how many LoadLibrary calls are made.  This means that, since your class factory’s CreateInstance() call could theoretically be called on two threads at the same time, any access CreateInstance() makes to globals in your DLL should be protected with synchronization primitives like a critical section.  Furthermore, if you allow multiple instances of your ICorProfilerCallback implementation to be created, then if they access any global or static class data, that access will need to be protected as well, if it isn’t already.

# Pick First, Pick One

The easiest way for your profiler to become side-by-side aware is to choose to profile only one CLR at a time, and to add code to enforce that.  This is fairly easy to do, and is a quite reasonable solution to the in-process side-by-side problem.  In fact, the Visual Studio 2010 profiler and the upcoming CLRProfiler V4 update currently choose this approach.

With **Pick First** , your class factory CreateInstance simply keeps track of whether it was already called.  First time through, it creates your ICorProfilerCallback implementation and succeeds.  Thereafter, it fails.  The advantage of this approach is that it’s the easiest to implement, and will always do what the user wants in scenarios where only one CLR is loaded.  The disadvantage is that, when multiple CLRs are loaded, although your profiler will operate just fine, the user may be upset if she was trying to profile the second CLR that got loaded, as your profiler provides no way to do that.

With **Pick One** , you provide some kind of UI to your user to specify which CLR to profile.  This could be fancy GUI, less-fancy command-line parameters, whatever you like.  The user would specify the CLR in terms of its version, and your profiler would refuse to profile any CLR that didn’t match that version.  While being only slightly more difficult to implement than Pick First, this ensures your user remains in control of what gets profiled.  With this approach, you’d always succeed your CreateInstance method, and then do the version checking inside your ICorProfilerCallback::Initialize() method, which you would succeed or fail, depending on whether your version check passes.  To do your version check, you would first QueryInterface for ICorProfilerInfo3.  If that fails, you know you’re dealing with a CLR based on 2.0 or earlier.  If that succeeds, the CLR is 4.0 or later.  You would then use ICorProfilerInfo3::GetRuntimeInformation to get the specific version information of the CLR to check against what the user selected.  Finally, you now know if you should succeed or fail ICorProfilerCallback::Initialize().

When you fail either your CreateInstance, ICorProfilerCallback::Initialize, or ICorProfilerCallback3::InitializeForAttach method due to an intentional choice not to profile that CLR (as opposed to encountering some kind of user-serviceable problem), I’d recommend you return the new “CORPROF\_E\_PROFILER\_CANCEL\_ACTIVATION” HRESULT. CORPROF\_E\_PROFILER\_CANCEL\_ACTIVATION is special in that the new CLR V4 will not log an error to the event log when it receives this HRESULT from the profiler’s CreateInstance or ICorProfilerCallback::Initialize method.  Instead, CLR V4 logs a less-alarming informational message to the event log stating that the profiler has intentionally chosen not to profile that CLR in that process.  Of course, in cases where you fail for an exceptional reason, you should continue to dutifully surface whatever HRESULT describes the problem, and let the CLR treat that as an error so that the user is properly informed of the problem.

# Pick Many, Pick All

You can provide more value to your users—particularly those who may be dealing with in-process side-by-side CLR scenarios more often—if you allow the user to profile multiple CLRs that may be loaded in a given process.  This approach would allow your profiler to provide the most information to the user, including capturing timings of all managed methods (regardless of the governing CLR), present interleaved call stacks including code from all runtimes, analyze all managed heaps from all runtimes, monitor the behavior of all managed code in the process, etc.  Doing this properly will require that you take care to synchronize, or in some cases remove entirely, global state from your DLL.

Just as a simple example, many profilers use global pointers that point to their ICorProfilerCallback implementation and / or the CLR’s ICorProfilerInfo implementation (e.g., say g\_pMyCallback, g\_pInfo).  This is no longer acceptable when there could be arbitrarily many of your ICorProfilerCallback implementations instantiated, and CLR’s ICorProfilerInfos lying around.  The key problem is this: if you pass an ID from one CLR to the ICorProfilerInfo of another CLR, you will crash.  Example: CLR #1 informs you about a FunctionID which you pass to CLR #2’s GetFunctionInfo().  Boom.  As many of you know, the CLR is intolerant about bogus IDs.  And from any given CLR’s point of view, an ID from a different CLR is garbage.

This means you must take care always to communicate with the appropriate CLR.  Thus, you’ll want to reevaluate any reliance your DLL has on global state, and protect or remove it appropriately.  In this section I’ll list some of the things to look out for, and recommended ways to address them.

## Global Profiler Manager

For the most part, communicating with the right CLR is straightforward.  Suppose multiple CLRs get loaded, and that forces multiple instances of your ICorProfilerCallback implementation to be created.  If:

- Each ICorProfilerCallback implementation keeps a pointer to the ICorProfilerInfo it was given at initialization time, and 
- Each ICorProfilerCallback implementation always uses this pointer to call into ICorProfilerInfo in response to callbacks 

then you’re mostly there.  Any Info calls you make in response to a callback will always be routed to the appropriate CLR.  Simple, right?  Yeah, but what about the ways your profiler gets control other than ICorProfilerCallback methods?  For example:

- Enter/Leave/Tailcall probes 
- Callouts you add via instrumentation 
- Separate threads you create for sampling, forcing GCs or other reasons 

An approach I’d recommend is that you create a single-instance, global profiler manager, which gets invoked in the above cases and “figures out” which ICorProfilerCallback implementation (and thus which ICorProfilerInfo pointer) to route the request to.

[![image](media/MSDNBlogsFS/prod.evol.blogs.msdn.com/CommunityServer.Blogs.Components.WeblogFiles/00/00/00/53/47/metablogapi/8715.image_thumb_01A0D243.png "image")](media/MSDNBlogsFS/prod.evol.blogs.msdn.com/CommunityServer.Blogs.Components.WeblogFiles/00/00/00/53/47/metablogapi/2110.image_051F632D.png)

 

Diagrams are spiffy.  But the interesting part is how the single global profiler manager figures out which ICorProfilerCallback implementation to talk to.  A diagram that just shows arrows doesn’t really help explain that.  The following sections address this.

## Enter / Leave / Tailcall / FunctionIDMapper

Since Enter, Leave, Tailcall, and FunctionIDMapper are implemented as global C functions, they’re technically part of your global profiler manager.  So they must somehow figure out which CLR invoked them.  The key to this is a new parameter added to the new V4 SetFunctionIDMapper2:

```
HRESULT SetFunctionIDMapper2( [in] FunctionIDMapper2 \*pFunc, [in] void \*clientData);
```

clientData can be anything you like, though typically it will be a pointer to your ICorProfilerCallback implementation instance that makes the call SetFunctionIDMapper2.  Then, when your mapper function gets called:

```
typedef UINT\_PTR \_\_stdcall FunctionIDMapper2( FunctionID funcId,  void \*clientData, BOOL \*pbHookFunction);
```

the CLR passes that clientData right back to you.  Now your global profiler manager can associate this FunctionID with the correct ICorProfilerCallback implementation.  You can store this association in a hash table or use the FunctionIDMapper as it was intended, and return an ID of your own, typically a index into an array you build up which would contain the correct ICorProfilerCallback implementation (as well as the FunctionID you remapped), rather than using a hash table.

Now, the next time your Enter/Leave/Tailcall probes are called, your global profiler manager will be able to map the FunctionID provided (or, your remapped client ID assuming your FunctionIDMapper returned one), to the appropriate ICorProfilerCallback implementation.

## Instrumentation

Some profilers rewrite IL to call into a managed helper library that ships with the profiler, and that managed library may then P/Invoke back into the native profiler code.  In such cases, how can the native profiler code know which CLR instance did the P/Invoke?  The target of the P/Invoke would likely be your global profiler manager, which then needs to determine which ICorProfilerCallback implementation to route the call to.  This knowledge is required if the native profiler code needs to call any ICorProfilerInfo methods to do further inspection on any of the parameters the managed helper library passed in the P/Invoke.  So how does the global profiler manager figure out the right CLR?

One way is to take advantage of a new method in the .NET Framework, [System.Runtime.InteropService.RuntimeEnvironment.GetRuntimeInterfaceAsObject](http://msdn.microsoft.com/en-us/library/system.runtime.interopservices.runtimeenvironment.getruntimeinterfaceasobject.aspx).  That returns the ICLRRuntimeInfo (i.e., the interface metahost uses to describe a given CLR version’s instance, as mentioned above) for the CLR instance that managed the calling code.  If your managed helper library passes that ICLRRuntimeInfo pointer to your native global profiler manager, then your native code can use that ICLRRuntimeInfo to determine which CLR version did the P/Invoke, and thus which ICorProfilerCallback implementation to route that call to.

Another option is that you can do version-specific instrumentation.  When your profiler receives JITCompilationStarted and then calls SetILFunctionBody, your profiler knows which CLR is managing that particular method (because you receive these notifications on the appropriate ICorProfilerCallback interface).  Your profiler could then add specific markers to the instrumented code (e.g., adding integer constants like 1 or 2 or 4 to indicate the CLR version, or really any other plan you can think up).  Then, when the instrumented code gets invoked, it can pass your special values to the P/Invoke, which your global profiler manager can inspect to determine which CLR instance was in control.

## DoStackSnapshot

If you’re writing a sampling profiler, or any profiler that needs to occasionally take snapshots of the stack (without building a shadow stack via Enter/Leave/Tailcall), then you use the DoStackSnapshot (DSS) API.  If your profiler implements the “Pick Many” or “Pick All” approach, then it can provide your users with the advantage of seeing more complete stacks, including managed code from all runtimes.  Remember that code managed by one CLR looks like native code to another CLR.  So by having your profiler simultaneously load against all runtimes, the profiler can then provide the most complete view of the stack.  Otherwise, chains of frames managed by a CLR the profiler is not loaded against would look like native code, with the profiler unable to report anything useful about those frames (such as function names).

[![image](media/MSDNBlogsFS/prod.evol.blogs.msdn.com/CommunityServer.Blogs.Components.WeblogFiles/00/00/00/53/47/metablogapi/8715.image1_thumb_38118445.png "image")](media/MSDNBlogsFS/prod.evol.blogs.msdn.com/CommunityServer.Blogs.Components.WeblogFiles/00/00/00/53/47/metablogapi/4276.image1_31CAADB7.png)

A pick-all, “mixed-mode” (i.e., native + managed) profiler can assemble the frames managed by various CLRs, along with native frames, into a single stack view by using an algorithm like the following.  (Note that a complete algorithm for doing a mixed-mode stack walk is out of scope of this post.  More information on mixed-mode stack walking can be found [here](http://blogs.msdn.com/davbr/archive/2005/10/06/profiler-stack-walking-basics-and-beyond.aspx).)

1. Global Profiler Manager begins an unmanaged walk of the stack starting at its thread’s current register context. 
2. Global Profiler Manager cycles through all ICorProfilerCallback implementations, having each one call into its CLR’s ICorProfilerInfo::GetFunctionFromIP with the IP of that stack frame.  
  - Note: I mentioned above that you should never pass a profiler ID (e.g., FunctionID, ClassID) to the wrong CLR’s ICorProfilerInfo, as that will easily cause an AV.  However, it is always safe to pass any IP address to GetFunctionFromIP(). 
3. If ICorProfilerInfo::GetFunctionFromIP succeeds with a FunctionID, that means this frame is managed, and you’ve found the CLR that manages it.  You may call DoStackSnapshot from this CLR’s ICorProfilerInfo2 to perform a complete stack walk starting at this frame.  **See below for details.** 
4. If ICorProfilerInfo::GetFunctionFromIP fails, continue cycling through the other CLRs’ ICorProfilerInfo::GetFunctionFromIP until you find one that works. 
5. If none of the GetFunctionFromIP calls succeed, then this really is a native frame.  Your Global Profiler Manager will need to use whatever native stack walking techniques you have to identify the frame, and then walk past it to the calling frame, and go back to step 2. 

Step 3 above occurs when your Global Profiler Manager finds a frame managed by a particular CLR, and has the corresponding profiler instance call DoStackSnapshot (on the corresponding CLR), seeded with that frame, to perform a walk from that point.  Your Global Profiler Manager will effectively repeat the above algorithm recursively, inside native blocks reported by that DoStackSnapshot.  Here are the details:

- You now have a view of the stack with information from all frames managed by this CLR. 
- All frames _not_ managed by this CLR appear as blocks of native frames.  Some of these frames really are native, and some are managed by a different CLR. 
- For each block of native frames, repeat the algorithm above (i.e., calling GetFunctionFromIP / DoStackSnapshot from each CLR to find the CLR that manages it (if any), or to walk to the next frame and retry otherwise). 
- Stitch together the frames from all CLRs, using SP as your guide on where each frame should be sorted. 

By the time you’re done, any frame for which you were unable to find a CLR that manages it really is native.  The rest of the frames are managed, and you should now have information from the appropriate CLR to identify them.

A pick-all, managed-only profiler has a simpler job:

1. Global Profiler Manager cycles through all ICorProfilerCallback implementations, having each one call into its CLR’s DoStackSnapshot to perform an unseeded walk. 
2. Global Profiler Manager stitches together all managed frames found by the above walks using SP as its guide on where each frame should be sorted. 

# Side-by-side and Profiler Backward Compatibility

Now that I’ve covered how a V4 profiler can properly support in-process side-by-side CLR instances, you may be wondering what happens if an older V2 profiler encounters multiple CLRs.  As I covered in a previous [post](http://blogs.msdn.com/davbr/archive/2009/05/26/run-your-v2-profiler-binary-on-clr-v4.aspx), V2 profilers will not even be loaded by V4 CLR by default—but users may set the “COMPLUS\_ProfAPI\_ProfilerCompatibilitySetting” environment variable to allow a V2 profiler to be loaded by a V4 CLR.  What happens then?

The V4 CLR attempts some low-cost heroics to try to shield the V2 profiler from pain caused by in-process side-by-side CLR instances.  However, it’s far from perfect.  As I mentioned from that previous [post](http://blogs.msdn.com/davbr/archive/2009/05/26/run-your-v2-profiler-binary-on-clr-v4.aspx), COMPLUS\_ProfAPI\_ProfilerCompatibilitySetting may be set to one of the following three values: EnableV2Profiler, DisableV2Profiler (default), and PreventLoad.  In fact, I mentioned that “PreventLoad” would be explained in more detail in a future post.  Well, this is that post.  And it’s only a year later.  Yowza, time flies.

The quick summary of the “low cost heroics” is that, if V4 CLR detects that V2 CLR has already been loaded, then V4 CLR will protectively refuse to load the V2 profiler (since it was already loaded by the V2 CLR).  The caveat with this plan is that it doesn’t work so well when the CLRs are loaded in the other order.  If V4 CLR loads first, it has no idea if a V2 CLR will ever load.  So it optimistically loads the V2 profiler and hopes for the best.  If a V2 CLR does load later, then the V2 profiler will likely fail in some horrible AV’ish kind of way.

It’s also worth noting how the V4 CLR decides whether a profiler is a V2 or V4 profiler in the first place.  The V4 CLR will QI for ICorProfilerCallback3.  If it works, it’s a V4 profiler; else it’s a V2 (or even older) profiler.  This means the V4 CLR actually has to LoadLibrary your DLL, use your class factory to create an instance of your ICorProfilerCallback implementation, and then QI for ICorProfilerCallback3.

The following table details the behavior of whether and how a V2 profiler gets loaded depending on the setting of COMPLUS\_ProfAPI\_ProfilerCompatibilitySetting, and the order in which the CLRs get loaded.

| ProfilerCompatibilitySetting | CLR Load Order | Result |
| EnableV2Profiler | V2, V4 | V2 loads profiler, V4 does not load profiler. |
| EnableV2Profiler | V4, V2 | V4 loads profiler, V2 loads profiler (profiler will likely AV due to active use of multiple callback instances) |
| DisableV2Profiler (default) | V2, V4 | V2 loads profiler, V4 queries then releases the V2 profiler interface but never unloads the profiler DLL (profiler may possibly AV on V4 instantiation) |
| DisableV2Profiler (default) | V4, V2 | V4 queries then releases the profiler interface but never unloads the profiler DLL, V2 loads the profiler. |
| PreventLoad | V2, V4 | V2 loads profiler, V4 does not load profiler. |
| PreventLoad | V4, V2 | V4 does not load profiler, V2 loads profiler. |

We’re now in a better position to see the point of setting COMPLUS\_ProfAPI\_ProfilerCompatibilitySetting= **PreventLoad**.  If a user is encountering a scenario with in-process side-by-side CLR instances, particularly where V4 CLR loads first, then a V2 profiler is likely to AV.  PreventLoad tells V4 CLR not to load any profilers whatsoever, regardless of whatever version they happen to be.  Of course, V2 CLR totally ignores COMPLUS\_ProfAPI\_ProfilerCompatibilitySetting (since that environment variable appeared after CLR V2 shipped!), so V2 CLR will happily load the V2 profiler.  Thus, PreventLoad allows the user to use a V2 profiler to profile the V2 CLR, without allowing a V4 CLR to spoil the fun.

# Free Test Harness

In-process side-by-side scenarios may be hard to test, so we have a harness you can use that will force multiple CLRs to get loaded:

Download [RunSxS](http://code.msdn.microsoft.com/RunSxS).

RunSxS has many options to customize its behavior, though you’ll probably want to start with something simple.  Here’s an example sequence of steps you can try out:

1. Open an ([elevated], if necessary) command prompt of the appropriate bitness. 
2. Register your profiler, if necessary. 
3. Set the usual environment variables, including COR\_ENABLE\_PROFILING, COR\_PROFILER, and optionally COR\_PROFILER\_PATH. 
4. You do not need to set COMPLUS\_ProfAPI\_ProfilerCompatibilitySetting unless you’re trying to test out your old V2 profiler. 
5. Have some sample V2 CLR and V4 CLR applications handy (we’ll call them AppV2.exe and AppV4.exe). 
6. Execute some of the RunSxS command-lines below. 

Here are some RunSxS command-lines to try out.  This one deterministically loads V2 CLR (and thus your V2 profiler), and then V4 CLR (and thus your V4 profiler):

- RunSxS /st v2.0.50727 c:\Path\To\AppV2.exe "appv2arg1 appv2arg2" v4.0.30319 c:\Path\To\AppV4.exe "appv4arg1 appv4arg2" 

Now, reverse the order:

- RunSxS /st v4.0.30319 c:\Path\To\AppV4.exe "appv4arg1 appv4arg2" v2.0.50727 c:\Path\To\AppV2.exe "appv2arg1 appv2arg2" 

This one will simultaneously launch a V2 & V4 app on separate threads, so you can see how your profiler fares with multi-threaded loading.  Try to catch some nondeterministic bugs:

- RunSxS v2.0.50727 c:\Path\To\AppV2.exe "appv2arg1 appv2arg2" v4.0.30319 c:\Path\To\AppV4.exe "appv4arg1 appv4arg2" 

# Is it over yet?!

Yes.  Yes it is.  To recap:

- In order to say your profiler works with V4 CLR, your profiler must be side-by-side-aware, which means it must support pick-first/one or pick-many/all. 
- The latter is harder to implement, but provides your user with the most information when multiple CLRs are loaded.  
  - Consider factoring your code so you have a single, global profiler manager, that is distinct from your (multiple) callback implementation instances. 
  - Enter/Leave/Tailcall, instrumentation, and stack walking have special considerations 
- Older, V2 profilers will probably have issues if multiple CLRs are loaded, though the V4 CLR half-heartedly tries to protect those older profilers. 
- Test, test, test! 

Thanks to Shane Yuan for much of the content and illustrations in this post!

