namespace JsonDatabase.DTO
{
    public class SettingsDTO(
        string ModsDirectory,
        string ModName,
        int WarnoMetters,
        double NerfDistanceCoefficientDivider,
        double AdditionalPointsCoefficientMultiplier,
        double AdditionalPointsArtileryCoefficientDivider,
        int NatoCommonAccuracityBonusPercentage,
        int NatoArtileryAccuracityBonusPercentage,
        int SovMinOpticalStrength,
        int NatoMinOpticalStrength,
        int SovGroundVisionPercentageFromAmunitionDistance,
        int NatoGroundVisionPercentageFromAmunitionDistance,
        int SovTBAVisionPercentageFromAmunitionDistance,
        int NatoTBAVisionPercentageFromAmunitionDistance,
        int SovHAVisionPercentageFromAmunitionDistance,
        int NatoHAVisionPercentageFromAmunitionDistance,
        int SovMinVisionDistance,
        int NatoMinVisionDistance,
        int TBAAndHADetectionPercentageFromVisionDistance,
        int GroudDetectionPercentageFromVisionDistance,
        int ArtileryDamagePercentage,
        string ConfigFileName,
        string BuildingDescriptorsFileName,
        string UniteDescriptorFileName,
        string WeaponDescriptorDescriptorsFileName,
        string AmmunitionDescriptorsFileName,
        string AmmunitionMissilesDescriptorsFileName,
        string ResourceCommandPoints,
        string AmunitionNameSMOKEMarker,
        string InfanterieTag,
        string Canon_AA_StandardTag,
        string Canon_AATag,
        string AirTag,
        string Weapon_Cursor_MachineGun,
        string ArtillerieTag,
        string ArtileryWeaponCursorType,
        string RavitaillementFileName,
        string DivisionsFileName,
        string DivisionRulesFileName)
    {
        public string ModsDirectory { get; set; } = ModsDirectory;
        public string ModName { get; set; } = ModName;

        #region Balance settings
        public int WarnoMetters { get; set; } = WarnoMetters;
        public double NerfDistanceCoefficientDivider { get; set; } = NerfDistanceCoefficientDivider;
        public double AdditionalPointsCoefficientMultiplier { get; set; } = AdditionalPointsCoefficientMultiplier;
        public double AdditionalPointsArtileryCoefficientDivider { get; set; } = AdditionalPointsArtileryCoefficientDivider;
        public int NatoCommonAccuracityBonusPercentage { get; set; } = NatoCommonAccuracityBonusPercentage;
        public int NatoArtileryAccuracityBonusPercentage { get; set; } = NatoArtileryAccuracityBonusPercentage;
        public int SovMinOpticalStrength { get; set; } = SovMinOpticalStrength;
        public int NatoMinOpticalStrength { get; set; } = NatoMinOpticalStrength;
        public int SovGroundVisionPercentageFromAmunitionDistance { get; set; } = SovGroundVisionPercentageFromAmunitionDistance;
        public int NatoGroundVisionPercentageFromAmunitionDistance { get; set; } = NatoGroundVisionPercentageFromAmunitionDistance;
        public int SovTBAVisionPercentageFromAmunitionDistance { get; set; } = SovTBAVisionPercentageFromAmunitionDistance;
        public int NatoTBAVisionPercentageFromAmunitionDistance { get; set; } = NatoTBAVisionPercentageFromAmunitionDistance;
        public int SovHAVisionPercentageFromAmunitionDistance { get; set; } = SovHAVisionPercentageFromAmunitionDistance;
        public int NatoHAVisionPercentageFromAmunitionDistance { get; set; } = NatoHAVisionPercentageFromAmunitionDistance;
        public int SovMinVisionDistance { get; set; } = SovMinVisionDistance;
        public int NatoMinVisionDistance { get; set; } = NatoMinVisionDistance;
        public int TBAAndHADetectionPercentageFromVisionDistance { get; set; } = TBAAndHADetectionPercentageFromVisionDistance;
        public int GroudDetectionPercentageFromVisionDistance { get; set; } = GroudDetectionPercentageFromVisionDistance;
        public int ArtileryDamagePercentage { get; set; } = ArtileryDamagePercentage;
        #endregion

        #region Serialization settings

        public string ConfigFileName { get; set; } = ConfigFileName;

        public string BuildingDescriptorsFileName { get; set; } = BuildingDescriptorsFileName;
        public string UniteDescriptorFileName { get; set; } = UniteDescriptorFileName;
        public string WeaponDescriptorDescriptorsFileName { get; set; } = WeaponDescriptorDescriptorsFileName;
        public string AmmunitionDescriptorsFileName { get; set; } = AmmunitionDescriptorsFileName;
        public string AmmunitionMissilesDescriptorsFileName { get; set; } = AmmunitionMissilesDescriptorsFileName;

        public string ResourceCommandPoints { get; set; } = ResourceCommandPoints;
        public string AmunitionNameSMOKEMarker { get; set; } = AmunitionNameSMOKEMarker;
        public string InfanterieTag { get; set; } = InfanterieTag;
        public string Canon_AA_StandardTag { get; set; } = Canon_AA_StandardTag;
        public string Canon_AATag { get; set; } = Canon_AATag;
        public string AirTag { get; set; } = AirTag;

        public string Weapon_Cursor_MachineGun { get; set; } = Weapon_Cursor_MachineGun;

        public string[] TagsCombinationWithNerf => [Canon_AA_StandardTag, Canon_AATag, AirTag];

        public string ArtillerieTag { get; set; } = ArtillerieTag;
        public string ArtileryWeaponCursorType { get; set; } = ArtileryWeaponCursorType;

        public string RavitaillementFileName { get; set; } = RavitaillementFileName;

        public string DivisionsFileName { get; set; } = DivisionsFileName;

        public string DivisionRulesFileName { get; set; } = DivisionRulesFileName;

        #endregion
    }
}
