using asknvl.logger;
using aviatorbot.Model.bot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace aviatorbot.Models.bot
{
    public class AviatorBot_v0 : AviatorBotBase
    {
        public AviatorBot_v0(BotModel model, ILogger logger) : base(logger)
        {
            Geotag = model.geotag;
            Token = model.token;
            Link = model.link;
            PM = model.pm;
            Channel = model.channel;

            foreach (var item in model.operators_id)
                if (!Operators.Contains(item))
                    Operators.Add(item);

        }
    }
}
