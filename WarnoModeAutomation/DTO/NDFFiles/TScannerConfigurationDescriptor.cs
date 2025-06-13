using NDFSerialization.Models;
using NDFSerialization.NDFDataTypes;
using NDFSerialization.NDFDataTypes.Primitive;

namespace WarnoModeAutomation.DTO.NDFFiles
{
    public class TScannerConfigurationDescriptor : Descriptor
    {
        public override Type Type => typeof(TScannerConfigurationDescriptor);

        public DistanceMetre PorteeVisionFOWGRU { get; set; }

        [NDFMAP]
        public Dictionary<string, DistanceMetre> SpecializedDetections { get; set; }

        [NDFMAP]
        public Dictionary<string, DistanceMetre> VisionRangesGRU { get; set; }

        [NDFMAP]
        public Dictionary<string, DistanceMetre> OpticalStrengths { get; set; }
    }
}
