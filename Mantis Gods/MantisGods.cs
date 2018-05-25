using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.IO;
using Modding;
using UnityEngine;
using UObject = UnityEngine.Object;

namespace Mantis_Gods
{
    public class MantisGods : Modding.Mod <MantisSettings, MantisGlobalSettings>, ITogglableMod
    {
        private const string version = "1.0.0";

        public override string GetVersion()
        {
            return FileVersionInfo.GetVersionInfo(Assembly.GetAssembly(typeof(MantisGods)).Location).FileVersion;
        }

        public override void Initialize()
        {
            Log("Initializing.");
            ModHooks.Instance.AfterSavegameLoadHook += AddComponent;
            ModHooks.Instance.NewGameHook += NewGame;

            // in game
            if (HeroController.instance != null && GameManager.instance.gameObject.GetComponent<Mantis>() == null)
            {
                GameManager.instance.gameObject.AddComponent<Mantis>();
            }
        }

        private void NewGame()
        {
            SetupSettings();

            if (GlobalSettings.rainbowFloor)
            {
                Mantis.rainbowFloor = true;
            } else
            {
                Mantis.floorColor = new Color(GlobalSettings.floorColorRed, GlobalSettings.floorColorGreen, GlobalSettings.floorColorBlue, GlobalSettings.floorColorAlpha);
            }

            GameManager.instance.gameObject.AddComponent<Mantis>();
        }

        private void AddComponent(SaveGameData data)
        {
            NewGame();
        }

        private void SetupSettings()
        {
            string settingsFilePath = Application.persistentDataPath + ModHooks.PathSeperator + GetType().Name + ".GlobalSettings.json";

            bool forceReloadGlobalSettings = (GlobalSettings != null && GlobalSettings.SettingsVersion != VersionInfo.SettingsVer);

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
                GameObject.Destroy(GameManager.instance.gameObject.GetComponent<Mantis>());
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
