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
using SDG.NetTransport;



namespace Random
{

    public class MedicalSystemR : RocketPlugin<MedicalSystemRConfiguration>
    {
        public int keyc;
        public ushort downUIid;
        public ushort suicidebutid;
        public static MedicalSystemR Instance;
        public static MedicalSystemRConfiguration Config;
        //pretty sure rocket has this or something but I cant find it
        //its literally just a list of online players
        public List<UnturnedPlayer> ListPlayers;

        //this list contains all the players who are downed : ignore any damage dealt to them
        public List<Player> downedplayers;
        //this list contains the players who should die
        public List<Player> tokill;
        public List<Player> explodekill;
        public Dictionary<Player, Coroutine> pcDict;
        public Dictionary<Player, DamagePlayerParameters> pdDict;
        protected override void Load()
        {

            suicidebutid = 19911;
            downUIid = 19912;
            keyc = 0;
            Instance = this;

            pdDict = new Dictionary<Player, DamagePlayerParameters>();
            pcDict = new Dictionary<Player, Coroutine>();
            explodekill = new List<Player>();
            tokill = new List<Player>();
            downedplayers = new List<Player>();
            ListPlayers = new List<UnturnedPlayer>();

            Config = Instance.Configuration.Instance;
            var harmony = new Harmony("com.random.medicalsystemr");
            harmony.PatchAll();

            UseableConsumeable.onPerformedAid += onPerformedAid;
            PlayerLife.OnSelectingRespawnPoint += OnPlayerRespawn;
            U.Events.OnPlayerDisconnected += OnPlayerDisconnection;
            U.Events.OnPlayerConnected += OnplayerConnection;
            EffectManager.onEffectButtonClicked += onEffectButtonClicked;
            PlayerLife.OnPreDeath += OnPreDeath;

            InvokeRepeating("senddowneffect", 0f, 0.25f);

            Rocket.Core.Logging.Logger.Log("Random's Mod loaded");


        }        

        protected override void Unload()
        {
            UseableConsumeable.onPerformedAid -= onPerformedAid;
            PlayerLife.OnSelectingRespawnPoint -= OnPlayerRespawn;
            U.Events.OnPlayerDisconnected -= OnPlayerDisconnection;
            U.Events.OnPlayerConnected -= OnplayerConnection;
            EffectManager.onEffectButtonClicked -= onEffectButtonClicked;
            PlayerLife.OnPreDeath -= OnPreDeath;

            //kill all players downed or about to be
            foreach (Player player in explodekill)
            {
                player.life.askDamage(200, Vector3.up, EDeathCause.SUICIDE, ELimb.SPINE, CSteamID.Nil, out EPlayerKill kill, false, ERagdollEffect.NONE, false);
            }

            foreach (Player player in downedplayers)
            {
                player.life.askDamage(200, Vector3.up, EDeathCause.SUICIDE, ELimb.SPINE, CSteamID.Nil, out EPlayerKill kill, false, ERagdollEffect.NONE, false);
            }

            foreach (Player player in tokill)
            {
                player.life.askDamage(200, Vector3.up, EDeathCause.SUICIDE, ELimb.SPINE, CSteamID.Nil, out EPlayerKill kill, false, ERagdollEffect.NONE, false);
            }

            //clear lists
            explodekill.Clear();
            tokill.Clear();
            downedplayers.Clear();
            //stop coroutines
            foreach(KeyValuePair<Player, Coroutine> entry in pcDict)
            {
                StopCoroutine(entry.Value);
            }


            Rocket.Core.Logging.Logger.Log("Random's Med unloaded");
        }

        private void OnPreDeath(PlayerLife obj)
        {
            Player player = obj.player;
            pcDict.TryGetValue(player, out var cor);
            if (!(cor == null))
            {
                StopCoroutine(cor);
            }
            pcDict.Remove(player);
            //JUST CLEAR UI PLS
            
            EffectManager.askEffectClearByID(downUIid, player.channel.owner.transportConnection);
            EffectManager.askEffectClearByID(suicidebutid, player.channel.owner.transportConnection);

        }

        private void onEffectButtonClicked(Player player, string buttonName)
        {
            if (buttonName != "Suicide")
            {
                return;
            }

            //claer UII effects
            EffectManager.askEffectClearByID(downUIid, player.channel.owner.transportConnection);
            EffectManager.askEffectClearByID(suicidebutid, player.channel.owner.transportConnection);
            //stop coroutine

            //kill player
            var DPP = pdDict[player];
            player.life.askDamage(200, DPP.direction, DPP.cause, DPP.limb, DPP.killer, out EPlayerKill kill);
            pcDict.TryGetValue(player, out var cor);
            if (!(cor == null))
            {
                StopCoroutine(cor);
            }
            pcDict.Remove(player);


        }

