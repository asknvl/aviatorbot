using aviatorbot.Model.bot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;

namespace aviatorbot.Models.messages
{
    public class MessageProcessorFactory : IMessageProcessorFactory
    {
        public MessageProcessorBase Get(BotType type, string geotag, string token, ITelegramBotClient bot)
        {
            switch (type)
            {
                case BotType.aviator_v0:
                    return new MessageProcessor_v0(geotag, token, bot);
                case BotType.aviator_v1:
                    return new MessageProcessor_v1(geotag, token, bot);
                    case BotType.aviator_v2:
                    return new MessageProcessor_v2(geotag, token, bot);
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
