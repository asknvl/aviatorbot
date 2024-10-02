using asknvl.logger;
using botservice.Models.messages.latam;
using botservice.Model.bot;
using Telegram.Bot;
using aviatorbot.Models.messages.latam;
using aviatorbot.Models.messages.group_manager;
using aviatorbot.Models.messages.raceup;
using botservice.Models.messages.raceup;
using aviatorbot.Models.messages.mostbet;

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
                    return new MP_landing_br_1w_hack_v2_vip(geotag, token, bot, logger);

                case BotType.landing_hack_v3_basic:
                    return new MP_landing_br_1w_hack_v2_vip(geotag, token, bot, logger);

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

                case BotType.lottery_basic_v2:
                    return new MP_lottery_basic_v2(geotag, token, bot);

                case BotType.moderator_itaa07_raceup:
                    return new MP_itaa07_basic_v2(geotag, token, bot);

                case BotType.pusher:
                    return new MP_pusher(geotag, token, bot);

                case BotType.group_manager_inda:
                    return new MP_group_manager_inda(geotag, token, bot);

                case BotType.group_manager_raceup:
                    return new MP_group_manager_tier1(geotag, token, bot);

                case BotType.landing_tier1_cana:
                    return new MP_landing_raceup(geotag, token, bot, Languages.en);

                case BotType.landing_tier1_deua:
                    return new MP_landing_raceup(geotag, token, bot, Languages.de);

                case BotType.landing_tier1_itaa:
                    return new MP_landing_raceup(geotag, token, bot, Languages.it);

                case BotType.landing_tier1_deua_postback:
                    return new MP_landing_raceup_postback(geotag, token, bot, Languages.de);

                case BotType.moderator_tier1_cana:
                    return new MP_modertator_raceup(geotag, token, bot, Languages.en);

                case BotType.moderator_tier1_deua:
                    return new MP_modertator_raceup(geotag, token, bot, Languages.de);

                case BotType.moderator_tier1_itaa:
                    return new MP_modertator_raceup(geotag, token, bot, Languages.it);

                case BotType.moderator_mostbet_inda:
                    return new MP_moderator_mostbet(geotag, token, bot, Languages.en);

                default:
                    return null;
            }
        }
    }
}
