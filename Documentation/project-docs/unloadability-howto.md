# Using and debugging unloadability in .NET Core
An ability load and later unload a set of assemblies is one of the features that were missing until now in the .NET Core. The desktop .NET has AppDomains that people can use for the purpose of the unloading. Since AppDomains were removed from the .NET Core, how can we achieve that goal there?

The .NET Core 3.0 brings support of unloadability using `AssemblyLoadContext`. You can now load a set of assemblies into an `AssemblyLoadContext`, execute methods in them or just inspect them using reflection and finally unload the `AssemblyLoadContext`. That results in unloading of the assemblies loaded into that `AssemblyLoadContext`.

There is one noteworthy difference between the unloading using `AssemblyLoadContext` and using AppDomains. With AppDomains, the unloading is forced. At the unload time, all threads running in the target AppDomain are aborted, managed COM objects created in the AppDomain that's being unloaded are destroyed, etc. 

With the new way that uses `AssemblyLoadContext`, the unload is "cooperative". Calling the `Unload` method on the `AssemblyLoadContext` just initiates the unloading. But the unload will not complete until there are no threads having code from the assemblies loaded into the `AssemblyLoadContext` on their call stacks and until there are no strong references to types from the assemblies loaded into the `AssemblyLoadContext`, their instances and the assemblies themselves. And that means that GC needs to collect those first.
## Using unloadable AssemblyLoadContext
### How to create an unloadable AssemblyLoadContext
You need to derive your own class from the `AssemblyLoadContext` and overload its Load method. That method is used to resolve references to all assemblies that are dependencies of assemblies loaded into that `AssemblyLoadContext`. 
Here is a trivial example of what the custom `AssemblyLoadContext` can look like:
```C++
class TestAssemblyLoadContext : AssemblyLoadContext
{
    public TestAssemblyLoadContext() : base(isCollectible: true)
    {
        protected override Assembly Load(AssemblyName name)
        {
            return null;
        }
    }
}
```
As you can see, the `Load` method returns `null`. That means that all the dependency assemblies are loaded into the default context and only the assemblies explicitly loaded into the new context are in this context. In case you want to load some or all of the dependencies into the `AssemblyLoadContext` too, you can call e.g. `LoadFromAssemblyPath` there and return its result. Here is an example (for the sake of simplicity assuming that all dependency assemblies are in a single directory):
```C++
protected override Assembly Load(AssemblyName name)
{
    return LoadFromAssemblyPath(Path.Combine("absolute/path/to/assembly/directory", name.Name + ".dll"));;
}
```
### How to use a custom unloadable AssemblyLoadContext
Now you can create an instance of the custom `AssemblyLoadContext` and load an assembly into it as follows:
```C++
var alc = new TestAssemblyLoadContext();
Assembly a = alc.LoadFromAssemblyPath("absolute/path/to/your/assembly");
```
For each of the assemblies referenced by the loaded assembly, the `TestAssemblyLoadContext.Load` method is called so that the `TestAssemblyLoadContext` can decide where to get the assembly from. In our case, it returns `null` to indicate that it should be loaded into the default context from locations that the runtime uses to load assemblies by default.

