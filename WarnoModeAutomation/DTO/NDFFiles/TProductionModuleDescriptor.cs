using NDFSerialization.Models;

namespace WarnoModeAutomation.DTO.NDFFiles
{
    public class TProductionModuleDescriptor : Descriptor
    {
        public override Type Type => typeof(TProductionModuleDescriptor);

        public int ProductionTime { get; set; }

        public Dictionary<string, int> ProductionRessourcesNeeded { get; set; } = [];
    }
}