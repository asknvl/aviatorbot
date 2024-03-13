using asknvl.logger;
using aviatorbot.Model.bot;
using aviatorbot.Models.storage;
using motivebot.Model.storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace aviatorbot.Models.bot
{
    public class LandingBot_cana37_raceup_nopostbacks : LandingBot_cana34_raceup_nopostbacks
    {
        public override BotType Type => BotType.landing_v0_cut_cana37; 
        public LandingBot_cana37_raceup_nopostbacks(BotModel model, IOperatorStorage operatorStorage, IBotStorage botStorage, ILogger logger) : base(model, operatorStorage, botStorage, logger)
        {
            Geotag = model.geotag;
            Token = model.token;
            SUPPORT_PM = model.support_pm;
            PM = model.pm;
            ChannelTag = model.channel_tag;
            Channel = model.channel;

            Help = model.help;
            Training = model.training;
            Reviews = model.reveiews;
            Strategy = model.strategy;
            Vip = model.vip;

            Postbacks = model.postbacks;
        }
    }
}
