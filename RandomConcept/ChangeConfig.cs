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
    class ChangeConfig : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Both;
        public string Name => "ChangeConfig";
        public string Help => "Change config numbers for Random's Medical Plugin";
        public string Syntax => "/ChangeConfig configfield configvalue";
        public List<string> Aliases => new List<string> { "cc" };
        public List<string> Permissions => new List<string> { "changeconfig" };

        public void Execute(IRocketPlayer caller, string[] args)
        {
            if (args.Length != 2)
            {
                UnturnedChat.Say(caller, "Invalid arguments!", Color.red);
                return;
            }
            string field = args[0];
            string value = args[1];


            bool found = false;
            switch(field)
            {
                case "downtime":
                    MedicalSystemR.Instance.Configuration.Instance.downtime = Convert.ToSingle(value);
                    break;
                case "explosionairtime":
                    MedicalSystemR.Instance.Configuration.Instance.explosionairtime = Convert.ToSingle(value);
                    break;
                case "downedhp":
                    MedicalSystemR.Instance.Configuration.Instance.downedhp = Convert.ToByte(value);
                    break;
                case "explosiongrav":
                    MedicalSystemR.Instance.Configuration.Instance.explosiongrav = Convert.ToSingle(value);
                    break;
                case "explforcemult":
                    MedicalSystemR.Instance.Configuration.Instance.explforcemult = Convert.ToSingle(value);
                    break;
                case "revivehp":
                    MedicalSystemR.Instance.Configuration.Instance.revivehp = Convert.ToByte(value);
                    break;
                case "offsety":
                    MedicalSystemR.Instance.Configuration.Instance.offsety = Convert.ToInt32(value);
                    break;
                default:
                    UnturnedChat.Say(caller, "No config field found!", Color.red);
                    break;
                    
            }
            MedicalSystemR.Instance.Configuration.Save();
            if (found == false)
            {                
                return;
            }
            UnturnedChat.Say(caller, "Success", Color.yellow);


        }




    }
}
