using asknvl.logger;
using aviatorbot.Model.bot;
using aviatorbot.Models.storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace aviatorbot.Models.bot
{
    public class AviatorBot_v3 : AviatorBotBase
    {
        public override BotType Type => BotType.aviator_v3;

        public AviatorBot_v3(BotModel model, IOperatorStorage operatorStorage, ILogger logger) : base(operatorStorage, logger)
        {
            Geotag = model.geotag;
            Token = model.token;
            Link = model.link;
            PM = model.pm;
            Channel = model.channel;
        }

        public override async Task processFollower(Message message)
        {
            long chat = message.Chat.Id;

            if (message.Text.Contains("/start"))
            {
                try
                {
                    var m = MessageProcessor.GetMessage("webapp", Link);
                    await m.Send(chat, bot, null);
                } catch (Exception ex)
                {

                }
            }
        }

    }
}
