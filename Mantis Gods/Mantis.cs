using HutongGames.PlayMaker.Actions;
using Modding;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static FsmUtil.FsmutilExt;
using USceneManager = UnityEngine.SceneManagement.SceneManager;

namespace Mantis_Gods
{
    internal class Mantis : MonoBehaviour
    {
        public GameObject MantisBattle;
        public GameObject Lord2, Lord3, Lord1, Shot;
        public GameObject plane;

        public static bool RainbowFloor;
        public static Color FloorColor;
        public int RainbowPos;
        public static int RainbowUpdateDelay;
        public int CurrentDelay;

        public readonly Dictionary<String, float> FPSdict = new Dictionary<String, float>
        {
            ["Dash Arrive"] = 30,
            ["Dash Antic"] = 35,
            ["Dash Attack"] = 45,
            ["Dash Recover"] = 45,
            ["Dash Leave"] = 45,
            ["Lance"] = 120,

            ["Dstab Leave"] = 45,
            ["Dstab Arrive"] = 34,
            ["Dstab Attack"] = 34,
            ["Dstab Land"] = 34,

            ["Wall Arrive"] = 45,
            ["Wall Leave 1"] = 15.6f,
            ["Wall Leave 2"] = 26,
            ["Wall Ready"] = 24,
            ["Throw"] = 55,
            ["Throw Antic"] = 15.6f,
        };

        public void Start()
        {
            USceneManager.sceneLoaded += Reset;
            ModHooks.Instance.LanguageGetHook += LangGet;
            ModHooks.Instance.GetPlayerBoolHook += GetBool;
            ModHooks.Instance.SetPlayerBoolHook += SetBool;
        }

        private void SetBool(string originalSet, bool value)
        {
            if (originalSet == "defeatedMantisLords" && PlayerData.instance.defeatedMantisLords && value)
            {
                Log("Defeated gods");
                MantisGods.SettingsInstance.DefeatedGods = true;
                Log("bool set");
                StartCoroutine(BattleBeat());
            }
            PlayerData.instance.SetBoolInternal(originalSet, value);
        }

        private bool GetBool(string originalSet)
        {
            if (originalSet == "defeatedMantisLords"
                && PlayerData.instance.defeatedMantisLords
                && !MantisGods.SettingsInstance.DefeatedGods
                && HeroController.instance.hero_state == GlobalEnums.ActorStates.no_input)
                return false;
            return PlayerData.instance.GetBoolInternal(originalSet);
        }

        // Used to override the text for Mantis Lords
        // Mantis Lords => Mantis Gods
        private string LangGet(string key, string sheetTitle)
        {
            if (key == "MANTIS_LORDS_MAIN" && PlayerData.instance.defeatedMantisLords)
                return "Gods";
            else
                return Language.Language.GetInternal(key, sheetTitle);
        }

        private void Reset(Scene arg0, LoadSceneMode arg1)
        {
            Log("Reset scene: " + arg0.name);
            Log(GameManager.instance.entryGateName);
            // if (!MantisGods.SettingsInstance.DefeatedGods)
            if (arg0.name != "Fungus2_15_boss") return;
            if (!PlayerData.instance.defeatedMantisLords) return;

            GameManager.instance.sm.mapZone = GlobalEnums.MapZone.WHITE_PALACE;
            PlayerData.instance.dreamReturnScene = "Fungus2_13";
            foreach (GameObject go in USceneManager.GetSceneByName("Fungus2_15").GetRootGameObjects())
            {
                if (go.name == "BossLoader") continue;
                Destroy(go);
            }

            Destroy(GameObject.Find("Mantis Battle/mantis_lord_opening_floors"));
            Destroy(GameObject.Find("Mantis Battle/mantis_lord_opening_floors (1)"));

            SceneParticlesController spc = FindObjectOfType<SceneParticlesController>();
            spc.DisableParticles();

            plane = new GameObject("Plane")
            {
                // make it able to be walked on
                tag = "HeroWalkable",
                layer = 8
            };

            // Dimensions
            MeshFilter meshFilter = plane.AddComponent<MeshFilter>();
            meshFilter.mesh = CreateMesh(200, 6.03f);
            MeshRenderer renderer = plane.AddComponent<MeshRenderer>();
            renderer.material.shader = Shader.Find("Particles/Additive");

            // Color
            if (RainbowFloor)
            {
                System.Random rand = new System.Random();
                RainbowPos = rand.Next(0, 767);
                FloorColor = GetNextRainbowColor();
                CurrentDelay = 0;
            }

            // Texture
            Texture2D tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, FloorColor);
            tex.Apply();

            // Renderer
            renderer.material.mainTexture = tex;
            renderer.material.color = Color.white;

            // Collider
            BoxCollider2D a = plane.AddComponent<BoxCollider2D>();
            a.isTrigger = false;

            plane.SetActive(true);
        }

        private Mesh CreateMesh(float width, float height)
        {
            Mesh m = new Mesh
            {
                name = "ScriptedMesh",
                vertices = new Vector3[]
                {
                    new Vector3(-width, -height, 0.01f),
                    new Vector3(width, -height, 0.01f),
                    new Vector3(width, height, 0.01f),
                    new Vector3(-width, height, 0.01f)
                },
                uv = new Vector2[]
                {
                    new Vector2 (0, 0),
                    new Vector2 (0, 1),
                    new Vector2 (1, 1),
                    new Vector2 (1, 0)
                },
                triangles = new int[] { 0, 1, 2, 0, 2, 3 }
            };
            m.RecalculateNormals();
            return m;
        }

