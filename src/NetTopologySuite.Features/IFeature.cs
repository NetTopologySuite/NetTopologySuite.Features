using NetTopologySuite.Geometries;

namespace NetTopologySuite.Features
{
    /// <summary>
    /// Contains a <see cref="Geometries.Geometry"/> and a description of its metadata.
    /// </summary>
    public interface IFeature
    {
        /// <summary>
        /// Gets or sets the <see cref="Geometries.Geometry"/> of this feature.
        /// </summary>
        Geometry Geometry { get; set; }

        /// <summary>
        /// Gets or sets an <see cref="Envelope"/> that describes the bounds of this feature.
        /// </summary>
        Envelope BoundingBox { get; set; }

        /// <summary>
        /// Gets or sets a representation of this feature's metadata, tagged by user-defined strings.
        /// </summary>
        IAttributesTable Attributes { get; set; }
    }
}
