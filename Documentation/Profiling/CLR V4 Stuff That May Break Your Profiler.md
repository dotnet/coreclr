*Adapted from an entry that appeared on David Broman's blog*


When CLR V2 came out, we made a big decision.  If your profiler has not been upgraded for V2 (i.e., if your profiler does not support ICorProfilerCallback2), then the CLR will not allow your profiler to run.  Why?  CLR V2 had some radical differences from V1.  Generics is a great example.  There were also some serious changes to the class loader, NGEN, and more.  A profiler that wasn’t designed for CLR V2 was more likely to fail than succeed.

CLR V4 is a different story.  There are certainly changes in V4 that may break older profilers (that’s what this post is all about!) but we made a bet that V2 profilers were more likely to _succeed_ than fail when run against V4.  So we’ve decided to allow V2 profilers to be loaded by CLR V4.  Below are the caveats profiler writers need to be aware of when allowing their older profilers to run against V4.

You should also read this post another way.  When you do refurbish your profiler to work against V4, you need to get the following right.

The following changes are organized into the following sections:

- The Big Ones 
- Profiling Infrastructure 
- Loader / DLLs 
- Type System 
- Security 
- Exception Handling 

# The Big Ones

In this section are the big caveats you need to know about.  I wanted to put them up front so you will have read them by the time you inevitably fall asleep midway through the post:

- No Love By Default 
- In-Process Side-by-Side CLR Instances 

## No Love by Default

