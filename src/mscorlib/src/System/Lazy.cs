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
    class LazyImplementation
    {
        public readonly static LazyImplementation NoneViaConstructor            = new LazyImplementation(LazyThreadSafetyMode.None, true);
        public readonly static LazyImplementation NoneViaFactory                = new LazyImplementation(LazyThreadSafetyMode.None, false);
        public readonly static LazyImplementation PublicationOnlyViaConstructor = new LazyImplementation(LazyThreadSafetyMode.PublicationOnly, true);
        public readonly static LazyImplementation PublicationOnlyViaFactory     = new LazyImplementation(LazyThreadSafetyMode.PublicationOnly, false);
        public readonly static LazyImplementation PublicationOnlySpinWait       = new LazyImplementation();

        private readonly LazyThreadSafetyMode m_mode;
        private readonly bool m_publicationOnlyWaiting;
        private readonly bool m_useDefaultConstructor;
        private readonly System.Runtime.ExceptionServices.ExceptionDispatchInfo m_exceptionDispatch;

        /// <summary>
        /// Constructor used for lazy construction
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="useDefaultConstructor"></param>
        public LazyImplementation(LazyThreadSafetyMode mode, bool useDefaultConstructor)
        {
            m_mode = mode;
            m_publicationOnlyWaiting = false;
            m_useDefaultConstructor = useDefaultConstructor;
            m_exceptionDispatch = null;
        }

        /// <summary>
        /// Constructor used for exceptions
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="exception"></param>
        public LazyImplementation(LazyThreadSafetyMode mode, Exception exception)
        {
            m_mode = mode;
            m_publicationOnlyWaiting = false;
            m_useDefaultConstructor = false;
            m_exceptionDispatch = System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(exception);
        }

        /// <summary>
        /// Constructor used for the spin state that is required for PublicationOnly to funciton correctly
        /// </summary>
        private LazyImplementation()
        {
            m_mode = LazyThreadSafetyMode.PublicationOnly;
            m_publicationOnlyWaiting = true;
            m_useDefaultConstructor = false;
            m_exceptionDispatch = null;
        }

        public T GetValue<T>(Lazy<T> owner)
        {
            // If we are an exception-based then we throw that
            if (m_exceptionDispatch != null)
                m_exceptionDispatch.Throw();

            if (m_mode == LazyThreadSafetyMode.ExecutionAndPublication)
                return owner.PopulateValueExecutionAndPublication(this, m_useDefaultConstructor);

            if (m_mode == LazyThreadSafetyMode.None)
                return owner.PopulateValue(LazyThreadSafetyMode.None, m_useDefaultConstructor);

            // Otherwise we're PublicationOnly, but we could be the creator or the spinner

            if (m_publicationOnlyWaiting)
            {
                owner.WaitForPublicationOnly();
                return owner.Value;
            }

            return owner.PopulateValuePublicationOnly(this, PublicationOnlySpinWait, m_useDefaultConstructor);
        }

        public LazyThreadSafetyMode GetMode() { return m_mode; }
        public bool GetIsValueFaulted() { return m_exceptionDispatch != null; }
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
    {
        private static T CreateViaDefaultConstructor()
        {
            try
            {
                return (T)Activator.CreateInstance(typeof(T));
            }
            catch (MissingMethodException)
            {
                throw new MissingMemberException(Environment.GetResourceString("Lazy_CreateValue_NoParameterlessCtorForT"));
            }
        }

        // m_implementation, a volatile reference, is set to null after m_value has been set
        private volatile LazyImplementation m_implementation;

        // we ensure that m_factory when finished is set to null to allow garbage collector to clean up
        // any referenced items
        private Func<T> m_factory;

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
            : this(null, LazyThreadSafetyMode.ExecutionAndPublication, true)
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
            m_factory = null;
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
            : this(valueFactory, LazyThreadSafetyMode.ExecutionAndPublication, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Threading.Lazy{T}"/>
        /// class that uses <typeparamref name="T"/>'s default constructor and a specified thread-safety mode.
        /// </summary>
        /// <param name="isThreadSafe">true if this instance should be usable by multiple threads concurrently; false if the instance will only be used by one thread at a time.
        /// </param>
        public Lazy(bool isThreadSafe) :
            this(null, GetMode(isThreadSafe), true)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Threading.Lazy{T}"/>
        /// class that uses <typeparamref name="T"/>'s default constructor and a specified thread-safety mode.
        /// </summary>
        /// <param name="mode">The lazy thread-safety mode mode</param>
        /// <exception cref="System.ArgumentOutOfRangeException"><paramref name="mode"/> mode contains an invalid valuee</exception>
        public Lazy(LazyThreadSafetyMode mode) :
            this(null, mode, true)
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
            this(valueFactory, GetMode(isThreadSafe), false)
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
            : this(valueFactory, mode, false)
        {
        }

        private Lazy(Func<T> valueFactory, LazyThreadSafetyMode mode, bool useDefaultConstructor)
        {
            if (valueFactory == null && !useDefaultConstructor)
                throw new ArgumentNullException(nameof(valueFactory));

            m_factory = valueFactory;

            if (mode == LazyThreadSafetyMode.None)
            {
                m_implementation = useDefaultConstructor ? LazyImplementation.NoneViaConstructor : LazyImplementation.NoneViaFactory;
            }
            else if (mode == LazyThreadSafetyMode.PublicationOnly)
            {
                m_implementation = useDefaultConstructor ? LazyImplementation.PublicationOnlyViaConstructor : LazyImplementation.PublicationOnlyViaFactory;
            }
            else if (mode == LazyThreadSafetyMode.ExecutionAndPublication)
            {
                m_implementation = new LazyImplementation(LazyThreadSafetyMode.ExecutionAndPublication, useDefaultConstructor);
            }
            else
            {
                throw new Exception();
            }
        }

        private static LazyThreadSafetyMode GetMode(bool isThreadSafe)
        {
            return isThreadSafe ? LazyThreadSafetyMode.ExecutionAndPublication : LazyThreadSafetyMode.None;
        }

        /// <summary>
        /// Creates the underlying value using the factory object, placing the result inside this
        /// object, and then used the Lazy objects LazyWorker&lt;T&gt; implemenation.to refer to it.
        /// </summary>
        /// <param name="factory">The object factory used to create the underlying object</param>
        /// <param name="mode">The mode of the Lazy object</param>
        /// <returns>The underlying object</returns>
        internal T PopulateValue(LazyThreadSafetyMode mode, bool useDefaultConstructor)
        {
            if (useDefaultConstructor)
            {
                m_value = CreateViaDefaultConstructor();
                m_implementation = null;
            }
            else
            {
                try
                {
                    var factory = m_factory;
                    if (factory == null)
                        throw new InvalidOperationException(Environment.GetResourceString("Lazy_Value_RecursiveCallsToValue"));
                    m_factory = null;

                    m_value = factory();
                    m_implementation = null;
                }
                catch (Exception exception)
                {
                    m_implementation = new LazyImplementation(mode, exception);
                    throw;
                }
            }
            return m_value;
        }

        internal T PopulateValueExecutionAndPublication(LazyImplementation sync, bool useDefaultConstructor)
        {
            lock (sync) // we're safe to lock on "this" as object is an private object used by Lazy
            {
                // it's possible for multiple calls to have piled up behind the lock, so we need to check
                // to see if the ExecutionAndPublication object is still the current implementation.
                if (ReferenceEquals(m_implementation, sync))
                    return PopulateValue(LazyThreadSafetyMode.ExecutionAndPublication, useDefaultConstructor);
                return Value;
            }
        }

        /// <summary>
        /// Creates the underlying value using the factory object into a helper object and, for the
        /// first object to complete its factory, uses that objects LazyWorker&lt;T&gt; implementation
        /// </summary>
        /// <param name="factory">The object factory used to create the underlying object</param>
        /// <param name="comparand">The publication object, used for synchronisation and comparison</param>
        /// <returns>The underlying object</returns>
        internal T PopulateValuePublicationOnly(LazyImplementation initializer, LazyImplementation spinner, bool useDefaultConstructor)
        {
            T possibleValue;
            if (useDefaultConstructor)
            {
                possibleValue = CreateViaDefaultConstructor();
            }
            else
            {
                var factory = m_factory;
                if (factory == null)
                    return spinner.GetValue(this);
                possibleValue = factory();
            }

            try { }
            finally
            {
                // we run this in a finally block to ensure that we don't get a partial completion due
                // to a Thread.Abort, which could mean that other threads might be left in infinite loops
                var previous = Interlocked.CompareExchange(ref m_implementation, spinner, initializer);
                if (previous == initializer)
                {
                    m_value = possibleValue;
                    m_factory = null;
                    m_implementation = null;
                }
            }

            return Value;
        }

        internal void WaitForPublicationOnly()
        {
            while (!ReferenceEquals(m_implementation, null))
            {
                // CreateValuePublicationOnly temporarily sets m_implementation to "this". The Lazy implementation
                // has an explicit iplementation of LazyWorker which just waits for the m_value to be set, which is
                // signalled by m_implemenation then being set to null.
                Thread.Sleep(0);
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
                return implementation.GetMode();
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
                return implementation.GetIsValueFaulted();
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
            get { return m_implementation == null; }
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

                return implementation.GetValue(this);
            }
        }
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
