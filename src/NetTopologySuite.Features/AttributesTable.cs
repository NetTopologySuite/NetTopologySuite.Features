using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace NetTopologySuite.Features
{
    /// <summary>
    /// Stores all attributes associated with a single <c>Geometry</c> feature.
    /// </summary>
    [Serializable]
    public sealed class AttributesTable : IAttributesTable, IEnumerable<KeyValuePair<string, object>>, ISerializable
    {
        private readonly Dictionary<string, object> _attributes;

        /// <summary>
        /// Gets or sets a value indicating if setting <see cref="this[string]"/> with a
        /// nonexistant index will throw an exception or if the attribute/value pair will
        /// silently be added.
        /// </summary>
        public static bool AddAttributeWithIndexer { get; set; }

        /// <summary>
        /// Creates an instance of this class using the default equality comparer for names.
        /// </summary>
        public AttributesTable()
            : this(default(IEqualityComparer<string>))
        {
        }

        /// <summary>
        /// Creates an instance of this class using the given equality comparer for names.
        /// </summary>
        /// <param name="nameComparer">The <see cref="IEqualityComparer{T}"/> to use for comparing names, or <see langword="null"/> to use the default.</param>
        public AttributesTable(IEqualityComparer<string> nameComparer)
            : this(new Dictionary<string, object>(nameComparer))
        {
        }

        /// <summary>
        /// Creates an instance of this class using the provided enumeration of key/value pairs and
        /// the default equality comparer for names.
        /// </summary>
        /// <param name="attributes">An enumeration of key/value pairs</param>
        public AttributesTable(IEnumerable<KeyValuePair<string, object>> attributes)
            : this(attributes, null)
        {
        }

        /// <summary>
        /// Creates an instance of this class using the provided enumeration of key/value pairs and
        /// the given equality comparer for names.
        /// </summary>
        /// <param name="attributes">An enumeration of key/value pairs</param>
        /// <param name="nameComparer">The <see cref="IEqualityComparer{T}"/> to use for comparing names, or <see langword="null"/> to use the default.</param>
        public AttributesTable(IEnumerable<KeyValuePair<string, object>> attributes, IEqualityComparer<string> nameComparer)
            : this(nameComparer)
        {
            foreach (var kvp in attributes)
            {
                Add(kvp.Key, kvp.Value);
            }
        }

        /// <summary>
        /// Creates an instance of this class using the provided enumeration of key/value pairs
        /// </summary>
        /// <param name="attributes">An attributes dictionary</param>
        /// <exception cref="ArgumentNullException">If the attributes are null</exception>
        public AttributesTable(Dictionary<string, object> attributes)
        {
            _attributes = attributes ?? throw new ArgumentNullException(nameof(attributes));
        }

        private AttributesTable(SerializationInfo info, StreamingContext context)
        {
            _attributes = (Dictionary<string, object>)info.GetValue("_attributes", typeof(Dictionary<string, object>));
        }

        /// <inheritdoc />
        public int Count => _attributes.Count;

        /// <inheritdoc />
        public object this[string attributeName]
        {
            get => GetValue(attributeName);
            set => SetValue(attributeName, value);
        }

        /// <inheritdoc />
        public string[] GetNames()
        {
            return _attributes.Keys.ToArray();
        }

        /// <inheritdoc />
        public object[] GetValues()
        {
            return _attributes.Values.ToArray();
        }

        /// <inheritdoc />
        public bool Exists(string attributeName)
        {
            return _attributes.ContainsKey(attributeName);
        }

        /// <inheritdoc />
        public void DeleteAttribute(string attributeName)
        {
            if (!_attributes.Remove(attributeName))
            {
                throw new ArgumentException($"Attribute {attributeName} does not exist!", nameof(attributeName));
            }
        }

        /// <inheritdoc />
        public Type GetType(string attributeName)
        {
            return _attributes.TryGetValue(attributeName, out object value)
                ? value?.GetType() ?? typeof(object)
                : throw new ArgumentOutOfRangeException($"Attribute {attributeName} does not exist!", nameof(attributeName));
        }

        /// <summary>
        /// Method to merge this attribute table with another attribute table
        /// </summary>
        /// <param name="other">The other attribute table</param>
        /// <param name="preferThis">A value indicating if values in this attribute table are preferable
        /// over those in <paramref name="other"/>.  The default is <value>true</value>.
        /// </param>
        public void MergeWith(IAttributesTable other, bool preferThis = true)
        {
            foreach (string name in other.GetNames())
            {
                if (!(preferThis && Exists(name)))
                {
                    _attributes[name] = other[name];
                }
            }
        }

        /// <inheritdoc />
        public void Add(string attributeName, object attributeValue)
        {
            if (Exists(attributeName))
            {
                throw new ArgumentException($"Attribute {attributeName} already exists!", nameof(attributeName));
            }

            _attributes.Add(attributeName, attributeValue);
        }

        /// <inheritdoc />
        public object GetOptionalValue(string attributeName)
        {
            _attributes.TryGetValue(attributeName, out object value);
            return value;
        }

        /// <inheritdoc cref="IEnumerable{T}.GetEnumerator" />
        public Dictionary<string, object>.Enumerator GetEnumerator()
        {
            return _attributes.GetEnumerator();
        }

        IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator()
        {
            return ((IEnumerable<KeyValuePair<string, object>>)_attributes).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_attributes).GetEnumerator();
        }

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info is null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            info.AddValue("_attributes", _attributes);
        }

        private object GetValue(string attributeName)
        {
            return _attributes.TryGetValue(attributeName, out object value)
                ? value
                : throw new ArgumentException($"Attribute {attributeName} does not exist!", nameof(attributeName));
        }

        private void SetValue(string attributeName, object value)
        {
            if (!AddAttributeWithIndexer && !Exists(attributeName))
            {
                throw new ArgumentException($"Attribute {attributeName} does not exist!", nameof(attributeName));
            }

            _attributes[attributeName] = value;
        }
    }
}
