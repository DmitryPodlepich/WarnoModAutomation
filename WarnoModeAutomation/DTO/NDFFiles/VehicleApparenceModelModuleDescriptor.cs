using NDFSerialization.Models;

namespace WarnoModeAutomation.DTO.NDFFiles
{
    public class VehicleApparenceModelModuleDescriptor : Descriptor
    {
        public string BlackHoleIdentifier { get; set; }
        public override Type Type => typeof(VehicleApparenceModelModuleDescriptor);
    }
}
