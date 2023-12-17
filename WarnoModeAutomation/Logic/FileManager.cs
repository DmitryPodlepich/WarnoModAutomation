using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarnoModeAutomation.Constants;
using WarnoModeAutomation.DTO;

namespace WarnoModeAutomation.Logic
{
    public static class FileManager
    {
        public readonly static string GfxPath =
            Path.Combine(Storage.ModeSettings.ModsDirectory, Storage.ModeSettings.ModeName, "GameData", "Generated", "Gameplay", "Gfx");

        public readonly static NDFFileInfo[] NDFFiles =
        [
            new(WarnoConstants.BuildingDescriptorsFileBame, Path.Combine(GfxPath, WarnoConstants.BuildingDescriptorsFileBame))
        ];

        public static bool IsModExist() 
        {
            var modPath = Path.Combine(Storage.ModeSettings.ModsDirectory, Storage.ModeSettings.ModeName);

            return Directory.Exists(modPath);
        }

        public static bool DeleteDirectoryWithFiles(string directoryPath, out string error)
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
    }
}
