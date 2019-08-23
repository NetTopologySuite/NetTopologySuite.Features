using System;
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
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Feature"/> class.
        /// </summary>
        /// <param name="geometry">The geometry</param>
        /// <param name="attributes">The attributes</param>
        public Feature(Geometry geometry, IAttributesTable attributes)
        {
            Geometry = geometry;
            Attributes = attributes;
        }

        private Feature(SerializationInfo info, StreamingContext context)
        {
            _boundingBox = info.GetBoundingBox();
            Attributes = (IAttributesTable)info.GetValue("attributes", typeof(IAttributesTable));
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
        public IAttributesTable Attributes { get; set; }

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
