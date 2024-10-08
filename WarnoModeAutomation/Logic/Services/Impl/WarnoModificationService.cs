using JsonDatabase.DTO;
using NDFSerialization.Models;
using NDFSerialization.NDFDataTypes.Primitive;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using WarnoModeAutomation.Constants;
using WarnoModeAutomation.DTO;
using WarnoModeAutomation.DTO.NDFFiles;
using WarnoModeAutomation.DTO.NDFFiles.Ammunition;
using WarnoModeAutomation.DTO.NDFFiles.Weapon;
using WarnoModeAutomation.DTO.NDFFiles.Weapon.Interfaces;
using WarnoModeAutomation.Extensions;
using WarnoModeAutomation.Logic.Services.Interfaces;

[assembly: InternalsVisibleTo("nUnitTests")]
namespace WarnoModeAutomation.Logic.Services.Impl
{
    public class WarnoModificationService : IWarnoModificationService
    {
        public delegate void Outputter(string data);
        public event IWarnoModificationService.Outputter OnOutput;

        private const int SupplyCapacity = 46000;
        private SettingsDTO _settings;
        private readonly ISettingsManagerService _settingsManagerService;

        public WarnoModificationService(ISettingsManagerService settingsManagerService)
        {
            _settingsManagerService = settingsManagerService;
            _ = InitializeSettings();
        }

