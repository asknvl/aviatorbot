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
    public class MP_latam_smrnv : MessageProcessorBase
    {
        #region const
        public int start_push_number = 8;
        public readonly string[] hi_outs = {
            "👉OBTEN S/10.000 SOLES 👈",
            "CLIC EN \"UNIRSE\"👋",
            "PULSA \"INICIO\"✅",
            "ESTOY LISTA PARA SER RICA!💰",
            "👋HAZ CLIC Y HAZTE RICA!",
            "💞VAMOS",
            "🥂GANEMOS",
            "¡TOMA EL ÚLTIMO LUGAR! 🔥"
        };
        #endregion

        public override ObservableCollection<messageControlVM> MessageTypes { get; }

        public MP_latam_smrnv(string geotag, string token, ITelegramBotClient bot) : base(geotag, token, bot)
        {
            MessageTypes = new ObservableCollection<messageControlVM>();

            for (int i = 0; i < start_push_number; i++)
            {
                MessageTypes.Add(new messageControlVM(this)
                {
                    Code = $"hi_{i}_in",
                    Description = $"HI {i + 1}"
                });

                MessageTypes.Add(new messageControlVM(this)
                {
                    Code = $"hi_{i}_out",
                    Description = $"Ответ на HI {i + 1}"
                });
            }

            MessageTypes.Add(new messageControlVM(this)
            {
                Code = "BYE",
                Description = "BYE"
            });

            for (int i = 1; i <= 30; i++)
            {
                MessageTypes.Add(new messageControlVM(this)
                {
                    Code = $"WREG{i}",
                    Description = $"Пуш {i + 1}"
                });
            }           
        }

        //public StateMessage GetMessage(string code)
        //{
        //    throw new NotImplementedException();
        //}

        //public StateMessage GetPush(string code)
        //{
        //    throw new NotImplementedException();
        //}

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

        //virtual protected InlineKeyboardMarkup getPushMarkup(string pm)
        //{
        //    InlineKeyboardButton[][] buttons = new InlineKeyboardButton[1][];
        //    buttons[0] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: getPushButtonName(), $"https://t.me/{pm.Replace("@", "")}") };
        //    return buttons;
        //}

        public override StateMessage GetChatJoinMessage()
        {
            throw new NotImplementedException();
        }

        public (StateMessage, ReplyKeyboardMarkup) GetMessageAndReplyMarkup(string status)
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
                //markup = getPushMarkup(pm);
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
