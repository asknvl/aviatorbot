using asknvl.logger;
using botservice.Model.bot;
using botservice.Models.bot.gmanager;
using botservice.Models.storage;
using motivebot.Model.storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace aviatorbot.Models.bot.group_moderator
{
    public class GroupManagerBot_raceup : GroupManagerBotBase
    {
        #region properties
        public override BotType Type => BotType.group_manager_raceup;
        #endregion
        public GroupManagerBot_raceup(BotModel model, IOperatorStorage operatorStorage, IBotStorage botStorage, ILogger logger) : base(model, operatorStorage, botStorage, logger)
        {
        }        
    }
}
