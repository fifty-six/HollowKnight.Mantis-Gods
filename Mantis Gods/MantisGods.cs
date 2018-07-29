using Modding;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using UObject = UnityEngine.Object;

namespace Mantis_Gods
{
    public class MantisGods : Mod<MantisSettings, MantisGlobalSettings>, ITogglableMod
    {
        public static MantisSettings SettingsInstance;

        public override string GetVersion()
        {
            return FileVersionInfo.GetVersionInfo(Assembly.GetAssembly(typeof(MantisGods)).Location).FileVersion;
        }

        public override void Initialize()
        {
            Log("Initializing.");
            ModHooks.Instance.AfterSavegameLoadHook += AfterSavegameLoad;
            ModHooks.Instance.NewGameHook += AddComponent;
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += ResetModSaveData;

            // in game
            if (HeroController.instance != null && GameManager.instance.gameObject.GetComponent<Mantis>() == null)
            {
                GameManager.instance.gameObject.AddComponent<Mantis>();
            }
        }

        // bug in modding api
        private void ResetModSaveData(Scene arg0, Scene arg1)
        {
            if (arg1.name == "Menu_Title")
            {
                Settings.DefeatedGods = false;
            }
        }

        private void AddComponent()
        {
            SetupSettings();

            if (GlobalSettings.RainbowFloor)
            {
                Mantis.RainbowFloor = true;
                Mantis.RainbowUpdateDelay = GlobalSettings.RainbowUpdateDelay;
            }
            else
            {
                Mantis.FloorColor = new Color(GlobalSettings.FloorColorRed, GlobalSettings.FloorColorGreen, GlobalSettings.FloorColorBlue, GlobalSettings.FloorColorAlpha);
            }

            Mantis.NormalArena = GlobalSettings.NormalArena;

            SettingsInstance = Settings;
            GameManager.instance.gameObject.AddComponent<Mantis>();
        }

        private void AfterSavegameLoad(SaveGameData data) => AddComponent();

        private void SetupSettings()
        {
            string settingsFilePath = Application.persistentDataPath + ModHooks.PathSeperator + GetType().Name + ".GlobalSettings.json";

            bool forceReloadGlobalSettings = GlobalSettings != null && GlobalSettings.SettingsVersion != MantisGlobalSettings.SETTINGS_VER;

            if (forceReloadGlobalSettings || !File.Exists(settingsFilePath))
            {
                GlobalSettings?.Reset();
            }

            SaveGlobalSettings();
        }

        public void Unload()
        {
            ModHooks.Instance.AfterSavegameLoadHook -= AfterSavegameLoad;
            ModHooks.Instance.NewGameHook -= AddComponent;

            // in game
            if (GameManager.instance != null)
            {
                UObject.Destroy(GameManager.instance.gameObject.GetComponent<Mantis>());
            }
        }
    }

    public static class Extensions
    {
        public static GameObject FindGameObjectInChildren(this GameObject gameObject, string name) => gameObject == null
            ? null
            : (from t in gameObject.GetComponentsInChildren<Transform>(true)
                where t.name == name
                select t.gameObject).FirstOrDefault();

        public static Transform FindTransformInChildren(this GameObject gameObject, string name) => gameObject == null
            ? null
            : gameObject.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == name);
    }
}