using asknvl.server;
using Avalonia.Styling;
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

namespace aviatorbot.Models.messages.latam
{
    public class MP_lottery_basic_v2 : MessageProcessorBase
    {
        #region const
        override public int start_push_number {get; set;} = 7;

        override public string[] hi_outs { get; set; } = {
            "🎉reservar un método🎉",
            "LOTERÍA🔥",
            "🔥LUGAR🔥",
            "LOTERÍA🔥",
            "VICTORIA🎉",
            "LOTERÍA🔥",
            "🎉GANADOR🎉"
        };
        #endregion

        public override ObservableCollection<messageControlVM> MessageTypes { get; }

        public MP_lottery_basic_v2(string geotag, string token, ITelegramBotClient bot) : base(geotag, token, bot)
        {
            MessageTypes = new ObservableCollection<messageControlVM>();

            for (int i = 0; i < start_push_number; i++)
            {
                MessageTypes.Add(new messageControlVM(this)
                {
                    Code = $"hi_{i}_in",
                    Description = $"Старт-пуш {i + 1}"
                });                
            }

            MessageTypes.Add(new messageControlVM(this)
            {
                Code = $"hi_out",
                Description = $"Ответ на Старт-пуш"
            });

            MessageTypes.Add(new messageControlVM(this)
            {
                Code = "BYE",
                Description = "Пощальное"
            });

            for (int i = 1; i <= 56; i++)
            {
                MessageTypes.Add(new messageControlVM(this)
                {
                    Code = $"WREG{i}",
                    Description = $"Пуш WREG{i}"
                });
            }           
        }

        ReplyKeyboardMarkup getStartMarkup(string text)
        {
            ReplyKeyboardMarkup button = new(new[]
            {
                new KeyboardButton[] { $"{text}" },
            })
            {
                ResizeKeyboard = true,
                Selective = true,
                OneTimeKeyboard = true
            };
            return button;
        }


        Random rand = new Random();
        string getPushButtonName()
        {
            string[] names = new[] { "💸TOMA EL DINERO💸", "💸ESCRIBE💸", "✍️ESCRÍBEME✍️", "🔥QUIERO DINERO🔥" };
            return names[rand.Next(names.Length)];
        }

        virtual protected InlineKeyboardMarkup getPushMarkup(string pm)
        {
            InlineKeyboardButton[][] buttons = new InlineKeyboardButton[1][];
            buttons[0] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: getPushButtonName(), $"https://t.me/{pm.Replace("@", "")}") };
            return buttons;
        }

        virtual protected InlineKeyboardMarkup getHiOutMarkup(string pm)
        {
            InlineKeyboardButton[][] buttons = new InlineKeyboardButton[1][];
            buttons[0] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: "🔥GANAR🔥", $"https://t.me/{pm.Replace("@", "")}") };
            return buttons;
        }

        public override StateMessage GetChatJoinMessage()
        {
            throw new NotImplementedException();
        }

        public override (StateMessage, ReplyKeyboardMarkup) GetMessageAndReplyMarkup(string status)
        {
            string code = string.Empty;
            ReplyKeyboardMarkup markUp = null;

            if (status.Contains("hi_"))
            {
                int index = 0;
                
                try
                {
                    string sindex = status.Replace("hi_", "").Replace("_in", "");
                    index = int.Parse(sindex);
                } catch (Exception ex) { }
                
                string text = hi_outs[index];

                markUp = getStartMarkup(text);
                code = status;
            }

            StateMessage msg = null;

            if (messages.ContainsKey(code))
            {
                msg = messages[code];//.Clone();                
            }
            else
            {
                var found = MessageTypes.FirstOrDefault(m => m.Code.Equals(code));
                if (found != null)
                    found.IsSet = false;

            }

            return (msg, markUp);
        }

        public override StateMessage GetMessage(string status, string? link = null, string? support_pm = null, string? pm = null, string? uuid = null, string? channel = null, bool? isnegative = false, string? training = null, string? vip = null, string? help = null)
        {
            string code = string.Empty;
            InlineKeyboardMarkup markUp = null;
            StateMessage msg = null;

            code = status;

            switch (status)
            {
                case "hi_out":
                    markUp = getHiOutMarkup(pm);
                    break;

                default:
                    break;
            }

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

        public override StateMessage GetMessage(TGBotFollowersStatApi.tgFollowerStatusResponse? resp, string? link = null, string? support_pm = null, string? pm = null, string? channel = null, bool? isnegative = false, string? training = null, string? vip = null, string? help = null)
        {
            throw new NotImplementedException();
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

        public override StateMessage GetPush(TGBotFollowersStatApi.tgFollowerStatusResponse? resp, string? code, string? link = null, string? support_pm = null, string? pm = null, string? channel = null, bool? isnegative = false, string? vip = null, string? help = null)
        {
            throw new NotImplementedException();
        }
    }
}
