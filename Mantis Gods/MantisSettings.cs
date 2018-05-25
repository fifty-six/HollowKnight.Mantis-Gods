using Modding;

namespace Mantis_Gods
{
    public class VersionInfo
    {
        readonly public static int SettingsVer = 2;
    }

    public class MantisGlobalSettings : IModSettings
    {


        public void Reset()
        {
            BoolValues.Clear();
            IntValues.Clear();
            FloatValues.Clear();
            StringValues.Clear();

            //infiniteGrimmIntegration = true;
            // just kidding...


            // floor color in json. defaults to invisible and black.
            rainbowFloor = false;
            // update every x frames
            rainbowUpdateDelay = 6;
            floorColorRed = 0.0f;
            floorColorGreen = 0.0f;
            floorColorBlue = 0.0f;
            floorColorAlpha = 0.0f;

            SettingsVersion = VersionInfo.SettingsVer;
        }
        public int SettingsVersion { get => GetInt(); set => SetInt(value); }

        public bool rainbowFloor { get => GetBool(); set => SetBool(value); }
        public int rainbowUpdateDelay { get => GetInt(); set => SetInt(value); }
        public float floorColorRed { get => GetFloat(); set => SetFloat(value); }
        public float floorColorGreen { get => GetFloat(); set => SetFloat(value); }
        public float floorColorBlue { get => GetFloat(); set => SetFloat(value); }
        public float floorColorAlpha { get => GetFloat(); set => SetFloat(value); }
    }


    public class MantisSettings : IModSettings
    {
        // none needed but this class is

    }



}