As I mentioned in a previous post, although you _can_ load an older profiler into V4, you can’t do so by default.  Because of the caveats of running an older profiler against CLR V4, the profiler user must opt in to this by setting the **COMPLUS\_ProfAPI\_ProfilerCompatibilitySetting** environment variable appropriately.  See this [post](http://blogs.msdn.com/davbr/archive/2009/05/26/run-your-v2-profiler-binary-on-clr-v4.aspx) for more information.

It’s worth stressing that this environment variable does _not_ turn on some kind of compatibility _mode_ that protects an older profiler from all the changes in V4.  This environment variable causes the CLR to add minimal protection for older profilers from just a couple changes, such as multiple in-process side-by-side CLR instances (see below).  But the vast majority of the changes mentioned in this post will affect the profiler as-is, and there is no protection from them.  So it’s best to update your profiler to work against V4 natively without needing to use the **COMPLUS\_ProfAPI\_ProfilerCompatibilitySetting** environment variable.

## In-Process Side-by-Side CLR Instances

CLR V4 can be loaded alongside other versions of the CLR, all living together in the same process.  For now, that means you can have one CLR V4 instance plus one CLR V1.1 **or** V2 instance all running in the same process.  In future releases, the list of possibilities will grow (e.g., V2, V4, V5, V6, etc. running together).  This will result in multiple instances of your callback object being created and used simultaneously.  This sort of thing is sure to mess with your profiler’s mind, particularly due to any global state you might have in your profiler DLL, or any communication you do between your profiler DLL and an out-of-process GUI shell.  There will be blog entries and MSDN topics on this in the future to give you more information, though for now you can take a look here:

[http://blogs.msdn.com/davbr/archive/2008/11/10/new-stuff-in-profiling-api-for-upcoming-clr-4-0.aspx](http://blogs.msdn.com/davbr/archive/2008/11/10/new-stuff-in-profiling-api-for-upcoming-clr-4-0.aspx "http://blogs.msdn.com/davbr/archive/2008/11/10/new-stuff-in-profiling-api-for-upcoming-clr-4-0.aspx")

# Profiling Infrastructure

In this section are changes to the profiling API and infrastructure itself that may impact your profiler:

- FreeLibrary 
- Enter/Leave/Tailcall 
- CORPROF\_E\_UNSUPPORTED\_CALL\_SEQUENCE 
- SetILInstrumentedCodeMap 

## FreeLibrary

In CLR V1 and V2, your profiler DLL would never be explicitly unloaded by the CLR.  In V4, however, the CLR will FreeLibrary your profiler DLL if your profiler chooses to detach before process shutdown.  This may be an issue if your product is split into DLLs with various interdependencies between them and your profiler DLL.  Although this won’t affect older profilers, as they cannot request to detach before process shutdown, this is something to be wary of when you upgrade your profiler, if you choose to take advantage of the Detach feature.

## Enter/Leave/Tailcall

### 

### New Signatures

We have made some enhancements to the Enter/Leave/Tailcall interface to cut down on code size of the JITted / NGENd code.  This required updating the signatures.  While the older signatures still work (Enter/Leave/Tailcall and Enter2/Leave2/Tailcall2), they must now go through another code layer before reaching your profiler (even if you were using fast-path before).  This means you will see some slow-down until you upgrade to the new V4 signatures.  Fast-path (i.e., calling from JITted/NGENd code directly into your profiler) does still exist; you just need to use the latest signatures to enable fast-path.

An artifact of this change is that your profiler will use different Info methods to install your hooks, depending on whether you want to inspect values in your probes (SetEnterLeaveFunctionHooks3 vs. SetEnterLeaveFunctionHooks3WithInfo).  To help ensure correctness, the CLR will not allow the profiler to install enter/leave/tailcall probes that are inconsistent with the COR\_PRF\_MONITOR flags you have specified to SetEventMask.  COR\_PRF\_ENABLE\_FUNCTION\_ARGS, COR\_PRF\_ENABLE\_FUNCTION\_RETVAL, and COR\_PRF\_ENABLE\_FRAME\_INFO require SetEnterLeaveFunctionHooks3 **WithInfo** , and if those flags are not set, then you must use SetEnterLeaveFunctionHooks3.

So you must be sure to call SetEventMask _first_, with the appropriate flags to establish whether you want to inspect arguments, return value, or frame information.  Then, you may call SetEnterLeaveFunctionHooks3 or SetEnterLeaveFunctionHooks3WithInfo _second_, so the CLR can verify you’re using probes that match the event flags you specified.  If you call SetEventMask and SetEnterLeaveFunctionHooks3(WithInfo) in the wrong order, or with inconsistent information, then the CLR will fail with CORPROF\_E\_INCONSISTENT\_WITH\_FLAGS.

### Placement of the Calls

There was another change to the Enter/Leave/Tailcall interface; this one on x86 regarding the placement of the call to the Enter probe relative to the prolog.  In V2, the order was:

Enter   
Prolog   
Leave   
Epilog

The above does not have mirror symmetry, and is also inconsistent with how the probes are called on the 64 bit platforms.  In V4, the order is now:

Prolog   
Enter   
Leave   
Epilog

This could break your profiler if it makes assumptions about the value of ESP during its Enter probe, and tries to find values on the stack based on those assumptions.  Of course, if your profiler does things the “proper” way and relies on the Enter/Leave/Tailcall interface to locate items such as argument values, then you’re fine.

## CORPROF\_E\_UNSUPPORTED\_CALL\_SEQUENCE

As you may recall from this [post](http://blogs.msdn.com/davbr/archive/2008/12/23/why-we-have-corprof-e-unsupported-call-sequence.aspx), CLR V2 will fail some ICorProfilerInfo\* calls with CORPROF\_E\_UNSUPPORTED\_CALL\_SEQUENCE if they are called asynchronously.  In V4, the CLR performs an additional check, related to GC safety, to ensure a given ICorProfilerInfo\* call is safe.  If the call is determined to be unsafe, then the call will fail with CORPROF\_E\_UNSUPPORTED\_CALL\_SEQUENCE.  Here’s how the check works.  Some ICorProfilerCallback\* methods are “unsafe for GC”, in that they cannot deal with a GC occurring while they’re on the stack.  Meanwhile, some ICorProfilerInfo\* calls may trigger a GC.  The problem lies in the intersection—if one of those “unsafe for GC” callback methods calls into your profiler, and then your profiler turns around and calls one of those “may trigger a GC” info methods, then that info method will detect the dangerous calls sequence and fail immediately with CORPROF\_E\_UNSUPPORTED\_CALL\_SEQUENCE.

| Unsafe-for-GC Callbacks | May-trigger-GC Infos |
| ThreadAssignedToOSThread   
ExceptionUnwindFunctionEnter   
ExceptionUnwindFunctionLeave   
ExceptionUnwindFinallyEnter   
ExceptionUnwindFinallyLeave   
ExceptionCatcherEnter   
RuntimeSuspendStarted   
RuntimeSuspendFinished   
RuntimeSuspendAborted   
RuntimeThreadSuspended   
RuntimeThreadResumed   
MovedReferences   
ObjectsAllocatedByClass   
ObjectReferences   
RootReferences(2)   
HandleCreated   
HandleDestroyed   
GarbageCollectionStarted   
GarbageCollectionFinished | GetILFunctionBodyAllocator   
SetILFunctionBody   
SetILInstrumentedCodeMap   
ForceGC   
GetAppDomainsContainingModule   
GetClassFromToken   
GetClassFromTokenAndTypeArgs   
GetFunctionFromTokenAndTypeArgs   
GetAppDomainInfo   
EnumModules   
RequestProfilerDetach

 

 |

## SetILInstrumentedCodeMap

Before CLR V4, if the profiler were to delete the map it passed to SetILInstrumentedCodeMap, that could cause AVs later on.  That was a CLR bug, as COM memory management conventions state that [in] parameters are allocated and deallocated by the caller (callee must make a copy if it wants to use the memory later).  This has been fixed in CLR V4, so that profilers should free the instrumented code map after calling SetILInstrumentedCodeMap, and that will no longer cause AVs.  If your profiler is not updated to delete this memory when run against CLR V4, then that will cause a memory leak--though that’s the worst that will happen (you won’t cause an AV by failing to delete memory, of course).

# Loader / DLLs

In this section are changes to how the CLR loads assemblies, as well as the DLLs that make up the CLR itself:

- Dynamic Module Names 
- DLL Name Changes 
- MSCOREE’s Exported Hosting Functions 

## Dynamic Module Names

In CLR V2, a call to GetModuleInfo would sometimes return an empty name.  In fact, some profilers may have used the base load address and name [out] parameters from GetModuleInfo in order to infer some details about the module in a rather indirect fashion.  In particular:

- In V2, any module created via Reflection.Emit will have a base load address of 0. 
- In V2, any module loaded directly by the CLR from disk will have a non-empty Name (and the Name will be the disk path).  All other modules will have an empty Name. 
  - Modules loaded from disk include: [name non-empty in V2] 
    - Any module loaded via fusion to facilitate execution.  (i.e., normal stuff) 
    - Any reflection-only-context module loaded from disk (Assembly.ReflectionOnlyLoadFrom) 
  - Modules NOT loaded from disk include: [name empty in V2] 
    - RefEmit-generated modules (AppDomain.DefineDynamicAssembly) 
    - Modules loaded from byte arrays (Assembly.Load) 
    - Reflection-only context modules loaded from byte arrays (Assembly.ReflectionOnlyLoad) 
    - Managed SQL modules, and any other host that overrides the module loading mechanism 

In CLR V4, although the behavior of the base load address is not changing, you will now see a non-empty Name, even for non-disk modules!  We will provide the module's "metadata name" in the Name parameter, in the case where it has no disk path.  By “metadata name”, I’m referring to the “Name” column from the Module table inside metadata.  This is also exposed as Module.ScopeName to managed code, and as IMetaDataImport::GetScopeProps’s szName parameter to unmanaged metadata client code.

For profilers that really need to distinguish between regular disk modules, RefEmit-generated modules, byte array modules, etc., we are introducing **GetModuleInfo2** in CLR V4.  It has identical behavior to the V4 GetModuleInfo; however, it adds one more [out] parameter that will be filled with bit flags that describe various properties of the module.  Take a look at the corprof.idl that comes with the beta 2 installation to see the goodies.

## DLL Name Changes

Mscorwks.dll has been renamed to clr.dll.  Mscordbc.dll, which used to contain profiling-specific functionality, has disappeared (all of its functionality is now present in clr.dll). Generally, this should not affect you, unless your profiler takes dependencies on the names of these DLLs.  You will be affected, however, while debugging if you use SOS.  Instead of using “.loadby sos mscorwks” you’ll now need to use “.loadby sos clr”.  Bonus: Less characters to type!

## MSCOREE’s Exported Hosting Functions

Some profilers use C exports from mscoree.dll, typically one or more of the [Hosting Global Static Functions](http://msdn.microsoft.com/en-us/library/aa964945%28VS.100%29.aspx).  These exports are almost all deprecated in CLR V4 (except for [CLRCreateInstance](http://msdn.microsoft.com/en-us/library/dd537633(VS.100).aspx)).  CLR V4 has also deprecated the use of CoCreateInstance to instantiate COM objects used for hosting the CLR, such as CLSID\_CorRuntimeHost, CLSID\_CLRRuntimeHost, CLSID\_TypeNameFactory, CLSID\_ComCallUnmarshal, etc.

For compatibility reasons, the deprecated exports will by default execute in a context capped to CLR V2.  For example, calling CorBindToRuntime with a NULL pwszVersion, which historically would bind you to the newest runtime installed on the box, will now bind you to the newest runtime _below V4_.  So if both V2 & V4 are installed on the box, CorBindToRuntime with a NULL pwszVersion will bind you to V2.  As another example, if both V2 & V4 runtimes are loaded into a process, GetRealProcAddress will locate the specified function in the V2 runtime.  And if only V4 is loaded into a process, GetRealProcAddress can load the V2 CLR into the process if it is installed.  Similarly, attempting to CoCreateInstance one of the CLR-hosting COM objects will by default cause the CLR V2 to load.

The reason that the exports from mscoree.dll are now capped to V2 is to ensure that, when a user installs CLR V4, that installation is non-impactful to older, CLR V2-based applications.  For example, a CLR V1.1 managed application on a machine with only CLR V2 installed would roll forward to "the latest" runtime on the box, which happened to be CLR V2.  We want to preserve this behavior even after CLR V4 is installed (i.e., the 1.1 app without a CLR 1.1 on the box should continue to bind against CLR V2, and not CLR V4).  As another example, we want to minimize damage to any old native hosts that were built against older runtimes, but that might end up running inside processes containing managed code that binds to CLR V4 (due to in-process side-by-side CLR instances).

Although there are ways to modify this behavior away from the default via configuration files, it is recommended that profilers (and hosts!) stop using C exports from mscoree.dll.  Instead, profilers that target CLR V4 should be upgraded to use the new [CLR V4 Hosting and Metahost interfaces](http://msdn.microsoft.com/en-us/library/dd233134(VS.100).aspx) wherever they had been using mscoree exports.

# Type System

In this section are changes related to how the CLR loads and manages type information:

- Collectible Assemblies 
- Type Forwarding 
- GetClassLayout and Value Type Size 
- No More Frozen Strings 
- String Layout 

## Collectible Assemblies

CLR V4 introduces the new feature Collectible Assemblies.  The quick summary of this feature is that it allows the developer to mark a Reflection.Emit-generated assembly as being collectible, and that will tell the GC to collect the assembly, as well as its modules, classes, and code, once they are no longer referenced.  See the MSDN [docs](http://msdn.microsoft.com/en-us/library/dd554932(VS.100).aspx) for more information and background on the feature itself.  This will affect your profiler in that assemblies, modules, and classes may now unload without AppDomainShutdown callbacks being issued first (because the AppDomain is not shutting down!).

This makes it all the more important that your profiler synchronize between module unload events, and any threads that may be using data from the unloading modules.  Of course, this is nothing new—AppDomain shutdown always made it possible for modules to unload.  What’s different is that you should expect modules to unload more often in the future, and that you can’t rely on the containing AppDomain to shutdown first.  It’s always good practice to block inside ModuleUnloadStarted while any of your other threads may be using classes or functions from that module.

Another consideration due to collectible assemblies is the fact that calls to GetAppDomainStaticAddress and GetThreadStaticAddress may give back to you pointers to moveable objects on the GC heap.  This was actually already the case in CLR V2 for some value types living inside movable objects.  Now this will also occur for non-value types in V4 due to collectible assemblies.  All this means to you is that you mustn’t store addresses of statics for use later on, unless you track their movement and update your references to them via the MovedReferences callback.

## Type Forwarding

Expect to see more use of type forwarding in CLR V4.  See this [post](http://blogs.msdn.com/davbr/archive/2009/09/30/type-forwarding.aspx) for more information.

## GetClassLayout and Value Type Size

Instances of value types have a slightly different layout in memory than reference types.  Whereas reference type instances have “header information” at the beginning, value type instances do not (unless they are boxed).  Unfortunately, CLR V2 had a bug where GetClassLayout would return, via the pulClassSize [out] parameter, the full boxed size of the value type, including the header.  Fortunately, the field offsets returned by GetClassLayout were always correct.  But any math the profiler did based on pulClassSize would be wrong for unboxed value type instances.

In CLR V4 this has been fixed, and pulClassSize gives the real size (including header for reference types, and excluding the header for value types).  So if your profiler was working around this problem in CLR V2, you’ll need to undo the workaround when running against CLR V4.

## No More Frozen Strings

In CLR V2, profilers could use EnumModuleFrozenObjects to enumerate over frozen strings present in a module.  This API still functions, but it will now always give you an enumerator over the empty set since there are no longer frozen strings to iterate over.  Also, in CLR V2, you may have noticed that GetGenerationBounds would sometimes give you generation 2 ranges inside NGENd modules (due to frozen strings).  You will no longer find this to be the case in CLR V4.

## String Layout

As an optimization, in CLR V4 we have removed the buffer length field from the layout of string objects in memory.  This makes the GetStringLayout method’s pBufferLengthOffset parameter kind of pointless in CLR V4 (it now just gives you the same value as the pStringLengthOffset parameter).  As a result, it is best for your profiler to now use GetStringLayout2, which no longer has the pBufferLengthOffset parameter.

# Security

In this section are changes related to security:

- Introduction to Security Changes 
- Transparent code in fully-trusted assemblies 
- Conditional APTCA 

## Introduction to Security Changes

What follows is a pathetically reduced summary of how security is changing in CLR V4, for the purpose of putting into perspective how your profiler may behave differently as a result.  Please note that my blog is _not_ the place to go for getting general information about managed security.  Check out [http://blogs.msdn.com/shawnfa/](http://blogs.msdn.com/shawnfa/ "http://blogs.msdn.com/shawnfa/") and [http://blogs.msdn.com/clrteam/archive/tags/Security/default.aspx](http://blogs.msdn.com/clrteam/archive/tags/Security/default.aspx "http://blogs.msdn.com/clrteam/archive/tags/Security/default.aspx").  Also, there’s a CLR Inside Out article with a great overview of all the security changes in V4: [http://msdn.microsoft.com/en-us/magazine/ee677170.aspx](http://msdn.microsoft.com/en-us/magazine/ee677170.aspx "http://msdn.microsoft.com/en-us/magazine/ee677170.aspx").

Ok, having said that…  CLR V4 introduces a new, simpler security model.  First off, the policy is simplified.  Basically, any run-of-the-mill, unhosted managed app will now run with all its assemblies granted full trust.  This is the case even if you run the app off of a network share.  A host can modify this, but there are some limitations:  GAC’d assemblies will always be fully trusted, the host can only specify a single partial-trust security grant set at the AppDomain level (and not customize partial-trust grant sets per assembly), and code groups are now deprecated.  It’s possible to revert back to the old CAS policy, but that’s outside the scope of this blog entry—by default the old CAS policy is now history.

Security enforcement has moved to a simpler model as well, known as Level 2 Security Transparency.  Rather than using LinkDemands, the JIT looks at annotations on types and methods that describe themselves as Transparent, SafeCritical, and Critical.  Transparent code cannot perform security-sensitive operations, whereas Critical code can.  Transparent code cannot call critical code directly.  That’s where SafeCritical code comes in: it can do everything Critical code can do _and_ allows transparent callers.  This makes SafeCritical code pretty dangerous to write, and it must do thoughtful validation of parameters and careful calls into other Critical code, to ensure it’s not being used maliciously.

“Nifty, Dave.  But how is this going to break my profiler?”

## Transparent code in fully-trusted assemblies

If you perform IL rewriting of mscorlib, you’re used to having full permissions granted to your rewritten IL.  However, in CLR V4, some code in mscorlib is now annotated as Transparent.  So if you instrument a transparent method in mscorlib, it will remain transparent and will be blocked from performing security-sensitive operations you may be used to, such as P/Invokes.

The recommended way to attack this problem is to isolate any code that needs to perform security-sensitive operations into new Critical methods you create.  You should then create the minimum necessary SafeCritical bridge code, so that you many instrument Transparent code to call into that bridge code (which in turn calls into your Critical methods).  I must stress that writing SafeCritical code requires thought and attention.  You do not want malicious, untrusted code to exploit your profiler by calling into your SafeCritical code to manipulate your profiler into doing nasty things.

There is an alternative you can use to help with the issue of instrumenting transparent code in fully-trusted assemblies.  You may pass the COR\_PRF\_DISABLE\_TRANSPARENCY\_CHECKS\_UNDER\_FULL\_TRUST flag in your call to SetEventMask().  This will make things work similarly to how they did in V2, in the sense that any code you instrument in mscorlib will run with full trust and be able to perform security-sensitive operations (even if it’s marked as transparent).  The advantage of this approach is that it’s easy.  The disadvantages are that it changes the security behavior of all full trust assemblies when your profiler is loaded (generally you want an application to behave as normally as possible when your profiler is loaded), and it still does not address the issue of partial-trust assemblies (whose transparent code will continue to be transparent).  The reason we added this flag was to reduce the work required to get your profiler up and running on V4.  But it’s best for you to give yourself a work item to deal with security properly (i.e., adding SafeCritical & Critical methods as described above).

## Conditional APTCA

It is impossible to summarize “Conditional APTCA (Allow Partially Trusted Callers Attribute)” into one sentence, but I’m going to do it anyway.  Conditional APTCA is a feature where an assembly with security-sensitive code says, “I _may_ allow partially-trusted callers”, and then the host makes the final call on a per-AppDomain basis.  If that doesn’t clarify it for you (and how could it, really?) go read the CLR Inside Out article I referenced above.

Conditional APTCA may affect your profiler in that you may see a sudden lack of “shareability” (i.e., domain neutrality) of managed assemblies you ship alongside your profiler to support instrumentation you perform.  While this is not typically an issue with regular old unhosted apps (which now run with all assemblies granted full trust by default), this can affect hosted scenarios such as ASP.NET.

As a review, the “shared domain” is a pseudo-domain into which the CLR loads all assemblies that should be loaded as domain neutral (as configured by the host and / or the LoaderOptimizationAttribute).  The application is generally unaware of this shared domain.  As far as the application is concerned, the assemblies are loaded separately into each “real” AppDomain that uses them, and any per-AppDomain information (e.g., values of statics) are duplicated and stored in each AppDomain as expected.  But any shared data that would always be identical across the AppDomains (e.g., JITted code) is stored only once, in the shared domain.

Shareability of assemblies does not affect the correctness of the application, but can enhance performance by not having to JIT the functions of those assemblies into every (real) AppDomain in which those functions are used.  The CLR must be careful about deciding whether an assembly is truly capable of being loaded shared—the security characteristics must be identical in every AppDomain that might use that assembly.

That’s where conditional APTCA makes an impact.  As a simplified example, if your profiler’s assembly (call it P) references a conditional APTCA assembly (call it A), then P’s shareability is called into question.  P can only be shared if the host has decided to _enable_ APTCA for A in all AppDomains, as well as the transitive closure of A’s conditional APTCA dependents.  In other words, the host must enable APTCA for all conditional APTCA assemblies referenced by P (directly or indirectly) in all AppDomains in order to allow P to be shared.

# Exception Handling

In this section are changes related to how the CLR implements exception handling:

- DynamicMethods 
- Windows 7 Unhandled Exceptions 
- GetNotifiedExceptionClauseInfo on 64-bits 
- CLR Exception Code 

## DynamicMethods

You may have noticed that the CLR “hides” from the profiler any functions created via System.Reflection.Emit.DynamicMethod.  You don’t get JIT notifications for them, and you don’t see frames for them when you call DoStackSnapshot.  This is intentional, because DynamicMethods have no metadata, and the usual things profilers might do with functions (e.g., getting a MethodDef and looking up the name or signature in metadata) will fail miserably with DynamicMethods.  And don’t get me started on trying to instrument those puppies.  Someday in the future, it may be nice to provide limited support for profiling DynamicMethods, but that’s a topic for another time.

For now, note that, in V2, we accidentally let slip some exception callbacks for DynamicMethod frames.  In particular, your profiler would get ExceptionSearchFunctionLeave and ExceptionUnwindFunctionLeave callbacks for DynamicMethod frames, but not the matching Enter callbacks!  This made maintaining shadow stacks unnecessarily difficult, as one would expect the search or unwind phase to enter a function before leaving it.  In V4, we’re now properly hiding the ExceptionSearchFunctionLeave and ExceptionUnwindFunctionLeave callbacks callbacks for DynamicMethod frames, so that the callbacks are now balanced.  Note that the story is still not perfect—the CLR still intentionally does notify the profiler of ExceptionThrown, even when the exception is thrown from a DynamicMethod (it’s more important for the profiler to know an exception has occurred, than to have that information be hidden).  There are also still some imbalances with some of the the other exception enter/leave callbacks (ExceptionSearchFilter\*, ExceptionUnwindFinally\*, ExceptionCatcher\*).  But we didn’t want to make too many breaking changes in this area, and the ExceptionSearch/UnwindFunction\* callbacks were the only ones essential for maintaining shadow stacks.  So we chose to be rather surgical with this fix.

Hopefully this shouldn’t break you at all, and if anything should make your life better.

## Windows 7 Unhandled Exceptions

In Windows 7, if an exception goes unhandled, then the DLLMain’s DLL\_PROCESS\_DETACH call is omitted.  So if you have any cleanup code here, be warned that it won’t get run in this case.  This actually has nothing to do with CLR V4—it occurs on CLR V2 as well.  Just thought I’d mention.

## GetNotifiedExceptionClauseInfo on 64-bits

A minor improvement has been made to the behavior of GetNotifiedExceptionClauseInfo during nested exceptions on x64.

```
public static void Foo() { try { throw new Exception("outer"); } catch (Exception exOuter) { try { throw new Exception("inner"); } catch (Exception exInner) { // Profiler calls GetNotifiedExceptionClauseInfo  } } }
```

If your profiler were to call GetNotifiedExceptionClauseInfo from inside the inner catch clause you would see a different value for the stack pointer in CLR V2 and CLR V4.  On CLR V4, you will get the stack pointer for Foo(), which is nice.  This can be used to compare against the SP you get for Foo() when you call DoStackSnapshot, so you can figure out which frame on the stack corresponds to the function containing the catch clause being executed.  In CLR V2, however, your profiler would get the stack pointer of a special “pseudo-stack-frame” the CLR generates for the outer catch clause.  This was silly, because these “pseudo-stack-frames” are a CLR internal implementation detail that is not normally exposed to the profiler.  As such, this stack pointer could not be easily compared against anything you get back from DoStackSnapshot.

So hopefully this change should not break you, but if anything, should help.

## CLR Exception Code

Windows structured exception handling uses a numerical “code” to identify exception types.  The CLR uses a single code for all managed exceptions.  This code changed in CLR V4.  That's about all I want to say about that.  No need to tell you what the old code and new code are, because your profiler shouldn’t be hard-coding the number anyway.  And if it is, well, you should go tinker around with CLR V4 to see how it’s changed—and consider removing any dependence you have on the CLR’s exception code.

 

If you made it this far, wow!  You now have a good idea of what things to investigate that could cause problems if your customers try running your V2 profiler against CLR V4.  You also have a “todo” list of things to look at fixing as you upgrade your profiler to support ICorProfilerCallback3 and work properly against CLR V4.

