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
            var harmony = new Harmony("com.random.medicalsystemr");
            harmony.PatchAll();
            Rocket.Core.Logging.Logger.Log("Random's Mod loaded");
        }
        protected override void Unload()
        {
            Rocket.Core.Logging.Logger.Log("Random's Med unloaded");
        }

    }
    [HarmonyPatch(typeof(DamageTool), "damagePlayer")]
    class Patch
    {
        static void Prefix(ref DamagePlayerParameters parameters, out EPlayerKill kill)
        {
            UnturnedChat.Say("before prefix");
            kill = EPlayerKill.NONE;
            var player = parameters.player;
            var hp = player.life.health;
            var hpint = Convert.ToSingle(hp);
            UnturnedChat.Say("hp is: " + hpint);
            UnturnedChat.Say("damage is: " + parameters.damage);
            if (parameters.damage > hpint)
            {
                parameters.damage = hpint - 1;
                UnturnedChat.Say("reduced damage is: " + parameters.damage);
            }
            UnturnedChat.Say("after prefix");

        }
    }

    //[HarmonyPatch(typeof(DamageTool), "damage")]
    //class Patch
    //{
    //    static void Prefix(Player player, EDeathCause cause, ELimb limb, CSteamID killer, Vector3 direction, ref float damage, float times, out EPlayerKill kill, bool applyGlobalArmorMultiplier = true, bool trackKill = false, ERagdollEffect ragdollEffect = ERagdollEffect.NONE)
    //    {                 
    //        UnturnedChat.Say("before prefix");
    //        kill = EPlayerKill.NONE;
    //        var hp = player.life.health;
    //        var hpint = Convert.ToSingle(hp);
    //        UnturnedChat.Say("hp is: " + hpint);
    //        UnturnedChat.Say("damage is: " + damage);
    //        if (damage > hpint)
    //        {
    //            damage = hpint - 1;
    //        }
    //        UnturnedChat.Say("after prefix");

    //    }
    //}

}





