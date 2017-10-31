// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

/*============================================================
**
** 
** 
**
** Purpose: Generic hash table implementation
**
** #DictionaryVersusHashtableThreadSafety
** Hashtable has multiple reader/single writer (MR/SW) thread safety built into 
** certain methods and properties, whereas Dictionary doesn't. If you're 
** converting framework code that formerly used Hashtable to Dictionary, it's
** important to consider whether callers may have taken a dependence on MR/SW
** thread safety. If a reader writer lock is available, then that may be used
** with a Dictionary to get the same thread safety guarantee. 
** 
===========================================================*/

namespace System.Collections.Generic
{
    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;

    /// <summary>
    /// Used internally to control behavior of insertion into a <see cref="Dictionary{TKey, TValue}"/>.
    /// </summary>
    internal interface IInsertionBehavior { }

    /// <summary>
    /// The default insertion behavior.
    /// </summary>
    internal struct RejectIfExisting : IInsertionBehavior { }

    /// <summary>
    /// Specifies that an existing entry with the same key should be overwritten if encountered.
    /// </summary>
    internal struct OverwriteExisting : IInsertionBehavior { }

    /// <summary>
    /// Specifies that if an existing entry with the same key is encountered, an exception should be thrown.
    /// </summary>
    internal struct ThrowOnExisting : IInsertionBehavior { }

    internal interface IComparerType { }
    internal struct CustomComparer : IComparerType { }
    internal struct DefaultComparer : IComparerType { }

    internal interface IResizeBehavior { }
    internal struct GenerateNewHashcodes : IResizeBehavior { }
    internal struct KeepHashcodes : IResizeBehavior { }

    [DebuggerTypeProxy(typeof(IDictionaryDebugView<,>))]
    [DebuggerDisplay("Count = {Count}")]
    [Serializable]
    [System.Runtime.CompilerServices.TypeForwardedFrom("mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")] 
    public class Dictionary<TKey, TValue> : IDictionary<TKey, TValue>, IDictionary, IReadOnlyDictionary<TKey, TValue>, ISerializable, IDeserializationCallback
    {
        private struct Entry
        {
            public int hashCode;    // Lower 31 bits of hash code, -1 if unused
            public int next;        // Index of next entry, -1 if last
            public TKey key;           // Key of entry
            public TValue value;         // Value of entry
        }

        private static Entry s_nullEntry;

        private int[] _buckets;
        private Entry[] _entries;
        private int _count;
        private int _version;
        private int _freeList;
        private int _freeCount;

        private IEqualityComparer<TKey> _customComparer;

        private KeyCollection _keys;
        private ValueCollection _values;
        private Object _syncRoot;

        // constants for serialization
        private const String VersionName = "Version"; // Do not rename (binary serialization)
        private const String HashSizeName = "HashSize"; // Do not rename (binary serialization). Must save buckets.Length
        private const String KeyValuePairsName = "KeyValuePairs"; // Do not rename (binary serialization)
        private const String ComparerName = "Comparer"; // Do not rename (binary serialization)

        public Dictionary() : this(0, null) { }

        public Dictionary(int capacity) : this(capacity, null) { }

        public Dictionary(IEqualityComparer<TKey> comparer) : this(0, comparer) { }

        public Dictionary(int capacity, IEqualityComparer<TKey> comparer)
        {
            if (capacity < 0) ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.capacity);
            if (capacity > 0) Initialize(capacity);
            _customComparer = comparer;

            // String has a more nuanced comparer as its GetHashCode is randomised for security
            if (typeof(TKey) == typeof(string))
            {
                // If TKey is a string, we move off the default comparer to a non-randomized comparer
                // Later if collisions become too high we will move back onto the default randomized comparer
                if (comparer == null || ReferenceEquals(comparer, EqualityComparer<string>.Default))
                {
                    _customComparer = (IEqualityComparer<TKey>)NonRandomizedStringEqualityComparer.Default;
                }
            }
        }

        public Dictionary(IDictionary<TKey, TValue> dictionary) : this(dictionary, null) { }

