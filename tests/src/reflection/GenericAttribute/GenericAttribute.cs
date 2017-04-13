using System;
using System.Reflection;
using System.Collections;
using System.Runtime.CompilerServices;

[assembly: ConsoleApplication21.SingleAttribute<int>()]
[assembly: ConsoleApplication21.SingleAttribute<bool>()]

[assembly: ConsoleApplication21.MultiAttribute<int>()]
[assembly: ConsoleApplication21.MultiAttribute<int>(1)]
[assembly: ConsoleApplication21.MultiAttribute<int>(Value = 2)]
[assembly: ConsoleApplication21.MultiAttribute<bool>()]
[assembly: ConsoleApplication21.MultiAttribute<bool>(true)]


namespace ConsoleApplication21
{
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = false)]
    class SingleAttribute<T> : Attribute
    {

    }
    
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = true)]
    class MultiAttribute<T> : Attribute
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
    class Program
    {
        class Derive : Program
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
        static void Main(string[] args)
        {
            Assembly assembly = typeof(Program).GetTypeInfo().Assembly;
            Assert(CustomAttributeExtensions.GetCustomAttribute<SingleAttribute<int>>(assembly) != null);
            Assert(((ICustomAttributeProvider)assembly).GetCustomAttributes(typeof(SingleAttribute<int>), true) != null);
            Assert(CustomAttributeExtensions.GetCustomAttribute<SingleAttribute<bool>>(assembly) != null);
            Assert(((ICustomAttributeProvider)assembly).GetCustomAttributes(typeof(SingleAttribute<bool>), true) != null);
            Assert(CustomAttributeExtensions.IsDefined(assembly, typeof(SingleAttribute<int>)));
            Assert(((ICustomAttributeProvider)assembly).IsDefined(typeof(SingleAttribute<int>), true));
            Assert(CustomAttributeExtensions.IsDefined(assembly, typeof(SingleAttribute<bool>)));
            Assert(((ICustomAttributeProvider)assembly).IsDefined(typeof(SingleAttribute<bool>), true));

            TypeInfo programTypeInfo = typeof(Program).GetTypeInfo();
            Assert(CustomAttributeExtensions.GetCustomAttribute<SingleAttribute<int>>(programTypeInfo) != null);
            Assert(((ICustomAttributeProvider)programTypeInfo).GetCustomAttributes(typeof(SingleAttribute<int>), true) != null);
            Assert(CustomAttributeExtensions.GetCustomAttribute<SingleAttribute<bool>>(programTypeInfo) != null);
            Assert(((ICustomAttributeProvider)programTypeInfo).GetCustomAttributes(typeof(SingleAttribute<bool>), true) != null);
            Assert(CustomAttributeExtensions.IsDefined(programTypeInfo, typeof(SingleAttribute<int>)));    
            Assert(((ICustomAttributeProvider)programTypeInfo).IsDefined(typeof(SingleAttribute<int>), true));        
            Assert(CustomAttributeExtensions.IsDefined(programTypeInfo, typeof(SingleAttribute<bool>)));
            Assert(((ICustomAttributeProvider)programTypeInfo).IsDefined(typeof(SingleAttribute<bool>), true));    

            var propertyPropertyInfo = typeof(Program).GetTypeInfo().GetProperty(nameof(Property));
            Assert(CustomAttributeExtensions.GetCustomAttribute<SingleAttribute<int>>(propertyPropertyInfo) != null);
            Assert(((ICustomAttributeProvider)propertyPropertyInfo).GetCustomAttributes(typeof(SingleAttribute<int>), true) != null);
            Assert(CustomAttributeExtensions.GetCustomAttribute<SingleAttribute<bool>>(propertyPropertyInfo) != null);
            Assert(((ICustomAttributeProvider)propertyPropertyInfo).GetCustomAttributes(typeof(SingleAttribute<bool>), true) != null);
            Assert(CustomAttributeExtensions.IsDefined(propertyPropertyInfo, typeof(SingleAttribute<int>)));    
            Assert(((ICustomAttributeProvider)propertyPropertyInfo).IsDefined(typeof(SingleAttribute<int>), true));              
            Assert(CustomAttributeExtensions.IsDefined(propertyPropertyInfo, typeof(SingleAttribute<bool>)));
            Assert(((ICustomAttributeProvider)propertyPropertyInfo).IsDefined(typeof(SingleAttribute<bool>), true));              

            var deriveTypeInfo = typeof(Derive).GetTypeInfo();
            Assert(CustomAttributeExtensions.GetCustomAttribute<SingleAttribute<int>>(deriveTypeInfo, false) == null);
            Assert(((ICustomAttributeProvider)deriveTypeInfo).GetCustomAttributes(typeof(SingleAttribute<int>), true) != null);
            Assert(CustomAttributeExtensions.GetCustomAttribute<SingleAttribute<bool>>(deriveTypeInfo, false) == null);
            Assert(((ICustomAttributeProvider)deriveTypeInfo).GetCustomAttributes(typeof(SingleAttribute<bool>), true) != null);
            Assert(!CustomAttributeExtensions.IsDefined(deriveTypeInfo, typeof(SingleAttribute<int>), false));            
            Assert(!CustomAttributeExtensions.IsDefined(deriveTypeInfo, typeof(SingleAttribute<bool>), false));

            Assert(CustomAttributeExtensions.GetCustomAttribute<SingleAttribute<int>>(deriveTypeInfo, true) != null);
            Assert(CustomAttributeExtensions.GetCustomAttribute<SingleAttribute<bool>>(deriveTypeInfo, true) != null);
            Assert(CustomAttributeExtensions.IsDefined(deriveTypeInfo, typeof(SingleAttribute<int>), true));   
            Assert(((ICustomAttributeProvider)deriveTypeInfo).IsDefined(typeof(SingleAttribute<int>), true));           
            Assert(CustomAttributeExtensions.IsDefined(deriveTypeInfo, typeof(SingleAttribute<bool>), true));
            Assert(((ICustomAttributeProvider)deriveTypeInfo).IsDefined(typeof(SingleAttribute<bool>), true));           

            var a1 = CustomAttributeExtensions.GetCustomAttributes(programTypeInfo, true).GetEnumerator();
            AssertNext<Attribute>(a1, a => a is SingleAttribute<int>);
            AssertNext<Attribute>(a1, a => a is SingleAttribute<bool>);
            AssertNext<Attribute>(a1, a => (a as MultiAttribute<int>).Value == 0);
            AssertNext<Attribute>(a1, a => (a as MultiAttribute<int>).Value == 1);
            AssertNext<Attribute>(a1, a => (a as MultiAttribute<int>).Value == 2);
            AssertNext<Attribute>(a1, a => !(a as MultiAttribute<bool>).Value);
            AssertNext<Attribute>(a1, a => (a as MultiAttribute<bool>).Value);
            AssertNext<Attribute>(a1, a => (a as MultiAttribute<bool>).Value);
            AssertNext<Attribute>(a1, a => (a as MultiAttribute<bool?>).Value == null);

            var b1 = ((ICustomAttributeProvider)programTypeInfo).GetCustomAttributes(true).GetEnumerator();
            AssertNext<Attribute>(b1, a => a is SingleAttribute<int>);
            AssertNext<Attribute>(b1, a => a is SingleAttribute<bool>);
            AssertNext<Attribute>(b1, a => (a as MultiAttribute<int>).Value == 0);
            AssertNext<Attribute>(b1, a => (a as MultiAttribute<int>).Value == 1);
            AssertNext<Attribute>(b1, a => (a as MultiAttribute<int>).Value == 2);
            AssertNext<Attribute>(b1, a => !(a as MultiAttribute<bool>).Value);
            AssertNext<Attribute>(b1, a => (a as MultiAttribute<bool>).Value);
            AssertNext<Attribute>(b1, a => (a as MultiAttribute<bool>).Value);
            AssertNext<Attribute>(b1, a => (a as MultiAttribute<bool?>).Value == null);
            
            var a2 = CustomAttributeExtensions.GetCustomAttributes(deriveTypeInfo, false).GetEnumerator();
            Assert(a2.MoveNext() == false);

            var b2 = ((ICustomAttributeProvider)deriveTypeInfo).GetCustomAttributes(false).GetEnumerator();
            Assert(b2.MoveNext() == false);

            var a3 = CustomAttributeExtensions.GetCustomAttributes(deriveTypeInfo, true).GetEnumerator();
            AssertNext<Attribute>(a3, a => a is SingleAttribute<int>);
            AssertNext<Attribute>(a3, a => a is SingleAttribute<bool>);
            AssertNext<Attribute>(a3, a => (a as MultiAttribute<int>).Value == 0);
            AssertNext<Attribute>(a3, a => (a as MultiAttribute<int>).Value == 1);
            AssertNext<Attribute>(a3, a => (a as MultiAttribute<int>).Value == 2);
            AssertNext<Attribute>(a3, a => !(a as MultiAttribute<bool>).Value);
            AssertNext<Attribute>(a3, a => (a as MultiAttribute<bool>).Value);

            var b3 = ((ICustomAttributeProvider)deriveTypeInfo).GetCustomAttributes(true).GetEnumerator();
            AssertNext<Attribute>(b3, a => a is SingleAttribute<int>);
            AssertNext<Attribute>(b3, a => a is SingleAttribute<bool>);
            AssertNext<Attribute>(b3, a => (a as MultiAttribute<int>).Value == 0);
            AssertNext<Attribute>(b3, a => (a as MultiAttribute<int>).Value == 1);
            AssertNext<Attribute>(b3, a => (a as MultiAttribute<int>).Value == 2);
            AssertNext<Attribute>(b3, a => !(a as MultiAttribute<bool>).Value);
            AssertNext<Attribute>(b3, a => (a as MultiAttribute<bool>).Value);

            var a4 = CustomAttributeExtensions.GetCustomAttributes<SingleAttribute<int>>(programTypeInfo, true).GetEnumerator();
            AssertNext<Attribute>(a4, a => a is SingleAttribute<int>);

            var b4 = ((ICustomAttributeProvider)programTypeInfo).GetCustomAttributes(typeof(SingleAttribute<int>), true).GetEnumerator();
            AssertNext<Attribute>(b4, a => a is SingleAttribute<int>);

            var a5 = CustomAttributeExtensions.GetCustomAttributes<SingleAttribute<bool>>(programTypeInfo).GetEnumerator();
            AssertNext<Attribute>(a5, a => a is SingleAttribute<bool>);

            var b5 = ((ICustomAttributeProvider)programTypeInfo).GetCustomAttributes(typeof(SingleAttribute<bool>), true).GetEnumerator();
            AssertNext<Attribute>(b5, a => a is SingleAttribute<bool>);

            var a6 = CustomAttributeExtensions.GetCustomAttributes<MultiAttribute<int>>(programTypeInfo, true).GetEnumerator();
            AssertNext<Attribute>(a6, a => (a as MultiAttribute<int>).Value == 0);
            AssertNext<Attribute>(a6, a => (a as MultiAttribute<int>).Value == 1);
            AssertNext<Attribute>(a6, a => (a as MultiAttribute<int>).Value == 2);

            var b6 = ((ICustomAttributeProvider)programTypeInfo).GetCustomAttributes(typeof(MultiAttribute<int>), true).GetEnumerator();
            AssertNext<Attribute>(b6, a => (a as MultiAttribute<int>).Value == 0);
            AssertNext<Attribute>(b6, a => (a as MultiAttribute<int>).Value == 1);
            AssertNext<Attribute>(b6, a => (a as MultiAttribute<int>).Value == 2);

            var a7 = CustomAttributeExtensions.GetCustomAttributes<MultiAttribute<bool>>(programTypeInfo, true).GetEnumerator();
            AssertNext<Attribute>(a7, a => !(a as MultiAttribute<bool>).Value);
            AssertNext<Attribute>(a7, a => (a as MultiAttribute<bool>).Value);

            var b7 = ((ICustomAttributeProvider)programTypeInfo).GetCustomAttributes(typeof(MultiAttribute<bool>), true).GetEnumerator();
            AssertNext<Attribute>(b7, a => !(a as MultiAttribute<bool>).Value);
            AssertNext<Attribute>(b7, a => (a as MultiAttribute<bool>).Value);

            var a8 = CustomAttributeExtensions.GetCustomAttributes<MultiAttribute<bool?>>(programTypeInfo, true).GetEnumerator();
            AssertNext<Attribute>(a8, a => (a as MultiAttribute<bool?>).Value == null);

            var b8 = ((ICustomAttributeProvider)programTypeInfo).GetCustomAttributes(typeof(MultiAttribute<bool?>), true).GetEnumerator();
            AssertNext<Attribute>(b8, a => (a as MultiAttribute<bool?>).Value == null);

            Assert(CustomAttributeExtensions.GetCustomAttributes(programTypeInfo, typeof(MultiAttribute<>), false) == null);
            Assert(CustomAttributeExtensions.GetCustomAttributes(programTypeInfo, typeof(MultiAttribute<>), true) == null);
            Assert(!((ICustomAttributeProvider)programTypeInfo).GetCustomAttributes(typeof(MultiAttribute<>), true).GetEnumerator().MoveNext());
        }

        static void Assert(bool condition, [CallerLineNumberAttribute]int line = 0)
        {
            if(!condition)
            {
                throw new Exception($"Error in line: {line}");
            }
        }

        static void AssertNext<T>(IEnumerator source, Func<T,bool> func, [CallerLineNumberAttribute]int line = 0)
        {
            source.MoveNext();
            if(!func((T)source.Current))
            {
                throw new Exception($"Error in line: {line}");
            }
        }
    }
}