using aksnvl.messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace botservice.Models.messages
{
    public interface IMessageProcessor
    {
        public int start_push_number { get; }
        Task<PushMessageBase> GetMessage(string status,
                                            string link = null,
                                            string pm = null,
                                            string uuid = null,
                                            string channel = null,
                                            bool? isnegative = false);

        (StateMessage, ReplyKeyboardMarkup) GetMessageAndReplyMarkup(string status);

        void Add(Message message, string pm);
        void Clear();
       
    }
}
