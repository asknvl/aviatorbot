using asknvl.messaging;
using asknvl.server;
using Avalonia.Styling;
using Avalonia.X11;
using botservice.Models.messages;
using botservice.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace aviatorbot.Models.messages.latam
{
    public class MP_modertator_raceup_tier1 : MessageProcessorBase
    {
        #region const
        override public int start_push_number {get; set;} = 7;
        public override Dictionary<Languages, string[]> locale_hi_outs { get; set; } = new Dictionary<Languages, string[]>()
        {
            { Languages.en, new string[] {
                "❗️ I'M NOT A BOT ❗️",
                "START🚀",
                "START👋",
                "START💸",
                "START💰",
                "👉START👈",
                "START😎"
            }}
        };

        #endregion

        #region vars
        Languages language;
        #endregion

        public override ObservableCollection<messageControlVM> MessageTypes { get; }

        public MP_modertator_raceup_tier1(string geotag, string token, ITelegramBotClient bot, Languages language) : base(geotag, token, bot)
        {
            this.language = language;

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

        virtual protected InlineKeyboardMarkup getPushMarkup(string pm, string link)
        {

            string title1 = "";
            string title2 = "";

            switch (language)
            {
                case Languages.en:
                    title1 = "TEXT ME";
                    title2 = "GAME";
                    break;
            }

            InlineKeyboardButton[][] buttons = new InlineKeyboardButton[2][];
            //buttons[0] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: "PLAY🚀", $"{link}") };
            buttons[0] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: $"{title1} ✍️", $"https://t.me/{pm.Replace("@", "")}") };
            buttons[1] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: $"{title2} 🔥", $"{link}") };
            return buttons;
        }

        virtual protected InlineKeyboardMarkup getHiOutMarkup(string pm, string link)
        {
            return getPushMarkup(pm, link);
        }

        protected virtual string getAtributedLink(string link, string? uuid, string? src)
        {

            string uuid_section = (!string.IsNullOrEmpty(uuid)) ? $"uuid={uuid.ToLower()}" : "";
            string src_section = (!string.IsNullOrEmpty(src)) ? $"src={src.ToLower()}" : "";

            if (!string.IsNullOrEmpty(uuid_section) && !string.IsNullOrEmpty(src_section))
                return $"{link}?{uuid_section}&{src_section}";
            else
                if (!string.IsNullOrEmpty(uuid_section))
                return $"{link}?{uuid_section}";
            else
                if (!string.IsNullOrEmpty(uuid_section))
                return $"{link}?{src_section}";

            return link;

        }

        public override StateMessage GetChatJoinMessage()
        {
            throw new NotImplementedException();
        }

        public override (StateMessage, ReplyKeyboardMarkup) GetMessageAndReplyMarkup(string status, string? pm = null, string? link = null, string? uuid = null)
        {
            string code = string.Empty;
            ReplyKeyboardMarkup markUp = null;

            int index = 0;

            if (status.Contains("hi_"))
            {   try
                {
                    string sindex = status.Replace("hi_", "").Replace("_in", "");
                    index = int.Parse(sindex);
                } catch (Exception ex) { }

                string text = locale_hi_outs[language][index];

                markUp = getStartMarkup(text);
                code = status;
            }

            StateMessage msg = null;

            if (messages.ContainsKey(code))
            {
                var _msg = messages[code];//.Clone();                

                List<AutoChange> autoChange = new List<AutoChange>()
                {
                    new AutoChange()
                    {
                        OldText = "https://partner.chng",
                        NewText = $"{getAtributedLink(link, uuid, $"startpush")}"
                    }
                };

                msg = _msg.Clone();
                msg.MakeAutochange(autoChange);                                

                if (index == start_push_number - 1)
                {
                    msg.Message.ReplyMarkup = getHiOutMarkup(pm, link); 
                }
            }
            else
            {
                var found = MessageTypes.FirstOrDefault(m => m.Code.Equals(code));
                if (found != null)
                    found.IsSet = false;

            }

            return (msg, markUp);
        }

        public override StateMessage GetMessage(string status, string? link = null, string? support_pm = null, string? pm = null, string? uuid = null, string? channel = null, bool? isnegative = false, string? training = null, string? vip = null, string? help = null, string? param1 = null)
        {
            string code = string.Empty;
            InlineKeyboardMarkup markUp = null;
            StateMessage msg = null;

            code = status;

            switch (status)
            {
                case "hi_out":
                    markUp = getHiOutMarkup(pm, getAtributedLink(link, uuid, status.ToLower()));
                    break;

                case "BYE":
                    markUp = getHiOutMarkup(pm, getAtributedLink(link, uuid, status.ToLower()));
                    break;

                default:
                    break;
            }

            if (messages.ContainsKey(code))
            {
                var _msg = messages[code];//.Clone();

                List<AutoChange> autoChange = new List<AutoChange>()
                {
                    new AutoChange()
                    {
                        OldText = "https://partner.chng",
                        NewText = $"{getAtributedLink(link, uuid, $"{status.ToLower()}")}"
                    },
                    new AutoChange()
                    {
                        OldText = "https://return.chng",
                        NewText = $"{channel}"
                    }
                };

                msg = _msg.Clone();
                msg.MakeAutochange(autoChange);
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
                markup = getPushMarkup(pm, getAtributedLink(link, uuid, code.ToLower()));
                push = messages[code].Clone();

                List<AutoChange> autoChange = new List<AutoChange>()
                {
                    new AutoChange()
                    {
                        OldText = "https://partner.chng",
                        NewText = $"{getAtributedLink(link, uuid, code.ToLower())}"
                    }
                };

                push.MakeAutochange(autoChange);
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
