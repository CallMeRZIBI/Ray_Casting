using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RayCasting.Sound
{
    public class Sound : IPlayer
    {
        private readonly IPlayer _player;
        public bool Playing => _player.Playing;
        public bool Paused => _player.Paused;

        public Sound(string path)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) _player = new WindowsSound(path);
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) _player = new LinuxSound(path);
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) _player = new MacSound(path);
        }
        
        public async Task Pause()
        {
            await _player.Pause();
        }

        public async Task Play()
        {
            await _player.Play();
        }

        public async Task Resume()
        {
            await _player.Resume();
        }

        public async Task Stop()
        {
            await _player.Stop();
        }
    }
}
