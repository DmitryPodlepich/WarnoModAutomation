using NDFSerialization.Models;
using NDFSerialization.NDFDataTypes;

namespace WarnoModeAutomation.DTO.NDFFiles
{
    public class TProductionModuleDescriptor : Descriptor
    {
        public override Type Type => typeof(TProductionModuleDescriptor);

        public int ProductionTime { get; set; }

        [NDFMAP]
        public Dictionary<string, int> ProductionRessourcesNeeded { get; set; }
    }
}