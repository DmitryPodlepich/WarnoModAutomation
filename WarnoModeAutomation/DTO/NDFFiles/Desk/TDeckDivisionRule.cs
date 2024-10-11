using NDFSerialization.Models;
using NDFSerialization.NDFDataTypes;

namespace WarnoModeAutomation.DTO.NDFFiles.Desk
{
    public class TDeckDivisionRule : Descriptor
    {
        public override Type Type => typeof(TDeckDivisionRule);

        public NDFVector UnitRuleList { get; set; }
    }
}
