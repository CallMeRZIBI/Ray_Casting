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

        void UpdateRayCast(bool W_Down, bool A_Down, bool S_Down, bool D_Down);
        void CalculateDelatTime();
    }
}
