using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.IO;
using Modding;
using UnityEngine;
using UObject = UnityEngine.Object;
using UnityEngine.SceneManagement;

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
            ModHooks.Instance.AfterSavegameLoadHook += AddComponent;
            ModHooks.Instance.NewGameHook += NewGame;
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

        private void NewGame()
        {
            SetupSettings();

            if (GlobalSettings.RainbowFloor)
            {
                Mantis.RainbowFloor = true;
                Mantis.RainbowUpdateDelay = GlobalSettings.RainbowUpdateDelay;
            } else
            {
                Mantis.FloorColor = new Color(GlobalSettings.FloorColorRed, GlobalSettings.FloorColorGreen, GlobalSettings.FloorColorBlue, GlobalSettings.FloorColorAlpha);
            }

            SettingsInstance = Settings;
            GameManager.instance.gameObject.AddComponent<Mantis>();
        }

        private void AddComponent(SaveGameData data)
        {
            NewGame();
        }

        private void SetupSettings()
        {
            string settingsFilePath = Application.persistentDataPath + ModHooks.PathSeperator + GetType().Name + ".GlobalSettings.json";

            bool forceReloadGlobalSettings = (GlobalSettings != null && GlobalSettings.SettingsVersion != MantisGlobalSettings.SettingsVer);

            if (forceReloadGlobalSettings || !File.Exists(settingsFilePath))
            {
                GlobalSettings.Reset();
            }

            SaveGlobalSettings();
        }

        public void Unload()
        {
            ModHooks.Instance.AfterSavegameLoadHook -= AddComponent;
            ModHooks.Instance.NewGameHook -= NewGame;

            // in game
            if (GameManager.instance != null)
            {
                UObject.Destroy(GameManager.instance.gameObject.GetComponent<Mantis>());
            }
        }
    }

    public static class Extensions
    {
        public static GameObject FindGameObjectInChildren(this GameObject gameObject, string name)
        {
            if (gameObject == null)
                return null;

            foreach (var t in gameObject.GetComponentsInChildren<Transform>(true))
            {
                if (t.name == name)
                    return t.gameObject;
            }
            return null;
        }

        public static Transform FindTransformInChildren(this GameObject gameObject, string name)
        {
            if (gameObject == null)
                return null;

            foreach (var t in gameObject.GetComponentsInChildren<Transform>(true))
            {
                if (t.name == name)
                    return t;
            }
            return null;
        }
    }
}
