using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Prepared;
using NetTopologySuite.Operation.Overlay;
using System.Collections.Generic;
using System.Linq;
using NetTopologySuite.Operation.Union;

namespace NetTopologySuite.Features.Operations
{
    public static class OverlayExtensions
    {
        public static IEnumerable<Feature> Intersection(this IEnumerable<Feature> features, IEnumerable<Feature> otherFeatures) =>
            features.Select(f => (Feature: f, PreparedGeometry: PreparedGeometryFactory.Prepare(f.Geometry)))
                    .SelectMany(tuple => otherFeatures.Select(other => new FeaturePair(tuple.PreparedGeometry, tuple.Feature, other)))
                    .Where(p => p.Intersects)
                    .Select(p => p.Intersection)
                    .Where(f => !f.Geometry.IsEmpty);

        public static IEnumerable<Feature> Difference(this IEnumerable<Feature> features, IEnumerable<Feature> otherFeatures)
        {
            var fullGeometry = new UnaryUnionOp(otherFeatures.Select(f => f.Geometry)).Union();
            
            return features.Select(f => (Feature: f, PreparedGeometry: PreparedGeometryFactory.Prepare(f.Geometry)))
                           .Select(tuple => tuple.PreparedGeometry.Intersects(fullGeometry)
                                ? new Feature {  Geometry = tuple.Feature.Geometry.Difference(fullGeometry), Attributes = tuple.Feature.Attributes }
                                : tuple.Feature)
                           .Where(f => !f.Geometry.IsEmpty);
        }

        public static IEnumerable<Feature> Union(this IEnumerable<Feature> features, IEnumerable<Feature> otherFeatures) =>
            features.Intersection(otherFeatures)
                    .Concat(features.SymDifference(otherFeatures))
                    .Where(f => !f.Geometry.IsEmpty);

        public static IEnumerable<Feature> SymDifference(this IEnumerable<Feature> features, IEnumerable<Feature> otherFeatures) =>
            features.Difference(otherFeatures).ToList()
                    .Concat(otherFeatures.Difference(features).ToList())
                    .Where(f => !f.Geometry.IsEmpty);

        public static IEnumerable<Feature> Overlay(this IEnumerable<Feature> features, IEnumerable<Feature> otherFeatures, SpatialFunction spatialFunction) =>
            spatialFunction == SpatialFunction.Intersection ? features.Intersection(otherFeatures)
            : spatialFunction == SpatialFunction.Difference ? features.Difference(otherFeatures)
            : spatialFunction == SpatialFunction.Union ? features.Union(otherFeatures)
            : spatialFunction == SpatialFunction.SymDifference ? features.SymDifference(otherFeatures)
            : null;

        public static IEnumerable<Feature> Intersection(this IEnumerable<Feature> features, IEnumerable<Geometry> geometries) =>
            features.Intersection(geometries.Select(g => g.ToFeature()));
        public static IEnumerable<Feature> Difference(this IEnumerable<Feature> features, IEnumerable<Geometry> geometries) =>
            features.Difference(geometries.Select(g => g.ToFeature()));
        public static IEnumerable<Feature> Union(this IEnumerable<Feature> features, IEnumerable<Geometry> geometries) =>
            features.Union(geometries.Select(g => g.ToFeature()));
        public static IEnumerable<Feature> SymDifference(this IEnumerable<Feature> features, IEnumerable<Geometry> geometries) =>
            features.SymDifference(geometries.Select(g => g.ToFeature()));
        public static IEnumerable<Feature> Overlay(this IEnumerable<Feature> features, IEnumerable<Geometry> otherFeatures, SpatialFunction spatialFunction) =>
            features.Overlay(otherFeatures.Select(g => g.ToFeature()), spatialFunction);

        public static IEnumerable<Feature> Intersection(this IEnumerable<Geometry> geometries, IEnumerable<Feature> features) =>
            geometries.Select(g => g.ToFeature()).Intersection(features);
        public static IEnumerable<Feature> Difference(this IEnumerable<Geometry> geometries, IEnumerable<Feature> features) =>
            geometries.Select(g => g.ToFeature()).Difference(features);
        public static IEnumerable<Feature> Union(this IEnumerable<Geometry> geometries, IEnumerable<Feature> features) =>
            geometries.Select(g => g.ToFeature()).Union(features);
        public static IEnumerable<Feature> SymDifference(this IEnumerable<Geometry> geometries, IEnumerable<Feature> features) =>
            geometries.Select(g => g.ToFeature()).SymDifference(features);
        public static IEnumerable<Feature> Overlay(this IEnumerable<Geometry> geometries, IEnumerable<Feature> features, SpatialFunction spatialFunction) =>
            geometries.Select(g => g.ToFeature()).Overlay(features, spatialFunction);

        public static Feature ToFeature(this Geometry geometry, IAttributesTable attributesTable = null) =>
            new Feature(geometry, attributesTable);




        public class FeaturePair
        {
            readonly IPreparedGeometry _preparedFeature1;

            public FeaturePair(IPreparedGeometry preparedFeature1, Feature feature1, Feature feature2)
            {
                _preparedFeature1 = preparedFeature1;
                Feature1 = feature1;
                Feature2 = feature2;
            }

            public bool Intersects => _preparedFeature1.Intersects(Feature2.Geometry);
            public Feature Intersection =>
                new Feature { Geometry = Feature1.Geometry.Intersection(Feature2.Geometry), Attributes = getMergedAttributes() };
            public Feature Union =>
                new Feature { Geometry = Feature1.Geometry.Union(Feature2.Geometry), Attributes = getMergedAttributes() };
            public Feature Difference =>
                new Feature { Geometry = Feature1.Geometry.Difference(Feature2.Geometry), Attributes = Feature1.Attributes };

            public Feature Feature1 { get; }
            public Feature Feature2 { get; }

            IAttributesTable getMergedAttributes()
            {
                var attrs = new AttributesTable();
                if(Feature1.Attributes != null)
                    Feature1.Attributes
                            .GetNames()
                            .ToList()
                            .ForEach(n => attrs.Add(n, Feature1.Attributes[n]));

                if (Feature2.Attributes != null)
                    Feature2.Attributes
                             .GetNames()
                             .Where(n => !attrs.Exists(n))
                             .ToList()
                             .ForEach(n => attrs.Add(n, Feature2.Attributes[n]));
                return attrs;
            }

        }
    }
}
