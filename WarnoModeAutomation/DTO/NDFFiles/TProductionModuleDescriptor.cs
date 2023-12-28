using NDFSerialization.Models;

namespace WarnoModeAutomation.DTO.NDFFiles
{
    public class TProductionModuleDescriptor : Descriptor
    {
        public override Type Type => typeof(TProductionModuleDescriptor);

        public int ProductionTime { get; set; }

        //ToDo: create attribute [NDFMAP]
        public Dictionary<string, int> ProductionRessourcesNeeded { get; set; } = [];
    }
}