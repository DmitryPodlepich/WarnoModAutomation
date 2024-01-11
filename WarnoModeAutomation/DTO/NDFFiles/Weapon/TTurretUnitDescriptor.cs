using NDFSerialization.Models;
using NDFSerialization.NDFDataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarnoModeAutomation.DTO.NDFFiles.Weapon.Interfaces;

namespace WarnoModeAutomation.DTO.NDFFiles.Weapon
{
    public class TTurretUnitDescriptor : Descriptor, ITTurretDescriptor
    {
        public override Type Type => typeof(TTurretUnitDescriptor);
        public NDFVector MountedWeaponDescriptorList { get; set; }
    }
}