        public async Task Modify(bool enableFullLog, CancellationToken cancellationToken)
        {
            _settings = await _settingsManagerService.LoadSettingsAsync();

            var amunitionFilePath = FileManager.NDFFilesPaths.SingleOrDefault(f => f.FileName == _settings.AmmunitionDescriptorsFileName);
            var amunitionMissilesFilePath = FileManager.NDFFilesPaths.SingleOrDefault(f => f.FileName == _settings.AmmunitionMissilesDescriptorsFileName);
            var weaponFilePath = FileManager.NDFFilesPaths.SingleOrDefault(f => f.FileName == _settings.WeaponDescriptorDescriptorsFileName);
            var unitsFilePath = FileManager.NDFFilesPaths.SingleOrDefault(f => f.FileName == _settings.UniteDescriptorFileName);
            var buildingsFilePath = FileManager.NDFFilesPaths.SingleOrDefault(f => f.FileName == _settings.BuildingDescriptorsFileName);
            var ravitaillementFilePath = FileManager.NDFFilesPaths.SingleOrDefault(f => f.FileName == _settings.RavitaillementFileName);

            FileDescriptor<TAmmunitionDescriptor> amunitionMissiles = null;
            FileDescriptor<TAmmunitionDescriptor> amunition = null;
            FileDescriptor<TWeaponManagerModuleDescriptor> weapon = null;
            FileDescriptor<TEntityDescriptor> units = null;
            FileDescriptor<TEntityDescriptor> buildings = null;
            FileDescriptor<TSupplyDescriptor> ravitaillement = null;

            Task[] tasks =
            [
                Task.Run(() =>
                {
                    amunitionMissiles = NDFSerializer.Deserialize<TAmmunitionDescriptor>(amunitionMissilesFilePath.FilePath, cancellationToken, OnCMDProviderOutput);

                    if (enableFullLog)
                        OnCMDProviderOutput($"{amunitionMissilesFilePath.FileName} desirialized!");
                }, cancellationToken),
                Task.Run(() =>
                {
                    amunition = NDFSerializer.Deserialize<TAmmunitionDescriptor>(amunitionFilePath.FilePath, cancellationToken, OnCMDProviderOutput);

                    if (enableFullLog)
                        OnCMDProviderOutput($"{amunitionFilePath.FileName} desirialized!");
                }, cancellationToken),
                Task.Run(() =>
                {
                    weapon = NDFSerializer.Deserialize<TWeaponManagerModuleDescriptor>(weaponFilePath.FilePath, cancellationToken, OnCMDProviderOutput);

                    if (enableFullLog)
                        OnCMDProviderOutput($"{weaponFilePath.FileName} desirialized!");
                }, cancellationToken),
                Task.Run(() =>
                {
                    units = NDFSerializer.Deserialize<TEntityDescriptor>(unitsFilePath.FilePath, cancellationToken, OnCMDProviderOutput);

                    if (enableFullLog)
                        OnCMDProviderOutput($"{unitsFilePath.FileName} desirialized!");
                }, cancellationToken),
                Task.Run(() =>
                {
                    buildings = NDFSerializer.Deserialize<TEntityDescriptor>(buildingsFilePath.FilePath, cancellationToken, OnCMDProviderOutput);

                    if (enableFullLog)
                        OnCMDProviderOutput($"{buildingsFilePath.FileName} desirialized!");
                }, cancellationToken),
                Task.Run(() =>
                {
                    ravitaillement = NDFSerializer.Deserialize<TSupplyDescriptor>(ravitaillementFilePath.FilePath, cancellationToken, OnCMDProviderOutput);

                    if (enableFullLog)
                        OnCMDProviderOutput($"{ravitaillementFilePath.FileName} desirialized!");
                }, cancellationToken)
            ];

            await Task.WhenAll(tasks);

            tasks =
            [
                Task.Run(() =>
                {
                    var unitsRelatedData = new UnitsRelatedDataDTO(amunitionMissiles, amunition, weapon, units);
                    ModifyUnits(unitsRelatedData, cancellationToken, enableFullLog);
                }, cancellationToken),
                Task.Run(() =>
                {
                    buildings = ModifyBuildings(buildings, cancellationToken);
                }, cancellationToken),
                Task.Run(() =>
                {
                    ravitaillement = ModifyRavitaillement(ravitaillement, cancellationToken);
                }, cancellationToken)
            ];

            await Task.WhenAll(tasks);

            tasks =
            [
                Task.Run(async () => await File.WriteAllTextAsync(unitsFilePath.FilePath, NDFSerializer.Serialize(units, OnCMDProviderOutput), cancellationToken), cancellationToken),
                Task.Run(async () => await File.WriteAllTextAsync(amunitionMissiles.FilePath, NDFSerializer.Serialize(amunitionMissiles, OnCMDProviderOutput), cancellationToken), cancellationToken),
                Task.Run(async () => await File.WriteAllTextAsync(amunitionFilePath.FilePath, NDFSerializer.Serialize(amunition, OnCMDProviderOutput), cancellationToken), cancellationToken),
                Task.Run(async () => await File.WriteAllTextAsync(weaponFilePath.FilePath, NDFSerializer.Serialize(weapon, OnCMDProviderOutput), cancellationToken), cancellationToken),
                Task.Run(async () => await File.WriteAllTextAsync(buildingsFilePath.FilePath, NDFSerializer.Serialize(buildings, OnCMDProviderOutput), cancellationToken), cancellationToken),
                Task.Run(async () => await File.WriteAllTextAsync(ravitaillementFilePath.FilePath, NDFSerializer.Serialize(ravitaillement, OnCMDProviderOutput), cancellationToken), cancellationToken),
            ];

            await Task.WhenAll(tasks);
        }

        private async Task InitializeSettings()
        {
            _settings = await _settingsManagerService.LoadSettingsAsync();
        }

        private FileDescriptor<TSupplyDescriptor> ModifyRavitaillement(FileDescriptor<TSupplyDescriptor> descriptor, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            const float commonSupplyCost = 10;
            const float commonSupplyBySecond = 10;

            foreach (var supplyDescriptor in descriptor.RootDescriptors)
            {
                cancellationToken.ThrowIfCancellationRequested();

                supplyDescriptor.FuelSupplyBySecond = commonSupplyBySecond;
                supplyDescriptor.FuelSupplyCostBySecond = commonSupplyCost;

                supplyDescriptor.HealthSupplyBySecond = 0.1f;
                supplyDescriptor.HealthSupplyCostBySecond = commonSupplyCost;

                supplyDescriptor.SupplySupplyBySecond = commonSupplyBySecond;
                supplyDescriptor.SupplySupplyCostBySecond = commonSupplyCost;

                supplyDescriptor.AmmunitionSupplyBySecond = commonSupplyBySecond;

                supplyDescriptor.CriticsSupplyBySecond = commonSupplyBySecond;
                supplyDescriptor.CriticsSupplyCostBySecond = commonSupplyCost;
            }

            return descriptor;
        }

