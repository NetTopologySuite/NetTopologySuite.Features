using GeoAPI;

namespace NetTopologySuite.Test
{
    /// <summary>
    /// Utility class to make sure that NTS is set up and registered properly
    /// </summary>
    [NUnit.Framework.SetUpFixture]
    public class FeaturesTestSetup
    {
        /// <summary>
        /// Method to wire NTS
        /// </summary>
        [NUnit.Framework.OneTimeSetUp]
        public void OneTimeSetUp()
        {
            GeometryServiceProvider.SetInstanceIfNotAlreadySetDirectly(NtsGeometryServices.Instance);
        }
    }
}
