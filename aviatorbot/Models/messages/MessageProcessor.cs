using aksnvl.messaging;
using aksnvl.storage;
using asknvl.storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace aviatorbot.Models.messages
{
    public class MessageProcessor : IMessageProcessor
    {
        #region vars
        List<StateMessage> messages = new();
        IStorage<List<StateMessage>> messageStorage;
        string geotag;
        ITelegramBotClient bot;
        #endregion

        public MessageProcessor(string geotag, ITelegramBotClient bot)
        {
            this.geotag = geotag;
            this.bot = bot; 

            messageStorage = new Storage<List<StateMessage>>("messages.json", "messages", messages);
            messages = messageStorage.load();
        }

        #region private
        InlineKeyboardMarkup getRegMarkup(string link, string pm, string uuid)
        {
            InlineKeyboardButton[][] reg_buttons = new InlineKeyboardButton[3][];
            reg_buttons[0] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: "📲Register", $"{link}?id={uuid}")};
            reg_buttons[1] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: "🔍Check registration", callbackData: "check_state") };
            reg_buttons[2] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: "🧑🏻‍💻Help", $"https://t.me/{pm.Replace("@", "")}") };

            return reg_buttons;
        }
        #endregion

        #region public
        public async void Add(Message message)
        {
            var pattern = await StateMessage.Create(bot, message, geotag);
            pattern.Id = messages.Count();
            messages.Add(pattern);
            messageStorage.save(messages);
        }
        public async Task<PushMessageBase> GetMessage(long userid, string link = null, string pm = null)
        {

            string state = "WREG";
            int index = 0;
            string uuid = "1488";

            InlineKeyboardMarkup markUp = null;

            switch (state)
            {
                case "WREG":
                    markUp = getRegMarkup(link, pm, uuid);
                    break;

                case "WFDEP":
                    index = 1;
                    break;

                case "WREDEP1":
                    index = 2;
                    break;

                case "WREDEP2":
                    index = 3;
                    break;

                default:
                    break;
                    
            }

            var msg = messages[index].Clone();
            msg.Message.ReplyMarkup = markUp;

            return msg;
        }
        #endregion
    }
}
