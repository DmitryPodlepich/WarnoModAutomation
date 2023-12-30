using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using WarnoModeAutomation.Constants;
using WarnoModeAutomation.DTO;

namespace WarnoModeAutomation.Logic
{
    public static class FileManager
    {
        public static string SystemPath => Path.GetPathRoot(Environment.SystemDirectory);
        public static string SavedGamesEugenSystemsModPath => Path.Combine(SystemPath, "Users", CurrentUserName, "Saved Games", "EugenSystems", "WARNO", "mod");
        public static string GeneratedGfxPath => Path.Combine(Storage.ModeSettings.ModsDirectory, Storage.ModeSettings.ModName, "GameData", "Generated", "Gameplay", "Gfx");
        public static string GamePlayGfxPath => Path.Combine(Storage.ModeSettings.ModsDirectory, Storage.ModeSettings.ModName, "GameData", "Gameplay", "Gfx");
        public static string DepictionResourcesPath => Path.Combine(GamePlayGfxPath, "DepictionResources");
        public static NDFFilePathInfo[] NDFFilesPaths =>
        [
            new(WarnoConstants.BuildingDescriptorsFileName, Path.Combine(GeneratedGfxPath, WarnoConstants.BuildingDescriptorsFileName)),
            new(WarnoConstants.UniteDescriptorFileName, Path.Combine(GeneratedGfxPath, WarnoConstants.UniteDescriptorFileName)),
            new(WarnoConstants.WeaponDescriptorDescriptorsFileName, Path.Combine(GeneratedGfxPath, WarnoConstants.WeaponDescriptorDescriptorsFileName)),
            new(WarnoConstants.AmmunitionDescriptorsFileName, Path.Combine(GeneratedGfxPath, WarnoConstants.AmmunitionDescriptorsFileName)),
        ];

        private static string CurrentUserName => WindowsIdentity.GetCurrent().Name;

        public static bool IsModExist()
        {
            var modPath = Path.Combine(Storage.ModeSettings.ModsDirectory, Storage.ModeSettings.ModName);

            return Directory.Exists(modPath);
        }

        public static bool TryDeleteDirectoryWithFiles(string directoryPath, out string error)
        {
            error = string.Empty;

            try
            {
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

        public static FileInfo TryGetFileWithFullSearch(string directoryPath, string fileName)
        {
            string[] files = Directory.GetFiles(directoryPath, fileName, SearchOption.AllDirectories);

            if (files.Length > 0)
            { 
                return new FileInfo(files[0]);
            }

            return null;
        }
    }
}
