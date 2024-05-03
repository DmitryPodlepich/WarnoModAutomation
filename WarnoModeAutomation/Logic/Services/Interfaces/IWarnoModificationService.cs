namespace WarnoModeAutomation.Logic.Services.Interfaces
{
    public interface IWarnoModificationService
    {
        public delegate void Outputter(string data);
        public event Outputter OnOutput;
        Task Modify(bool enableFullLog, CancellationToken cancellationToken);
    }
}