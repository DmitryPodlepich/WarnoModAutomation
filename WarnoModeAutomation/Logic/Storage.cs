using Microsoft.Extensions.Configuration;
using WarnoModeAutomation.DTO;
using WarnoModeAutomation.Logic.Helpers;

namespace WarnoModeAutomation.Logic
{
    internal static class Storage
    {
        public static ModeSettingsDTO ModeSettings { get; set; } = new ModeSettingsDTO();
        public static StatusDTO Status { get; set; } = new StatusDTO();

        static Storage()
        {
            var storageConfiguration = ConfigurationHelper.Config.GetSection("Storage").Get<ModeSettingsDTO>();

            ModeSettings.ModsDirectory = storageConfiguration.ModsDirectory;
            ModeSettings.ModName = storageConfiguration.ModName;
        }
    }
}
