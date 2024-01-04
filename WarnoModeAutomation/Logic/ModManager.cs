using Microsoft.Maui.Storage;
using NDFSerialization.Models;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using WarnoModeAutomation.Constants;
using WarnoModeAutomation.DTO;
using WarnoModeAutomation.DTO.NDFFiles;
using WarnoModeAutomation.DTO.NDFFiles.Ammunition;
using WarnoModeAutomation.DTO.NDFFiles.Weapon;
using WebSearch;

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

        //ToDo: Not tested yet!
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

        public static async Task FillDatabaseAsync(CancellationTokenSource cancellationTokenSource) 
        {
            WebSearchEngine.OnOutput += OnCMDProviderOutput;
            await WebSearchEngine.FillDatabaseWithMilitaryTodayAsync(cancellationTokenSource);
        }

        public static async Task Modify()
        {
            var amunitionFilePath = FileManager.NDFFilesPaths.SingleOrDefault(f => f.FileName == WarnoConstants.AmmunitionDescriptorsFileName);

            var amunition = NDFSerializer.Deserialize<TAmmunitionDescriptor>(amunitionFilePath.FilePath);

            var weaponFilePath = FileManager.NDFFilesPaths.SingleOrDefault(f => f.FileName == WarnoConstants.WeaponDescriptorDescriptorsFileName);

            var weapon = NDFSerializer.Deserialize<TWeaponManagerModuleDescriptor>(weaponFilePath.FilePath);

            var unitsFilePath = FileManager.NDFFilesPaths.SingleOrDefault(f => f.FileName == WarnoConstants.UniteDescriptorFileName);

            var units = NDFSerializer.Deserialize<TEntityDescriptor>(unitsFilePath.FilePath);

            var buildingsFilePath = FileManager.NDFFilesPaths.SingleOrDefault(f => f.FileName == WarnoConstants.BuildingDescriptorsFileName);

            var buildings = NDFSerializer.Deserialize<TEntityDescriptor>(buildingsFilePath.FilePath);
        }

        private static async Task ModifyUnits() 
        {
            var filePath = FileManager.NDFFilesPaths.SingleOrDefault(f => f.FileName == WarnoConstants.UniteDescriptorFileName);

            var unitsFileDescriptor = NDFSerializer.Deserialize<TEntityDescriptor>(filePath.FilePath);

            const string infanterie = "Infanterie";

            var tagsCombinationWithNerf = new []{ "Canon_AA_Standard", "Canon_AA", "Air" };

            //units which will use real fire range with nerf (Aircrafts, anti air vehicles, except anti air infantry)
            var unitsWithNerf = new List<TEntityDescriptor>();

            foreach (var tEntityDescriptor in unitsFileDescriptor.RootDescriptors)
            {
                var tagsModule = tEntityDescriptor.ModulesDescriptors.OfType<TTagsModuleDescriptor>().SingleOrDefault();

                if (tagsModule.TagSet.Any(tag => tagsCombinationWithNerf.Contains(tag)) && !tagsModule.TagSet.Contains(infanterie))
                    unitsWithNerf.Add(tEntityDescriptor);
            }

            //units which will use real fire range values without nerf (Infantry, helicopters, all ground vehicles except anti air)
            var unitsWithoutNerf = unitsFileDescriptor.RootDescriptors.Except(unitsWithNerf);

            Debug.WriteLine(Environment.NewLine);
            Debug.WriteLine("unitsWithNerf:");
            Debug.WriteLine(Environment.NewLine);

            foreach (var unit in unitsWithNerf)
            {
                Debug.WriteLine(unit.ClassNameForDebug);
            }

            Debug.WriteLine(Environment.NewLine);
            Debug.WriteLine("unitsWithoutNerf:");
            Debug.WriteLine(Environment.NewLine);

            foreach (var unit in unitsWithoutNerf)
            {
                Debug.WriteLine(unit.ClassNameForDebug);
            }

            Debug.WriteLine(Environment.NewLine);

            //Chaparral

            //var chaparralEntityDescriptor = unitsFileDescriptor.RootDescriptors.FirstOrDefault(x => x.ClassNameForDebug.Equals("Unit_M48_Chaparral_MIM72F_US"));

            //if (chaparralEntityDescriptor is null)
            //    return;

            //chaparralEntityDescriptor.SetRealUnitName();

            //await WebSearchEngine.Initialize();

            //var result = await WebSearchEngine.GetRealFireRange("MIM72G", chaparralEntityDescriptor.GameUIUnitName, chaparralEntityDescriptor.ClassNameForDebug);

            //var tProductionModuleDescriptor = chaparralEntityDescriptor.ModulesDescriptors
            //        .OfType<TProductionModuleDescriptor>()
            //        .SingleOrDefault();

            //if (tProductionModuleDescriptor.ProductionRessourcesNeeded.ContainsKey("~/Resource_CommandPoints"))
            //{
            //    tProductionModuleDescriptor.ProductionRessourcesNeeded["~/Resource_CommandPoints"] = 105;
            //}
        }

        private static void ModifyBuildings()
        {
            var buildingsFilePath = FileManager.NDFFilesPaths.SingleOrDefault(f => f.FileName == WarnoConstants.BuildingDescriptorsFileName);

            var buildingsFileDescriptor = NDFSerializer.Deserialize<TEntityDescriptor>(buildingsFilePath.FilePath);

            foreach (var entityDescriptor in buildingsFileDescriptor.RootDescriptors)
            {
                var tSupplyModuleDescriptor = entityDescriptor.ModulesDescriptors.OfType<TSupplyModuleDescriptor>().SingleOrDefault();

                tSupplyModuleDescriptor.SupplyCapacity = 46000;

                var tProductionModuleDescriptors = entityDescriptor.ModulesDescriptors.OfType<TProductionModuleDescriptor>().SingleOrDefault();

                if (tProductionModuleDescriptors.ProductionRessourcesNeeded.ContainsKey("~/Resource_CommandPoints"))
                {
                    tProductionModuleDescriptors.ProductionRessourcesNeeded["~/Resource_CommandPoints"] = 225;
                }
            }

            var stringToSave = NDFSerializer.Serialize(buildingsFileDescriptor);

            File.WriteAllText(buildingsFilePath.FilePath, stringToSave);
        }

        private static void OnCMDProviderOutput(string data)
        {
            OnOutput?.Invoke(data);
            Debug.WriteLine("OnCMDProviderOutput: " + data);
        }
    }
}
