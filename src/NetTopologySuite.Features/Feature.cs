using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

using NetTopologySuite.Geometries;

namespace NetTopologySuite.Features
{
    ///<summary>
    /// Standard implementation of <see cref="IFeature"/>.
    ///</summary>
    [Serializable]
    public sealed class Feature : IFeature, ISerializable
    {
        private Envelope _boundingBox;

        /// <summary>
        /// Initializes a new instance of the <see cref="Feature"/> class.
        /// </summary>
        public Feature()
        {
            Attributes = new Dictionary<string, object>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Feature"/> class.
        /// </summary>
        /// <param name="geometry">The geometry</param>
        /// <param name="attributes">The attributes</param>
        public Feature(Geometry geometry, IEnumerable<KeyValuePair<string, object>> attributes)
        {
            Geometry = geometry;
            Attributes = new Dictionary<string, object>();
            if (!(attributes is null))
            {
                foreach (var kvp in attributes)
                {
                    Attributes.Add(kvp.Key, kvp.Value);
                }
            }
        }

        private Feature(SerializationInfo info, StreamingContext context)
        {
            _boundingBox = info.GetBoundingBox();
            Attributes = (Dictionary<string, object>)info.GetValue("attributes", typeof(Dictionary<string, object>));
            Geometry = (Geometry)info.GetValue("geometry", typeof(Geometry));
        }

        /// <summary>
        /// Gets or sets a value indicating how bounding box on <see cref="Feature"/> should be handled
        /// </summary>
        /// <remarks>Default is <value>false</value></remarks>
        public static bool ComputeBoundingBoxWhenItIsMissing { get; set; }

        /// <summary>
        /// Geometry representation of the feature.
        /// </summary>
        public Geometry Geometry { get; set; }

        /// <summary>
        /// Attributes table of the feature.
        /// </summary>
        public Dictionary<string, object> Attributes { get; }

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
                    if (ComputeBoundingBoxWhenItIsMissing)
                    {
                        _boundingBox = Geometry?.EnvelopeInternal;
                    }
                }

                return _boundingBox?.Copy();
            }

            set => _boundingBox = value;
        }

        /// <inheritdoc />
        IReadOnlyDictionary<string, object> IFeature.Attributes => Attributes;

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddBoundingBox(_boundingBox);
            info.AddValue("attributes", Attributes);
            info.AddValue("geometry", Geometry);
        }
    }
}
