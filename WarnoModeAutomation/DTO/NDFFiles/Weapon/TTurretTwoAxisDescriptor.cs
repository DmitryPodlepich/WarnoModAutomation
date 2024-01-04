using NDFSerialization.Models;
using NDFSerialization.NDFDataTypes;

namespace WarnoModeAutomation.DTO.NDFFiles.Weapon
{
    public class TTurretTwoAxisDescriptor : Descriptor
    {
        public override Type Type => typeof(TTurretTwoAxisDescriptor);

        public NDFVector MountedWeaponDescriptorList { get; set; }
    }
}