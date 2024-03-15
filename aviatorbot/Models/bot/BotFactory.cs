using asknvl.logger;
using aviatorbot.Model.bot;
using aviatorbot.Models.storage;
using aviatorbot.Models.storage.local;
using aviatorbot.Operators;
using motivebot.Model.storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace aviatorbot.Models.bot
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
                case BotType.landing_v0_strategies:
                    return new LandingBot_strategies(model, operatorStorage, botStorage, logger);                    
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
