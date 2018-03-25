using System;
using System.Collections.Generic;
using System.Linq;
using Modding;
using GlobalEnums;
using UnityEngine;
using HutongGames.PlayMaker;
using FsmUtil;

namespace Mantis_Gods
{
    public class MantisGods : Modding.Mod
    {
        private static string version = "1.0.0";

        public override string GetVersion()
        {
            return version;
        }

        public override void Initialize()
        {
            Log("Initializing.");
            ModHooks.Instance.AfterSavegameLoadHook += AddComponent;
            ModHooks.Instance.NewGameHook += NewGame;
        }

        private void NewGame()
        {
            GameManager.instance.gameObject.AddComponent<Mantis>();
        }

        private void AddComponent(SaveGameData data)
        {
            GameManager.instance.gameObject.AddComponent<Mantis>();
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
