using NDFSerialization.Models;

namespace WarnoModeAutomation.DTO.NDFFiles
{
    public class TemplateMeshDescriptor : Descriptor
    {
        public string FileName { get; set; }
        public override Type Type => typeof(TemplateMeshDescriptor);
    }
}
