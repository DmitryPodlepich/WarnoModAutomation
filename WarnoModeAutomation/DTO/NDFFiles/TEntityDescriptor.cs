using NDFSerialization.Models;
using NDFSerialization.NDFDataTypes;

namespace WarnoModeAutomation.DTO.NDFFiles
{
    public class TEntityDescriptor : Descriptor
    {
        public string ClassNameForDebug { get; set; }
        public NDFVector ModulesDescriptors { get; set; }
        public override Type Type => typeof(TEntityDescriptor);
    }
}
