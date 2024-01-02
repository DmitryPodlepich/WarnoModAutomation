using NDFSerialization.Models;
using NDFSerialization.NDFDataTypes;

namespace WarnoModeAutomation.DTO.NDFFiles
{
    public class TTagsModuleDescriptor : Descriptor
    {
        public NDFVectorGeneric<string> TagSet { get; set; }
        public override Type Type => typeof(TTagsModuleDescriptor);
    }
}
