// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

/*============================================================
** 
**
** Description: Compiler support for runtime-generated "object fields."
**
**    Lets DLR and other language compilers expose the ability to
**    attach arbitrary "properties" to instanced managed objects at runtime.
**
**    We expose this support as a dictionary whose keys are the
**    instanced objects and the values are the "properties."
**
**    Unlike a regular dictionary, ConditionalWeakTables will not
**    keep keys alive.
**
**
** Lifetimes of keys and values:
**
**    Inserting a key and value into the dictonary will not
**    prevent the key from dying, even if the key is strongly reachable
**    from the value.
**
**    Prior to ConditionalWeakTable, the CLR did not expose
**    the functionality needed to implement this guarantee.
**
**    Once the key dies, the dictionary automatically removes
**    the key/value entry.
**
**
** Relationship between ConditionalWeakTable and Dictionary:
**
**    ConditionalWeakTable mirrors the form and functionality
**    of the IDictionary interface for the sake of api consistency.
**
**    Unlike Dictionary, ConditionalWeakTable is fully thread-safe
**    and requires no additional locking to be done by callers.
**
**    ConditionalWeakTable defines equality as Object.ReferenceEquals().
**    ConditionalWeakTable does not invoke GetHashCode() overrides.
**
**    It is not intended to be a general purpose collection
**    and it does not formally implement IDictionary or
**    expose the full public surface area.
**
**
** Thread safety guarantees:
**
**    ConditionalWeakTable is fully thread-safe and requires no
**    additional locking to be done by callers.
**
**
** OOM guarantees:
**
**    Will not corrupt unmanaged handle table on OOM. No guarantees
**    about managed weak table consistency. Native handles reclamation
**    may be delayed until appdomain shutdown.
===========================================================*/

using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

namespace System.Runtime.CompilerServices
{
    #region ConditionalWeakTable
    public sealed class ConditionalWeakTable<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
        where TKey : class
        where TValue : class
    {
        #region Fields
        private const int InitialCapacity = 8;  // Initial length of the table. Must be a power of two.
        private readonly object _lock;          // This lock protects all mutation of data in the table.  Readers do not take this lock.
        private volatile Container _container;  // The actual storage for the table; swapped out as the table grows.
        private int _activeEnumeratorRefCount;  // The number of outstanding enumerators on the table
        #endregion

        #region Constructors
        public ConditionalWeakTable()
        {
            _lock = new object();
            _container = new Container(this);
        }
        #endregion

        #region Public Members
        //--------------------------------------------------------------------------------------------
        // key:   key of the value to find. Cannot be null.
        // value: if the key is found, contains the value associated with the key upon method return.
        //        if the key is not found, contains default(TValue).
        //
        // Method returns "true" if key was found, "false" otherwise.
        //
        // Note: The key may get garbaged collected during the TryGetValue operation. If so, TryGetValue
        // may at its discretion, return "false" and set "value" to the default (as if the key was not present.)
        //--------------------------------------------------------------------------------------------
        public bool TryGetValue(TKey key, out TValue value)
        {
            if (key == null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.key);
            }

            return _container.TryGetValueWorker(key, out value);
        }

        //--------------------------------------------------------------------------------------------
        // key: key to add. May not be null.
        // value: value to associate with key.
        //
        // If the key is already entered into the dictionary, this method throws an exception.
        //
        // Note: The key may get garbage collected during the Add() operation. If so, Add()
        // has the right to consider any prior entries successfully removed and add a new entry without
        // throwing an exception.
        //--------------------------------------------------------------------------------------------
        public void Add(TKey key, TValue value)
        {
            if (key == null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.key);
            }

            lock (_lock)
            {
                object otherValue;
                int entryIndex = _container.FindEntry(key, out otherValue);
                if (entryIndex != -1)
                {
                    ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_AddingDuplicate);
                }

