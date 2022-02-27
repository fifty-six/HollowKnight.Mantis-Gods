using Modding;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using UnityEngine;
using UObject = UnityEngine.Object;

namespace Mantis_Gods
{
    [UsedImplicitly]
    public class MantisGods : Mod, ILocalSettings<LocalSettings>, IGlobalSettings<GlobalSettings>, ITogglableMod
    {
        public static MantisGods Instance;

        public LocalSettings LocalData = new LocalSettings();
        
        private GlobalSettings _settings = new GlobalSettings();

        public override string GetVersion() => Assembly.GetExecutingAssembly().GetName().Version.ToString();

        public override void Initialize()
        {
            Instance = this;
            
            Log("Initializing.");
            
            ModHooks.AfterSavegameLoadHook += AfterSavegameLoad;
            ModHooks.NewGameHook += AddComponent;

            // In game
            if (HeroController.instance != null && GameManager.instance.gameObject.GetComponent<Mantis>() == null)
                AddComponent();
        }

        private void AddComponent()
        {
            if (_settings.RainbowFloor) 
            {
                Mantis.RainbowFloor = true;
                Mantis.RainbowUpdateDelay = _settings.RainbowUpdateDelay;
            }
            else
            {
                Mantis.FloorColor = _settings.FloorColor;
            }

            Mantis.NormalArena = _settings.NormalArena;
            Mantis.KeepSpikes = _settings.KeepSpikes;

            GameManager.instance.gameObject.AddComponent<Mantis>();
        }

        private void AfterSavegameLoad(SaveGameData data) => AddComponent();

        public void Unload()
        {
            ModHooks.AfterSavegameLoadHook -= AfterSavegameLoad;
            ModHooks.NewGameHook -= AddComponent;

            // In game
            if (GameManager.instance != null)
                UObject.Destroy(GameManager.instance.gameObject.GetComponent<Mantis>());
        }

        public void OnLoadLocal(LocalSettings s) => LocalData = s;

        public LocalSettings OnSaveLocal() => LocalData;

        public void OnLoadGlobal(GlobalSettings s) => _settings = s;

        public GlobalSettings OnSaveGlobal() => _settings;
    }

    public static class Extensions
    {
        public static GameObject FindGameObjectInChildren(this GameObject gameObject, string name) => gameObject == null
            ? null
            : (
                gameObject.GetComponentsInChildren<Transform>(true)
                          .Where(t => t.name == name)
                          .Select(t => t.gameObject)
                          .FirstOrDefault()
            );
    }
}