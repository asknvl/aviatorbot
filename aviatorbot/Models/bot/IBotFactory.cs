using asknvl.logger;
using aviatorbot.Model.bot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace aviatorbot.Models.bot
{
    public interface IBotFactory
    {
        AviatorBotBase Get(BotModel model, ILogger logger);
    }
}
