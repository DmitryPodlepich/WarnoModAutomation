using NDFSerialization.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarnoModeAutomation.DTO.NDFFiles
{
    public class TTypeUnitModuleDescriptor : Descriptor
    {
        public string AcknowUnitType { get; set; }
        public string MotherCountry { get; set; }
        public override Type Type => typeof(TTypeUnitModuleDescriptor);
    }
}
