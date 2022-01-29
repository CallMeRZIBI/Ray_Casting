using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RayCasting.Sound
{
    public interface IPlayer
    {
        bool Playing { get; }
        bool Paused { get; }
        Task Play();
        Task Pause();
        Task Resume();
        Task Stop();
    }
}
