using NDFSerialization.Models;

namespace WarnoModeAutomation.DTO.NDFFiles.Weapon
{
    public class TMountedWeaponDescriptor : Descriptor
    {
        public override Type Type => typeof(TMountedWeaponDescriptor);

        public string Ammunition { get; set; }
    }
}
