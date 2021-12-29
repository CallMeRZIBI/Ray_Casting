using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RayCasting
{
    public class Sprite
    {
        public double posX { get; set; }
        public double posY { get; set; }
        public double? posZ { get; set; }        // default is 0 *relies on resolution
        public double? scaleX { get; set; }      // default is 1
        public double? scaleY { get; set; }      // default is 1
        public Texture texture { get; set; }

        public Sprite()
        {
            posZ = posZ == null ? 0 : posZ;
            scaleX = scaleX == null ? 1.0 : scaleX;
            scaleY = scaleY == null ? 1.0 : scaleY;
        }

        public void LoadTextureFromPath(string path)
        {
            texture = new Texture(path);
        }
    }
}
