using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarnoModeAutomation.Logic;

namespace WarnoModeAutomation.DTO
{
    public class StatusDTO
    {
        public string Status => GetStatus();

        public string GetStatus() 
        {
            var isModExist = FileManager.IsModExist();

            if (isModExist)
                return "Mod exist";

            return "Mod not found";
        }
    }
}
