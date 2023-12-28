using NDFSerialization.Models;
using NDFSerialization.NDFDataTypes;

namespace WarnoModeAutomation.DTO.NDFFiles
{
    public class TEntityDescriptor : Descriptor
    {
        public string ClassNameForDebug { get; set; }

        //ToDo: If collection is null there is a bug! We cannot check if it is Ienumerable with reflection.
        public NDFVector ModulesDescriptors { get; set; } = [];
        public override Type Type => typeof(TEntityDescriptor);
    }
}
