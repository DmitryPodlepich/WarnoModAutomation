﻿using NDFSerialization.Models;
using NDFSerialization.NDFDataTypes.Primitive;

namespace WarnoModeAutomation.DTO.NDFFiles.Ammunition
{
    public class TAmmunitionDescriptor : Descriptor
    {
        public override Type Type => typeof(TAmmunitionDescriptor);
        public string WeaponCursorType { get; set; }
        public float TempsEntreDeuxTirs { get; set; }
        public float TempsEntreDeuxFx { get; set; }
        public DistanceMetre PorteeMaximale { get; set; }
        public DistanceMetre PorteeMinimaleTBA { get; set; }
        public DistanceMetre PorteeMaximaleTBA { get; set; }
        public DistanceMetre PorteeMinimaleHA { get; set; }
        public DistanceMetre PorteeMaximaleHA { get; set; }
        public DistanceMetre PorteeMinimale { get; set; }
        public DistanceMetre AltitudeAPorteeMinimale { get; set; }
        public DistanceMetre AltitudeAPorteeMaximale { get; set; }
        public float AngleDispersion { get; set; }
        public DistanceMetre DispersionAtMaxRange { get; set; }
        public DistanceMetre DispersionAtMinRange { get; set; }
        public bool DispersionWithoutSorting { get; set; }
        public float CorrectedShotAimtimeMultiplier { get; set; }
        public DistanceMetre RadiusSplashPhysicalDamages { get; set; }
        public float PhysicalDamages { get; set; }
        public DistanceMetre RadiusSplashSuppressDamages { get; set; }
        public float SuppressDamages { get; set; }
        public TDiceHitRollRuleDescriptor HitRollRuleDescriptor { get ;set; }
        public float MaxSuccessiveHitCount { get; set; }
        public float TempsDeVisee { get; set; }
        public float TempsEntreDeuxSalves { get; set; }
        public float TempsEntreDeuxSalves_Min { get; set; }
        public float TempsEntreDeuxSalves_Max { get; set; }
        public int NbTirParSalves { get; set; }
        public int SupplyCost { get; set; }
        public int NbSalvosShootOnPosition { get; set; }
        public bool CanShootOnPosition { get; set; }
        public bool CanShootWhileMoving { get; set; }
        public int NbrProjectilesSimultanes { get; set; }
        public int AffichageMunitionParSalve { get; set; }
    }
}
