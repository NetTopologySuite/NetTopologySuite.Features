using NetTopologySuite.Geometries;

using NUnit.Framework;

namespace NetTopologySuite.Features.Test
{
    /// <summary>
    /// A test fixture for the <see cref="FeatureExtensions"/> class
    /// </summary>
    public sealed class FeatureExtensionsTest
    {
        [Test]
        public void GetOptionalIdTest_FromIUnique()
        {
            string idPropertyName = TestContext.CurrentContext.Random.GetString();
            object idValue = TestContext.CurrentContext.Random.NextGuid();
            object notIdValue = TestContext.CurrentContext.Random.NextGuid();
            var feature = new UniqueFeature(idValue) { Attributes = new AttributesTable { { idPropertyName, notIdValue } } };

            Assert.That(feature.GetOptionalId(idPropertyName), Is.EqualTo(idValue));
        }

        [Test]
        public void GetOptionalIdTest_FromAttributes()
        {
            string idPropertyName = TestContext.CurrentContext.Random.GetString();
            object idValue = TestContext.CurrentContext.Random.NextGuid();
            var feature = new Feature { Attributes = new AttributesTable { { idPropertyName, idValue } } };

            Assert.That(feature.GetOptionalId(idPropertyName), Is.EqualTo(idValue));
        }

        [Test]
        public void GetOptionalIdTest_Missing()
        {
            Assert.That(default(IFeature).GetOptionalId("id"), Is.Null);
            Assert.That(new Feature { Attributes = null }.GetOptionalId("id"), Is.Null);
            Assert.That(new Feature { Attributes = new AttributesTable { { "notId", "ignored" } } }.GetOptionalId("id"), Is.Null);
        }

        private sealed class UniqueFeature : IFeature, IUnique
        {
            public UniqueFeature(object id)
                => Id = id;

            public object Id { get; }
            public Geometry Geometry { get; set; }
            public Envelope BoundingBox { get; set; }
            public IAttributesTable Attributes { get; set; }
        }
    }
}
