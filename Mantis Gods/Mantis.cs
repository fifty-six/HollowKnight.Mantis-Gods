using HutongGames.PlayMaker.Actions;
using Modding;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using static FsmUtil.FsmUtil;
using static FsmUtil.FsmutilExt;
using USceneManager = UnityEngine.SceneManagement.SceneManager;

namespace Mantis_Gods
{
    internal class Mantis : MonoBehaviour
    {
        public GameObject MantisBattle;
        public GameObject lord2, lord3, lord1, shot;
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

            // Repeat for down stab
            ["Dstab Leave"] = 45,
            ["Dstab Arrive"] = 34,
            ["Dstab Attack"] = 34,
            ["Dstab Land"] = 34,

            // Repeat for wall throw
            ["Wall Arrive"] = 45,
            ["Wall Leave 1"] = 15.6f,
            ["Wall Leave 2"] = 26,
            ["Wall Ready"] = 24,
            ["Throw"] = 55,
            ["Throw Antic"] = 15.6f,

            // Lance
            ["Lance"] = 120,
        };

        public void Start()
        {
            USceneManager.sceneLoaded += Reset;
            ModHooks.Instance.LanguageGetHook += LangGet;
        }

        // Used to override the text for Mantis Lords
        // Mantis Lords => Mantis Gods
        private string LangGet(string key, string sheetTitle)
        {
            if (key == "MANTIS_LORDS_MAIN")
                return "Gods";
            else
                return Language.Language.GetInternal(key, sheetTitle);
        }

        private void Reset(Scene arg0, LoadSceneMode arg1)
        {
            Log("Reset scene: " + arg0.name);

            if (PlayerData.instance.defeatedMantisLords)
                PlayerData.instance.defeatedMantisLords = false;

            if (arg0.name != "Fungus2_15_boss") return;

            GameManager.instance.sm.mapZone = GlobalEnums.MapZone.WHITE_PALACE;
            PlayerData.instance.dreamReturnScene = "Fungus2_13";
            foreach(GameObject go in USceneManager.GetSceneByName("Fungus2_15").GetRootGameObjects())
            {
                // TODO: Deep Spikes (x) -- use contains
                if (go.name != "BossLoader")
                    Destroy(go);
            }

            SceneParticlesController spc = FindObjectOfType<SceneParticlesController>();
            spc.DisableParticles();


            plane = new GameObject("Plane")
            {
                // make it able to be walked on 
                tag = "HeroWalkable",
                layer = 8
            };

            // Dimensions 
            MeshFilter meshFilter = (MeshFilter)plane.AddComponent(typeof(MeshFilter));
            meshFilter.mesh = CreateMesh(200, 6.03f);
            MeshRenderer renderer = plane.AddComponent<MeshRenderer>();
            renderer.material.shader = Shader.Find("Particles/Additive");

            // Color
            if (RainbowFloor)
            {
                System.Random rand = new System.Random();
                RainbowPos = rand.Next(0, 767);
                FloorColor = getNextRainbowColor();
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

        public Mesh CreateMesh(float width, float height)
        {
            Mesh m = new Mesh();
            m.name = "ScriptedMesh";
            m.vertices = new Vector3[]
            {
                new Vector3(-width, -height, 0.01f),
                new Vector3(width, -height, 0.01f),
                new Vector3(width, height, 0.01f),
                new Vector3(-width, height, 0.01f)
            };
            m.uv = new Vector2[]
            {
                new Vector2 (0, 0),
                new Vector2 (0, 1),
                new Vector2 (1, 1),
                new Vector2 (1, 0)
            };
            m.triangles = new int[] { 0, 1, 2, 0, 2, 3 };
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
            foreach(KeyValuePair<String,float> i in FPSdict)
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

        public void Update()
        {
            if (RainbowFloor)
            {
                CurrentDelay++;
                if (CurrentDelay >= RainbowUpdateDelay)
                {
                    Texture2D tex = new Texture2D(1, 1);
                    tex.SetPixel(0, 0, getNextRainbowColor());
                    tex.Apply();
                    plane.GetComponent<MeshRenderer>().material.mainTexture = tex;
                    CurrentDelay = 0;
                }
            }


            //shot = GameObject.Find("Shot Mantis Lord");
            //PlayMakerFSM shotFSM = shot?.LocateMyFSM("Control");
            //if (shotFSM != null)
            //    shotFSM.FsmVariables.FindFsmFloat("X Velocity").Value *= 2;

            //GameObject pls = GameObject.Find("Challenge Prompt");
            if (lord1 != null && lord2 != null && lord3 != null) return;

            if (MantisBattle == null)
            {
                MantisBattle = GameObject.Find("Mantis Battle");
            }

            if (lord1 == null)
            {
                lord1 = GameObject.Find("Mantis Lord") ?? MantisBattle.FindGameObjectInChildren("Mantis Lord");
                if (lord1 != null)
                {
                    UpdateLord(lord1);
                }
            }

            if (lord3 != null && lord2 != null) return;

            lord2 = GameObject.Find("Mantis Lord S1") ?? MantisBattle.FindGameObjectInChildren("Mantis Lord S1");
            lord3 = GameObject.Find("Mantis Lord S2") ?? MantisBattle.FindGameObjectInChildren("Mantis Lord S2");

            if (lord2 == null || lord3 == null) return;

            UpdateLord(lord2);
            UpdateLord(lord3);
        }

        public void rainbowFloorColorGen()
        {


        }

        public Color getNextRainbowColor()
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
