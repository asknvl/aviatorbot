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
        public MessageProcessorBase Get(BotType type, string geotag, ITelegramBotClient bot)
        {
            switch (type)
            {
                case BotType.aviator_v0:
                    return new MessageProcessor_v0(geotag, bot);
                case BotType.aviator_v1:
                    return new MessageProcessor_v1(geotag, bot);
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
