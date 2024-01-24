using BlazorBootstrap;
using JsonDatabase.DTO;
using NDFSerialization.Models;
using NDFSerialization.NDFDataTypes.Primitive;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using WarnoModeAutomation.Constants;
using WarnoModeAutomation.DTO;
using WarnoModeAutomation.DTO.NDFFiles;
using WarnoModeAutomation.DTO.NDFFiles.Ammunition;
using WarnoModeAutomation.DTO.NDFFiles.Weapon;
using WarnoModeAutomation.DTO.NDFFiles.Weapon.Interfaces;
using WarnoModeAutomation.Extensions;
using WebSearch;

[assembly: InternalsVisibleTo("nUnitTests")]
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
                Task.Run(() =>
                {
                    amunition = NDFSerializer.Deserialize<TAmmunitionDescriptor>(amunitionFilePath.FilePath);
                    OnCMDProviderOutput($"{amunitionFilePath.FileName} desirialized!");
                }),
                Task.Run(() =>
                {
                    weapon = NDFSerializer.Deserialize<TWeaponManagerModuleDescriptor>(weaponFilePath.FilePath);
                    OnCMDProviderOutput($"{weaponFilePath.FileName} desirialized!");
                }),
                Task.Run(() =>
                {
                    units = NDFSerializer.Deserialize<TEntityDescriptor>(unitsFilePath.FilePath);
                    OnCMDProviderOutput($"{unitsFilePath.FileName} desirialized!");
                }),
                Task.Run(() =>
                {
                    buildings = NDFSerializer.Deserialize<TEntityDescriptor>(buildingsFilePath.FilePath);
                    OnCMDProviderOutput($"{buildingsFilePath.FileName} desirialized!");
                }),
            ];

            await Task.WhenAll(tasks);

            var unitsRelatedData = new UnitsRelatedDataDTO(amunition, weapon, units);

            ModifyUnits(unitsRelatedData);

            buildings = ModifyBuildings(buildings);

            //Serialize all descriptors here back to ndf files
            await File.WriteAllTextAsync(unitsFilePath.FilePath, NDFSerializer.Serialize(units));
            await File.WriteAllTextAsync(amunitionFilePath.FilePath, NDFSerializer.Serialize(amunition));
            await File.WriteAllTextAsync(weaponFilePath.FilePath, NDFSerializer.Serialize(weapon));
            await File.WriteAllTextAsync(buildingsFilePath.FilePath, NDFSerializer.Serialize(buildings));
        }

        private static void ModifyUnits(UnitsRelatedDataDTO unitsRelatedData) 
        {
            var modifiedAmunition = new Dictionary<string, int>();

            foreach (var unit in unitsRelatedData.UnitsEntityDescriptor.RootDescriptors)
            {
                ModifyUnit(unit, unitsRelatedData, ref modifiedAmunition);
            }
        }

        private static void ModifyUnit(TEntityDescriptor unit, UnitsRelatedDataDTO unitsRelatedData, ref Dictionary<string, int> modifiedAmunition) 
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
                    return;
                }

                var weaponManagerModule = unitsRelatedData.WeaponManagerModuleDescriptor.RootDescriptors
                    .SingleOrDefault(w => unitWeaponModuleDescriptor.Default.Contains(w.EntityNDFType, StringComparison.InvariantCultureIgnoreCase));

                if (weaponManagerModule is null)
                {
                    OnCMDProviderOutput($"Cannot find weapon module by name: {unitWeaponModuleDescriptor.Default}");
                    return;
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
                    return;
                }

                var tProductionModuleDescriptors = unit.ModulesDescriptors.OfType<TProductionModuleDescriptor>().SingleOrDefault();

                if (tProductionModuleDescriptors is null)
                {
                    OnCMDProviderOutput($"Unit: {unit.ClassNameForDebug} TProductionModuleDescriptor not found!");
                    return;
                }

                if (!tProductionModuleDescriptors.ProductionRessourcesNeeded.TryGetValue("~/Resource_CommandPoints", out int resourceCommandPoints))
                {
                    OnCMDProviderOutput($"Cannot find Resource_CommandPoints for unit: {unit.ClassNameForDebug}");
                    return;
                }

                var tTypeUnitModuleDescriptor = unit.ModulesDescriptors.OfType<TTypeUnitModuleDescriptor>().SingleOrDefault();

                if (tTypeUnitModuleDescriptor is null)
                {
                    OnCMDProviderOutput($"Unit: {unit.ClassNameForDebug} TTypeUnitModuleDescriptor not found!");
                    return;
                }

                var tagsModuleDescriptor = unit.ModulesDescriptors.OfType<TTagsModuleDescriptor>().SingleOrDefault();

                if (tagsModuleDescriptor is null)
                {
                    OnCMDProviderOutput($"Unit: {unit.ClassNameForDebug} TTagsModuleDescriptor not found!");
                    return;
                }

                bool shouldNerf =
                    tagsModuleDescriptor.TagSet.Any(tag => WarnoConstants.TagsCombinationWithNerf.Contains(tag))
                    && !tagsModuleDescriptor.TagSet.Contains(WarnoConstants.InfanterieTag);

                bool unitAmunitionsHasDistanceChanges = false;

                var additionalCommandPoins = 0;

                foreach (var unitAmunition in unitAmunitions)
                {
                    //If unit has the same amunition which already has been modified we need to add additional command point to that unit also for each modified amunition.
                    if (modifiedAmunition.TryGetValue(unitAmunition.EntityNDFType, out int value))
                    {
                        additionalCommandPoins += value;
                        continue;
                    }

                    if (EnsureModifiedArtileryDamage(unitAmunition))
                        OnCMDProviderOutput($"ArtileryDamage for: {unit.ClassNameForDebug}. unitAmunition: {unitAmunition.EntityNDFType} has been increased by {WarnoConstants.ArtileryDamagePercentage} %");

                    if (!NDFTypesExtensions.IsSovUnit(tTypeUnitModuleDescriptor))
                        additionalCommandPoins += ModifyAmunitionAccuracity(unitAmunition);

                    var realFireRange = JsonDatabase.JsonDatabase.FindAmmoRange(unitAmunition.EntityNDFType);

                    if (realFireRange is not null)
                    {
                        ModifyAmunitionDistance(tProductionModuleDescriptors, unitAmunition, realFireRange, shouldNerf);

                        OnCMDProviderOutput($"Fire range for unit: {unit.ClassNameForDebug}. unitAmunition: {unitAmunition.EntityNDFType} has been changed!");

                        unitAmunitionsHasDistanceChanges = true;
                    }

                    modifiedAmunition.Add(unitAmunition.EntityNDFType, additionalCommandPoins);

                    OnCMDProviderOutput($"Unit: {unit.ClassNameForDebug} amunition {unitAmunition.EntityNDFType} modified!");
                }

                if (unitAmunitionsHasDistanceChanges)
                    UpdateUnitVisionByAmunitionDistance(unit, unitAmunitions);

                if (additionalCommandPoins > 0)
                    tProductionModuleDescriptors.ProductionRessourcesNeeded["~/Resource_CommandPoints"] += additionalCommandPoins.RoundOff();

            }
            catch (Exception ex)
            {
                OnCMDProviderOutput($"ModifyUnit exception: {unit.ClassNameForDebug} message {ex.Message}");
                OnCMDProviderOutput(ex.StackTrace);
                throw;
            }
        }

        private static int ModifyAmunitionAccuracity(TAmmunitionDescriptor ammunition) 
        {
            int additionalResourceCommandPoints = 0;

            if(ammunition.EntityNDFType.Contains(WarnoConstants.AmunitionNameSMOKEMarker, StringComparison.InvariantCultureIgnoreCase)
                || ammunition.WeaponCursorType == WarnoConstants.Weapon_Cursor_MachineGun)
                return additionalResourceCommandPoints;

            foreach (var baseHitValueModifier in ammunition.HitRollRuleDescriptor.BaseHitValueModifiers)
            {
                if (baseHitValueModifier.Value > 0)
                {
                    var additionalAccuracityPercentage = GetNumberPercentage(WarnoConstants.NatoCommonAccuracityBonusPercentage, baseHitValueModifier.Value);
                    var newAccuracityValue = baseHitValueModifier.Value + additionalAccuracityPercentage;
                    additionalResourceCommandPoints += ((int)additionalAccuracityPercentage) * 2;
                    ammunition.HitRollRuleDescriptor.BaseHitValueModifiers[baseHitValueModifier.Key] = (float)Math.Round(newAccuracityValue);
                }
            }

            if (ammunition.WeaponCursorType == WarnoConstants.ArtileryWeaponCursorType)
            {
                if (ammunition.DispersionAtMaxRange.FloatValue > 0)
                {
                    var additionalAccuracityValue = GetNumberPercentage(WarnoConstants.NatoArtileryAccuracityBonusPercentage, ammunition.DispersionAtMaxRange.FloatValue);
                    var newAccuracityValue = ammunition.DispersionAtMaxRange.FloatValue - additionalAccuracityValue;
                    additionalResourceCommandPoints += ((int)additionalAccuracityValue / 5);
                    ammunition.DispersionAtMaxRange.FloatValue = (float)Math.Round(newAccuracityValue);
                }

                if (ammunition.DispersionAtMinRange.FloatValue > 0)
                {
                    var additionalAccuracityValue = GetNumberPercentage(WarnoConstants.NatoArtileryAccuracityBonusPercentage, ammunition.DispersionAtMinRange.FloatValue);
                    var newAccuracityValue = ammunition.DispersionAtMinRange.FloatValue - additionalAccuracityValue;
                    additionalResourceCommandPoints += ((int)additionalAccuracityValue / 5);
                    ammunition.DispersionAtMinRange.FloatValue = (float)Math.Round(newAccuracityValue);
                }
            }

            return additionalResourceCommandPoints;
        }

        private static bool EnsureModifiedArtileryDamage(TAmmunitionDescriptor ammunition) 
        {
            if (ammunition.WeaponCursorType != WarnoConstants.ArtileryWeaponCursorType && !ammunition.EntityNDFType.Contains(WarnoConstants.AmunitionNameSMOKEMarker, StringComparison.InvariantCultureIgnoreCase))
                return false;

            ammunition.PhysicalDamages += GetNumberPercentage(WarnoConstants.ArtileryDamagePercentage, ammunition.PhysicalDamages);
            ammunition.RadiusSplashPhysicalDamages.FloatValue += GetNumberPercentage(WarnoConstants.ArtileryDamagePercentage, ammunition.RadiusSplashPhysicalDamages.FloatValue);
            ammunition.SuppressDamages += GetNumberPercentage(WarnoConstants.ArtileryDamagePercentage, ammunition.SuppressDamages);
            ammunition.RadiusSplashSuppressDamages.FloatValue += GetNumberPercentage(WarnoConstants.ArtileryDamagePercentage, ammunition.RadiusSplashSuppressDamages.FloatValue);

            return true;
        }

        private static void ModifyAmunitionDistance(TProductionModuleDescriptor tProductionModuleDescriptors, TAmmunitionDescriptor ammunition, AmmoRangeDTO ammoRangeDTO, bool shouldNerf) 
        {
            var realDistanceInMetersToWarnoDistance = ConvertToWarnoDistance(ammoRangeDTO.FireRangeInMeters);

            var unitPorteeMaximaleModificationData = new UnitModificationDataDTO(
                ammunition.PorteeMaximale,
                ammunition,
                tProductionModuleDescriptors.ProductionRessourcesNeeded["~/Resource_CommandPoints"],
                realDistanceInMetersToWarnoDistance,
                shouldNerf);

            var unitPorteeMaximaleTBAModificationData = unitPorteeMaximaleModificationData with { DistanceMetre = ammunition.PorteeMaximaleTBA };

            var unitPorteeMaximaleHAModificationData = unitPorteeMaximaleModificationData with { DistanceMetre = ammunition.PorteeMaximaleHA };

            SetNewDistanceMetre(ref unitPorteeMaximaleModificationData);

            SetNewDistanceMetre(ref unitPorteeMaximaleTBAModificationData);

            SetNewDistanceMetre(ref unitPorteeMaximaleHAModificationData);
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
                ? GetNumberPercentage(WarnoConstants.SovGroundVisionPercentage, amunitionDistance.FloatValue)
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

        private static void SetNewDistanceMetre(ref UnitModificationDataDTO unitModificationDataDTO)
        {
            if (IsAllowedToChangeValue(unitModificationDataDTO.DistanceMetre, unitModificationDataDTO.RealFireRangeDistance))
            {
                if (unitModificationDataDTO.NerfRequired)
                    unitModificationDataDTO.RealFireRangeDistance = NerfDistanceV2(unitModificationDataDTO.RealFireRangeDistance, unitModificationDataDTO.DistanceMetre.FloatValue);

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

        //internal static float NerfDistance(float newValue, float originalValue)
        //{
        //    if (newValue <= originalValue)
        //        return originalValue;

        //    var difference = newValue - originalValue;

        //    var percentageoriginalValue = GetPercentageAfromB(originalValue, newValue);

        //    var repcentageTotal = difference / percentageoriginalValue;

        //    return (float)Math.Round((double)(newValue - repcentageTotal));
        //}

        internal static float NerfDistanceV2(float newValue, float originalValue)
        {
            if (newValue <= originalValue)
                return originalValue;

            var difference = newValue - originalValue;

            var percentageDifference = GetPercentageAfromB(difference, newValue);

            var percentageoriginalValue = GetPercentageAfromB(originalValue, newValue);

            var pcentageTotal = difference / percentageoriginalValue;

            var result = (newValue - pcentageTotal) / Math.Max(percentageDifference / percentageoriginalValue / 2.5, 1);

            //return (float)Math.Round((double)(newValue - repcentageTotal));
            return (float)Math.Round((double)result);
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
        }
    }
}
