using asknvl.logger;
using aviatorbot.Model.bot;
using aviatorbot.Models.storage;
using motivebot.Model.storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace aviatorbot.Models.bot
{
    public class AviatorBot_v3 : AviatorBot_v2
    {
        public override BotType Type => BotType.aviator_v3;

        public AviatorBot_v3(BotModel model, IOperatorStorage operatorStorage, IBotStorage botStorage, ILogger logger) : base(model, operatorStorage, botStorage, logger)
        {
            Geotag = model.geotag;
            Token = model.token;
            Link = model.link;
            PM = model.pm;
            Channel = model.channel;
            Postbacks = model.postbacks;
        }

    }
}
