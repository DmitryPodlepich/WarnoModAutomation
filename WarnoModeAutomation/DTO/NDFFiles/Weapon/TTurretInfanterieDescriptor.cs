using NDFSerialization.Models;
using NDFSerialization.NDFDataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarnoModeAutomation.DTO.NDFFiles.Weapon
{
    public class TTurretInfanterieDescriptor : Descriptor
    {
        public override Type Type => typeof(TTurretInfanterieDescriptor);

        public NDFVector MountedWeaponDescriptorList { get; set; }
    }
}
