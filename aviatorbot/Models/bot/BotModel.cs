using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace aviatorbot.Model.bot
{
    public class BotModel
    {
        public string service { get; set; } = "aviator_bot";
        public string geotag { get; set; }
        public string token { get; set; }
        public string link { get; set; }
        public string pm { get; set; }
        public List<long> operators_id { get; set; } = new() { 1481806946 };

    }
}
