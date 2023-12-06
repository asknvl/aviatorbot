using asknvl.server;
using aviatorbot.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace aviatorbot.Models.messages
{
    internal class MessageProcessor_v3 : MessageProcessorBase
    {
        public override ObservableCollection<messageControlVM> MessageTypes { get; }

        public override StateMessage GetChatJoinMessage()
        {
            throw new NotImplementedException();
        }

        public override StateMessage GetMessage(string status, string? link = null, string? pm = null, string? uuid = null, string? channel = null, bool? isnegative = false)
        {
            string code = string.Empty;
            InlineKeyboardMarkup markUp = null;


            switch (status)
            {
                case "webapp":
                    markUp = getWebAppMarkup(link);
                    code = "webapp";
                    break;
            }

            StateMessage msg = null;

            if (messages.ContainsKey(code))
            {
                msg = messages[code];//.Clone();
                msg.Message.ReplyMarkup = markUp;
            }
            else
            {
                var found = MessageTypes.FirstOrDefault(m => m.Code.Equals(code));
                if (found != null)
                    found.IsSet = false;

            }

            return msg;
        }

        public override StateMessage GetMessage(TGBotFollowersStatApi.tgFollowerStatusResponse? resp, string? link = null, string? pm = null, string? channel = null, bool? isnegative = false)
        {
            throw new NotImplementedException();
        }

        public override StateMessage GetPush(string? code, string? link = null, string? pm = null, string? uuid = null, string? channel = null, bool? isnegative = false)
        {
            throw new NotImplementedException();
        }

        public MessageProcessor_v3(string geotag, string token, ITelegramBotClient bot) : base(geotag, token, bot)
        {
            MessageTypes = new ObservableCollection<messageControlVM>()
            {
                new messageControlVM(this)
                {
                    Code = "webapp",
                    Description = "webapp"
                }
            };
        }

        InlineKeyboardMarkup getWebAppMarkup(string link)
        {
            InlineKeyboardButton[][] buttons = new InlineKeyboardButton[1][];
            

            var winfo = new WebAppInfo()
            {
                Url = link              
            };

            buttons[0] = new InlineKeyboardButton[] { InlineKeyboardButton.WithWebApp(text: "ПОИГРАТЬ ✈ В ТГ", winfo)};

            return buttons;
        }
    }
}
