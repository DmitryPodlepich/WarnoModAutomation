using Microsoft.Extensions.Configuration;

namespace WarnoModeAutomation.Logic.Helpers
{
    public static class ConfigurationHelper
    {
        public static IConfiguration Config { get; set; }

        public static void Initialize(IConfiguration Configuration)
        {
            Config = Configuration;
        }
    }
}