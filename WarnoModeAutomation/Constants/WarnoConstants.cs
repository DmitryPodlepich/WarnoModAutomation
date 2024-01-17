namespace WarnoModeAutomation.Constants
{
    public static class WarnoConstants
    {
        public const string ConfigFileName = "Config.ini";

        public const string BuildingDescriptorsFileName = "BuildingDescriptors.ndf";
        public const string UniteDescriptorFileName = "UniteDescriptor.ndf";
        public const string WeaponDescriptorDescriptorsFileName = "WeaponDescriptor.ndf";
        public const string AmmunitionDescriptorsFileName = "Ammunition.ndf";

        //ToDo: move to configuration
        public const string ArtillerieTag = "Artillerie";
        public const string ArtileryWeaponCursorType = "Weapon_Cursor_CorrectedArtillery";
        public const int NatoCommonAccuracityBonusPercentage = 20;

        public const int SovMinOpticalStrength = 20;
        public const int NatoMinOpticalStrength = 40;

        public const int SovGroundisionPercentage = 15;
        public const int NatoGroundVisionPercentage = 10;

        public const int SovTBAVisionPercentage = 30;
        public const int NatoTBAVisionPercentage = 20;

        public const int SovHAVisionPercentage = 50;
        public const int NatoHAVisionPercentage = 30;

        public const int TBAAndHADetectionPercentage = 20;
        public const int GroudDetectionPercentage = 10;

        public const int ArtileryDamagePercentage = 50;
        public const int NatoArtileryAccuracityBonusPercentage = 50;
    }
}
