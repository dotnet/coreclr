*Adapted from an entry that appeared on David Broman's blog*


In my [previous post](http://blogs.msdn.com/b/davbr/archive/2012/11/19/clrprofiler-4-5-released-includes-windows-store-app-support.aspx), I mentioned the new home for CLRProfiler, where you can find its latest version, 4.5: [http://clrprofiler.codeplex.com/](http://clrprofiler.codeplex.com/ "http://clrprofiler.codeplex.com/").  On the same CodePlex site you can also find a new sample, **ILRewrite**.  ILRewrite contains sample code demonstrating the following:

- Parsing a stream of IL bytes into a linked list of editable structures 
- Writing that linked list back out into a new stream of IL bytes 
- Using the metadata API to add AssemblyRefs, TypeRefs, and MemberRefs to modules you instrument 
- Using the metadata API to add brand new methods into mscorlib.dll 
- Using the new RequestReJIT and RequestRevert APIs. 

Click on the **ILRewrite10Source** download from the **Downloads**  **tab**.  You can find some basic documentation on the **Documentation tab** , and more detailed documentation in the **readme.txt** distributed as part of the source.

