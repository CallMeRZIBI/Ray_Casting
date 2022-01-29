using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RayCasting.Sound
{
    class MacSound : IPlayer
    {
        private string _path;
        public bool Playing { get; private set; }
        public bool Paused { get; private set; }

        public MacSound(string path)
        {
            _path = path;
        }

        public Task Pause()
        {
            throw new NotImplementedException();
        }

        public Task Play()
        {
            throw new NotImplementedException();
        }

        public Task Resume()
        {
            throw new NotImplementedException();
        }

        public Task Stop()
        {
            throw new NotImplementedException();
        }
    }
}
