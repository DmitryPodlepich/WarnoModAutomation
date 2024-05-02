using WarnoModeAutomation.DTO;

namespace WarnoModeAutomation.Logic
{
    internal static class Storage
    {
        public static ModeSettingsDTO ModeSettings { get; set; } = new ModeSettingsDTO();
        public static StatusDTO Status { get; set; } = new StatusDTO();

        static Storage()
        {
            ModeSettings.ModsDirectory = @"C:\Program Files (x86)\Steam\steamapps\common\WARNO\Mods";
            ModeSettings.ModName = "TestAutoMode";
        }
    }
}
