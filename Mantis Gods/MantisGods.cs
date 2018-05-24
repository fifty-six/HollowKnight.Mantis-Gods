using System;
using System.Collections.Generic;
using Modding;
using UnityEngine;
using UObject = UnityEngine.Object;

namespace Mantis_Gods
{
    public class MantisGods : Modding.Mod, ITogglableMod
    {
        private const string version = "1.0.0";

        public override string GetVersion()
        {
            return version;
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
            GameManager.instance.gameObject.AddComponent<Mantis>();
        }

        private void AddComponent(SaveGameData data)
        {
            GameManager.instance.gameObject.AddComponent<Mantis>();
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
    }
}