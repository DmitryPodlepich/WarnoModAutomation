using NDFSerialization.Models;

namespace WarnoModeAutomation.DTO.NDFFiles
{
    public class TSupplyDescriptor : Descriptor
    {
        public override Type Type => typeof(TSupplyDescriptor);

        public float FuelSupplyBySecond { get; set; }
        public float FuelSupplyCostBySecond { get; set; }

        public float HealthSupplyBySecond { get; set; }
        public float HealthSupplyCostBySecond { get;set; }

        public float SupplySupplyBySecond { get; set; }
        public float SupplySupplyCostBySecond { get; set; }

        public float AmmunitionSupplyBySecond { get; set; }

        public float CriticsSupplyBySecond { get; set; }
        public float CriticsSupplyCostBySecond { get; set; }
    }
}
