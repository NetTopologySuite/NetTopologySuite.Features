using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace NetTopologySuite.Features
{
    /// <summary>
    /// A base class to help implement the <see cref="IDictionary{TKey, TValue}"/> interface with
    /// far fewer required members than the interface.
    /// </summary>
    /// <typeparam name="TKey">
    /// The type of key in the collection.
    /// </typeparam>
    /// <typeparam name="TValue"></typeparam>
    [Serializable]
    public abstract class DictionaryBase<TKey, TValue> : IReadOnlyDictionary<TKey, TValue>, IDictionary<TKey, TValue>, ISerializable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DictionaryBase{TKey, TValue}"/> class.
        /// </summary>
        protected DictionaryBase()
            : this(isReadOnly: false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DictionaryBase{TKey, TValue}"/> class.
        /// </summary>
        /// <param name="isReadOnly">
        /// <see langword="true"/> if all public attempts to modify this dictionary should fail,
        /// <see langword="false"/> otherwise.
        /// </param>
        protected DictionaryBase(bool isReadOnly)
        {
            IsReadOnly = isReadOnly;
            Keys = new KeyCollection(this);
            Values = new ValueCollection(this);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DictionaryBase{TKey, TValue}"/> class with
        /// serialized data.
        /// </summary>
        /// <param name="info">
        /// A <see cref="SerializationInfo"/> object containing the information required to
        /// serialize the <see cref="DictionaryBase{TKey, TValue}"/>.
        /// </param>
        /// <param name="context">
        /// A <see cref="StreamingContext"/> structure containing the source and destination of the
        /// serialized stream associated with the <see cref="DictionaryBase{TKey, TValue}"/>.
        /// </param>
        protected DictionaryBase(SerializationInfo info, StreamingContext context)
            : this(info.GetBoolean("IsReadOnly"))
        {
        }

        /// <inheritdoc />
        public bool IsReadOnly { get; }

        /// <inheritdoc />
        public abstract int Count { get; }

        /// <inheritdoc />
        public abstract IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator();

        /// <summary>
        /// Clears all data in this dictionary.
        /// </summary>
        /// <remarks>
        /// The base class will never call this method on read-only instances.
        /// </remarks>
        protected abstract void ClearCore();

        /// <summary>
        /// Accesses a given key, retrieving or removing its value.
        /// </summary>
        /// <param name="key">
        /// The <typeparamref name="TKey"/> key to access.
        /// </param>
        /// <param name="value">
        /// A variable that will receive either the <typeparamref name="TValue"/> value (if it's
        /// present) or the default value for the type if it's absent.
        /// </param>
        /// <param name="remove">
        /// <see langword="true"/> if this key should be removed if present, <see langword="false"/>
        /// otherwise.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="key"/> was present in the dictionary,
        /// <see langword="false"/> otherwise.
        /// </returns>
        /// <remarks>
        /// The base class will never call this method with <paramref name="remove"/> set to
        /// <see langword="true"/> on read-only instances.
        /// </remarks>
        protected abstract bool GetOrRemoveValue(TKey key, out TValue value, bool remove);

        /// <summary>
        /// Attempts to set the value of a particular key.
        /// </summary>
        /// <param name="key">
        /// The <typeparamref name="TKey"/> key whose value to attempt to set.
        /// </param>
        /// <param name="value">
        /// The <typeparamref name="TValue"/> value to attempt to set for the key.
        /// </param>
        /// <param name="onlyIfMissing">
        /// A value indicating whether or not to only set the value if it's missing (to avoid a
        /// potentially unnecessary lookup).
        /// </param>
        /// <returns>
        /// <see langword="false"/> if <paramref name="onlyIfMissing"/> is <see langword="true"/>
        /// and this dictionary already contains a value for <paramref name="key"/>,
        /// <see langword="true"/> otherwise.
        /// </returns>
        /// <remarks>
        /// <para>
        /// The base class will never call this method on read-only instances.
        /// </para>
        /// <para>
        /// The <b>only</b> circumstance where this method should return <see langword="false"/> is
        /// if <paramref name="onlyIfMissing"/> is <see langword="true"/> and the key is present in
        /// the dictionary.
        /// </para>
        /// </remarks>
        protected abstract bool SetValue(TKey key, TValue value, bool onlyIfMissing);

        /// <inheritdoc />
        public KeyCollection Keys { get; }
        ICollection<TKey> IDictionary<TKey, TValue>.Keys => Keys;
        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => Keys;

        /// <inheritdoc />
        public ValueCollection Values { get; }
        ICollection<TValue> IDictionary<TKey, TValue>.Values => Values;
        IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => Values;

        /// <summary>
        /// Gets or sets an <see cref="IEqualityComparer{T}"/> to use to compare two instances of
        /// <typeparamref name="TValue"/> for equality, or <see langword="null"/> to use the default
        /// comparer for its type.
        /// </summary>
        public IEqualityComparer<TValue> ValueComparer { get; set; }

        /// <inheritdoc />
        public TValue this[TKey key]
        {
            get
            {
                if (!GetOrRemoveValue(key, out var value, false))
                {
                    ThrowHelpers.ThrowKeyNotFoundException();
                }

                return value;
            }

            set
            {
                if (IsReadOnly)
                {
                    ThrowHelpers.ThrowNotSupportedExceptionForReadOnlyCollection();
                }

                SetValue(key, value, false);
            }
        }

        /// <inheritdoc />
        public bool ContainsKey(TKey key) => GetOrRemoveValue(key, out _, false);

        /// <inheritdoc />
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            foreach (var kvp in this)
            {
                array[arrayIndex++] = kvp;
            }
        }

        /// <inheritdoc />
        public void Add(TKey key, TValue value)
        {
            if (IsReadOnly)
            {
                ThrowHelpers.ThrowNotSupportedExceptionForReadOnlyCollection();
            }

            if (!SetValue(key, value, true))
            {
                ThrowHelpers.ThrowArgumentExceptionForDuplicateKey();
            }
        }

        /// <inheritdoc />
        public bool Remove(TKey key)
        {
            if (IsReadOnly)
            {
                ThrowHelpers.ThrowNotSupportedExceptionForReadOnlyCollection();
            }

            return GetOrRemoveValue(key, out _, true);
        }

        /// <inheritdoc />
        public bool TryGetValue(TKey key, out TValue value) => GetOrRemoveValue(key, out value, false);

        /// <inheritdoc />
        public void Clear()
        {
            if (IsReadOnly)
            {
                ThrowHelpers.ThrowNotSupportedExceptionForReadOnlyCollection();
            }

            ClearCore();
        }

        /// <inheritdoc />
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info is null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            info.AddValue("IsReadOnly", IsReadOnly);
        }

        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> kvp) => Add(kvp.Key, kvp.Value);

        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
        {
            if (!GetOrRemoveValue(item.Key, out var value, false))
            {
                return false;
            }

            var valueComparer = ValueComparer ?? EqualityComparer<TValue>.Default;
            return valueComparer.Equals(item.Value, value);
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
        {
            if (IsReadOnly)
            {
                ThrowHelpers.ThrowNotSupportedExceptionForReadOnlyCollection();
            }

            if (!TryGetValue(item.Key, out var value))
            {
                return false;
            }

            var valueComparer = ValueComparer ?? EqualityComparer<TValue>.Default;
            if (!valueComparer.Equals(item.Value, value))
            {
                return false;
            }

            return GetOrRemoveValue(item.Key, out _, true);
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Represents the collection of keys in a <see cref="DictionaryBase{TKey, TValue}"/>.
        /// </summary>
        public sealed class KeyCollection : ICollection<TKey>
        {
            private readonly DictionaryBase<TKey, TValue> _dict;

            internal KeyCollection(DictionaryBase<TKey, TValue> dict) => _dict = dict;

            /// <inheritdoc />
            public int Count => _dict.Count;

            /// <inheritdoc />
            public void CopyTo(TKey[] array, int arrayIndex)
            {
                foreach (var kvp in _dict)
                {
                    array[arrayIndex++] = kvp.Key;
                }
            }

            /// <inheritdoc />
            public Enumerator GetEnumerator() => new Enumerator(_dict);

            bool ICollection<TKey>.IsReadOnly => true;
            IEnumerator<TKey> IEnumerable<TKey>.GetEnumerator() => GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
            bool ICollection<TKey>.Contains(TKey item) => _dict.ContainsKey(item);
            void ICollection<TKey>.Add(TKey item) => ThrowHelpers.ThrowNotSupportedExceptionForReadOnlyCollection();
            void ICollection<TKey>.Clear() => ThrowHelpers.ThrowNotSupportedExceptionForReadOnlyCollection();
            bool ICollection<TKey>.Remove(TKey item) { ThrowHelpers.ThrowNotSupportedExceptionForReadOnlyCollection(); return false; }

            /// <summary>
            /// Enumerates the elements of a <see cref="KeyCollection"/>.
            /// </summary>
            public struct Enumerator : IEnumerator<TKey>
            {
                private readonly IEnumerator<KeyValuePair<TKey, TValue>> _enumerator;

                internal Enumerator(DictionaryBase<TKey, TValue> dict) => _enumerator = dict.GetEnumerator();

                /// <inheritdoc />
                public TKey Current => _enumerator.Current.Key;

                /// <inheritdoc />
                public void Dispose() => _enumerator.Dispose();

                /// <inheritdoc />
                public bool MoveNext() => _enumerator.MoveNext();

                object IEnumerator.Current => Current;
                void IEnumerator.Reset() => _enumerator.Reset();
            }
        }

        /// <summary>
        /// Represents the collection of values in a <see cref="DictionaryBase{TKey, TValue}"/>.
        /// </summary>
        public sealed class ValueCollection : ICollection<TValue>
        {
            private readonly DictionaryBase<TKey, TValue> _dict;

            internal ValueCollection(DictionaryBase<TKey, TValue> dict) => _dict = dict;

            /// <inheritdoc />
            public int Count => _dict.Count;

            /// <inheritdoc />
            public void CopyTo(TValue[] array, int arrayIndex)
            {
                foreach (var kvp in _dict)
                {
                    array[arrayIndex++] = kvp.Value;
                }
            }

            /// <inheritdoc />
            public Enumerator GetEnumerator() => new Enumerator(_dict);

            bool ICollection<TValue>.IsReadOnly => true;
            IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator() => GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            bool ICollection<TValue>.Contains(TValue item)
            {
                var valueComparer = _dict.ValueComparer ?? EqualityComparer<TValue>.Default;
                foreach (var kvp in _dict)
                {
                    if (valueComparer.Equals(kvp.Value, item))
                    {
                        return true;
                    }
                }

                return false;
            }

            void ICollection<TValue>.Add(TValue item) => ThrowHelpers.ThrowNotSupportedExceptionForReadOnlyCollection();
            void ICollection<TValue>.Clear() => ThrowHelpers.ThrowNotSupportedExceptionForReadOnlyCollection();
            bool ICollection<TValue>.Remove(TValue item) { ThrowHelpers.ThrowNotSupportedExceptionForReadOnlyCollection(); return false; }

            /// <summary>
            /// Enumerates the elements of a <see cref="ValueCollection"/>.
            /// </summary>
            public struct Enumerator : IEnumerator<TValue>
            {
                private readonly IEnumerator<KeyValuePair<TKey, TValue>> _enumerator;

                internal Enumerator(DictionaryBase<TKey, TValue> dict) => _enumerator = dict.GetEnumerator();

                /// <inheritdoc />
                public TValue Current => _enumerator.Current.Value;

                /// <inheritdoc />
                public void Dispose() => _enumerator.Dispose();

                /// <inheritdoc />
                public bool MoveNext() => _enumerator.MoveNext();

                object IEnumerator.Current => Current;
                void IEnumerator.Reset() => _enumerator.Reset();
            }
        }
    }
}
