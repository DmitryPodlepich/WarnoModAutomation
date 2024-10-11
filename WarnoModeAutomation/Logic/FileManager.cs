using Microsoft.Extensions.Configuration;
using WarnoModeAutomation.Constants;
using WarnoModeAutomation.DTO;
using WarnoModeAutomation.DTO.Configuration;
using WarnoModeAutomation.Logic.Helpers;

namespace WarnoModeAutomation.Logic
{
    internal static class FileManager
    {
        public static string WindowsSystemPath => Path.GetPathRoot(Environment.SystemDirectory);
        public static string SavedGamesEugenSystemsModPath => Path.Combine(WindowsSystemPath, Configuration.Users, CurrentUserName, Configuration.SavedGames, Configuration.EugenSystems, Configuration.WARNO, Configuration.Mod);
        public static string GeneratedGfxPath => Path.Combine(Storage.ModeSettings.ModsDirectory, Storage.ModeSettings.ModName, Configuration.GameData, Configuration.Generated, Configuration.Gameplay, Configuration.Gfx);
        public static string GamePlayGfxPath => Path.Combine(Storage.ModeSettings.ModsDirectory, Storage.ModeSettings.ModName, Configuration.GameData, Configuration.Gameplay, Configuration.Gfx);
        public static string ConstantesPath => Path.Combine(Storage.ModeSettings.ModsDirectory, Storage.ModeSettings.ModName, Configuration.GameData, Configuration.Gameplay, Configuration.Constantes);
        public static string DeskPath => Path.Combine(Storage.ModeSettings.ModsDirectory, Storage.ModeSettings.ModName, Configuration.GameData, Configuration.Generated, Configuration.Gameplay, Configuration.Decks);

        public static NDFFilePathInfo[] NDFFilesPaths =>
        [
            new(WarnoConstants.BuildingDescriptorsFileName, Path.Combine(GeneratedGfxPath, WarnoConstants.BuildingDescriptorsFileName)),
            new(WarnoConstants.UniteDescriptorFileName, Path.Combine(GeneratedGfxPath, WarnoConstants.UniteDescriptorFileName)),
            new(WarnoConstants.WeaponDescriptorDescriptorsFileName, Path.Combine(GeneratedGfxPath, WarnoConstants.WeaponDescriptorDescriptorsFileName)),
            new(WarnoConstants.AmmunitionDescriptorsFileName, Path.Combine(GeneratedGfxPath, WarnoConstants.AmmunitionDescriptorsFileName)),
            new(WarnoConstants.AmmunitionMissilesDescriptorsFileName, Path.Combine(GeneratedGfxPath, WarnoConstants.AmmunitionMissilesDescriptorsFileName)),
            new(WarnoConstants.RavitaillementFileName, Path.Combine(ConstantesPath, WarnoConstants.RavitaillementFileName)),
            new(WarnoConstants.DivisionsFileName, Path.Combine(DeskPath, WarnoConstants.DivisionsFileName)),
            new(WarnoConstants.DivisionRulesFileName, Path.Combine(DeskPath, WarnoConstants.DivisionRulesFileName)),
        ];

        private static ConfigurationDTO Configuration => ConfigurationHelper.Config.GetSection("FileManager").Get<ConfigurationDTO>();

        private static string CurrentUserName => Environment.UserName;

        public static bool IsModExist()
        {
            var modPath = Path.Combine(Storage.ModeSettings.ModsDirectory, Storage.ModeSettings.ModName);

            return Directory.Exists(modPath);
        }

        private static void SetAttributesNormal(DirectoryInfo dir)
        {
            foreach (var subDir in dir.GetDirectories())
                SetAttributesNormal(subDir);
            foreach (var file in dir.GetFiles())
                file.Attributes = FileAttributes.Normal;
        }

        public static bool TryDeleteDirectoryWithFiles(string directoryPath, out string error)
        {
            error = string.Empty;

            try
            {
                SetAttributesNormal(new DirectoryInfo(directoryPath));
                Directory.Delete(directoryPath, true);
            }
            catch (IOException ex)
            {
                error = ex.Message;
                return false;
            }

            return true;
        }

        public static bool TryDeleteFile(string filePath, out string error)
        {
            error = string.Empty;

            try
            {
                File.Delete(filePath);
            }
            catch (IOException ex)
            {
                error = ex.Message;
                return false;
            }

            return true;
        }
    }
}
