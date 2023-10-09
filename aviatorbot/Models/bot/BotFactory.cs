using asknvl.logger;
using aviatorbot.Model.bot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace aviatorbot.Models.bot
{
    public class BotFactory : IBotFactory
    {
        public AviatorBotBase Get(BotModel model, ILogger logger)
        {
            switch (model.type)
            {
                case BotType.aviator_v0:
                    return new AviatorBot_v0(model, logger);
                    case BotType.aviator_v1:
                        return new AviatorBot_v1(model, logger);
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
