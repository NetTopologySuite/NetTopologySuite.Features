using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

using NetTopologySuite.Geometries;

namespace NetTopologySuite.Features
{
    ///<summary>
    /// Represents a feature collection.
    ///</summary>
    [Serializable]
    public sealed class FeatureCollection : Collection<IFeature>, ISerializable
    {
        /// <summary>
        /// The bounding box of this <see cref="FeatureCollection"/>
        /// </summary>
        private Envelope _boundingBox;

        /// <summary>
        /// Initializes a new instance of the <see cref="FeatureCollection"/> class.
        /// </summary>
        public FeatureCollection()
            : base(new List<IFeature>())
        {
        }

        private FeatureCollection(SerializationInfo info, StreamingContext context)
            : base((List<IFeature>)info.GetValue("features", typeof(List<IFeature>)))
        {
            _boundingBox = info.GetBoundingBox();
        }

        /// <summary>
        /// Gets or sets the (optional) <see href="http://geojson.org/geojson-spec.html#geojson-objects"> Bounding box (<c>bbox</c>) Object</see>.
        /// </summary>
        /// <value>
        /// A <see cref="Envelope"/> describing the bounding box or <see langword="null"/>.
        /// </value>
        public Envelope BoundingBox
        {
            get
            {
                if (_boundingBox is null)
                {
                    _boundingBox = ComputeBoundingBox();
                }

                return _boundingBox?.Copy();
            }

            set => _boundingBox = value;
        }

        /// <summary>
        /// Function to compute the bounding box (when it isn't set)
        /// </summary>
        /// <returns>A bounding box for this <see cref="FeatureCollection"/></returns>
        private Envelope ComputeBoundingBox()
        {
            if (!Feature.ComputeBoundingBoxWhenItIsMissing)
            {
                return null;
            }

            var res = new Envelope();
            foreach (var feature in (List<IFeature>)Items)
            {
                if (feature is null)
                {
                    continue;
                }

                if (!(feature.BoundingBox is null))
                {
                    res.ExpandToInclude(feature.BoundingBox);
                }
                else if (!(feature.Geometry is null))
                {
                    res.ExpandToInclude(feature.Geometry.EnvelopeInternal);
                }
            }

            return res;
        }

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddBoundingBox(_boundingBox);
            info.AddValue("features", Items);
        }
    }
}