        private FileDescriptor<TEntityDescriptor> ModifyBuildings(FileDescriptor<TEntityDescriptor> buildingsFileDescriptor, CancellationToken cancellationToken)
        {
            foreach (var entityDescriptor in buildingsFileDescriptor.RootDescriptors)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var tSupplyModuleDescriptor = entityDescriptor.ModulesDescriptors.OfType<TSupplyModuleDescriptor>().SingleOrDefault();

                if (tSupplyModuleDescriptor is null)
                {
                    OnCMDProviderOutput($"TSupplyModuleDescriptor not found for: {entityDescriptor.ClassNameForDebug}");
                    continue;
                }

                tSupplyModuleDescriptor.SupplyCapacity = SupplyCapacity;
            }

            return buildingsFileDescriptor;
        }

        private void ModifyUnits(UnitsRelatedDataDTO unitsRelatedData, CancellationToken cancellationToken, bool enableFullLog = false)
        {
            var modifiedAmunition = new Dictionary<string, int>();

            foreach (var unit in unitsRelatedData.UnitsEntityDescriptor.RootDescriptors)
            {
                cancellationToken.ThrowIfCancellationRequested();

                ModifyUnit(unit, unitsRelatedData, ref modifiedAmunition, cancellationToken, enableFullLog);
            }
        }

        private void ModifyUnit(TEntityDescriptor unit, UnitsRelatedDataDTO unitsRelatedData, ref Dictionary<string, int> modifiedAmunition, CancellationToken cancellationToken, bool enableFullLog = false)
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
                    .Select(w => {
                        var array = w.Ammunition.Split('/').ToArray();
                        return array[^1].Trim();
                    });

                var unitAmunitions = unitsRelatedData.AmmunitionDescriptor.RootDescriptors
                    .Where(d => unitAmunitionNames.Contains(d.EntityNDFType.Trim()));

                var unitMissilesAmunition = unitsRelatedData.AmmunitionMissilesDescriptor.RootDescriptors
                    .Where(d => unitAmunitionNames.Contains(d.EntityNDFType.Trim()));

                unitAmunitions = unitAmunitions.Union(unitMissilesAmunition);

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

                var resourceCommandPointsKey = tProductionModuleDescriptors.ProductionRessourcesNeeded.Keys.SingleOrDefault(x => x.Contains(_settings.ResourceCommandPoints, StringComparison.OrdinalIgnoreCase));

                if (resourceCommandPointsKey == null || !tProductionModuleDescriptors.ProductionRessourcesNeeded.TryGetValue(resourceCommandPointsKey, out int resourceCommandPoints))
                {
                    OnCMDProviderOutput($"Cannot find {_settings.ResourceCommandPoints} for unit: {unit.ClassNameForDebug}");
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
                    tagsModuleDescriptor.TagSet.Any(tag => _settings.TagsCombinationWithNerf.Contains(tag))
                    && !tagsModuleDescriptor.TagSet.Contains(_settings.InfanterieTag);

                bool unitAmunitionsHasDistanceChanges = false;

                var additionalCommandPoins = 0;

                foreach (var unitAmunition in unitAmunitions)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (modifiedAmunition.TryGetValue(unitAmunition.EntityNDFType, out int value))
                    {
                        additionalCommandPoins += value;
                        continue;
                    }

                    if (EnsureModifiedArtileryDamage(unitAmunition) && enableFullLog)
                        OnCMDProviderOutput($"ArtileryDamage for: {unit.ClassNameForDebug}. unitAmunition: {unitAmunition.EntityNDFType} has been increased by {_settings.ArtileryDamagePercentage} %");

                    if (!tTypeUnitModuleDescriptor.IsSovUnit())
                    {
                        var accuracityAdditionalCommandPoins = ModifyAmunitionAccuracity(unitAmunition);

                        modifiedAmunition.Add(unitAmunition.EntityNDFType, accuracityAdditionalCommandPoins);

                        additionalCommandPoins += accuracityAdditionalCommandPoins;
                    }

                    var realFireRange = JsonDatabase.JsonDatabase.FindAmmoRange(unitAmunition.EntityNDFType);

                    if (realFireRange is not null)
                    {
                        if (!unitAmunition.HasAnyAmmunitionDistance())
                        {
                            OnCMDProviderOutput($"Unit: {unit.ClassNameForDebug} amunition {unitAmunition.EntityNDFType} has no AmmunitionDistance data!");
                            continue;
                        }

                        ModifyAmunitionDistance(resourceCommandPoints, unitAmunition, realFireRange, shouldNerf);

                        if (enableFullLog)
                            OnCMDProviderOutput($"Fire range for unit: {unit.ClassNameForDebug}. unitAmunition: {unitAmunition.EntityNDFType} has been changed!");

                        unitAmunitionsHasDistanceChanges = true;
                    }

                    if (enableFullLog)
                        OnCMDProviderOutput($"Unit: {unit.ClassNameForDebug} amunition {unitAmunition.EntityNDFType} modified!");
                }

