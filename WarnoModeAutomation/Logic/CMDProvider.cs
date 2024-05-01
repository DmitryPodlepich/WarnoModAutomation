using System.Diagnostics;

namespace WarnoModeAutomation.Logic
{
    public class CMDProvider(string workingDirectory) : IDisposable
    {
        public delegate void Outputter(string data);
        public event Outputter OnOutput;

        private CancellationTokenSource _cancellationTokenSource;
        private readonly string _workingDirectory = workingDirectory;

        private Process _process;

        public async Task<bool> PerformCMDCommand(string command, string workingDirectory = null) 
        {
            _cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromMinutes(1));

            using (_process = new Process())
            {
                _process.StartInfo.UseShellExecute = false;
                _process.StartInfo.RedirectStandardOutput = true;
                _process.StartInfo.RedirectStandardError = true;
                _process.StartInfo.CreateNoWindow = true;
                _process.StartInfo.RedirectStandardInput = true;
                _process.StartInfo.WorkingDirectory = workingDirectory ?? _workingDirectory;
                _process.StartInfo.FileName = Path.Combine(Environment.SystemDirectory, "cmd.exe");

                _process.OutputDataReceived += ProcessOutputDataHandler;
                _process.ErrorDataReceived += ProcessErrorDataHandler;

                try
                {
                    _process.Start();
                    _process.BeginOutputReadLine();
                    _process.BeginErrorReadLine();

                    _process.StandardInput.WriteLine(command);

                    await _process.WaitForExitAsync(_cancellationTokenSource.Token);
                }
                catch (TaskCanceledException)
                {
                    return false;
                }
                catch(OperationCanceledException)
                {
                    return false;
                }
                catch (Exception ex)
                {
                    OnOutput?.Invoke(ex.Message);

                    return false;
                }
            }

            return true;
        }

        public void Dispose()
        {
            _process.Dispose();
            _cancellationTokenSource.Dispose();
        }

        private void ProcessOutputDataHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            OnOutput?.Invoke(outLine.Data);

            if (outLine.Data == "New mod created successfully!" || outLine.Data.Contains("Error:"))
                _cancellationTokenSource.Cancel();
        }

        private void ProcessErrorDataHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            OnOutput?.Invoke(outLine.Data);
            _cancellationTokenSource.Cancel();
        }
    }
}
