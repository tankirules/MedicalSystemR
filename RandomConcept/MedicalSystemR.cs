using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SDG.Unturned;
using HarmonyLib;
using Rocket.Core;
using Rocket.Unturned;
using Rocket.Core.Plugins;
using Rocket.Unturned.Chat;
using Rocket.API;
using Steamworks;
using UnityEngine;
using System.Collections;
namespace RandomConcept
{
    public class RandomConcept : RocketPlugin
    {
        public static RandomConcept Instance;
        protected override void Load()
        {
            Instance = this;
            Rocket.Core.Logging.Logger.Log("Random's Mod loaded");
            //PlayerLife.OnPreDeath += OnPreDeath;
        }
        protected override void Unload()
        {
            Rocket.Core.Logging.Logger.Log("Random's Med unloaded");
            //PlayerLife.OnPreDeath -= OnPreDeath;
        }
        //public void OnPreDeath(PlayerLife vari)
        //{
        //    UnturnedChat.Say("waiting 10 Seconds");
        //    wait inst = new wait();
        //    inst.Start();

        //}
    }

    [HarmonyPatch(typeof(PlayerLife), nameof(PlayerLife.askDamage))]
    class Patch
    {
        static void Prefix(byte amount, Vector3 newRagdoll, EDeathCause newCause, ELimb newLimb, CSteamID newKiller, out EPlayerKill kill, bool trackKill = false, ERagdollEffect newRagdollEffect = ERagdollEffect.NONE, bool canCauseBleeding = true, bool bypassSafezone = false)
        {
            kill = EPlayerKill.NONE;
            if (base.player.movement.isSafe && base.player.movement.isSafeInfo.noWeapons && !bypassSafezone)
            {
                return;
            }
            if (this.lastRespawn > 0f && Time.realtimeSinceStartup - this.lastRespawn < 0.5f && !bypassSafezone)
            {
                return;
            }
            this.doDamage(amount, newRagdoll, newCause, newLimb, newKiller, out kill, trackKill, newRagdollEffect, canCauseBleeding);
        }
    }
    public class wait : MonoBehaviour
    {
        public void Start()
        {
            StartCoroutine(Wait10sec());
        }

        IEnumerator Wait10sec()
        {
            //yield on a new YieldInstruction that waits for 5 seconds.
            yield return new WaitForSeconds(10);

        }
    }
}





