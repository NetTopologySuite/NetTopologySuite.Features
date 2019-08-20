using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

using NUnit.Framework;

namespace NetTopologySuite.Features.Test
{
    [TestFixture]
    public sealed class DictionaryBaseTests
    {
        [Test]
        public void TestReadWrite()
        {
            var expected = new Dictionary<string, object>();
            var actual = new DictionarySubclass<string, object>(expected, false);

            // test everything after a serialize + deserialize round-trip
            ExpectedAndActual.RoundTrip(ref expected, ref actual);

            Assert.That(!actual.IsReadOnly);

            expected.Add("hello", "hi");
            actual.Add("hello", "hi");

            AssertEqual(expected, actual);

            expected.Add("oh", 321);
            actual.Add("oh", 321);

            AssertEqual(expected, actual);

            expected.Remove("hello");
            Assert.That(actual.Remove("hello"));

            AssertEqual(expected, actual);

            Assert.That(!actual.Remove("hello"));

            AssertEqual(expected, actual);

            expected.Add("hello", new object());
            actual.Add("hello", expected["hello"]);

            AssertEqual(expected, actual);

            Assert.That(() => actual.Add("hello", "oh, hi there"), Throws.ArgumentException);

            AssertEqual(expected, actual);

            Assert.That(!((IDictionary<string, object>)actual).Remove(KeyValuePair.Create("hello", new object())));

            AssertEqual(expected, actual);

            ((IDictionary<string, object>)expected).Remove(KeyValuePair.Create("hello", expected["hello"]));
            Assert.That(((IDictionary<string, object>)actual).Remove(KeyValuePair.Create("hello", actual["hello"])));

            AssertEqual(expected, actual);

            Assert.That(() => actual["a key that I won't use anywhere else"], Throws.InstanceOf<KeyNotFoundException>());

            AssertEqual(expected, actual);

            expected.Clear();
            actual.Clear();

            AssertEqual(expected, actual);
        }

        [Test]
        public void TestReadOnly()
        {
            var expected= new Dictionary<string, object>
            {
                ["hello"] = new object(),
                ["hi"] = Guid.NewGuid(),
                ["rando"] = null,
                ["good"] = "bye",
            };

            var actual = new DictionarySubclass<string, object>(expected, true);

            // test everything after a serialize + deserialize round-trip
            ExpectedAndActual.RoundTrip(ref expected, ref actual);

            Assert.That(actual.IsReadOnly);

            AssertEqual(expected, actual);

            Assert.That(() => actual["x"] = 1, Throws.InstanceOf<NotSupportedException>());
            AssertEqual(expected, actual);

            Assert.That(() => actual.Add("x", 1), Throws.InstanceOf<NotSupportedException>());
            AssertEqual(expected, actual);

            Assert.That(() => actual.Remove("hello"), Throws.InstanceOf<NotSupportedException>());
            AssertEqual(expected, actual);

            Assert.That(() => ((IDictionary<string, object>)actual).Remove(KeyValuePair.Create("hello", expected["hello"])), Throws.InstanceOf<NotSupportedException>());
            AssertEqual(expected, actual);

            Assert.That(() => actual.Clear(), Throws.InstanceOf<NotSupportedException>());
            AssertEqual(expected, actual);
        }

