using NDFSerialization.Models;
using NDFSerialization.NDFDataTypes;
using WarnoModeAutomation.DTO.NDFFiles.Weapon.Interfaces;

namespace WarnoModeAutomation.DTO.NDFFiles.Weapon
{
    public class TTurretBombardierDescriptor : Descriptor, ITTurretDescriptor
    {
        public override Type Type => typeof(TTurretBombardierDescriptor);

        public NDFVector MountedWeaponDescriptorList { get; set; }
    }
}