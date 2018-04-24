using GeoAPI;

namespace NetTopologySuite.Test
{
    [NUnit.Framework.SetUpFixture]
    public class FeaturesTestSetup
    {
        [NUnit.Framework.OneTimeSetUp]
        public void OneTimeSetUp()
        {
            NetTopologySuiteBootstrapper.Bootstrap();
        }
    }
}