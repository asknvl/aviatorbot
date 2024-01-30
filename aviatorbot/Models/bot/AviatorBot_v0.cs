using asknvl.logger;
using aviatorbot.Model.bot;
using aviatorbot.Models.storage;
using aviatorbot.Operators;
using aviatorbot.rest;
using motivebot.Model.storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace aviatorbot.Models.bot
{
    public class AviatorBot_v0 : AviatorBotBase
    {
        public override BotType Type => BotType.aviator_v0;

        public AviatorBot_v0(BotModel model, IOperatorStorage operatorStorage, IBotStorage botStorage, ILogger logger) : base(operatorStorage, botStorage, logger)
        {
            Geotag = model.geotag;
            Token = model.token;
            Link = model.link;
            PM = model.pm;
            Channel = model.channel;
            Postbacks = model.postbacks;
        }

        public override Task UpdateStatus(StatusUpdateDataDto updateData)
        {
            throw new NotImplementedException();
        }

        public override Task Notify(object notifyObject)
        {
            throw new NotImplementedException();
        }
    }
}
