namespace NetTopologySuite.Features
{
    /// <summary>
    /// Defines extensions for <see cref="IFeature"/>.
    /// </summary>
    public static class FeatureExtensions
    {
        /// <summary>
        /// Gets an ID value from this <see cref="IFeature"/>, using its <see cref="IUnique.Id"/>
        /// property if it happens to implement that interface (otherwise, searching through its
        /// <see cref="IFeature.Attributes"/> looking for an attribute with a specified name).
        /// </summary>
        /// <param name="feature">
        /// The <see cref="IFeature"/> whose ID to get.
        /// </param>
        /// <param name="idPropertyName">
        /// The name of the attribute to look for in <see cref="IFeature.Attributes"/>, if the
        /// <paramref name="feature"/> is not an instance of <see cref="IUnique"/>.
        /// </param>
        /// <returns>
        /// The ID value, or <see langword="null"/> if this instance has no ID.
        /// </returns>
        public static object GetOptionalId(this IFeature feature, string idPropertyName)
        {
            return feature is IUnique unique
                ? unique.Id
                : feature?.Attributes?.GetOptionalValue(idPropertyName);
        }
    }
}
