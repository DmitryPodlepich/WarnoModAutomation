using WarnoModeAutomation.Logic;

namespace nUnitTests
{
    public class BalanceTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [TestCase("Ammo_SAM_MIM72G_TBA", 16_980, 7995)]
        [TestCase("Ammo_SAM_MIM72G_HA", 16_980, 9976)]
        [TestCase("Ammo_AA_AIM120A_AMRAAM", 452_800, 22003)]
        [TestCase("Ammo_AGM_BGM71C_ITOW_x8", 10_471, 7429)]
        [TestCase("Ammo_AGM_AGM65D_Maverick", 62_260, 8915)]
        public void NerfDistanceTest(string ammoName, float newDistance, float originalDistance)
        {
            var actualResult = ModManager.NerfDistanceV2(newDistance, originalDistance);

            Assert.That(actualResult, Is.GreaterThan(originalDistance));
            Assert.That(actualResult,  Is.LessThanOrEqualTo(newDistance));
        }
    }
}