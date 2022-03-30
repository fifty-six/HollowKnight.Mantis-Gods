using System.Collections;
using System.Linq;
using GlobalEnums;
using HutongGames.PlayMaker.Actions;
using Modding;
using UnityEngine;
using UnityEngine.SceneManagement;
using Vasi;
using Logger = Modding.Logger;
using USceneManager = UnityEngine.SceneManagement.SceneManager;

namespace Mantis_Gods
{
    internal class Mantis : MonoBehaviour
    {
        private static GlobalSettings Config => MantisGods.Instance.Settings;
        
        private GameObject[] _lords = new GameObject[3];
        private GameObject _mantisBattle;
        private GameObject _plane;
        
        private bool _inBattle;

        private IEnumerator BattleBeat()
        {
            LogDebug("Started Battle Beat Coroutine");

            yield return new WaitForSeconds(13);

            Destroy(_plane);

            yield return new WaitForSeconds(6);

            ReflectionHelper.SetField(GameManager.instance, "entryDelay", 1f);
            ReflectionHelper.SetField(GameManager.instance, "targetScene", "Fungus2_14");
            GameManager.instance.entryGateName = "bot3";

            // Go to Fungus2_15 from the bottom 3 entrance
            // This is where you would fall down from where claw is into the arena
            GameManager.instance.BeginSceneTransition
            (
                new GameManager.SceneLoadInfo
                {
                    AlwaysUnloadUnusedAssets = true,
                    EntryGateName = "bot3",
                    PreventCameraFadeOut = false,
                    SceneName = "Fungus2_14",
                    Visualization = GameManager.SceneLoadVisualizations.Dream
                }
            );

            LogDebug("Finished Coroutine");
        }

        private static void LogDebug(string str) => Logger.LogDebug("[Mantis Gods]: " + str);

        public void OnDestroy()
        {
            USceneManager.sceneLoaded -= ResetScene;
            ModHooks.LanguageGetHook -= LangGet;
            ModHooks.GetPlayerBoolHook -= GetBool;
            ModHooks.SetPlayerBoolHook -= SetBool;
        }

        public void Start()
        {
            USceneManager.sceneLoaded += ResetScene;
            ModHooks.LanguageGetHook += LangGet;
            ModHooks.GetPlayerBoolHook += GetBool;
            ModHooks.SetPlayerBoolHook += SetBool;
            ModHooks.ObjectPoolSpawnHook += ShotHandler;
        }

        private GameObject ShotHandler(GameObject go)
        {
            // If you haven't defeated the normal lords, do nothing
            if (!_inBattle)
                return go;

            if (!go.name.Contains("Shot Mantis Lord"))
                return go;

            PlayMakerFSM shot = go.LocateMyFSM("Control");

            if (shot == null)
                return go;

            // 40, 40, 20, 20
            shot.GetAction<SetFloatValue>("Set L", 0).floatValue = -48f;
            shot.GetAction<SetFloatValue>("Set R", 0).floatValue = 48f;
            shot.GetAction<SetFloatValue>("Set L", 1).floatValue = -20f;
            shot.GetAction<SetFloatValue>("Set R", 1).floatValue = 20f;

            return go;
        }

        private static Mesh CreateMesh(float width, float height)
        {
            var m = new Mesh
            {
                name = "ScriptedMesh",
                vertices = new[]
                {
                    new Vector3(-width, -height, 0.01f),
                    new Vector3(width, -height, 0.01f),
                    new Vector3(width, height, 0.01f),
                    new Vector3(-width, height, 0.01f)
                },
                uv = new[]
                {
                    new Vector2(0, 0),
                    new Vector2(0, 1),
                    new Vector2(1, 1),
                    new Vector2(1, 0)
                },
                triangles = new[] { 0, 1, 2, 0, 2, 3 }
            };

            m.RecalculateNormals();

            return m;
        }

        private static bool GetBool(string originalSet, bool orig)
        {
            return (
                    originalSet != "defeatedMantisLords"
                    || !PlayerData.instance.defeatedMantisLords
                    || MantisGods.Instance.LocalData.DefeatedGods
                    || HeroController.instance.hero_state != ActorStates.no_input
                )
                && orig;
        }


        // Used to override the text for Mantis Lords
        // Mantis Lords => Mantis Gods
        private string LangGet(string key, string sheetTitle, string orig)
        {
            if (key == "MANTIS_LORDS_MAIN" && _inBattle)
                return "Gods";

            return orig;
        }

