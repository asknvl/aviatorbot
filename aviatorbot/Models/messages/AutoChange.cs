﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace asknvl.messaging
{
    public class AutoChange
    {
        [JsonProperty]
        public string OldText { get; set; }
        [JsonProperty]
        public string NewText { get; set; }
    }
}
