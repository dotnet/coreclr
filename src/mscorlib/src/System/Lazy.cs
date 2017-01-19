// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
#pragma warning disable 0420

// =+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+
//
//
//
// --------------------------------------------------------------------------------------
//
// A class that provides a simple, lightweight implementation of lazy initialization, 
// obviating the need for a developer to implement a custom, thread-safe lazy initialization 
// solution.
//
// =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-

using System.Runtime;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Threading;
using System.Diagnostics.Contracts;
using System.Runtime.ExceptionServices;

namespace System
{
    /// <summary>
    /// ILazyItem&lt;T&gt; is used to determine the initialization logic that the Lazy object uses.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal interface ILazyItem<T>
    {
        T Value { get; }
        bool IsValueCreated { get; }
        bool IsValueFaulted { get; }
        LazyThreadSafetyMode Mode { get; }
    }

    /// <summary>
    /// Provides support for lazy initialization.
    /// </summary>
    /// <typeparam name="T">Specifies the type of element being lazily initialized.</typeparam>
    /// <remarks>
    /// <para>
    /// By default, all public and protected members of <see cref="Lazy{T}"/> are thread-safe and may be used
    /// concurrently from multiple threads.  These thread-safety guarantees may be removed optionally and per instance
    /// using parameters to the type's constructors.
    /// </para>
    /// </remarks>
    [Serializable]
    [ComVisible(false)]
    [DebuggerTypeProxy(typeof(System_LazyDebugView<>))]
    [DebuggerDisplay("ThreadSafetyMode={Mode}, IsValueCreated={IsValueCreated}, IsValueFaulted={IsValueFaulted}, Value={ValueForDebugDisplay}")]
    public class Lazy<T>
        : ISerializable
        , ILazyItem<T>
    {
        // m_implementation, a volatile reference, is set to null after m_value has been set
        private volatile ILazyItem<T> m_implementation;

        // m_value eventually stores the lazily created value. It is ready when m_implementation = null.
        private T m_value; 

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Threading.Lazy{T}"/> class that 
        /// uses <typeparamref name="T"/>'s default constructor for lazy initialization.
        /// </summary>
        /// <remarks>
        /// An instance created with this constructor may be used concurrently from multiple threads.
        /// </remarks>
        public Lazy()
            : this(CreateInstance.Factory, LazyThreadSafetyMode.ExecutionAndPublication)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Threading.Lazy{T}"/> class that
        /// uses a pre-initialized specified value.
        /// </summary>
        /// <remarks>
        /// An instance created with this constructor should be usable by multiple threads
        //  concurrently.
        /// </remarks>
        public Lazy(T value)
        {
            m_value = value;
            m_implementation = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Threading.Lazy{T}"/> class that uses a
        /// specified initialization function.
        /// </summary>
        /// <param name="valueFactory">
        /// The <see cref="T:System.Func{T}"/> invoked to produce the lazily-initialized value when it is
        /// needed.
        /// </param>
        /// <exception cref="System.ArgumentNullException"><paramref name="valueFactory"/> is a null
        /// reference (Nothing in Visual Basic).</exception>
        /// <remarks>
        /// An instance created with this constructor may be used concurrently from multiple threads.
        /// </remarks>
        public Lazy(Func<T> valueFactory)
            : this(valueFactory, LazyThreadSafetyMode.ExecutionAndPublication)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Threading.Lazy{T}"/>
        /// class that uses <typeparamref name="T"/>'s default constructor and a specified thread-safety mode.
        /// </summary>
        /// <param name="isThreadSafe">true if this instance should be usable by multiple threads concurrently; false if the instance will only be used by one thread at a time.
        /// </param>
        public Lazy(bool isThreadSafe) :
            this(CreateInstance.Factory, GetMode(isThreadSafe))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Threading.Lazy{T}"/>
        /// class that uses <typeparamref name="T"/>'s default constructor and a specified thread-safety mode.
        /// </summary>
        /// <param name="mode">The lazy thread-safety mode mode</param>
        /// <exception cref="System.ArgumentOutOfRangeException"><paramref name="mode"/> mode contains an invalid valuee</exception>
        public Lazy(LazyThreadSafetyMode mode) :
            this(CreateInstance.Factory, mode)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Threading.Lazy{T}"/> class
        /// that uses a specified initialization function and a specified thread-safety mode.
        /// </summary>
        /// <param name="valueFactory">
        /// The <see cref="T:System.Func{T}"/> invoked to produce the lazily-initialized value when it is needed.
        /// </param>
        /// <param name="isThreadSafe">true if this instance should be usable by multiple threads concurrently; false if the instance will only be used by one thread at a time.
        /// </param>
        /// <exception cref="System.ArgumentNullException"><paramref name="valueFactory"/> is
        /// a null reference (Nothing in Visual Basic).</exception>
        public Lazy(Func<T> valueFactory, bool isThreadSafe) :
            this(valueFactory, GetMode(isThreadSafe))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Threading.Lazy{T}"/> class
        /// that uses a specified initialization function and a specified thread-safety mode.
        /// </summary>
        /// <param name="valueFactory">
        /// The <see cref="T:System.Func{T}"/> invoked to produce the lazily-initialized value when it is needed.
        /// </param>
        /// <param name="mode">The lazy thread-safety mode.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="valueFactory"/> is
        /// a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="System.ArgumentOutOfRangeException"><paramref name="mode"/> mode contains an invalid value.</exception>
        public Lazy(Func<T> valueFactory, LazyThreadSafetyMode mode)
        {
            if (valueFactory == null)
                throw new ArgumentNullException(nameof(valueFactory));

            m_implementation = CreateInitializerFromMode(mode, this, valueFactory);
        }

#region constructor helpers

        private static class CreateInstance
        {
            private static T Construct()
            {
                try
                {
                    return (T)(Activator.CreateInstance(typeof(T)));
                }
                catch (MissingMethodException)
                {
                    throw new MissingMemberException(Environment.GetResourceString("Lazy_CreateValue_NoParameterlessCtorForT"));
                }
            }

            public readonly static Func<T> Factory = Construct;
        }

        private static LazyThreadSafetyMode GetMode(bool isThreadSafe)
        {
            return isThreadSafe ? LazyThreadSafetyMode.ExecutionAndPublication : LazyThreadSafetyMode.None;
        }

        private static ILazyItem<T> CreateInitializerFromMode(LazyThreadSafetyMode mode, Lazy<T> owner, Func<T> valueFactory)
        {
            switch (mode)
            {
                case LazyThreadSafetyMode.None:                    return new None(owner, valueFactory);
                case LazyThreadSafetyMode.PublicationOnly:         return new PublicationOnly(owner, valueFactory);
                case LazyThreadSafetyMode.ExecutionAndPublication: return new ExecutionAndPublication(owner, valueFactory);

                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), Environment.GetResourceString("Lazy_ctor_ModeInvalid"));
            }
        }

#endregion

