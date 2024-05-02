using WarnoModeAutomation.Logic;

namespace WarnoModeAutomation.DTO
{
    public class StatusDTO
    {
        public static string Status => GetStatus();

        public static string GetStatus() 
        {
            var isModExist = FileManager.IsModExist();

            if (isModExist)
                return "Mod exist";

            return "Mod not found";
        }
    }
}
