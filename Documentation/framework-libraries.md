Framework Libraries
===================

.NET has an expansive standard set of class libraries, referred to as either the base class libraries (core set) or framework class libraries (complete set). These libraries provide implementations for many general and app-specific types, algorithms and utility functionality. Both commercial and community libraries build on top of the framework class libraries, providing easy to use off-the-shelf libraries for a wide set of computing tasks.

A subset of these libraries are provided with each .NET implementation. Base Class Library (BCL) APIs are expected with any .NET implementation, both because developers will want them and because popular libraries will need them to run. App-specific libraries above the BCL, such as ASP.NET, will not be available on all .NET implementations.

Base Class Libraries
====================

The BCL provides the most foundational types and utility functionality and are the base of all other .NET class libraries. They aim to provide very general implementations without any bias to any workload. Performance is always an important consideration, since apps might prefer a particular policy, such as low-latency to high-throughput or low-memory to low-CPU usage. These libraries are intended to be high-performance generally, and take a middle-ground approach according to these various performance concerns. For most apps, this approach has been quite successful. 

Primitive Types
---------------

.NET includes a set of primitive types, which are used (to varying degrees) in all programs. These types contain data, such as numbers, strings, bytes and arbitrary objects. The C# language includes keywords for these types. A sample set of these types is listed below, with the matching C# keywords.

- [System.Object](https://msdn.microsoft.com/library/system.object.aspx) ([object](https://msdn.microsoft.com/library/9kkx3h3c.aspx)) - The ultimate base class in the CLR type system. It is the root of the type hierarchy.
- [System.Int16](https://msdn.microsoft.com/library/system.int16.aspx) ([short](https://msdn.microsoft.com/library/ybs77ex4.aspx)) - A 16-bit signed integer type. The unsigned [UInt16](https://msdn.microsoft.com/en-us/library/system.uint16.aspx) also exists.
- [System.Int32](https://msdn.microsoft.com/library/system.int32.aspx) ([int](https://msdn.microsoft.com/library/5kzh1b5w.aspx)) - A 32-bit signed integer type. The unsigned [UInt32](https://msdn.microsoft.com/library/x0sksh43.aspx) also exists.
- [System.Single](https://msdn.microsoft.com/library/system.single.aspx) ([float](https://msdn.microsoft.com/library/b1e65aza.aspx)) - A 32-bit floating-point type.
- [System.Decimal](https://msdn.microsoft.com/library/system.decimal.aspx) ([decimal](https://msdn.microsoft.com/library/364x0z75.aspx)) - A 128-bit decimal type.
- [System.Byte](https://msdn.microsoft.com/en-us/library/system.byte.aspx) ([byte](https://msdn.microsoft.com/library/5bdb6693.aspx)) - An unsigned 8-bit integeger that represents a byte of memory.
- [System.Boolean](https://msdn.microsoft.com/library/system.boolean.aspx) ([bool](https://msdn.microsoft.comlibrary/c8f5xwh7.aspx)) - A boolean type that represents 'true' or 'false'.
- [System.Char](https://msdn.microsoft.com/library/system.char.aspx) ([char](https://msdn.microsoft.com/library/x9h8tsay.aspx)) - A 16-bit numeric type that represents a Unicode character.
- [System.String](https://msdn.microsoft.com/library/system.string.aspx) ([string](https://msdn.microsoft.com/library/362314fe.aspx)) - Represents a series of characters. Different than a ```char[]```, but enables indexing into each individual ```char``` in the ```string```.


Data Structures
---------------

.NET includes a set of data structures that are the workhorses of almost any .NET apps. These are mostly collections, but also include other types.

- [Array](https://msdn.microsoft.com/library/system.array.aspx) - Represents an array of strongly types objects that can be accessed by index. Has a fixed size, per its construction.
- [List<T>](https://msdn.microsoft.com/library/6sh2ey19.aspx) - Represents a strongly typed list of objects that can be accessed by index. Is automatically resized as needed.
- [Dictionary<K,V>](https://msdn.microsoft.com/library/xfhwa508.aspx) - Represents a collection of values that are indexed by a key. Values can be accessed via key. Is automatically resized as needed.
- [Uri](https://msdn.microsoft.com/library/system.uri.aspx) - Provides an object representation of a uniform resource identifier (URI) and easy access to the parts of the URI.
- [DateTime](https://msdn.microsoft.com/library/system.datetime.aspx) - Represents an instant in time, typically expressed as a date and time of day.

Utility APIs
------------

.NET includes a set of utility APIs that provide functionality for many important tasks.

- [HttpClient](https://msdn.microsoft.com/library/system.net.http.httpclient.aspx) - An API for sending HTTP requests and receiving HTTP responses from a resource identified by a URI.
- [XDocument](https://msdn.microsoft.com/library/system.xml.linq.xdocument.aspx) - An API for loading, and querying XML documents with LINQ.
- [StreamReader](https://msdn.microsoft.com/library/system.io.streamreader.aspx) - An API for reading files ([StreamWriter](https://msdn.microsoft.com/library/system.io.stringwriter.aspx)) Can be used to write files.

App-Model APIs
--------------

There are many app-models that can be used with .NET, provided by several companies.

- [ASP.NET](http://asp.net) - Provides a web framework for building Web sites and services. Supported on Windows, Linux and OS X (depends on ASP.NET version).