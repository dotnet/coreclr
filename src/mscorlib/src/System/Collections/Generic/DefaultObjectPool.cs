// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;
using System.Threading;

namespace System.Collections.Generic
{
    internal class DefaultObjectPool<T> : ObjectPool<T> where T : class
    {
        /// <summary>The default maximum number of oebjects that are available for rent.</summary>
        private const int DefaultMaxPooled = 128;

        private readonly T[] _objects;

        private SpinLock _lock; // do not make this readonly; it's a mutable struct
        private int _index;

        /// <summary>
        /// Creates the pool with DefaultMaxPooled objects.
        /// </summary>
        public DefaultObjectPool()
            : this(DefaultMaxPooled)
        {
        }

        /// <summary>
        /// Creates the pool with maxPooled objects.
        /// </summary>
        public DefaultObjectPool(int maxPooled)
        {
            if (maxPooled <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxPooled));
            }

            _lock = new SpinLock(Debugger.IsAttached); // only enable thread tracking if debugger is attached; it adds non-trivial overheads to Enter/Exit
            _objects = new T[maxPooled];
        }

        /// <summary>Tries to take an object from the pool, returns true if sucessful.</summary>
        public override bool TryRent(out T obj)
        {
            T[] objects = _objects;
            obj = null;
            // While holding the lock, grab whatever is at the next available index and
            // update the index.  We do as little work as possible while holding the spin
            // lock to minimize contention with other threads.  The try/finally is
            // necessary to properly handle thread aborts on platforms which have them.
            bool lockTaken = false;
            try
            {
                _lock.Enter(ref lockTaken);

                if (_index < objects.Length)
                {
                    obj = objects[_index];
                    objects[_index++] = null;
                }
            }
            finally
            {
                if (lockTaken) _lock.Exit(false);
            }

            return obj != null;
        }

        /// <summary>
        /// Attempts to return the object to the pool.  If successful, the object will be stored
        /// in the pool; otherwise, the buffer won't be stored.
        /// </summary>
        public override void Return(T obj)
        {
            if (obj == null)
            {
                return;
            }

            // While holding the spin lock, if there's room available in the array,
            // put the object into the next available slot.  Otherwise, we just drop it.
            // The try/finally is necessary to properly handle thread aborts on platforms
            // which have them.
            bool lockTaken = false;
            try
            {
                _lock.Enter(ref lockTaken);

                if (_index != 0)
                {
                    _objects[--_index] = obj;
                }
            }
            finally
            {
                if (lockTaken) _lock.Exit(false);
            }
        }
    }
}
