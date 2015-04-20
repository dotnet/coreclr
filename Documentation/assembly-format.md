.NET Assembly File Format
=========================

The .NET binary format is based on the Windows [PE file](http://en.wikipedia.org/wiki/Portable_Executable) format. In fact, .NET class libraries are conformant Windows PEs, and appear on first glance to be Windows dynamic link libraries (DLLs). This is a very useful characteristic on Windows, where they can masquerade as native DLLs and get some of the same treatment (e.g. OS load, PE tools). 
CPU-agnostic and OS-agnostic. It is also fully specified and standardized. As a result, you can use class libraries within any application using any .NET implementation.