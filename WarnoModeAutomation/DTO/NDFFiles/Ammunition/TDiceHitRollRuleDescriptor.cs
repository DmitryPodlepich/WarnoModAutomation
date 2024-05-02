using NDFSerialization.Models;
using NDFSerialization.NDFDataTypes;

namespace WarnoModeAutomation.DTO.NDFFiles.Ammunition
{
    public class TDiceHitRollRuleDescriptor : Descriptor
    {
        public override Type Type => typeof(TDiceHitRollRuleDescriptor);

        public float BaseCriticModifier { get; set; }
        public float BaseEffectModifier { get; set; }

        [NDFMAP]
        public Dictionary<string, float> BaseHitValueModifiers { get; set; }
    }
}
