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

        public void LoadDefaults()
        {
            downtime = 30;
            explosionairtime = 0.7f;
        }
    }
}
