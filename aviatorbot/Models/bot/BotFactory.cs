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

        public BotFactory(IOperatorStorage operatorStorage)
        {            
            this.operatorStorage = operatorStorage;
        }

        public AviatorBotBase Get(BotModel model, ILogger logger)
        {
            switch (model.type)
            {
                case BotType.aviator_v0:
                    return new AviatorBot_v0(model, operatorStorage, logger);
                case BotType.aviator_v1:
                    return new AviatorBot_v1(model, operatorStorage, logger);
                case BotType.aviator_v2:
                    return new AviatorBot_v2(model, operatorStorage, logger);
                case BotType.getinfo_v0:
                    return new infoBot(model, operatorStorage, logger);

                default:
                    throw new NotImplementedException();
            }
        }
    }
}
