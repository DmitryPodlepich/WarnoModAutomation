using NDFSerialization.Models;
using WarnoModeAutomation.DTO.NDFFiles;
using WarnoModeAutomation.DTO.NDFFiles.Ammunition;
using WarnoModeAutomation.DTO.NDFFiles.Weapon;

namespace WarnoModeAutomation.DTO
{
    public class UnitsRelatedDataDTO(
        FileDescriptor<TAmmunitionDescriptor> ammunitionMissilesDescriptor, 
        FileDescriptor<TAmmunitionDescriptor> ammunitionDescriptor, 
        FileDescriptor<TWeaponManagerModuleDescriptor> weaponManagerModuleDescriptor,
        FileDescriptor<TEntityDescriptor> unitsEntityDescriptor)
    {
        public readonly FileDescriptor<TAmmunitionDescriptor> AmmunitionMissilesDescriptor = ammunitionMissilesDescriptor;
        public readonly FileDescriptor<TAmmunitionDescriptor> AmmunitionDescriptor = ammunitionDescriptor;
        public readonly FileDescriptor<TWeaponManagerModuleDescriptor> WeaponManagerModuleDescriptor = weaponManagerModuleDescriptor;
        public readonly FileDescriptor<TEntityDescriptor> UnitsEntityDescriptor = unitsEntityDescriptor;
    }
}