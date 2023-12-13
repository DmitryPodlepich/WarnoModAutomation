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
    }
}
