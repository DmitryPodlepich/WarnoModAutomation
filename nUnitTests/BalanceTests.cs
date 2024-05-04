using Moq;
using WarnoModeAutomation.Logic.Services.Impl;
using WarnoModeAutomation.Logic.Services.Interfaces;

namespace nUnitTests
{
    public class BalanceTests
    {
        Mock<WarnoModificationService> _modificationService;

        [SetUp]
        public void Setup()
        {
            _modificationService = new Mock<WarnoModificationService>();
        }

        [TestCase(16_980, 7995)] //Ammo_SAM_MIM72G_TBA
        [TestCase(16_980, 9976)] //"Ammo_SAM_MIM72G_HA"
        [TestCase(452_800, 22003)] //"Ammo_AA_AIM120A_AMRAAM"
        [TestCase(10_471, 7429)] //"Ammo_AGM_BGM71C_ITOW_x8"
        [TestCase(62_260, 8915)] //"Ammo_AGM_AGM65D_Maverick"
        public void NerfDistanceTest(float newDistance, float originalDistance)
        {
            var actualResult = _modificationService.Object.NerfDistance(newDistance, originalDistance);

            Assert.That(actualResult, Is.GreaterThan(originalDistance));
            Assert.That(actualResult,  Is.LessThanOrEqualTo(newDistance));
        }
    }
}