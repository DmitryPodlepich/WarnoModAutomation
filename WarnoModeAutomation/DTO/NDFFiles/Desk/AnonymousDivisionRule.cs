using NDFSerialization.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarnoModeAutomation.DTO.NDFFiles.Desk
{
    public class AnonymousDivisionRule : AnonymousDescriptor
    {
        public override Type Type => typeof(AnonymousDivisionRule);

        public TDeckDivisionRule TDeckDivisionRule { get; set; }
    }
}