        public void senddowneffect()
        {
            foreach (UnturnedPlayer player in Instance.ListPlayers)
            {
                //TODO: only show downed symbol for medics
                foreach (Player p in Instance.downedplayers)
                {
                    //TODO: changable effect id
                    UnturnedPlayer up = UnturnedPlayer.FromPlayer(p);
                    SteamPlayer sp = player.Player.channel.owner;
                    EffectManager.sendEffect(61, sp.transportConnection, up.Position);
                }
            }

        }

        public void OnplayerConnection(UnturnedPlayer player)
        {
            var p = player.Player;
            playerwrapper pw = new playerwrapper(p);
            ListPlayers.Add(player);
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

            EffectManager.askEffectClearByID(downUIid, player.channel.owner.transportConnection);
            EffectManager.askEffectClearByID(suicidebutid, player.channel.owner.transportConnection);
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
            //clear UI effect
            EffectManager.askEffectClearByID(19912, player.channel.owner.transportConnection);
            EffectManager.askEffectClearByID(suicidebutid, player.channel.owner.transportConnection);

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
            UnturnedChat.Say("doing delayed kill");
            yield return new WaitForSeconds(MedicalSystemR.Instance.Configuration.Instance.explosionairtime);
            MedicalSystemR.Instance.tokill.Add(player);

            player.life.askDamage(amount, newRagdoll, newCause, newLimb, newKiller, out kill, false, ERagdollEffect.NONE, true);
            player.movement.sendPluginSpeedMultiplier(1f);
            player.movement.sendPluginGravityMultiplier(1f);




        }
        //CODE FOR DOWNING PLAYERS AND HNALDING IT HERE
        public void downplayer(Player player, byte amount, Vector3 newRagdoll, EDeathCause newCause, ELimb newLimb, CSteamID newKiller, bool trackKill = false, ERagdollEffect newRagdollEffect = ERagdollEffect.NONE, bool canCauseBleeding = true)
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
            var kill = EPlayerKill.PLAYER;
            var co = StartCoroutine(Downtimer(player, amount, newRagdoll, newCause, newLimb, newKiller, false, ERagdollEffect.NONE, true));
            pcDict[player] = co;

            DamagePlayerParameters DPP = new DamagePlayerParameters();
            DPP.player = player;
            DPP.damage = amount;
            DPP.direction = newRagdoll;
            DPP.cause = newCause;
            DPP.limb = newLimb;
            DPP.killer = newKiller;
            DPP.trackKill = false;
            DPP.ragdollEffect = ERagdollEffect.NONE;
            pdDict[player] = DPP;

        }

