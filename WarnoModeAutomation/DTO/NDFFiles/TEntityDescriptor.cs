using NDFSerialization.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarnoModeAutomation.DTO.NDFFiles
{
    public class TEntityDescriptor : Descriptor
    {
        public string ClassNameForDebug { get; set; }

        //ToDo: create own collection Vector or attribute.
        //ToDo: Also think about MAP and nested objects...
        public List<object> ModulesDescriptors { get; set; } = [];
        public override Type Type => typeof(TEntityDescriptor);
    }
}
