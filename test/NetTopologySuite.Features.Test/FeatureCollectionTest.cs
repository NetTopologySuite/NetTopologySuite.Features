﻿using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

using NetTopologySuite.Geometries;

using NUnit.Framework;

namespace NetTopologySuite.Features.Test
{
    /// <summary>
    /// A test fixture for the <see cref="FeatureCollection"/> class
    /// </summary>
    public sealed class FeatureCollectionTest
    {
        private bool _savedComputeWhenMissing;

        [SetUp]
        public void SetUp() => _savedComputeWhenMissing = Feature.ComputeBoundingBoxWhenItIsMissing;

        [TearDown]
        public void TearDown() => Feature.ComputeBoundingBoxWhenItIsMissing = _savedComputeWhenMissing;

        [Test]
        public void SimpleTest()
        {
            Feature[] features = { CreateFeature(0), CreateFeature(1), CreateFeature(2), CreateFeature(3) };

            var featureCollection = new FeatureCollection();
            foreach (var f in features)
            {
                featureCollection.Add(f);
            }

            Assert.That(featureCollection, Is.EqualTo(features));
        }

        [Test]
        public void BoundingBoxTest()
        {
                var featureCollection = new FeatureCollection();
                for (int i = 0; i < 4; i++)
                {
                    featureCollection.Add(CreateFeature(i));
                }

                Feature.ComputeBoundingBoxWhenItIsMissing = false;
                Assert.That(featureCollection.BoundingBox, Is.Null);

                Feature.ComputeBoundingBoxWhenItIsMissing = true;
                Assert.That(featureCollection.BoundingBox, Is.EqualTo(new Envelope(0, 3, 0, 3)));

                Feature.ComputeBoundingBoxWhenItIsMissing = false;
                Assert.That(featureCollection.BoundingBox, Is.EqualTo(new Envelope(0, 3, 0, 3)));
        }

        [Test]
        public void SerializationTest()
        {
            var featureCollection = new FeatureCollection();
            for (int i = 0; i < 4; i++)
            {
                featureCollection.Add(CreateFeature(i));
            }

            featureCollection.BoundingBox = new Envelope(2, 4, 8, 16);

            FeatureCollection deserialized;
            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, featureCollection);

                ms.Position = 0;
                deserialized = (FeatureCollection)formatter.Deserialize(ms);
            }

            // ensure that nulls we wrote out stay null when reading them in.
            Feature.ComputeBoundingBoxWhenItIsMissing = false;

            for (int i = 0; i < 4; i++)
            {
                Assert.That(deserialized[i].Geometry, Is.EqualTo(featureCollection[i].Geometry));
                Assert.That(deserialized[i].Attributes, Is.EqualTo(featureCollection[i].Attributes));
                Assert.That(deserialized[i].BoundingBox, Is.EqualTo(featureCollection[i].BoundingBox));
            }

            Assert.That(deserialized.BoundingBox, Is.EqualTo(new Envelope(2, 4, 8, 16)));
        }

        private static Feature CreateFeature(int i)
        {
            var geom = GeometryFactory.Default.CreatePoint(new Coordinate(i, i));
            var attributes = new[] { KeyValuePair.Create($"test.{i}", (object)i) };
            var bbox = i % 2 == 0 ? geom.EnvelopeInternal : null;
            return new Feature(geom, attributes) { BoundingBox = bbox };
        }
    }
}