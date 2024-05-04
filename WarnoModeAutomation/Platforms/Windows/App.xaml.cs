// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WarnoModeAutomation.WinUI
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : MauiWinUIApplication
    {
        private static readonly string _path = AppDomain.CurrentDomain.BaseDirectory + "/CrashLog.txt";

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            Environment.SetEnvironmentVariable("DOTNET_DbgEnableMiniDump", "1");
            Environment.SetEnvironmentVariable("DOTNET_DbgMiniDumpType", "4");
            Environment.SetEnvironmentVariable("DOTNET_CreateDumpDiagnostics", "1");
            Environment.SetEnvironmentVariable("DOTNET_EnableCrashReport", "1");
            Environment.SetEnvironmentVariable("DOTNET_CreateDumpVerboseDiagnostics", "1");

            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += new UnhandledExceptionEventHandler(Handler);

            this.InitializeComponent();
        }

        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

        private static void Handler(object sender, UnhandledExceptionEventArgs args)
        {
            Exception e = (Exception)args.ExceptionObject;

            using StreamWriter sw = File.CreateText(_path);

            sw.WriteLine("MyHandler caught : " + e.Message);
            sw.WriteLine("Runtime terminating: {0}", args.IsTerminating);
        }
    }
}