        /// <summary>
        /// Creates the underlying value using the factory object, placing the result inside this
        /// object, and then used the Lazy objects ILazyItem&lt;T&gt; implemenation.to refer to it.
        /// </summary>
        /// <param name="factory">The object factory used to create the underlying object</param>
        /// <param name="mode">The mode of the Lazy object</param>
        /// <returns>The underlying object</returns>
        private T CreateValue(Func<T> factory, LazyThreadSafetyMode mode)
        {
            try
            {
                m_value = factory();
                m_implementation = null;
                return m_value;
            }
            catch (Exception exception) when (!ReferenceEquals(CreateInstance.Factory, factory))
            {
                m_implementation = new LazyException(exception, mode);
                throw;
            }
        }

        /// <summary>
        /// Creates the underlying value using the factory object into a helper object and, for the
        /// first object to complete its factory, uses that objects ILazyItem&lt;T&gt; implementation
        /// </summary>
        /// <param name="factory">The object factory used to create the underlying object</param>
        /// <param name="comparand">The publication object, used for synchronisation and comparison</param>
        /// <returns>The underlying object</returns>
        private T CreateValuePublicationOnly(Func<T> factory, PublicationOnly comparand)
        {
            var possibleValue = factory();

            try {}
            finally
            {
                // we run this in a finally block to ensure that we don't get a partial completion due
                // to a Thread.Abort, which could mean that other threads might be left in infinite loops
                var previous = Interlocked.CompareExchange(ref m_implementation, this, comparand);
                if (previous == comparand)
                {
                    m_value = possibleValue;
                    m_implementation = null;
                }
            }

            return Value;
        }

        private void WaitForPublicationOnly()
        {
            while (!ReferenceEquals(m_implementation, null))
            {
                // CreateValuePublicationOnly temporarily sets m_implementation to "this". The Lazy implementation
                // has an explicit iplementation of ILazyItem which just waits for the m_value to be set, which is
                // signalled by m_implemenation then being set to null.
                Thread.Sleep(0);
            }
        }

        T ILazyItem<T>.Value
        {
            get
            {
                WaitForPublicationOnly();
                return Value;
            }
        }

        bool ILazyItem<T>.IsValueCreated
        {
            get
            {
                WaitForPublicationOnly();
                return IsValueCreated;
            }
        }

        bool ILazyItem<T>.IsValueFaulted
        {
            get
            {
                WaitForPublicationOnly();
                return IsValueFaulted;
            }
        }

