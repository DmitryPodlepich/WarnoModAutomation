using NDFSerialization.Interfaces;
using NDFSerialization.Models;
using WarnoModeAutomation.DTO.NDFFiles;
using WarnoModeAutomation.DTO.NDFFiles.Ammunition;
using WarnoModeAutomation.DTO.NDFFiles.Desk;
using WarnoModeAutomation.DTO.NDFFiles.Weapon;

namespace WarnoModeAutomation.Logic.Services.Interfaces
{
    public interface INDFPreloader
    {
        FileDescriptor<TAmmunitionDescriptor> Ammunition { get; }
        FileDescriptor<TAmmunitionDescriptor> AmmunitionMissiles { get; }
        FileDescriptor<TWeaponManagerModuleDescriptor> Weapons { get; }
        FileDescriptor<TEntityDescriptor> Building { get; }
        FileDescriptor<TEntityDescriptor> Units { get; }
        FileDescriptor<TSupplyDescriptor> Ravitaillements { get; }
        FileDescriptor<TDeckDivisionDescriptor> Divisions { get; }
        Dictionary<string, IFileDescriptor<Descriptor>> NdfFiles { get; }
        public bool Initialized { get; }

        delegate void InitializedDel();
        event InitializedDel OnInitialized;

        Task Initialize();
    }
}
