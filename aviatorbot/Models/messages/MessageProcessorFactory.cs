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
                case BotType.landing_v0_1win_wv_eng:
                    return new MP_landing_v0(geotag, token, bot, logger);

                case BotType.landing_v0_cut_cana34:                    
                    return new MP_Landing_Raceup_cana(geotag, token, bot, logger,
                        "https://linkraceupcasinoaffiliate.com/da062c1a4",
                        "https://linkraceupcasinoaffiliate.com/dac82359e",
                        "https://linkraceupcasinoaffiliate.com/d1e61b393"
                        );

                default:
                    return null;
            }
        }
    }
}
