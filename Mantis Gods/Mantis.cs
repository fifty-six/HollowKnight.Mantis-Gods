using HutongGames.PlayMaker.Actions;
using Modding;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using GlobalEnums;
using UnityEngine;
using UnityEngine.SceneManagement;
using Logger = Modding.Logger;
using Random = System.Random;
using USceneManager = UnityEngine.SceneManagement.SceneManager;

namespace Mantis_Gods
{
    internal class Mantis : MonoBehaviour
    {
        public static Color FloorColor;
        public static bool RainbowFloor;
        public static int RainbowUpdateDelay;
        public static bool NormalArena;
        public int CurrentDelay;
        public int RainbowPos;

        // ReSharper disable once InconsistentNaming
        // Dictionary of attacks and the new FPS values they're set to
        // Higher is faster.
        private readonly Dictionary<string, float> FPSdict = new Dictionary<string, float>
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

        public GameObject Lord2, Lord3, Lord1;
        public GameObject MantisBattle;
        public GameObject Plane;

        private IEnumerator BattleBeat()
        {
            Log("Started Battle Beat Coroutine");
            
            yield return new WaitForSeconds(13);
            
            Destroy(Plane);
            
            yield return new WaitForSeconds(6);
            
            GameManager.instance.GetType().GetField("entryDelay", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(GameManager.instance, 1f);
            GameManager.instance.GetType().GetField("targetScene", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(GameManager.instance, "Fungus2_14");
            GameManager.instance.entryGateName = "bot3";
            
            // Go to Fungus2_15 from the bottom 3 entrance
            // This is where you would fall down from where claw is into the arena
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

        private Color GetNextRainbowColor()
        {
            Color c = new Color();
            // the cycle repeats every 768

            int realCyclePos = RainbowPos % 768;
            c.a = 1.0f;
            if (realCyclePos < 256)
            {
                c.b = 0;
                c.r = (256 - realCyclePos) / 256f;
                c.g = realCyclePos / 256f;
            }
            else if (realCyclePos < 512)
            {
                c.b = (realCyclePos - 256) / 256f;
                c.r = 0;
                c.g = (512 - realCyclePos) / 256f;
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

        private static void Log(string str)
        {
            Logger.Log("[Mantis Gods]: " + str);
        }

        public void OnDestroy()
        {
            // So that they don't remain after you quit out
            USceneManager.sceneLoaded -= ResetScene;
            ModHooks.Instance.LanguageGetHook -= LangGet;
            ModHooks.Instance.GetPlayerBoolHook -= GetBool;
            ModHooks.Instance.SetPlayerBoolHook -= SetBool;
        }

        public void Start()
        {
            USceneManager.sceneLoaded += ResetScene;
            ModHooks.Instance.LanguageGetHook += LangGet;
            ModHooks.Instance.GetPlayerBoolHook += GetBool;
            ModHooks.Instance.SetPlayerBoolHook += SetBool;
            ModHooks.Instance.ObjectPoolSpawnHook += ShotHandler;
        }

        private static GameObject ShotHandler(GameObject go)
        {
            // if you haven't defeated the normal lords do nothing
            if (!PlayerData.instance.defeatedMantisLords) return go;
            
            // check for spikes
            if (!go.name.Contains("Shot Mantis Lord")) return go;
            
            PlayMakerFSM shotFsm = go.LocateMyFSM("Control");
            if (shotFsm == null) return go;
            // 40, 40, 20, 20
            shotFsm.GetAction<SetFloatValue>("Set L", 0).floatValue = -48f;
            shotFsm.GetAction<SetFloatValue>("Set R", 0).floatValue = 48f;
            shotFsm.GetAction<SetFloatValue>("Set L", 1).floatValue = -20f;
            shotFsm.GetAction<SetFloatValue>("Set R", 1).floatValue = 20f;
                
            return go;
        }

        public void Update()
        {
            if (RainbowFloor && Plane != null)
            {
                CurrentDelay++;
                if (CurrentDelay >= RainbowUpdateDelay)
                {
                    Texture2D tex = new Texture2D(1, 1);
                    tex.SetPixel(0, 0, GetNextRainbowColor());
                    tex.Apply();
                    Plane.GetComponent<MeshRenderer>().material.mainTexture = tex;
                    CurrentDelay = 0;
                }
            }

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

        private void UpdateLord(GameObject lord)
        {
            if (lord == null) return;

            Log("Got Lord: " + lord.name);

            // Get control FSM for lord
            PlayMakerFSM lordFsm = lord.LocateMyFSM("Mantis Lord");

            // Double contact damage
            foreach (DamageHero x in lord.GetComponents<DamageHero>())
                x.damageDealt *= 2;

            // Double dash damage
            foreach (DamageHero x in lord.FindTransformInChildren("Dash Hit").GetComponents<DamageHero>())
                x.damageDealt *= 2;

            // 3x HP
            lord.GetComponent<HealthManager>().hp *= 3;

            // Remove some waits
            // shit hack
            lordFsm.GetAction<Wait>("Idle", 0).time.Value = 0f;//0.0001f;
            lordFsm.GetAction<Wait>("Start Pause", 0).time.Value = 0;
            lordFsm.GetAction<Wait>("Throw CD", 0).time.Value = 0;

            // new
            lordFsm.GetAction<Wait>("Arrive Pause", 0).time.Value /= 2;
            lordFsm.GetAction<Wait>("Arrive", 4).time.Value /= 3;
            lordFsm.GetAction<Wait>("Leave Pause", 0).time.Value /= 2;
            lordFsm.GetAction<Wait>("After Throw Pause", 3).time.Value /= 4;//= 0.0001f;

            // Get animations
            tk2dSpriteAnimator lordAnim = lord.GetComponent<tk2dSpriteAnimator>();
            
            // Set the fps values as indicated in the dictionary
            foreach (KeyValuePair<string, float> i in FPSdict)
            {
                lordAnim.GetClipByName(i.Key).fps = i.Value;
            }

            if (lord.name.Contains("S"))
            {
                UpdatePhase2(lordFsm);
            }
            else
            {
                lordFsm.GetAction<Wait>("Leave Pause", 0).time.Value /= 3;
            }

            Log("Updated lord: " + lord.name);
        }

        private static void UpdatePhase2(PlayMakerFSM lordFsm)
        {
            // DASH, DSTAB, THROW
            // 1, 1, 1 => 1/6
            lordFsm.GetAction<SendRandomEventV3>("Attack Choice", 5).weights[2].Value /= 10f;
            Log("Updated Phase 2 Lord: " + lordFsm.name);
        }

        private static Mesh CreateMesh(float width, float height)
        {
            Mesh m = new Mesh
            {
                name = "ScriptedMesh",
                vertices = new Vector3[]
                {
                    new Vector3(-width, -height, 0.01f),
                    new Vector3(width,  -height, 0.01f),
                    new Vector3(width,  height,  0.01f),
                    new Vector3(-width, height,  0.01f)
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

        private static bool GetBool(string originalSet)
        {
            if (originalSet == "defeatedMantisLords"
                && PlayerData.instance.defeatedMantisLords
                && !MantisGods.SettingsInstance.DefeatedGods
                && HeroController.instance.hero_state == ActorStates.no_input)
            {
                return false;
            }

            return PlayerData.instance.GetBoolInternal(originalSet);
        }

        // Used to override the text for Mantis Lords
        // Mantis Lords => Mantis Gods
        private string LangGet(string key, string sheetTitle)
        {
            if (key == "MANTIS_LORDS_MAIN" && PlayerData.instance.defeatedMantisLords)
                return "Gods";
            return Language.Language.GetInternal(key, sheetTitle);
        }

        private void ResetScene(Scene arg0, LoadSceneMode arg1)
        {
            Lord1 = Lord2 = Lord3 = null;
            
            Log("Reset scene: " + arg0.name);
            Log(GameManager.instance.entryGateName);

            if (arg0.name != "Fungus2_15_boss") return;
            if (!PlayerData.instance.defeatedMantisLords) return;

            // Set the mapZone to White Palace so when you die
            // you don't spawn a shade and don't lose geo
            GameManager.instance.sm.mapZone = MapZone.WHITE_PALACE;

            if (NormalArena)
            {
                // :(
                return;
            }
            
            // Destroy all game objects that aren't the BossLoader
            // BossLoader unloads the mantis lord stuff after you die
            foreach (GameObject go in USceneManager.GetSceneByName("Fungus2_15").GetRootGameObjects())
            {
                if (go.name == "BossLoader") continue;
                Destroy(go);
            }

            // Remove the brown floor thingies
            Destroy(GameObject.Find("Mantis Battle/mantis_lord_opening_floors"));
            Destroy(GameObject.Find("Mantis Battle/mantis_lord_opening_floors (1)"));

            // Remove any particles
            SceneParticlesController spc = FindObjectOfType<SceneParticlesController>();
            spc.DisableParticles();

            // Make the floor plane
            Plane = new GameObject("Plane")
            {
                // make it able to be walked on
                tag = "HeroWalkable",
                layer = 8
            };

            // Dimensions
            MeshFilter meshFilter = Plane.AddComponent<MeshFilter>();
            meshFilter.mesh = CreateMesh(200, 6.03f);
            MeshRenderer renderer = Plane.AddComponent<MeshRenderer>();
            renderer.material.shader = Shader.Find("Particles/Additive");

            // Color
            if (RainbowFloor)
            {
                Random rand = new Random();
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
            BoxCollider2D a = Plane.AddComponent<BoxCollider2D>();
            a.isTrigger = false;

            // Make it exist.
            Plane.SetActive(true);
        }

        private void SetBool(string originalSet, bool value)
        {
            // If you've already defeated mantis lords and you'r defeating them again
            // then you've just beat mantis gods
            if (originalSet == "defeatedMantisLords" && PlayerData.instance.defeatedMantisLords && value)
            {
                MantisGods.SettingsInstance.DefeatedGods = true;
                StartCoroutine(BattleBeat());
            }
            PlayerData.instance.SetBoolInternal(originalSet, value);
        }
    }
}