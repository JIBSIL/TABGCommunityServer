﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TABG
{
    internal class Configuration
    {
        public string Version { get; set; }
        public bool? IsBeta { get; set; }
        public bool? IsAlpha { get; set; }

        public bool Initialized = false;
        public ServerConcurrencyHandler ServerConcurrencyHandler { get; set; }

        public Configuration() { }
    }
}