        LazyThreadSafetyMode ILazyItem<T>.Mode
        {
            get
            {
                WaitForPublicationOnly();
                return Mode;
            }
        }

#region Serialization
        // to remain compatible with previous version, custom serialization has been added
        // which should be binary compatible. Only valid values were ever serialized. Exceptions
        // were thrown from the serializer, which halted it, if the Lazy object through an exception.

        /// <summary>
        /// wrapper class to box the initialized value, this is mainly created to avoid boxing/unboxing the value each time the value is called in case T is 
        /// a value type
        /// </summary>
        [Serializable]
        class Boxed
        {
            internal Boxed(T value)
            {
                m_value = value;
            }
            internal T m_value;
        }

        public Lazy(SerializationInfo information, StreamingContext context)
        {
            var boxed = (Boxed)information.GetValue("m_boxed", typeof(Boxed));
            m_value = boxed.m_value;
            m_implementation = null;
        }

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            var boxed = new Boxed(Value);
            info.AddValue("m_boxed", boxed);
        }

#endregion

        /// <summary>Forces initialization during serialization.</summary>
        /// <param name="context">The StreamingContext for the serialization operation.</param>
        [OnSerializing]
        private void OnSerializing(StreamingContext context)
        {
            // Force initialization
            T dummy = Value;
        }

        /// <summary>Creates and returns a string representation of this instance.</summary>
        /// <returns>The result of calling <see cref="System.Object.ToString"/> on the <see
        /// cref="Value"/>.</returns>
        /// <exception cref="T:System.NullReferenceException">
        /// The <see cref="Value"/> is null.
        /// </exception>
        public override string ToString()
        {
            return IsValueCreated ? Value.ToString() : Environment.GetResourceString("Lazy_ToString_ValueNotCreated");
        }

        /// <summary>Gets the value of the Lazy&lt;T&gt; for debugging display purposes.</summary>
        internal T ValueForDebugDisplay
        {
            get
            {
                if (!IsValueCreated)
                {
                    return default(T);
                }
                return Value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance may be used concurrently from multiple threads.
        /// </summary>
        internal LazyThreadSafetyMode Mode
        {
            get
            {
                var implementation = m_implementation;
                if (implementation == null)
                    return LazyThreadSafetyMode.None; // we don't know the mode anymore
                return implementation.Mode;
            }
        }

        /// <summary>
        /// Gets whether the value creation is faulted or not
        /// </summary>
        internal bool IsValueFaulted
        {
            get
            {
                var implementation = m_implementation;
                if (implementation == null)
                    return false;
                return implementation.IsValueFaulted;
            }
        }

        /// <summary>Gets a value indicating whether the <see cref="T:System.Lazy{T}"/> has been initialized.
        /// </summary>
        /// <value>true if the <see cref="T:System.Lazy{T}"/> instance has been initialized;
        /// otherwise, false.</value>
        /// <remarks>
        /// The initialization of a <see cref="T:System.Lazy{T}"/> instance may result in either
        /// a value being produced or an exception being thrown.  If an exception goes unhandled during initialization, 
        /// <see cref="IsValueCreated"/> will return false.
        /// </remarks>
        public bool IsValueCreated
        {
            get
            {
                var implementation = m_implementation;
                if (implementation == null)
                    return true;
                return implementation.IsValueCreated;
            }
        }

        /// <summary>Gets the lazily initialized value of the current <see
        /// cref="T:System.Threading.Lazy{T}"/>.</summary>
        /// <value>The lazily initialized value of the current <see
        /// cref="T:System.Threading.Lazy{T}"/>.</value>
        /// <exception cref="T:System.MissingMemberException">
        /// The <see cref="T:System.Threading.Lazy{T}"/> was initialized to use the default constructor 
        /// of the type being lazily initialized, and that type does not have a public, parameterless constructor.
        /// </exception>
        /// <exception cref="T:System.MemberAccessException">
        /// The <see cref="T:System.Threading.Lazy{T}"/> was initialized to use the default constructor 
        /// of the type being lazily initialized, and permissions to access the constructor were missing.
        /// </exception>
        /// <exception cref="T:System.InvalidOperationException">
        /// The <see cref="T:System.Threading.Lazy{T}"/> was constructed with the <see cref="T:System.Threading.LazyThreadSafetyMode.ExecutionAndPublication"/> or
        /// <see cref="T:System.Threading.LazyThreadSafetyMode.None"/>  and the initialization function attempted to access <see cref="Value"/> on this instance.
        /// </exception>
        /// <remarks>
        /// If <see cref="IsValueCreated"/> is false, accessing <see cref="Value"/> will force initialization.
        /// Please <see cref="System.Threading.LazyThreadSafetyMode"> for more information on how <see cref="T:System.Threading.Lazy{T}"/> will behave if an exception is thrown
        /// from initialization delegate.
        /// </remarks>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public T Value
        {
            get
            {
                var implementation = m_implementation;
                if (implementation == null)
                    return m_value;
                return implementation.Value;
            }
        }

#region ILazyItem<T> implementations

        private sealed class LazyException : ILazyItem<T>
        {
            private readonly System.Runtime.ExceptionServices.ExceptionDispatchInfo m_exceptionInfo;
            private readonly LazyThreadSafetyMode m_mode;

            internal LazyException(Exception exception, LazyThreadSafetyMode mode)
            {
                m_exceptionInfo = System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(exception);
                m_mode = mode;
            }

            public bool IsValueCreated => false;
            public bool IsValueFaulted => true;
            public LazyThreadSafetyMode Mode => m_mode;

            public T Value
            {
                get { m_exceptionInfo.Throw(); return default(T); }
            }
        }

        abstract private class LazyInitializer : ILazyItem<T>
        {
            protected Lazy<T> Owner { get; }
            protected Func<T> Factory { get; private set; }

            protected Func<T> TakeFactory()
            {
                // None and ExecutionAndPublication use TakeFactory to protect against re-enterency,
                // signalling recursion.
                var factory = Factory;
                if (!ReferenceEquals(CreateInstance.Factory, factory))
                {
                    if (factory == null)
                        throw new InvalidOperationException(Environment.GetResourceString("Lazy_Value_RecursiveCallsToValue"));
                    Factory = null;
                }
                return factory;
            }

            internal LazyInitializer(Lazy<T> owner, Func<T> factory)
            {
                Owner = owner;
                Factory = factory;
            }

            public bool IsValueCreated => false;
            public bool IsValueFaulted => false;
            abstract public T Value { get; }
            abstract public LazyThreadSafetyMode Mode { get; }
        }

        private sealed class None : LazyInitializer
        {
            internal None(Lazy<T> owner, Func<T> factory) : base(owner, factory) { }

            public override T Value
            {
                get { return Owner.CreateValue(TakeFactory(), LazyThreadSafetyMode.None); }
            }

            public override LazyThreadSafetyMode Mode
            {
                get { return LazyThreadSafetyMode.None; }
            }
        }

        private sealed class ExecutionAndPublication : LazyInitializer
        {
            internal ExecutionAndPublication(Lazy<T> owner, Func<T> factory) : base(owner, factory) { }

            public override T Value
            {
                get
                {
                    lock (this) // we're safe to lock on "this" as object is an private object used by Lazy
                    {
                        // it's possible for multiple calls to have piled up behind the lock, so we need to check
                        // to see if the ExecutionAndPublication object is still the current implementation.
                        return ReferenceEquals(Owner.m_implementation, this) ? Owner.CreateValue(TakeFactory(), LazyThreadSafetyMode.ExecutionAndPublication) : Owner.Value;
                    }
                }
            }

            public override LazyThreadSafetyMode Mode
            {
                get { return LazyThreadSafetyMode.ExecutionAndPublication; }
            }
        }

        private sealed class PublicationOnly : LazyInitializer
        {
            internal PublicationOnly(Lazy<T> owner, Func<T> factory) : base(owner, factory) { }

            public override T Value
            {
                get { return Owner.CreateValuePublicationOnly(Factory, this); }
            }

            public override LazyThreadSafetyMode Mode
            {
                get { return LazyThreadSafetyMode.PublicationOnly; }
            }
        }
#endregion
    }

    /// <summary>A debugger view of the Lazy&lt;T&gt; to surface additional debugging properties and 
    /// to ensure that the Lazy&lt;T&gt; does not become initialized if it was not already.</summary>
    internal sealed class System_LazyDebugView<T>
    {
        //The Lazy object being viewed.
        private readonly Lazy<T> m_lazy;

        /// <summary>Constructs a new debugger view object for the provided Lazy object.</summary>
        /// <param name="lazy">A Lazy object to browse in the debugger.</param>
        public System_LazyDebugView(Lazy<T> lazy)
        {
            m_lazy = lazy;
        }

        /// <summary>Returns whether the Lazy object is initialized or not.</summary>
        public bool IsValueCreated
        {
            get { return m_lazy.IsValueCreated; }
        }

        /// <summary>Returns the value of the Lazy object.</summary>
        public T Value
        {
            get
            { return m_lazy.ValueForDebugDisplay; }
        }

        /// <summary>Returns the execution mode of the Lazy object</summary>
        public LazyThreadSafetyMode Mode
        {
            get { return m_lazy.Mode; }
        }

        /// <summary>Returns the execution mode of the Lazy object</summary>
        public bool IsValueFaulted
        {
            get { return m_lazy.IsValueFaulted; }
        }
    }
}
