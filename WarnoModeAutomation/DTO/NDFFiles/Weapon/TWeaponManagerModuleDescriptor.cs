using NDFSerialization.Models;
using NDFSerialization.NDFDataTypes;

namespace WarnoModeAutomation.DTO.NDFFiles.Weapon
{
    public class TWeaponManagerModuleDescriptor : Descriptor
    {
        public override Type Type => typeof(TWeaponManagerModuleDescriptor);

        public NDFVector TurretDescriptorList { get; set; }
    }
}
