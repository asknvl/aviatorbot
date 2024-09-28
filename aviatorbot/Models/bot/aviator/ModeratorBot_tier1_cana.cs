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
    public class ModeratorBot_tier1_cana : ModeratorBot_raceup_tier1_base
    {
        public override BotType Type => BotType.moderator_tier1_cana;
        public ModeratorBot_tier1_cana(BotModel model, IOperatorStorage operatorStorage, IBotStorage botStorage, ILogger logger) : base(model, operatorStorage, botStorage, logger)
        {
        }
    }
}
