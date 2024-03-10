using asknvl.logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;

namespace aviatorbot.Models.messages.rcp_canada
{
    public class MP_cana34 : MP_Landing_Raceup_cana
    {
        public MP_cana34(string geotag, string token, ITelegramBotClient bot, ILogger logger, string reg_link_part, string fd_link_part, string play_link_part) : base(geotag, token, bot, logger, reg_link_part, fd_link_part, play_link_part)
        {
        }
    }
}
