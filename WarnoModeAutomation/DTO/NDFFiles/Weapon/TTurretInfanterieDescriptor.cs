﻿using NDFSerialization.Models;
using NDFSerialization.NDFDataTypes;
using WarnoModeAutomation.DTO.NDFFiles.Weapon.Interfaces;

namespace WarnoModeAutomation.DTO.NDFFiles.Weapon
{
    public class TTurretInfanterieDescriptor : Descriptor, ITTurretDescriptor
    {
        public override Type Type => typeof(TTurretInfanterieDescriptor);

        public NDFVector MountedWeaponDescriptorList { get; set; }
    }
}