        //THIS HANDLES A PLAYER BEING DOWNWED
        public IEnumerator Downtimer(Player player, byte amount, Vector3 newRagdoll, EDeathCause newCause, ELimb newLimb, CSteamID newKiller, bool trackKill = false, ERagdollEffect newRagdollEffect = ERagdollEffect.NONE, bool canCauseBleeding = true)
        {
            float seconds = MedicalSystemR.Instance.Configuration.Instance.downtime;
            //wait the downed amount of time
            UnturnedChat.Say("Downed for " + MedicalSystemR.Instance.Configuration.Instance.downtime + " seconds");
            var key = keyc;
            keyc = key + 2;
            EffectManager.sendUIEffect(suicidebutid, (short)(key + 1), player.channel.owner.transportConnection, true);
            while (seconds >= 0)
            {
                EffectManager.sendUIEffect(downUIid, (short)key, player.channel.owner.transportConnection, true, seconds.ToString());
                yield return new WaitForSeconds(1);                
                
                
                seconds = seconds - 1;
            }            
            //if player still downed
            UnturnedChat.Say("doing final down check");

            if (downedplayers.Contains(player))
            {
                var kill = EPlayerKill.PLAYER;
                amount = 200;
                player.life.askDamage(amount, newRagdoll, newCause, newLimb, newKiller, out kill, trackKill, newRagdollEffect, canCauseBleeding, false);
            }
            EffectManager.askEffectClearByID(downUIid, player.channel.owner.transportConnection);
            EffectManager.askEffectClearByID(suicidebutid, player.channel.owner.transportConnection);

        }
               
    }


    public class playerwrapper
    {
        public playerwrapper(Player player)
        {
            W_player = player;
            W_player.stance.onStanceUpdated += OnStanceUpdated;
        }
        ~playerwrapper()
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
        static bool Prefix(PlayerLife __instance, ref byte amount, Vector3 newRagdoll, EDeathCause newCause, ELimb newLimb, CSteamID newKiller, out EPlayerKill kill, bool trackKill = false, ERagdollEffect newRagdollEffect = ERagdollEffect.NONE, bool canCauseBleeding = true)
        {
            UnturnedChat.Say("doing " + amount + " damage");
            kill = EPlayerKill.NONE;
            var cause = newCause;
            var player = __instance.player;
            //instakill headshot player
            if (newLimb == ELimb.SKULL)
            {
                return true;
            }
            //kill a downed player
            //THIS must be before explodekill so exploded players in the air will die
            if ((MedicalSystemR.Instance.tokill.Contains(player) == true))
            {
                
                //remove them from the list of doomed players
                MedicalSystemR.Instance.tokill.Remove(player);
                //skip prefix, and execute original method
                player.movement.sendPluginSpeedMultiplier(1f);
                //remove corotuine
                MedicalSystemR.Instance.pcDict.TryGetValue(player, out var cor);
                if (!(cor == null))
                {
                    __instance.StopCoroutine(cor);
                }

                MedicalSystemR.Instance.pcDict.Remove(player);

                EffectManager.askEffectClearByID(MedicalSystemR.Instance.downUIid, player.channel.owner.transportConnection);
                EffectManager.askEffectClearByID(MedicalSystemR.Instance.suicidebutid, player.channel.owner.transportConnection);
                UnturnedChat.Say("clearing effects");
                return true;
            }
            //this downs a player
            var hp = player.life.health;
            if (hp <= amount)
            {
                //downed p layer has been killed
                if (MedicalSystemR.Instance.downedplayers.Contains(player))
                {
                    MedicalSystemR.Instance.downedplayers.Remove(player);
                    player.movement.sendPluginSpeedMultiplier(1f);
                    return true;
                }
                //kill exploded player
                if (cause == EDeathCause.CHARGE || cause == EDeathCause.GRENADE || cause == EDeathCause.LANDMINE || cause == EDeathCause.MISSILE || cause == EDeathCause.SPLASH)
                {
                    if (MedicalSystemR.Instance.tokill.Contains(player))
                    {
                        MedicalSystemR.Instance.tokill.Remove(player);
                        return true;
                    }
                    if (hp <= amount)
                    {
                        UnturnedChat.Say("hp is " + player.life.health + " and dmg is " + amount);
                        player.movement.sendPluginGravityMultiplier(MedicalSystemR.Instance.Configuration.Instance.explosiongrav);
                        MedicalSystemR.Instance.tokill.Add(player);
                        MedicalSystemR.Instance.delaykill(player, amount, newRagdoll, newCause, newLimb, newKiller, out kill, false, ERagdollEffect.NONE, true);
                        return false;
                    }

                }

                player.life.serverSetBleeding(false);
                //set players heaklth to downed hp
                if (player.life.health > MedicalSystemR.Instance.Configuration.Instance.downedhp)
                {
                    amount = (byte)(MedicalSystemR.Instance.Configuration.Instance.downedhp - player.life.health);
                }
                else
                {
                    player.life.askHeal((byte)(MedicalSystemR.Instance.Configuration.Instance.downedhp - player.life.health), true, true);
                    amount = 0;
                }


                MedicalSystemR.Instance.downplayer(player, amount, newRagdoll, newCause, newLimb, newKiller, false, ERagdollEffect.NONE, true);
                

                //prevent equippipng
                player.equipment.dequip();
                player.equipment.canEquip = false;

                UnturnedChat.Say("healing for " + (MedicalSystemR.Instance.Configuration.Instance.downedhp - 1) + " hp");
                UnturnedChat.Say("hp is " + hp + " and dmg is " + amount);
                return true;


            }
            return true;

        }
    }
    [HarmonyPatch(typeof(DamageTool), nameof(DamageTool.explode), new Type[] { typeof(ExplosionParameters), typeof(List<EPlayerKill>) }, new[] { ArgumentType.Normal, ArgumentType.Out })]
    class patch1
    {
        static void Prefix(ref ExplosionParameters parameters, out List<EPlayerKill> kills)
        {
            UnturnedChat.Say("explode triggered");
            kills = new List<EPlayerKill>();
            parameters.launchSpeed = parameters.launchSpeed * MedicalSystemR.Instance.Configuration.Instance.explforcemult;
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