        public Dictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer) :
            this(dictionary?.Count ?? 0, comparer)
        {
            if (dictionary == null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.dictionary);
            }

            // It is likely that the passed-in dictionary is Dictionary<TKey,TValue>. When this is the case,
            // avoid the enumerator allocation and overhead by looping through the entries array directly.
            // We only do this when dictionary is Dictionary<TKey,TValue> and not a subclass, to maintain
            // back-compat with subclasses that may have overridden the enumerator behavior.
            if (dictionary.GetType() == typeof(Dictionary<TKey, TValue>))
            {
                Dictionary<TKey, TValue> d = (Dictionary<TKey, TValue>)dictionary;
                int count = d._count;
                Entry[] entries = d._entries;
                for (int i = 0; i < count; i++)
                {
                    ref Entry entry = ref entries[i];
                    if (entry.hashCode >= 0)
                    {
                        Add(entry.key, entry.value);
                    }
                }
                return;
            }

            foreach (KeyValuePair<TKey, TValue> pair in dictionary)
            {
                Add(pair.Key, pair.Value);
            }
        }

        public Dictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection) :
            this(collection, null)
        { }

        public Dictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection, IEqualityComparer<TKey> comparer) :
            this((collection as ICollection<KeyValuePair<TKey, TValue>>)?.Count ?? 0, comparer)
        {
            if (collection == null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.collection);
            }

            foreach (KeyValuePair<TKey, TValue> pair in collection)
            {
                Add(pair.Key, pair.Value);
            }
        }

        protected Dictionary(SerializationInfo info, StreamingContext context)
        {
            //We can't do anything with the keys and values until the entire graph has been deserialized
            //and we have a resonable estimate that GetHashCode is not going to fail.  For the time being,
            //we'll just cache this.  The graph is not valid until OnDeserialization has been called.
            HashHelpers.SerializationInfoTable.Add(this, info);
        }

        public IEqualityComparer<TKey> Comparer => _customComparer ?? EqualityComparer<TKey>.Default;

        public int Count => _count - _freeCount;

        public KeyCollection Keys
        {
            get
            {
                return _keys ?? (_keys = new KeyCollection(this));
            }
        }

        ICollection<TKey> IDictionary<TKey, TValue>.Keys => _keys ?? (_keys = new KeyCollection(this));

        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => _keys ?? (_keys = new KeyCollection(this));

        public ValueCollection Values
        {
            get
            {
                return _values ?? (_values = new ValueCollection(this));
            }
        }

        ICollection<TValue> IDictionary<TKey, TValue>.Values => _values ?? (_values = new ValueCollection(this));

        IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => _values ?? (_values = new ValueCollection(this));

        public TValue this[TKey key]
        {
            get
            {
                ref Entry entry = ref FindEntry(key, out bool found);
                if (found)
                {
                    return entry.value;
                }
                ThrowHelper.ThrowKeyNotFoundException();
                return default(TValue);
            }
            set
            {
                bool modified = TryInsert<OverwriteExisting>(key, value);
                Debug.Assert(modified);
            }
        }

        public void Add(TKey key, TValue value)
        {
            bool modified = TryInsert<ThrowOnExisting>(key, value);
            Debug.Assert(modified); // If there was an existing key and the Add failed, an exception will already have been thrown.
        }

        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> keyValuePair)
        {
            Add(keyValuePair.Key, keyValuePair.Value);
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> keyValuePair)
        {
            ref Entry entry = ref FindEntry(keyValuePair.Key, out bool found);
            return found && EqualityComparer<TValue>.Default.Equals(entry.value, keyValuePair.Value);
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> keyValuePair)
        {
            ref Entry entry = ref FindEntry(keyValuePair.Key, out bool found);
            if (found && EqualityComparer<TValue>.Default.Equals(entry.value, keyValuePair.Value))
            {
                Remove(keyValuePair.Key);
                return true;
            }
            return false;
        }

        public void Clear()
        {
            int count = _count;
            if (count > 0)
            {
                int[] buckets = _buckets;
                for (int i = 0; i < buckets.Length; i++)
                {
                    buckets[i] = -1;
                }
                Array.Clear(_entries, 0, count);
                _freeList = -1;
                _count = 0;
                _freeCount = 0;
                _version++;
            }
        }

        public bool ContainsKey(TKey key)
        {
            FindEntry(key, out bool found);
            return found;
        }

        public bool ContainsValue(TValue value)
        {
            Entry[] entries = _entries;
            int count = _count;
            if (value == null)
            {
                for (int i = 0; i < count; i++)
                {
                    ref Entry entry = ref entries[i];
                    if (entry.hashCode >= 0 && entry.value == null) return true;
                }
            }
            else
            {
                EqualityComparer<TValue> c = EqualityComparer<TValue>.Default;
                for (int i = 0; i < count; i++)
                {
                    ref Entry entry = ref entries[i];
                    if (entry.hashCode >= 0 && c.Equals(entry.value, value)) return true;
                }
            }
            return false;
        }

        private void CopyTo(KeyValuePair<TKey, TValue>[] array, int index)
        {
            if (array == null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.array);
            }

            if (index < 0 || index > array.Length)
            {
                ThrowHelper.ThrowIndexArgumentOutOfRange_NeedNonNegNumException();
            }

            if (array.Length - index < Count)
            {
                ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_ArrayPlusOffTooSmall);
            }

            int count = this._count;
            Entry[] entries = this._entries;
            for (int i = 0; i < count; i++)
            {
                ref Entry entry = ref entries[i];
                if (entry.hashCode >= 0)
                {
                    array[index++] = new KeyValuePair<TKey, TValue>(entry.key, entry.value);
                }
            }
        }

        public Enumerator GetEnumerator() => new Enumerator(this, Enumerator.KeyValuePair);

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator() => new Enumerator(this, Enumerator.KeyValuePair);

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.info);
            }
            info.AddValue(VersionName, _version);
            info.AddValue(ComparerName, _customComparer ?? EqualityComparer<TKey>.Default, typeof(IEqualityComparer<TKey>));
            info.AddValue(HashSizeName, _buckets == null ? 0 : _buckets.Length); //This is the length of the bucket array.
            if (_buckets != null)
            {
                KeyValuePair<TKey, TValue>[] array = new KeyValuePair<TKey, TValue>[Count];
                CopyTo(array, 0);
                info.AddValue(KeyValuePairsName, array, typeof(KeyValuePair<TKey, TValue>[]));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ref Entry FindEntry(TKey key, out bool found)
        {
            if (key == null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.key);
            }

            IEqualityComparer<TKey> comparer = _customComparer;
            if (comparer == null)
            {
                return ref FindEntry<DefaultComparer>(key, out found, null);
            }
            else
            {
                return ref FindEntry<CustomComparer>(key, out found, comparer);
            }
        }

        private ref Entry FindEntry<TComparer>(TKey key, out bool found, IEqualityComparer<TKey> customComparer) where TComparer : struct, IComparerType
        {
            Debug.Assert(typeof(TComparer) == typeof(DefaultComparer) || typeof(TComparer) == typeof(CustomComparer));
            Debug.Assert(key != null);

            found = true;
            int[] buckets = _buckets;
            if (buckets != null)
            {
                int hashCode = 0;
                if (typeof(TComparer) == typeof(DefaultComparer))
                {
                    // Keys are never null
                    hashCode = key.GetHashCode() & 0x7FFFFFFF;
                }
                else if (typeof(TComparer) == typeof(CustomComparer))
                {
                    hashCode = customComparer.GetHashCode(key) & 0x7FFFFFFF;
                }

                Entry[] entries = _entries;
                int i = buckets[HashHelpers.FindBucket((uint)hashCode, (uint)buckets.Length)];
                while (i >= 0)
                {
                    ref Entry entry = ref entries[i];
                    if (entry.hashCode == hashCode)
                    {
                        if (typeof(TComparer) == typeof(DefaultComparer))
                        {
                            if (EqualityComparer<TKey>.Default.Equals(entry.key, key))
                            {
                                return ref entry;
                            }
                        }
                        else if (typeof(TComparer) == typeof(CustomComparer))
                        {
                            if (customComparer.Equals(entry.key, key))
                            {
                                return ref entry;
                            }
                        }
                    }

                    i = entry.next;
                }
            }

            found = false;
            return ref NotFound;
        }

        private ref Entry NotFound
        {
            [MethodImpl(MethodImplOptions.NoInlining)]
            get => ref s_nullEntry;
        }

        private void Initialize(int capacity)
        {
            int size = HashHelpers.GetPrime(capacity);
            int[] buckets = new int[size];
            for (int i = 0; i < buckets.Length; i++)
            {
                buckets[i] = -1;
            }
            _entries = new Entry[size];
            _freeList = -1;

            _buckets = buckets;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool TryInsert<TInsertionBehavior>(TKey key, TValue value) where TInsertionBehavior : struct, IInsertionBehavior
        {
            Debug.Assert(typeof(TInsertionBehavior) == typeof(RejectIfExisting) || typeof(TInsertionBehavior) == typeof(OverwriteExisting) || typeof(TInsertionBehavior) == typeof(ThrowOnExisting));

            if (key == null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.key);
            }

            IEqualityComparer<TKey> customComparer = _customComparer;
            if (customComparer == null)
            {
                return TryInsert<TInsertionBehavior, DefaultComparer>(key, value, null);
            }
            else
            {
                return TryInsert<TInsertionBehavior, CustomComparer>(key, value, customComparer);
            }
        }

        private bool TryInsert<TInsertionBehavior, TComparer>(TKey key, TValue value, IEqualityComparer<TKey> customComparer) where TInsertionBehavior : struct, IInsertionBehavior where TComparer : struct, IComparerType
        {
            Debug.Assert(typeof(TComparer) == typeof(DefaultComparer) || typeof(TComparer) == typeof(CustomComparer));
            Debug.Assert(typeof(TInsertionBehavior) == typeof(RejectIfExisting) || typeof(TInsertionBehavior) == typeof(OverwriteExisting) || typeof(TInsertionBehavior) == typeof(ThrowOnExisting));
            Debug.Assert(key != null);

            if (_buckets == null)
            {
                Initialize(0);
            }
            int[] buckets = _buckets;
            // Keys are never null
            int hashCode = 0;
            if (typeof(TComparer) == typeof(DefaultComparer))
            {
                // Keys are never null
                hashCode = key.GetHashCode() & 0x7FFFFFFF;
            }
            else if (typeof(TComparer) == typeof(CustomComparer))
            {
                hashCode = customComparer.GetHashCode(key) & 0x7FFFFFFF;
            }

            uint targetBucket = HashHelpers.FindBucket((uint)hashCode, (uint)buckets.Length);

            // Count collisions to see if we need to move to randomized hashing for string keys
            int collisionCount = 0;
            Entry[] entries = _entries;

            int i = buckets[targetBucket];
            while (i >= 0)
            {
                ref Entry candidateEntry = ref entries[i];
                if (candidateEntry.hashCode == hashCode)
                {
                    bool keysEqual = false;
                    if (typeof(TComparer) == typeof(DefaultComparer))
                    {
                        keysEqual = EqualityComparer<TKey>.Default.Equals(candidateEntry.key, key);
                    }
                    else if (typeof(TComparer) == typeof(CustomComparer))
                    {
                        keysEqual = customComparer.Equals(candidateEntry.key, key);
                    }

                    if (keysEqual)
                    {
                        if (typeof(TInsertionBehavior) == typeof(OverwriteExisting))
                        {
                            candidateEntry.value = value;
                            _version++;
                            return true;
                        }
                        else if (typeof(TInsertionBehavior) == typeof(RejectIfExisting))
                        {
                            return false;
                        }
                        else if (typeof(TInsertionBehavior) == typeof(ThrowOnExisting))
                        {
                            ThrowHelper.ThrowAddingDuplicateWithKeyArgumentException(key);
                        }
                    }
                }

                i = candidateEntry.next;
                if (typeof(TComparer) == typeof(CustomComparer))
                {
                    collisionCount++;
                }
            }

            int index;
            if (_freeCount == 0)
            {
                int count = _count;
                if (count == entries.Length)
                {
                    Resize<KeepHashcodes>(HashHelpers.ExpandPrime(count));
                    // Update local cached items
                    buckets = _buckets;
                    entries = _entries;
                    targetBucket = HashHelpers.FindBucket((uint)hashCode, (uint)buckets.Length);
                }
                index = count;
                _count = count + 1;
            }
            else
            {
                index = _freeList;
                _freeList = entries[index].next;
                _freeCount--;
            }

            ref Entry entry = ref entries[index];
            entry.hashCode = hashCode;
            entry.next = buckets[targetBucket];
            buckets[targetBucket] = index;
            entry.key = key;
            entry.value = value;
            _version++;

            if (typeof(TComparer) == typeof(CustomComparer))
            {
                // If we hit the collision threshold we'll need to switch to the comparer which is using randomized string hashing
                // i.e. EqualityComparer<string>.Default.
                if (collisionCount > HashHelpers.HashCollisionThreshold && ReferenceEquals(_customComparer, NonRandomizedStringEqualityComparer.Default))
                {
                    _customComparer = null; // Use default comparer
                    Resize<GenerateNewHashcodes>(_entries.Length);
                }
            }

            return true;
        }

        public virtual void OnDeserialization(Object sender)
        {
            SerializationInfo siInfo;
            HashHelpers.SerializationInfoTable.TryGetValue(this, out siInfo);

            if (siInfo == null)
            {
                // It might be necessary to call OnDeserialization from a container if the container object also implements
                // OnDeserialization. However, remoting will call OnDeserialization again.
                // We can return immediately if this function is called twice. 
                // Note we set remove the serialization info from the table at the end of this method.
                return;
            }

            int realVersion = siInfo.GetInt32(VersionName);
            int hashsize = siInfo.GetInt32(HashSizeName);
            _customComparer = (IEqualityComparer<TKey>)siInfo.GetValue(ComparerName, typeof(IEqualityComparer<TKey>));

            if (hashsize != 0)
            {
                int[] buckets = new int[hashsize];
                for (int i = 0; i < buckets.Length; i++)
                {
                    buckets[i] = -1;
                }
                _buckets = buckets;
                _entries = new Entry[hashsize];
                _freeList = -1;

                KeyValuePair<TKey, TValue>[] array = (KeyValuePair<TKey, TValue>[])
                    siInfo.GetValue(KeyValuePairsName, typeof(KeyValuePair<TKey, TValue>[]));

                if (array == null)
                {
                    ThrowHelper.ThrowSerializationException(ExceptionResource.Serialization_MissingKeys);
                }

                for (int i = 0; i < array.Length; i++)
                {
                    if (array[i].Key == null)
                    {
                        ThrowHelper.ThrowSerializationException(ExceptionResource.Serialization_NullKey);
                    }
                    Add(array[i].Key, array[i].Value);
                }
            }
            else
            {
                _buckets = null;
            }

            _version = realVersion;
            HashHelpers.SerializationInfoTable.Remove(this);
        }

        private void Resize<TResizeBehavior>(int newSize) where TResizeBehavior : struct, IResizeBehavior
        {
            Debug.Assert(typeof(TResizeBehavior) == typeof(KeepHashcodes) || typeof(TResizeBehavior) == typeof(GenerateNewHashcodes));
            // Should only be rehashing when switching from custom NonRandomised string to default randomised 
            Debug.Assert(typeof(TResizeBehavior) == typeof(KeepHashcodes) || _customComparer == null);
            Debug.Assert(newSize >= _entries.Length);

            int[] newBuckets = new int[newSize];
            for (int i = 0; i < newBuckets.Length; i++)
            {
                newBuckets[i] = -1;
            }

            int count = _count;
            Entry[] newEntries = new Entry[newSize];
            Array.Copy(_entries, 0, newEntries, 0, count);

            // If the Jit eliminates bounds checks for a loop not limited by Length 
            // if the variable has been pre-confirmed
            // add a check that (uint)count < (uint)newEntries.Length

            for (int i = 0; i < count; i++)
            {
                if (typeof(TResizeBehavior) == typeof(GenerateNewHashcodes))
                {
                    ref Entry entry = ref newEntries[i];
                    int hashCode = entry.hashCode;
                    if (hashCode >= 0)
                    {
                        uint targetBucket = HashHelpers.FindBucket((uint)hashCode, (uint)newBuckets.Length);
                        // Keys are never null
                        hashCode = entry.key.GetHashCode() & 0x7FFFFFFF;
                        entry.hashCode = hashCode;
                        entry.next = newBuckets[targetBucket];
                        newBuckets[targetBucket] = i;
                    }
                }
                else
                {
                    int hashCode = newEntries[i].hashCode;
                    if (hashCode >= 0)
                    {
                        uint targetBucket = HashHelpers.FindBucket((uint)hashCode, (uint)newBuckets.Length);
                        newEntries[i].next = newBuckets[targetBucket];
                        newBuckets[targetBucket] = i;
                    }
                }
            }

            _buckets = newBuckets;
            _entries = newEntries;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(TKey key)
        {
            if (key == null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.key);
            }

            IEqualityComparer<TKey> customComparer = _customComparer;
            bool success;
            // Compiler doesn't support ref ternary yet https://github.com/dotnet/roslyn/issues/17797
            if (customComparer == null)
            {
                ref Entry entry = ref Remove<DefaultComparer>(key, out success, null);
                if (success && RuntimeHelpers.IsReferenceOrContainsReferences<TValue>())
                {
                    entry.value = default(TValue);
                }
            }
            else
            {
                ref Entry entry = ref Remove<CustomComparer>(key, out success, customComparer);
                if (success && RuntimeHelpers.IsReferenceOrContainsReferences<TValue>())
                {
                    entry.value = default(TValue);
                }
            }

            return success;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(TKey key, out TValue value)
        {
            if (key == null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.key);
            }

            IEqualityComparer<TKey> customComparer = _customComparer;
            bool success;
            // Compiler doesn't support ref ternary yet https://github.com/dotnet/roslyn/issues/17797
            if (customComparer == null)
            {
                ref Entry entry = ref Remove<DefaultComparer>(key, out success, null);
                value = entry.value;
                if (success && RuntimeHelpers.IsReferenceOrContainsReferences<TValue>())
                {
                    entry.value = default(TValue);
                }
            }
            else
            {
                ref Entry entry = ref Remove<CustomComparer>(key, out success, customComparer);
                value = entry.value;
                if (success && RuntimeHelpers.IsReferenceOrContainsReferences<TValue>())
                {
                    entry.value = default(TValue);
                }
            }

            return success;
        }

        private ref Entry Remove<TComparer>(TKey key, out bool success, IEqualityComparer<TKey> customComparer) where TComparer : struct, IComparerType
        {
            Debug.Assert(typeof(TComparer) == typeof(DefaultComparer) || typeof(TComparer) == typeof(CustomComparer));
            Debug.Assert(key != null);

            int[] buckets = _buckets;
            if (buckets != null)
            {
                int hashCode = 0;
                if (typeof(TComparer) == typeof(DefaultComparer))
                {
                    // Keys are never null
                    hashCode = key.GetHashCode() & 0x7FFFFFFF;
                }
                else if (typeof(TComparer) == typeof(CustomComparer))
                {
                    hashCode = customComparer.GetHashCode(key) & 0x7FFFFFFF;
                }

                int last = -1;
                ref int bucket = ref buckets[HashHelpers.FindBucket((uint)hashCode, (uint)buckets.Length)];

                Entry[] entries = _entries;
                int i = bucket;
                while (i >= 0)
                {
                    ref Entry entry = ref entries[i];
                    if (entry.hashCode == hashCode)
                    {
                        bool keysEqual = false;
                        if (typeof(TComparer) == typeof(DefaultComparer))
                        {
                            keysEqual = EqualityComparer<TKey>.Default.Equals(entry.key, key);
                        }
                        else if (typeof(TComparer) == typeof(CustomComparer))
                        {
                            keysEqual = customComparer.Equals(entry.key, key);
                        }

                        if (keysEqual)
                        {
                            if (last < 0)
                            {
                                bucket = entry.next;
                            }
                            else
                            {
                                entries[last].next = entry.next;
                            }

                            entry.hashCode = -1;
                            entry.next = _freeList;

                            if (RuntimeHelpers.IsReferenceOrContainsReferences<TKey>())
                            {
                                entry.key = default(TKey);
                            }

                            _freeList = i;
                            _freeCount++;
                            _version++;
                            success = true;

                            return ref entry;
                        }
                    }

                    last = i;
                    i = entry.next;
                }
            }

            success = false;
            return ref NotFound;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            ref Entry entry = ref FindEntry(key, out bool found);
            value = found ? entry.value : default(TValue);
            return found;
        }

        public bool TryAdd(TKey key, TValue value) => TryInsert<RejectIfExisting>(key, value);

        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => false;

        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int index) => CopyTo(array, index);

        void ICollection.CopyTo(Array array, int index)
        {
            if (array == null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.array);
            }

            if (array.Rank != 1)
            {
                ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_RankMultiDimNotSupported);
            }

            if (array.GetLowerBound(0) != 0)
            {
                ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_NonZeroLowerBound);
            }

            if (index < 0 || index > array.Length)
            {
                ThrowHelper.ThrowIndexArgumentOutOfRange_NeedNonNegNumException();
            }

            if (array.Length - index < Count)
            {
                ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_ArrayPlusOffTooSmall);
            }

            KeyValuePair<TKey, TValue>[] pairs = array as KeyValuePair<TKey, TValue>[];
            if (pairs != null)
            {
                CopyTo(pairs, index);
            }
            else if (array is DictionaryEntry[])
            {
                DictionaryEntry[] dictEntryArray = array as DictionaryEntry[];
                Entry[] entries = this._entries;
                for (int i = 0; i < _count; i++)
                {
                    if (entries[i].hashCode >= 0)
                    {
                        dictEntryArray[index++] = new DictionaryEntry(entries[i].key, entries[i].value);
                    }
                }
            }
            else
            {
                object[] objects = array as object[];
                if (objects == null)
                {
                    ThrowHelper.ThrowArgumentException_Argument_InvalidArrayType();
                }

                try
                {
                    int count = this._count;
                    Entry[] entries = this._entries;
                    for (int i = 0; i < count; i++)
                    {
                        if (entries[i].hashCode >= 0)
                        {
                            objects[index++] = new KeyValuePair<TKey, TValue>(entries[i].key, entries[i].value);
                        }
                    }
                }
                catch (ArrayTypeMismatchException)
                {
                    ThrowHelper.ThrowArgumentException_Argument_InvalidArrayType();
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => new Enumerator(this, Enumerator.KeyValuePair);

        bool ICollection.IsSynchronized => false;

        object ICollection.SyncRoot
        {
            get
            {
                if (_syncRoot == null)
                {
                    System.Threading.Interlocked.CompareExchange<Object>(ref _syncRoot, new Object(), null);
                }
                return _syncRoot;
            }
        }

        bool IDictionary.IsFixedSize => false;

        bool IDictionary.IsReadOnly => false;

        ICollection IDictionary.Keys => (ICollection)Keys;

        ICollection IDictionary.Values => (ICollection)Values;

        object IDictionary.this[object key]
        {
            get
            {
                if (IsCompatibleKey(key))
                {
                    ref Entry entry = ref FindEntry((TKey)key, out bool found);
                    if (found)
                    {
                        return entry.value;
                    }
                }
                return null;
            }
            set
            {
                if (key == null)
                {
                    ThrowHelper.ThrowArgumentNullException(ExceptionArgument.key);
                }
                ThrowHelper.IfNullAndNullsAreIllegalThenThrow<TValue>(value, ExceptionArgument.value);

                try
                {
                    TKey tempKey = (TKey)key;
                    try
                    {
                        this[tempKey] = (TValue)value;
                    }
                    catch (InvalidCastException)
                    {
                        ThrowHelper.ThrowWrongValueTypeArgumentException(value, typeof(TValue));
                    }
                }
                catch (InvalidCastException)
                {
                    ThrowHelper.ThrowWrongKeyTypeArgumentException(key, typeof(TKey));
                }
            }
        }

        private static bool IsCompatibleKey(object key)
        {
            if (key == null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.key);
            }
            return (key is TKey);
        }

        void IDictionary.Add(object key, object value)
        {
            if (key == null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.key);
            }
            ThrowHelper.IfNullAndNullsAreIllegalThenThrow<TValue>(value, ExceptionArgument.value);

            try
            {
                TKey tempKey = (TKey)key;

                try
                {
                    Add(tempKey, (TValue)value);
                }
                catch (InvalidCastException)
                {
                    ThrowHelper.ThrowWrongValueTypeArgumentException(value, typeof(TValue));
                }
            }
            catch (InvalidCastException)
            {
                ThrowHelper.ThrowWrongKeyTypeArgumentException(key, typeof(TKey));
            }
        }

        bool IDictionary.Contains(object key) => IsCompatibleKey(key) && ContainsKey((TKey)key);

        IDictionaryEnumerator IDictionary.GetEnumerator() => new Enumerator(this, Enumerator.DictEntry);

        void IDictionary.Remove(object key)
        {
            if (IsCompatibleKey(key))
            {
                Remove((TKey)key);
            }
        }

        public struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>,
            IDictionaryEnumerator
        {
            private Dictionary<TKey, TValue> dictionary;
            private int version;
            private int index;
            private KeyValuePair<TKey, TValue> current;
            private int getEnumeratorRetType;  // What should Enumerator.Current return?

            internal const int DictEntry = 1;
            internal const int KeyValuePair = 2;

            internal Enumerator(Dictionary<TKey, TValue> dictionary, int getEnumeratorRetType)
            {
                this.dictionary = dictionary;
                version = dictionary._version;
                index = 0;
                this.getEnumeratorRetType = getEnumeratorRetType;
                current = new KeyValuePair<TKey, TValue>();
            }

            public bool MoveNext()
            {
                if (version != dictionary._version)
                {
                    ThrowHelper.ThrowInvalidOperationException_InvalidOperation_EnumFailedVersion();
                }

                // Use unsigned comparison since we set index to dictionary.count+1 when the enumeration ends.
                // dictionary.count+1 could be negative if dictionary.count is Int32.MaxValue
                while ((uint)index < (uint)dictionary._count)
                {
                    ref Entry entry = ref dictionary._entries[index++];

                    if (entry.hashCode >= 0)
                    {
                        current = new KeyValuePair<TKey, TValue>(entry.key, entry.value);
                        return true;
                    }
                }

                index = dictionary._count + 1;
                current = new KeyValuePair<TKey, TValue>();
                return false;
            }

            public KeyValuePair<TKey, TValue> Current => current;

            public void Dispose()
            {
            }

            object IEnumerator.Current
            {
                get
                {
                    if (index == 0 || (index == dictionary._count + 1))
                    {
                        ThrowHelper.ThrowInvalidOperationException_InvalidOperation_EnumOpCantHappen();
                    }

                    if (getEnumeratorRetType == DictEntry)
                    {
                        return new System.Collections.DictionaryEntry(current.Key, current.Value);
                    }
                    else
                    {
                        return new KeyValuePair<TKey, TValue>(current.Key, current.Value);
                    }
                }
            }

            void IEnumerator.Reset()
            {
                if (version != dictionary._version)
                {
                    ThrowHelper.ThrowInvalidOperationException_InvalidOperation_EnumFailedVersion();
                }

                index = 0;
                current = new KeyValuePair<TKey, TValue>();
            }

            DictionaryEntry IDictionaryEnumerator.Entry
            {
                get
                {
                    if (index == 0 || (index == dictionary._count + 1))
                    {
                        ThrowHelper.ThrowInvalidOperationException_InvalidOperation_EnumOpCantHappen();
                    }

                    return new DictionaryEntry(current.Key, current.Value);
                }
            }

            object IDictionaryEnumerator.Key
            {
                get
                {
                    if (index == 0 || (index == dictionary._count + 1))
                    {
                        ThrowHelper.ThrowInvalidOperationException_InvalidOperation_EnumOpCantHappen();
                    }

                    return current.Key;
                }
            }

            object IDictionaryEnumerator.Value
            {
                get
                {
                    if (index == 0 || (index == dictionary._count + 1))
                    {
                        ThrowHelper.ThrowInvalidOperationException_InvalidOperation_EnumOpCantHappen();
                    }

                    return current.Value;
                }
            }
        }

        [DebuggerTypeProxy(typeof(DictionaryKeyCollectionDebugView<,>))]
        [DebuggerDisplay("Count = {Count}")]
        public sealed class KeyCollection : ICollection<TKey>, ICollection, IReadOnlyCollection<TKey>
        {
            private Dictionary<TKey, TValue> dictionary;

            public KeyCollection(Dictionary<TKey, TValue> dictionary)
            {
                if (dictionary == null)
                {
                    ThrowHelper.ThrowArgumentNullException(ExceptionArgument.dictionary);
                }
                this.dictionary = dictionary;
            }

            public Enumerator GetEnumerator() => new Enumerator(dictionary);

            public void CopyTo(TKey[] array, int index)
            {
                if (array == null)
                {
                    ThrowHelper.ThrowArgumentNullException(ExceptionArgument.array);
                }

                if (index < 0 || index > array.Length)
                {
                    ThrowHelper.ThrowIndexArgumentOutOfRange_NeedNonNegNumException();
                }

                if (array.Length - index < dictionary.Count)
                {
                    ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_ArrayPlusOffTooSmall);
                }

                int count = dictionary._count;
                Entry[] entries = dictionary._entries;
                for (int i = 0; i < count; i++)
                {
                    if (entries[i].hashCode >= 0) array[index++] = entries[i].key;
                }
            }

            public int Count => dictionary.Count;

            bool ICollection<TKey>.IsReadOnly => true;

            void ICollection<TKey>.Add(TKey item) => ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_KeyCollectionSet);

            void ICollection<TKey>.Clear() => ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_KeyCollectionSet);

            bool ICollection<TKey>.Contains(TKey item) => dictionary.ContainsKey(item);

            bool ICollection<TKey>.Remove(TKey item)
            {
                ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_KeyCollectionSet);
                return false;
            }

            IEnumerator<TKey> IEnumerable<TKey>.GetEnumerator() => new Enumerator(dictionary);

            IEnumerator IEnumerable.GetEnumerator() => new Enumerator(dictionary);

            void ICollection.CopyTo(Array array, int index)
            {
                if (array == null)
                {
                    ThrowHelper.ThrowArgumentNullException(ExceptionArgument.array);
                }

                if (array.Rank != 1)
                {
                    ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_RankMultiDimNotSupported);
                }

                if (array.GetLowerBound(0) != 0)
                {
                    ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_NonZeroLowerBound);
                }

                if (index < 0 || index > array.Length)
                {
                    ThrowHelper.ThrowIndexArgumentOutOfRange_NeedNonNegNumException();
                }

                if (array.Length - index < dictionary.Count)
                {
                    ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_ArrayPlusOffTooSmall);
                }

                TKey[] keys = array as TKey[];
                if (keys != null)
                {
                    CopyTo(keys, index);
                }
                else
                {
                    object[] objects = array as object[];
                    if (objects == null)
                    {
                        ThrowHelper.ThrowArgumentException_Argument_InvalidArrayType();
                    }

                    int count = dictionary._count;
                    Entry[] entries = dictionary._entries;
                    try
                    {
                        for (int i = 0; i < count; i++)
                        {
                            if (entries[i].hashCode >= 0) objects[index++] = entries[i].key;
                        }
                    }
                    catch (ArrayTypeMismatchException)
                    {
                        ThrowHelper.ThrowArgumentException_Argument_InvalidArrayType();
                    }
                }
            }

            bool ICollection.IsSynchronized => false;

            Object ICollection.SyncRoot => ((ICollection)dictionary).SyncRoot;

            public struct Enumerator : IEnumerator<TKey>, System.Collections.IEnumerator
            {
                private Dictionary<TKey, TValue> dictionary;
                private int index;
                private int version;
                private TKey currentKey;

                internal Enumerator(Dictionary<TKey, TValue> dictionary)
                {
                    this.dictionary = dictionary;
                    version = dictionary._version;
                    index = 0;
                    currentKey = default(TKey);
                }

                public void Dispose()
                {
                }

                public bool MoveNext()
                {
                    if (version != dictionary._version)
                    {
                        ThrowHelper.ThrowInvalidOperationException_InvalidOperation_EnumFailedVersion();
                    }

                    while ((uint)index < (uint)dictionary._count)
                    {
                        ref Entry entry = ref dictionary._entries[index++];

                        if (entry.hashCode >= 0)
                        {
                            currentKey = entry.key;
                            return true;
                        }
                    }

                    index = dictionary._count + 1;
                    currentKey = default(TKey);
                    return false;
                }

                public TKey Current => currentKey;

                Object System.Collections.IEnumerator.Current
                {
                    get
                    {
                        if (index == 0 || (index == dictionary._count + 1))
                        {
                            ThrowHelper.ThrowInvalidOperationException_InvalidOperation_EnumOpCantHappen();
                        }

                        return currentKey;
                    }
                }

                void System.Collections.IEnumerator.Reset()
                {
                    if (version != dictionary._version)
                    {
                        ThrowHelper.ThrowInvalidOperationException_InvalidOperation_EnumFailedVersion();
                    }

                    index = 0;
                    currentKey = default(TKey);
                }
            }
        }

        [DebuggerTypeProxy(typeof(DictionaryValueCollectionDebugView<,>))]
        [DebuggerDisplay("Count = {Count}")]
        public sealed class ValueCollection : ICollection<TValue>, ICollection, IReadOnlyCollection<TValue>
        {
            private Dictionary<TKey, TValue> dictionary;

            public ValueCollection(Dictionary<TKey, TValue> dictionary)
            {
                if (dictionary == null)
                {
                    ThrowHelper.ThrowArgumentNullException(ExceptionArgument.dictionary);
                }
                this.dictionary = dictionary;
            }

            public Enumerator GetEnumerator() => new Enumerator(dictionary);

            public void CopyTo(TValue[] array, int index)
            {
                if (array == null)
                {
                    ThrowHelper.ThrowArgumentNullException(ExceptionArgument.array);
                }

                if (index < 0 || index > array.Length)
                {
                    ThrowHelper.ThrowIndexArgumentOutOfRange_NeedNonNegNumException();
                }

                if (array.Length - index < dictionary.Count)
                {
                    ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_ArrayPlusOffTooSmall);
                }

                int count = dictionary._count;
                Entry[] entries = dictionary._entries;
                for (int i = 0; i < count; i++)
                {
                    if (entries[i].hashCode >= 0) array[index++] = entries[i].value;
                }
            }

            public int Count => dictionary.Count;

            bool ICollection<TValue>.IsReadOnly => true;

            void ICollection<TValue>.Add(TValue item) => ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_ValueCollectionSet);

            bool ICollection<TValue>.Remove(TValue item)
            {
                ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_ValueCollectionSet);
                return false;
            }

            void ICollection<TValue>.Clear() => ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_ValueCollectionSet);

            bool ICollection<TValue>.Contains(TValue item) => dictionary.ContainsValue(item);

            IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator() => new Enumerator(dictionary);

            IEnumerator IEnumerable.GetEnumerator() => new Enumerator(dictionary);

            void ICollection.CopyTo(Array array, int index)
            {
                if (array == null)
                {
                    ThrowHelper.ThrowArgumentNullException(ExceptionArgument.array);
                }

                if (array.Rank != 1)
                {
                    ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_RankMultiDimNotSupported);
                }

                if (array.GetLowerBound(0) != 0)
                {
                    ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_NonZeroLowerBound);
                }

                if (index < 0 || index > array.Length)
                {
                    ThrowHelper.ThrowIndexArgumentOutOfRange_NeedNonNegNumException();
                }

                if (array.Length - index < dictionary.Count)
                    ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_ArrayPlusOffTooSmall);

                TValue[] values = array as TValue[];
                if (values != null)
                {
                    CopyTo(values, index);
                }
                else
                {
                    object[] objects = array as object[];
                    if (objects == null)
                    {
                        ThrowHelper.ThrowArgumentException_Argument_InvalidArrayType();
                    }

                    int count = dictionary._count;
                    Entry[] entries = dictionary._entries;
                    try
                    {
                        for (int i = 0; i < count; i++)
                        {
                            if (entries[i].hashCode >= 0) objects[index++] = entries[i].value;
                        }
                    }
                    catch (ArrayTypeMismatchException)
                    {
                        ThrowHelper.ThrowArgumentException_Argument_InvalidArrayType();
                    }
                }
            }

            bool ICollection.IsSynchronized => false;

            Object ICollection.SyncRoot => ((ICollection)dictionary).SyncRoot;

            public struct Enumerator : IEnumerator<TValue>, System.Collections.IEnumerator
            {
                private Dictionary<TKey, TValue> dictionary;
                private int index;
                private int version;
                private TValue currentValue;

                internal Enumerator(Dictionary<TKey, TValue> dictionary)
                {
                    this.dictionary = dictionary;
                    version = dictionary._version;
                    index = 0;
                    currentValue = default(TValue);
                }

                public void Dispose()
                {
                }

                public bool MoveNext()
                {
                    if (version != dictionary._version)
                    {
                        ThrowHelper.ThrowInvalidOperationException_InvalidOperation_EnumFailedVersion();
                    }

                    while ((uint)index < (uint)dictionary._count)
                    {
                        ref Entry entry = ref dictionary._entries[index++];

                        if (entry.hashCode >= 0)
                        {
                            currentValue = entry.value;
                            return true;
                        }
                    }
                    index = dictionary._count + 1;
                    currentValue = default(TValue);
                    return false;
                }

                public TValue Current => currentValue;

                Object System.Collections.IEnumerator.Current
                {
                    get
                    {
                        if (index == 0 || (index == dictionary._count + 1))
                        {
                            ThrowHelper.ThrowInvalidOperationException_InvalidOperation_EnumOpCantHappen();
                        }

                        return currentValue;
                    }
                }

                void System.Collections.IEnumerator.Reset()
                {
                    if (version != dictionary._version)
                    {
                        ThrowHelper.ThrowInvalidOperationException_InvalidOperation_EnumFailedVersion();
                    }
                    index = 0;
                    currentValue = default(TValue);
                }
            }
        }
    }
}
