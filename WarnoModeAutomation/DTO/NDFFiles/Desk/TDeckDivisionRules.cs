using NDFSerialization.Models;
using NDFSerialization.NDFDataTypes;

namespace WarnoModeAutomation.DTO.NDFFiles.Desk
{
    public class TDeckDivisionRules : Descriptor
    {
        public override Type Type => typeof(TDeckDivisionRules);

        public override Dictionary<string, string> PropertiesToAnonymousNestedDescriptiors => new()
        {
            { nameof(DivisionRules), nameof(AnonymousDivisionRule) }
        };

        public NDFVector DivisionRules { get; set; }
    }
}
