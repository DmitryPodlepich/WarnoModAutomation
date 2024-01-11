using NDFSerialization.Models;
using NDFSerialization.NDFDataTypes;
using NDFSerialization.NDFDataTypes.Primitive;

namespace WarnoModeAutomation.DTO.NDFFiles
{
    public class TScannerConfigurationDescriptor : Descriptor
    {
        public override Type Type => typeof(TScannerConfigurationDescriptor);

        public DistanceMetre PorteeVisionTBA {get; set; }
        public DistanceMetre PorteeVisionFOW { get; set; }
        public DistanceMetre DetectionTBA { get; set; }
        public DistanceMetre PorteeVision { get; set; }
        public DistanceMetre PorteeIdentification { get; set; }
        public float OpticalStrength { get; set; }
        public float OpticalStrengthAltitude { get; set; }
        public bool UnitDetectStealthUnit { get; set; }

        [NDFMAP]
        public Dictionary<string, DistanceMetre> SpecializedDetections { get; set; }
    }
}
