namespace WarnoModeAutomation.Constants
{
    public static class WarnoConstants
    {
        public const string ConfigFileName = "Config.ini";

        public const string BuildingDescriptorsFileName = "BuildingDescriptors.ndf";
        public const string UniteDescriptorFileName = "UniteDescriptor.ndf";
        public const string WeaponDescriptorDescriptorsFileName = "WeaponDescriptor.ndf";
        public const string AmmunitionDescriptorsFileName = "Ammunition.ndf";
        public const string AmmunitionMissilesDescriptorsFileName = "AmmunitionMissiles.ndf";
        public const string RavitaillementFileName = "Ravitaillement.ndf";

        public const string ResourceCommandPoints = "Resource_CommandPoints";
        public const string AmunitionNameSMOKEMarker = "SMOKE";
        public const string InfanterieTag = "Infanterie";
        public const string Canon_AA_StandardTag = "Canon_AA_Standard";
        public const string Canon_AATag = "Canon_AA";
        public const string AirTag = "Air";

        public const string Weapon_Cursor_MachineGun = "Weapon_Cursor_MachineGun";

        public static readonly string[] TagsCombinationWithNerf = [Canon_AA_StandardTag, Canon_AATag, AirTag];

        public const string ArtillerieTag = "Artillerie";
        public const string ArtileryWeaponCursorType = "Weapon_Cursor_CorrectedArtillery";

        public const int MaxAccuracity = 98;
        public const double NerfDistanceCoefficientDivider = 2.5;
        public const int WarnoMetters = 2830;

        public const double AdditionalPointsCoefficientMultiplier = 1.1;
        public const double AdditionalPointsArtileryCoefficientDivider = 6;
        public const int NatoCommonAccuracityBonusPercentage = 25;
        public const int NatoArtileryAccuracityBonusPercentage = 50;

        public const int SovMinOpticalStrength = 5;
        public const int NatoMinOpticalStrength = 50;

        public const int SovGroundVisionPercentageFromAmunitionDistance = 35;
        public const int NatoGroundVisionPercentageFromAmunitionDistance = 5;

        public const int SovTBAVisionPercentageFromAmunitionDistance = 35;
        public const int NatoTBAVisionPercentageFromAmunitionDistance = 5;

        public const int SovHAVisionPercentageFromAmunitionDistance = 35;
        public const int NatoHAVisionPercentageFromAmunitionDistance = 5;

        public const int SovMinVisionDistance = 10000;
        public const int NatoMinVisionDistance = 14000;

        public const int TBAAndHADetectionPercentageFromVisionDistance = 20;
        public const int GroudDetectionPercentageFromVisionDistance = 10;

        public const int ArtileryDamagePercentage = 40;
    }
}
