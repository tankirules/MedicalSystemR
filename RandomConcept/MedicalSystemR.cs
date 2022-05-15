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

    [HarmonyPatch(typeof(PlayerLife), "doDamage")]
    class Patch
    {
        static void Prefix(PlayerLife __instance, ref byte amount, Vector3 newRagdoll, EDeathCause newCause, ELimb newLimb, CSteamID newKiller, out EPlayerKill kill, bool trackKill = false, ERagdollEffect newRagdollEffect = ERagdollEffect.NONE, bool canCauseBleeding = true)
        {
            UnturnedChat.Say("before prefix");
            kill = EPlayerKill.NONE;
            var hp = __instance.health;
            if (amount > hp)
            {
                var inthp = Convert.ToDouble(hp);
                var intamount = Convert.ToDouble(amount);
                intamount = inthp - 1;
                amount = Convert.ToByte(intamount);
            }
            UnturnedChat.Say("after prefix");

        }
    }
    [HarmonyPatch(typeof(PlayerLife), "askDamage")]
    class Patch1
    {
        static void Prefix(byte amount, Vector3 newRagdoll, EDeathCause newCause, ELimb newLimb, CSteamID newKiller, out EPlayerKill kill, bool trackKill = false, ERagdollEffect newRagdollEffect = ERagdollEffect.NONE, bool canCauseBleeding = true, bool bypassSafezone = false)
        {
            kill = EPlayerKill.NONE;
            UnturnedChat.Say("Damage Asked");
        }
    }

}





