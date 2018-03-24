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
        private static string version = "Pre-alpha";

        public override string GetVersion()
        {
            return version;
        }

        public override void Initialize()
        {
            Log("Initializing.");
            ModHooks.Instance.AfterSavegameLoadHook += AddComponent;
        }

        private void AddComponent(SaveGameData data)
        {
            GameManager.instance.gameObject.AddComponent<Mantis>();
        }
    }
}
