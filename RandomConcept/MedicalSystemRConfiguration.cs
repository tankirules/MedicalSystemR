using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rocket.API;

namespace Random
{
    public class MedicalSystemRConfiguration : IRocketPluginConfiguration
    {
        public float downtime;
        public float explosionairtime;
        public byte downedhp;
        public float explosiongrav;
        public float explforcemult;
        public byte revivehp;
        public int offsety;
        public void LoadDefaults()
        {
            downtime = 30.0f;
            explosionairtime = 0.35f;
            downedhp = 40;
            explosiongrav = 0f;
            explforcemult = 2f;
            revivehp = 40;
            offsety = 2;
        }
    }
}
