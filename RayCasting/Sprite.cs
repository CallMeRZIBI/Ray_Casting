using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RayCasting.RayCasting
{
    class Sprite
    {
        public double posX { get; set; }
        public double posY { get; set; }
        public Texture texture { get; set; }

        public void LoadTextureFromPath(string path)
        {
            texture = new Texture(path);
        }
    }
}
