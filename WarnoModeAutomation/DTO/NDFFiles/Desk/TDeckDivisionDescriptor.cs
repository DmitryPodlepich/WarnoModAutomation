using NDFSerialization.Models;
using NDFSerialization.NDFDataTypes;

namespace WarnoModeAutomation.DTO.NDFFiles.Desk
{
    public class TDeckDivisionDescriptor : Descriptor
    {
        public override Type Type => typeof(TDeckDivisionDescriptor);

        public string CfgName { get; set; }

        public int MaxActivationPoints { get; set; }

        [NDFMAP]
        public Dictionary<string, float> PackList { get; set; }
    }
}
