using CsQuery.ExtensionMethods;
using JsonDatabase.DTO;
using NDFSerialization.Interfaces;
using NDFSerialization.Models;
using NDFSerialization.NDFDataTypes;
using NDFSerialization.NDFDataTypes.Primitive;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using WarnoModeAutomation.Constants;
using WarnoModeAutomation.DTO;
using WarnoModeAutomation.DTO.NDFFiles;
using WarnoModeAutomation.DTO.NDFFiles.Ammunition;
using WarnoModeAutomation.DTO.NDFFiles.Desk;
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

        private const int SupplyCapacity = 96000;
        private SettingsDTO _settings;
        private readonly ISettingsManagerService _settingsManagerService;
        private readonly INDFPreloader _nDFPreloader;

        public WarnoModificationService(ISettingsManagerService settingsManagerService, INDFPreloader nDFPreloader)
        {
            _settingsManagerService = settingsManagerService;
            _nDFPreloader = nDFPreloader;
            _ = InitializeSettings();
        }

        public async Task Modify(bool enableFullLog, CancellationToken cancellationToken)
        {
            _settings = await _settingsManagerService.LoadSettingsAsync();

            await _nDFPreloader.Initialize();

            var amunitionFilePath = FileManager.NDFFilesPaths.SingleOrDefault(f => f.FileName == _settings.AmmunitionDescriptorsFileName);
            var amunitionMissilesFilePath = FileManager.NDFFilesPaths.SingleOrDefault(f => f.FileName == _settings.AmmunitionMissilesDescriptorsFileName);
            var weaponFilePath = FileManager.NDFFilesPaths.SingleOrDefault(f => f.FileName == _settings.WeaponDescriptorDescriptorsFileName);
            var unitsFilePath = FileManager.NDFFilesPaths.SingleOrDefault(f => f.FileName == _settings.UniteDescriptorFileName);
            var buildingsFilePath = FileManager.NDFFilesPaths.SingleOrDefault(f => f.FileName == _settings.BuildingDescriptorsFileName);
            var ravitaillementFilePath = FileManager.NDFFilesPaths.SingleOrDefault(f => f.FileName == _settings.RavitaillementFileName);
            var divisionsFilePath = FileManager.NDFFilesPaths.SingleOrDefault(f => f.FileName == _settings.DivisionsFileName);
            var divisionsRulesFilePath = FileManager.NDFFilesPaths.SingleOrDefault(f => f.FileName == _settings.DivisionRulesFileName);

            FileDescriptor<TEntityDescriptor> buildings = null;
            FileDescriptor<TSupplyDescriptor> ravitaillement = null;

            Task[] tasks =
            [
                Task.Run(() =>
                {
                    var unitsRelatedData = new UnitsRelatedDataDTO(_nDFPreloader.AmmunitionMissiles, _nDFPreloader.Ammunition, _nDFPreloader.Weapons, _nDFPreloader.Units);
                    ModifyUnits(unitsRelatedData, cancellationToken, enableFullLog);
                }, cancellationToken),
                Task.Run(() =>
                {
                    buildings = ModifyBuildings(_nDFPreloader.Building, cancellationToken);
                }, cancellationToken),
                Task.Run(() =>
                {
                    ravitaillement = ModifyRavitaillement(_nDFPreloader.Ravitaillements, cancellationToken);
                }, cancellationToken),
                Task.Run(async () =>
                {
                    await ModifyDivisionRules(_nDFPreloader, cancellationToken);
                }, cancellationToken),
            ];

            await Task.WhenAll(tasks);

            tasks =
            [
                Task.Run(async () => await File.WriteAllTextAsync(unitsFilePath.FilePath, NDFSerializer.Serialize(_nDFPreloader.Units, OnCMDProviderOutput), cancellationToken), cancellationToken),
                Task.Run(async () => await File.WriteAllTextAsync(amunitionMissilesFilePath.FilePath, NDFSerializer.Serialize(_nDFPreloader.AmmunitionMissiles, OnCMDProviderOutput), cancellationToken), cancellationToken),
                Task.Run(async () => await File.WriteAllTextAsync(amunitionFilePath.FilePath, NDFSerializer.Serialize(_nDFPreloader.Ammunition, OnCMDProviderOutput), cancellationToken), cancellationToken),
                Task.Run(async () => await File.WriteAllTextAsync(weaponFilePath.FilePath, NDFSerializer.Serialize(_nDFPreloader.Weapons, OnCMDProviderOutput), cancellationToken), cancellationToken),
                Task.Run(async () => await File.WriteAllTextAsync(buildingsFilePath.FilePath, NDFSerializer.Serialize(buildings, OnCMDProviderOutput), cancellationToken), cancellationToken),
                Task.Run(async () => await File.WriteAllTextAsync(ravitaillementFilePath.FilePath, NDFSerializer.Serialize(ravitaillement, OnCMDProviderOutput), cancellationToken), cancellationToken),
                //Task.Run(async () => await File.WriteAllTextAsync(divisionsFilePath.FilePath, NDFSerializer.Serialize(_nDFPreloader.Divisions, OnCMDProviderOutput), cancellationToken), cancellationToken),
                //Task.Run(async () => await File.WriteAllTextAsync(divisionsRulesFilePath.FilePath, NDFSerializer.Serialize(_nDFPreloader.DeckDivisionRules, OnCMDProviderOutput), cancellationToken), cancellationToken),
            ];

            await Task.WhenAll(tasks);
        }

        private async Task InitializeSettings()
        {
            _settings = await _settingsManagerService.LoadSettingsAsync();
        }

        private async Task ModifyDivisionRules(INDFPreloader preloader, CancellationToken cancellationToken)
        {
            var customDivisionRules = await JsonDatabase.JsonDatabase.LoadDivisionRulesAsync();

            try
            {
                var allAnonymousRulesObjects = preloader.DeckDivisionRules.RootDescriptors
                    .SelectMany(x => x.DivisionRules.OfType<AnonymousDivisionRule>());

                foreach (var rule in customDivisionRules) 
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var division = preloader.Divisions.RootDescriptors
                        .SingleOrDefault(x => x.CfgName == rule.DivisionName);

                    var anonymousRule = allAnonymousRulesObjects
                        .SingleOrDefault(x => x.Descriptor_Deck_Division.Value.Contains(rule.DivisionName, StringComparison.OrdinalIgnoreCase));

                    if (anonymousRule is null || division is null)
                        continue;

                    var newRuleToAdd = new TDeckUniteRule()
                    {
                        UnitDescriptor = $"$/GFX/Unit/Descriptor_{rule.UnitName}",
                        AvailableWithoutTransport = false,
                        NumberOfUnitInPack = 4,
                        NumberOfUnitInPackXPMultiplier = "[1.0, 1.0, 1.0, 1.0]",
                    };

                    anonymousRule.TDeckDivisionRule.UnitRuleList.Add(newRuleToAdd);
                    division.PackList.Add($"~/Descriptor_Deck_Pack_{rule.UnitName}", 4);

                    var unitRuleListKey = nameof(anonymousRule.TDeckDivisionRule.UnitRuleList);
                    var packListKey = nameof(division.PackList);

                    if (!preloader.DeckDivisionRules.NewRawLines.ContainsKey(unitRuleListKey))
                        preloader.DeckDivisionRules.NewRawLines.Add(unitRuleListKey, []);

                    if (!preloader.Divisions.NewRawLines.ContainsKey(packListKey))
                        preloader.Divisions.NewRawLines.Add(packListKey, []);

                    preloader.DeckDivisionRules.NewRawLines[unitRuleListKey].Add(newRuleToAdd.ToString());
                    preloader.Divisions.NewRawLines[packListKey].Add($"(~/Descriptor_Deck_Pack_{rule.UnitName.Replace("Unit_","")}, 4),");
                }
            }
            catch (Exception ex)
            {
                OnCMDProviderOutput($"ModifyDivisionRules exception: {ex.Message}.");
            }

            OnCMDProviderOutput("ModifyDivisionRules successfully finished.");
        }

        private FileDescriptor<TSupplyDescriptor> ModifyRavitaillement(FileDescriptor<TSupplyDescriptor> descriptor, CancellationToken cancellationToken, bool enableFullLog = false)
        {
            cancellationToken.ThrowIfCancellationRequested();

            const float commonSupplyCost = 10;
            const float commonSupplyBySecond = 10;
            const float healthSupplyBySecond = 0.10f;

            foreach (var supplyDescriptor in descriptor.RootDescriptors)
            {
                cancellationToken.ThrowIfCancellationRequested();

                supplyDescriptor.FuelSupplyBySecond = commonSupplyBySecond;
                supplyDescriptor.FuelSupplyCostBySecond = commonSupplyCost;

                supplyDescriptor.HealthSupplyBySecond = healthSupplyBySecond;
                supplyDescriptor.HealthSupplyCostBySecond = commonSupplyCost;

                supplyDescriptor.SupplySupplyBySecond = commonSupplyBySecond;
                supplyDescriptor.SupplySupplyCostBySecond = commonSupplyCost;

                supplyDescriptor.AmmunitionSupplyBySecond = commonSupplyBySecond;

                supplyDescriptor.CriticsSupplyBySecond = commonSupplyBySecond;
                supplyDescriptor.CriticsSupplyCostBySecond = commonSupplyCost;

                if(enableFullLog)
                    OnCMDProviderOutput($"ModifyRavitaillement successfully finished for: {supplyDescriptor.EntityNDFType}.");
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

                var weaponManagerModules = unitsRelatedData.WeaponManagerModuleDescriptor.RootDescriptors
                    .Where(w => unitWeaponModuleDescriptor.Default.Contains(w.EntityNDFType, StringComparison.InvariantCultureIgnoreCase)).ToArray();

                if (weaponManagerModules.Length > 1)
                {
                    OnCMDProviderOutput($"Find more than one weapon modules: {string.Join(',',weaponManagerModules.Select(x => x.EntityNDFType))} fro Unit: {unit.ClassNameForDebug}");
                }

                var weaponManagerModule = weaponManagerModules.FirstOrDefault();

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
                    }

                    if (enableFullLog)
                        OnCMDProviderOutput($"Unit: {unit.ClassNameForDebug} amunition {unitAmunition.EntityNDFType} modified!");
                }

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
                if (ammunition.DispersionAtMaxRangeGRU.FloatValue > 0)
                {
                    var additionalAccuracityValue = MathExtensions.GetNumberPercentage(_settings.NatoArtileryAccuracityBonusPercentage, ammunition.DispersionAtMaxRangeGRU.FloatValue);
                    var newAccuracityValue = ammunition.DispersionAtMaxRangeGRU.FloatValue - additionalAccuracityValue;
                    additionalResourceCommandPoints += (int)Math.Round(additionalAccuracityValue / _settings.AdditionalPointsArtileryCoefficientDivider);
                    ammunition.DispersionAtMaxRangeGRU.FloatValue = (float)Math.Round(newAccuracityValue);
                }

                if (ammunition.DispersionAtMinRangeGRU.FloatValue > 0)
                {
                    var additionalAccuracityValue = MathExtensions.GetNumberPercentage(_settings.NatoArtileryAccuracityBonusPercentage, ammunition.DispersionAtMinRangeGRU.FloatValue);
                    var newAccuracityValue = ammunition.DispersionAtMinRangeGRU.FloatValue - additionalAccuracityValue;
                    additionalResourceCommandPoints += (int)Math.Round(additionalAccuracityValue / _settings.AdditionalPointsArtileryCoefficientDivider);
                    ammunition.DispersionAtMinRangeGRU.FloatValue = (float)Math.Round(newAccuracityValue);
                }
            }

            return additionalResourceCommandPoints;
        }

        private bool EnsureModifiedArtileryDamage(TAmmunitionDescriptor ammunition)
        {
            if (ammunition.WeaponCursorType != _settings.ArtileryWeaponCursorType && !ammunition.EntityNDFType.Contains(_settings.AmunitionNameSMOKEMarker, StringComparison.InvariantCultureIgnoreCase))
                return false;

            ammunition.PhysicalDamages += MathExtensions.GetNumberPercentage(_settings.ArtileryDamagePercentage, ammunition.PhysicalDamages);
            ammunition.RadiusSplashPhysicalDamagesGRU.FloatValue += MathExtensions.GetNumberPercentage(_settings.ArtileryDamagePercentage, ammunition.RadiusSplashPhysicalDamagesGRU.FloatValue);
            ammunition.SuppressDamages += MathExtensions.GetNumberPercentage(_settings.ArtileryDamagePercentage, ammunition.SuppressDamages);
            ammunition.RadiusSplashSuppressDamagesGRU.FloatValue += MathExtensions.GetNumberPercentage(_settings.ArtileryDamagePercentage, ammunition.RadiusSplashSuppressDamagesGRU.FloatValue);

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

        /// <summary>
        /// Updates unit <see cref="TScannerConfigurationDescriptor" /> with formula: percent - max amunition distance.
        /// </summary>
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

            foreach (var opticalStrenghts in scannerConfigurationDescriptor.OpticalStrengths)
            {
                opticalStrenghts.Value.FloatValue = isSovUnit 
                    ? Math.Max(opticalStrenghts.Value.FloatValue, _settings.SovMinOpticalStrength)
                    : Math.Max(opticalStrenghts.Value.FloatValue, _settings.NatoMinOpticalStrength);
            }

            var longestGroundAmunitionRange = ammunitionDescriptor.Where(x => x.PorteeMaximaleGRU != null).Select(x => x.PorteeMaximaleGRU).MaxBy(x => x.FloatValue);

            var longestTBAAmunitionRange = ammunitionDescriptor.Where(x => x.PorteeMaximaleTBAGRU != null).Select(x => x.PorteeMaximaleTBAGRU).MaxBy(x => x.FloatValue);

            var longestHAAmunitionRange = ammunitionDescriptor.Where(x => x.PorteeMaximaleHAGRU != null).Select(x => x.PorteeMaximaleHAGRU).MaxBy(x => x.FloatValue);

            var minVisionDistance = isSovUnit ? _settings.SovMinVisionDistance : _settings.NatoMinVisionDistance;

            if (longestHAAmunitionRange != null || longestTBAAmunitionRange != null)
            {
                float calculatedUnitVisionTBA = longestTBAAmunitionRange != null ? CalculateUnitVisionTBA(longestTBAAmunitionRange, isSovUnit) : 0.0f;
                float calculatedUnitVisionHA = longestHAAmunitionRange != null ? CalculateUnitVisionHA(longestHAAmunitionRange, isSovUnit) : 0.0f;

                var newTBAVisionValue = Math.Max(calculatedUnitVisionTBA, calculatedUnitVisionHA);

                if (scannerConfigurationDescriptor.VisionRangesGRU.TryGetValue("EVisionUnitType/HighAltitude", out DistanceMetre highAltitudevalue)
                    && highAltitudevalue.FloatValue > 0)
                {
                    highAltitudevalue.FloatValue = Math.Max(minVisionDistance, newTBAVisionValue);
                }

                if (scannerConfigurationDescriptor.VisionRangesGRU.TryGetValue("EVisionUnitType/LowAltitude", out DistanceMetre lowAltitudevalue)
                    && lowAltitudevalue.FloatValue > 0)
                {
                    lowAltitudevalue.FloatValue = Math.Max(minVisionDistance, newTBAVisionValue);
                }

            }

            if(longestGroundAmunitionRange != null 
                && scannerConfigurationDescriptor.VisionRangesGRU.TryGetValue("EVisionUnitType/Standard", out DistanceMetre value)
                && value.FloatValue > 0)
            {
                var newGroundVisionValue = Math.Max(value.FloatValue, CalculateUnitVisionGround(longestGroundAmunitionRange, isSovUnit));

                value.FloatValue = Math.Max(minVisionDistance, newGroundVisionValue);
            }
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