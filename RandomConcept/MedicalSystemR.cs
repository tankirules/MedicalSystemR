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
            PlayerLife.OnPreDeath += OnPreDeath;
        }
        protected override void Unload()
        {
            Rocket.Core.Logging.Logger.Log("Random's Med unloaded");
            PlayerLife.OnPreDeath -= OnPreDeath;
        }
        public void OnPreDeath(PlayerLife vari)
        {
            UnturnedChat.Say("waiting 10 Seconds");
            wait inst = new wait();
            inst.Start();

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





