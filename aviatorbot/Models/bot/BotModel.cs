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
        public BotType type { get; set; }
        public string service { get; set; } = "aviator_bot";
        public string geotag { get; set; }
        public string token { get; set; }
        public string link { get; set; }
        public string pm { get; set; }
        public string channel { get; set; }
        //public List<long> operators_id { get; set; } = new();
        public List<Operators.Operator> operators { get; set; } = new();    
    }

    public enum BotType
    {
        /// <summary>
        /// Аналог RAUJET
        /// </summary>
        aviator_v0,        
        aviator_v1,
        aviator_v2
    }
}
