using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RayCasting.Sound
{
    internal class LinuxSound : UnixSoundBase, IPlayer
    {
        public LinuxSound(string path) : base(path) { }

        protected override string GetBashCommand(string filename)
        {
            if (Path.GetExtension(filename).ToLower().Equals(".mp3"))
            {
                return "mpg123 -q";
            }
            else
            {
                return "aplay -q";
            }
        }
    }
}
