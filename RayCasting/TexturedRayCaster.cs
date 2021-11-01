using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RayCasting.RayCasting
{
    class TexturedRayCaster : IRayCaster
    {
        private readonly int _screenWidth;
        private readonly int _screenHeight;
        private readonly int _renderWidth;
        private readonly int _renderHeight;

        private readonly int _texWidth; // Tex is short for texture
        private readonly int _texHeight; // Tex is short for texture
        private readonly int _mapWidth;
        private readonly int _mapHeight;

        private byte[,,] _buffer;
        private List<Texture> _texture;

        // Map must have boundings otherwise the ray would just fly away exactly it will get out of the bounds of the array
        private  int[,] _map;

        private readonly Stopwatch _timer;
        private double _deltaTime;

        // Getting info about positioning
        private double _posX;
        private double _posY;
        private double _dirX;
        private double _dirY;
        private double _planeX;
        private double _planeY;

        private double _moveSpeed;
        private double _rotSpeed;

        public TexturedRayCaster(int screenWidth, int screenHeight, int textureWidth, int textureHeight, float rederingScale = 1)
        {
            _screenWidth = screenWidth;
            _screenHeight = screenHeight;
            // It is sometimes creating weird graphical artifacts so maybye fix it later?
            _renderWidth = (int)(_screenWidth * rederingScale);
            _renderHeight = (int)(_screenHeight * rederingScale);

            _buffer = new byte[_renderHeight, _renderWidth, 3]; // Y-coordinate first because it works per scanline
            _texture = new List<Texture>();
            _texWidth = textureWidth;
            _texHeight = textureHeight;
            //GenerateTextures();

            _timer = new Stopwatch();
            _deltaTime = 0;

            _moveSpeed = 0;
            _rotSpeed = 0;
        }

        public void CreateMap(int[,] map, double StartingPosX, double StartingPosY, double dirX = -1, double dirY = 0, double planeX = 0, double planeY = 0.66)
        {
            _map = map;
            _posX = StartingPosX;
            _posY = StartingPosY;
            _dirX = dirX;
            _dirY = dirY;
            _planeX = planeX;
            _planeY = planeY;
        }

        public void UpdateRayCast(bool W_Down, bool A_Down, bool S_Down, bool D_Down)
        {
            // Timing for frame time
            _timer.Start();

            for (int x = 0; x < _renderWidth; x++)
            {
                // Calculate ray position and direction
                double cameraX = 2 * x / (double)_renderWidth - 1; // X-coordinate in camera space
                double rayDirX = _dirX + _planeX * cameraX;
                double rayDirY = _dirY + _planeY * cameraX;

                // Wcich box of the map we're in
                int mapX = (int)_posX;
                int mapY = (int)_posY;

                // Length of ray from current position to next X or Y-side
                double sideDistX;
                double sideDistY;

                // Length of ray from one X or Y-side to next X or Y-side
                double deltaDistX = Math.Sqrt(1 + (rayDirY * rayDirY) / (rayDirX * rayDirX));
                double deltaDistY = Math.Sqrt(1 + (rayDirX * rayDirX) / (rayDirY * rayDirY));
                double perpWallDist;

                // What direction to step in X or Y-direction (either +1 or -1)
                int stepX;
                int stepY;

                int hit = 0; // Was there a awall hit?
                int side = 0; // Was a NS or a EW wall hit?

                // Calculate step and initial sideDist
                if(rayDirX < 0)
                {
                    stepX = -1;
                    sideDistX = (_posX - mapX) * deltaDistX;
                }
                else
                {
                    stepX = 1;
                    sideDistX = (mapX + 1.0 - _posX) * deltaDistX;
                }
                if(rayDirY < 0)
                {
                    stepY = -1;
                    sideDistY = (_posY - mapY) * deltaDistY;
                }
                else
                {
                    stepY = 1;
                    sideDistY = (mapY + 1.0 - _posY) * deltaDistY;
                }

                // Perform DDA
                while(hit == 0)
                {
                    // Jump to next map square, either in X-direction, or in Y-direction
                    if(sideDistX < sideDistY)
                    {
                        sideDistX += deltaDistX;
                        mapX += stepX;
                        side = 0;
                    }
                    else
                    {
                        sideDistY += deltaDistY;
                        mapY += stepY;
                        side = 1;
                    }
                    // Check if ray has hit a wall
                    if (_map[mapX, mapY] > 0) hit = 1;
                }

                // Calculate distance of perpendicular ray (Euclidean distance would give fisheye effect!)
                if (side == 0) perpWallDist = sideDistX - deltaDistX;
                else           perpWallDist = sideDistY - deltaDistY;

                // Calculate height of line to draw on screen
                int lineHeight = (int)(_renderHeight / perpWallDist);

                // Calculate lowest and highest pixel to fill in current stripe
                int drawStart = -lineHeight / 2 + _renderHeight / 2;
                if (drawStart < 0) drawStart = 0;
                int drawEnd = lineHeight / 2 + _renderHeight / 2;
                if (drawEnd >= _renderHeight) drawEnd = _renderHeight - 1;

                // Texturing calculations
                int texNum = _map[mapX, mapY] - 1; // 1 subtracted from it so that texture 0 can be used

                // Calculate value of wallX
                double wallX; // where exactly the wall was hit
                if (side == 0) wallX = _posY + perpWallDist * rayDirY;
                else           wallX = _posX + perpWallDist * rayDirX;
                wallX -= Math.Floor(wallX);

                // X coordinate on the texture
                int texX = (int)(wallX * (double)_texWidth);
                if (side == 0 && rayDirX > 0) texX = _texWidth - texX - 1;
                if (side == 1 && rayDirY < 0) texX = _texWidth - texX - 1;

                // How much to increase the texture coordinate per screen pixel
                double step = 1.0 * _texHeight / lineHeight;
                // Starting texture coordinate
                double texPos = (drawStart - _renderHeight / 2 + lineHeight / 2) * step;

                // Drawing before the line
                for(int i = 0; i < drawStart; i++)
                {
                    _buffer[i, x, 0] = (byte)255;
                    _buffer[i, x, 1] = (byte)0;
                    _buffer[i, x, 2] = (byte)0;
                }

                // TODO: The walls are rendering normally but the texture is doing weird things
                // Drawing the inside of line
                for (int y = drawStart; y < drawEnd; y++)
                {
                    // Cast the texture coordinate to integer, and mask with (texHeight - 1) in case of overflow
                    int texY = (int)texPos & (_texHeight - 1);
                    texPos += step;
                    // Copying the colors instead of allocating them cause that would just refer to the original image and mess it up
                    byte[] color = new byte[4];
                    _texture[texNum].GetPixels()[_texHeight * texY + texX].CopyTo(color, 0);
                    // Make color darker for Y-sides: R, G and B byte each divided through two
                    if (side == 1)
                    {
                        color[0] = (byte)(color[0] / (byte)2);
                        color[1] = (byte)(color[1] / (byte)2);
                        color[2] = (byte)(color[2] / (byte)2);
                    }
                    _buffer[y, x, 0] = color[0];
                    _buffer[y, x, 1] = color[1];
                    _buffer[y, x, 2] = color[2];
                }

                // Drawing after the line
                for(int i = drawEnd; i < _renderHeight; i++)
                {
                    _buffer[i, x, 0] = (byte)0;
                    _buffer[i, x, 1] = (byte)255;
                    _buffer[i, x, 2] = (byte)0;
                }
            }

            // speed modifiers
            _moveSpeed = _deltaTime * 0.005; // Constant value is in squares/second * 10
            _rotSpeed = _deltaTime * 0.003; // Constant value is in radians/second * 10

            // timing for input and FPS counter
            _timer.Stop();
            _deltaTime = _timer.Elapsed.TotalMilliseconds;
            _timer.Reset();

            // Moving
            // Move froward if no wall in front of you
            if (W_Down)
            {
                if (_map[(int)(_posX + _dirX * _moveSpeed), (int)_posY] == 0) _posX += _dirX * _moveSpeed;
                if (_map[(int)_posX, (int)(_posY + _dirY * _moveSpeed)] == 0) _posY += _dirY * _moveSpeed;
            }
            // Move backwards if no wall behind you
            if (S_Down)
            {
                if (_map[(int)(_posX - _dirX * _moveSpeed), (int)_posY] == 0) _posX -= _dirX * _moveSpeed;
                if (_map[(int)_posX, (int)(_posY - _dirY * _moveSpeed)] == 0) _posY -= _dirY * _moveSpeed;
            }
            // Rotate to the right
            if (D_Down)
            {
                // both camera direction and camera plane must be rotated
                double oldDirX = _dirX;
                _dirX = _dirX * Math.Cos(-_rotSpeed) - _dirY * Math.Sin(-_rotSpeed);
                _dirY = oldDirX * Math.Sin(-_rotSpeed) + _dirY * Math.Cos(-_rotSpeed);
                double oldPlaneX = _planeX;
                _planeX = _planeX * Math.Cos(-_rotSpeed) - _planeY * Math.Sin(-_rotSpeed);
                _planeY = oldPlaneX * Math.Sin(-_rotSpeed) + _planeY * Math.Cos(-_rotSpeed);
            }
            // Rotate to the left
            if (A_Down)
            {
                // both camera direction and camera plane must be rotated
                double oldDirX = _dirX;
                _dirX = _dirX * Math.Cos(_rotSpeed) - _dirY * Math.Sin(_rotSpeed);
                _dirY = oldDirX * Math.Sin(_rotSpeed) + _dirY * Math.Cos(_rotSpeed);
                double oldPlaneX = _planeX;
                _planeX = _planeX * Math.Cos(_rotSpeed) - _planeY * Math.Sin(_rotSpeed);
                _planeY = oldPlaneX * Math.Sin(_rotSpeed) + _planeY * Math.Cos(_rotSpeed);
            }
        }

        // Loading Textures from path and setting the size in constructor manually
        public void LoadTexturesFromPaths(string[] paths)
        {
            for(int i = 0; i < paths.Length; i++) 
            {
                _texture.Add(new Texture(paths[i]));            
            }
        }

        // Preloading Textures outside of RayCaster so you can give RayCaster the size of textures if you don't know it
        public void LoadTextures(List<Texture> textures)
        {
            _texture = textures;
        }

        public byte[,,] GetRawBuffer()
        {
            return _buffer;
        }

        // NOT WORKING!!!
        // I have something wrong with the i * something etc. so it returns just 0
        private static byte[] ConvertTo1D(byte[,,] input)
        {
            byte[] output = new byte[input.Length];

            for(int i = 0; i < input.GetLength(0); i++)
            {
                for(int j = 0; j < input.GetLength(1); j++)
                {
                    for(int k = 0; k < 3; k++)
                    {
                        output[i * input.GetLength(0) + j * input.GetLength(1) + k] = input[i,j,k];
                    }
                }
            }

            return output;
        }
    }
}
