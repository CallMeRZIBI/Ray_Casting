﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RayCasting
{
    public class Camera
    {

        // TODO: Give The rendered output to the camera if there will be more cameras so each one would have its own render
        // TODO: Move the textured and untextured raycasters to the camera class so you could have "level" which can hold all the cmaeras, map, sprites...
        // TODO: Or make camera just struct that will contain position and leave it as it is cause that would be simplier and also the game engine would work as it works now
        //       and not as new engines where you have your level etc. that just holds all those things

        public double _posX { get; set; }
        public double _posY { get; set; }
        public double _dirX { get; set; }
        public double _dirY { get; set; }
        public double _planeX { get; set; }
        public double _planeY { get; set; }

        public Camera(){}

        public Camera(double posX, double posY, double dirX, double dirY, double planeX, double planeY)
        {
            _posX = posX;
            _posY = posY;
            _dirX = dirX;
            _dirY = dirY;
            _planeX = planeX;
            _planeY = planeY;
        }

        /// <summary>
        /// Sets camera position and rotation when use default movement is not choosen
        /// </summary>
        /// <param name="PosX">X position on map grid</param>
        /// <param name="PosY">Y position on map grid</param>
        /// <param name="DirX">X direction (to which X position you are rotated)</param>
        /// <param name="DirY">Y direction (to which Y position you are rotated)</param>
        public void SetCameraPos(double PosX, double PosY, double DirX, double DirY)
        {
            _posX = PosX;
            _posY = PosY;
            _dirX = DirX;
            _dirY = DirY;
        }

        // Movement won't be implemented in camera but in RayCaster because it needs map
    }
}
