using asknvl.logger;
using aviatorbot.Models.bot.latam;
using botservice.Model.bot;
using botservice.Models.bot.aviator;
using botservice.Models.bot.latam;
using botservice.Models.storage;
using motivebot.Model.storage;
using System;

namespace botservice.Models.bot
{
    public class BotFactory : IBotFactory
    {

        #region vars        
        IOperatorStorage operatorStorage;
        IBotStorage botStorage;
        #endregion

        public BotFactory(IOperatorStorage operatorStorage, IBotStorage botStorage)
        {            
            this.operatorStorage = operatorStorage;
            this.botStorage = botStorage;
        }

        public BotBase Get(BotModel model, ILogger logger)
        {
            switch (model.type)
            {       
                case BotType.getinfo_v0:
                    return new infoBot(model, operatorStorage, botStorage, logger);             
                case BotType.landing_v0_1win_wv_eng:
                    return new LandingBot_v0(model, operatorStorage, botStorage, logger);
                case BotType.landing_v0_cut_cana34:
                    //return new LandingBot_cana_raceup(model, operatorStorage, botStorage, logger);
                    return new LandingBot_cana34_raceup_nopostbacks(model, operatorStorage, botStorage, logger);
                case BotType.landing_v0_cut_cana37:
                    return new LandingBot_cana37_raceup_nopostbacks(model, operatorStorage, botStorage, logger);
                case BotType.landing_v0_strategies:
                    return new LandingBot_strategies(model, operatorStorage, botStorage, logger);
                case BotType.latam_basic_esp:
                    return new LatamBot_basic(model, operatorStorage, botStorage, logger);
                case BotType.latam_jet_esp:
                    return new LatamBot_jet(model, operatorStorage, botStorage, logger);
                case BotType.landing_vishal:
                    return new LandingBot_vishal(model, operatorStorage, botStorage, logger);
                case BotType.latam_smrnv:
                    return new Latam_smrnv(model, operatorStorage, botStorage, logger);
                case BotType.latam_basic_v2:
                    return new LatamBot_basic_v2(model, operatorStorage, botStorage, logger);
                case BotType.landing_hack_v2_basic:
                    return new LandingBot_hack_basic_v2(model, operatorStorage, botStorage, logger);
                case BotType.moderator_v2_strategies:
                    return new ModeratorBot_strategies_basic_v2(model, operatorStorage, botStorage, logger);
                case BotType.moderator_cana_raceup:
                    return new ModeratorBot_cana_raceup(model, operatorStorage, botStorage, logger);    
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
