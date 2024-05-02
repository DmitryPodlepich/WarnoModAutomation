using JsonDatabase.DTO;
using WarnoModeAutomation.Logic.Services.Interfaces;

namespace WarnoModeAutomation.Logic.Services.Impl
{
    public class SettingsManagerService : ISettingsManagerService
    {
        public async Task<SettingsDTO> LoadSettingsAsync()
        {
            return await JsonDatabase.JsonDatabase.LoadSettingsAsync();
        }

        public async Task SaveSettingsAsync(SettingsDTO settingsDTO) 
        {
            await JsonDatabase.JsonDatabase.SaveSettingsasync(settingsDTO);
        }
    }
}
