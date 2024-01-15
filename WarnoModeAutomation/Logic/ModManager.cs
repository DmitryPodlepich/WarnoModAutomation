using JsonDatabase.DTO;
using NDFSerialization.Models;
using NDFSerialization.NDFDataTypes.Primitive;
using System.ComponentModel;
using System.Diagnostics;
using WarnoModeAutomation.Constants;
using WarnoModeAutomation.DTO;
using WarnoModeAutomation.DTO.NDFFiles;
using WarnoModeAutomation.DTO.NDFFiles.Ammunition;
using WarnoModeAutomation.DTO.NDFFiles.Weapon;
using WarnoModeAutomation.DTO.NDFFiles.Weapon.Interfaces;
using WarnoModeAutomation.Extensions;
using WebSearch;

namespace WarnoModeAutomation.Logic
{
    public static class ModManager
    {
        private const string _createNewModBatFileName = "CreateNewMod.bat";
        private const string _generateModBatFileName = "GenerateMod.bat";
        private const string _updateModBatFileName = "UpdateMod.bat";

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

            var savedGamesConfigFilePath = Path.Combine(FileManager.SavedGamesEugenSystemsModPath, WarnoConstants.ConfigFileName);

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

        public static async Task UpdateModAsync() 
        {
            var batFullPath = Path.Combine(Storage.ModeSettings.ModsDirectory, Storage.ModeSettings.ModName, _updateModBatFileName);

            var modDirectory = Path.Combine(Storage.ModeSettings.ModsDirectory, Storage.ModeSettings.ModName);

            if (!File.Exists(batFullPath))
            {
                OnOutput?.Invoke($"{batFullPath} file does not exist!");
            }

            using var cmdProvier = new CMDProvider(modDirectory);

            cmdProvier.OnOutput += OnCMDProviderOutput;

            _ = await cmdProvier.PerformCMDCommand(_generateModBatFileName);
        }

        public static async Task FillDatabaseAsync(CancellationTokenSource cancellationTokenSource) 
        {
            WebSearchEngine.OnOutput += OnCMDProviderOutput;
            await WebSearchEngine.FillDatabaseWithMilitaryTodayAsync(cancellationTokenSource);
        }

        public static async Task Modify()
        {
            var amunitionFilePath = FileManager.NDFFilesPaths.SingleOrDefault(f => f.FileName == WarnoConstants.AmmunitionDescriptorsFileName);
            var weaponFilePath = FileManager.NDFFilesPaths.SingleOrDefault(f => f.FileName == WarnoConstants.WeaponDescriptorDescriptorsFileName);
            var unitsFilePath = FileManager.NDFFilesPaths.SingleOrDefault(f => f.FileName == WarnoConstants.UniteDescriptorFileName);
            var buildingsFilePath = FileManager.NDFFilesPaths.SingleOrDefault(f => f.FileName == WarnoConstants.BuildingDescriptorsFileName);


            FileDescriptor<TAmmunitionDescriptor> amunition = null;
            FileDescriptor<TWeaponManagerModuleDescriptor> weapon = null;
            FileDescriptor<TEntityDescriptor> units = null;
            FileDescriptor<TEntityDescriptor> buildings = null;

            Task[] tasks =
            [
                Task.Run(() => amunition = NDFSerializer.Deserialize<TAmmunitionDescriptor>(amunitionFilePath.FilePath)),
                Task.Run(() => weapon = NDFSerializer.Deserialize<TWeaponManagerModuleDescriptor>(weaponFilePath.FilePath)),
                Task.Run(() => units = NDFSerializer.Deserialize<TEntityDescriptor>(unitsFilePath.FilePath)),
                Task.Run(() => buildings = NDFSerializer.Deserialize<TEntityDescriptor>(buildingsFilePath.FilePath)),
            ];

            await Task.WhenAll(tasks);

            var unitsRelatedData = new UnitsRelatedDataDTO(amunition, weapon, units);

            unitsRelatedData = ModifyUnits(unitsRelatedData);

            buildings = ModifyBuildings(buildings);

            //Serialize all descriptors here back to ndf files
            await File.WriteAllTextAsync(unitsFilePath.FilePath, NDFSerializer.Serialize(units));
            await File.WriteAllTextAsync(amunitionFilePath.FilePath, NDFSerializer.Serialize(amunition));
            await File.WriteAllTextAsync(weaponFilePath.FilePath, NDFSerializer.Serialize(weapon));
            await File.WriteAllTextAsync(buildingsFilePath.FilePath, NDFSerializer.Serialize(buildings));
        }

