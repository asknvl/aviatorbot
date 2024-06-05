using asknvl.logger;
using asknvl.server;
using aviatorbot.Models.user_storage;
using botservice.Model.bot;
using botservice.Models.storage;
using motivebot.Model.storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace botservice.Models.bot.latam
{
    public class LatamBot_basic : LatamBotBase
    {
        public override BotType Type => BotType.latam_basic_esp;

        public LatamBot_basic(BotModel model, IOperatorStorage operatorStorage, IBotStorage botStorage, IDBStorage dbStorage, ILogger logger) : base(model, operatorStorage, botStorage, dbStorage, logger)
        {

        }       
    }
}
