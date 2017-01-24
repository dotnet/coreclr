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
    internal enum LazyState
    {
        NoneViaConstructor = 0,
        NoneViaFactory     = 1,
        NoneException      = 2,

        PublicationOnlyViaConstructor = 3,
        PublicationOnlyViaFactory     = 4,
        PublicationOnlyWait           = 5,
        PublicationOnlyException      = 6,

        ExecutionAndPublicationViaConstructor = 7,
        ExecutionAndPublicationViaFactory     = 8,
        ExecutionAndPublicationException      = 9,
    }

    internal class LazyNonGeneric
    {
        internal readonly static LazyNonGeneric NoneViaConstructor            = new LazyNonGeneric(LazyState.NoneViaConstructor);
        internal readonly static LazyNonGeneric NoneViaFactory                = new LazyNonGeneric(LazyState.NoneViaFactory);
        internal readonly static LazyNonGeneric PublicationOnlyViaConstructor = new LazyNonGeneric(LazyState.PublicationOnlyViaConstructor);
        internal readonly static LazyNonGeneric PublicationOnlyViaFactory     = new LazyNonGeneric(LazyState.PublicationOnlyViaFactory);
        internal readonly static LazyNonGeneric PublicationOnlySpinWait       = new LazyNonGeneric(LazyState.PublicationOnlyWait);

        internal LazyState State { get; }

        private readonly System.Runtime.ExceptionServices.ExceptionDispatchInfo _exceptionDispatch;

        /// <summary>
        /// Constructor that defines the state
        /// </summary>
        /// <param name="state"></param>
        internal LazyNonGeneric(LazyState state)
        {
            State = state;
        }

        private Exception InvalidLogic()
        {
            // correctly implemented, we should never create this exception
            return new Exception("Invalid logic; execution should not get here");
        }

        /// <summary>
        /// Constructor used for exceptions
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="exception"></param>
        internal LazyNonGeneric(LazyThreadSafetyMode mode, Exception exception)
        {
            if (mode == LazyThreadSafetyMode.ExecutionAndPublication)
                State = LazyState.ExecutionAndPublicationException;
            else if (mode == LazyThreadSafetyMode.None)
                State = LazyState.NoneException;
            else if (mode == LazyThreadSafetyMode.PublicationOnly)
                State = LazyState.PublicationOnlyException;
            else
                throw InvalidLogic();

            _exceptionDispatch = System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(exception);
        }

        internal void ThrowException()
        {
            if (_exceptionDispatch == null)
                throw InvalidLogic();

            _exceptionDispatch.Throw();
        }

        private LazyThreadSafetyMode GetMode()
        {
            switch (State)
            {
                case LazyState.NoneViaConstructor:
                case LazyState.NoneViaFactory:
                case LazyState.NoneException:
                    return LazyThreadSafetyMode.None;

                case LazyState.PublicationOnlyViaConstructor:
                case LazyState.PublicationOnlyViaFactory:
                case LazyState.PublicationOnlyWait:
                case LazyState.PublicationOnlyException:
                    return LazyThreadSafetyMode.PublicationOnly;

                case LazyState.ExecutionAndPublicationViaConstructor:
                case LazyState.ExecutionAndPublicationViaFactory:
                case LazyState.ExecutionAndPublicationException:
                    return LazyThreadSafetyMode.ExecutionAndPublication;

                default:
                    throw InvalidLogic();
            }
        }

        internal static LazyThreadSafetyMode GetMode(LazyNonGeneric state)
        {
            if (state == null)
                return LazyThreadSafetyMode.None; // we don't know the mode anymore
            return state.GetMode();
        }

        private bool GetIsValueFaulted()
        {
            return _exceptionDispatch != null;
        }

        internal static bool GetIsValueFaulted(LazyNonGeneric state)
        {
            if (state == null)
                return false;
            return state.GetIsValueFaulted();
        }

        internal static LazyNonGeneric Create(LazyThreadSafetyMode mode, bool useDefaultConstructor)
        {
            if (mode == LazyThreadSafetyMode.None)
            {
                return useDefaultConstructor ? NoneViaConstructor : NoneViaFactory;
            }

            if (mode == LazyThreadSafetyMode.PublicationOnly)
            {
                return useDefaultConstructor ? PublicationOnlyViaConstructor : PublicationOnlyViaFactory;
            }

            if (mode == LazyThreadSafetyMode.ExecutionAndPublication)
            {
                // we need to create an object for ExecutionAndPublication because we use Monitor-based locking
                var state = useDefaultConstructor ? LazyState.ExecutionAndPublicationViaConstructor : LazyState.ExecutionAndPublicationViaFactory;
                return new LazyNonGeneric(state);
            }

            throw new ArgumentOutOfRangeException(nameof(mode), Environment.GetResourceString("Lazy_ctor_ModeInvalid"));
        }

        internal static object CreateViaDefaultConstructor(Type type)
        {
            try
            {
                return Activator.CreateInstance(type);
            }
            catch (MissingMethodException)
            {
                throw new MissingMemberException(Environment.GetResourceString("Lazy_CreateValue_NoParameterlessCtorForT"));
            }
        }

        internal static LazyThreadSafetyMode GetModeFromIsThreadSafe(bool isThreadSafe)
        {
            return isThreadSafe ? LazyThreadSafetyMode.ExecutionAndPublication : LazyThreadSafetyMode.None;
        }
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
    {
        private static T CreateViaDefaultConstructor()
        {
            return (T)LazyNonGeneric.CreateViaDefaultConstructor(typeof(T));
        }

        // _state, a volatile reference, is set to null after m_value has been set
        [NonSerialized]
        private volatile LazyNonGeneric _state;

        // we ensure that m_factory when finished is set to null to allow garbage collector to clean up
        // any referenced items
        [NonSerialized]
        private Func<T> _factory;

        // m_value eventually stores the lazily created value. It is ready when _state = null.
        private T _value;

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
            _value = value;
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
            this(null, LazyNonGeneric.GetModeFromIsThreadSafe(isThreadSafe), true)
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
            this(valueFactory, LazyNonGeneric.GetModeFromIsThreadSafe(isThreadSafe), false)
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

            _factory = valueFactory;
            _state = LazyNonGeneric.Create(mode, useDefaultConstructor);
        }

        private T ViaConstructor()
        {
            _value = CreateViaDefaultConstructor();
            _state = null;

            return _value;
        }

        private T ViaFactory(LazyThreadSafetyMode mode)
        {
            try
            {
                var factory = _factory;
                if (factory == null)
                    throw new InvalidOperationException(Environment.GetResourceString("Lazy_Value_RecursiveCallsToValue"));
                _factory = null;

                _value = factory();
                _state = null;

                return _value;
            }
            catch (Exception exception)
            {
                _state = new LazyNonGeneric(mode, exception);
                throw;
            }
        }

        private T ExecutionAndPublication(LazyNonGeneric executionAndPublication, bool useDefaultConstructor)
        {
            lock (executionAndPublication)
            {
                // it's possible for multiple calls to have piled up behind the lock, so we need to check
                // to see if the ExecutionAndPublication object is still the current implementation.
                if (ReferenceEquals(_state, executionAndPublication))
                    return useDefaultConstructor ? ViaConstructor() : ViaFactory(LazyThreadSafetyMode.ExecutionAndPublication);

                return Value;
            }
        }

        private T PublicationOnly(LazyNonGeneric publicationOnly, T possibleValue)
        {
            try { }
            finally
            {
                // we run this in a finally block to ensure that we don't get a partial completion due
                // to a Thread.Abort, which could mean that other threads might be left in infinite loops
                var previous = Interlocked.CompareExchange(ref _state, LazyNonGeneric.PublicationOnlySpinWait, publicationOnly);
                if (previous == publicationOnly)
                {
                    _value = possibleValue;
                    _factory = null;
                    _state = null;
                }
            }

            return Value;
        }

        private T PublicationOnlyViaConstructor(LazyNonGeneric initializer)
        {
            return PublicationOnly(initializer, CreateViaDefaultConstructor());
        }

        private T PublicationOnlyViaFactory(LazyNonGeneric initializer)
        {
            var factory = _factory;
            if (factory == null)
                return PublicationOnlySpinWait();
            return PublicationOnly(initializer, factory());
        }

        private T PublicationOnlySpinWait()
        {
            var spinWait = new SpinWait();
            while (!ReferenceEquals(_state, null))
            {
                // We get here when PublicationOnly temporarily sets _state to LazyNonGeneric.PublicationOnlySpinWait.
                // This temporary state should be quickly followed by _state being set to null.
                spinWait.SpinOnce();
            }
            return Value;
        }

        private T LazyGetValue(LazyNonGeneric state)
        {
            switch (state.State)
            {
                case LazyState.NoneViaConstructor:                    return ViaConstructor();
                case LazyState.NoneViaFactory:                        return ViaFactory(LazyThreadSafetyMode.None);

                case LazyState.PublicationOnlyViaConstructor:         return PublicationOnlyViaConstructor(state);
                case LazyState.PublicationOnlyViaFactory:             return PublicationOnlyViaFactory(state);
                case LazyState.PublicationOnlyWait:                   return PublicationOnlySpinWait();

                case LazyState.ExecutionAndPublicationViaConstructor: return ExecutionAndPublication(state, true);
                case LazyState.ExecutionAndPublicationViaFactory:     return ExecutionAndPublication(state, false);

                default:
                    state.ThrowException();
                    return default(T);
            }
        }

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
            get { return LazyNonGeneric.GetMode(_state); }
        }

        /// <summary>
        /// Gets whether the value creation is faulted or not
        /// </summary>
        internal bool IsValueFaulted
        {
            get { return LazyNonGeneric.GetIsValueFaulted(_state); }
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
            get { return _state == null; }
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
                var state = _state;
                if (state == null)
                    return _value;
                return LazyGetValue(state);
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