        private static UnitsRelatedDataDTO ModifyUnits(UnitsRelatedDataDTO unitsRelatedData) 
        {
            const string infanterie = "Infanterie";

            var tagsCombinationWithNerf = new[] { "Canon_AA_Standard", "Canon_AA", "Air" };

            //units which will use real fire range with nerf (Aircrafts, anti air vehicles, except anti air infantry)
            var unitsWithNerf = new List<TEntityDescriptor>();

            foreach (var tEntityDescriptor in unitsRelatedData.UnitsEntityDescriptor.RootDescriptors)
            {
                var tagsModule = tEntityDescriptor.ModulesDescriptors.OfType<TTagsModuleDescriptor>().SingleOrDefault();

                if (tagsModule.TagSet.Any(tag => tagsCombinationWithNerf.Contains(tag)) && !tagsModule.TagSet.Contains(infanterie))
                    unitsWithNerf.Add(tEntityDescriptor);
            }

            //units which will use real fire range values without nerf (Infantry, helicopters, all ground vehicles except anti air)
            var unitsWithoutNerf = unitsRelatedData.UnitsEntityDescriptor.RootDescriptors.Except(unitsWithNerf);

            var modifiedAmunition = new HashSet<string>();

            foreach (var unit in unitsWithNerf)
            {
                try
                {

                    //find unit weapon and ammo information
                    var unitWeaponModuleDescriptor = unit.ModulesDescriptors
                        .OfType<TModuleSelector>()
                        .SingleOrDefault(d => d.EntityNDFType == "WeaponManager");

                    if (unitWeaponModuleDescriptor is null)
                    {
                        OnCMDProviderOutput($"Cannot find TModuleSelector for unit: {unit.ClassNameForDebug}");
                        continue;
                    }

                    var weaponManagerModule = unitsRelatedData.WeaponManagerModuleDescriptor.RootDescriptors
                        .SingleOrDefault(w => unitWeaponModuleDescriptor.Default.Contains(w.EntityNDFType, StringComparison.InvariantCultureIgnoreCase));

                    if (weaponManagerModule is null)
                    {
                        OnCMDProviderOutput($"Cannot find weapon module by name: {unitWeaponModuleDescriptor.Default}");
                        continue;
                    }

                    var unitAmunitionNames = weaponManagerModule
                        .TurretDescriptorList.OfType<ITTurretDescriptor>()
                        .SelectMany(d => d.MountedWeaponDescriptorList.OfType<TMountedWeaponDescriptor>())
                        .Select(w => w.Ammunition.Replace("~/", ""));

                    var unitAmunitions = unitsRelatedData.AmmunitionDescriptor.RootDescriptors
                        .Where(d => unitAmunitionNames.Contains(d.EntityNDFType));

                    if (!unitAmunitions.Any())
                    {
                        OnCMDProviderOutput($"Cannot find unitAmunitions for unit: {unit.ClassNameForDebug}");
                        continue;
                    }

                    bool unitAmunitionsHasChanges = false;

                    foreach (var unitAmunition in unitAmunitions)
                    {
                        if (modifiedAmunition.Contains(unitAmunition.EntityNDFType))
                            continue;

                        var realFireRange = JsonDatabase.JsonDatabase.FindAmmoRange(unitAmunition.EntityNDFType);

                        if (realFireRange is null)
                        {
                            OnCMDProviderOutput($"Cannot find real amunition fire range fro unit: {unit.ClassNameForDebug}. unitAmunition: {unitAmunition.EntityNDFType}");
                            continue;
                        }

                        //ToDo: There potentially will be a bug. If unit have several amunitions for same ground or air target.
                        ModifyAmunition(unit, weaponManagerModule, unitAmunition, realFireRange, true);

                        modifiedAmunition.Add(unitAmunition.EntityNDFType);

                        unitAmunitionsHasChanges = true;

                        OnCMDProviderOutput($"Unit: {unit.ClassNameForDebug} amunition {unitAmunition.EntityNDFType} modified!");
                    }

                    if (unitAmunitionsHasChanges)
                        UpdateUnitVisionByAmunitionDistance(unit, unitAmunitions);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }

            return unitsRelatedData;
        }

        private static void ModifyAmunition(TEntityDescriptor unit, TWeaponManagerModuleDescriptor weaponManager, TAmmunitionDescriptor ammunition, AmmoRangeDTO ammoRangeDTO, bool shouldNerf) 
        {
            int additionalResourceCommandPoints = 0;

            var tProductionModuleDescriptors = unit.ModulesDescriptors.OfType<TProductionModuleDescriptor>().SingleOrDefault();

            if (tProductionModuleDescriptors is null)
            {
                Debugger.Break();
                OnCMDProviderOutput($"Unit: {unit.ClassNameForDebug} TProductionModuleDescriptor not found!");
                return;
            }

            if (!tProductionModuleDescriptors.ProductionRessourcesNeeded.TryGetValue("~/Resource_CommandPoints", out int resourceCommandPoints))
            {
                Debugger.Break();
                OnCMDProviderOutput($"Cannot find Resource_CommandPoints for unit: {unit.ClassNameForDebug}");
                return;
            }

            var tTypeUnitModuleDescriptor = unit.ModulesDescriptors.OfType<TTypeUnitModuleDescriptor>().SingleOrDefault();

            if (tTypeUnitModuleDescriptor is null)
            {
                Debugger.Break();
                OnCMDProviderOutput($"Unit: {unit.ClassNameForDebug} TTypeUnitModuleDescriptor not found!");
                return;
            }

            var realDistanceInMetersToWarnoDistance = ConvertToWarnoDistance(ammoRangeDTO.FireRangeInMeters);

            var unitPorteeMaximaleModificationData = new UnitModificationDataDTO(
                ammunition.PorteeMaximale,
                ammunition,
                resourceCommandPoints,
                realDistanceInMetersToWarnoDistance,
                shouldNerf);

            var unitPorteeMaximaleTBAModificationData = unitPorteeMaximaleModificationData with { DistanceMetre = ammunition.PorteeMaximaleTBA };

            var unitPorteeMaximaleHAModificationData = unitPorteeMaximaleModificationData with { DistanceMetre = ammunition.PorteeMaximaleHA };

            SetNewDistanceMetre(ref unitPorteeMaximaleModificationData, ref additionalResourceCommandPoints);

            SetNewDistanceMetre(ref unitPorteeMaximaleTBAModificationData, ref additionalResourceCommandPoints);

            SetNewDistanceMetre(ref unitPorteeMaximaleHAModificationData, ref additionalResourceCommandPoints);

            if (!NDFTypesExtensions.IsSovUnit(tTypeUnitModuleDescriptor))
            {
                foreach (var baseHitValueModifier in ammunition.HitRollRuleDescriptor.BaseHitValueModifiers)
                {
                    if (baseHitValueModifier.Value > 0)
                    {
                        var additionalAccuracityPercentage = GetNumberPercentage(WarnoConstants.NatoAccuracityBuffPercentage, baseHitValueModifier.Value);
                        var newAccuracityValue = baseHitValueModifier.Value + additionalAccuracityPercentage;
                        additionalResourceCommandPoints += ((int)additionalAccuracityPercentage) * 2;
                        ammunition.HitRollRuleDescriptor.BaseHitValueModifiers[baseHitValueModifier.Key] = newAccuracityValue;
                    }
                }
            }

            //Add additional resource command points
            tProductionModuleDescriptors.ProductionRessourcesNeeded["~/Resource_CommandPoints"] += additionalResourceCommandPoints;
        }

        private static void UpdateUnitVisionByAmunitionDistance(TEntityDescriptor unit, IEnumerable<TAmmunitionDescriptor> ammunitionDescriptor)
        {
            var scannerConfigurationDescriptor = unit.ModulesDescriptors.OfType<TScannerConfigurationDescriptor>().SingleOrDefault();

            if (scannerConfigurationDescriptor is null)
            {
                Debugger.Break();
                OnCMDProviderOutput($"Unit: {unit.ClassNameForDebug} TScannerConfigurationDescriptor not found!");
                return;
            }

            var tTypeUnitModuleDescriptor = unit.ModulesDescriptors.OfType<TTypeUnitModuleDescriptor>().SingleOrDefault();

            if (tTypeUnitModuleDescriptor is null)
            {
                Debugger.Break();
                OnCMDProviderOutput($"Unit: {unit.ClassNameForDebug} TTypeUnitModuleDescriptor not found!");
                return;
            }

            var isSovUnit = NDFTypesExtensions.IsSovUnit(tTypeUnitModuleDescriptor);

            if (scannerConfigurationDescriptor.OpticalStrength == 0)
            {
                scannerConfigurationDescriptor.OpticalStrength = isSovUnit ? WarnoConstants.SovMinOpticalStrength : WarnoConstants.NatoMinOpticalStrength;
            }

            var longestGroundAmunitionRange = ammunitionDescriptor.Select(x => x.PorteeMaximale).MaxBy(x => x.FloatValue);

            var longestTBAAmunitionRange = ammunitionDescriptor.Select(x => x.PorteeMaximaleTBA).MaxBy(x => x.FloatValue);

            var longestHAAmunitionRange = ammunitionDescriptor.Select(x => x.PorteeMaximaleHA).MaxBy(x => x.FloatValue);

            var newTBAVisionValue = Math.Max(CalculateUnitVisionTBA(longestTBAAmunitionRange, isSovUnit), CalculateUnitVisionHA(longestHAAmunitionRange, isSovUnit));

            scannerConfigurationDescriptor.PorteeVisionTBA.FloatValue = Math.Max(scannerConfigurationDescriptor.PorteeVisionTBA.FloatValue, newTBAVisionValue);

            var newDetectionValue = CalculateUnitDetectionTBAAndHA(scannerConfigurationDescriptor.PorteeVisionTBA.FloatValue);

            scannerConfigurationDescriptor.DetectionTBA.FloatValue = Math.Max(scannerConfigurationDescriptor.DetectionTBA.FloatValue, newDetectionValue);

            var newGroundVisionValue = Math.Max(scannerConfigurationDescriptor.PorteeVision.FloatValue, CalculateUnitVisionGround(longestGroundAmunitionRange, isSovUnit));

            scannerConfigurationDescriptor.PorteeVision.FloatValue = Math.Max(scannerConfigurationDescriptor.PorteeVision.FloatValue, newGroundVisionValue);

            //ToDo: change unit vison according to a new fire range
            //-For Scout unit: vision - 2 %;
            //SpecializedDetections research
        }

        private static float CalculateUnitDetectionTBAAndHA(float visionDistance) 
        {
            var percentage = GetNumberPercentage(WarnoConstants.TBAAndHADetectionPercentage ,visionDistance);

            return visionDistance - percentage;
        }

        private static float CalculateUnitVisionGround(DistanceMetre amunitionDistance, bool isSovUnit)
        {
            var percentage = isSovUnit
                ? GetNumberPercentage(WarnoConstants.SovGroundisionPercentage, amunitionDistance.FloatValue)
                : GetNumberPercentage(WarnoConstants.NatoGroundVisionPercentage, amunitionDistance.FloatValue);

            return amunitionDistance.FloatValue - percentage;
        }

        private static float CalculateUnitVisionTBA(DistanceMetre amunitionDistance, bool isSovUnit)
        {
            var percentage = isSovUnit 
                ? GetNumberPercentage(WarnoConstants.SovTBAVisionPercentage, amunitionDistance.FloatValue) 
                : GetNumberPercentage(WarnoConstants.NatoTBAVisionPercentage, amunitionDistance.FloatValue);

            return amunitionDistance.FloatValue - percentage;
        }

        private static float CalculateUnitVisionHA(DistanceMetre amunitionDistance, bool isSovUnit)
        {
            var percentage = isSovUnit
                ? GetNumberPercentage(WarnoConstants.SovHAVisionPercentage, amunitionDistance.FloatValue)
                : GetNumberPercentage(WarnoConstants.NatoHAVisionPercentage, amunitionDistance.FloatValue);

            return amunitionDistance.FloatValue - percentage;
        }

        private static void SetNewDistanceMetre(ref UnitModificationDataDTO unitModificationDataDTO, ref int additionalResourceCommandPoints)
        {
            if (IsAllowedToChangeValue(unitModificationDataDTO.DistanceMetre, unitModificationDataDTO.RealFireRangeDistance))
            {
                if (unitModificationDataDTO.NerfRequired)
                    unitModificationDataDTO.RealFireRangeDistance = NerfDistance(unitModificationDataDTO.RealFireRangeDistance, unitModificationDataDTO.DistanceMetre.FloatValue);

                AddAdditionalResourceCommandPoints(ref additionalResourceCommandPoints, unitModificationDataDTO.OriginalResourceCommandPoints, unitModificationDataDTO.DistanceMetre.FloatValue, unitModificationDataDTO.RealFireRangeDistance);

                unitModificationDataDTO.DistanceMetre.FloatValue = unitModificationDataDTO.RealFireRangeDistance;
            }
        }

        /// <summary>
        /// Returns true if original value more than zero and less than new value otherwise false.
        /// </summary>
        private static bool IsAllowedToChangeValue(DistanceMetre distanceMetre, float realDistanceInMetersToWarnoDistance)
        {
            return distanceMetre.FloatValue > 0 && distanceMetre.FloatValue < realDistanceInMetersToWarnoDistance;
        }

        private static void AddAdditionalResourceCommandPoints(ref int pointsCount, int originalPointsCount, float originalDistance, float increasedDistance)
        {
            var difference = increasedDistance - originalDistance;

            var increasePercentage = GetPercentageAfromB(difference, increasedDistance);

            pointsCount += (int)Math.Round((increasePercentage / 8) + (originalPointsCount / 8));
        }

        private static float GetNumberPercentage(int percent, float fromNumber)
        {
            return (float)percent / 100 * fromNumber;
        }

        private static float GetPercentageAfromB(float a, float b)
        {
            return a / b * 100;
        }

        private static float ConverToWarnoDistance(float value)
        {
            return value / 1000 * 2830;
        }

        private static float NerfDistance(float newValue, float originalValue)
        {
            if((originalValue * 2) >= newValue)
                return (int)newValue;

            var difference = newValue - originalValue;

            var percentageDifference = GetPercentageAfromB(difference, newValue);

            var percentageoriginalValue = GetPercentageAfromB(originalValue, newValue);

            //var repcentageTotal = difference / percentageoriginalValue * 10;

            var repcentageTotal = difference / percentageoriginalValue * (percentageDifference / 10);

            return (float)Math.Round((double)(newValue - repcentageTotal));

            // ThunderBold agm65d maveric
            //result = 62260
            //original = 8915
            //62260 bigger than 8915 on 698%
        }

        private static float ConvertToWarnoDistance(int valueToConvert)
        {
            return valueToConvert / 1000 * 2830;
        }

        private static FileDescriptor<TEntityDescriptor> ModifyBuildings(FileDescriptor<TEntityDescriptor> buildingsFileDescriptor)
        {
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

            return buildingsFileDescriptor;
        }

        private static void OnCMDProviderOutput(string data)
        {
            OnOutput?.Invoke(data);
            Debug.WriteLine("OnCMDProviderOutput: " + data);
        }
    }
}
