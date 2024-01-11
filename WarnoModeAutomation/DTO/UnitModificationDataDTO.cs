using NDFSerialization.NDFDataTypes.Primitive;
using WarnoModeAutomation.DTO.NDFFiles.Ammunition;

namespace WarnoModeAutomation.DTO
{
    public record UnitModificationDataDTO(
        DistanceMetre DistanceMetre,
        TAmmunitionDescriptor Ammunition,
        int OriginalResourceCommandPoints,
        float RealFireRangeDistance,
        bool NerfRequired)
    {
        public DistanceMetre DistanceMetre = DistanceMetre;
        public float RealFireRangeDistance = RealFireRangeDistance;
    }
}
