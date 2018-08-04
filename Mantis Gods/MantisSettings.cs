using Modding;

namespace Mantis_Gods
{
    public class MantisGlobalSettings : IModSettings
    {
        public const int SETTINGS_VER = 4;

        public void Reset()
        {
            BoolValues.Clear();
            IntValues.Clear();
            FloatValues.Clear();
            StringValues.Clear();

            // floor color in json. defaults to invisible and black.
            RainbowFloor = false;
            // update every x frames
            RainbowUpdateDelay = 6;
            FloorColorRed = 0.0f;
            FloorColorGreen = 0.0f;
            FloorColorBlue = 0.0f;
            FloorColorAlpha = 0.0f;
            NormalArena = false;
            KeepSpikes = true;

            SettingsVersion = SETTINGS_VER;
        }

        public  int    SettingsVersion    { get => GetInt();   private set => SetInt(value);   }
        public  bool   RainbowFloor       { get => GetBool();  private set => SetBool(value);  }
        public  int    RainbowUpdateDelay { get => GetInt();   private set => SetInt(value);   }
        public  float  FloorColorRed      { get => GetFloat(); private set => SetFloat(value); }
        public  float  FloorColorGreen    { get => GetFloat(); private set => SetFloat(value); }
        public  float  FloorColorBlue     { get => GetFloat(); private set => SetFloat(value); }
        public  float  FloorColorAlpha    { get => GetFloat(); private set => SetFloat(value); }
        public  bool   NormalArena        { get => GetBool();  private set => SetBool(value);  }
        public  bool   KeepSpikes         { get => GetBool();  private set => SetBool(value);  }
    }

    public class MantisSettings : IModSettings
    {
        public bool DefeatedGods { get => GetBool(); set => SetBool(value); }
    }
}