using asknvl.logger;
using aviatorbot.Model.bot;
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
        IOperatorsProcessor operatorsProcessor;
        IBotStorage botStorage;
        #endregion

        public BotFactory(IBotStorage botStorage)
        {
            this.botStorage = botStorage;
            operatorsProcessor = new LocalOperatorProcessor(botStorage);
        }

        public AviatorBotBase Get(BotModel model, ILogger logger)
        {
            switch (model.type)
            {
                case BotType.aviator_v0:
                    return new AviatorBot_v0(model, operatorsProcessor, logger);
                case BotType.aviator_v1:
                    return new AviatorBot_v1(model, operatorsProcessor, logger);
                case BotType.aviator_v2:
                    return new AviatorBot_v2(model, operatorsProcessor, logger);
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