                CreateEntry(key, value);
            }
        }

        //--------------------------------------------------------------------------------------------
        // key: key to add or update. May not be null.
        // value: value to associate with key.
        //
        // If the key is already entered into the dictionary, this method will update the value associated with key.
        //--------------------------------------------------------------------------------------------
        public void AddOrUpdate(TKey key, TValue value)
        {
            if (key == null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.key);
            }

            lock (_lock)
            {
                object otherValue;
                int entryIndex = _container.FindEntry(key, out otherValue);

                // if we found a key we should just update, if no we should create a new entry.
                if (entryIndex != -1)
                {
                    _container.UpdateValue(entryIndex, value);
                }
                else
                {
                    CreateEntry(key, value);
                }

            }
        }

        //--------------------------------------------------------------------------------------------
        // key: key to remove. May not be null.
        //
        // Returns true if the key is found and removed. Returns false if the key was not in the dictionary.
        //
        // Note: The key may get garbage collected during the Remove() operation. If so,
        // Remove() will not fail or throw, however, the return value can be either true or false
        // depending on who wins the race.
        //--------------------------------------------------------------------------------------------
        public bool Remove(TKey key)
        {
            if (key == null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.key);
            }

            lock (_lock)
            {
                return _container.Remove(key);
            }
        }

        //--------------------------------------------------------------------------------------------
        // Clear all the key/value pairs
        //--------------------------------------------------------------------------------------------
        public void Clear()
        {
            lock (_lock)
            {
                // To clear, we would prefer to simply drop the existing container
                // and replace it with an empty one, as that's overall more efficient.
                // However, if there are any active enumerators, we don't want to do
                // that as it will end up removing all of the existing entries and
                // allowing new items to be added at the same indices when the container
                // is filled and replaced, and one of the guarantees we try to make with
                // enumeration is that new items added after enumeration starts won't be
                // included in the enumeration. As such, if there are active enumerators,
                // we simply use the container's removal functionality to remove all of the
                // keys; then when the table is resized, if there are still active enumerators,
                // these empty slots will be maintained.
                if (_activeEnumeratorRefCount > 0)
                {
                    _container.RemoveAllKeys();
                }
                else
                {
                    _container = new Container(this);
                }
            }
        }

        //--------------------------------------------------------------------------------------------
        // key:                 key of the value to find. Cannot be null.
        // createValueCallback: callback that creates value for key. Cannot be null.
        //
        // Atomically tests if key exists in table. If so, returns corresponding value. If not,
        // invokes createValueCallback() passing it the key. The returned value is bound to the key in the table
        // and returned as the result of GetValue().
        //
        // If multiple threads try to initialize the same key, the table may invoke createValueCallback
        // multiple times with the same key. Exactly one of these calls will succeed and the returned
        // value of that call will be the one added to the table and returned by all the racing GetValue() calls.
        // 
        // This rule permits the table to invoke createValueCallback outside the internal table lock
        // to prevent deadlocks.
        //--------------------------------------------------------------------------------------------
        public TValue GetValue(TKey key, CreateValueCallback createValueCallback)
        {
            // key is validated by TryGetValue

            if (createValueCallback == null)
            {
                throw new ArgumentNullException(nameof(createValueCallback));
            }

            TValue existingValue;
            return TryGetValue(key, out existingValue) ?
                existingValue :
                GetValueLocked(key, createValueCallback);
        }

        private TValue GetValueLocked(TKey key, CreateValueCallback createValueCallback)
        {
            // If we got here, the key was not in the table. Invoke the callback (outside the lock)
            // to generate the new value for the key. 
            TValue newValue = createValueCallback(key);

            lock (_lock)
            {
                // Now that we've taken the lock, must recheck in case we lost a race to add the key.
                TValue existingValue;
                if (_container.TryGetValueWorker(key, out existingValue))
                {
                    return existingValue;
                }
                else
                {
                    // Verified in-lock that we won the race to add the key. Add it now.
                    CreateEntry(key, newValue);
                    return newValue;
                }
            }
        }

        //--------------------------------------------------------------------------------------------
        // key:                 key of the value to find. Cannot be null.
        //
        // Helper method to call GetValue without passing a creation delegate.  Uses Activator.CreateInstance
        // to create new instances as needed.  If TValue does not have a default constructor, this will
        // throw.
        //--------------------------------------------------------------------------------------------

        public TValue GetOrCreateValue(TKey key) => GetValue(key, _ => Activator.CreateInstance<TValue>());

        public delegate TValue CreateValueCallback(TKey key);

        //--------------------------------------------------------------------------------------------
        // Gets an enumerator for the table.  The returned enumerator will not extend the lifetime of
        // any object pairs in the table, other than the one that's Current.  It will not return entries
        // that have already been collected, nor will it return entries added after the enumerator was
        // retrieved.  It may not return all entries that were present when the enumerat was retrieved,
        // however, such as not returning entries that were collected or removed after the enumerator
        // was retrieved but before they were enumerated.
        //--------------------------------------------------------------------------------------------
        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        {
            lock (_lock)
            {
                Container c = _container;
                return c == null || c.FirstFreeEntry == 0 ?
                    ((IEnumerable<KeyValuePair<TKey, TValue>>)Array.Empty<KeyValuePair<TKey, TValue>>()).GetEnumerator() :
                    new Enumerator(this);
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<KeyValuePair<TKey, TValue>>)this).GetEnumerator();

        /// <summary>Provides an enumerator for the table.</summary>
        private sealed class Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>
        {
            // The enumerator would ideally hold a reference to the Container and the end index within that
            // container.  However, the safety of the CWT depends on the only reference to the Container being
            // from the CWT itself; the Container then employs a two-phase finalization scheme, where the first
            // phase nulls out that parent CWT's reference, guaranteeing that the second time it's finalized there
            // can be no other existing references to it in use that would allow for concurrent usage of the
            // native handles with finalization.  We would break that if we allowed this Enumerator to hold a
            // reference to the Container.  Instead, the Enumerator holds a reference to the CWT rather than to
            // the Container, and it maintains the CWT._activeEnumeratorRefCount field to track whether there
            // are outstanding enumerators that have yet to be disposed/finalized.  If there aren't any, the CWT
            // behaves as it normally does.  If there are, certain operations are affected, in particular resizes.
            // Normally when the CWT is resized, it enumerates the contents of the table looking for indices that
            // contain entries which have been collected or removed, and it frees those up, effectively moving
            // down all subsequent entries in the container (not in the existing container, but in a replacement).
            // This, however, would cause the enumerator's understanding of indices to break.  So, as long as
            // there is any outstanding enumerator, no compaction is performed.

            private ConditionalWeakTable<TKey, TValue> _table; // parent table, set to null when disposed
            private readonly int _maxIndexInclusive;           // last index in the container that should be enumerated
            private int _currentIndex = -1;                    // the current index into the container
            private KeyValuePair<TKey, TValue> _current;       // the current entry set by MoveNext and returned from Current

            public Enumerator(ConditionalWeakTable<TKey, TValue> table)
            {
                Debug.Assert(table != null, "Must provide a valid table");
                Debug.Assert(Monitor.IsEntered(table._lock), "Must hold the _lock lock to construct the enumerator");
                Debug.Assert(table._container != null, "Should not be used on a finalized table");
                Debug.Assert(table._container.FirstFreeEntry > 0, "Should have returned an empty enumerator instead");

                // Store a reference to the parent table and increase its active enumerator count.
                _table = table;
                Debug.Assert(table._activeEnumeratorRefCount >= 0, "Should never have a negative ref count before incrementing");
                table._activeEnumeratorRefCount++;

                // Store the max index to be enumerated.
                _maxIndexInclusive = table._container.FirstFreeEntry - 1;
                _currentIndex = -1;
            }

            ~Enumerator() { Dispose(); }

            public void Dispose()
            {
                // Use an interlocked operation to ensure that only one thread can get access to
                // the _table for disposal and thus only decrement the ref count once.
                ConditionalWeakTable<TKey, TValue> table = Interlocked.Exchange(ref _table, null);
                if (table != null)
                {
                    // Ensure we don't keep the last current alive unnecessarily
                    _current = default(KeyValuePair<TKey, TValue>);

                    // Decrement the ref count that was incremented when constructed
                    lock (table._lock)
                    {
                        table._activeEnumeratorRefCount--;
                        Debug.Assert(table._activeEnumeratorRefCount >= 0, "Should never have a negative ref count after decrementing");
                    }

                    // Finalization is purely to decrement the ref count.  We can suppress it now.
                    GC.SuppressFinalize(this);
                }
            }

            public bool MoveNext()
            {
                // Start by getting the current table.  If it's already been disposed, it will be null.
                ConditionalWeakTable<TKey, TValue> table = _table;
                if (table != null)
                {
                    // Once have the table, we need to lock to synchronize with other operations on
                    // the table, like adding.
                    lock (table._lock)
                    {
                        // From the table, we have to get the current container.  This could have changed
                        // since we grabbed the enumerator, but the index-to-pair mapping should not have
                        // due to there being at least one active enumerator.  If the table (or rather its
                        // container at the time) has already been finalized, this will be null.
                        Container c = table._container;
                        if (c != null)
                        {
                            // We have the container.  Find the next entry to return, if there is one.
                            // We need to loop as we may try to get an entry that's already been removed
                            // or collected, in which case we try again.
                            while (_currentIndex < _maxIndexInclusive)
                            {
                                _currentIndex++;
                                TKey key;
                                TValue value;
                                if (c.TryGetEntry(_currentIndex, out key, out value))
                                {
                                    _current = new KeyValuePair<TKey, TValue>(key, value);
                                    return true;
                                }
                            }
                        }
                    }
                }

                // Nothing more to enumerate.
                return false;
            }

            public KeyValuePair<TKey, TValue> Current
            {
                get
                {
                    if (_currentIndex < 0)
                    {
                        ThrowHelper.ThrowInvalidOperationException_InvalidOperation_EnumOpCantHappen();
                    }
                    return _current;
                }
            }

            object IEnumerator.Current => Current;

            public void Reset() { }
        }

        #endregion

        #region Internal members

        //--------------------------------------------------------------------------------------------
        // Find a key that equals (value equality) with the given key - don't use in perf critical path
        // Note that it calls out to Object.Equals which may calls the override version of Equals
        // and that may take locks and leads to deadlock
        // Currently it is only used by WinRT event code and you should only use this function
        // if you know for sure that either you won't run into dead locks or you need to live with the
        // possiblity
        //--------------------------------------------------------------------------------------------
        [FriendAccessAllowed]
        internal TKey FindEquivalentKeyUnsafe(TKey key, out TValue value)
        {
            lock (_lock)
            {
                return _container.FindEquivalentKeyUnsafe(key, out value);
            }
        }

        //--------------------------------------------------------------------------------------------
        // Returns a collection of keys - don't use in perf critical path
        //--------------------------------------------------------------------------------------------
        internal ICollection<TKey> Keys
        {
            get
            {
                lock (_lock)
                {
                    return _container.Keys;
                }
            }
        }

        //--------------------------------------------------------------------------------------------
        // Returns a collection of values - don't use in perf critical path
        //--------------------------------------------------------------------------------------------
        internal ICollection<TValue> Values
        {
            get
            {
                lock (_lock)
                {
                    return _container.Values;
                }
            }
        }

        #endregion

        #region Private Members

        //----------------------------------------------------------------------------------------
        // Worker for adding a new key/value pair.
        // Will resize the container if it is full
        //
        // Preconditions:
        //     Must hold _lock.
        //     Key already validated as non-null and not already in table.
        //----------------------------------------------------------------------------------------
        private void CreateEntry(TKey key, TValue value)
        {
            Debug.Assert(Monitor.IsEntered(_lock));

            Container c = _container;
            if (!c.HasCapacity)
            {
                _container = c = c.Resize();
            }
            c.CreateEntryNoResize(key, value);
        }

        private static bool IsPowerOfTwo(int value) => (value > 0) && ((value & (value - 1)) == 0);

        #endregion

        #region Private Data Members
        //--------------------------------------------------------------------------------------------
        // Entry can be in one of four states:
        //
        //    - Unused (stored with an index _firstFreeEntry and above)
        //         depHnd.IsAllocated == false
        //         hashCode == <dontcare>
        //         next == <dontcare>)
        //
        //    - Used with live key (linked into a bucket list where _buckets[hashCode & (_buckets.Length - 1)] points to first entry)
        //         depHnd.IsAllocated == true, depHnd.GetPrimary() != null
        //         hashCode == RuntimeHelpers.GetHashCode(depHnd.GetPrimary()) & Int32.MaxValue
        //         next links to next Entry in bucket. 
        //                          
        //    - Used with dead key (linked into a bucket list where _buckets[hashCode & (_buckets.Length - 1)] points to first entry)
        //         depHnd.IsAllocated == true, depHnd.GetPrimary() == null
        //         hashCode == <notcare> 
        //         next links to next Entry in bucket. 
        //
        //    - Has been removed from the table (by a call to Remove)
        //         depHnd.IsAllocated == true, depHnd.GetPrimary() == <notcare>
        //         hashCode == -1 
        //         next links to next Entry in bucket. 
        //
        // The only difference between "used with live key" and "used with dead key" is that
        // depHnd.GetPrimary() returns null. The transition from "used with live key" to "used with dead key"
        // happens asynchronously as a result of normal garbage collection. The dictionary itself
        // receives no notification when this happens.
        //
        // When the dictionary grows the _entries table, it scours it for expired keys and does not
        // add those to the new container.
        //--------------------------------------------------------------------------------------------
        private struct Entry
        {
            public DependentHandle depHnd;      // Holds key and value using a weak reference for the key and a strong reference
                                                // for the value that is traversed only if the key is reachable without going through the value.
            public int HashCode;    // Cached copy of key's hashcode
            public int Next;        // Index of next entry, -1 if last
        }

        //
        // Container holds the actual data for the table.  A given instance of Container always has the same capacity.  When we need
        // more capacity, we create a new Container, copy the old one into the new one, and discard the old one.  This helps enable lock-free
        // reads from the table, as readers never need to deal with motion of entries due to rehashing.
        //
        private sealed class Container
        {
            private readonly ConditionalWeakTable<TKey, TValue> _parent;  // the ConditionalWeakTable with which this container is associated
            private int[] _buckets;                // _buckets[hashcode & (_buckets.Length - 1)] contains index of the first entry in bucket (-1 if empty)
            private Entry[] _entries;              // the table entries containing the stored dependency handles
            private int _firstFreeEntry;           // _firstFreeEntry < _entries.Length => table has capacity,  entries grow from the bottom of the table.
            private bool _invalid;                 // flag detects if OOM or other background exception threw us out of the lock.
            private bool _finalized;               // set to true when initially finalized
            private volatile object _oldKeepAlive; // used to ensure the next allocated container isn't finalized until this one is GC'd

            internal Container(ConditionalWeakTable<TKey, TValue> parent)
            {
                Debug.Assert(parent != null);
                Debug.Assert(IsPowerOfTwo(InitialCapacity));

                int size = InitialCapacity;
                _buckets = new int[size];
                for (int i = 0; i < _buckets.Length; i++)
                {
                    _buckets[i] = -1;
                }
                _entries = new Entry[size];

                // Only store the parent after all of the allocations have happened successfully.
                // Otherwise, as part of growing or clearing the container, we could end up allocating
                // a new Container that fails (OOMs) part way through construction but that gets finalized
                // and ends up clearing out some other container present in the associated CWT.
                _parent = parent;
            }

            private Container(ConditionalWeakTable<TKey, TValue> parent, int[] buckets, Entry[] entries, int firstFreeEntry)
            {
                Debug.Assert(parent != null);
                Debug.Assert(buckets != null);
                Debug.Assert(entries != null);
                Debug.Assert(buckets.Length == entries.Length);
                Debug.Assert(IsPowerOfTwo(buckets.Length));

                _parent = parent;
                _buckets = buckets;
                _entries = entries;
                _firstFreeEntry = firstFreeEntry;
            }

            internal bool HasCapacity => _firstFreeEntry < _entries.Length;

            internal int FirstFreeEntry => _firstFreeEntry;

            //----------------------------------------------------------------------------------------
            // Worker for adding a new key/value pair.
            // Preconditions:
            //     Container must NOT be full
            //----------------------------------------------------------------------------------------
            internal void CreateEntryNoResize(TKey key, TValue value)
            {
                Debug.Assert(HasCapacity);

                VerifyIntegrity();
                _invalid = true;

                int hashCode = RuntimeHelpers.GetHashCode(key) & int.MaxValue;
                int newEntry = _firstFreeEntry++;

                _entries[newEntry].HashCode = hashCode;
                _entries[newEntry].depHnd = new DependentHandle(key, value);
                int bucket = hashCode & (_buckets.Length - 1);
                _entries[newEntry].Next = _buckets[bucket];

                // This write must be volatile, as we may be racing with concurrent readers.  If they see
                // the new entry, they must also see all of the writes earlier in this method.
                Volatile.Write(ref _buckets[bucket], newEntry);

                _invalid = false;
            }

            //----------------------------------------------------------------------------------------
            // Worker for finding a key/value pair
            //
            // Preconditions:
            //     Must hold _lock.
            //     Key already validated as non-null
            //----------------------------------------------------------------------------------------
            internal bool TryGetValueWorker(TKey key, out TValue value)
            {
                object secondary;
                int entryIndex = FindEntry(key, out secondary);
                value = JitHelpers.UnsafeCast<TValue>(secondary);
                return entryIndex != -1;
            }

            //----------------------------------------------------------------------------------------
            // Returns -1 if not found (if key expires during FindEntry, this can be treated as "not found.")
            //
            // Preconditions:
            //     Must hold _lock, or be prepared to retry the search while holding _lock.
            //     Key already validated as non-null.
            //----------------------------------------------------------------------------------------
            internal int FindEntry(TKey key, out object value)
            {
                int hashCode = RuntimeHelpers.GetHashCode(key) & int.MaxValue;
                int bucket = hashCode & (_buckets.Length - 1);
                for (int entriesIndex = Volatile.Read(ref _buckets[bucket]); entriesIndex != -1; entriesIndex = _entries[entriesIndex].Next)
                {
                    if (_entries[entriesIndex].HashCode == hashCode)
                    {
                        object primary, secondary;
                        _entries[entriesIndex].depHnd.GetPrimaryAndSecondary(out primary, out secondary);
                        if (primary == key)
                        {
                            GC.KeepAlive(this); // ensure we don't get finalized while accessing DependentHandles.
                            value = secondary;
                            return entriesIndex;
                        }
                    }
                }

                GC.KeepAlive(this); // ensure we don't get finalized while accessing DependentHandles.
                value = null;
                return -1;
            }

            //----------------------------------------------------------------------------------------
            // Gets the entry at the specified entry index.
            //----------------------------------------------------------------------------------------
            internal bool TryGetEntry(int index, out TKey key, out TValue value)
            {
                if (index < _entries.Length)
                {
                    object oKey, oValue;
                    _entries[index].depHnd.GetPrimaryAndSecondary(out oKey, out oValue);
                    GC.KeepAlive(this); // ensure we don't get finalized while accessing DependentHandles.

                    if (oKey != null)
                    {
                        key = JitHelpers.UnsafeCast<TKey>(oKey);
                        value = JitHelpers.UnsafeCast<TValue>(oValue);
                        return true;
                    }
                }

                key = default(TKey);
                value = default(TValue);
                return false;
            }

            //----------------------------------------------------------------------------------------
            // Removes all of the keys in the table.
            //----------------------------------------------------------------------------------------
            internal void RemoveAllKeys()
            {
                for (int i = 0; i < _firstFreeEntry; i++)
                {
                    RemoveIndex(i);
                }
            }

            //----------------------------------------------------------------------------------------
            // Removes the specified key from the table, if it exists.
            //----------------------------------------------------------------------------------------
            internal bool Remove(TKey key)
            {
                VerifyIntegrity();

                object value;
                int entryIndex = FindEntry(key, out value);
                if (entryIndex != -1)
                {
                    RemoveIndex(entryIndex);
                    return true;
                }

                return false;
            }

            private void RemoveIndex(int entryIndex)
            {
                Debug.Assert(entryIndex >= 0 && entryIndex < _firstFreeEntry);

                ref Entry entry = ref _entries[entryIndex];

                // We do not free the handle here, as we may be racing with readers who already saw the hash code.
                // Instead, we simply overwrite the entry's hash code, so subsequent reads will ignore it.
                // The handle will be free'd in Container's finalizer, after the table is resized or discarded.
                Volatile.Write(ref entry.HashCode, -1);

                // Also, clear the key to allow GC to collect objects pointed to by the entry
                entry.depHnd.SetPrimary(null);
            }

            internal void UpdateValue(int entryIndex, TValue newValue)
            {
                Debug.Assert(entryIndex != -1);

                VerifyIntegrity();
                _invalid = true;

                _entries[entryIndex].depHnd.SetSecondary(newValue);

                _invalid = false;
            }

            //----------------------------------------------------------------------------------------
            // This does two things: resize and scrub expired keys off bucket lists.
            //
            // Precondition:
            //      Must hold _lock.
            //
            // Postcondition:
            //      _firstEntry is less than _entries.Length on exit, that is, the table has at least one free entry.
            //----------------------------------------------------------------------------------------
            internal Container Resize()
            {
                Debug.Assert(!HasCapacity);

                bool hasExpiredEntries = false;
                int newSize = _buckets.Length;

                if (_parent == null || _parent._activeEnumeratorRefCount == 0)
                {
                    // If any expired or removed keys exist, we won't resize.
                    // If there any active enumerators, though, we don't want
                    // to compact and thus have no expired entries.
                    for (int entriesIndex = 0; entriesIndex < _entries.Length; entriesIndex++)
                    {
                        ref Entry entry = ref _entries[entriesIndex];

                        if (entry.HashCode == -1)
                        {
                            // the entry was removed
                            hasExpiredEntries = true;
                            break;
                        }

                        if (entry.depHnd.IsAllocated && entry.depHnd.GetPrimary() == null)
                        {
                            // the entry has expired
                            hasExpiredEntries = true;
                            break;
                        }
                    }
                }

                if (!hasExpiredEntries)
                {
                    // Not necessary to check for overflow here, the attempt to allocate new arrays will throw
                    newSize = _buckets.Length * 2;
                }

                return Resize(newSize);
            }            

            internal Container Resize(int newSize)
            {
                Debug.Assert(newSize >= _buckets.Length);
                Debug.Assert(IsPowerOfTwo(newSize));

                // Reallocate both buckets and entries and rebuild the bucket and entries from scratch.
                // This serves both to scrub entries with expired keys and to put the new entries in the proper bucket.
                int[] newBuckets = new int[newSize];
                for (int bucketIndex = 0; bucketIndex < newBuckets.Length; bucketIndex++)
                {
                    newBuckets[bucketIndex] = -1;
                }
                Entry[] newEntries = new Entry[newSize];
                int newEntriesIndex = 0;
                bool activeEnumerators = _parent != null && _parent._activeEnumeratorRefCount > 0;

                // Migrate existing entries to the new table.
                if (activeEnumerators)
                {
                    // There's at least one active enumerator, which means we don't want to
                    // remove any expired/removed entries, in order to not affect existing
                    // entries indices.  Copy over the entries while rebuilding the buckets list,
                    // as the buckets are dependent on the buckets list length, which is changing.
                    for (; newEntriesIndex < _entries.Length; newEntriesIndex++)
                    {
                        ref Entry oldEntry = ref _entries[newEntriesIndex];
                        ref Entry newEntry = ref newEntries[newEntriesIndex];
                        int hashCode = oldEntry.HashCode;

                        newEntry.HashCode = hashCode;
                        newEntry.depHnd = oldEntry.depHnd;
                        int bucket = hashCode & (newBuckets.Length - 1);
                        newEntry.Next = newBuckets[bucket];
                        newBuckets[bucket] = newEntriesIndex;
                    }
                }
                else
                {
                    // There are no active enumerators, which means we want to compact by
                    // removing expired/removed entries.
                    for (int entriesIndex = 0; entriesIndex < _entries.Length; entriesIndex++)
                    {
                        ref Entry oldEntry = ref _entries[entriesIndex];
                        int hashCode = oldEntry.HashCode;
                        DependentHandle depHnd = oldEntry.depHnd;
                        if (hashCode != -1 && depHnd.IsAllocated)
                        {
                            if (depHnd.GetPrimary() != null)
                            {
                                ref Entry newEntry = ref newEntries[newEntriesIndex];

                                // Entry is used and has not expired. Link it into the appropriate bucket list.
                                newEntry.HashCode = hashCode;
                                newEntry.depHnd = depHnd;
                                int bucket = hashCode & (newBuckets.Length - 1);
                                newEntry.Next = newBuckets[bucket];
                                newBuckets[bucket] = newEntriesIndex;
                                newEntriesIndex++;
                            }
                            else
                            {
                                // Pretend the item was removed, so that this container's finalizer
                                // will clean up this dependent handle.
                                Volatile.Write(ref oldEntry.HashCode, -1);
                            }
                        }
                    }
                }

                // Create the new container.  We want to transfer the responsibility of freeing the handles from
                // the old container to the new container, and also ensure that the new container isn't finalized
                // while the old container may still be in use.  As such, we store a reference from the old container
                // to the new one, which will keep the new container alive as long as the old one is.
                var newContainer = new Container(_parent, newBuckets, newEntries, newEntriesIndex);
                if (activeEnumerators)
                {
                    // If there are active enumerators, both the old container and the new container may be storing
                    // the same entries with -1 hash codes, which the finalizer will clean up even if the container
                    // is not the active container for the table.  To prevent that, we want to stop the old container
                    // from being finalized, as it no longer has any responsibility for any cleanup.
                    GC.SuppressFinalize(this);
                }
                _oldKeepAlive = newContainer; // once this is set, the old container's finalizer will not free transferred dependent handles

                GC.KeepAlive(this); // ensure we don't get finalized while accessing DependentHandles.

                return newContainer;
            }

            internal ICollection<TKey> Keys
            {
                get
                {
                    var list = new List<TKey>();

                    for (int bucket = 0; bucket < _buckets.Length; ++bucket)
                    {
                        for (int entriesIndex = _buckets[bucket]; entriesIndex != -1; entriesIndex = _entries[entriesIndex].Next)
                        {
                            TKey thisKey = JitHelpers.UnsafeCast<TKey>(_entries[entriesIndex].depHnd.GetPrimary());
                            if (thisKey != null)
                            {
                                list.Add(thisKey);
                            }
                        }
                    }

                    GC.KeepAlive(this); // ensure we don't get finalized while accessing DependentHandles.
                    return list;
                }
            }

            internal ICollection<TValue> Values
            {
                get
                {
                    var list = new List<TValue>();

                    for (int bucket = 0; bucket < _buckets.Length; ++bucket)
                    {
                        for (int entriesIndex = _buckets[bucket]; entriesIndex != -1; entriesIndex = _entries[entriesIndex].Next)
                        {
                            object primary = null, secondary = null;
                            _entries[entriesIndex].depHnd.GetPrimaryAndSecondary(out primary, out secondary);

                            // Now that we've secured a strong reference to the secondary, must check the primary again
                            // to ensure it didn't expire (otherwise, we open a race where TryGetValue misreports an
                            // expired key as a live key with a null value.)
                            if (primary != null)
                            {
                                list.Add(JitHelpers.UnsafeCast<TValue>(secondary));
                            }
                        }
                    }

                    GC.KeepAlive(this); // ensure we don't get finalized while accessing DependentHandles.
                    return list;
                }
            }

            internal TKey FindEquivalentKeyUnsafe(TKey key, out TValue value)
            {
                for (int bucket = 0; bucket < _buckets.Length; ++bucket)
                {
                    for (int entriesIndex = _buckets[bucket]; entriesIndex != -1; entriesIndex = _entries[entriesIndex].Next)
                    {
                        if (_entries[entriesIndex].HashCode == -1)
                        {
                            continue;   // removed entry whose handle is awaiting condemnation by the finalizer.
                        }

                        object thisKey, thisValue;
                        _entries[entriesIndex].depHnd.GetPrimaryAndSecondary(out thisKey, out thisValue);
                        if (Equals(thisKey, key))
                        {
                            GC.KeepAlive(this); // ensure we don't get finalized while accessing DependentHandles.
                            value = JitHelpers.UnsafeCast<TValue>(thisValue);
                            return JitHelpers.UnsafeCast<TKey>(thisKey);
                        }
                    }
                }

                GC.KeepAlive(this); // ensure we don't get finalized while accessing DependentHandles.
                value = default(TValue);
                return null;
            }

            //----------------------------------------------------------------------------------------
            // Precondition:
            //     Must hold _lock.
            //----------------------------------------------------------------------------------------
            private void VerifyIntegrity()
            {
                if (_invalid)
                {
                    throw new InvalidOperationException(Environment.GetResourceString("CollectionCorrupted"));
                }
            }

            //----------------------------------------------------------------------------------------
            // Finalizer.
            //----------------------------------------------------------------------------------------
            ~Container()
            {
                // We're just freeing per-appdomain unmanaged handles here. If we're already shutting down the AD,
                // don't bother. (Despite its name, Environment.HasShutdownStart also returns true if the current
                // AD is finalizing.)  We also skip doing anything if the container is invalid, including if someone
                // the container object was allocated but its associated table never set.
                if (Environment.HasShutdownStarted || _invalid || _parent == null)
                {
                    return;
                }

                // It's possible that the ConditionalWeakTable could have been resurrected, in which case code could
                // be accessing this Container as it's being finalized.  We don't support usage after finalization,
                // but we also don't want to potentially corrupt state by allowing dependency handles to be used as
                // or after they've been freed.  To avoid that, if it's at all possible that another thread has a
                // reference to this container via the CWT, we remove such a reference and then re-register for
                // finalization: the next time around, we can be sure that no references remain to this and we can
                // clean up the dependency handles without fear of corruption.
                if (!_finalized)
                {
                    _finalized = true;
                    lock (_parent._lock)
                    {
                        if (_parent._container == this)
                        {
                            _parent._container = null;
                        }
                    }
                    GC.ReRegisterForFinalize(this); // next time it's finalized, we'll be sure there are no remaining refs
                    return;
                }

                Entry[] entries = _entries;
                _invalid = true;
                _entries = null;
                _buckets = null;

                if (entries != null)
                {
                    for (int entriesIndex = 0; entriesIndex < entries.Length; entriesIndex++)
                    {
                        // We need to free handles in two cases:
                        // - If this container still owns the dependency handle (meaning ownership hasn't been transferred
                        //   to another container that replaced this one), then it should be freed.
                        // - If this container had the entry removed, then even if in general ownership was transferred to
                        //   another container, removed entries are not, therefore this container must free them.
                        if (_oldKeepAlive == null || entries[entriesIndex].HashCode == -1)
                        {
                            entries[entriesIndex].depHnd.Free();
                        }
                    }
                }
            }
        }
        #endregion
    }
    #endregion

    #region DependentHandle
    //=========================================================================================
    // This struct collects all operations on native DependentHandles. The DependentHandle
    // merely wraps an IntPtr so this struct serves mainly as a "managed typedef."
    //
    // DependentHandles exist in one of two states:
    //
    //    IsAllocated == false
    //        No actual handle is allocated underneath. Illegal to call GetPrimary
    //        or GetPrimaryAndSecondary(). Ok to call Free().
    //
    //        Initializing a DependentHandle using the nullary ctor creates a DependentHandle
    //        that's in the !IsAllocated state.
    //        (! Right now, we get this guarantee for free because (IntPtr)0 == NULL unmanaged handle.
    //         ! If that assertion ever becomes false, we'll have to add an _isAllocated field
    //         ! to compensate.)
    //        
    //
    //    IsAllocated == true
    //        There's a handle allocated underneath. You must call Free() on this eventually
    //        or you cause a native handle table leak.
    //
    // This struct intentionally does no self-synchronization. It's up to the caller to
    // to use DependentHandles in a thread-safe way.
    //=========================================================================================
    internal struct DependentHandle
    {
        #region Constructors
        public DependentHandle(object primary, object secondary)
        {
            IntPtr handle = (IntPtr)0;
            nInitialize(primary, secondary, out handle);
            // no need to check for null result: nInitialize expected to throw OOM.
            _handle = handle;
        }
        #endregion

        #region Public Members
        public bool IsAllocated => _handle != IntPtr.Zero;

        // Getting the secondary object is more expensive than getting the first so
        // we provide a separate primary-only accessor for those times we only want the
        // primary.
        public object GetPrimary()
        {
            object primary;
            nGetPrimary(_handle, out primary);
            return primary;
        }

        public void GetPrimaryAndSecondary(out object primary, out object secondary)
        {
            nGetPrimaryAndSecondary(_handle, out primary, out secondary);
        }

        public void SetPrimary(object primary)
        {
            nSetPrimary(_handle, primary);
        }

        public void SetSecondary(object secondary)
        {
            nSetSecondary(_handle, secondary);
        }

        //----------------------------------------------------------------------
        // Forces dependentHandle back to non-allocated state (if not already there)
        // and frees the handle if needed.
        //----------------------------------------------------------------------
        public void Free()
        {
            if (_handle != (IntPtr)0)
            {
                IntPtr handle = _handle;
                _handle = (IntPtr)0;
                 nFree(handle);
            }
        }
        #endregion

        #region Private Members
        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern void nInitialize(object primary, object secondary, out IntPtr dependentHandle);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern void nGetPrimary(IntPtr dependentHandle, out object primary);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern void nGetPrimaryAndSecondary(IntPtr dependentHandle, out object primary, out object secondary);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern void nSetPrimary(IntPtr dependentHandle, object primary);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern void nSetSecondary(IntPtr dependentHandle, object secondary);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern void nFree(IntPtr dependentHandle);
        #endregion

        #region Private Data Member
        private IntPtr _handle;
        #endregion

    } // struct DependentHandle
    #endregion
}
