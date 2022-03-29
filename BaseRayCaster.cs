using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RayCasting
{
    public abstract class BaseRayCaster : IRayCaster
    {

        // Timing
        internal double _deltaTime;
        internal Stopwatch _timer;

        // Map must have boundings otherwise the ray would just fly away exactly it will get out of the bounds of the array
        internal Map _map;

        internal double _moveSpeed;
        internal double _rotSpeed;

        public BaseRayCaster()
        {
            _deltaTime = 0;
            _timer = new Stopwatch();

            _moveSpeed = 0;
            _rotSpeed = 0;
        }

        public void CreateMap(Map map)
        {
            _map = map;
        }

        public abstract void CreateCamera(int screenWidth, int ScreenHeight, double StartingPosX, double StartingPosY, double dirX = -1, double dirY = 0, double planeX = 0, double planeY = 0.66);

        public abstract void CreateCamera(Camera camera);

        public abstract void Move(bool W_down, bool A_down, bool S_down, bool D_down, int cameraId, float moveSpeed = 5.0f, float rotSpeed = 3.0f);

        // Trying to find out how to create overridable method that would automatically calculate delta time at the start of it
        public abstract void UpdateRayCast();

        public void CalculateDeltaTime()
        {
            _deltaTime = _timer.Elapsed.TotalSeconds;
        }
    }
}
