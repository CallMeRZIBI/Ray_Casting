using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Runtime.InteropServices;

namespace RayCasting.Sound
{
    class WindowsSound : IPlayer
    {
        [DllImport("winmm.dll")]
        private static extern long mciSendString(string command, StringBuilder stringReturn, int returnLength, IntPtr hwndCallback);
        private string _path;
        public bool Playing { get; private set; }
        public bool Paused { get; private set; }

        public WindowsSound(string path)
        {
            _path = path;
            Playing = false;
            Paused = false;
        }

        ~WindowsSound()
        {
            ExecuteMsiCommand("Close All");
        }

        public Task Pause()
        {
            if(Playing && !Paused)
            {
                ExecuteMsiCommand($"Pause {_path}");
                Playing = true;
                Paused = true;
            }

            return Task.CompletedTask;
        }

        public Task Play()
        {
            if (!Playing && !Paused)
            {
                ExecuteMsiCommand("Close All");
                ExecuteMsiCommand($"Status {_path} Length");
                ExecuteMsiCommand($"Play {_path}");
                Playing = true;
                Paused = false;
            }

            return Task.CompletedTask;
        }

        public Task Resume()
        {
            if(Playing && Paused)
            {
                ExecuteMsiCommand($"Resume {_path}");
                Playing = true;
                Paused = false;
            }

            return Task.CompletedTask;
        }

        public Task Stop()
        {
            if(Playing && !Paused)
            {
                ExecuteMsiCommand($"Stop {_path}");
                Playing = false;
                Paused = false;
            }

            return Task.CompletedTask;
        }

        private void ExecuteMsiCommand(string commandString)
        {
            var result = mciSendString(commandString, null, 0, IntPtr.Zero);

            if(result != 0)
            {
                throw new Exception($"Error executing MSI command. Error code: {result}");
            }
        }
    }
}
