using Microsoft.AspNetCore.Components.Forms;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WarnoModeAutomation.Logic
{
    public static class ModManager
    {
        private const string _createNewModBatFileName = "CreateNewMod.bat";
        private static CancellationTokenSource _cancellationTokenSource = new();

        public delegate void Outputter(string data);
        public static event Outputter OnOutput;

        public static async Task<bool> CreateModAsync()
        {
            var batFullPath = Path.Combine(Storage.ModeSettings.ModeDirectory, _createNewModBatFileName);

            if (!File.Exists(batFullPath))
            {
                OnOutput?.Invoke($"{batFullPath} file does not exist!");
                return false;
            }

            using (_cancellationTokenSource = new CancellationTokenSource())
            {
                using var process = new Process();

                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.RedirectStandardInput = true;
                process.StartInfo.WorkingDirectory = Storage.ModeSettings.ModeDirectory;
                process.StartInfo.FileName = Path.Combine(Environment.SystemDirectory, "cmd.exe");

                process.OutputDataReceived += ProcessOutputDataHandler;
                process.ErrorDataReceived += ProcessErrorDataHandler;

                try
                {
                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    process.StandardInput.WriteLine($"{_createNewModBatFileName} {Storage.ModeSettings.ModeName}");

                    await process.WaitForExitAsync(_cancellationTokenSource.Token);
                }
                catch (Exception ex)
                {
                    OnOutput?.Invoke(ex.Message);

                    return false;
                }

                return true;
            }
        }

        public static void ProcessOutputDataHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            OnOutput?.Invoke(outLine.Data);

            if (outLine.Data == "New mod created successfully!" || outLine.Data.Contains("Error:"))
                _cancellationTokenSource.Cancel();
        }

        public static void ProcessErrorDataHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            OnOutput?.Invoke(outLine.Data);

            _cancellationTokenSource.Cancel();
        }
    }
}