Now that we have loaded an assembly, we can execute a method from it. Let's run the `Main` method:
```C++
var args = new object[1] {new string[] {"Hello"}};
int result = (int)a.EntryPoint.Invoke(null, args);
```
After the `Main` method returns, we can initiate unloading by either calling the Unload method on the custom AssemblyLoadContext or getting rid of the reference we have to the AssemblyLoadContext:
```C++
alc.Unload();
```
This should be sufficient to get the test assembly unloaded. Let's actually put all of this into a separate non-inlineable method to ensure that none of the `TestAssemblyLoadContext`, `Assembly` and `MethodInfo` (the `Assembly.EntryPoint`) can be kept alive by stack slot references (real or JIT introduced locals). That could keep the `TestAssemblyLoadContext` alive and prevent the unload from happening.
Let's also return a weak reference to the `AssemblyLoadContext` so that we can use it later to detect unload completion.
```C++
[MethodImpl(MethodImplOptions.NoInlining)]
int ExecuteAndUnload(string assemblyPath, out WeakReference alcWeakRef)
{
    var alc = new TestAssemblyLoadContext();
    alcWeakRef = new WeakReference(alc, trackResurrection: true);

    Assembly a = alc.LoadFromAssemblyPath(assemblyPath);

    var args = new object[1] { new string[] {"Hello"}};
    int result = (int)a.EntryPoint.Invoke(null, args);
    alc.Unload();

    return result;
}
```
Now we can run this function to Load, execute and Unload the assembly.
```C++
WeakReference testAlcWeakRef;
int result = ExecuteAndUnload("absolute/path/to/your/assembly", out testAlcWeakRef);
```
However, the unloading doesn't complete immediately. As I've already mentioned, it relies on GC to collect all the objects from the test assembly (etc.). In many cases, it is not necessary to wait for the unload completion. However there are cases where it is useful to know that the unload has finished. For example, you may want to delete the assembly file that was loaded into the custom context from disk. In such case, the following code snippet can be used. It triggers a GC and waits for pending finalizers in a loop until the weak reference to the custom AssemblyLoadContext is set to null, indicating the target object was collected. Please note that in most cases, just one pass through the loop is required. However for more complex cases where objects created by the code running in the AssemblyLoadContext have finalizers, more passes may be needed.
```C++
for (int i = 0; testAlcWeakRef.IsAlive && (i < 10); i++)
{
    GC.Collect();
    GC.WaitForPendingFinalizers();
}
```
### The Unloading event
In some cases, it may be necessary for the code loaded into a custom AssemblyLoadContext to perform some cleanup when the unloading is initiated. For example, it may need to stop threads, cleanup some strong GC handles, etc. The `Unloading` event can be used in such cases. A handler that performs the necessary cleanup can be hooked to this event.
### Troubleshooting unloadability issues
While the previous paragraphs may sound like it is all easy and simple, it is often not the case. Due to the cooperative nature of the unloading, it is easy to forget about references keeping the stuff in the custom AssemblyLoadContext alive and preventing unload. Here is a summary of things (some of them non-obvious) that can hold the references:
* Regular reference held from outside of the custom AssemblyLoadContext  or a strong / pinning GC handle. Such reference may point to:
  * An assembly loaded into the custom `AssemblyLoadContext`
  * Type from such an assembly
  * Instance of a type  from such an assembly
* Threads running code from an assembly loaded into the custom AssemblyLoadContext
The above mentioned reference can come from any object with root in a stack slot or processor register (method locals, either explicitly created by the user code or implicitly by the JIT), a static variable or strong / pinning GC handle. TODO: explain what root means
Hints to find stack slot / processor register rooting an object
* Passing function call results directly to another function may create a root
* If a reference to an object was available at any point in a method, the JIT might have decided to keep the reference in a stack slot / processor register for as long as it wants in the current function.
## Debugging unloading issues
Debugging issues with unloading can be tedious. You can get into situation where you don't know what can be holding an `AssemblyLoadContext` alive, but the unload fails.
The best weapon to help with that is Windbg (lldb on Unix) with the SOS plugin. We need to find what's keeping a `LoaderAllocator` belonging to the specific `AssemblyLoadContext` alive.
This plugin allows us to look at GC heap objects, their hierarchies and roots. 
To load the plugin into the debugger, enter the following command in the debugger command line:
```
In WinDbg (it seems WinDbg does that automatically when breaking into .NET Core application):
.loadby sos coreclr

In LLDB:
plugin load /path/to/libsosplugin.so
```
Let's try to debug an example program that has problems with unloading. I have included its source code below. When you run it under WinDbg, the program breaks into the debugger right after attempting to check for the unload success. We can then start looking for the culprits.

Please note that if you debug using LLDB on Unix, the SOS commands in the examples below don't have the `!` in front of them.
```
!dumpheap -type LoaderAllocator
```
It will dump all objects with type name containing `LoaderAllocator` string that are in the GC heap. Here is an example
```
         Address               MT     Size
000002b78000ce40 00007ffadc93a288       48     
000002b78000ceb0 00007ffadc93a218       24     

Statistics:
              MT    Count    TotalSize Class Name
00007ffadc93a218        1           24 System.Reflection.LoaderAllocatorScout
00007ffadc93a288        1           48 System.Reflection.LoaderAllocator
Total 2 objects
```
In the "Statistics:" part below, check the `MT` (`MethodTable`) belonging to the `System.Reflection.LoaderAllocator` which is the object we care about. Then in the list at the beginning, find the entry with `MT` matching that one and get the address of the object itself. In our case, it is "000002b78000ce40"

Now that we know the address of the `LoaderAllocator` object, we can use another command to find its GC roots
```
!gcroot -all 0x000002b78000ce40 
```
This will dump the chain of object references that lead to the `LoaderAllocator` instance. The list starts with the root, which is the entity that keeps our `LoaderAllocator` alive and thus is the core of the problem we are debugging. The root can be a stack slot, a processor register, a GC handle or a static variable. 

