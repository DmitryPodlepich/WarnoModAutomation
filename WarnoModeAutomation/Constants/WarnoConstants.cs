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
        public const string AmunitionNameSMOKEMarker = "SMOKE";
        public const string InfanterieTag = "Infanterie";
        public const string Canon_AA_StandardTag = "Canon_AA_Standard";
        public const string Canon_AATag = "Canon_AA";
        public const string AirTag = "Air";

        public const string Weapon_Cursor_MachineGun = "Weapon_Cursor_MachineGun";

        public static readonly string[] TagsCombinationWithNerf = [Canon_AA_StandardTag, Canon_AATag, AirTag];

        public const string ArtillerieTag = "Artillerie";
        public const string ArtileryWeaponCursorType = "Weapon_Cursor_CorrectedArtillery";
        public const int NatoCommonAccuracityBonusPercentage = 15;

        public const int SovMinOpticalStrength = 20;
        public const int NatoMinOpticalStrength = 40;

        public const int SovGroundVisionPercentage = 15;
        public const int NatoGroundVisionPercentage = 10;

        public const int SovTBAVisionPercentage = 30;
        public const int NatoTBAVisionPercentage = 20;

        public const int SovHAVisionPercentage = 50;
        public const int NatoHAVisionPercentage = 30;

        public const int TBAAndHADetectionPercentage = 20;
        public const int GroudDetectionPercentage = 10;

        public const int ArtileryDamagePercentage = 50;
        public const int NatoArtileryAccuracityBonusPercentage = 40;
    }
}
