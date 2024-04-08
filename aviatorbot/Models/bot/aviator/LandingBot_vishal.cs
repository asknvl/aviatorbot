using aksnvl.messaging;
using asknvl.logger;
using asknvl.server;
using botservice.Model.bot;
using botservice.Models.messages;
using botservice.Models.storage;
using botservice.Operators;
using botservice.rest;
using motivebot.Model.storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using static asknvl.server.TGBotFollowersStatApi;

namespace botservice.Models.bot.aviator
{
    public class LandingBot_vishal : LandingBot_v0
    {
        #region vars
        Dictionary<long, int> prevRegIds = new();
        #endregion
        public override BotType Type => BotType.landing_vishal;
        public LandingBot_vishal(BotModel model, IOperatorStorage operatorStorage, IBotStorage botStorage, ILogger logger) : base(model, operatorStorage, botStorage, logger)
        {
            Geotag = model.geotag;
            Token = model.token;
            Link = model.link;
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
