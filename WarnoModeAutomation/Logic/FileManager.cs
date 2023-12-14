using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarnoModeAutomation.Logic
{
    public static class FileManager
    {
        public static bool IsModExist() 
        {
            var modPath = Path.Combine(Storage.ModeSettings.ModeDirectory, Storage.ModeSettings.ModeName);

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
