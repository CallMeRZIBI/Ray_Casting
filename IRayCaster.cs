using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RayCasting
{
    interface IRayCaster
    {
        /// <summary>
        /// Create map with location of camera.
        /// </summary>
        /// <param name="map">Map object</param>
        /// <param name="StartingPosX">Initial position of camera in X axis</param>
        /// <param name="StartingPosY">Initial position of camera in Y axis</param>
        /// <param name="dirX">X direction (to which X position you are rotated)</param>
        /// <param name="dirY">Y direction (to which Y position you are rotated)</param>
        /// <param name="planeX"></param>
        /// <param name="planeY"></param>
        void CreateMap(Map map);

        void CreateCamera(Camera camera);
        void CreateCamera(int ScreenWidth, int ScreenHeight, double StartingPosX, double StartingPosY, double dirX = -1, double dirY = 0, double planeX = 0, double planeY = 0.66);

        /// <summary>
        /// Default built-in movement the doom style.
        /// </summary>
        /// <param name="W_Down"></param>
        /// <param name="A_Down"></param>
        /// <param name="S_Down"></param>
        /// <param name="D_Down"></param>
        /// <param name="moveSpeed"></param>
        /// <param name="rotSpeed"></param>
        void Move(bool W_down, bool A_down, bool S_down, bool D_down, int cameraId, float moveSpeed = 5.0f, float rotSpeed = 3.0f);

        /// <summary>
        /// Updates Frame.
        /// </summary>
        void UpdateRayCast();
        void CalculateDelatTime();
    }
}
