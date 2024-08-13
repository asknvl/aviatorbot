using asknvl.logger;
using botservice.Models.messages.latam;
using botservice.Model.bot;
using Telegram.Bot;
using aviatorbot.Models.messages.latam;

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
                    return new MP_landing_br_Raceup_cana_v1(geotag, token, bot, logger,
                        "https://linkraceupcasinoaffiliate.com/da062c1a4",
                        "https://linkraceupcasinoaffiliate.com/dac82359e",
                        "https://linkraceupcasinoaffiliate.com/d1e61b393"
                        );

                case BotType.landing_v0_cut_cana37:
                    return new MP_landing_br_Raceup_cana_v1(geotag, token, bot, logger,
                        "https://linkraceupcasinoaffiliate.com/d00a9a9e4",
                        "https://linkraceupcasinoaffiliate.com/de84b36ee",
                        "https://linkraceupcasinoaffiliate.com/d03213077"
                        );

                case BotType.landing_v0_strategies:
                    return new MP_landing_br_1w_strategies(geotag, token, bot);

                case BotType.latam_basic_esp:
                    return new MP_latam_basic_v1(geotag, token, bot);

                case BotType.latam_jet_esp:
                    return new MP_latam_jet_v1(geotag, token, bot);

                case BotType.landing_vishal:
                    return new MP_landing_br_1w_vishal(geotag, token, bot, logger);

                case BotType.latam_smrnv:
                    return new MP_latam_smrnv(geotag, token, bot);

                case BotType.latam_basic_v2:
                    return new MP_latam_basic_v2(geotag, token, bot);

                case BotType.landing_hack_v2_basic:
                    return new MP_landing_br_1w_hack_v2(geotag, token, bot, logger);

                case BotType.landing_hack_v3_basic:
                    return new MP_landing_br_1w_hack_v2(geotag, token, bot, logger);

                case BotType.moderator_v2_strategies:
                    //return new MP_ind_strategy_basic_v2(geotag, token, bot);
                    return new MP_ind_strategy_basic_v2_no_link(geotag, token, bot);

                case BotType.moderator_cana34_raceup:
                    return new MP_cana34_basic_v2(geotag, token, bot);

                case BotType.moderator_cana35_raceup:
                    return new MP_cana35_basic_v2(geotag, token, bot);

                case BotType.moderator_inda120_raceup:
                    return new MP_inda120_basic_v2(geotag, token, bot);

                case BotType.moderator_deua01_raceup:
                    return new MP_deua01_basic_v2(geotag, token, bot);

                case BotType.moderator_cana53_raceup:
                    return new MP_cana53_basic_v2(geotag, token, bot);

                case BotType.trading_basic:
                    return new MP_trading_basic(geotag, token, bot);

                case BotType.pusher:
                    return new MP_pusher(geotag, token, bot);  

                default:
                    return null;
            }
        }
    }
}
