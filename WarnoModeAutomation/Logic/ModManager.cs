using Microsoft.Maui.Storage;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using WarnoModeAutomation.Constants;
using WarnoModeAutomation.DTO;

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

            return await cmdProvier.PerformCMDCommand($"{_createNewModBatFileName} {Storage.ModeSettings.ModName}");
        }

        //ToDo: Required Remove mod aslo from C:\Users\Dmitry\Saved Games\EugenSystems\WARNO\mod
        public static bool DeleteMod() 
        {
            var modDirectory = Path.Combine(Storage.ModeSettings.ModsDirectory, Storage.ModeSettings.ModName);

            var modSavedGamesDirectory = Path.Combine(FileManager.SavedGamesEugenSystemsModPath, Storage.ModeSettings.ModName);

            var savedGamesConfigFilePath = Path.Combine(modSavedGamesDirectory, WarnoConstants.ConfigFileName);

            if (!File.Exists(savedGamesConfigFilePath))
            {
                OnOutput?.Invoke($"{savedGamesConfigFilePath} not found!");
                return false;
            }

            if (!Directory.Exists(modDirectory))
            {
                OnOutput?.Invoke($"{modDirectory} not found!");
                return false;
            }

            if (!FileManager.TryDeleteDirectoryWithFiles(modDirectory, out string modDirectoryErrors))
            {
                OnOutput?.Invoke(modDirectoryErrors);
                return false;
            }

            if (!FileManager.TryDeleteDirectoryWithFiles(modSavedGamesDirectory, out string modSavedGamesDirectoryErrors))
            {
                OnOutput?.Invoke(modSavedGamesDirectoryErrors);
                return false;
            }

            if (!FileManager.TryDeleteFile(savedGamesConfigFilePath, out string savedGamesConfigFilePathErrors))
            {
                OnOutput?.Invoke(savedGamesConfigFilePathErrors);
                return false;
            }

            OnOutput?.Invoke($"{Storage.ModeSettings.ModName} mod has been deleted.");

            return true;
        }

        public static async Task<bool> GenerateModAsync() 
        {
            var batFullPath = Path.Combine(Storage.ModeSettings.ModsDirectory, Storage.ModeSettings.ModName, _generateModBatFileName);

            var modDirectory = Path.Combine(Storage.ModeSettings.ModsDirectory, Storage.ModeSettings.ModName);

            if (!File.Exists(batFullPath))
            {
                OnOutput?.Invoke($"{batFullPath} file does not exist!");
                return false;
            }

            using var cmdProvier = new CMDProvider(modDirectory);

            cmdProvier.OnOutput += OnCMDProviderOutput;

            return await cmdProvier.PerformCMDCommand(_generateModBatFileName);
        }

        public static void Modify()
        {
            //modifying buildings
            ModifyBuildings();
        }

        private static void ModifyUnits() 
        {
            var filePath = FileManager.NDFFilesPaths.SingleOrDefault(f => f.FileName == WarnoConstants.UniteDescriptorFileName);

            var fileDescriptor = NDFSerializer.Deserialize<TEntityDescriptor>(filePath.FilePath);

            //Chaparral

            var chaparralEntityDescriptor = fileDescriptor.EntityDescriptors.FirstOrDefault(x => x.EntityName.Contains("M48_Chaparral"));

            if (chaparralEntityDescriptor is null)
                return;

            var tProductionModuleDescriptor = chaparralEntityDescriptor.ModulesDescriptors
                    .Single(x => x.Value.Type.Equals(typeof(TProductionModuleDescriptor)))
                    .Value.DescriptorObject as TProductionModuleDescriptor;

            foreach (var item in tProductionModuleDescriptor.ProductionRessourcesNeeded)
            {
                if (item.Key.Contains("Resource_CommandPoints"))
                {
                    tProductionModuleDescriptor.ProductionRessourcesNeeded.Remove(item.Key);
                    tProductionModuleDescriptor.ProductionRessourcesNeeded[item.Key] = 105;
                }
            }

        }

        private static void ModifyBuildings()
        {
            var buildingsFilePath = FileManager.NDFFilesPaths.SingleOrDefault(f => f.FileName == WarnoConstants.BuildingDescriptorsFileName);

            var fileDescriptor = NDFSerializer.Deserialize<TEntityDescriptor>(buildingsFilePath.FilePath);

            foreach (var item in fileDescriptor.EntityDescriptors)
            {
                var tSupplyModuleDescriptors = item.ModulesDescriptors
                    .Where(x => x.Value.Type.Equals(typeof(TSupplyModuleDescriptor)))
                    .Select(x => x.Value.DescriptorObject as TSupplyModuleDescriptor);

                foreach (var tSupplyModuleDescriptor in tSupplyModuleDescriptors)
                {
                    tSupplyModuleDescriptor.SupplyCapacity = 36000;
                }

                var tProductionModuleDescriptors = item.ModulesDescriptors
                    .Where(x => x.Value.Type.Equals(typeof(TProductionModuleDescriptor)))
                    .Select(x => x.Value.DescriptorObject as TProductionModuleDescriptor);

                foreach (var tProductionModuleDescriptor in tProductionModuleDescriptors)
                {
                    if (tProductionModuleDescriptor.ProductionRessourcesNeeded.ContainsKey("~/Resource_CommandPoints"))
                    {
                        tProductionModuleDescriptor.ProductionRessourcesNeeded["~/Resource_CommandPoints"] = 200;
                    }
                }
            }

            var stringToSave = NDFSerializer.Serialize(fileDescriptor);

            File.WriteAllText(buildingsFilePath.FilePath, stringToSave);
        }

        private static void OnCMDProviderOutput(string data)
        {
            OnOutput?.Invoke(data);
        }
    }
}
