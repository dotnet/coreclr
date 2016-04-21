// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Runtime.CompilerServices;
using System.Threading;

namespace System.Collections.Generic
{
     /// <summary>
     /// Provides a resource pool that enables reusing instances of type <see cref="T:T"/>. It is up to the user to ensure these objects are reset before returning to pool.
     /// </summary>
     /// <remarks>
     /// <para>
     /// Ensure these objects are reset before returning to pool. This is so they do not maintain references to objects that should be GC'd and they are ready for reuse.
     /// </para>
     /// <para>
     /// Renting and returning objects with an <see cref="ObjectPool{T}"/> can increase performance
     /// in situations where objects are created and destroyed frequently, resulting in significant
     /// memory pressure on the garbage collector.
     /// </para>
     /// <para>
     /// This class is thread-safe.  All members may be used by multiple threads concurrently.
     /// </para>
     /// </remarks>
    internal abstract class ObjectPool<T> where T : class
    {
        /// <summary>The lazily-initialized shared pool instance.</summary>
        private static ObjectPool<T> s_sharedInstance;

        /// <summary>
        /// Retrieves a shared <see cref="ObjectPool{T}"/> instance.
        /// </summary>
        /// <remarks>
        /// The shared pool provides a default implementation of <see cref="ObjectPool{T}"/>
        /// that's intended for general applicability. Renting an object from it with <see cref="Rent"/> will result in an 
        /// existing object being taken from the pool if one is available or in a new 
        /// object being allocated if one is not available.
        /// </remarks>
        public static ObjectPool<T> Shared
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return Volatile.Read(ref s_sharedInstance) ?? EnsureSharedCreated(); }
        }

        /// <summary>Ensures that <see cref="s_sharedInstance"/> has been initialized to a pool and returns it.</summary>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static ObjectPool<T> EnsureSharedCreated()
        {
            Interlocked.CompareExchange(ref s_sharedInstance, Create(), null);
            return s_sharedInstance;
        }

        /// <summary>
        /// Creates a new <see cref="ObjectPool{T}"/> instance using default configuration options.
        /// </summary>
        /// <returns>A new <see cref="ObjectPool{T}"/> instance.</returns>
        public static ObjectPool<T> Create()
        {
            return new DefaultObjectPool<T>();
        }

        /// <summary>
        /// Creates a new <see cref="ObjectPool{T}"/> instance using custom configuration options.
        /// </summary>
        /// <param name="maxPooled">The maximum number of object instances that may be stored in the pool.</param>
        /// <returns>A new <see cref="ObjectPool{T}"/> instance with the specified configuration options.</returns>
        public static ObjectPool<T> Create(int maxPooled)
        {
            return new DefaultObjectPool<T>(maxPooled);
        }

        public abstract bool TryRent(out T value);

        /// <summary>
        /// Returns to the pool an object that was previously obtained via <see cref="Rent"/> on the same 
        /// <see cref="ObjectPool{T}"/> instance.
        /// Ensure this object is reset before returning to pool. This is so it does do not maintain references to objects that should be GC'd and they are ready for reuse.
        /// </summary>
        /// <param name="obj">
        /// The object previously obtained from <see cref="Rent"/> to return to the pool.
        /// </param>
        /// <remarks>
        /// <para>
        /// Ensure the object is reset before returning to pool. This is so it does not maintain references to objects that should be GC'd and they are ready for reuse.
        /// </para>
        /// <para>
        /// Once an object has been returned to the pool, the caller gives up all ownership of the buffer 
        /// and must not use it. The reference returned from a given call to <see cref="Rent"/> must only be
        /// returned via <see cref="Return"/> once.  The default <see cref="ObjectPool{T}"/>
        /// may hold onto the returned object in order to rent it again, or it may release the returned object
        /// if it's determined that the pool already has enough objects stored.
        /// </para>
        /// </remarks>
        public abstract void Return(T obj);
    }
}
