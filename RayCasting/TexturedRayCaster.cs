using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RayCasting.RayCasting
{
    class TexturedRayCaster : IRayCaster
    {
        private readonly int _screenWidth;
        private readonly int _screenHeight;
        private readonly int _renderWidth;
        private readonly int _renderHeight;

        private byte[,,] _buffer;

        // 1D Zbuffer
        private double[] _ZBuffer;

        private List<Texture> _texture;
        private List<Sprite> _sprites;

        // Arrays used to sort the sprites
        private int[] _spriteOrder;
        private double[] _spriteDistance;

        // Map must have boundings otherwise the ray would just fly away exactly it will get out of the bounds of the array
        private Map _map;

        // Timing *will be deleted
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

        private bool _DrawFloorCeiling = false;

        // Floor Ceiling Texture
        private int _floorTexture;
        private int _ceilingTexture;

        // Floor Ceiling Color
        private byte[] _floorColor;
        private byte[] _ceilingColor;

        // Multithreading
        private bool _isMultithreaded;
        private int _threads;
        private byte[,,] _bufferFloor;
        private byte[,,] _bufferWall;

        public TexturedRayCaster(int screenWidth, int screenHeight, float rederingScale = 1)
        {
            _screenWidth = screenWidth;
            _screenHeight = screenHeight;
            // It is sometimes creating weird graphical artifacts so maybye fix it later?
            _renderWidth = (int)(_screenWidth * rederingScale);
            _renderHeight = (int)(_screenHeight * rederingScale);

            _buffer = new byte[_renderHeight, _renderWidth, 3]; // Y-coordinate first because it works per scanline
            _bufferFloor = new byte[_renderHeight, _renderWidth, 3];
            _bufferWall = new byte[_renderHeight, _renderWidth, 3];
            _ZBuffer = new double[_renderWidth];
            _texture = new List<Texture>();

            _timer = new Stopwatch();
            _deltaTime = 0;

            _moveSpeed = 0;
            _rotSpeed = 0;

            _isMultithreaded = false;
        }

        public void CreateMap(Map map, double StartingPosX, double StartingPosY, double dirX = -1, double dirY = 0, double planeX = 0, double planeY = 0.66)
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
            switch (_isMultithreaded)
            {
                case false:
                    // Floor Casting
                    if (_DrawFloorCeiling) CastFloor(0, _renderHeight);

                    // Wall Casting
                    CastWall(0, _renderWidth);

                    // Sprite Casting
                    CastSprites();
                    break;
                case true:
                    // TODO: also you can create three different buffers for floorCasting, wallCasting, spriteCasting and render them multithreadly and then combine those buffers

                    // TODO: make it multithreaded
                    // Multithreaded Floor Casting
                    /*if (_DrawFloorCeiling)
                    {
                        int FloorThreads = _threads;
                        using(ManualResetEvent resetEvent = new ManualResetEvent(false))
                        {
                            List<int> list = new List<int>();
                            foreach(int i in Enumerable.Range(0, FloorThreads))
                            {
                                list.Add(i);
                                // Closure for anonymous function call begins here, because foreach works a bit differently than for.
                                ThreadPool.QueueUserWorkItem(new WaitCallback(x =>
                                {
                                    // TODO: Fix it!! there is out of range exception, for some reason it works maximally with 2 threads, so for now I will limit in on 2 threads
                                    // it's probably because of how it's rendered, there are some calculations for perspective
                                    CastFloor((i / _threads) * _renderHeight, ((i + 1 / _threads)) * _renderHeight);
                                    if (Interlocked.Decrement(ref FloorThreads) == 0) resetEvent.Set();
                                }), list[i]);
                            }

                            resetEvent.WaitOne();
                        }
                    }

                    // MultiThreaded Wall Casting
                    int WallThreads = _threads;
                    using (ManualResetEvent resetEvent = new ManualResetEvent(false))
                    {
                        List<int> list = new List<int>();
                        foreach (int i in Enumerable.Range(0, WallThreads))
                        {
                            list.Add(i);
                            // Closure for anonymous function call begins here, because foreach works a bit differently than for.
                            ThreadPool.QueueUserWorkItem(new WaitCallback(x =>
                            {
                                CastWall((i / _threads) * _renderWidth, ((i + 1) / _threads) * _renderWidth);
                                if (Interlocked.Decrement(ref WallThreads) == 0) resetEvent.Set();
                            }), list[i]);
                        }

                        resetEvent.WaitOne();
                    }

                    // TODO: probably make it mutlithreaded?
                    // Sprite casting
                    CastSprites();*/

                    // Testing
                    RunMultithreaded();
                    break;
            }

            // speed modifiers
            _moveSpeed = _deltaTime * 5.0; // Constant value is idk
            _rotSpeed = _deltaTime * 3.0; // Constant value is idk

            // timing for input and FPS counter
            _timer.Stop();
            CalculateDelatTime();
            _timer.Reset();

            // Moving
            // TODO: Add some boundaries around player for normal collision
            Move(W_Down, A_Down, S_Down, D_Down);

            // Timing for frame time
            _timer.Start();
        }

        // Testing floor and wall casting at once
        public void RunMultithreaded()
        {
            List<int> list = new List<int>();

            // MultiThreaded Wall Casting
            using (ManualResetEvent resetEvent = new ManualResetEvent(false))
            {
                int FloorThreads = _threads > 2 ? 2 : _threads;
                if (_DrawFloorCeiling)
                {
                    foreach (int i in Enumerable.Range(0, FloorThreads))
                    {
                        list.Add(i);
                        // Closure for anonymous function call begins here, because foreach works a bit differently than for.
                        ThreadPool.QueueUserWorkItem(new WaitCallback(x =>
                        {
                            // TODO: Fix it!! there is out of range exception, for some reason it works maximally with 2 threads, so for now I will limit in on 2 threads
                            // it's probably because of how it's rendered, there are some calculations for perspective
                            CastFloor((i / (_threads > 2 ? 2 : _threads)) * _renderHeight, ((i + 1 / (_threads > 2 ? 2 : _threads))) * _renderHeight);
                            if (Interlocked.Decrement(ref FloorThreads) == 0) resetEvent.Set();
                        }), list[i]);
                    }
                }

                int WallThreads = _threads;
                foreach (int i in Enumerable.Range(0, WallThreads))
                {
                    list.Add(i + _threads > 2 ? 2 : _threads);
                    // Closure for anonymous function call begins here, because foreach works a bit differently than for.
                    ThreadPool.QueueUserWorkItem(new WaitCallback(x =>
                    {
                        CastWall((i / _threads) * _renderWidth, ((i + 1) / _threads) * _renderWidth);
                        if (Interlocked.Decrement(ref WallThreads) == 0) resetEvent.Set();
                    }), list[i + _threads > 2 ? 2 : _threads]);
                }

                resetEvent.WaitOne();
            }

            Array.Copy(_bufferFloor, _buffer, _bufferFloor.Length);
            for(int x = 0; x < _buffer.GetLength(0); x++)
            {
                for(int y = 0; y < _buffer.GetLength(1); y++)
                {
                    if (_bufferWall[x, y, 0] == 0 && _bufferWall[x, y, 1] == 0 && _bufferWall[x, y, 2] == 0)
                    {
                        continue;
                    }
                    _buffer[x, y, 0] = _bufferWall[x, y, 0];
                    _buffer[x, y, 1] = _bufferWall[x, y, 1];
                    _buffer[x, y, 2] = _bufferWall[x, y, 2];
                }
            }

            // Sprite casting
            CastSprites();
        }

        public void CalculateDelatTime()
        {
            _deltaTime = _timer.Elapsed.TotalSeconds;
        }

        // Floor Casting
        private void CastFloor(int startRenderHeight, int endRenderHeight)
        {
            for (int y = startRenderHeight; y < endRenderHeight; y++)
            {
                // rayDir for leftmost ray (x = 0) and rightmost ray (x = width)
                float rayDirX0 = (float)(_dirX - _planeX);
                float rayDirY0 = (float)(_dirY - _planeY);
                float rayDirX1 = (float)(_dirX + _planeX);
                float rayDirY1 = (float)(_dirY + _planeY);

                // Current y position compared to the center of the screen (the horizon)
                int p = y - _renderHeight / 2;

                // Vertical position of the camera
                float posZ = 0.5f * (float)_renderHeight;

                // Horizontal distance from the camera to the floor for the current row
                // 0.5 is the z position exactly in the middle between floor and ceiling
                float rowDistance = posZ / p;

                // Calculate the real world step vector we have to add for each x (parallel to camera plane)
                // adding step by step avoids multiplications with a weight in the inner loop
                float floorStepX = rowDistance * (rayDirX1 - rayDirX0) / _renderWidth;
                float floorStepY = rowDistance * (rayDirY1 - rayDirY0) / _renderWidth;

                // Real world coordinates of the leftmost column, this will be updated as we step to the right
                float floorX = (float)_posX + rowDistance * rayDirX0;
                float floorY = (float)_posY + rowDistance * rayDirY0;

                for (int x = 0; x < _renderWidth; ++x)
                {
                    // Choose the floor and ceiling texture
                    int ceilingTexWidth = _texture[_ceilingTexture].Width;
                    int ceilingTexHeight = _texture[_ceilingTexture].Height;
                    int floorTexWidth = _texture[_floorTexture].Width;
                    int floorTexHeight = _texture[_floorTexture].Height;

                    // The cell coord is simply got from the integer parts of floorX and floorY
                    int cellX = (int)floorX;
                    int cellY = (int)floorY;

                    // Floor
                    // Get the texture coordinate from the fractional part
                    int tx = (int)(ceilingTexWidth * (floorX - cellX)) & (ceilingTexWidth - 1);
                    int ty = (int)(ceilingTexHeight * (floorY - cellY)) & (ceilingTexHeight - 1);

                    floorX += floorStepX;
                    floorY += floorStepY;

                    // Draw the pixel
                    byte[] color = new byte[3];

                    // Floor
                    //_texture[_ceilingTexture].GetPixels()[ceilingTexWidth * ty + tx].CopyTo(color, 0);
                    //Buffer.BlockCopy(_texture[_ceilingTexture].GetPixels()[ceilingTexWidth * ty + tx], 0, color, 0, 3 * sizeof(byte));
                    Array.Copy(_texture[_ceilingTexture].GetPixels()[ceilingTexWidth * ty + tx], color, 3);   // The fastest implementation
                    //_buffer[y, x, 0] = color[0];
                    //_buffer[y, x, 1] = color[1];
                    //_buffer[y, x, 2] = color[2];

                    // Total Mutlithreading test
                    _bufferFloor[y, x, 0] = color[0];
                    _bufferFloor[y, x, 1] = color[1];
                    _bufferFloor[y, x, 2] = color[2];

                    // Ceiling
                    // Get the texture coordinate from the fractional part
                    tx = (int)(floorTexWidth * (floorX - cellX)) & (floorTexWidth - 1);
                    ty = (int)(floorTexHeight * (floorY - cellY)) & (floorTexHeight - 1);

                    // Ceiling (symmetrical, at _renderHeight - y - 1 instead of y)
                    //_texture[_floorTexture].GetPixels()[floorTexWidth * ty + tx].CopyTo(color, 0);
                    //Buffer.BlockCopy(_texture[_floorTexture].GetPixels()[floorTexWidth * ty + tx], 0, color, 0, 3 * sizeof(byte)); 
                    Array.Copy(_texture[_floorTexture].GetPixels()[floorTexWidth * ty + tx], color, 3);     // The fastest implementation
                    //_buffer[_renderHeight - y - 1, x, 0] = color[0];
                    //_buffer[_renderHeight - y - 1, x, 1] = color[1];
                    //_buffer[_renderHeight - y - 1, x, 2] = color[2];

                    // Total Mutlithreading test
                    _bufferFloor[_renderHeight - y - 1, x, 0] = color[0];
                    _bufferFloor[_renderHeight - y - 1, x, 1] = color[1];
                    _bufferFloor[_renderHeight - y - 1, x, 2] = color[2];
                }
            }
        }

        // Casting walls
        private void CastWall(int startRenderWidth, int endRenderWidth)
        {
            for (int x = startRenderWidth; x < endRenderWidth; x++)
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

                // This was causing fish eye
                //double deltaDistX = Math.Sqrt(1 + (rayDirY * rayDirY) / (rayDirX * rayDirX));
                //double deltaDistY = Math.Sqrt(1 + (rayDirX * rayDirX) / (rayDirY * rayDirY));
                // Length of ray from one X or Y-side to next X or Y-side
                double deltaDistX = Math.Abs(1 / rayDirX);
                double deltaDistY = Math.Abs(1 / rayDirY);
                double perpWallDist;

                // What direction to step in X or Y-direction (either +1 or -1)
                int stepX;
                int stepY;

                int hit = 0; // Was there a awall hit?
                int side = 0; // Was a NS or a EW wall hit?

                // Calculate step and initial sideDist
                if (rayDirX < 0)
                {
                    stepX = -1;
                    sideDistX = (_posX - mapX) * deltaDistX;
                }
                else
                {
                    stepX = 1;
                    sideDistX = (mapX + 1.0 - _posX) * deltaDistX;
                }
                if (rayDirY < 0)
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
                while (hit == 0)
                {
                    // Jump to next map square, either in X-direction, or in Y-direction
                    if (sideDistX < sideDistY)
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
                    if (_map.map[mapX, mapY] > 0) hit = 1;
                }

                // Calculate distance of perpendicular ray (Euclidean distance would give fisheye effect!)
                if (side == 0) perpWallDist = sideDistX - deltaDistX;
                else perpWallDist = sideDistY - deltaDistY;

                // Calculate height of line to draw on screen
                int lineHeight = (int)(_renderHeight / perpWallDist);

                // Calculate lowest and highest pixel to fill in current stripe
                int drawStart = -lineHeight / 2 + _renderHeight / 2;
                if (drawStart < 0) drawStart = 0;
                int drawEnd = lineHeight / 2 + _renderHeight / 2;
                if (drawEnd >= _renderHeight) drawEnd = _renderHeight - 1;

                // Texturing calculations
                int texNum = _map.map[mapX, mapY] - 1; // 1 subtracted from it so that texture 0 can be used
                int HitTexWidth = _texture[texNum].Width;
                int HitTexHeight = _texture[texNum].Height;

                // Calculate value of wallX
                double wallX; // where exactly the wall was hit
                if (side == 0) wallX = _posY + perpWallDist * rayDirY;
                else wallX = _posX + perpWallDist * rayDirX;
                wallX -= Math.Floor(wallX);

                // X coordinate on the texture
                int texX = (int)(wallX * (double)HitTexWidth);
                if (side == 0 && rayDirX > 0) texX = HitTexWidth - texX - 1;
                if (side == 1 && rayDirY < 0) texX = HitTexWidth - texX - 1;

                // How much to increase the texture coordinate per screen pixel
                double step = 1.0 * HitTexHeight / lineHeight;
                // Starting texture coordinate
                double texPos = (drawStart - _renderHeight / 2 + lineHeight / 2) * step;

                // Draw only colors if user want it
                if (!_DrawFloorCeiling)
                {
                    // Draw before line
                    for(int i = 0; i < drawStart; i++)
                    {
                        _buffer[i, x, 0] = _floorColor[0];
                        _buffer[i, x, 1] = _floorColor[1];
                        _buffer[i, x, 2] = _floorColor[2];
                    }

                    // Draw after line
                    for(int i = drawEnd; i < _renderHeight; i++)
                    {
                        _buffer[i, x, 0] = _ceilingColor[0];
                        _buffer[i, x, 1] = _ceilingColor[1];
                        _buffer[i, x, 2] = _ceilingColor[2];
                    }
                }

                // Drawing the inside of line
                for (int y = drawStart; y < drawEnd; y++)
                {
                    // Cast the texture coordinate to integer, and mask with (texHeight - 1) in case of overflow
                    int texY = (int)texPos & (HitTexHeight - 1);
                    texPos += step;
                    // Copying the colors instead of allocating them cause that would just refer to the original image and mess it up
                    byte[] color = new byte[3];

                    //_texture[texNum].GetPixels()[HitTexHeight * texY + texX].CopyTo(color, 0);
                    //Buffer.BlockCopy(_texture[texNum].GetPixels()[HitTexHeight * texY + texX], 0, color, 0, 3 * sizeof(byte));
                    Array.Copy(_texture[texNum].GetPixels()[HitTexHeight * texY + texX], color, 3);     // The fastest solution

                    // Make color darker for Y-sides: R, G and B byte each divided through two
                    if (side == 1)
                    {
                        color[0] = (byte)(color[0] / (byte)2);
                        color[1] = (byte)(color[1] / (byte)2);
                        color[2] = (byte)(color[2] / (byte)2);
                    }
                    //_buffer[y, x, 0] = color[0];
                    //_buffer[y, x, 1] = color[1];
                    //_buffer[y, x, 2] = color[2];

                    // Total Mutlithreading test
                    _bufferWall[y, x, 0] = color[0];
                    _bufferWall[y, x, 1] = color[1];
                    _bufferWall[y, x, 2] = color[2];
                }

                // SET THE ZBUFFER FOR THE SPRITE CASTING
                _ZBuffer[x] = perpWallDist;
            }
        }

        // TODO: The Z value relies on resolution fix it
        // Sprite Casting
        private void CastSprites()
        {
            // Sort sprites from far to close
            for(int i = 0; i < _sprites.Count; i++)
            {
                _spriteOrder[i] = i;
                _spriteDistance[i] = (_posX - _sprites[i].posX) * (_posX - _sprites[i].posX) + (_posY - _sprites[i].posY) * (_posY - _sprites[i].posY); // Sqrt not taken, unneeded
            }
            sortSprites( ref _spriteOrder, ref _spriteDistance, _sprites.Count);

            // After sorting the sprites, do the projection and draw them
            for(int i = 0; i < _sprites.Count; i++)
            {
                int spriteTexWidth = _sprites[_spriteOrder[i]].texture.Width;
                int spriteTexHeight = _sprites[_spriteOrder[i]].texture.Height;

                // Translate sprite position to relative to camera
                double spriteX = _sprites[_spriteOrder[i]].posX - _posX;
                double spriteY = _sprites[_spriteOrder[i]].posY - _posY;

                Sprite sprite = _sprites[_spriteOrder[i]];

                // Transform sprite with the inverse camera matrix
                // [ planeX   dirX ] -1                                       [ dirY      -dirX ]
                // [               ]       =  1/(planeX*dirY-dirX*planeY) *   [                 ]
                // [ planeY   dirY ]                                          [ -planeY  planeX ]

                double invDet = 1.0 / (_planeX * _dirY - _dirX * _planeY); // Reyuired for correct matrix multiplication

                double transformX = invDet * (_dirY * spriteX - _dirX * spriteY);
                double transformY = invDet * (-_planeY * spriteX + _planeX * spriteY);

                int spriteScreenX = (int)((_renderWidth / 2) * (1 + transformX / transformY));

                // Moving sprite up or down
                int vMoveScreen = (int)(sprite.posZ / transformY);

                // Calculate height of the sprite on screen
                int spriteHeight = (int)(Math.Abs((int)(_renderHeight / transformY)) * sprite.scaleY); // Using 'transformY' instead of the real distance prevents fisheye
                // Calculate lowest and highest pixel to fill in current stripe
                int drawStartY = -spriteHeight / 2 + _renderHeight / 2 + vMoveScreen;
                if (drawStartY < 0) drawStartY = 0;
                int drawEndY = spriteHeight / 2 + _renderHeight / 2 + vMoveScreen;
                if (drawEndY >= _renderHeight) drawEndY = _renderHeight - 1;

                // Calculate width of the sprite
                int spriteWidth = (int)(Math.Abs((int)(_renderHeight / transformY)) * sprite.scaleX);
                int drawStartX = -spriteWidth / 2 + spriteScreenX;
                if (drawStartX < 0) drawStartX = 0;
                int drawEndX = spriteWidth / 2 + spriteScreenX;
                if (drawEndX >= _renderWidth) drawEndX = _renderWidth - 1;

                // loop through every vertical stripe of the sprite on screen
                for(int stripe = drawStartX; stripe < drawEndX; stripe++)
                {
                    int texX = (int)(256 * (stripe - (-spriteWidth / 2 + spriteScreenX)) * spriteTexWidth / spriteWidth) / 256;
                    // The conditions in the if are:
                    // 1) It's in front of camera plane so you don't see things behind you
                    // 2) It's on the screen (left)
                    // 3) It's on the screen (right)
                    // 4) ZBuffer, with perpendicular distance
                    if(transformY > 0 && stripe > 0 && stripe < _renderWidth && transformY < _ZBuffer[stripe])
                    {
                        for(int y = drawStartY; y < drawEndY - 1; y++) // For every pixel of the current stripe
                        {
                            int d = ((y - vMoveScreen) * 256) - (_renderHeight * 128) + (spriteHeight * 128); // 256 and 128 factors to avoid floats
                            int texY = ((d * spriteTexHeight) / spriteHeight) / 256;
                            byte[] color = new byte[4];
                            // Out of range exception

                            //_sprites[_spriteOrder[i]].texture.GetPixels()[spriteTexWidth * texY + texX].CopyTo(color, 0); // Get current color from the texture
                            //Buffer.BlockCopy(sprite.texture.GetPixels()[spriteTexWidth * texY + texX], 0, color, 0, 4 * sizeof(byte));
                            Array.Copy(sprite.texture.GetPixels()[spriteTexWidth * texY + texX], color, 4);  // The fastest solution

                            // TODO: Change the invisible color to alfa = 0 or make calculations so you can have semi transparent sprites
                            if (color[3] != 0) // Paint pixel if alfa isn't maximum
                            {
                                _buffer[y, stripe, 0] = (byte)((color[0] * color[3] + _buffer[y, stripe, 0] * (255 - color[3])) / 255); // Uses transparency
                                _buffer[y, stripe, 1] = (byte)((color[1] * color[3] + _buffer[y, stripe, 1] * (255 - color[3])) / 255); 
                                _buffer[y, stripe, 2] = (byte)((color[2] * color[3] + _buffer[y, stripe, 2] * (255 - color[3])) / 255);
                            }
                        }
                    }
                }
            }
        }

        // Using reference so it will change the passed arrays and won't just copy data from them and change them only inside the method
        private void sortSprites(ref int[] order, ref double[] dist, int amount)
        {
            List<Tuple<double, int>> sprites = new List<Tuple<double, int>>(amount);
            for(int i = 0; i < amount; i++)
            {
                sprites.Add(new Tuple<double, int>(dist[i], order[i]));
            }

            sprites.Sort();
            // Restore in reverse order to go from farthest to nearest
            //sprites.Reverse(); // List.Reverse takes almost 2ms
            for(int i = 0; i < amount; i++)
            {
                dist[i] = sprites[amount - i - 1].Item1;
                order[i] = sprites[amount - i - 1].Item2;
            }
        }

        // Movement
        private void Move(bool W_Down, bool A_Down, bool S_Down, bool D_Down)
        {
            // Move froward if no wall in front of you
            if (W_Down)
            {
                if (_map.map[(int)(_posX + _dirX * _moveSpeed), (int)_posY] == 0) _posX += _dirX * _moveSpeed;
                if (_map.map[(int)_posX, (int)(_posY + _dirY * _moveSpeed)] == 0) _posY += _dirY * _moveSpeed;
            }
            // Move backwards if no wall behind you
            if (S_Down)
            {
                if (_map.map[(int)(_posX  - _dirX * _moveSpeed), (int)_posY] == 0) _posX -= _dirX * _moveSpeed;
                if (_map.map[(int)_posX, (int)(_posY - _dirY * _moveSpeed)] == 0) _posY -= _dirY * _moveSpeed;
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

        /// <summary>
        /// Use Multithreading.
        /// </summary>
        /// <param name="threadNum">Number of threads</param>
        public void MultiThreaded(int threadNum)
        {
            _isMultithreaded = true;
            _threads = threadNum;
        }

        // Draws walls and ceilings as texture if user want it
        /// <summary>
        /// Draws floor and ceiling as texture.
        /// This can dramadically reduce fps.
        /// </summary>
        /// <param name="TexFloorIndex">Index of the texture</param>
        /// <param name="TexCeilingIndex">Index of the texture</param>
        public void UseFloorCeilingTextures(int TexFloorIndex, int TexCeilingIndex)
        {
            _DrawFloorCeiling = true;

            _floorTexture = TexFloorIndex;
            _ceilingTexture = TexCeilingIndex;
        }

        // Draws walls and ceiling just as color
        /// <summary>
        /// Draws floor and ceiling just as color
        /// </summary>
        /// <param name="FloorColor">RGB value from 0 - 255 in byte array</param>
        /// <param name="CeilingColor">RGB value from 0 - 255 in byte array</param>
        public void UseFloorCeilingColors(byte[] FloorColor, byte[] CeilingColor)
        {
            _DrawFloorCeiling = false;

            _floorColor = FloorColor;
            _ceilingColor = CeilingColor;
        }

        // Loading Textures from path
        public void LoadTexturesFromPaths(string[] paths)
        {
            for(int i = 0; i < paths.Length; i++) 
            {
                _texture.Add(new Texture(paths[i]));            
            }
        }

        // Preloading Textures outside of RayCaster
        public void LoadTextures(List<Texture> textures)
        {
            _texture = textures;
        }

        // Preloading Sprites outside of rayCaster
        public void LoadSprites(List<Sprite> sprites)
        {
            _sprites = sprites;
            _spriteOrder = new int[_sprites.Count];
            _spriteDistance = new double[_sprites.Count];
        }

        public byte[,,] GetRawBuffer()
        {
            return _buffer;
        }
    }
}
