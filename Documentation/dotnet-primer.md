.NET Primer
===========

.NET is a general purpose development platform. It can be used for any kind of app type or workload where general purpose solutions are used. It has several key features that are attractive to many developers, including automatic memory management and modern programming languages, that make it easier to efficiently build high-quality apps. .NET enables a high-level programming environment with many convenience features, while providing low-level access to native memory and APIs. 

Multiple implementations of .NET are available, based on open [.NET Standards](dotnet-standards.md) that specify the fundamentals of the platform. They are separately optimized for different app types (e.g. desktop, mobile, gaming, cloud) and support many chips (e.g. x86/x64, ARM) and operating systems (e.g. Windows, Linux, iOS, Android, OS X). Open source is also an important part of the .NET ecosystem, with multiple .NET implementations and many libraries available under OSI-approved licenses.

Defining .NET Features
======================

There are a set of key features that together define .NET. Most of them are not unique on their own, but the peculiar aggregation of these features is what defines .NET as being distinct.

- [Automatic memory management](garbage-collection-overview.md)
- Verifiable code - type and memory safety
- Flexible native code compilation (enables JIT and AOT)
- Fast native interop and access to native memory
- Operating system agnostic 
- Chip-agnostic (e.g. x86, ARM64) byte-code
- Language-agnostic (e.g. C#, F#, VB) runtime
- Flexible type system
- Expressive (imperative and functional) programming languages
- Programmable source code compiler (Roslyn)
- Large standard class library
- [.NET Class Libraries](class-libraries.md)
- [Assembly format](assembly-format.md)
- Package management

TODO: Ensure that this list is correct. Write and link to a chapter for each of these defining features.

.NET by Example
===============

.NET includes many features and capabilities that you will use on a daily basis for common tasks and needs. You can see these described below, with examples. Some of the same topics are discussed in more detail in documents linked above.

Automatic Memory Management
---------------------------

[Garbage collection](garbage-collection.md) is the most well-known of .NET features. Developers do not need to actively manage memory, athough there are affordances to provide more information to the garbage collector (GC). C# includes the `new` keyword to allocate memory in terms of a particular type, and the `using` keyword to scope the usage of an object. The GC operates on a lazy approach to memory management, preferring application throughput to the immediate collection of memory.

The following two lines both allocate. There is no analogous keyword to de-allocate the memory. The GC reclaims the memory sometime after the two variables go out of scope.

	var title = ".NET Primer";
	var list = new List<string>;

Method variables normally go out of scope once a method completes, at which point they can be collected. The following example indicates to the GC that an object has gone out of scope, once the `using` statement has been completed, and the dependent `stream` object can be collected.

	using(FileStream stream = GetFileStream(context))
	{
		//operations on the stream
	}

Memory Safety
-------------

One of the less obvious but quite far-reaching features that a garbage collector enables is memory safety.  The invariant of memory safety is very simple: a program is memory safe if it accesses only memory that has been allocated (and not freed). Dangling pointers are always bugs, and tracking them down is often quite difficult.

The .NET runtime provides additional services, to complete the promise of memory safety, not naturally offered by a GC. It ensures that programs do not index off the end of an array or accessing a phantom field off the end of an object.

The following example will throw as a result of memory safety.

	int[] numbers = new int[42];
	int number = numbers[42] // will throw (indexes are 0-based)

Type Safety
-----------

Objects are allocated in terms of types. The only operations allowed for a given object, and the memory it consumes, are those of its type. A `Dog` type may have `Jump` and `WagTail` methods, but not likely a `SumTotal` method. A program can only call the declared methods of a given type. All other calls will result in an exception.

.NET languages can be object-oriented, with hiearchies of base and derived classes. The .NET runtime will only allow object casts and calls that align with the object hierarchy.

	Dog dog = Dog.AdoptDog();
	Pet pet = (Pet)dog; // Dog derives from Pet
	pet.ActCute(); 
	Car car = (Car)dog; // will throw - no relationship between Car and Dog
	object temp = (object)dog; // legal - a Dog is an object
	car = (Car)temp; // will throw - the runtime isn't fooled
	car.Accelerate() // the dog won't like this, nor will the program get this far

Type safety also guarantees the fidelity of accessor keywords (e.g. private, public, internal). This is particularly useful for non-public data that an implementation uses to manage its behavior.

	Dog dog = Dog._nextDogToBeAdopted; // will throw - this is a private field

Generic Types
-------------

Generic types enable a kind of uncoordinated mix-ins of (largely) arbitrary types. They allow the creation of a kind of _compound type_. A generic type is considered to be _open_, as it requires a set of types (however many type parameters it exposes) to be provided in order to be _closed_. While static in nature, like the rest of the .NET type system, generics provide a kind of faux dynamism that developers find very useful.

The most commonly used generic type is `List<T>` and its sibling `Dictionary<K,V>`. You can use these types, and also create your own generic types. The following example demonstrates the use of these two built-in generic types.

	List<string> strings = new List<string>();
	strings.Add("string A");
	strings.Add("string B");

	Dictionary<int,List<string>> lists = new Dictionary<int,List<string>>();
	lists.Add(DateTime.Now.Hour,strings); // new strings added every hour

Async Programming
-----------------

Async is a first-class concept within .NET, with async support in the runtime, the framework libraries and various .NET languages. Async is based off of the `Task` concept, which encaptsulates a set of operations to be completed. Tasks are distinct from threads and may not rely on threads or require CPU time much at all, particularly for i/o-bound tasks.

C# includes special treatment for async, including the special keyword `await` for managing tasks. The following example demonstrates calling a web service as an async operation.

	string url = "http://someUrl";
	HttpClient client = new HttpClient();
	string json = await client.GetStringAsync(url);

The call to `client.GetStringAsync(url)` does not block, but instead immediately yields by returing a `Task`. Computation resumes and the call returns the requested string when the network activity has completed.

Language Integrated Query (Linq)
--------------------------------

.NET programs typically operate on some form of data. The data can be database-resident or in the form of objects (sometimes called POCOs - "Plain Old CLR Objects"). Linq provides a language-integrated uniform query model over data, independent of the source. Linq providers bridge the gap between the uniform query model and the form of the data, such as SQL Server tables.

The follow examples demonstrate various uses of Linq to query different forms of data.

TODO: Examples.

Lambdas
-------

Lambdas are a kind of mini-method. They declare a signature and a method body, but don't have an identity of their own, like a method on a type. They can however, be assigned to a variable and be called, with the appropriate arguments, and passed in a method signature. They are effectively an alternative delegate syntax. Unlike delegates, they can be directly assigned as the left-hand side of event registration or as a Linq select clause.

The following example demonstrated the use of a lambda as an event handler. You can see the use of lambda as a linq select clause in the Linq section above.

Native Interop
--------------

.NET provides low-level access to native APIs via the platform invoke or P/Invoke facility. It enables a mapping of .NET types to native types, which the .NET runtime marshalls before calling the native API.

TODO: Examples.

Higher-level native interop can be established with P/Invoke. The COM and WinRT interop systems in the CLR are both built on top of P/Invoke. The Java and Objective-C interop systems provided by Xamarin on top of Mono are fundamentally the same.

Unsafe Code
-----------

The CLR enables the ability to acccess native memory and do pointer arithmetic. These operations are needed for some algortithms and for calling some native APIs. The use of these capabilities is discouraged, since you no longer get the benefit of verifiability, nor will your code be allowed to run in all environments. The best practice is to confine unsafe code as much as possible and that the vast majority of code is type-safe.

TODO: Examples.

Notes
=====

The term ".NET runtime" is used throughout the document to accomodate for the multiple implementations of .NET, such as CLR, Mono, IL2CPP and others. The more specific names are only used if needed.

This document is not intended to be historical in nature, but describe the .NET platform as it is now. It isn't important whether a .NET feature has always been available or was only recently introduced, only that it is important enough to highlight and discuss.