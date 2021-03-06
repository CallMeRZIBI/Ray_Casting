using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RayCasting
{
    public class UntexturedRayCaster : BaseRayCaster, IRayCaster
    {
        private readonly int _screenWidth;
        private readonly int _screenHeight;
        private readonly int _renderWidth;
        private readonly int _renderHeight;

        // VertexData
        private float[] _vertexData;
        public float LineThickness { get; private set; }

        private Camera _camera;

        /// <summary>
        /// Constructor of Raycaster, declaring needed values for rendering.
        /// </summary>
        /// <param name="screenWidth">Getting width of rendered screen.</param>
        /// <param name="screenHeight">Getting Height of rendered screen.</param>
        /// <param name="rederingScale">Getting scale of rendering. Be careful with values, so try if screenWidth and screenHeight multiplied by renderingScale always outputs full number. Otherwise weird graphical artifacts can happen. Also don't use numbers larger than one.</param>
        public UntexturedRayCaster(int screenWidth, int screenHeight, float rederingScale = 1) : base()
        {
            _screenWidth = screenWidth;
            _screenHeight = screenHeight;
            // It is sometimes creating weird graphical artifacts so maybye fix it later?
            _renderWidth = (int)(_screenWidth * rederingScale);
            _renderHeight = (int)(_screenHeight * rederingScale);
            LineThickness = _screenWidth / _renderWidth;

            _vertexData = new float[_screenWidth * 12];
        }

        // Update RayCast calculations with given pos and save vertices in OpenGL format which later can be fromated
        /// <summary>
        /// Updates Frame.
        /// </summary>
        public override void UpdateRayCast()
        {
            for (int x = 0; x < _renderWidth; x++)
            {
                // Calculate ray position and direction
                double cameraX = 2 * x / (double)_renderWidth - 1; // X coordinate in camera space
                double rayDirX = _camera.dirX + _camera.planeX * cameraX;
                double rayDirY = _camera.dirY + _camera.planeY * cameraX;

                // Which box of the map we're in
                int mapX = (int)_camera.posX;
                int mapY = (int)_camera.posY;

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
                    sideDistX = (_camera.posX - mapX) * deltaDistX;
                }
                else
                {
                    stepX = 1;
                    sideDistX = (mapX + 1.0 - _camera.posX) * deltaDistX;
                }
                if (rayDirY < 0)
                {
                    stepY = -1;
                    sideDistY = (_camera.posY - mapY) * deltaDistY;
                }
                else
                {
                    stepY = 1;
                    sideDistY = (mapY + 1.0 - _camera.posY) * deltaDistY;
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
        }

        /// <summary>
        /// Default built-in movement the doom style.
        /// </summary>
        /// <param name="W_Down"></param>
        /// <param name="A_Down"></param>
        /// <param name="S_Down"></param>
        /// <param name="D_Down"></param>
        /// <param name="moveSpeed"></param>
        /// <param name="rotSpeed"></param>
        public override void Move(bool W_Down, bool A_Down, bool S_Down, bool D_Down, int cameraId = 0, float moveSpeed = 5.0f, float rotSpeed = 3.0f)
        {
            float radius = 0.25f;

            // Speed modifiers
            _moveSpeed = _deltaTime * moveSpeed; // Constant value is idk
            _rotSpeed = _deltaTime * rotSpeed; // Constant value is idk

            // Timing for input and FPS counter
            _timer.Stop();
            CalculateDeltaTime();
            _timer.Reset();

            // Move froward if no wall in front of you
            if (W_Down)
            {
                // TODO: Figure out how to make circle collider around camera
                // This is square collider
                if (_camera.dirX > 0) { if (_map.map[(int)((_camera.posX + _camera.dirX * _moveSpeed) + radius), (int)(_camera.posY)] == 0) _camera.posX += _camera.dirX * _moveSpeed; }
                else { if (_map.map[(int)((_camera.posX + _camera.dirX * _moveSpeed) - radius), (int)(_camera.posY)] == 0) _camera.posX += _camera.dirX * _moveSpeed; }
                if (_camera.dirY > 0) { if (_map.map[(int)(_camera.posX), (int)((_camera.posY + _camera.dirY * _moveSpeed) + radius)] == 0) _camera.posY += _camera.dirY * _moveSpeed; }
                else { if (_map.map[(int)(_camera.posX), (int)((_camera.posY + _camera.dirY * _moveSpeed) - radius)] == 0) _camera.posY += _camera.dirY * _moveSpeed; }
            }
            // Move backwards if no wall behind you
            if (S_Down)
            {
                if (_camera.dirX > 0) { if (_map.map[(int)((_camera.posX - _camera.dirX * _moveSpeed) - radius), (int)(_camera.posY)] == 0) _camera.posX -= _camera.dirX * _moveSpeed; }
                else { if (_map.map[(int)((_camera.posX - _camera.dirX * _moveSpeed) + radius), (int)(_camera.posY)] == 0) _camera.posX -= _camera.dirX * _moveSpeed; }
                if (_camera.dirY > 0) { if (_map.map[(int)(_camera.posX), (int)((_camera.posY - _camera.dirY * _moveSpeed) - radius)] == 0) _camera.posY -= _camera.dirY * _moveSpeed; }
                else { if (_map.map[(int)(_camera.posX), (int)((_camera.posY - _camera.dirY * _moveSpeed) + radius)] == 0) _camera.posY -= _camera.dirY * _moveSpeed; }
            }
            // Rotate to the right
            if (D_Down)
            {
                // both camera direction and camera plane must be rotated
                double oldDirX = _camera.dirX;
                _camera.dirX = _camera.dirX * Math.Cos(-_rotSpeed) - _camera.dirY * Math.Sin(-_rotSpeed);
                _camera.dirY = oldDirX * Math.Sin(-_rotSpeed) + _camera.dirY * Math.Cos(-_rotSpeed);
                double oldPlaneX = _camera.planeX;
                _camera.planeX = _camera.planeX * Math.Cos(-_rotSpeed) - _camera.planeY * Math.Sin(-_rotSpeed);
                _camera.planeY = oldPlaneX * Math.Sin(-_rotSpeed) + _camera.planeY * Math.Cos(-_rotSpeed);
            }
            // Rotate to the left
            if (A_Down)
            {
                // both camera direction and camera plane must be rotated
                double oldDirX = _camera.dirX;
                _camera.dirX = _camera.dirX * Math.Cos(_rotSpeed) - _camera.dirY * Math.Sin(_rotSpeed);
                _camera.dirY = oldDirX * Math.Sin(_rotSpeed) + _camera.dirY * Math.Cos(_rotSpeed);
                double oldPlaneX = _camera.planeX;
                _camera.planeX = _camera.planeX * Math.Cos(_rotSpeed) - _camera.planeY * Math.Sin(_rotSpeed);
                _camera.planeY = oldPlaneX * Math.Sin(_rotSpeed) + _camera.planeY * Math.Cos(_rotSpeed);
            }

            // Timing for frame time
            _timer.Start();
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
            _camera.posX = PosX;
            _camera.posY = PosY;
            _camera.dirX = DirX;
            _camera.dirY = DirY;
        }

        public float[] GetGLVertices()
        {
            return _vertexData;
        }

        private static float Map(float value, float fromLow, float fromHigh, float toLow, float toHigh)
        {
            return (value - fromLow) * (toHigh - toLow) / (fromHigh - fromLow) + toLow;
        }

        public override void CreateCamera(Camera camera)
        {
            _camera = camera;
        }

        public override void CreateCamera(int ScreenWidth, int ScreenHeight, double StartingPosX, double StartingPosY, double dirX = -1, double dirY = 0, double planeX = 0, double planeY = 0.66)
        {
            _camera = new Camera(ScreenWidth, ScreenHeight ,StartingPosX, StartingPosY, dirX, dirY, planeX, planeY);
        }

        private struct Color
        {
            public float r;
            public float g;
            public float b;
        }
    }
}
