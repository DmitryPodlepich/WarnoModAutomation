using NDFSerialization.Models;

namespace WarnoModeAutomation.DTO.NDFFiles
{
    public class TSupplyModuleDescriptor : Descriptor
    {
        public override Type Type => typeof(TSupplyModuleDescriptor);
        public float SupplyCapacity { get; set; }
    }
}
