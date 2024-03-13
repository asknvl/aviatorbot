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
        public string support_pm { get; set; }
        public string pm { get; set; }
        public string channel_tag { get; set; }
        public string channel { get; set; }

        public string help { get; set; }
        public string training { get; set; }
        public string reveiews { get; set; }
        public string strategy { get; set; }
        public string vip { get; set; }        

        public bool? postbacks { get; set; }
        //public List<long> operators_id { get; set; } = new();
        public List<Operators.Operator> operators { get; set; } = new();    
    }

    public enum BotType
    {
        /// <summary>
        /// Аналог RAUJET
        /// </summary>
        //aviator_v0,        
        //aviator_v1,
        //aviator_v2_1w_br_eng,// 1fd pass release version        
        getinfo_v0 = 3,
        //aviator_v3_1win_wv_eng,// test webapp
        //aviator_v2_1win_br_esp,// 1fd pass release version        
        //aviator_v4_cana34,
        //aviator_v4_cana35,
        //aviator_v3_1win_wv_esp,
        landing_v0_1win_wv_eng = 9,
        landing_v0_cut_cana34,
        landing_v0_cut_cana37,
        landing_v0_strategies,

    }
}
