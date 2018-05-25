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
using static FsmUtil.FsmutilExt;
using System.Reflection;
using System.Collections;
using USceneManager = UnityEngine.SceneManagement.SceneManager;

namespace Mantis_Gods
{
    internal class Mantis : MonoBehaviour
    {
        public GameObject MantisBattle;
        public GameObject lord2, lord3, lord1, shot;

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
            //foreach (GameObject go in GetObjectsFromScene("Fungus2_15"))
            foreach (GameObject go in USceneManager.GetSceneByName("Fungus2_15").GetRootGameObjects())
            {
                // if (go.name != "Deep Spikes")
                    Destroy(go);
            }

            SceneParticlesController spc = FindObjectOfType<SceneParticlesController>();
            spc.DisableParticles();


            GameObject plane = new GameObject("Plane");

            // make it able to be walked on 
            plane.tag = "HeroWalkable";
            plane.layer = 8;

            // Dimensions 
            MeshFilter meshFilter = (MeshFilter)plane.AddComponent(typeof(MeshFilter));
            meshFilter.mesh = CreateMesh(200, 6.03f);
            MeshRenderer renderer = plane.AddComponent<MeshRenderer>();
            renderer.material.shader = Shader.Find("Particles/Additive");

            // Texture
            Texture2D tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, Color.black);
            tex.Apply();

            // Renderer
            renderer.material.mainTexture = tex;
            renderer.material.color = Color.black;

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

        //public static List<GameObject> GetObjectsFromScene(string sceneName)
        //{
        //    List<GameObject> list = new List<GameObject>();
        //    GameObject[] rootGameObjects = UnityEngine.SceneManagement.SceneManager.GetSceneByName(sceneName).GetRootGameObjects();
        //    if (rootGameObjects != null && rootGameObjects.Length != 0 && rootGameObjects != null && rootGameObjects.Length != 0)
        //    {
        //        list.AddRange(rootGameObjects);
        //        for (int i = 0; i < rootGameObjects.Length; i++)
        //        {
        //            List<Transform> list2 = new List<Transform>();
        //            foreach (object obj in rootGameObjects[i].transform)
        //            {
        //                Transform transform = (Transform)obj;
        //                list.Add(transform.gameObject);
        //                list2.Add(transform);
        //            }
        //            for (int j = 0; j < list2.Count; j++)
        //            {
        //                if (list2[j].childCount > 0)
        //                {
        //                    foreach (object obj2 in list2[j])
        //                    {
        //                        Transform transform2 = (Transform)obj2;
        //                        list.Add(transform2.gameObject);
        //                        list2.Add(transform2);
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    return list;
        //}

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
            Wait IdleAction = (Wait)GetAction(lordFSM, "Idle", 0);
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
            lordAnim.GetClipByName("Dash Arrive").fps = 30;
            lordAnim.GetClipByName("Dash Antic").fps = 35;
            lordAnim.GetClipByName("Dash Attack").fps = 45;
            lordAnim.GetClipByName("Dash Recover").fps = 45;
            lordAnim.GetClipByName("Dash Leave").fps = 45;

            // Repeat for down stab
            lordAnim.GetClipByName("Dstab Leave").fps = 45;
            // 30
            lordAnim.GetClipByName("Dstab Arrive").fps = 34;
            lordAnim.GetClipByName("Dstab Attack").fps = 34;
            lordAnim.GetClipByName("Dstab Land").fps = 34;

            // Repeat for wall throw
            lordAnim.GetClipByName("Wall Arrive").fps = 45;
            lordAnim.GetClipByName("Wall Leave 1").fps *= 1.3f;
            lordAnim.GetClipByName("Wall Leave 2").fps *= 1.3f;
            lordAnim.GetClipByName("Wall Ready").fps *= 2;
            lordAnim.GetClipByName("Throw").fps = 55;
            lordAnim.GetClipByName("Throw Antic").fps *= 1.3f;

            // Lance
            lordAnim.GetClipByName("Lance").fps *= 4;

            foreach(tk2dSpriteAnimationClip meme in lordAnim.Library.clips)
            {
                Log(meme.name);
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

        public void Log(String str)
        {
            Modding.Logger.Log("[Mantis Gods]: " + str);
        }

        public void OnDestroy()
        {
            USceneManager.sceneLoaded -= Reset;
        }
    }
}