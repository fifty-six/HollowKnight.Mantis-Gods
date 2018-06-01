using Modding;

namespace Mantis_Gods
{
    public static class VersionInfo
    {
    }

    public class MantisGlobalSettings : IModSettings
    {
        readonly public static int SettingsVer = 2;

        public void Reset()
        {
            BoolValues.Clear();
            IntValues.Clear();
            FloatValues.Clear();
            StringValues.Clear();

            //infiniteGrimmIntegration = true;
            // just kidding...

            // floor color in json. defaults to invisible and black.
            RainbowFloor = false;
            // update every x frames
            RainbowUpdateDelay = 6;
            FloorColorRed = 0.0f;
            FloorColorGreen = 0.0f;
            FloorColorBlue = 0.0f;
            FloorColorAlpha = 0.0f;

            SettingsVersion = SettingsVer;
        }

        public int SettingsVersion { get => GetInt(); set => SetInt(value); }
        public bool RainbowFloor { get => GetBool(); set => SetBool(value); }
        public int RainbowUpdateDelay { get => GetInt(); set => SetInt(value); }
        public float FloorColorRed { get => GetFloat(); set => SetFloat(value); }
        public float FloorColorGreen { get => GetFloat(); set => SetFloat(value); }
        public float FloorColorBlue { get => GetFloat(); set => SetFloat(value); }
        public float FloorColorAlpha { get => GetFloat(); set => SetFloat(value); }
    }

    public class MantisSettings : IModSettings
    {
        public bool DefeatedGods { get => GetBool(); set => SetBool(value); }
    }
}