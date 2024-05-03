using Microsoft.Extensions.Configuration;

namespace WarnoModeAutomation.Logic.Helpers
{
    public static class ConfigurationHelper
    {
        public static IConfiguration Config;
        public static void Initialize(IConfiguration Configuration)
        {
            Config = Configuration;
        }
    }
}