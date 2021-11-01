using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace RayCasting.RayCasting
{
    class Texture
    {
        private readonly List<byte[]> _pixels;

        public Texture(string path)
        {
            Image<Rgba32> image = Image.Load<Rgba32>(path);

            // Flipping the image array vertically
            image.Mutate(x => x.Flip(FlipMode.Vertical));

            // Convert ImageSharp's format to byte array
            var pixels = new List<byte[]>(image.Width * image.Height);

            for(int y = 0; y < image.Height; y++)
            {
                var row = image.GetPixelRowSpan(y);

                for(int x = 0; x < image.Width; x++)
                {
                    pixels.Add(new byte[] {row[x].R, row[x].G, row[x].B, row[x].A});
                }
            }

            _pixels = pixels;
        }

        public List<byte[]> GetPixels()
        {
            return _pixels;
        }
    }
}
