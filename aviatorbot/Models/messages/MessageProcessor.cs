using aksnvl.messaging;
using aksnvl.storage;
using asknvl.messaging;
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
            reg_buttons[0] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: "📲REGISTER", $"{link}/?id={uuid}")};
            reg_buttons[1] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: "🔍CHECK REGISTRATION", callbackData: "check_register") };
            reg_buttons[2] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: "🧑🏻‍💻Help", $"https://t.me/{pm.Replace("@", "")}") };

            return reg_buttons;
        }

        InlineKeyboardMarkup getFDMarkup(string link, string uuid)
        {
            InlineKeyboardButton[][] dep_buttons = new InlineKeyboardButton[2][];
            dep_buttons[0] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: "💸TOP UP", $"{link}/?id={uuid}") };
            dep_buttons[1] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: "🔍CHECK TOP-UP", callbackData: $"check_fd") };           
            return dep_buttons;
        }

        InlineKeyboardMarkup getRD1Markup(string link, string uuid)
        {
            InlineKeyboardButton[][] dep_buttons = new InlineKeyboardButton[2][];
            dep_buttons[0] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: "💸Deposit", $"{link}/?id={uuid}") };
            dep_buttons[1] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: "🔍CHECK TOP-UP", callbackData: $"check_rd1") };
            return dep_buttons;
        }

        InlineKeyboardMarkup getVipMarkup(string link, string uuid)
        {
            InlineKeyboardButton[][] vip_buttons = new InlineKeyboardButton[2][];
            vip_buttons[0] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: "🔥GO TO VIP🔥", $"https://t.me/+BWcUWfU3HEVlOWRk") };
            vip_buttons[1] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: "🔍PLAY💰", $"{link}/?id={uuid}") };
            return vip_buttons;
        }
        #endregion

        #region public
        public async void Add(Message message, string pm)
        {
            var pattern = await StateMessage.Create(bot, message, geotag);
            AutoChange pm_autochange = new AutoChange()
            {
                OldText = "@booowos",
                NewText = pm
            };
            var autochanges = new List<AutoChange>() { pm_autochange };
            pattern.MakeAutochange(autochanges);

            pattern.Id = messages.Count();
            messages.Add(pattern);
            messageStorage.save(messages);
        }

        public void Clear()
        {
            messages.Clear();
            messageStorage.save(messages);
        }

        public async Task<PushMessageBase> GetMessage(string status,
                                                        string link = null,
                                                        string pm = null,
                                                        string uuid = null,
                                                        string channel = null,
                                                        bool? isnegative = false)
        {               
            int index = 0;
            InlineKeyboardMarkup markUp = null;

            switch (status)
            {             

                case "WREG":
                    markUp = getRegMarkup(link, pm, uuid);
                    index = (isnegative == true) ? 1 : 0;
                    break;

                case "WFDEP":
                    index = (isnegative == true) ? 3 : 2;
                    markUp = getFDMarkup(link, uuid);
                    break;

                case "WREDEP1":
                    index = (isnegative == true) ? 5 : 4;
                    markUp = getRD1Markup(link, uuid);
                    break;

                //case "WREDEP2":                    
                //    break;

                default:
                    index = 6;
                    markUp = getVipMarkup(link, uuid);
                    break;
                    
            }

            var msg = messages[index].Clone();
            msg.Message.ReplyMarkup = markUp;

            return msg;
        }
        #endregion
    }
}
