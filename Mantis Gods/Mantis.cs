using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using Modding;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using static FsmUtil.FsmUtil;

namespace Mantis_Gods
{
    class Mantis : MonoBehaviour
    {
        public GameObject lord2, lord3, lord1;
        public bool p1, p2, p3;

        public void Start()
        {
            ModHooks.Instance.BeforeSceneLoadHook += reset;
        }

        private string reset(string sceneName)
        {
            p1 = false;
            p2 = false;
            p3 = false;
            lord1 = null;
            lord2 = null;
            lord3 = null;
            Log("Reset scene: " + sceneName);
            return sceneName;
        }

        public void updateLord(GameObject lord)
        {
            // get control fsm for lord
            PlayMakerFSM lordFSM = FSMUtility.LocateFSM(lord, "Mantis Lord");
            PlayMakerFSM lordHealth = FSMUtility.LocateFSM(lord, "health_manager_enemy");

            // boost health
            lordHealth.FsmVariables.GetFsmInt("HP").Value *= 3;

            // Remove Idle.
            Wait IdleAction;
            IdleAction = (Wait) GetAction(lordFSM, "Idle", 0);
            IdleAction.time.Value = 0;
            IdleAction = (Wait) GetAction(lordFSM, "Start Pause", 0);
            IdleAction.time.Value = 0;

            // Speed up throwing
            IdleAction = (Wait)GetAction(lordFSM, "Throw CD", 0);
            IdleAction.time.Value /= 3;

            // Get animations
            tk2dSpriteAnimator lordAnim = lord.GetComponent<tk2dSpriteAnimator>();

            // Increase fps for Dash recovery/arrival
            // This makes them faster.
            lordAnim.GetClipByName("Dash Recover").fps = 45;
            lordAnim.GetClipByName("Dash Arrive").fps = 60;
            lordAnim.GetClipByName("Dash Attack").fps = 45;

            // Repeat for down stab
            lordAnim.GetClipByName("Dstab Leave").fps = 45;
            lordAnim.GetClipByName("Dstab Arrive").fps = 30;

            // Repeat for wall throw
            lordAnim.GetClipByName("Wall Arrive").fps = 45;
            lordAnim.GetClipByName("Throw").fps = 75;

            Log("Updated lord: " + lord.name);

        }

        public void Update()
        {
            // If phase 1 hasn't been set to true, find the mantis lord
            // Unless it's phase 2 or phase 1 has been completed, leaving lord1 not null
            if (lord1 == null)
            {
                lord1 = GameObject.Find("Mantis Lord");
            }
            else if (!p2)
            {
                updateLord(lord1);
                p2 = true;
            }

            // If phase 2 hasn't been set
            // unless it's phase 3
            if (lord3 == null || lord2 == null)
            {
                lord2 = GameObject.Find("Mantis Lord S1");
                lord3 = GameObject.Find("Mantis Lord S2");
            }
            else if (!p3)
            {
                updateLord(lord2);
                updateLord(lord3);
                p3 = true;
            }
        }
    
        public void Log(String str)
        {
            Modding.Logger.Log("[Mantis Gods]: " + str);
        }
        
        public void OnDestroy()
        {
            ModHooks.Instance.BeforeSceneLoadHook -= reset;
        }
    }
}