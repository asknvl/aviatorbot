using aviatorbot.Model.bot;
using aviatorbot.Models.messages.rcp_canada;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;

namespace aviatorbot.Models.messages
{
    public class MessageProcessorFactory : IMessageProcessorFactory
    {
        public MessageProcessorBase Get(BotType type, string geotag, string token, ITelegramBotClient bot)
        {
            switch (type)
            {
                case BotType.aviator_v0:
                    return new MessageProcessor_v0(geotag, token, bot);
                case BotType.aviator_v1:
                    return new MessageProcessor_v1(geotag, token, bot);
                case BotType.aviator_v2_1w_br_eng:
                    return new MessageProcessor_v2(geotag, token, bot);
                case BotType.aviator_v2_1win_br_esp:
                    return new MessageProcessor_v2_lat(geotag, token, bot);
                case BotType.aviator_v3_1win_wv_eng:
                    return new MessageProcessor_v3(geotag, token, bot);
                case BotType.aviator_v4_cana34:
                    return new MessageProcessor_cana34(geotag, token, bot);
                case BotType.aviator_v4_cana35:
                    return new MessageProcessor_cana35(geotag, token, bot);
                default:
                    return null;
            }
        }
    }
}
