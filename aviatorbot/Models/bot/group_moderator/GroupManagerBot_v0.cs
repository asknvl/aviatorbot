using asknvl.logger;
using botservice.Model.bot;
using botservice.Models.storage;
using botservice.Operators;
using motivebot.Model.storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace botservice.Models.bot.gmanager
{
    public class GroupManagerBot_v0 : GroupManagerBotBase
    {

        public override BotType Type => BotType.group_manager;

        public GroupManagerBot_v0(BotModel model, IOperatorStorage operatorStorage, IBotStorage botStorage, ILogger logger) : base(model, operatorStorage, botStorage, logger)
        {
        }

    }
}
