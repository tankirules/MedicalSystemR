﻿using System;
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

        public void LoadDefaults()
        {
            downtime = 30.0f;
            explosionairtime = 0.7f;
            downedhp = 40;
        }
    }
}
