using JsonDatabase.DTO;

namespace WarnoModeAutomation.Logic.Services.Interfaces
{
    public interface ISettingsManagerService
    {
        Task<SettingsDTO> LoadSettingsAsync();
        Task SaveSettingsAsync(SettingsDTO settingsDTO);
    }
}
