using asknvl.logger;
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
        #region vars
        ILogger logger;
        #endregion

        public MessageProcessorFactory(ILogger logger) { 
            this.logger = logger;
        }

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
                    return new MessageProcessor_v3(geotag, token, bot, logger);
                case BotType.aviator_v4_cana34:
                    return new MessageProcessor_cana34(geotag, token, bot);
                case BotType.aviator_v4_cana35:
                    return new MessageProcessor_cana35(geotag, token, bot);
                case BotType.aviator_v3_1win_wv_esp:
                    return new MessageProcessor_v3_lat(geotag, token, bot, logger);
                case BotType.landing_v0_1win_wv_eng:

                    string payment_address = "www.ya.ru";
                    string strategy_channel = "https://t.me/+e6ICUiDSpqk2ZTA0";
                    string vip_channel = "https://t.me/+88i3qPf6BII5MWVk";
                    string trainig_channel = "https://t.me/+yhzfSgWGqmExZjJk";

                    return new MP_landing_v0(geotag, token, bot, logger, payment_address, strategy_channel, vip_channel, trainig_channel);

                default:
                    return null;
            }
        }
    }
}
