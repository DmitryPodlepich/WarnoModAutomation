using JsonDatabase.DTO;
using NDFSerialization.Interfaces;
using NDFSerialization.Models;
using WarnoModeAutomation.DTO.NDFFiles;
using WarnoModeAutomation.DTO.NDFFiles.Ammunition;
using WarnoModeAutomation.DTO.NDFFiles.Desk;
using WarnoModeAutomation.DTO.NDFFiles.Weapon;
using WarnoModeAutomation.Logic.Services.Interfaces;

namespace WarnoModeAutomation.Logic.Services.Impl
{
    internal class NDFPreloader : INDFPreloader
    {
        public FileDescriptor<TAmmunitionDescriptor> Ammunition => NdfFiles[_settings.AmmunitionDescriptorsFileName] as FileDescriptor<TAmmunitionDescriptor>;
        public FileDescriptor<TAmmunitionDescriptor> AmmunitionMissiles => NdfFiles[_settings.AmmunitionMissilesDescriptorsFileName] as FileDescriptor<TAmmunitionDescriptor>;
        public FileDescriptor<TWeaponManagerModuleDescriptor> Weapons => NdfFiles[_settings.WeaponDescriptorDescriptorsFileName] as FileDescriptor<TWeaponManagerModuleDescriptor>;
        public FileDescriptor<TEntityDescriptor> Building => NdfFiles[_settings.BuildingDescriptorsFileName] as FileDescriptor<TEntityDescriptor>;
        public FileDescriptor<TEntityDescriptor> Units => NdfFiles[_settings.UniteDescriptorFileName] as FileDescriptor<TEntityDescriptor>;
        public FileDescriptor<TSupplyDescriptor> Ravitaillements => NdfFiles[_settings.RavitaillementFileName] as FileDescriptor<TSupplyDescriptor>;
        public FileDescriptor<TDeckDivisionDescriptor> Divisions => NdfFiles[_settings.DivisionsFileName] as FileDescriptor<TDeckDivisionDescriptor>;
        public FileDescriptor<TDeckDivisionRules> DeckDivisionRules => NdfFiles[_settings.DivisionRulesFileName] as FileDescriptor<TDeckDivisionRules>;

        public Dictionary<string, IFileDescriptor<Descriptor>> NdfFiles { get; private set; }

        public delegate void InitializedDel();
        public event INDFPreloader.InitializedDel OnInitialized;

        private SettingsDTO _settings;
        private readonly ISettingsManagerService _settingsManagerService;
        private CancellationToken _cancellationToken;

        public bool Initialized { get; private set; }
        private volatile bool _isInitializing;

        public NDFPreloader(ISettingsManagerService settingsManagerService)
        {
            _settingsManagerService = settingsManagerService;

            _ = Task.Run(Initialize);
        }

        public async Task Initialize()
        {
            if (!FileManager.IsModExist())
                return;

            if (Initialized || _isInitializing)
                return;

            _isInitializing = true;

            _cancellationToken = new CancellationToken();
            _settings = await _settingsManagerService.LoadSettingsAsync();

            var nfdFileNames = new string[]
            {
                _settings.AmmunitionDescriptorsFileName,
                _settings.AmmunitionMissilesDescriptorsFileName,
                _settings.WeaponDescriptorDescriptorsFileName, 
                _settings.UniteDescriptorFileName, 
                _settings.BuildingDescriptorsFileName,
                _settings.RavitaillementFileName, 
                _settings.DivisionsFileName,
                _settings.DivisionRulesFileName
            };

            NdfFiles = new Dictionary<string, IFileDescriptor<Descriptor>>(nfdFileNames.Length);

            _ = Parallel.ForEach(nfdFileNames, new ParallelOptions() { CancellationToken = _cancellationToken, MaxDegreeOfParallelism = Environment.ProcessorCount } ,(item) =>
            {
                try
                {
                    var filePath = FileManager.NDFFilesPaths.Single(f => f.FileName == item).FilePath;

                    if (item == _settings.AmmunitionDescriptorsFileName || item == _settings.AmmunitionMissilesDescriptorsFileName)
                        NdfFiles[item] = NDFSerializer.Deserialize<TAmmunitionDescriptor>(filePath, _cancellationToken);

                    else if (item == _settings.WeaponDescriptorDescriptorsFileName)
                        NdfFiles[item] = NDFSerializer.Deserialize<TWeaponManagerModuleDescriptor>(filePath, _cancellationToken);

                    else if (item == _settings.UniteDescriptorFileName || item == _settings.BuildingDescriptorsFileName)
                        NdfFiles[item] = NDFSerializer.Deserialize<TEntityDescriptor>(filePath, _cancellationToken);

                    else if (item == _settings.RavitaillementFileName)
                        NdfFiles[item] = NDFSerializer.Deserialize<TSupplyDescriptor>(filePath, _cancellationToken);

                    else if (item == _settings.DivisionsFileName)
                        NdfFiles[item] = NDFSerializer.Deserialize<TDeckDivisionDescriptor>(filePath, _cancellationToken);

                    else if(item == _settings.DivisionRulesFileName)
                        NdfFiles[item] = NDFSerializer.Deserialize<TDeckDivisionRules>(filePath, _cancellationToken);

                    else
                        throw new NotSupportedException($"File {item} has not supported ndf descriptor type.");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex);
                }
            });

            _isInitializing = false;
            Initialized = true;

            OnInitialized?.Invoke();
        }

        public void ResetInitialization()
        {
            if (_isInitializing)
                return;

            Initialized = false;
        }
    }
}
