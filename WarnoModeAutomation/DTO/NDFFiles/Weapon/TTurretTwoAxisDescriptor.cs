using NDFSerialization.Models;
using NDFSerialization.NDFDataTypes;
using WarnoModeAutomation.DTO.NDFFiles.Weapon.Interfaces;

namespace WarnoModeAutomation.DTO.NDFFiles.Weapon
{
    public class TTurretTwoAxisDescriptor : Descriptor, ITTurretDescriptor
    {
        public override Type Type => typeof(TTurretTwoAxisDescriptor);

        public NDFVector MountedWeaponDescriptorList { get; set; }
    }
}