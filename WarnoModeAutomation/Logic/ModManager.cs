namespace WarnoModeAutomation.Logic
{
    public static class ModManager
    {
        private const string _createNewModBatFileName = "CreateNewMod.bat";
        private const string _generateModBatFileName = "GenerateMod.bat";

        public delegate void Outputter(string data);
        public static event Outputter OnOutput;

        public static async Task<bool> CreateModAsync()
        {
            var batFullPath = Path.Combine(Storage.ModeSettings.ModsDirectory, _createNewModBatFileName);

            if (!File.Exists(batFullPath))
            {
                OnOutput?.Invoke($"{batFullPath} file does not exist!");
                return false;
            }

            using var cmdProvier = new CMDProvider(Storage.ModeSettings.ModsDirectory);

            cmdProvier.OnOutput += OnCMDProviderOutput;

            return await cmdProvier.PerformCMDCommand($"{_createNewModBatFileName} {Storage.ModeSettings.ModeName}");
        }

        public static bool DeleteMod() 
        {
            var modDirectory = Path.Combine(Storage.ModeSettings.ModsDirectory, Storage.ModeSettings.ModeName);

            if (!Directory.Exists(modDirectory))
            {
                OnOutput?.Invoke($"{modDirectory} not found!");
                return false;
            }

            if (!FileManager.DeleteDirectoryWithFiles(modDirectory, out string error))
            {
                OnOutput?.Invoke(error);
                return false;
            }

            OnOutput?.Invoke($"{modDirectory} has been deleted.");

            return true;
        }

        public static async Task<bool> GenerateModAsync() 
        {
            var batFullPath = Path.Combine(Storage.ModeSettings.ModsDirectory, Storage.ModeSettings.ModeName, _generateModBatFileName);

            var modDirectory = Path.Combine(Storage.ModeSettings.ModsDirectory, Storage.ModeSettings.ModeName);

            if (!File.Exists(batFullPath))
            {
                OnOutput?.Invoke($"{batFullPath} file does not exist!");
                return false;
            }

            using var cmdProvier = new CMDProvider(modDirectory);

            cmdProvier.OnOutput += OnCMDProviderOutput;

            return await cmdProvier.PerformCMDCommand(_generateModBatFileName);
        }

        private static void OnCMDProviderOutput(string data)
        {
            OnOutput?.Invoke(data);
        }
    }
}
