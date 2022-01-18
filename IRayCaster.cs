using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RayCasting
{
    interface IRayCaster
    {
        void CreateMap(Map map, double StartingPosX, double StartingPosY, double dirX = -1, double dirY = 0, double planeX = 0, double planeY = 0.66);

        void Move(bool W_down, bool A_down, bool S_down, bool D_down);
        void UpdateRayCast();
        void CalculateDelatTime();
    }
}
