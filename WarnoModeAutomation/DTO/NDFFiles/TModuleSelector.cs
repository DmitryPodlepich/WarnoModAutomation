using NDFSerialization.Models;

namespace WarnoModeAutomation.DTO.NDFFiles
{
    public class TModuleSelector : Descriptor
    {
        public override Type Type => typeof(TModuleSelector);

        public string Default {get; set;}
    }
}
