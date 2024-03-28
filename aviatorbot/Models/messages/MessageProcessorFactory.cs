using asknvl.logger;
using botservice.Models.messages.latam;
using botservice.Model.bot;
using Telegram.Bot;

namespace botservice.Models.messages
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
                    return new MP_landing_br_1w_hack(geotag, token, bot, logger);

                case BotType.landing_v0_cut_cana34:                    
                    return new MP_landing_br_Raceup_cana(geotag, token, bot, logger,
                        "https://linkraceupcasinoaffiliate.com/da062c1a4",
                        "https://linkraceupcasinoaffiliate.com/dac82359e",
                        "https://linkraceupcasinoaffiliate.com/d1e61b393"
                        );

                case BotType.landing_v0_cut_cana37:
                    return new MP_landing_br_Raceup_cana(geotag, token, bot, logger,
                        "https://linkraceupcasinoaffiliate.com/d00a9a9e4",
                        "https://linkraceupcasinoaffiliate.com/de84b36ee",
                        "https://linkraceupcasinoaffiliate.com/d03213077"
                        );

                case BotType.landing_v0_strategies:
                    return new MP_landing_br_1w_strategies(geotag, token, bot);

                case BotType.latam_basic_esp:
                    return new MP_latam_basic(geotag, token, bot);

                case BotType.latam_jet_esp:
                    return new MP_latam_jet(geotag, token, bot);

                case BotType.landing_vishal:
                    return new MP_landing_br_1w_vishal(geotag, token, bot, logger);


                default:
                    return null;
            }
        }
    }
}
