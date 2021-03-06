using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RayCasting
{
    public class TexturedRayCaster : BaseRayCaster, IRayCaster
    {
        private List<Texture> _texture;
        private List<Sprite> _sprites;

        // Arrays used to sort the sprites
        private int[] _spriteOrder;
        private double[] _spriteDistance;

        private List<Camera> _cameras;
        private int _cameraIndex; // local variable for more readable code

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

        /// <summary>
        /// Creates Textured RayCaster.
        /// </summary>
        public TexturedRayCaster() : base()
        {
            _texture = new List<Texture>();

            _isMultithreaded = false;

            _cameras = new List<Camera>();
        }

        public override void CreateCamera(Camera camera)
        {
            _cameras.Add(camera);
        }

        public override void CreateCamera(int ScreenWidth, int ScreenHeight, double StartingPosX, double StartingPosY, double dirX = -1, double dirY = 0, double planeX = 0, double planeY = 0.66)
        {
            Camera cam = new Camera(ScreenWidth, ScreenHeight, StartingPosX, StartingPosY, dirX, dirY, planeX, planeY);
            _cameras.Add(cam);
        }

        /// <summary>
        /// Updates Frame.
        /// </summary>
        public override void UpdateRayCast()
        {
            // timing for input and FPS counter
            _timer.Stop();
            CalculateDeltaTime();
            _timer.Reset();

            // Timing for frame time
            _timer.Start();

            for (int count = 0; count < _cameras.Count(); count++)
            {
                _cameraIndex = count;
                switch (_isMultithreaded)
                {
                    case false:
                        // Floor Casting
                        if (_DrawFloorCeiling) CastFloor(0, _cameras[_cameraIndex].screenWidth);

                        // Wall Casting
                        CastWall(0, _cameras[_cameraIndex].screenWidth);

                        // Sprite Casting
                        CastSprites();
                        break;
                    case true:
                        // TODO: also you can create three different buffers for floorCasting, wallCasting, spriteCasting and render them multithreadly and then combine those buffers - was working on it

                        // TODO: in CastFloor fix the screen space when running from multiple threads it uses the first
                        // Multithreaded Floor Casting
                        if (_DrawFloorCeiling)
                        {
                            int FloorThreads = _threads;
                            using (ManualResetEvent resetEvent = new ManualResetEvent(false))
                            {
                                List<int> list = new List<int>();
                                foreach (int i in Enumerable.Range(0, FloorThreads))
                                {
                                    list.Add(i);
                                    // Closure for anonymous function call begins here, because foreach works a bit differently than for.
                                    ThreadPool.QueueUserWorkItem(new WaitCallback(x =>
                                    {
                                        CastFloor(i * _cameras[_cameraIndex].screenWidth / _threads, (i + 1) * _cameras[_cameraIndex].screenWidth / _threads);
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
                                    CastWall(i * _cameras[_cameraIndex].screenWidth / _threads, (i + 1) * _cameras[_cameraIndex].screenWidth / _threads);
                                    if (Interlocked.Decrement(ref WallThreads) == 0) resetEvent.Set();
                                }), list[i]);
                            }

                            resetEvent.WaitOne();
                        }

                        // TODO: probably make it mutlithreaded?
                        // Sprite casting
                        CastSprites();

                        break;
                }
            }
        }

        // Maybye delete the cameraIndex and make it local variable for cleaner code
        // Floor Casting
        private void CastFloor(int startRenderWidth, int endRenderWidth)
        {
            for (int y = 0; y < _cameras[_cameraIndex].screenHeight; y++)
            {
                // rayDir for leftmost ray (x = 0) and rightmost ray (x = width)
                float rayDirX0 = (float)(_cameras[_cameraIndex].dirX - _cameras[_cameraIndex].planeX);
                float rayDirY0 = (float)(_cameras[_cameraIndex].dirY - _cameras[_cameraIndex].planeY);
                float rayDirX1 = (float)(_cameras[_cameraIndex].dirX + _cameras[_cameraIndex].planeX);
                float rayDirY1 = (float)(_cameras[_cameraIndex].dirY + _cameras[_cameraIndex].planeY);

                // Current y position compared to the center of the screen (the horizon)
                int p = y - _cameras[_cameraIndex].screenHeight / 2;

                // Vertical position of the camera
                float posZ = 0.5f * (float)_cameras[_cameraIndex].screenHeight;

                // Horizontal distance from the camera to the floor for the current row
                // 0.5 is the z position exactly in the middle between floor and ceiling
                float rowDistance = posZ / p;

                // Calculate the real world step vector we have to add for each x (parallel to camera plane)
                // adding step by step avoids multiplications with a weight in the inner loop
                float floorStepX = rowDistance * (rayDirX1 - rayDirX0) / _cameras[_cameraIndex].screenWidth;
                float floorStepY = rowDistance * (rayDirY1 - rayDirY0) / _cameras[_cameraIndex].screenWidth;

                // Real world coordinates of the leftmost column, this will be updated as we step to the right
                float floorX = (float)_cameras[_cameraIndex].posX + rowDistance * rayDirX0;
                float floorY = (float)_cameras[_cameraIndex].posY + rowDistance * rayDirY0;

                // Adding the offset to floor X and Y because in multithreading the rendering starts from different points
                floorX += floorStepX * startRenderWidth;
                floorY += floorStepY * startRenderWidth;

                for (int x = startRenderWidth; x < endRenderWidth; ++x)
                {
                    // Choose the floor and ceiling textures
                    int floorTexWidth = _texture[_floorTexture].Width;
                    int floorTexHeight = _texture[_floorTexture].Height;
                    int ceilingTexWidth = _texture[_ceilingTexture].Width;
                    int ceilingTexHeight = _texture[_ceilingTexture].Height;

                    // The cell coord is simply got from the integer parts of floorX and floorY
                    int cellX = (int)floorX;
                    int cellY = (int)floorY;

                    floorX += floorStepX;
                    floorY += floorStepY;

                    // Draw the pixel
                    byte[] color = new byte[3];

                    // Get the texture coordinate from the fractional part
                    int tx = (int)(floorTexWidth * (floorX - cellX)) & (floorTexWidth - 1);
                    int ty = (int)(floorTexHeight * (floorY - cellY)) & (floorTexHeight - 1);

                    // Ceiling (symmetrical, at _cameras[cameraIndex].screenHeight - y - 1 instead of y)
                    //_texture[_floorTexture].GetPixels()[floorTexWidth * ty + tx].CopyTo(color, 0);
                    //Buffer.BlockCopy(_texture[_floorTexture].GetPixels()[floorTexWidth * ty + tx], 0, color, 0, 3 * sizeof(byte)); 
                    Array.Copy(_texture[_floorTexture].GetPixels()[floorTexWidth * ty + tx], color, 3);     // The fastest implementation
                    _cameras[_cameraIndex].buffer[_cameras[_cameraIndex].screenHeight - y - 1, x, 0] = color[0];
                    _cameras[_cameraIndex].buffer[_cameras[_cameraIndex].screenHeight - y - 1, x, 1] = color[1];
                    _cameras[_cameraIndex].buffer[_cameras[_cameraIndex].screenHeight - y - 1, x, 2] = color[2];

                    // Get the texture coordinate from the fractional part
                    tx = (int)(ceilingTexWidth * (floorX - cellX)) & (ceilingTexWidth - 1);
                    ty = (int)(ceilingTexHeight * (floorY - cellY)) & (ceilingTexHeight - 1);

                    // Floor
                    //_texture[_ceilingTexture].GetPixels()[ceilingTexWidth * ty + tx].CopyTo(color, 0);
                    //Buffer.BlockCopy(_texture[_ceilingTexture].GetPixels()[ceilingTexWidth * ty + tx], 0, color, 0, 3 * sizeof(byte));
                    Array.Copy(_texture[_ceilingTexture].GetPixels()[ceilingTexWidth * ty + tx], color, 3);   // The fastest implementation
                    _cameras[_cameraIndex].buffer[y, x, 0] = color[0];
                    _cameras[_cameraIndex].buffer[y, x, 1] = color[1];
                    _cameras[_cameraIndex].buffer[y, x, 2] = color[2];
                }
            }
        }

        // Casting walls
        private void CastWall(int startRenderWidth, int endRenderWidth)
        {
            for (int x = startRenderWidth; x < endRenderWidth; x++)
            {
                // Calculate ray position and direction
                double cameraX = 2 * x / (double)_cameras[_cameraIndex].screenWidth - 1; // X-coordinate in camera space
                double rayDirX = _cameras[_cameraIndex].dirX + _cameras[_cameraIndex].planeX * cameraX;
                double rayDirY = _cameras[_cameraIndex].dirY + _cameras[_cameraIndex].planeY * cameraX;

                // Wcich box of the map we're in
                int mapX = (int)_cameras[_cameraIndex].posX;
                int mapY = (int)_cameras[_cameraIndex].posY;

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
                    sideDistX = (_cameras[_cameraIndex].posX - mapX) * deltaDistX;
                }
                else
                {
                    stepX = 1;
                    sideDistX = (mapX + 1.0 - _cameras[_cameraIndex].posX) * deltaDistX;
                }
                if (rayDirY < 0)
                {
                    stepY = -1;
                    sideDistY = (_cameras[_cameraIndex].posY - mapY) * deltaDistY;
                }
                else
                {
                    stepY = 1;
                    sideDistY = (mapY + 1.0 - _cameras[_cameraIndex].posY) * deltaDistY;
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
                int lineHeight = (int)(_cameras[_cameraIndex].screenHeight / perpWallDist);

                // Calculate lowest and highest pixel to fill in current stripe
                int drawStart = -lineHeight / 2 + _cameras[_cameraIndex].screenHeight / 2;
                if (drawStart < 0) drawStart = 0;
                int drawEnd = lineHeight / 2 + _cameras[_cameraIndex].screenHeight / 2;
                if (drawEnd >= _cameras[_cameraIndex].screenHeight) drawEnd = _cameras[_cameraIndex].screenHeight - 1;

                // Texturing calculations
                int texNum = _map.map[mapX, mapY] - 1; // 1 subtracted from it so that texture 0 can be used
                int HitTexWidth = _texture[texNum].Width;
                int HitTexHeight = _texture[texNum].Height;

                // Calculate value of wallX
                double wallX; // where exactly the wall was hit
                if (side == 0) wallX = _cameras[_cameraIndex].posY + perpWallDist * rayDirY;
                else wallX = _cameras[_cameraIndex].posX + perpWallDist * rayDirX;
                wallX -= Math.Floor(wallX);

                // X coordinate on the texture
                int texX = (int)(wallX * (double)HitTexWidth);
                if (side == 0 && rayDirX > 0) texX = HitTexWidth - texX - 1;
                if (side == 1 && rayDirY < 0) texX = HitTexWidth - texX - 1;

                // How much to increase the texture coordinate per screen pixel
                double step = 1.0 * HitTexHeight / lineHeight;
                // Starting texture coordinate
                double texPos = (drawStart - _cameras[_cameraIndex].screenHeight / 2 + lineHeight / 2) * step;

                // Draw only colors if user want it
                if (!_DrawFloorCeiling)
                {
                    // Draw before line
                    for(int i = 0; i < drawStart; i++)
                    {
                        _cameras[_cameraIndex].buffer[i, x, 0] = _floorColor[0];
                        _cameras[_cameraIndex].buffer[i, x, 1] = _floorColor[1];
                        _cameras[_cameraIndex].buffer[i, x, 2] = _floorColor[2];
                    }

                    // Draw after line
                    for(int i = drawEnd; i < _cameras[_cameraIndex].screenHeight; i++)
                    {
                        _cameras[_cameraIndex].buffer[i, x, 0] = _ceilingColor[0];
                        _cameras[_cameraIndex].buffer[i, x, 1] = _ceilingColor[1];
                        _cameras[_cameraIndex].buffer[i, x, 2] = _ceilingColor[2];
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
                    _cameras[_cameraIndex].buffer[y, x, 0] = color[0];
                    _cameras[_cameraIndex].buffer[y, x, 1] = color[1];
                    _cameras[_cameraIndex].buffer[y, x, 2] = color[2];
                }

                // SET THE ZBUFFER FOR THE SPRITE CASTING
                _cameras[_cameraIndex].ZBuffer[x] = perpWallDist;
            }
        }

        // TODO: The Z value relies on resolution fix it
        // Sprite Casting
        private void CastSprites()
        {
            // Sort sprites from far to close
            for (int i = 0; i < _sprites.Count; i++)
            {
                _spriteOrder[i] = i;
                _spriteDistance[i] = (_cameras[_cameraIndex].posX - _sprites[i].posX) * (_cameras[_cameraIndex].posX - _sprites[i].posX) + (_cameras[_cameraIndex].posY - _sprites[i].posY) * (_cameras[_cameraIndex].posY - _sprites[i].posY); // Sqrt not taken, unneeded
            }
            sortSprites( ref _spriteOrder, ref _spriteDistance, _sprites.Count);

            // After sorting the sprites, do the projection and draw them
            for(int i = 0; i < _sprites.Count; i++)
            {
                int spriteTexWidth = _sprites[_spriteOrder[i]].texture.Width;
                int spriteTexHeight = _sprites[_spriteOrder[i]].texture.Height;

                // Translate sprite position to relative to camera
                double spriteX = _sprites[_spriteOrder[i]].posX - _cameras[_cameraIndex].posX;
                double spriteY = _sprites[_spriteOrder[i]].posY - _cameras[_cameraIndex].posY;

                Sprite sprite = _sprites[_spriteOrder[i]];

                // Transform sprite with the inverse camera matrix
                // [ planeX   dirX ] -1                                       [ dirY      -dirX ]
                // [               ]       =  1/(planeX*dirY-dirX*planeY) *   [                 ]
                // [ planeY   dirY ]                                          [ -planeY  planeX ]

                double invDet = 1.0 / (_cameras[_cameraIndex].planeX * _cameras[_cameraIndex].dirY - _cameras[_cameraIndex].dirX * _cameras[_cameraIndex].planeY); // Reyuired for correct matrix multiplication

                double transformX = invDet * (_cameras[_cameraIndex].dirY * spriteX - _cameras[_cameraIndex].dirX * spriteY);
                double transformY = invDet * (-_cameras[_cameraIndex].planeY * spriteX + _cameras[_cameraIndex].planeX * spriteY);

                int spriteScreenX = (int)((_cameras[_cameraIndex].screenWidth / 2) * (1 + transformX / transformY));

                // Moving sprite up or down
                int vMoveScreen = (int)(sprite.posZ / transformY);

                // Calculate height of the sprite on screen
                int spriteHeight = (int)(Math.Abs((int)(_cameras[_cameraIndex].screenHeight / transformY)) * sprite.scaleY); // Using 'transformY' instead of the real distance prevents fisheye
                // Calculate lowest and highest pixel to fill in current stripe
                int drawStartY = -spriteHeight / 2 + _cameras[_cameraIndex].screenHeight / 2 + vMoveScreen;
                if (drawStartY < 0) drawStartY = 0;
                int drawEndY = spriteHeight / 2 + _cameras[_cameraIndex].screenHeight / 2 + vMoveScreen;
                if (drawEndY >= _cameras[_cameraIndex].screenHeight) drawEndY = _cameras[_cameraIndex].screenHeight - 1;

                // Calculate width of the sprite
                int spriteWidth = (int)(Math.Abs((int)(_cameras[_cameraIndex].screenHeight / transformY)) * sprite.scaleX);
                int drawStartX = -spriteWidth / 2 + spriteScreenX;
                if (drawStartX < 0) drawStartX = 0;
                int drawEndX = spriteWidth / 2 + spriteScreenX;
                if (drawEndX >= _cameras[_cameraIndex].screenWidth) drawEndX = _cameras[_cameraIndex].screenWidth - 1;

                // loop through every vertical stripe of the sprite on screen
                for(int stripe = drawStartX; stripe < drawEndX; stripe++)
                {
                    int texX = (int)(256 * (stripe - (-spriteWidth / 2 + spriteScreenX)) * spriteTexWidth / spriteWidth) / 256;
                    // The conditions in the if are:
                    // 1) It's in front of camera plane so you don't see things behind you
                    // 2) It's on the screen (left)
                    // 3) It's on the screen (right)
                    // 4) ZBuffer, with perpendicular distance
                    if(transformY > 0 && stripe > 0 && stripe < _cameras[_cameraIndex].screenWidth && transformY < _cameras[_cameraIndex].ZBuffer[stripe])
                    {
                        for(int y = drawStartY; y < drawEndY - 1; y++) // For every pixel of the current stripe
                        {
                            int d = ((y - vMoveScreen) * 256) - (_cameras[_cameraIndex].screenHeight * 128) + (spriteHeight * 128); // 256 and 128 factors to avoid floats
                            int texY = ((d * spriteTexHeight) / spriteHeight) / 256;
                            byte[] color = new byte[4];
                            // Out of range exception

                            //_sprites[_spriteOrder[i]].texture.GetPixels()[spriteTexWidth * texY + texX].CopyTo(color, 0); // Get current color from the texture
                            //Buffer.BlockCopy(sprite.texture.GetPixels()[spriteTexWidth * texY + texX], 0, color, 0, 4 * sizeof(byte));
                            Array.Copy(sprite.texture.GetPixels()[spriteTexWidth * texY + texX], color, 4);  // The fastest solution

                            // TODO: Change the invisible color to alfa = 0 or make calculations so you can have semi transparent sprites
                            if (color[3] != 0) // Paint pixel if alfa isn't maximum
                            {
                                _cameras[_cameraIndex].buffer[y, stripe, 0] = (byte)((color[0] * color[3] + _cameras[_cameraIndex].buffer[y, stripe, 0] * (255 - color[3])) >> 8); // Uses transparency, now using byte shifting instead of dividing,
                                _cameras[_cameraIndex].buffer[y, stripe, 1] = (byte)((color[1] * color[3] + _cameras[_cameraIndex].buffer[y, stripe, 1] * (255 - color[3])) >> 8); // I need to divide by 255 but 2 power 8 is 256 so it's not as precise, but I think it's negligible
                                _cameras[_cameraIndex].buffer[y, stripe, 2] = (byte)((color[2] * color[3] + _cameras[_cameraIndex].buffer[y, stripe, 2] * (255 - color[3])) >> 8);
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
        // TODO: Update boundaries to circle boundaries
        /// <summary>
        /// Default built-in movement the doom style.
        /// </summary>
        /// <param name="W_Down"></param>
        /// <param name="A_Down"></param>
        /// <param name="S_Down"></param>
        /// <param name="D_Down"></param>
        /// <param name="cameraId"></param>
        /// <param name="moveSpeed"></param>
        /// <param name="rotSpeed"></param>
        public override void Move(bool W_Down, bool A_Down, bool S_Down, bool D_Down, int cameraId, float moveSpeed = 5.0f, float rotSpeed = 3.0f)
        {
            Camera cam = _cameras.Find(item => item.Id == cameraId);
            int camIndex = _cameras.FindIndex(item => item == cam);
            float radius = 0.25f;

            // speed modifiers
            _moveSpeed = _deltaTime * moveSpeed; // Constant value is idk
            _rotSpeed = _deltaTime * rotSpeed; // Constant value is idk

            // Move froward if no wall in front of you
            if (W_Down)
            {
                // TODO: Figure out how to make circle collider around camera
                // This is square collider
                if (_cameras[camIndex].dirX > 0) { if (_map.map[(int)((_cameras[camIndex].posX + _cameras[camIndex].dirX * _moveSpeed) + radius), (int)(_cameras[camIndex].posY)] == 0) _cameras[camIndex].posX += _cameras[camIndex].dirX * _moveSpeed; }
                else { if (_map.map[(int)((_cameras[camIndex].posX + _cameras[camIndex].dirX * _moveSpeed) - radius), (int)(_cameras[camIndex].posY)] == 0) _cameras[camIndex].posX += _cameras[camIndex].dirX * _moveSpeed; }
                if (_cameras[camIndex].dirY > 0) { if (_map.map[(int)(_cameras[camIndex].posX), (int)((_cameras[camIndex].posY + _cameras[camIndex].dirY * _moveSpeed) + radius)] == 0) _cameras[camIndex].posY += _cameras[camIndex].dirY * _moveSpeed; }
                else { if (_map.map[(int)(_cameras[camIndex].posX), (int)((_cameras[camIndex].posY + _cameras[camIndex].dirY * _moveSpeed) - radius)] == 0) _cameras[camIndex].posY += _cameras[camIndex].dirY * _moveSpeed; }
            }
            // Move backwards if no wall behind you
            if (S_Down)
            {
                if (_cameras[camIndex].dirX > 0) { if (_map.map[(int)((_cameras[camIndex].posX - _cameras[camIndex].dirX * _moveSpeed) - radius), (int)(_cameras[camIndex].posY)] == 0) _cameras[camIndex].posX -= _cameras[camIndex].dirX * _moveSpeed; }
                else { if (_map.map[(int)((_cameras[camIndex].posX - _cameras[camIndex].dirX * _moveSpeed) + radius), (int)(_cameras[camIndex].posY)] == 0) _cameras[camIndex].posX -= _cameras[camIndex].dirX * _moveSpeed; }
                if (_cameras[camIndex].dirY > 0) { if (_map.map[(int)(_cameras[camIndex].posX), (int)((_cameras[camIndex].posY - _cameras[camIndex].dirY * _moveSpeed) - radius)] == 0) _cameras[camIndex].posY -= _cameras[camIndex].dirY * _moveSpeed; }
                else { if (_map.map[(int)(_cameras[camIndex].posX), (int)((_cameras[camIndex].posY - _cameras[camIndex].dirY * _moveSpeed) + radius)] == 0) _cameras[camIndex].posY -= _cameras[camIndex].dirY * _moveSpeed; }
            }
            // Rotate to the right
            if (D_Down)
            {
                // both camera direction and camera plane must be rotated
                double oldDirX = _cameras[camIndex].dirX;
                _cameras[camIndex].dirX = _cameras[camIndex].dirX * Math.Cos(-_rotSpeed) - _cameras[camIndex].dirY * Math.Sin(-_rotSpeed);
                _cameras[camIndex].dirY = oldDirX * Math.Sin(-_rotSpeed) + _cameras[camIndex].dirY * Math.Cos(-_rotSpeed);
                double oldPlaneX = _cameras[camIndex].planeX;
                _cameras[camIndex].planeX = _cameras[camIndex].planeX * Math.Cos(-_rotSpeed) - _cameras[camIndex].planeY * Math.Sin(-_rotSpeed);
                _cameras[camIndex].planeY = oldPlaneX * Math.Sin(-_rotSpeed) + _cameras[camIndex].planeY * Math.Cos(-_rotSpeed);
            }
            // Rotate to the left
            if (A_Down)
            {
                // both camera direction and camera plane must be rotated
                double oldDirX = _cameras[camIndex].dirX;
                _cameras[camIndex].dirX = _cameras[camIndex].dirX * Math.Cos(_rotSpeed) - _cameras[camIndex].dirY * Math.Sin(_rotSpeed);
                _cameras[camIndex].dirY = oldDirX * Math.Sin(_rotSpeed) + _cameras[camIndex].dirY * Math.Cos(_rotSpeed);
                double oldPlaneX = _cameras[camIndex].planeX;
                _cameras[camIndex].planeX = _cameras[camIndex].planeX * Math.Cos(_rotSpeed) - _cameras[camIndex].planeY * Math.Sin(_rotSpeed);
                _cameras[camIndex].planeY = oldPlaneX * Math.Sin(_rotSpeed) + _cameras[camIndex].planeY * Math.Cos(_rotSpeed);
            }
        }

        // User can set camera position and direction by himself if default movement is not choosen
        /// <summary>
        /// Sets camera position and rotation when use default movement is not choosen
        /// </summary>
        /// <param name="PosX">X position on map grid</param>
        /// <param name="PosY">Y position on map grid</param>
        /// <param name="DirX">X direction (to which X position you are rotated)</param>
        /// <param name="DirY">Y direction (to which Y position you are rotated)</param>
        //public void CameraPos(double PosX, double PosY, double DirX, double DirY)
        //{            
        //    _posX = PosX;
        //    _posY = PosY;
        //    _dirX = DirX;
        //    _dirY = DirY;
        //}

        // Runs rendering multithreaded
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

    }
}
