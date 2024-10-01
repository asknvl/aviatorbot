using botservice.Models.messages;
using botservice.Models.messages.raceup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;

namespace aviatorbot.Models.messages.mostbet
{
    public class MP_moderator_mostbet : MP_modertator_raceup
    {
        public MP_moderator_mostbet(string geotag, string token, ITelegramBotClient bot, Languages language) : base(geotag, token, bot, language)
        {
        }

        protected override string getAtributedLink(string link, string? uuid, string? src)
        {
            return base.getAtributedLink(link, uuid, src);
        }
    }
}
