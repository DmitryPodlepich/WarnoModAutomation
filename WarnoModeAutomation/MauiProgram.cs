using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WarnoModeAutomation.Logic.Helpers;
using WarnoModeAutomation.Logic.Providers.Impl;
using WarnoModeAutomation.Logic.Providers.Interfaces;
using WarnoModeAutomation.Logic.Services.Impl;
using WarnoModeAutomation.Logic.Services.Interfaces;

namespace WarnoModeAutomation
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder.Services.AddMauiBlazorWebView();

#if DEBUG
    		builder.Services.AddBlazorWebViewDeveloperTools();
    		builder.Logging.AddDebug();
#endif

            builder.Services.AddBlazorBootstrap();

            builder.Services.AddTransient<ICMDProvider, CMDProvider>();
            builder.Services.AddSingleton<ISettingsManagerService, SettingsManagerService>();
            builder.Services.AddSingleton<IWarnoModificationService, WarnoModificationService>();

            builder.Configuration.AddJsonFile("appsettings.json");

            ConfigurationHelper.Initialize(builder.Configuration);

            return builder.Build();
        }
    }
}
