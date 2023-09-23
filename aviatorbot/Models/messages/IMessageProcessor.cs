using aksnvl.messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace aviatorbot.Models.messages
{
    public interface IMessageProcessor
    {
        Task<PushMessageBase> GetMessage(long userid, string link = null, string pm = null);
        public void Add(Message message);
    }
}