Here is an example of the output of the `gcroot` command:
```
Thread 4ac:
    000000cf9499dd20 00007ffa7d0236bc example.Program.Main(System.String[]) [E:\unloadability\example\Program.cs @ 70]
        rbp-20: 000000cf9499dd90
            ->  000002b78000d328 System.Reflection.RuntimeMethodInfo
            ->  000002b78000d1f8 System.RuntimeType+RuntimeTypeCache
            ->  000002b78000d1d0 System.RuntimeType
            ->  000002b78000ce40 System.Reflection.LoaderAllocator

HandleTable:
    000002b7f8a81198 (strong handle)
    -> 000002b78000d948 test.Test
    -> 000002b78000ce40 System.Reflection.LoaderAllocator

    000002b7f8a815f8 (pinned handle)
    -> 000002b790001038 System.Object[]
    -> 000002b78000d390 example.TestInfo
    -> 000002b78000d328 System.Reflection.RuntimeMethodInfo
    -> 000002b78000d1f8 System.RuntimeType+RuntimeTypeCache
    -> 000002b78000d1d0 System.RuntimeType
    -> 000002b78000ce40 System.Reflection.LoaderAllocator

Found 3 roots.
```
Now you need to figure out where is the root located so that you can fix it. The easiest case is when the root is a stack slot or a processor register. In that case, the `gcroot` shows you the name of the function whose frame contains the root and the thread executing that function. The difficult case is when the root is a static variable or a GC handle. 

In our example above, the first root is a local of type `System.Reflection.RuntimeMethodInfo` stored in the frame of the function `example.Program.Main(System.String[])` at address `rbp-20` (RBP is the processor register RBP and -20 is a hexadecimal offset from that register).

The second root is a normal (strong) `GCHandle` that holds reference to an instance of the `test.Test` class. 

The third root is a pinned `GCHandle`. This one is actually a static variable. Unfortunately, there is no way to tell. Statics for reference types are stored in a managed object array in internal runtime structures.

