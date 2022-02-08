using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RayCasting.Sound
{
    internal abstract class UnixSoundBase : IPlayer
    {
        private Process _process = null;
        internal const string PauseProcess = "kill -STOP {0}";
        internal const string ResumeProcess = "kill -CONT {0}";
        private string? _path;

        public event EventHandler PlaybackFinished;
        public bool Playing { get; private set; }
        public bool Paused { get; private set; }

        protected abstract string GetBashCommand(string filename);

        public UnixSoundBase(string? path)
        {
            _path = path;
            Playing = false;
            Paused = false;
        }

        public Task Pause()
        {
            if (_process != null && Playing && !Paused)
            {
                var tempProcess = StartBashProcess(string.Format(PauseProcess, _process.Id));
                tempProcess.WaitForExit();
                Paused = true;
            }

            return Task.CompletedTask;
        }

        public async Task Play()
        {
            await Stop();
            var BashToolName = GetBashCommand(_path);
            _process = StartBashProcess($"{BashToolName} '{_path}'");
            _process.EnableRaisingEvents = true;
            _process.Exited += HandlePlaybackFinished;
            _process.ErrorDataReceived += HandlePlaybackFinished;
            _process.Disposed += HandlePlaybackFinished;
            Playing = true;
        }

        public Task Resume()
        {
            if (_process != null && Playing && Paused)
            {
                var tempProcess = StartBashProcess(string.Format(ResumeProcess, _process.Id));
                tempProcess.WaitForExit();
                Paused = false;
            }

            return Task.CompletedTask;
        }

        public Task Stop()
        {
            if (_process != null)
            {
                _process.Kill();
                _process.Dispose();
                _process = null;
            }

            Playing = false;
            Paused = false;

            return Task.CompletedTask;
        }

        public void SetPath(string path)
        {
            _path = path;
        }

        protected Process StartBashProcess(string command)
        {
            var escapedArgs = command.Replace("\"", "\\\"");

            var process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = $"-c \"{escapedArgs}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardInput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            process.Start();
            return process;
        }

        internal void HandlePlaybackFinished(object sender, EventArgs e)
        {
            if (Playing)
            {
                Playing = false;
                PlaybackFinished?.Invoke(this, e);
            }
        }
    }
}
