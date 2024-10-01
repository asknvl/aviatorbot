using asknvl.logger;
using botservice.Model.bot;
using botservice.Models.bot.aviator;
using botservice.Models.storage;
using motivebot.Model.storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace aviatorbot.Models.bot.aviator
{
    public class ModeratorBot_tier1_itaa : ModeratorBot_Aviator_base
    {
        public override BotType Type => BotType.moderator_tier1_itaa;
        public ModeratorBot_tier1_itaa(BotModel model, IOperatorStorage operatorStorage, IBotStorage botStorage, ILogger logger) : base(model, operatorStorage, botStorage, logger)
        {
        }
    }
}
