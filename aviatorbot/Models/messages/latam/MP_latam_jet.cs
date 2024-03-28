using asknvl.server;
using botservice.Models.messages;
using botservice.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace botservice.Models.messages.latam
{
    public class MP_latam_jet : MessageProcessorBase
    {
        public override ObservableCollection<messageControlVM> MessageTypes { get; }

        public MP_latam_jet(string geotag, string token, ITelegramBotClient bot) : base(geotag, token, bot)
        {
            MessageTypes = new ObservableCollection<messageControlVM>()
            {
                new messageControlVM(this)
                {
                    Code = "start",
                    Description = "Первое сообщение"
                }
            };

            for (int i = 1; i <= 17; i++)
            {
                var mcv = new messageControlVM(this)
                {
                    Code = $"WREG{i}",
                    Description = $"Пуш {i}"
                };

                MessageTypes.Add(mcv);
            }
        }

        Random rand = new Random();
        string getPushButtonName()
        {
            return "🚀CAMINO AL ÉXITO🚀";            
        }

        virtual protected InlineKeyboardMarkup getSubscribeMarkup(string landing_channel, string pm)
        {
            InlineKeyboardButton[][] buttons = new InlineKeyboardButton[2][];
            buttons[0] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: "✅SUSCRIBETE✅", $"{landing_channel}") };
            buttons[1] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: "📩TEXTARME📩", $"https://t.me/{pm.Replace("@", "")}") };
            return buttons;
        }

        virtual protected InlineKeyboardMarkup getPushMarkup(string pm)
        {
            InlineKeyboardButton[][] buttons = new InlineKeyboardButton[1][];
            buttons[0] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: getPushButtonName(), $"https://t.me/{pm.Replace("@", "")}") };            
            return buttons;
        }

        public override StateMessage GetMessage(string status, string? link = null, string? support_pm = null, string? pm = null, string? uuid = null, string? channel = null, bool? isnegative = false, string? training = null, string? vip = null, string? help = null)
        {
            string code = string.Empty;
            InlineKeyboardMarkup markUp = null;

            switch (status)
            {
                case "start":
                    markUp = getSubscribeMarkup(channel, pm);
                    code = "start";
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

        public override StateMessage GetPush(string? code, string? link = null, string? pm = null, string? uuid = null, string? channel = null, bool? isnegative = false)
        {
            StateMessage push = null;

            var found = messages.ContainsKey(code);
            if (found)
            {
                InlineKeyboardMarkup markup = null;
                markup = getPushMarkup(pm);
                push = messages[code].Clone();
                push.Message.ReplyMarkup = markup;
            }
            return push;
        }

        public override StateMessage GetMessage(TGBotFollowersStatApi.tgFollowerStatusResponse? resp, string? link = null, string? support_pm = null, string? pm = null, string? channel = null, bool? isnegative = false, string? training = null, string? vip = null, string? help = null)
        {
            throw new NotImplementedException();
        }

        public override StateMessage GetPush(TGBotFollowersStatApi.tgFollowerStatusResponse? resp, string? code, string? link = null, string? support_pm = null, string? pm = null, string? channel = null, bool? isnegative = false, string? vip = null, string? help = null)
        {
            throw new NotImplementedException();
        }

        public override StateMessage GetChatJoinMessage()
        {
            throw new NotImplementedException();
        }
    }
}
