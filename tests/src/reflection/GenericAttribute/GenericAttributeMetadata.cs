using System;
using System.Reflection;
using System.Collections;
using System.Runtime.CompilerServices;

[assembly: SingleAttribute<int>()]
[assembly: SingleAttribute<bool>()]

[assembly: MultiAttribute<int>()]
[assembly: MultiAttribute<int>(1)]
[assembly: MultiAttribute<int>(Value = 2)]
[assembly: MultiAttribute<bool>()]
[assembly: MultiAttribute<bool>(true)]

[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = false)]
public class SingleAttribute<T> : Attribute
{

}

[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = true)]
public class MultiAttribute<T> : Attribute
{
    public T Value { get; set; }
    
    public MultiAttribute()
    {
    }
    
    public MultiAttribute(T value)
    {
        Value = value;
    }
}

[SingleAttribute<int>()]
[SingleAttribute<bool>()]
[MultiAttribute<int>()]
[MultiAttribute<int>(1)]
[MultiAttribute<int>(Value = 2)]
[MultiAttribute<bool>()]
[MultiAttribute<bool>(true)]
[MultiAttribute<bool>(Value = true)]
[MultiAttribute<bool?>()]
public class Class
{
    public class Derive : Class
    {

    }

    [SingleAttribute<int>()]
    [SingleAttribute<bool>()]
    [MultiAttribute<int>()]
    [MultiAttribute<int>(1)]
    [MultiAttribute<int>(Value = 2)]
    [MultiAttribute<bool>()]
    [MultiAttribute<bool>(true)]
    public int Property { get; set; }
}