using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace botservice.Model.bot
{
    public class BotModel
    {
        public BotType type { get; set; }
        public string service { get; set; } = "aviator_bot";
        public string geotag { get; set; }
        public string token { get; set; }
        public string link { get; set; }

        public string? link_reg { get; set; }
        public string? link_dep { get; set; }   
        public string? link_gam { get; set; }

        public string support_pm { get; set; }
        public string pm { get; set; }
        public string channel_tag { get; set; }
        public string channel { get; set; }
        public string help { get; set; }
        public string training { get; set; }
        public string reveiews { get; set; }
        public string strategy { get; set; }
        public string vip { get; set; }

        public string? register_source { get; set; }
        public string? register_source_link { get; set; }

        public bool? channel_approve { get; set; } = true;
        public bool? postbacks { get; set; }
        //public List<long> operators_id { get; set; } = new();
        public List<Operators.Operator> operators { get; set; } = new();
        public group_manager_settings? group_manager_settings { get; set; } = null;
    }

    public class group_manager_settings
    {
        public long? group_tg_id { get; set; }
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
        latam_basic_esp,//13
        latam_jet_esp,
        landing_vishal,
        latam_smrnv, //16
        latam_basic_v2,//17
        landing_hack_v3_basic, //18
        moderator_v2_strategies, //19
        moderator_cana34_raceup, //20
        trading_basic, //21
        landing_hack_v2_basic, //22
        moderator_cana35_raceup, //23
        moderator_inda120_raceup,//24
        moderator_deua01_raceup,//25
        moderator_cana53_raceup,//26

        moderator_inda_push_only,//27
        lottery_basic_v2,//28

        moderator_itaa07_raceup,//29

        landing_tier1_cana, //30
        landing_tier1_deua, //31
        landing_tier1_itaa, //32       

        landing_tier1_deua_postback, //33

        moderator_tier1_cana = 101,

        pusher = 200,

        group_manager_inda = 300,
        group_manager_raceup = 301,
    }
}
