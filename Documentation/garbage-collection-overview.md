Garbage Collection
==================

Garbage collection is one of most important features of the .NET managed code platform. The garbage collector (GC) manages allocating and releasing memory for you. You do not need to how to allocate and release memory or manage the lifetime of the objects that use that memory. An allocation is made any time you _new_ an object or a value type is boxed. Allocations are typically very fast. When there isn't enough memory to allocate an object, the GC must collect and dispose of garbage memory to make memory available for new allocations. This process is called  "garbage collection".

The garbage collector serves as an automatic memory manager. It provides the following benefits:

- Enables you to develop your application without having to free memory. 
- Allocates objects on the managed heap efficiently. 
- Reclaims objects that are no longer being used, clears their memory, and keeps the memory available for future allocations. Managed objects automatically get clean content to start with, so their constructors do not have to initialize every data field.
- Provides memory safety by making sure that an object cannot use the content of another object.

The .NET GC is generational and has 3 generations. Each generation has its own heap that it uses for storage of allocated objects. There is a basic principle that most objects are either short lived or long lived. Generation 0 is where objects are first allocated. Objects often don't live past the first generation, since they are no longer in use (out of scope) by the time the next garbage collection occurs. Generation 0 is quick to collect because its associated heap is small. Generation 1 is really a second chance space. Objects that are short lived but survive the generation 0 collection (often based on coincidental timing) go to generation 1. Generation 1 collections are also quick because its associated heap is also small. The first two heaps remain small because objects are either collected or are promoted to the next generation heap. Generation 2 is where all long lived objects are. The generation 2 heap can grow to be very large, since the objects it contains can survive a long time and there is no generation 3 heap to further promote objects. 

The GC has has an additional heap for large objects called the Large Object Heap (LOH). It is reserved for objects that are  85,000 bytes or greater. A byte array (Byte[]) with 85k elements would be an example of a large object. Large objects are not allocated to the generational heaps but are allocated directly to the LOH.

Generation 2 and LOH collections can take noticeable time for programs that have run for a long time or operate over large amounts of data. Large server programs are known to have heaps in the 10s of GBs. The GC employs a variety of techniques to reduce the amount of time that it blocks program execution. The primary approach is to do as much garbage collection work as possible on a background thread in a way that does not interfere with program execution. The GC also exposes a few ways for developers to influence its behavior, which can be quite useful to improve performance.

For more information, see [Garbage Collection](http://msdn.microsoft.com/library/0xy59wtx.aspx) on MSDN.