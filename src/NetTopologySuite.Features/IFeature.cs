using System.Collections.Generic;

using NetTopologySuite.Geometries;

namespace NetTopologySuite.Features
{
    /// <summary>
    /// Contains a <see cref="Geometries.Geometry"/> and a description of its metadata.
    /// </summary>
    public interface IFeature
    {
        /// <summary>
        /// Gets the <see cref="Geometries.Geometry"/> of this feature.
        /// </summary>
        Geometry Geometry { get; }

        /// <summary>
        /// Gets an <see cref="Envelope"/> that describes the bounds of this feature.
        /// </summary>
        Envelope BoundingBox { get; }

        /// <summary>
        /// Gets a representation of this feature's metadata, tagged by user-defined strings.
        /// </summary>
        IReadOnlyDictionary<string, object> Attributes { get; }
    }
}
