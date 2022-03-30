using System;
using UnityEngine;

namespace Mantis_Gods
{
    [Serializable]
    public class GlobalSettings
    {
        public bool AllowInPantheons;
        
        public bool NormalArena;
        public bool KeepSpikes = true;
    }

    /*
     * Most Unity types are blacklisted due to having properties which lead to cycles,
     * so it's easier to just declare our own and operate on that
     */
    [Serializable]
    public struct SColor
    {
        public float r;
        public float g;
        public float b;
        public float a;

        public static implicit operator Color(SColor s) => new Color(s.r, s.g, s.b, s.a);
        public static implicit operator SColor(Color c) => new SColor { r = c.r, g = c.g, b = c.b, a = c.a };
    }

    [Serializable]
    public class LocalSettings 
    {
        public bool DefeatedGods;
    }
}