        private static void AssertEqual(Dictionary<string, object> expected, DictionarySubclass<string, object> actual)
        {
            // test everything after a serialize + deserialize round-trip
            ExpectedAndActual.RoundTrip(ref expected, ref actual);

            // use the interfaces since there are explicit implementations
            IDictionary<string, object> expectedDict = expected;
            IDictionary<string, object> actualDict = actual;

            Assert.That(actualDict.Count, Is.EqualTo(expectedDict.Count));
            Assert.That(actualDict, Is.EquivalentTo(expectedDict));

            Assert.That(actualDict.Keys.Count, Is.EqualTo(expectedDict.Keys.Count));
            Assert.That(actualDict.Keys, Is.EquivalentTo(expectedDict.Keys));

            Assert.That(actualDict.Values.Count, Is.EqualTo(expectedDict.Values.Count));
            Assert.That(actualDict.Values, Is.EquivalentTo(expectedDict.Values));

            foreach ((string key, object value) in expectedDict)
            {
                Assert.That(actualDict.ContainsKey(key));
                Assert.That(actualDict.Contains(KeyValuePair.Create(key, value)));
                Assert.That(!actualDict.Contains(KeyValuePair.Create(key, new object())));
                Assert.That(actualDict.Keys.Contains(key));
                Assert.That(actualDict.Values.Contains(value));
                Assert.That(actualDict[key], Is.EqualTo(value));

                Assert.That(actualDict.TryGetValue(key, out object retrieved));
                Assert.That(retrieved, Is.EqualTo(value));

                if (!actualDict.IsReadOnly)
                {
                    Assert.That(!actualDict.Remove(KeyValuePair.Create(key, new object())));
                }
            }

            var pairs = new KeyValuePair<string, object>[actualDict.Count + 4];
            actualDict.CopyTo(pairs, 3);
            Assert.That(pairs.Skip(3).Take(actualDict.Count), Is.EquivalentTo(expectedDict));

            string[] keys = new string[actualDict.Count + 5];
            actualDict.Keys.CopyTo(keys, 2);
            Assert.That(keys.Skip(2).Take(actualDict.Count), Is.EquivalentTo(expectedDict.Keys));

            object[] values = new object[actualDict.Count + 8];
            actualDict.Values.CopyTo(values, 1);
            Assert.That(values.Skip(1).Take(actualDict.Count), Is.EquivalentTo(expectedDict.Values));
        }

        [Serializable]
        private sealed class ExpectedAndActual
        {
            private readonly Dictionary<string, object> _expected;

            private readonly DictionarySubclass<string, object> _actual;

            private ExpectedAndActual(Dictionary<string, object> expected, DictionarySubclass<string, object> actual) =>
                (_expected, _actual) = (expected, actual);

            public static void RoundTrip(ref Dictionary<string, object> expected, ref DictionarySubclass<string, object> actual)
            {
                using (var ms = new MemoryStream())
                {
                    new BinaryFormatter().Serialize(ms, new ExpectedAndActual(expected, actual));
                    ms.Position = 0;
                    var deserialized = (ExpectedAndActual)new BinaryFormatter().Deserialize(ms);
                    expected = deserialized._expected;
                    actual = deserialized._actual;
                }
            }
        }

        [Serializable]
        private sealed class DictionarySubclass<TKey, TValue> : DictionaryBase<TKey, TValue>
        {
            private readonly Dictionary<TKey, TValue> _dict;

            public DictionarySubclass(Dictionary<TKey, TValue> dict, bool isReadOnly)
                : base(isReadOnly)
            {
                _dict = new Dictionary<TKey, TValue>(dict, dict.Comparer);
            }

            private DictionarySubclass(SerializationInfo info, StreamingContext context)
                : base(info, context)
            {
                _dict = (Dictionary<TKey, TValue>)info.GetValue("dict", typeof(Dictionary<TKey, TValue>));
            }

            public override int Count => _dict.Count;

            public override IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => _dict.GetEnumerator();

            protected override void ClearCore() => _dict.Clear();

            protected override bool GetOrRemoveValue(TKey key, out TValue value, bool remove)
            {
                if (remove)
                {
                    value = default;
                    return _dict.Remove(key);
                }

                return _dict.TryGetValue(key, out value);
            }

            protected override bool SetValue(TKey key, TValue value, bool onlyIfMissing)
            {
                if (onlyIfMissing)
                {
                    return _dict.TryAdd(key, value);
                }

                _dict[key] = value;
                return true;
            }

            public override void GetObjectData(SerializationInfo info, StreamingContext context)
            {
                base.GetObjectData(info, context);
                info.AddValue("dict", _dict);
            }
        }
    }
}
