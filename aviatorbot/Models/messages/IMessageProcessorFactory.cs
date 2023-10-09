using aviatorbot.Model.bot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;

namespace aviatorbot.Models.messages
{
    public interface IMessageProcessorFactory
    {
        MessageProcessorBase Get(BotType type, string geotag, ITelegramBotClient bot);
    }
}
