using System.Diagnostics;
using WarnoModeAutomation.Constants;
using WarnoModeAutomation.DTO.NDFFiles;

namespace WarnoModeAutomation.Extensions
{
    public static class NDFTypesExtensions
    {
        private static readonly string[] _sovMotherCountries = ["SOV", "DDR", "POL"];

        public static bool IsSovUnit(this TTypeUnitModuleDescriptor typeUnitModuleDescriptor)
        {
            Debug.WriteLine(typeUnitModuleDescriptor.MotherCountry);
            return _sovMotherCountries.Contains(typeUnitModuleDescriptor.MotherCountry);
        }

        public static bool IsArtileryUnit(this TTagsModuleDescriptor tagsModuleDescriptor)
        {
            return tagsModuleDescriptor.TagSet.Contains(WarnoConstants.ArtillerieTag);
        }
    }
}
