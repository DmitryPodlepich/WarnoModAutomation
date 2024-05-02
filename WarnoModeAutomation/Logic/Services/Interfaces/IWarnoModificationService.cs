namespace WarnoModeAutomation.Logic.Services.Interfaces
{
    public interface IWarnoModificationService
    {
        Task Modify(bool enableFullLog, CancellationToken cancellationToken);
    }
}