                if (unitAmunitionsHasDistanceChanges)
                    UpdateUnitVisionByAmunitionDistance(unit, unitAmunitions);

                if (additionalCommandPoins > 0)
                    tProductionModuleDescriptors.ProductionRessourcesNeeded[resourceCommandPointsKey] += additionalCommandPoins.RoundOff();

            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                OnCMDProviderOutput($"ModifyUnit exception: {unit.ClassNameForDebug} message {ex.Message}");
                OnCMDProviderOutput(ex.StackTrace);
                throw;
            }
        }

        private int ModifyAmunitionAccuracity(TAmmunitionDescriptor ammunition)
        {
            int additionalResourceCommandPoints = 0;

            if (ammunition.EntityNDFType.Contains(_settings.AmunitionNameSMOKEMarker, StringComparison.InvariantCultureIgnoreCase)
                || ammunition.WeaponCursorType == _settings.Weapon_Cursor_MachineGun)
                return additionalResourceCommandPoints;

            foreach (var baseHitValueModifier in ammunition.HitRollRuleDescriptor.BaseHitValueModifiers)
            {
                if (baseHitValueModifier.Value > 0)
                {
                    var additionalAccuracityPercentage = MathExtensions.GetNumberPercentage(_settings.NatoCommonAccuracityBonusPercentage, baseHitValueModifier.Value);
                    var newAccuracityValue = Math.Min(baseHitValueModifier.Value + additionalAccuracityPercentage, WarnoConstants.MaxAccuracity);
                    additionalResourceCommandPoints += (int)Math.Round(additionalAccuracityPercentage * _settings.AdditionalPointsCoefficientMultiplier);
                    ammunition.HitRollRuleDescriptor.BaseHitValueModifiers[baseHitValueModifier.Key] = (float)Math.Round(newAccuracityValue);
                }
            }

            if (ammunition.WeaponCursorType == _settings.ArtileryWeaponCursorType)
            {
                if (ammunition.DispersionAtMaxRange.FloatValue > 0)
                {
                    var additionalAccuracityValue = MathExtensions.GetNumberPercentage(_settings.NatoArtileryAccuracityBonusPercentage, ammunition.DispersionAtMaxRange.FloatValue);
                    var newAccuracityValue = ammunition.DispersionAtMaxRange.FloatValue - additionalAccuracityValue;
                    additionalResourceCommandPoints += (int)Math.Round(additionalAccuracityValue / _settings.AdditionalPointsArtileryCoefficientDivider);
                    ammunition.DispersionAtMaxRange.FloatValue = (float)Math.Round(newAccuracityValue);
                }

                if (ammunition.DispersionAtMinRange.FloatValue > 0)
                {
                    var additionalAccuracityValue = MathExtensions.GetNumberPercentage(_settings.NatoArtileryAccuracityBonusPercentage, ammunition.DispersionAtMinRange.FloatValue);
                    var newAccuracityValue = ammunition.DispersionAtMinRange.FloatValue - additionalAccuracityValue;
                    additionalResourceCommandPoints += (int)Math.Round(additionalAccuracityValue / _settings.AdditionalPointsArtileryCoefficientDivider);
                    ammunition.DispersionAtMinRange.FloatValue = (float)Math.Round(newAccuracityValue);
                }
            }

            return additionalResourceCommandPoints;
        }

        private bool EnsureModifiedArtileryDamage(TAmmunitionDescriptor ammunition)
        {
            if (ammunition.WeaponCursorType != _settings.ArtileryWeaponCursorType && !ammunition.EntityNDFType.Contains(_settings.AmunitionNameSMOKEMarker, StringComparison.InvariantCultureIgnoreCase))
                return false;

            ammunition.PhysicalDamages += MathExtensions.GetNumberPercentage(_settings.ArtileryDamagePercentage, ammunition.PhysicalDamages);
            ammunition.RadiusSplashPhysicalDamages.FloatValue += MathExtensions.GetNumberPercentage(_settings.ArtileryDamagePercentage, ammunition.RadiusSplashPhysicalDamages.FloatValue);
            ammunition.SuppressDamages += MathExtensions.GetNumberPercentage(_settings.ArtileryDamagePercentage, ammunition.SuppressDamages);
            ammunition.RadiusSplashSuppressDamages.FloatValue += MathExtensions.GetNumberPercentage(_settings.ArtileryDamagePercentage, ammunition.RadiusSplashSuppressDamages.FloatValue);

            return true;
        }

        private void ModifyAmunitionDistance(int originalResourceCommandPoints, TAmmunitionDescriptor ammunition, AmmoRangeDTO ammoRangeDTO, bool shouldNerf)
        {
            var realDistanceInMetersToWarnoDistance = MathExtensions.ConverToWarnoDistance(ammoRangeDTO.FireRangeInMeters);

            var unitPorteeMaximaleModificationData = new UnitModificationDataDto(
                ammunition.PorteeMaximaleGRU,
                ammunition,
                originalResourceCommandPoints,
                realDistanceInMetersToWarnoDistance,
                shouldNerf);

            var unitPorteeMaximaleTBAModificationData = unitPorteeMaximaleModificationData with { DistanceMetre = ammunition.PorteeMaximaleTBAGRU };

            var unitPorteeMaximaleHAModificationData = unitPorteeMaximaleModificationData with { DistanceMetre = ammunition.PorteeMaximaleHAGRU };

            SetNewDistanceMetre(ref unitPorteeMaximaleModificationData);

            SetNewDistanceMetre(ref unitPorteeMaximaleTBAModificationData);

            SetNewDistanceMetre(ref unitPorteeMaximaleHAModificationData);
        }

        private void UpdateUnitVisionByAmunitionDistance(TEntityDescriptor unit, IEnumerable<TAmmunitionDescriptor> ammunitionDescriptor)
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

            var isSovUnit = tTypeUnitModuleDescriptor.IsSovUnit();

            if (scannerConfigurationDescriptor.OpticalStrength <= 0)
            {
                scannerConfigurationDescriptor.OpticalStrength = isSovUnit ? _settings.SovMinOpticalStrength : _settings.NatoMinOpticalStrength;
            }

            var longestGroundAmunitionRange = ammunitionDescriptor.Where(x => x.PorteeMaximaleGRU != null).Select(x => x.PorteeMaximaleGRU).MaxBy(x => x.FloatValue);

            var longestTBAAmunitionRange = ammunitionDescriptor.Where(x => x.PorteeMaximaleTBAGRU != null).Select(x => x.PorteeMaximaleTBAGRU).MaxBy(x => x.FloatValue);

            var longestHAAmunitionRange = ammunitionDescriptor.Where(x => x.PorteeMaximaleHAGRU != null).Select(x => x.PorteeMaximaleHAGRU).MaxBy(x => x.FloatValue);

            var minVisionDistance = isSovUnit ? _settings.SovMinVisionDistance : _settings.NatoMinVisionDistance;

            if (longestHAAmunitionRange != null)
            {
                var newTBAVisionValue = Math.Max(CalculateUnitVisionTBA(longestTBAAmunitionRange, isSovUnit), CalculateUnitVisionHA(longestHAAmunitionRange, isSovUnit));
                scannerConfigurationDescriptor.PorteeVisionTBAGRU.FloatValue = Math.Max(minVisionDistance, newTBAVisionValue);
            }

            var newDetectionValue = CalculateUnitDetectionTBAAndHA(scannerConfigurationDescriptor.PorteeVisionTBAGRU.FloatValue);
            scannerConfigurationDescriptor.DetectionTBAGRU.FloatValue = Math.Max(minVisionDistance, newDetectionValue);

            if (longestGroundAmunitionRange != null)
            {
                var newGroundVisionValue = Math.Max(scannerConfigurationDescriptor.PorteeVisionGRU.FloatValue, CalculateUnitVisionGround(longestGroundAmunitionRange, isSovUnit));
                scannerConfigurationDescriptor.PorteeVisionGRU.FloatValue = Math.Max(minVisionDistance, newGroundVisionValue);
            }
        }

        private float CalculateUnitDetectionTBAAndHA(float visionDistance)
        {
            var percentage = MathExtensions.GetNumberPercentage(_settings.TBAAndHADetectionPercentageFromVisionDistance, visionDistance);

            return visionDistance - percentage;
        }

        private float CalculateUnitVisionGround(DistanceMetre amunitionDistance, bool isSovUnit)
        {
            var percentage = isSovUnit
                ? MathExtensions.GetNumberPercentage(_settings.SovGroundVisionPercentageFromAmunitionDistance, amunitionDistance.FloatValue)
                : MathExtensions.GetNumberPercentage(_settings.NatoGroundVisionPercentageFromAmunitionDistance, amunitionDistance.FloatValue);

            return amunitionDistance.FloatValue - percentage;
        }

        private float CalculateUnitVisionTBA(DistanceMetre amunitionDistance, bool isSovUnit)
        {
            var percentage = isSovUnit
                ? MathExtensions.GetNumberPercentage(_settings.SovTBAVisionPercentageFromAmunitionDistance, amunitionDistance.FloatValue)
                : MathExtensions.GetNumberPercentage(_settings.NatoTBAVisionPercentageFromAmunitionDistance, amunitionDistance.FloatValue);

            return amunitionDistance.FloatValue - percentage;
        }

        private float CalculateUnitVisionHA(DistanceMetre amunitionDistance, bool isSovUnit)
        {
            var percentage = isSovUnit
                ? MathExtensions.GetNumberPercentage(_settings.SovHAVisionPercentageFromAmunitionDistance, amunitionDistance.FloatValue)
                : MathExtensions.GetNumberPercentage(_settings.NatoHAVisionPercentageFromAmunitionDistance, amunitionDistance.FloatValue);

            return amunitionDistance.FloatValue - percentage;
        }

        private void SetNewDistanceMetre(ref UnitModificationDataDto unitModificationDataDTO)
        {
            if (unitModificationDataDTO.DistanceMetre != null
                && IsAllowedToChangeValue(unitModificationDataDTO.DistanceMetre, unitModificationDataDTO.RealFireRangeDistance))
            {
                if (unitModificationDataDTO.NerfRequired)
                    unitModificationDataDTO.RealFireRangeDistance = NerfDistance(unitModificationDataDTO.RealFireRangeDistance, unitModificationDataDTO.DistanceMetre.FloatValue);

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

        internal float NerfDistance(float newValue, float originalValue)
        {
            if (newValue <= originalValue)
                return originalValue;

            var difference = newValue - originalValue;

            var percentageDifference = MathExtensions.GetPercentageAfromB(difference, newValue);

            var percentageoriginalValue = MathExtensions.GetPercentageAfromB(originalValue, newValue);

            var pcentageTotal = difference / percentageoriginalValue;

            var result = (newValue - pcentageTotal) / Math.Max(percentageDifference / percentageoriginalValue / _settings.NerfDistanceCoefficientDivider, 1);

            return (float)Math.Round(result);
        }

        private void OnCMDProviderOutput(string data)
        {
            OnOutput?.Invoke(data);
        }
    }
}