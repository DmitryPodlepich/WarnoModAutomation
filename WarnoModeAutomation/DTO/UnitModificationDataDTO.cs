using NDFSerialization.NDFDataTypes.Primitive;
using WarnoModeAutomation.DTO.NDFFiles.Ammunition;

namespace WarnoModeAutomation.DTO
{
    public record UnitModificationDataDto(
        DistanceMetre DistanceMetre,
        TAmmunitionDescriptor Ammunition,
        int OriginalResourceCommandPoints,
        float RealFireRangeDistance,
        bool NerfRequired)
    {
        public DistanceMetre DistanceMetre { get; set; } = DistanceMetre;
        public float RealFireRangeDistance { get; set; } = RealFireRangeDistance;
    }
}
