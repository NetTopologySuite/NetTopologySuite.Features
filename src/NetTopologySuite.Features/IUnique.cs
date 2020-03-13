namespace NetTopologySuite.Features
{
    /// <summary>
    /// Interface for things tagged by an identifier that's assumed to be unique.
    /// </summary>
    public interface IUnique
    {
        /// <summary>
        /// Gets the identifier of this object (assumed unique).
        /// </summary>
        object Id { get; }
    }
}
