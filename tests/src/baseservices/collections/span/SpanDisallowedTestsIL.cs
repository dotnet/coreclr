using System;
using System.Globalization;
using System.Runtime.CompilerServices;

public static class SpanDisallowedTests
{
    public static void ClassStaticSpanTest()
    {
        Assert.Throws<TypeLoadException>(() => ClassStaticSpanTestCore());
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ClassStaticSpanTestCore()
    {
        ClassStaticSpanTest_SpanContainer.s_span = new Span<int>(new int[] { 1, 2 });
    }

    private static class ClassStaticSpanTest_SpanContainer
    {
        public static Span<int> s_span;
    }

    public static void ClassInstanceSpanTest()
    {
        Assert.Throws<TypeLoadException>(() => ClassInstanceSpanTestCore());
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ClassInstanceSpanTestCore()
    {
        var spanContainer = new ClassInstanceSpanTest_SpanContainer();
        spanContainer._span = new Span<int>(new int[] { 1, 2 });
    }

    private sealed class ClassInstanceSpanTest_SpanContainer
    {
        public Span<int> _span;
    }

    public static void GenericClassInstanceSpanTest()
    {
        Assert.Throws<TypeLoadException>(() => GenericClassInstanceSpanTestCore());
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void GenericClassInstanceSpanTestCore()
    {
        var spanContainer = new GenericClassInstanceSpanTest_SpanContainer<Span<int>>();
        spanContainer._t = new Span<int>(new int[] { 1, 2 });
    }

    private sealed class GenericClassInstanceSpanTest_SpanContainer<T>
    {
        public T _t;
    }

    public static void GenericInterfaceOfSpanTest()
    {
        Assert.Throws<TypeLoadException>(() => GenericInterfaceOfSpanTestCore());
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void GenericInterfaceOfSpanTestCore()
    {
        Assert.NotNull(typeof(GenericInterfaceOfSpanTest_ISpanContainer<Span<int>>));
    }

    private interface GenericInterfaceOfSpanTest_ISpanContainer<T>
    {
        T Foo(T t);
    }

    public static void GenericStructInstanceSpanTest()
    {
        Assert.Throws<TypeLoadException>(() => GenericStructInstanceSpanTestCore());
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void GenericStructInstanceSpanTestCore()
    {
        var spanContainer = new GenericStructInstanceSpanTest_SpanContainer<Span<int>>();
        spanContainer._t = new Span<int>(new int[] { 1, 2 });
    }

    private struct GenericStructInstanceSpanTest_SpanContainer<T>
    {
        public T _t;
    }

    public static void GenericDelegateOfSpanTest()
    {
        Assert.Throws<TypeLoadException>(() => GenericDelegateOfSpanTestCore());
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void GenericDelegateOfSpanTestCore()
    {
        Assert.NotNull(typeof(GenericDelegateOfSpan_Delegate<Span<int>>));
    }

    private delegate T GenericDelegateOfSpan_Delegate<T>(T span);

    public static void ArrayOfSpanTest()
    {
        Assert.Throws<TypeLoadException>(() => ArrayOfSpanTestCore());
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ArrayOfSpanTestCore()
    {
        Assert.NotNull(new Span<int>[2]);
    }

    public static void BoxSpanTest()
    {
        Assert.Throws<InvalidProgramException>(() => BoxSpanTestCore());
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void BoxSpanTestCore()
    {
        var span = new Span<int>(new int[] { 1, 2 });
        object o = span;
        Assert.NotNull(o);
    }
}

public static class Assert
{
    public static void NotNull(object value)
    {
        if (value != null)
            return;
        throw new AssertionFailureException("Expected null, got value of type '{1}'.", value.GetType().FullName);
    }

    public static void Throws<T>(Action action) where T : Exception
    {
        try
        {
            action();
        }
        catch (T ex) when (ex.GetType() == typeof(T))
        {
            return;
        }
        catch (Exception ex)
        {
            throw new AssertionFailureException(ex, "Expected exception of type '{0}', got '{1}'.", ex.GetType().FullName);
        }
        throw new AssertionFailureException("Exception was not thrown, expected '{0}'.", typeof(T).FullName);
    }
}

public class AssertionFailureException : Exception
{
    public AssertionFailureException(string format, params object[] args) : this(null, format, args)
    {
    }

    public AssertionFailureException(Exception innerException, string format, params object[] args)
        : base(string.Format(CultureInfo.InvariantCulture, format, args), innerException)
    {
    }
}
