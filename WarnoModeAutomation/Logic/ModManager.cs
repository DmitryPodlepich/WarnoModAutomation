﻿using JsonDatabase.DTO;
using WarnoModeAutomation.Logic.Providers.Interfaces;
using WarnoModeAutomation.Logic.Services.Impl;
using WarnoModeAutomation.Logic.Services.Interfaces;
using WebSearch;

namespace WarnoModeAutomation.Logic
{
    public class ModManager
    {
        private const string CreateNewModBatFileName = "CreateNewMod.bat";
        private const string GenerateModBatFileName = "GenerateMod.bat";
        private const string UpdateModBatFileName = "UpdateMod.bat";
        private const string GitInitializationCommand = "git init && git add . && git commit -m \"Initial commit\"";

        public delegate void Outputter(string data);
        public event Outputter OnOutput;

        private readonly ICMDProvider _CMDProvider;
        private readonly ISettingsManagerService _settingsManagerService;
        private readonly IWarnoModificationService _warnoModificationService;
        public ModManager(
            ICMDProvider cmdProvider,
            ISettingsManagerService settingsManagerService, 
            IWarnoModificationService warnoModificationService)
        {
            _settingsManagerService = settingsManagerService;
            _warnoModificationService = warnoModificationService;
            _warnoModificationService.OnOutput += OnCMDProviderOutput;

            _CMDProvider = cmdProvider;
            _CMDProvider.OnOutput += OnCMDProviderOutput;
        }

        public async Task<bool> CreateModAsync(CancellationTokenSource cancellationToken)
        {
            var batFullPath = Path.Combine(Storage.ModeSettings.ModsDirectory, CreateNewModBatFileName);

            if (!File.Exists(batFullPath))
            {
                OnOutput?.Invoke($"{batFullPath} file does not exist!");
                return false;
            }

            _CMDProvider.SetWorkingDirectory(Storage.ModeSettings.ModsDirectory);

            var creationResult = await _CMDProvider.PerformCMDCommand($"{CreateNewModBatFileName} {Storage.ModeSettings.ModName}", cancellationToken);

            if (!creationResult)
                return creationResult;

            var modFullPath = Path.Combine(Storage.ModeSettings.ModsDirectory, Storage.ModeSettings.ModName);

            return await _CMDProvider.PerformCMDCommand(GitInitializationCommand, cancellationToken, modFullPath);
        }

        public async Task<bool> DeleteMod() 
        {
            var modDirectory = Path.Combine(Storage.ModeSettings.ModsDirectory, Storage.ModeSettings.ModName);

            var modSavedGamesDirectory = Path.Combine(FileManager.SavedGamesEugenSystemsModPath, Storage.ModeSettings.ModName);

            var settings = await _settingsManagerService.LoadSettingsAsync();

            var savedGamesConfigFilePath = Path.Combine(FileManager.SavedGamesEugenSystemsModPath, settings.ConfigFileName);

            if (!File.Exists(savedGamesConfigFilePath))
            {
                OnOutput?.Invoke($"{savedGamesConfigFilePath} not found!");
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

        public async Task<bool> GenerateModAsync(CancellationTokenSource cancellationToken) 
        {
            var batFullPath = Path.Combine(Storage.ModeSettings.ModsDirectory, Storage.ModeSettings.ModName, GenerateModBatFileName);

            var modDirectory = Path.Combine(Storage.ModeSettings.ModsDirectory, Storage.ModeSettings.ModName);

            if (!File.Exists(batFullPath))
            {
                OnOutput?.Invoke($"{batFullPath} file does not exist!");
                return false;
            }

            _CMDProvider.SetWorkingDirectory(modDirectory);

            return await _CMDProvider.PerformCMDCommand(GenerateModBatFileName, cancellationToken);
        }

        public async Task UpdateModAsync(CancellationTokenSource cancellationToken) 
        {
            var batFullPath = Path.Combine(Storage.ModeSettings.ModsDirectory, Storage.ModeSettings.ModName, UpdateModBatFileName);

            var modDirectory = Path.Combine(Storage.ModeSettings.ModsDirectory, Storage.ModeSettings.ModName);

            if (!File.Exists(batFullPath))
            {
                OnOutput?.Invoke($"{batFullPath} file does not exist!");
            }

            _CMDProvider.SetWorkingDirectory(modDirectory);

            _ = await _CMDProvider.PerformCMDCommand(GenerateModBatFileName, cancellationToken);
        }

        public async Task FillDatabaseAsync(CancellationTokenSource cancellationTokenSource) 
        {
            WebSearchEngine.OnOutput += OnCMDProviderOutput;
            await WebSearchEngine.FillDatabaseWithMilitaryTodayAsync(cancellationTokenSource);
        }

        public async Task Modify(bool enableFullLog, CancellationToken cancellationToken)
        {
            var duplicatedAmmoNames = JsonDatabase.JsonDatabase.GetDuplicatedAmmoNames();

            if (duplicatedAmmoNames.Length > 0)
            {
                OnCMDProviderOutput("Modification validation error!");
                OnCMDProviderOutput("Amunition names duplicated detected:");

                foreach (var item in duplicatedAmmoNames)
                {
                    OnCMDProviderOutput(item);
                }

                return;
            }

            await _warnoModificationService.Modify(enableFullLog, cancellationToken);

            OnCMDProviderOutput("Modification successfully completed!");
        }

        private void OnCMDProviderOutput(string data)
        {
            OnOutput?.Invoke(data);
        }
    }
}
