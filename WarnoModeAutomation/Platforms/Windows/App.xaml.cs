using Microsoft.Maui.Storage;
using Microsoft.UI.Xaml;

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

            //Environment.SetEnvironmentVariable("DOTNET_DbgMiniDumpName", "WarnoModeAutomation");

            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += new System.UnhandledExceptionEventHandler(MyHandler);

            try
            {
                this.InitializeComponent();
            }
            catch (Exception ex) 
            {
                using StreamWriter sw = File.CreateText(_path);

                sw.WriteLine("MyHandler caught : " + ex.Message);
                sw.WriteLine("Runtime terminating: {0}", ex.StackTrace);
            }
        }

        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

        static void MyHandler(object sender, System.UnhandledExceptionEventArgs args)
        {
            Exception e = (Exception)args.ExceptionObject;

            using StreamWriter sw = File.CreateText(_path);

            sw.WriteLine("MyHandler caught : " + e.Message);
            sw.WriteLine("Runtime terminating: {0}", args.IsTerminating);
        }
    }

}
