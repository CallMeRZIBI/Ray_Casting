using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RayCasting.Sound
{
    internal class MacSound : UnixSoundBase, IPlayer
    {
        public MacSound(string path) : base(path) { }

        protected override string GetBashCommand(string filename)
        {
            return "afplay";
        }
    }
}
