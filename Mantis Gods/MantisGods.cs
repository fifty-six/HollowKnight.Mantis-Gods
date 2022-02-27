using Modding;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UObject = UnityEngine.Object;

namespace Mantis_Gods
{
    public class MantisGods : Mod, ILocalSettings<LocalSettings>, IGlobalSettings<GlobalSettings>, ITogglableMod
    {
        public static MantisGods Instance;

        public LocalSettings Settings=new LocalSettings();
        
        private GlobalSettings _global=new GlobalSettings();

        public override string GetVersion() => Assembly.GetExecutingAssembly().GetName().Version.ToString();

        public override void Initialize()
        {
            Instance = this;
            
            Log("Initializing.");
            
            ModHooks.AfterSavegameLoadHook += AfterSavegameLoad;
            ModHooks.NewGameHook += AddComponent;

            // in game
            if (HeroController.instance != null && GameManager.instance.gameObject.GetComponent<Mantis>() == null)
            {
                GameManager.instance.gameObject.AddComponent<Mantis>();
            }
        }

        private void AddComponent()
        {
            if (_global.RainbowFloor)
            {
                Mantis.RainbowFloor = true;
                Mantis.RainbowUpdateDelay = _global.RainbowUpdateDelay;
            }
            else
            {
                Mantis.FloorColor = new Color(_global.FloorColorRed, _global.FloorColorGreen, _global.FloorColorBlue, _global.FloorColorAlpha);
            }

            Mantis.NormalArena = _global.NormalArena;
            Mantis.KeepSpikes = _global.KeepSpikes;

            GameManager.instance.gameObject.AddComponent<Mantis>();
        }

        private void AfterSavegameLoad(SaveGameData data) => AddComponent();

        public void Unload()
        {
            ModHooks.AfterSavegameLoadHook -= AfterSavegameLoad;
            ModHooks.NewGameHook -= AddComponent;

            // in game
            if (GameManager.instance != null)
            {
                UObject.Destroy(GameManager.instance.gameObject.GetComponent<Mantis>());
            }
        }

        public void OnLoadLocal(LocalSettings s) => Settings = s;

        public LocalSettings OnSaveLocal() => Settings;

        public void OnLoadGlobal(GlobalSettings s) => _global = s;

        public GlobalSettings OnSaveGlobal() => _global;
    }

    public static class Extensions
    {
        public static GameObject FindGameObjectInChildren(this GameObject gameObject, string name) => gameObject == null
            ? null
            : (from t in gameObject.GetComponentsInChildren<Transform>(true)
                where t.name == name
                select t.gameObject).FirstOrDefault();
    }
}