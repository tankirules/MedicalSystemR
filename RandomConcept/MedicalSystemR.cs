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
        Dictionary<Player, Coroutine> pcDict = new Dictionary<Player, Coroutine>();
        protected override void Load()
        {
            Instance = this;
            Config = Instance.Configuration.Instance;
            var harmony = new Harmony("com.random.medicalsystemr");
            harmony.PatchAll();

            UseableConsumeable.onPerformedAid += onPerformedAid;
            PlayerLife.OnSelectingRespawnPoint += OnPlayerRespawn;
            DamageTool.damagePlayerRequested += DamagePlayerRequested;
            U.Events.OnPlayerDisconnected += OnPlayerDisconnection;
            U.Events.OnPlayerConnected += OnplayerConnection;

            Rocket.Core.Logging.Logger.Log("Random's Mod loaded");


        }



        protected override void Unload()
        {
            UseableConsumeable.onPerformedAid -= onPerformedAid;
            PlayerLife.OnSelectingRespawnPoint -= OnPlayerRespawn;
            DamageTool.damagePlayerRequested -= DamagePlayerRequested;
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
            //stop coroutine
            pcDict.TryGetValue(p, out var cor);
            if (!(cor == null))
            {
                StopCoroutine(cor);
            }
            pcDict.Remove(p);
        }

        private void OnPlayerRespawn(PlayerLife sender, bool wantsToSpawnAtHome, ref Vector3 position, ref float yaw)
        {
            var player = sender.player;
            UnturnedChat.Say("respawned aksed");
            downedplayers.Remove(player);
            tokill.Remove(player);
            explodekill.Remove(player);
            player.equipment.canEquip = true;
            //stop coroutine
            pcDict.TryGetValue(player, out var cor);
            if (!(cor == null))
            {
                StopCoroutine(cor);
            }
            pcDict.Remove(player);

            player.movement.sendPluginGravityMultiplier(1f);
            player.movement.sendPluginSpeedMultiplier(1f);
        }

        //check if reviving
        public void onPerformedAid(Player instigator, Player target)
        {
            //TODO: CHECK IF PLAYER IS ALLOWED EG TEAM AND ROLE?
            if (downedplayers.Contains(target))
            {
                upplayer(target);
            }
        }

        public void upplayer(Player player)
        {
            downedplayers.Remove(player);
            player.movement.sendPluginSpeedMultiplier(1f);
            //set players hp to reivve hp
            var dmg = (byte)(player.life.health - Config.revivehp);
            player.life.askDamage(dmg, Vector3.up, EDeathCause.SUICIDE, ELimb.SPINE, CSteamID.Nil, out EPlayerKill kill, false, ERagdollEffect.NONE, false);
            //stop coroutine
            pcDict.TryGetValue(player, out var cor);
            if (!(cor == null))
            {
                StopCoroutine(cor);
            }
            pcDict.Remove(player);
            player.equipment.canEquip = true;

        }

        //CODE FOR A DELAY KILL FROM EXPLOSION
        public void delaykill(Player player, byte amount, Vector3 newRagdoll, EDeathCause newCause, ELimb newLimb, CSteamID newKiller, out EPlayerKill kill, bool trackKill = false, ERagdollEffect newRagdollEffect = ERagdollEffect.NONE, bool canCauseBleeding = true)
        {
            kill = EPlayerKill.NONE;
            StartCoroutine(Delayedkill(player, amount, newRagdoll, newCause, newLimb, newKiller, false, ERagdollEffect.NONE, true));

        }
        public IEnumerator Delayedkill(Player player, byte amount, Vector3 newRagdoll, EDeathCause newCause, ELimb newLimb, CSteamID newKiller, bool trackKill = false, ERagdollEffect newRagdollEffect = ERagdollEffect.NONE, bool canCauseBleeding = true)
        {
            var kill = EPlayerKill.NONE;
            UnturnedChat.Say("bam!");
            yield return new WaitForSeconds(MedicalSystemR.Instance.Configuration.Instance.explosionairtime);
            MedicalSystemR.Instance.tokill.Add(player);

            player.life.askDamage(amount, newRagdoll, newCause, newLimb, newKiller, out kill, false, ERagdollEffect.NONE, true);
            player.movement.sendPluginSpeedMultiplier(1f);
            player.movement.sendPluginGravityMultiplier(1f);




        }
        //CODE FOR DOWNING PLAYERS AND HNALDING IT HERE
        public void downplayer(DamagePlayerParameters parameters)
        {
            

            var player = parameters.player;
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
            var co = StartCoroutine(Downtimer(parameters));
            pcDict[player] = co;


        }

        //THIS HANDLES A PLAYER BEING DOWNWED
        public IEnumerator Downtimer(DamagePlayerParameters parameters)
        {
            var player = parameters.player;
            //wait the downed amount of time
            UnturnedChat.Say("Downed for " + MedicalSystemR.Instance.Configuration.Instance.downtime + " seconds");
            //yield return new WaitForSeconds(MedicalSystemR.Instance.Configuration.Instance.downtime);
            yield return new WaitForSeconds(MedicalSystemR.Instance.Configuration.Instance.downtime);
            //if player still downed
            UnturnedChat.Say("doing final down check");

            if (downedplayers.Contains(player))
            {
                parameters.damage = 500;
                MedicalSystemR.Instance.downedplayers.Remove(player);
                MedicalSystemR.Instance.tokill.Add(player);
                DamageTool.damagePlayer(parameters, out EPlayerKill kill);
                parameters.player.movement.sendPluginSpeedMultiplier(1f);
            }




        }

        private void DamagePlayerRequested(ref DamagePlayerParameters parameters, ref bool shouldAllow)
        {
            var player = parameters.player;
            //instalkill headshot player
            if (parameters.limb == ELimb.SKULL)
            {
                shouldAllow = true;
                return;
            }
            //kill a downed player
            //THIS must be before explodekill
            //to kill exlpoded players already
            if ((Instance.tokill.Contains(player) == true))
            {
                UnturnedChat.Say("Executing player");
                //remove them from the list of doomed players
                MedicalSystemR.Instance.tokill.Remove(player);
                //skip prefix, and execute original method
                player.movement.sendPluginSpeedMultiplier(1f);
                shouldAllow = true;
                //remove corotuine
                pcDict.TryGetValue(player, out var cor);
                if (!(cor == null))
                {
                    StopCoroutine(cor);
                }
                
                pcDict.Remove(player);
                return;
            }
            //this downs a player
            var hp = player.life.health;
            if (hp < parameters.damage)
            {
                //downed p layer has been killed
                if (MedicalSystemR.Instance.downedplayers.Contains(player))
                {
                    MedicalSystemR.Instance.downedplayers.Remove(player);
                    player.movement.sendPluginSpeedMultiplier(1f);
                    shouldAllow = true;
                    return;
                }
                //kill exploded player
                var cause = parameters.cause;
                UnturnedChat.Say("cause is " + cause);
                if (cause == EDeathCause.CHARGE || cause == EDeathCause.GRENADE || cause == EDeathCause.LANDMINE || cause == EDeathCause.MISSILE || cause == EDeathCause.SPLASH)
                {
                    //HANDLE EXLOSIONS WITH DODAMAGE PREFIX AS IT IS MORE ACCURATE
                    shouldAllow = true;
                    return;
                }
                
                player.life.serverSetBleeding(false);
                //set players heaklth to downed hp
                if (player.life.health > MedicalSystemR.Instance.Configuration.Instance.downedhp)
                {
                    parameters.damage = MedicalSystemR.Instance.Configuration.Instance.downedhp - player.life.health;
                }
                else
                {
                    player.life.askHeal((byte)(MedicalSystemR.Instance.Configuration.Instance.downedhp - player.life.health), true, true);
                    parameters.damage = 0;
                }


                MedicalSystemR.Instance.downplayer(parameters);

                
                //prevent equippipng
                player.equipment.dequip();
                player.equipment.canEquip = false;

                UnturnedChat.Say("healing for " + (MedicalSystemR.Instance.Configuration.Instance.downedhp - 1) + " hp");
                UnturnedChat.Say("hp is " + hp + " and dmg is " + parameters.damage);
                shouldAllow = true;
                
                return;
            }
            return;
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

    [HarmonyPatch(typeof(PlayerLife), "doDamage")]
    class patch
    {
        static bool Prefix(PlayerLife __instance, byte amount, Vector3 newRagdoll, EDeathCause newCause, ELimb newLimb, CSteamID newKiller, out EPlayerKill kill, bool trackKill = false, ERagdollEffect newRagdollEffect = ERagdollEffect.NONE, bool canCauseBleeding = true)
        {
            kill = EPlayerKill.NONE;
            var cause = newCause;
            var player = __instance.player;
            //ONLY HANDLE EXPLOSIONS
            if (cause == EDeathCause.CHARGE || cause == EDeathCause.GRENADE || cause == EDeathCause.LANDMINE || cause == EDeathCause.MISSILE || cause == EDeathCause.SPLASH)
            {
                if (MedicalSystemR.Instance.tokill.Contains(player))
                {
                    MedicalSystemR.Instance.tokill.Remove(player);
                    return true;
                }
                if (player.life.health < amount)
                {
                    UnturnedChat.Say("hp is " + player.life.health + " and dmg is " + amount);
                    player.movement.sendPluginGravityMultiplier(MedicalSystemR.Instance.Configuration.Instance.explosiongrav);
                    MedicalSystemR.Instance.tokill.Add(player);
                    MedicalSystemR.Instance.delaykill(player,amount, newRagdoll, newCause, newLimb, newKiller, out kill, false,  ERagdollEffect.NONE,  true);
                    return false;
                }

            }

            
            UnturnedChat.Say("doing " + amount + " damage");
            return true;
        }
    }
    //harmony cant find the correct method
    //[HarmonyPatch(typeof(DamageTool), "explode", typeof(ExplosionParameters))]
    //class patch
    //{
    //    static void Prefix(ref ExplosionParameters parameters, out List<EPlayerKill> kills)
    //    {
    //        kills = new List<EPlayerKill>();
    //        UnturnedChat.Say("look at her go!");
    //        parameters.launchSpeed = parameters.launchSpeed * MedicalSystemR.Instance.Configuration.Instance.explforcemult;
    //    }
    //}





}





