using HarmonyLib;
using Rocket.Core.Plugins;
using Rocket.Unturned;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rocket.Core;
using Rocket.Unturned.Chat;
using Rocket.API;
using Steamworks;
using UnityEngine;
using System.Collections;
using Rocket.Unturned.Player;

namespace Random
{

    public class MedicalSystemR : RocketPlugin<MedicalSystemRConfiguration>
    {
        public static MedicalSystemR Instance;
        public static MedicalSystemRConfiguration Config;
        //this list contains all the players who are downed : ignore any damage dealt to them
        public List<Player> downedplayers = new List<Player>();
        //this list contains the players who should die
        public List<Player> tokill = new List<Player>();
        public List<Player> explodekill = new List<Player>();
        protected override void Load()
        {
            Instance = this;
            Config = Instance.Configuration.Instance;
            var harmony = new Harmony("com.random.medicalsystemr");
            harmony.PatchAll();

            U.Events.OnPlayerDisconnected += OnPlayerDisconnection;
            U.Events.OnPlayerConnected += OnplayerConnection;

            Rocket.Core.Logging.Logger.Log("Random's Mod loaded");


        }



        protected override void Unload()
        {
            U.Events.OnPlayerDisconnected -= OnPlayerDisconnection;
            U.Events.OnPlayerConnected -= OnplayerConnection;

            Rocket.Core.Logging.Logger.Log("Random's Med unloaded");
        }

        public void OnplayerConnection(UnturnedPlayer player)
        {
            var p = player.Player;
            stancewrapper sw = new stancewrapper(p);
        }



        private void OnPlayerDisconnection(Rocket.Unturned.Player.UnturnedPlayer player)
        {
            //remove player from any lists
            var p = player.Player;
            downedplayers.Remove(p);
            tokill.Remove(p);

        }
        //CODE FOR A DELAY KILL FROM EXPLOSION
        public void delaykill(Player player, Vector3 newRagdoll, EDeathCause newCause, ELimb newLimb, CSteamID newKiller, bool trackKill = false, ERagdollEffect newRagdollEffect = ERagdollEffect.NONE, bool canCauseBleeding = true)
        {
            StartCoroutine(Delayedkill(player, newRagdoll, newCause, newLimb, newKiller, false, ERagdollEffect.NONE, true));

        }
        public IEnumerator Delayedkill(Player player, Vector3 newRagdoll, EDeathCause newCause, ELimb newLimb, CSteamID newKiller, bool trackKill = false, ERagdollEffect newRagdollEffect = ERagdollEffect.NONE, bool canCauseBleeding = true)
        {
            UnturnedChat.Say("bam!");
            yield return new WaitForSeconds(MedicalSystemR.Instance.Configuration.Instance.explosionairtime);
            MedicalSystemR.Instance.tokill.Add(player);
            DamagePlayerParameters para = new DamagePlayerParameters(player);
            para.cause = newCause;
            para.limb = newLimb;
            para.killer = newKiller;
            para.damage = 100f;
            DamageTool.damagePlayer(para, out EPlayerKill kill);
            player.movement.sendPluginSpeedMultiplier(1f);




        }
        //CODE FOR DOWNING PLAYERS AND HNALDING IT HERE
        public void downplayer(Player player, Vector3 newRagdoll, EDeathCause newCause, ELimb newLimb, CSteamID newKiller, bool trackKill = false, ERagdollEffect newRagdollEffect = ERagdollEffect.NONE, bool canCauseBleeding = true)
            {
            //add to downed list
            Random.MedicalSystemR.Instance.downedplayers.Add(player);
            //make them unable to move
            player.movement.sendPluginSpeedMultiplier(0);
            //make them prone
            var pronestance = EPlayerStance.PRONE;
            player.stance.checkStance(pronestance, true);


            //var tempgo = new GameObject();
            //down d = tempgo.AddComponent<down>();
            //d.startcor(player, newRagdoll, newCause, newLimb, newKiller, false, ERagdollEffect.NONE, true);
            startcor(player, newRagdoll, newCause, newLimb, newKiller, false, ERagdollEffect.NONE, true);
        }
        public void startcor(Player player, Vector3 newRagdoll, EDeathCause newCause, ELimb newLimb, CSteamID newKiller, bool trackKill = false, ERagdollEffect newRagdollEffect = ERagdollEffect.NONE, bool canCauseBleeding = true)
        {
            StartCoroutine(Downtimer(player, newRagdoll, newCause, newLimb, newKiller, false, ERagdollEffect.NONE, true));
        }
        
