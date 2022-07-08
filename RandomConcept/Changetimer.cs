using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rocket.API;
using Rocket.Core;
using Rocket.Unturned.Chat;
using UnityEngine;
using System.Reflection;

namespace Random
{
    public class Changetimer : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Both;
        public string Name => "Changetimer";
        public string Help => "Change timer for Random's Medical Plugin's down icon effect loop sending";
        public string Syntax => "/Changetimer time";
        public List<string> Aliases => new List<string> { "ct" };
        public List<string> Permissions => new List<string> { "changetimer" };

        public void Execute(IRocketPlayer caller, string[] args)
        {
            if (args.Length != 1)
            {
                UnturnedChat.Say(caller, "Invalid arguments!", Color.red);
                return;
            }
            var arg = args[0];
            bool valid = float.TryParse(arg, out float time);
            if (!valid)
            {
                UnturnedChat.Say(caller, "not a number", Color.red);
                return;
            }
            MedicalSystemR.Instance.Changetimer(time);
            UnturnedChat.Say("Changing down timer to " + time, Color.yellow);

        }
    }
}