Another case that can prevent unloading of an `AssemblyLoadContext` is when a thread has a frame of a method from assembly loaded into the `AssemblyLoadContext` on its stack. You can check that by dumping managed call stacks of all threads:
```
~*e !clrstack
```
The command means "apply to all threads the !clrstack command". Here is an example of an output of that command for our example. Unfortunately, LLDB on Unix doesn't have any way to apply a command to all threads, so you'll need to resort to manual switching threads and repeating the `clrstack` command.
You should ignore all threads where the debugger says "Unable to walk the managed stack."
```
OS Thread Id: 0x6ba8 (0)
        Child SP               IP Call Site
0000001fc697d5c8 00007ffb50d9de12 [HelperMethodFrame: 0000001fc697d5c8] System.Diagnostics.Debugger.BreakInternal()
0000001fc697d6d0 00007ffa864765fa System.Diagnostics.Debugger.Break()
0000001fc697d700 00007ffa864736bc example.Program.Main(System.String[]) [E:\unloadability\example\Program.cs @ 70]
0000001fc697d998 00007ffae5fdc1e3 [GCFrame: 0000001fc697d998] 
0000001fc697df28 00007ffae5fdc1e3 [GCFrame: 0000001fc697df28] 
OS Thread Id: 0x2ae4 (1)
Unable to walk the managed stack. The current thread is likely not a 
managed thread. You can run !threads to get a list of managed threads in
the process
Failed to start stack walk: 80070057
OS Thread Id: 0x61a4 (2)
Unable to walk the managed stack. The current thread is likely not a 
managed thread. You can run !threads to get a list of managed threads in
the process
Failed to start stack walk: 80070057
OS Thread Id: 0x7fdc (3)
Unable to walk the managed stack. The current thread is likely not a 
managed thread. You can run !threads to get a list of managed threads in
the process
Failed to start stack walk: 80070057
OS Thread Id: 0x5390 (4)
Unable to walk the managed stack. The current thread is likely not a 
managed thread. You can run !threads to get a list of managed threads in
the process
Failed to start stack walk: 80070057
OS Thread Id: 0x5ec8 (5)
        Child SP               IP Call Site
0000001fc70ff6e0 00007ffb5437f6e4 [DebuggerU2MCatchHandlerFrame: 0000001fc70ff6e0] 
OS Thread Id: 0x4624 (6)
        Child SP               IP Call Site
GetFrameContext failed: 1
0000000000000000 0000000000000000 
OS Thread Id: 0x60bc (7)
        Child SP               IP Call Site
0000001fc727f158 00007ffb5437fce4 [HelperMethodFrame: 0000001fc727f158] System.Threading.Thread.SleepInternal(Int32)
0000001fc727f260 00007ffb37ea7c2b System.Threading.Thread.Sleep(Int32)
0000001fc727f290 00007ffa865005b3 test.Program.ThreadProc() [E:\unloadability\test\Program.cs @ 17]
0000001fc727f2c0 00007ffb37ea6a5b System.Threading.Thread.ThreadMain_ThreadStart()
0000001fc727f2f0 00007ffadbc4cbe3 System.Threading.ExecutionContext.RunInternal(System.Threading.ExecutionContext, System.Threading.ContextCallback, System.Object)
0000001fc727f568 00007ffae5fdc1e3 [GCFrame: 0000001fc727f568] 
0000001fc727f7f0 00007ffae5fdc1e3 [DebuggerU2MCatchHandlerFrame: 0000001fc727f7f0] 

```
As you can see the last thread has `test.Program.ThreadProc()`. This is a function from the assembly loaded into the `AssemblyLoadContext` and so it keeps the `AssemblyLoadContext` alive.
## Example source with unloadability issues
This example is used in the debugging above.
### Main testing program
```C#
using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;

namespace example
{
    
    class TestAssemblyLoadContext : AssemblyLoadContext
    {
        public TestAssemblyLoadContext() : base(true)
        {
        }
        protected override Assembly Load(AssemblyName name)
        {
            return null;
        }
    }

    class TestInfo
    {
        public TestInfo(MethodInfo mi)
        {
            entryPoint = mi;
        }
        MethodInfo entryPoint;
    }

    class Program
    {
        static TestInfo entryPoint;

        [MethodImpl(MethodImplOptions.NoInlining)]
        static int ExecuteAndUnload(string assemblyPath, out WeakReference testAlcWeakRef, out MethodInfo testEntryPoint)
        {
            var alc = new TestAssemblyLoadContext();
            testAlcWeakRef = new WeakReference(alc);

            Assembly a = alc.LoadFromAssemblyPath(assemblyPath);
            if (a == null)
            {
                testEntryPoint = null;
                Console.WriteLine("Loading the test assembly failed");
                return -1;
            }

            var args = new object[1] {new string[] {"Hello"}};

            // Issue preventing unloading #1 - we keep MethodInfo of a method for an assembly loaded into the TestAssemblyLoadContext in a static variable
            entryPoint = new TestInfo(a.EntryPoint);
            testEntryPoint = a.EntryPoint;

            int result = (int)a.EntryPoint.Invoke(null, args);
            alc.Unload();

            return result;
        }    

        static void Main(string[] args)
        {
            WeakReference testAlcWeakRef;
            // Issue preventing unloading #2 - we keep MethodInfo of a method for an assembly loaded into the TestAssemblyLoadContext in a local variable
            MethodInfo testEntryPoint;
            int result = ExecuteAndUnload(@"absolute/path/to/test.dll", out testAlcWeakRef, out testEntryPoint);

            for (int i = 0; testAlcWeakRef.IsAlive && (i < 10); i++)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }

            System.Diagnostics.Debugger.Break();

            Console.WriteLine($"Test completed, result={result}, entryPoint: {testEntryPoint} unload success: {!testAlcWeakRef.IsAlive}");
        }
    }
}
```
## Program loaded into the TestAssemblyLoadContext
This is the `test.dll` passed to the `ExecuteAndUnload` method in the main testing program.
```C#
using System;
using System.Runtime.InteropServices;

namespace test
{
    class Test
    {
        string message = "Hello";
    }

    class Program
    {
        public static void ThreadProc()
        {
            // Issue preventing unlopading #4 - a thread running method inside of the TestAssemblyLoadContext at the unload time
            Thread.Sleep(Timeout.Infinite);
        }

        static GCHandle handle;
        static int Main(string[] args)
        {
            // Issue preventing unloading #3 - normal GC handle
            handle = GCHandle.Alloc(new Test());
            Thread t = new Thread(new ThreadStart(ThreadProc));
            t.IsBackground = true;
            t.Start();
            Console.WriteLine($"Hello from the test: args[0] = {args[0]}");

            return 1;
        }
    }
}

```