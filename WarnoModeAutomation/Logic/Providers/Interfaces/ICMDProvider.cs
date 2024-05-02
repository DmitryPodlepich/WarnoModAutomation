namespace WarnoModeAutomation.Logic.Providers.Interfaces
{
    public interface ICMDProvider
    {
        delegate void Outputter(string data);
        event Outputter OnOutput;
        void SetWorkingDirectory(string workingDirectory);
        Task<bool> PerformCMDCommand(string command, string workingDirectory = null);
    }
}