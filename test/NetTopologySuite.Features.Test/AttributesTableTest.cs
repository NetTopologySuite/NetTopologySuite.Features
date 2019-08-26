using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

using NUnit.Framework;

namespace NetTopologySuite.Features.Test
{
    [TestFixture]
    public sealed class AttributesTableTests
    {
        private bool _savedAddAttributeWithIndexer;

        [SetUp]
        public void SetUp() => _savedAddAttributeWithIndexer = AttributesTable.AddAttributeWithIndexer;

        [TearDown]
        public void TearDown() => AttributesTable.AddAttributeWithIndexer = _savedAddAttributeWithIndexer;

        [Test]
        public void TestBasicReadAndWriteOperations()
        {
            var expected = new Dictionary<string, object>();
            var actual = new AttributesTable();

            // test everything after a serialize + deserialize round-trip
            ExpectedAndActual.RoundTrip(ref expected, ref actual);

            expected.Add("hello", "hi");
            actual.Add("hello", "hi");

            AssertEqual(expected, actual);

            AttributesTable.AddAttributeWithIndexer = false;
            Assert.That(() => actual["oh"] = 321, Throws.ArgumentException);

            AssertEqual(expected, actual);

            AttributesTable.AddAttributeWithIndexer = true;
            expected["oh"] = 321;
            Assert.That(() => actual["oh"] = 321, Throws.Nothing);

            AssertEqual(expected, actual);

            AssertEqual(expected, actual);

            expected.Remove("hello");
            actual.DeleteAttribute("hello");

            AssertEqual(expected, actual);

            Assert.That(() => actual.DeleteAttribute("hello"), Throws.ArgumentException);

            AssertEqual(expected, actual);

            expected.Add("hello", new object());
            actual.Add("hello", expected["hello"]);

            AssertEqual(expected, actual);

            expected["hello"] = Guid.NewGuid();
            actual["hello"] = expected["hello"];

            AssertEqual(expected, actual);

            Assert.That(() => actual.Add("hello", "oh, hi there"), Throws.ArgumentException);

            AssertEqual(expected, actual);

            Assert.That(() => actual["a key that shouldn't exist"], Throws.ArgumentException);
            Assert.That(() => actual.GetType("a key that shouldn't exist"), Throws.InstanceOf<ArgumentOutOfRangeException>());

            AssertEqual(expected, actual);
        }

        [Test]
        public void TestMergeWith_PreferThis()
        {
            var table1 = new AttributesTable
            {
                { "1", "a" },
                { "2", "b" },
                { "3", "c" },
            };

            var table2 = new AttributesTable
            {
                { "2", "x" },
                { "3", "y" },
                { "4", "z" },
            };

            table1.MergeWith(table2, preferThis: true);

            var expected = new Dictionary<string, object>
            {
                { "1", "a" },
                { "2", "b" },
                { "3", "c" },
                { "4", "z" },
            };

            AssertEqual(expected, table1);
        }

        [Test]
        public void TestMergeWith_DoNotPreferThis()
        {
            var table1 = new AttributesTable
            {
                { "1", "a" },
                { "2", "b" },
                { "3", "c" },
            };

            var table2 = new AttributesTable
            {
                { "2", "x" },
                { "3", "y" },
                { "4", "z" },
            };

            table1.MergeWith(table2, preferThis: false);

            var expected = new Dictionary<string, object>
            {
                { "1", "a" },
                { "2", "x" },
                { "3", "y" },
                { "4", "z" },
            };

            AssertEqual(expected, table1);
        }

        private static void AssertEqual(Dictionary<string, object> expected, AttributesTable actual)
        {
            // test everything after a serialize + deserialize round-trip
            ExpectedAndActual.RoundTrip(ref expected, ref actual);

            Assert.That(actual.Count, Is.EqualTo(expected.Count));
            Assert.That(actual, Is.EquivalentTo(expected));

            Assert.That(actual.GetNames().Count, Is.EqualTo(expected.Keys.Count));
            Assert.That(actual.GetNames(), Is.EquivalentTo(expected.Keys));

            Assert.That(actual.GetValues().Count, Is.EqualTo(expected.Values.Count));
            Assert.That(actual.GetValues(), Is.EquivalentTo(expected.Values));

            foreach ((string key, object value) in expected)
            {
                Assert.That(actual.Exists(key));
                Assert.That(actual[key], Is.EqualTo(value));

                object retrieved = actual.GetOptionalValue(key);
                Assert.That(retrieved, Is.EqualTo(value));

                Assert.That(actual.GetType(key), Is.EqualTo(value?.GetType() ?? typeof(object)));
            }
        }

        [Serializable]
        private sealed class ExpectedAndActual
        {
            private readonly Dictionary<string, object> _expected;

            private readonly AttributesTable _actual;

            private ExpectedAndActual(Dictionary<string, object> expected, AttributesTable actual) =>
                (_expected, _actual) = (expected, actual);

            public static void RoundTrip(ref Dictionary<string, object> expected, ref AttributesTable actual)
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
    }
}
