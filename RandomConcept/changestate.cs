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
namespace Random
{
    public class down : MonoBehaviour
    {
        public void startcor(Player player, Vector3 newRagdoll, EDeathCause newCause, ELimb newLimb, CSteamID newKiller, bool trackKill = false, ERagdollEffect newRagdollEffect = ERagdollEffect.NONE, bool canCauseBleeding = true)
        {
            StartCoroutine(Downtimer(player, newRagdoll, newCause, newLimb, newKiller, false, ERagdollEffect.NONE, true));
        }

        public IEnumerator Downtimer(Player player, Vector3 newRagdoll, EDeathCause newCause, ELimb newLimb, CSteamID newKiller, bool trackKill = false, ERagdollEffect newRagdollEffect = ERagdollEffect.NONE, bool canCauseBleeding = true)
        {
            //wait the downed amount of time
            UnturnedChat.Say("Downed for " + MedicalSystemR.Instance.Configuration.Instance.downtime + " seconds");
            //yield return new WaitForSeconds(MedicalSystemR.Instance.Configuration.Instance.downtime);
            yield return new WaitForSeconds(30);
            //if player still downed
            UnturnedChat.Say("prepare to die");
            if (MedicalSystemR.Instance.downedplayers.Contains(player))
            {
                MedicalSystemR.Instance.downedplayers.Remove(player);
                MedicalSystemR.Instance.tokill.Add(player);
                DamagePlayerParameters para = new DamagePlayerParameters(player);
                para.cause = newCause;
                para.limb = newLimb;
                para.killer = newKiller;
                para.damage = 100f;
                DamageTool.damagePlayer(para, out EPlayerKill kill);
            }
            player.movement.sendPluginSpeedMultiplier(1f);
        }
    }
    public class changestate 
    {
        //public static changestate Instance;
        public void downplayer(Player player, Vector3 newRagdoll, EDeathCause newCause, ELimb newLimb, CSteamID newKiller, bool trackKill = false, ERagdollEffect newRagdollEffect = ERagdollEffect.NONE, bool canCauseBleeding = true)
        {
            //add to downed list
            Random.MedicalSystemR.Instance.downedplayers.Add(player);
            //make them unable to move
            player.movement.sendPluginSpeedMultiplier(0);
            //make them prone
            var pronestance = EPlayerStance.PRONE;
            player.stance.checkStance(pronestance, true);


            var tempgo = new GameObject();
            down d = tempgo.AddComponent<down>();
            d.startcor(player, newRagdoll, newCause, newLimb, newKiller, false, ERagdollEffect.NONE, true);
        }


    }


}

