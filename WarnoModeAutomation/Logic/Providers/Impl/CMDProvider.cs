using System.Diagnostics;
using WarnoModeAutomation.Logic.Providers.Interfaces;

namespace WarnoModeAutomation.Logic.Providers.Impl
{
    public class CMDProvider : ICMDProvider, IDisposable
    {
        public delegate void Outputter(string data);
        public event ICMDProvider.Outputter OnOutput;

        private CancellationTokenSource _cancellationTokenSource;
        private string _workingDirectory;

        private Process _process;

        ~CMDProvider()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && _process != null)
            {
                _process.Dispose();
                _cancellationTokenSource.Dispose();
            }
        }

        public void SetWorkingDirectory(string workingDirectory)
        {
            _workingDirectory = workingDirectory;
        }

        public async Task<bool> PerformCMDCommand(string command, CancellationTokenSource cancellationTokenSource, string workingDirectory = null)
        {
            _cancellationTokenSource = cancellationTokenSource;

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
                catch (OperationCanceledException)
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
