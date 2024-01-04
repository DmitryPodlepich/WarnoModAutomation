using NDFSerialization.Models;
using NDFSerialization.NDFDataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
