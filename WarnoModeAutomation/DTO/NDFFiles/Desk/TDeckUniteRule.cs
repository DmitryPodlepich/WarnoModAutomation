using NDFSerialization.Models;

namespace WarnoModeAutomation.DTO.NDFFiles.Desk
{
    public class TDeckUniteRule : Descriptor
    {
        public override Type Type => typeof(TDeckUniteRule);

        public string UnitDescriptor { get; set; }
        public bool AvailableWithoutTransport { get; set; }
        public int NumberOfUnitInPack { get; set; }
        public string NumberOfUnitInPackXPMultiplier { get; set; }
    }
}
