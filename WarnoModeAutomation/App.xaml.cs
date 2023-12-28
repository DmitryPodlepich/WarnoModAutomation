using WarnoModeAutomation.Logic;

namespace WarnoModeAutomation
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new MainPage();

            _ = NDFSerializer.Initialize();
        }
    }
}
