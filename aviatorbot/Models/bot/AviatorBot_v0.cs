using asknvl.logger;
using aviatorbot.Model.bot;
using aviatorbot.Models.storage;
using aviatorbot.Operators;
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

        public AviatorBot_v0(BotModel model, IOperatorStorage operatorStorage, ILogger logger) : base(operatorStorage, logger)
        {
            Geotag = model.geotag;
            Token = model.token;
            Link = model.link;
            PM = model.pm;
            Channel = model.channel;

            //foreach (var item in model.operators_id)
            //    if (!Operators.Contains(item))
            //        Operators.Add(item);

            foreach (var item in model.operators)
                if (!Operators.Contains(item))
                    Operators.Add(item);
        }
    }
}
