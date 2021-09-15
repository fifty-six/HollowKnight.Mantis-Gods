using System;
using Modding;

namespace Mantis_Gods
{
    [Serializable]
    public class GlobalSettings
    {
        public bool RainbowFloor;
        public int RainbowUpdateDelay = 6;
        public float FloorColorRed;
        public float FloorColorGreen;
        public float FloorColorBlue;
        public float FloorColorAlpha;
        public bool NormalArena;
        public bool KeepSpikes = true;
    }

    [Serializable]
    public class LocalSettings 
    {
        public bool DefeatedGods;
    }
}