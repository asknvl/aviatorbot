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

        public AviatorBotBase Get(BotModel model, ILogger logger)
        {
            switch (model.type)
            {
                case BotType.aviator_v0:
                    return new AviatorBot_v0(model, operatorStorage, botStorage, logger);
                case BotType.aviator_v1:
                    return new AviatorBot_v1(model, operatorStorage, botStorage, logger);
                case BotType.aviator_v2_1w_br_eng:
                    return new AviatorBot_v2(model, operatorStorage, botStorage, logger);                
                case BotType.getinfo_v0:
                    return new infoBot(model, operatorStorage, botStorage, logger);
                case BotType.aviator_v3_1win_wv_eng:
                    return new AviatorBot_v3(model, operatorStorage, botStorage, logger);
                case BotType.aviator_v2_1win_br_esp:
                    return new AviatorBot_v2_lat(model, operatorStorage, botStorage, logger);
                case BotType.aviator_v4_cana34:
                    return new AviatorBot_cana34(model, operatorStorage, botStorage, logger);
                case BotType.aviator_v4_cana35:
                    return new AviatorBot_cana35(model, operatorStorage, botStorage, logger);
                case BotType.aviator_v3_1win_wv_esp:
                    return new AviatorBot_v3_lat(model, operatorStorage, botStorage, logger);
                case BotType.landing_v0_1win_wv_eng:
                    return new LandingBot_v0(model, operatorStorage, botStorage, logger);
                case BotType.landing_v0_cut_cana37:
                    return new LandingBot_cana_raceup(model, operatorStorage, botStorage, logger);
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
