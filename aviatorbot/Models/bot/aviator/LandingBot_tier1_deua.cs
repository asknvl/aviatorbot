﻿using asknvl.logger;
using botservice.Model.bot;
using botservice.Models.storage;
using motivebot.Model.storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace botservice.Models.bot.aviator
{
    public class LandingBot_tier1_deua : LandingBot_raceup_tier1_base
    {
        public override BotType Type => BotType.landing_tier1_deua;
        public LandingBot_tier1_deua(BotModel model, IOperatorStorage operatorStorage, IBotStorage botStorage, ILogger logger) : base(model, operatorStorage, botStorage, logger)
        {
        }     
    }
}