        //THIS HANDLES A PLAYER BEING DOWNWED
        public IEnumerator Downtimer(Player player, Vector3 newRagdoll, EDeathCause newCause, ELimb newLimb, CSteamID newKiller, bool trackKill = false, ERagdollEffect newRagdollEffect = ERagdollEffect.NONE, bool canCauseBleeding = true)
        {
            //wait the downed amount of time
            UnturnedChat.Say("Downed for " + MedicalSystemR.Instance.Configuration.Instance.downtime + " seconds");
            //yield return new WaitForSeconds(MedicalSystemR.Instance.Configuration.Instance.downtime);
            yield return new WaitForSeconds(MedicalSystemR.Instance.Configuration.Instance.downtime);
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
                player.movement.sendPluginSpeedMultiplier(1f);
            }
            
        }


    }
    public class stancewrapper
    {
        public stancewrapper(Player player)
        {
            W_player = player;
            W_player.stance.onStanceUpdated += OnStanceUpdated;
        }
        ~stancewrapper()
        {
            W_player.stance.onStanceUpdated -= OnStanceUpdated;
        }
        public void OnStanceUpdated()
        {
            //UnturnedChat.Say("stance change detected");
            
            if (MedicalSystemR.Instance.downedplayers.Contains(W_player))
            {
                var pronestance = EPlayerStance.PRONE;
                W_player.stance.checkStance(pronestance, true);
            }
        }

        SDG.Unturned.Player W_player { get; }
        
        
    }


    /*[HarmonyPatch(typeof(DamageTool), "damagePlayer")]
    class patch3
    {
        static void Prefix(DamagePlayerParameters parameters, out EPlayerKill kill)
        {
            //kill = EPlayerKill.NONE;
            //var cause = parameters.cause;
            

            //if (cause == EDeathCause.CHARGE || cause == EDeathCause.GRENADE || cause == EDeathCause.LANDMINE || cause == EDeathCause.MISSILE || cause == EDeathCause.SPLASH)
            //{
            //    var hp = parameters.player.life.health;
            //    var dmg = parameters.damage;
            //    var hpf = Convert.ToSingle(hp);
            //    if (hpf < dmg)
            //    {
            //        MedicalSystemR.Instance.explodekill.Add(parameters.player);
            //    }
                
            //}

        }
    }*/

    //patch damage
    [HarmonyPatch(typeof(PlayerLife), "doDamage")]
    class patch2
    {
        static bool Prefix(PlayerLife __instance, ref byte amount, Vector3 newRagdoll, EDeathCause newCause, ELimb newLimb, CSteamID newKiller, out EPlayerKill kill, bool trackKill = false, ERagdollEffect newRagdollEffect = ERagdollEffect.NONE, bool canCauseBleeding = true)
        {
            kill = EPlayerKill.NONE;
            var player = __instance.player;

            //instalkill headshot player
            if (newLimb == ELimb.SKULL)
            {
                return true;
            }
            //ignore damage on downed player
            if (MedicalSystemR.Instance.downedplayers.Contains(player) == true)
            {
                //skip prefix and skip original method
                return false;
            }
            //kill a downed player
            //THIS must be before explodekill
            //to kill exlpoded players already
            if ((MedicalSystemR.Instance.tokill.Contains(player) == true))
            {
                UnturnedChat.Say("Executing player");
                //remove them from the list of doomed players
                MedicalSystemR.Instance.tokill.Remove(player);
                //skip prefix, and execute original method
                return true;
            }






            //this downs a player
            //TODO: ADD TIMER
            var hp = player.life.health;
            var hpint = Convert.ToSingle(hp);
            var amint = Convert.ToSingle(amount);
            if (amint > hpint)
            {
                //kill exploded player
                var cause = newCause;
                if (cause == EDeathCause.CHARGE || cause == EDeathCause.GRENADE || cause == EDeathCause.LANDMINE || cause == EDeathCause.MISSILE)
                {
                    UnturnedChat.Say("hp is " + hpint + " and dmg is " + amint);
                    MedicalSystemR.Instance.tokill.Add(player);
                    MedicalSystemR.Instance.delaykill(player, newRagdoll, newCause, newLimb, newKiller, false, ERagdollEffect.NONE, true);
                    return false;

                }
                amint = hpint - 1;
                amount = Convert.ToByte(amint);
                MedicalSystemR.Instance.downplayer(player, newRagdoll, newCause, newLimb, newKiller, false, ERagdollEffect.NONE, true);

            }
            return true;

        }
    }



    //[HarmonyPatch(typeof(PlayerAnimator), "sendGesture")]
    //class Patch1
    //{
    //    static void Prefix(EPlayerGesture gesture, ref bool all)
    //    {
    //        all = true;
    //        UnturnedChat.Say("Gesture Sent!");
    //    }
    //}
    //[HarmonyPatch(typeof(DamageTool), "damagePlayer")]
    //class Patch
    //{
    //    static bool Prefix(ref DamagePlayerParameters parameters, out EPlayerKill kill)
    //    {
    //        kill = EPlayerKill.NONE;
    //        var player = parameters.player;

    //        //ignore damage on downed player
    //        if (MedicalSystemR.Instance.downedplayers.Contains(player) == true)
    //        {
    //            //skip prefix and skip original method
    //            return false;
    //        }

    //        //kill a downed player
    //        if ((MedicalSystemR.Instance.tokill.Contains(player) == true))
    //        {
    //            //remove them from the list of doomed players
    //            MedicalSystemR.Instance.tokill.Remove(player);
    //            //skip prefix, and execute original method
    //            return true;
    //        }




    //        var hp = player.life.health;
    //        var hpint = Convert.ToSingle(hp);
    //        if (parameters.damage > hpint)
    //        {
    //            parameters.damage = hpint - 1;
    //            MedicalSystemR.Instance.downedplayers.Add(player);
    //            var pronestance = EPlayerStance.PRONE;
    //            player.stance.checkStance(pronestance, true);

    //        }
    //        parameters.bleedingModifier = DamagePlayerParameters.Bleeding.Never;
    //        return true;

    //    }
    //}



}





