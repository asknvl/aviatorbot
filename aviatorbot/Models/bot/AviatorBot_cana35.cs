﻿using asknvl.logger;
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
    public class AviatorBot_cana35 : AviatorBot_v2
    {
        public override BotType Type => BotType.aviator_v4_cana35;

        public AviatorBot_cana35(BotModel model, IOperatorStorage operatorStorage, IBotStorage botStorage, ILogger logger) : base(model, operatorStorage, botStorage, logger)
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