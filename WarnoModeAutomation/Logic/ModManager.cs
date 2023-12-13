using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarnoModeAutomation.Logic
{
    public static class ModManager
    {
        private const string _createNewModBatFileName = "CreateNewMod.bat";

        public static async Task<bool> CreateModAsync()
        {
            var batFullPath = Path.Combine(Storage.ModeSettings.ModeDirectory, _createNewModBatFileName);

            if (!File.Exists(batFullPath))
                return false;

            var creatingProcess = Process.Start(batFullPath, Storage.ModeSettings.ModeName);

            ProcessStartInfo processStartInfo = new ProcessStartInfo(batFullPath, Storage.ModeSettings.ModeName);

            Process process = new Process();
            process.StartInfo = processStartInfo;
            if (!process.Start())
            {
                // That didn't work
            }

            await process.WaitForExitAsync();

            return true;
        }
    }
}
