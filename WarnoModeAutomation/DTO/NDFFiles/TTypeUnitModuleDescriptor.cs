using NDFSerialization.Models;

namespace WarnoModeAutomation.DTO.NDFFiles
{
    public class TTypeUnitModuleDescriptor : Descriptor
    {
        public string AcknowUnitType { get; set; }
        public string MotherCountry { get; set; }
        public override Type Type => typeof(TTypeUnitModuleDescriptor);
    }
}
