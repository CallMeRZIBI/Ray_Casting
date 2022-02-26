using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RayCasting
{
    public class Camera
    {
        public double posX { get; set; }
        public double posY { get; set; }
        public double dirX { get; set; }
        public double dirY { get; set; }
        public double planeX { get; set; }
        public double planeY { get; set; }

        public int screenWidth { get; set; }
        public int screenHeight { get; set; }
        public byte[,,] buffer { get; set; }
        public double[] ZBuffer { get; set; }
        public int Id { get; private set; }
        private static int Count = 0;

        public Camera(int IScreenWidth, int IScreenHeight)
        {
            createId();
            createBuffer(IScreenWidth, IScreenHeight);
        }

        public Camera(int IScreenWidth, int IScreenHeight, double IposX, double IposY, double IdirX = -1, double IdirY = 0, double IplaneX = 0, double IplaneY = 0.66)
        {
            createId();
            createBuffer(IScreenWidth, IScreenHeight);

            screenWidth = IScreenWidth;
            screenHeight = IScreenHeight;
            posX = IposX;
            posY = IposY;
            dirX = IdirX;
            dirY = IdirY;
            planeX = IplaneX;
            planeY = IplaneY;

            // Debug
            Console.WriteLine($"Debug:\nCreating Camera with Id {Id}");
        }

        private void createBuffer(int X, int Y)
        {
            buffer = new byte[Y, X, 3];
            ZBuffer = new double[X];
        }

        private void createId()
        {
            Id = Count;
            Count += 1;
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
            posX = PosX;
            posY = PosY;
            dirX = DirX;
            dirY = DirY;
        }


        public void loadBuffer(byte[,,] buff)
        {
            buffer = new byte[buff.GetLength(0),buff.GetLength(1),buff.GetLength(2)];
            Array.Copy(buff, buffer, buff.Length);                                      // Needs to be copy otherwise it will just take reference of the array and overwrite with each other camera
        }
        // Movement won't be implemented in camera but in RayCaster because it needs map
    }
}