        private void ResetScene(Scene arg0, LoadSceneMode arg1)
        {
            _inBattle = false;
            
            for (int i = 0; i < _lords.Length; i++)
                _lords[i] = null;


            if (arg0.name is "GG_Mantis_Lords" or "GG_Mantis_Lords_V")
            {
                if (BossSequenceController.IsInSequence && !Config.AllowInPantheons)
                    return;
                
                _inBattle = true;
                StartCoroutine(AddComponents());
            }

            if (arg0.name != "Fungus2_15_boss" || !PlayerData.instance.defeatedMantisLords)
                return;

            _inBattle = true;
            StartCoroutine(AddComponents());

            // Set the mapZone to White Palace so when you die
            // you don't spawn a shade and don't lose geo
            GameManager.instance.sm.mapZone = MapZone.WHITE_PALACE;

            GameObject[] floors =
            {
                GameObject.Find("Mantis Battle/mantis_lord_opening_floors"),
                GameObject.Find("Mantis Battle/mantis_lord_opening_floors (1)")
            };
            
            GameObject[] roots = USceneManager.GetSceneByName("Fungus2_15").GetRootGameObjects();

            if (MantisGods.Instance.Settings.NormalArena)
            {
                TransformArena(roots, floors);
            }
            else
            {
                CreateArena(roots, floors);
            }
        }

        private IEnumerator AddComponents()
        {
            yield return null;

            if (_lords.Any(x => x != null))
                yield break;

            if (_mantisBattle == null)
                _mantisBattle = GameObject.Find("Mantis Battle");

            if (_mantisBattle == null)
                yield break;

            _lords = new[]
            {
                _mantisBattle.FindGameObjectInChildren("Mantis Lord"),
                _mantisBattle.FindGameObjectInChildren("Mantis Lord S1"),
                _mantisBattle.FindGameObjectInChildren("Mantis Lord S2"),
            };

            if (_lords.Any(x => x == null))
                yield break;

            foreach (GameObject lord in _lords)
                lord.AddComponent<Lord>();
        }

        private void TransformArena(GameObject[] roots, GameObject[] floors)
        {
            foreach (GameObject go in roots)
            {
                if (!Config.KeepSpikes && go.name.Contains("Deep Spikes") || go.GetComponent<HealthManager>() != null)
                {
                    Destroy(go);
                }
            }
            
            if (Config.KeepSpikes)
                return;

            foreach (GameObject go in floors)
            {
                go.LocateMyFSM("Floor Control").ChangeTransition("Extended", "MLORD FLOOR RETRACT", "Extended");
            }
        }

        private void CreateArena(GameObject[] roots, GameObject[] floors)
        {
            // Destroy all game objects that aren't the BossLoader
            // BossLoader unloads the mantis lord stuff after you die
            foreach (GameObject go in roots)
            {
                if (go.name == "BossLoader")
                    continue;

                Destroy(go);
            }

            // Remove the brown floor thingies
            foreach (GameObject go in floors)
            {
                Destroy(go);
            }

            // Remove any particles
            var spc = FindObjectOfType<SceneParticlesController>();
            spc.DisableParticles();

            // Make the floor plane
            _plane = new GameObject("Plane")
            {
                // Make it able to be walked on
                tag = "HeroWalkable",
                layer = 8
            };

            // Dimensions
            var meshFilter = _plane.AddComponent<MeshFilter>();
            meshFilter.mesh = CreateMesh(200, 6.03f);
            var renderer = _plane.AddComponent<MeshRenderer>();
            renderer.material.shader = Shader.Find("Particles/Additive");

            // Texture
            var tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, Color.black);
            tex.Apply();

            // Renderer
            renderer.material.mainTexture = tex;
            renderer.material.color = Color.white;

            // Collider
            var col = _plane.AddComponent<BoxCollider2D>();
            col.isTrigger = false;

            // Make it exist.
            _plane.SetActive(true);
        }

        private bool SetBool(string originalSet, bool value)
        {
            // If you've already defeated mantis lords and you're defeating them again
            // then you've just beat mantis gods
            if (originalSet != "defeatedMantisLords" || !PlayerData.instance.defeatedMantisLords || !value)
                return value;

            MantisGods.Instance.LocalData.DefeatedGods = true;
            StartCoroutine(BattleBeat());

            return true;
        }
    }
}