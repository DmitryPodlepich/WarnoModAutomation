using Microsoft.Extensions.Configuration;
using WarnoModeAutomation.DTO;
using WarnoModeAutomation.Logic.Helpers;

namespace WarnoModeAutomation.Logic
{
    internal static class Storage
    {
        public static ModeSettingsDTO ModeSettings => ConfigurationHelper.Config.GetSection("Storage").Get<ModeSettingsDTO>();

        //static Storage()
        //{
            //var storageConfiguration = ConfigurationHelper.Config.GetSection("Storage").Get<ModeSettingsDTO>();

            //ModeSettings.ModsDirectory = storageConfiguration.ModsDirectory;
            //ModeSettings.ModName = storageConfiguration.ModName;
        //}

        //private static ModeSettingsDTO GetModeSettings() 
        //{
            
        //}
    }
}
