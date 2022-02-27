using System.Collections.Generic;
using HutongGames.PlayMaker.Actions;
using Modding;
using UnityEngine;
using Vasi;

namespace Mantis_Gods
{
    public class Lord : MonoBehaviour
    {
        // Dictionary of attacks and the new FPS values they're set to
        // Higher is faster.
        private readonly Dictionary<string, float> AnimationFps = new Dictionary<string, float>
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

        private PlayMakerFSM _control;
        private HealthManager _hm;
        private tk2dSpriteAnimator _anim;

        private void Awake()
        {
            _control = gameObject.LocateMyFSM("Mantis Lord");
            _hm = gameObject.GetComponent<HealthManager>();
            _anim = gameObject.GetComponent<tk2dSpriteAnimator>();
        }

        private void Start()
        {
            // Double contact damage
            foreach (DamageHero x in gameObject.GetComponentsInChildren<DamageHero>(true))
                x.damageDealt = 2;

            // 3x HP
            _hm.hp = 210 * 3;

            // Remove some waits
            // A 0 on the Idle causes the Battle Sub FSM in the second phase
            // to not take control and allows desync of the mantises
            _control.GetAction<Wait>("Idle", 0).time.Value = 0f;
            _control.GetAction<Wait>("Start Pause", 0).time.Value = 0;
            _control.GetAction<Wait>("Throw CD", 0).time.Value = 0;
            _control.GetAction<Wait>("Arrive Pause", 0).time.Value /= 2;
            _control.GetAction<Wait>("Arrive", 4).time.Value /= 3;
            _control.GetAction<Wait>("Leave Pause", 0).time.Value /= 2;
            _control.GetAction<Wait>("After Throw Pause", 3).time.Value /= 4;
            
            _control.GetState("Start Pause").InsertMethod(0, () => HeroController.instance.SetHazardRespawn(new Vector3(31.9f, 7.4f), facingRight: false));
            
            // Set the fps values as indicated in the dictionary
            foreach ((string key, float value) in AnimationFps)
                _anim.GetClipByName(key).fps = value;

            if (gameObject.name.Contains("S"))
                UpdatePhase2(gameObject, _control);
            else
                _control.GetAction<Wait>("Leave Pause", 0).time.Value /= 3;
        }
        
        private static void UpdatePhase2(GameObject lord, PlayMakerFSM lordFsm)
        {
            lord.GetComponent<HealthManager>().hp = 480;
            
            // DASH, DSTAB, THROW
            // 1, 1, 1 => 1/6
            lordFsm.GetAction<SendRandomEventV3>("Attack Choice", 5).weights[2].Value /= 10f;
            PlayerData.instance.SetHazardRespawn(new Vector3(31.9f, 7.4f, 0), false);
        }
    }
}