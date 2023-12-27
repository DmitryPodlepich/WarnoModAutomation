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

        public static string GfxPath => Path.Combine(Storage.ModeSettings.ModsDirectory, Storage.ModeSettings.ModName, "GameData", "Generated", "Gameplay", "Gfx");

        public static NDFFilePathInfo[] NDFFilesPaths =>
        [
            new(WarnoConstants.BuildingDescriptorsFileName, Path.Combine(GfxPath, WarnoConstants.BuildingDescriptorsFileName)),
            new(WarnoConstants.UniteDescriptorFileName, Path.Combine(GfxPath, WarnoConstants.UniteDescriptorFileName)),
            new(WarnoConstants.WeaponDescriptorDescriptorsFileName, Path.Combine(GfxPath, WarnoConstants.WeaponDescriptorDescriptorsFileName)),
            new(WarnoConstants.AmmunitionDescriptorsFileName, Path.Combine(GfxPath, WarnoConstants.AmmunitionDescriptorsFileName)),
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
            catch(IOException ex) 
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
