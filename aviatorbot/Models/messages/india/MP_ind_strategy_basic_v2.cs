using asknvl.messaging;
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
using static asknvl.server.TGBotFollowersStatApi;

namespace aviatorbot.Models.messages.latam
{
    public class MP_ind_strategy_basic_v2 : MessageProcessorBase
    {        
        override public int start_push_number { get; set; } = 7;
        override public string[] hi_outs { get; set; } = {
            "START✅",
            "START🚀",
            "YES👋",
            "WRITEME🙏",
            "MONEY💰",
            "👉START👈",
            "BERICH😎"
        };
     

        public override ObservableCollection<messageControlVM> MessageTypes { get; }

        public MP_ind_strategy_basic_v2(string geotag, string token, ITelegramBotClient bot) : base(geotag, token, bot)
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
                Code = $"reg",
                Description = $"Рега"
            });

            MessageTypes.Add(new messageControlVM(this) {
                Code = "reg_fail",
                Description = "Регистрация не завершена"
            });

            MessageTypes.Add(new messageControlVM(this)
            {
                Code = "fd",
                Description = "ФД"
            });

            MessageTypes.Add(new messageControlVM(this)
            {
                Code = "fd_fail",
                Description = "нет ФД"
            });

            MessageTypes.Add(new messageControlVM(this)
            {
                Code = "fd_ok",
                Description = "ФД OK"
            });

            MessageTypes.Add(new messageControlVM(this)
            {
                Code = "BYE",
                Description = "Пощальное"
            });

            for (int i = 1; i <= 8; i++)
            {
                MessageTypes.Add(new messageControlVM(this)
                {
                    Code = $"WREG{i}",
                    Description = $"Пуш WREG{i}"
                });
            }

            for (int i = 1; i <= 8; i++)
            {
                MessageTypes.Add(new messageControlVM(this)
                {
                    Code = $"WFDEP{i}",
                    Description = $"Пуш WFDEP{i}"
                });
            }

            for (int i = 1; i <= 56; i++)
            {
                MessageTypes.Add(new messageControlVM(this)
                {
                    Code = $"WREDEP{i}",
                    Description = $"Пуш WREDEP{i}"
                });
            }

        }

        protected virtual string getRegUrl(string link, string uuid)
        {
            return _1wLinkGenerator.getRegUrl(link, uuid);
        }

        virtual protected string getFDUrl(string link, string uuid)
        {
            return _1wLinkGenerator.getFDUrl(link, uuid);
        }

        virtual protected string getGameUrl(string link)
        {
            return _1wLinkGenerator.getGameUrl(link);
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

        virtual protected InlineKeyboardMarkup getHiOutMarkup()
        {
            InlineKeyboardButton[][] buttons = new InlineKeyboardButton[1][];
            buttons[0] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: "REGISTRATION 🚀", callbackData: "reg") };
            return buttons;
        }

        protected virtual InlineKeyboardMarkup getRegMarkup(string link, string uuid, string pm)
        {
            InlineKeyboardButton[][] buttons = new InlineKeyboardButton[3][];
            buttons[0] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: "REGISTRATION 🚀", getRegUrl(link, uuid)) };
            buttons[1] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: "CHECK REGISTRATION🔍", callbackData: "check_register") };
            buttons[2] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: "HELP🤝", $"https://t.me/{pm.Replace("@", "")}") };
            return buttons;
        }

        protected virtual InlineKeyboardMarkup getFDMarkup(string link, string uuid, string pm)
        {
            InlineKeyboardButton[][] buttons = new InlineKeyboardButton[3][];
            buttons[0] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: "BALANCE💸", getFDUrl(link, uuid)) };
            buttons[1] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: "CHECK DEPOSIT🔍", callbackData: $"check_fd") };
            buttons[2] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: "HELP🤝", $"https://t.me/{pm.Replace("@", "")}") };
            return buttons;
        }

        virtual protected InlineKeyboardMarkup getRegPushMarkup(string? link, string uuid, string pm)
        {
            InlineKeyboardButton[][] buttons = new InlineKeyboardButton[3][];
            buttons[0] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: "REGISTRATION 🚀", getRegUrl(link, uuid)) };
            buttons[1] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: "CHECK REGISTRATION🔍", callbackData: "check_register") };
            buttons[2] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: "HELP🤝", $"https://t.me/{pm.Replace("@", "")}") };
            return buttons;
        }

        virtual protected InlineKeyboardMarkup getFdPushMarkup(string? link, string uuid, string pm)
        {
            InlineKeyboardButton[][] buttons = new InlineKeyboardButton[3][];
            buttons[0] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: "BALANCE💸", getFDUrl(link, uuid)) };
            buttons[1] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: "⚠️CHECK DEPOSIT", callbackData: $"check_fd") };
            buttons[2] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: "HELP🤝", $"https://t.me/{pm.Replace("@", "")}") };
            return buttons;
        }

        virtual protected InlineKeyboardMarkup getRdPushMarkup(string? link, string pm)
        {
            InlineKeyboardButton[][] dep_buttons = new InlineKeyboardButton[2][];
            dep_buttons[0] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: "PLAY🚀", getGameUrl(link)) };
            dep_buttons[1] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: "TEXT ME💲", $"https://t.me/{pm.Replace("@", "")}") };
            return dep_buttons;
        }

        virtual protected InlineKeyboardMarkup getByePushMarkup(string? link, string uuid, string pm)
        {
            InlineKeyboardButton[][] buttons = new InlineKeyboardButton[2][];
            buttons[0] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: "Registration🚀", getRegUrl(link, uuid)) };            
            buttons[1] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: "TEXT ME💸", $"https://t.me/{pm.Replace("@", "")}") };
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

        StateMessage getMessage(string status, string uuid, string? link, string? pm, string? channel, bool? isnegative, int? paid_sum = null, int? add_pay_sum = null)
        {

            InlineKeyboardMarkup markUp = null;
            string code = string.Empty;

            switch (status)
            {

                case "hi_out":
                    code = "hi_out";
                    markUp = getHiOutMarkup();
                    break;

                case "reg":
                case "WREG":
                    markUp = getRegMarkup(link, uuid, pm);
                    code = (isnegative == true) ? "reg_fail" : "reg";
                    break;

                case "WFDEP":
                    code = (isnegative == true) ? "fd_fail" : "fd";
                    markUp = getFDMarkup(link, uuid, pm);
                    break;

                case "WREDEP1":
                    code = "fd_ok";                    
                    break;

                case "BYE":
                    code = "BYE";
                    markUp = getByePushMarkup(link, uuid, pm);
                    break;

                default:
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

        public override StateMessage GetMessage(string status, string? link = null, string? support_pm = null, string? pm = null, string? uuid = null, string? channel = null, bool? isnegative = false, string? training = null, string? vip = null, string? help = null, string? param1 = null)
        {
            return getMessage(status, uuid, link: link, pm: pm, channel: channel, isnegative: isnegative);
        }

        public override StateMessage GetMessage(TGBotFollowersStatApi.tgFollowerStatusResponse? resp, string? link = null, string? support_pm = null, string? pm = null, string? channel = null, bool? isnegative = false, string? training = null, string? vip = null, string? help = null)
        {
            var uuid = resp.uuid;
            int paid_sum = (int)resp.amount_local_currency;
            int add_pay_sum = (int)resp.target_amount_local_currency;
            var status = resp.status_code;

            return getMessage(status, uuid, link: link, pm: pm, channel: channel, isnegative: isnegative, paid_sum: paid_sum, add_pay_sum: add_pay_sum);
        }

        public override StateMessage GetPush(string? code, string? link = null, string? pm = null, string? uuid = null, string? channel = null, bool? isnegative = false)
        {
            throw new NotImplementedException();
        }

        public override StateMessage GetPush(tgFollowerStatusResponse? resp,
                                              string? code,
                                              string? link = null,
                                              string? support_pm = null,
                                              string? pm = null,
                                              string? channel = null,
                                              bool? isnegative = false,
                                              string? vip = null,
                                              string? help = null)
        {
            StateMessage push = null;
            var start_params = resp.start_params;
            var uuid = resp.uuid;

            var found = messages.ContainsKey(code);
            if (found)
            {
                InlineKeyboardMarkup markup = null;

                if (code.Contains("WREG"))
                {
                    markup = getRegPushMarkup(link, uuid, pm);
                }
                else
                    if (code.Contains("WFDEP"))
                {
                    markup = getFdPushMarkup(link, uuid, pm);
                }
                else
                    if (code.Contains("WREDEP"))
                {
                    markup = getRdPushMarkup(link, pm);
                }

                push = messages[code].Clone();
                push.Message.ReplyMarkup = markup;
            }
            return push;
        }
    }
}
