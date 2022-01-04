using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RayCasting
{
    public class UntexturedRayCaster : IRayCaster
    {
        private readonly int _screenWidth;
        private readonly int _screenHeight;
        private readonly int _renderWidth;
        private readonly int _renderHeight;

        // VertexData
        private float[] _vertexData;
        public float LineThickness { get; private set; }

        // Map must have boundings otherwise the ray would just fly away exactly it will get out of the bounds of the array
        private Map _map;

        private readonly Stopwatch _timer;
        private double _deltaTime = 0;

        // Getting info about positioning
        private double _posX;
        private double _posY;
        private double _dirX;
        private double _dirY;
        private double _planeX;
        private double _planeY;

        private bool _builtInMovement;

        private double _moveSpeed;
        private double _rotSpeed;

        /// <summary>
        /// Constructor of Raycaster, declaring needed values for rendering.
        /// </summary>
        /// <param name="screenWidth">Getting width of rendered screen.</param>
        /// <param name="screenHeight">Getting Height of rendered screen.</param>
        /// <param name="rederingScale">Getting scale of rendering. Be careful with values, so try if screenWidth and screenHeight multiplied by renderingScale always outputs full number. Otherwise weird graphical artifacts can happen. Also don't use numbers larger than one.</param>
        public UntexturedRayCaster(int screenWidth, int screenHeight, float rederingScale = 1)
        {
            _screenWidth = screenWidth;
            _screenHeight = screenHeight;
            // It is sometimes creating weird graphical artifacts so maybye fix it later?
            _renderWidth = (int)(_screenWidth * rederingScale);
            _renderHeight = (int)(_screenHeight * rederingScale);
            LineThickness = _screenWidth / _renderWidth;

            _vertexData = new float[_screenWidth * 12];
            _timer = new Stopwatch();
            _deltaTime = 0;
            _moveSpeed = 0;
            _rotSpeed = 0;
        }

        public void CreateMap(Map map, double StartingPosX, double StartingPosY, double dirX = -1, double dirY = 0, double planeX = 0, double planeY = 0.66)
        {
            _map = map;
            _posX = StartingPosX;
            _posY = StartingPosY;
            _dirX = -1;
            _dirY = 0;
            _planeX = planeX;
            _planeY = planeY;
        }

        // Update RayCast calculations with given pos and save vertices in OpenGL format which later can be fromated
        public void UpdateRayCast(bool W_Down, bool A_Down, bool S_Down, bool D_Down)
        {
            for (int x = 0; x < _renderWidth; x++)
            {
                // Calculate ray position and direction
                double cameraX = 2 * x / (double)_renderWidth - 1; // X coordinate in camera space
                double rayDirX = _dirX + _planeX * cameraX;
                double rayDirY = _dirY + _planeY * cameraX;

                // Which box of the map we're in
                int mapX = (int)_posX;
                int mapY = (int)_posY;

                // Length of ray from current position to next x or y-side
                double sideDistX;
                double sideDistY;

                // Length of ray from one x or y-side to next x or y-side
                double deltaDistX = Math.Abs(1 / rayDirX);
                double deltaDistY = Math.Abs(1 / rayDirY);
                double perpWallDist;

                // What direction to step in x or y-direction (either +1 or -1)
                int stepX;
                int stepY;

                int hit = 0; // Was there a wall hit?
                int side = 0; // Was a NS or a EW wall hit?

                // Calculate the step and initial sideDist
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

                // perform DDA
                while (hit == 0)
                {
                    // Jump to next map square, either in x-direction, or in y-direction
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

                // Calculate distance projected on camera direction (Euclidean distance would give fisheye effect!)
                if (side == 0) perpWallDist = (sideDistX - deltaDistX);
                else perpWallDist = (sideDistY - deltaDistY);

                // Calculate height of line to draw on screen
                int lineHeight = (int)(_renderHeight / perpWallDist);

                // Replace this part with your own calculation of vertexs positions
                float upperVertY = Map((float)lineHeight / 2f, 0.0f, (float)_renderHeight / 2, 0.0f, 1.0f);
                float bottomVertY = Map((float)lineHeight / 2f, 0.0f, (float)_renderHeight / 2, 0.0f, -1.0f);

                // Choose wall color
                Color color;
                switch (_map.map[mapX, mapY])
                {
                    case 1:
                        color = new Color() { r = 1.0f, g = 0.0f, b = 0.0f }; //red
                        break;
                    case 2:
                        color = new Color() { r = 0.0f, g = 1.0f, b = 0.0f }; //green
                        break;
                    case 3:
                        color = new Color() { r = 0.0f, g = 0.0f, b = 1.0f }; //blue
                        break;
                    case 4:
                        color = new Color() { r = 1.0f, g = 1.0f, b = 1.0f }; //white
                        break;
                    default:
                        color = new Color() { r = 1.0f, g = 1.0f, b = 0.0f }; //yellow
                        break;
                }

                // Give X and Y sides different brightness
                if (side == 1)
                {
                    color.r /= 2;
                    color.g /= 2;
                    color.b /= 2;
                }

                // Draw the pixels of the stripe as a vertical line
                _vertexData[x * 12 + 0] = Map(x, 0.0f, _renderWidth, -1.0f, 1.0f);
                _vertexData[x * 12 + 1] = upperVertY;
                _vertexData[x * 12 + 2] = 0.0f;
                _vertexData[x * 12 + 3] = color.r;
                _vertexData[x * 12 + 4] = color.g;
                _vertexData[x * 12 + 5] = color.b;
                _vertexData[x * 12 + 6] = Map(x, 0.0f, _renderWidth, -1.0f, 1.0f);
                _vertexData[x * 12 + 7] = bottomVertY;
                _vertexData[x * 12 + 8] = 0.0f;
                _vertexData[x * 12 + 9] = color.r;
                _vertexData[x * 12 + 10] = color.g;
                _vertexData[x * 12 + 11] = color.b;
            }

            // Speed modifiers
            _moveSpeed = _deltaTime * 5.0 * LineThickness; // Constant value is in idk
            _rotSpeed = _deltaTime * 3.0 * LineThickness; // Constant value is in idk

            // Timing for input *some problem with probably stopwatch or me cause it flickers sometimes
            _timer.Stop();
            CalculateDelatTime();
            _timer.Reset();

            // Moving
            if (_builtInMovement)
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
                    if (_map.map[(int)(_posX - _dirX * _moveSpeed), (int)_posY] == 0) _posX -= _dirX * _moveSpeed;
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

            // Timing for frame times
            _timer.Start();
        }

        public void CalculateDelatTime()
        {
            _deltaTime = _timer.Elapsed.TotalSeconds;
        }

        // User can use default movement
        /// <summary>
        /// Uses default movement with W S for moving and A D for rotating
        /// </summary>
        public void UseDefaultMovement()
        {
            _builtInMovement = true;
        }

        // User can set camera position and direction by himself if default movement is not choosen
        // TODO: Probably create Camera class or struct
        /// <summary>
        /// Sets camera position and rotation when use default movement is not choosen
        /// </summary>
        /// <param name="PosX">X position on map grid</param>
        /// <param name="PosY">Y position on map grid</param>
        /// <param name="DirX">X direction (to which X position you are rotated)</param>
        /// <param name="DirY">Y direction (to which Y position you are rotated)</param>
        public void CameraPos(double PosX, double PosY, double DirX, double DirY)
        {
            if (!_builtInMovement) // prevent from updating position when default movement is used
            {
                _posX = PosX;
                _posY = PosY;
                _dirX = DirX;
                _dirY = DirY;
            }
        }

        public float[] GetGLVertices()
        {
            return _vertexData;
        }

        private static float Map(float value, float fromLow, float fromHigh, float toLow, float toHigh)
        {
            return (value - fromLow) * (toHigh - toLow) / (fromHigh - fromLow) + toLow;
        }

        private struct Color
        {
            public float r;
            public float g;
            public float b;
        }
    }
}
