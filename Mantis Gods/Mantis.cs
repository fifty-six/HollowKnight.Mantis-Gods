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
using System.Reflection;

namespace Mantis_Gods
{
    class Mantis : MonoBehaviour
    {
        public GameObject MantisBattle;
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
            // checks for null due to patch changes
            if (lordHealth != null)
            {
                lordHealth.FsmVariables.GetFsmInt("HP").Value *= 3;
            } else {
                var lordHM = lord.GetComponent("HealthManager");
                FieldInfo fi = lordHM.GetType().GetField("hp");
                var temp = (int)fi.GetValue(lordHM);
                Log(temp.ToString());
                fi.SetValue(lordHM, temp * 3);
                Log(fi.GetValue(lordHM).ToString());
            }

            // Remove Idle.
            Wait IdleAction;
            IdleAction = (Wait)GetAction(lordFSM, "Idle", 0);
            IdleAction.time.Value = 0;
            IdleAction = (Wait)GetAction(lordFSM, "Start Pause", 0);
            IdleAction.time.Value = 0;

            // Speed up throwing
            IdleAction = (Wait)GetAction(lordFSM, "Throw CD", 0);
            IdleAction.time.Value /= 4;

            // Get animations
            tk2dSpriteAnimator lordAnim = lord.GetComponent<tk2dSpriteAnimator>();

            // Increase fps for Dash recovery/arrival
            // This makes them faster.
            lordAnim.GetClipByName("Dash Recover").fps = 45;
            lordAnim.GetClipByName("Dash Arrive").fps = 60;
            lordAnim.GetClipByName("Dash Attack").fps = 45;


            // Repeat for down stab
            lordAnim.GetClipByName("Dstab Leave").fps = 45;
            // 30
            lordAnim.GetClipByName("Dstab Arrive").fps = 34;

            // Repeat for wall throw
            lordAnim.GetClipByName("Wall Arrive").fps = 45;
            lordAnim.GetClipByName("Throw").fps = 75;

            if (lord.name.Contains("S"))
            {
                updatePhase2(lordFSM);
            }

            Log("Updated lord: " + lord.name);

        }

        public void updatePhase2(PlayMakerFSM lordFSM)
        {
            SendRandomEventV3 rand = (SendRandomEventV3) GetAction(lordFSM, "Attack Choice", 5);

            // DASH, DSTAB, THROW
            // 1, 1, 1 => 1/6
            rand.weights[2].Value /= 6f;
            Log("Updated Phase 2 Lord: " + lordFSM.name);
        }

        public void Update()
        {
            // If phase 1 hasn't been set to true, find the mantis lord
            // Unless it's phase 2 or phase 1 has been completed, leaving lord1 not null
            if (lord1 == null | lord2 == null | lord3 == null)
            {
                if (MantisBattle == null)
                {
                    MantisBattle = GameObject.Find("Mantis Battle");
                    if (MantisBattle != null)
                    {
                        Log("Got mantis battle");
                    }
                }

                if (lord1 == null)
                {
                    lord1 = Extensions.FindGameObjectInChildren(MantisBattle, "Mantis Lord");
                    if (lord1 != null)
                    {
                        Log("Got mantis lord");

                        // idk why this works
                        // but it does
                        updateLord(lord1);
                        p2 = true;
                    }
                    else
                    {
                        // for earlier patches
                        lord1 = GameObject.Find("Mantis Lord");
                    }
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
                    //lord2 = GameObject.Find("Mantis Lord S1");
                    //lord3 = GameObject.Find("Mantis Lord S2");
                    lord2 = Extensions.FindGameObjectInChildren(MantisBattle, "Mantis Lord S1");
                    lord3 = Extensions.FindGameObjectInChildren(MantisBattle, "Mantis Lord S2");

                    if (lord2 != null && lord3 != null)
                    {
                        Log("Got mantis lordSSS");
                        updateLord(lord2);
                        updateLord(lord3);
                        p3 = true;
                    }
                    else
                    {
                        lord2 = GameObject.Find("Mantis Lord S1");
                        lord3 = GameObject.Find("Mantis Lord S2");
                    }
                }
                else if (!p3)
                {
                    updateLord(lord2);
                    updateLord(lord3);
                    p3 = true;
                }
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