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

        private IDictionary<string, object> _attributes;

        /// <summary>
        /// Initializes a new instance of the <see cref="Feature"/> class.
        /// </summary>
        public Feature()
        {
            _attributes = new Dictionary<string, object>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Feature"/> class.
        /// </summary>
        /// <param name="geometry">The geometry</param>
        /// <param name="attributes">The attributes</param>
        public Feature(Geometry geometry, IEnumerable<KeyValuePair<string, object>> attributes)
        {
            Geometry = geometry;
            _attributes = new Dictionary<string, object>();
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
            _attributes = (IDictionary<string, object>)info.GetValue("attributes", typeof(IDictionary<string, object>));
            Geometry = (Geometry)info.GetValue("geometry", typeof(Geometry));
        }

        /// <summary>
        /// Gets or sets a value indicating how bounding box on <see cref="Feature"/> should be handled
        /// </summary>
        /// <remarks>Default is <value>false</value></remarks>
        public static bool ComputeBoundingBoxWhenItIsMissing { get; set; }

        /// <inheritdoc />
        public Geometry Geometry { get; set; }

        /// <inheritdoc />
        public IDictionary<string, object> Attributes
        {
            get => _attributes;
            set => _attributes = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <inheritdoc />
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

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddBoundingBox(_boundingBox);
            info.AddValue("attributes", Attributes);
            info.AddValue("geometry", Geometry);
        }
    }
}
