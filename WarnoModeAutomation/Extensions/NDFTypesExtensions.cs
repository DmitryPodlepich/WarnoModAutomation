using WarnoModeAutomation.DTO.NDFFiles;

namespace WarnoModeAutomation.Extensions
{
    public static class NDFTypesExtensions
    {
        private static readonly string[] _sovMotherCountries = ["SOV", "DDR"];

        public static bool IsSovUnit(this TTypeUnitModuleDescriptor typeUnitModuleDescriptor)
        {
            return _sovMotherCountries.Contains(typeUnitModuleDescriptor.MotherCountry);
        }
    }
}
