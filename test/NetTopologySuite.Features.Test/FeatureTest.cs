using System.Collections.Generic;

using NetTopologySuite.Geometries;

using NUnit.Framework;

namespace NetTopologySuite.Features.Test
{
    /// <summary>
    /// A test fixture for the <see cref="Feature"/> class
    /// </summary>
    public sealed class FeatureTest
    {
        private bool _savedComputeWhenMissing;

        [SetUp]
        public void SetUp() => _savedComputeWhenMissing = Feature.ComputeBoundingBoxWhenItIsMissing;

        [TearDown]
        public void TearDown() => Feature.ComputeBoundingBoxWhenItIsMissing = _savedComputeWhenMissing;

        [Test]
        public void SimpleTest()
        {
            var geom = GeometryFactory.Default.CreatePoint(new Coordinate(1, 1));
            var attributes = new[] { KeyValuePair.Create("test.1", new object()) };
            var feature = new Feature(geom, attributes);

            Assert.That(feature.Geometry, Is.EqualTo(geom));
            Assert.That(feature.Attributes, Is.EqualTo(attributes));
        }

        [Test]
        public void BoundingBoxTest()
        {
            var c = new Coordinate(1, 1);
            var feature = new Feature
            {
                Geometry = GeometryFactory.Default.CreatePoint(c),
            };

            Feature.ComputeBoundingBoxWhenItIsMissing = false;
            Assert.That(feature.BoundingBox, Is.Null);

            Feature.ComputeBoundingBoxWhenItIsMissing = true;
            Assert.That(feature.BoundingBox, Is.EqualTo(new Envelope(c)));

            Feature.ComputeBoundingBoxWhenItIsMissing = false;
            Assert.That(feature.BoundingBox, Is.EqualTo(new Envelope(c)));
        }
    }
}