        public void UpdateLord(GameObject lord)
        {
            if (lord == null) return;

            Log("Got Lord: " + lord.name);

            // get control fsm for lord
            PlayMakerFSM lordFSM = lord.LocateMyFSM("Mantis Lord");

            lord.GetComponent<DamageHero>().damageDealt *= 2;

            lord.FindTransformInChildren("Dash Hit")
                .GetComponent<DamageHero>()
                .damageDealt *= 2;

            lord.GetComponent<HealthManager>().hp *= 3;

            // Remove Idle.
            lordFSM.GetAction<Wait>("Idle", 0).time.Value = 0;
            lordFSM.GetAction<Wait>("Start Pause", 0).time.Value = 0;

            // Speed up throwing
            lordFSM.GetAction<Wait>("Throw CD", 0).time.Value /= 4;

            // Get animations
            tk2dSpriteAnimator lordAnim = lord.GetComponent<tk2dSpriteAnimator>();
            foreach (KeyValuePair<String, float> i in FPSdict)
            {
                lordAnim.GetClipByName(i.Key).fps = i.Value;
            }

            if (lord.name.Contains("S"))
            {
                UpdatePhase2(lordFSM);
            }

            Log("Updated lord: " + lord.name);
        }

        public void UpdatePhase2(PlayMakerFSM lordFSM)
        {
            SendRandomEventV3 rand = lordFSM.GetAction<SendRandomEventV3>("Attack Choice", 5);

            // DASH, DSTAB, THROW
            // 1, 1, 1 => 1/6
            rand.weights[2].Value /= 10f;
            Log("Updated Phase 2 Lord: " + lordFSM.name);
        }

        public IEnumerator BattleBeat()
        {
            Log("Started Battle Beat Coroutine");
            yield return new WaitForSeconds(13);
            Destroy(plane);
            yield return new WaitForSeconds(8);
            GameManager.instance.entryGateName = "bot3";
            try
            {
                GameManager.instance.GetType().GetField("entryDelay").SetValue(GameManager.instance, 1f);
                GameManager.instance.GetType().GetField("targetScene").SetValue(GameManager.instance, "Fungus2_14");
            }
            catch { }
            GameManager.instance.BeginSceneTransition(new GameManager.SceneLoadInfo
            {
                AlwaysUnloadUnusedAssets = true,
                EntryGateName = "bot3",
                PreventCameraFadeOut = false,
                SceneName = "Fungus2_14",
                Visualization = GameManager.SceneLoadVisualizations.Dream
            });
            Log("Finished Coroutine");
        }

        public void Update()
        {
            if (RainbowFloor && plane != null)
            {
                CurrentDelay++;
                if (CurrentDelay >= RainbowUpdateDelay)
                {
                    Texture2D tex = new Texture2D(1, 1);
                    tex.SetPixel(0, 0, GetNextRainbowColor());
                    tex.Apply();
                    plane.GetComponent<MeshRenderer>().material.mainTexture = tex;
                    CurrentDelay = 0;
                }
            }

            //Shot = GameObject.Find("Shot Mantis Lord");
            //PlayMakerFSM shotFSM = Shot?.LocateMyFSM("Control");
            //if (shotFSM != null)
            //    shotFSM.FsmVariables.FindFsmFloat("X Velocity").Value *= 2;

            //GameObject pls = GameObject.Find("Challenge Prompt");
            if (!PlayerData.instance.defeatedMantisLords) return;
            if (Lord1 != null && Lord2 != null && Lord3 != null) return;

            if (MantisBattle == null)
            {
                MantisBattle = GameObject.Find("Mantis Battle");
            }

            if (Lord1 == null)
            {
                Lord1 = GameObject.Find("Mantis Lord") ?? MantisBattle.FindGameObjectInChildren("Mantis Lord");
                UpdateLord(Lord1);
            }

            if (Lord3 != null && Lord2 != null) return;

            Lord2 = GameObject.Find("Mantis Lord S1") ?? MantisBattle.FindGameObjectInChildren("Mantis Lord S1");
            Lord3 = GameObject.Find("Mantis Lord S2") ?? MantisBattle.FindGameObjectInChildren("Mantis Lord S2");

            UpdateLord(Lord2);
            UpdateLord(Lord3);
        }

        public Color GetNextRainbowColor()
        {
            Color c = new Color();
            // the cycle repeats every 768

            int realCyclePos = RainbowPos % 768;
            c.a = 1.0f;
            if (realCyclePos < 256)
            {
                c.b = 0;
                c.r = (256 - realCyclePos) / (256f);
                c.g = realCyclePos / 256f;
            }
            else if (realCyclePos < 512)
            {
                c.b = (realCyclePos - 256) / 256f;
                c.r = 0;
                c.g = (512 - realCyclePos) / (256f);
            }
            else
            {
                c.b = (768 - realCyclePos) / 256f;
                c.r = (realCyclePos - 512) / 256f;
                c.g = 0;
            }

            RainbowPos++;
            return c;
        }

        public void Log(String str)
        {
            Modding.Logger.Log("[Mantis Gods]: " + str);
        }

        public void OnDestroy()
        {
            USceneManager.sceneLoaded -= Reset;
            ModHooks.Instance.LanguageGetHook -= LangGet;
        }
    }
}