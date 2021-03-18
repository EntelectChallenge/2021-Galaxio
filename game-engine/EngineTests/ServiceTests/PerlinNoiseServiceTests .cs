using Engine.Services;
using NUnit.Framework;

namespace EngineTests.ServiceTests
{
    [TestFixture]
    public class PerlinNoiseServiceTests : TestBase
    {
        private PerlinNoiseService perlinNoiseService;

        [SetUp]
        public new void Setup()
        {
            base.Setup();
            perlinNoiseService = new PerlinNoiseService();
        }

        [Test]
        public void PerlinTest()
        {
            var value1 = perlinNoiseService.Perlin(0, 0);

            Assert.AreEqual(0.5, value1);
        }
    }
}