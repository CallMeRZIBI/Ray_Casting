using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RayCasting.Sound
{
    class LinuxSound : IPlayer
    {
        private Process _process = null;
        private string _path;
        public bool Playing { get; private set; }
        public bool Paused { get; private set; }

        public LinuxSound(string path)
        {
            _path = path;
        }

        public Task Pause()
        {
            if (_process != null && Playing && !Paused) {
                var process = new Process()
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "/bin/bash",
                        Arguments = $"kill -STOP {_process.Id}",
                        RedirectStandardOutput = true,
                        RedirectStandardInput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };
                process.Start();
                process.Kill();
                process.Dispose();

                Playing = true;
                Paused = true;
            }

            return Task.CompletedTask;
        }

        public Task Play()
        {
            if (!Playing && !Paused)
            {
                //Stop();
                _process = StartAplayPlayback(_path);

                Playing = true;
                Paused = false;
            }

            return Task.CompletedTask;
        }

        public Task Resume()
        {
            if(_process != null && Playing && Paused)
            {
                var process = new Process()
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "/bin/bash",
                        Arguments = $"kill -CONT {_process.Id}",
                        RedirectStandardOutput = true,
                        RedirectStandardInput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };
                process.Start();
                process.Kill();
                process.Dispose();

                Playing = true;
                Paused = false;
            }

            return Task.CompletedTask;
        }

        public Task Stop()
        {
            if(_process != null && Playing && !Paused)
            {
                _process.Kill();
                _process.Dispose();
                _process = null;

                Playing = false;
                Paused = false;
            }

            return Task.CompletedTask;
        }

        private Process StartAplayPlayback(string path)
        {
            var escapedArgs = path.Replace("\"", "\\\"");

            var process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = $"-c \"aplay {escapedArgs}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardInput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            process.Start();
            return process;
        }
    }
}
