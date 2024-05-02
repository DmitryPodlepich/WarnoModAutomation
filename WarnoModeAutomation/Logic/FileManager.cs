using WarnoModeAutomation.Constants;
using WarnoModeAutomation.DTO;

namespace WarnoModeAutomation.Logic
{
    internal static class FileManager
    {
        public static string WindowsSystemPath => Path.GetPathRoot(Environment.SystemDirectory);
        public static string SavedGamesEugenSystemsModPath => Path.Combine(WindowsSystemPath, "Users", CurrentUserName, "Saved Games", "EugenSystems", "WARNO", "mod");
        public static string GeneratedGfxPath => Path.Combine(Storage.ModeSettings.ModsDirectory, Storage.ModeSettings.ModName, "GameData", "Generated", "Gameplay", "Gfx");
        public static string GamePlayGfxPath => Path.Combine(Storage.ModeSettings.ModsDirectory, Storage.ModeSettings.ModName, "GameData", "Gameplay", "Gfx");
        public static string DepictionResourcesPath => Path.Combine(GamePlayGfxPath, "DepictionResources");
        public static NDFFilePathInfo[] NDFFilesPaths =>
        [
            new(WarnoConstants.BuildingDescriptorsFileName, Path.Combine(GeneratedGfxPath, WarnoConstants.BuildingDescriptorsFileName)),
            new(WarnoConstants.UniteDescriptorFileName, Path.Combine(GeneratedGfxPath, WarnoConstants.UniteDescriptorFileName)),
            new(WarnoConstants.WeaponDescriptorDescriptorsFileName, Path.Combine(GeneratedGfxPath, WarnoConstants.WeaponDescriptorDescriptorsFileName)),
            new(WarnoConstants.AmmunitionDescriptorsFileName, Path.Combine(GeneratedGfxPath, WarnoConstants.AmmunitionDescriptorsFileName)),
            new(WarnoConstants.AmmunitionMissilesDescriptorsFileName, Path.Combine(GeneratedGfxPath, WarnoConstants.AmmunitionMissilesDescriptorsFileName)),
        ];

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
