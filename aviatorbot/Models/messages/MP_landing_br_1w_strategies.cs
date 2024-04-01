using asknvl.logger;
using asknvl.messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using static asknvl.server.TGBotFollowersStatApi;
using Telegram.Bot.Types.ReplyMarkups;
using DynamicData;
using System.Threading.Channels;
using System.Collections.ObjectModel;
using botservice.ViewModels;
using Telegram.Bot.Types;

namespace botservice.Models.messages
{
    public class MP_landing_br_1w_strategies : MessageProcessorBase
    {

        public override ObservableCollection<messageControlVM> MessageTypes { get; }

        public MP_landing_br_1w_strategies(string geotag, string token, ITelegramBotClient bot) : base(geotag, token, bot)
        {

            MessageTypes = new ObservableCollection<messageControlVM>() {

                new messageControlVM(this)
                {
                    Code = "start",
                    Description = "Первое сообщение"
                },
                new messageControlVM(this)
                {
                    Code = "video",
                    Description = "Видео сообщение"
                },
                new messageControlVM(this)
                {
                    Code = "before",
                    Description = "Трогательное сообщение"
                },
                new messageControlVM(this)
                {
                    Code = "reg",
                    Description = "Регистрация"
                },               
                new messageControlVM(this)
                {
                    Code = "reg_fail",
                    Description = "Регистрация не завершена"
                },
                new messageControlVM(this)
                {
                    Code = "fd",
                    Description = "ФД"
                },
                new messageControlVM(this)
                {
                    Code = "fd_fail",
                    Description = "нет ФД"
                },
                new messageControlVM(this)
                {
                    Code = "push_sum",
                    Description = "Неполная сумма"
                },               
                new messageControlVM(this)
                {
                    Code = "pm_access",
                    Description = "Доступ в личку перса"
                }                
            };

            for (int i = 1; i <= 10; i++)
            {
                var mcv = new messageControlVM(this)
                {
                    Code = $"WREG{i}",
                    Description = $"Пуш рег {i}"
                };

                MessageTypes.Add(mcv);
            }

            for (int i = 1; i <= 10; i++)
            {
                var mcv = new messageControlVM(this)
                {
                    Code = $"WFDEP{i}",
                    Description = $"Пуш деп {i}"
                };

                MessageTypes.Add(mcv);
            }

            for (int i = 1; i <= 10; i++)
            {
                var mcv = new messageControlVM(this)
                {
                    Code = $"WREDEP{i}",
                    Description = $"Пуш редеп {i}"
                };

                MessageTypes.Add(mcv);
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

        protected virtual InlineKeyboardMarkup getSubscribeMarkup(string channel)
        {
            InlineKeyboardButton[][] buttons = new InlineKeyboardButton[1][];
            buttons[0] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: "✅SUBSCRIBE✅", $"{channel}") };
            return buttons;
        }

        protected virtual InlineKeyboardMarkup getRegMarkup(string link, string uuid)
        {
            InlineKeyboardButton[][] buttons = new InlineKeyboardButton[2][];
            buttons[0] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: "🔥REGISTER", getRegUrl(link, uuid)) };
            buttons[1] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: "⚠️CHECK REGISTRATION", callbackData: "check_register") };            
            return buttons;
        }

        protected virtual InlineKeyboardMarkup getFDMarkup(string link, string uuid)
        {
            InlineKeyboardButton[][] dep_buttons = new InlineKeyboardButton[2][];
            dep_buttons[0] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: "💰DEPOSIT", getFDUrl(link, uuid)) };
            dep_buttons[1] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: "⚠️CHECK DEPOSIT", callbackData: $"check_fd") };         
            return dep_buttons;
        }

        virtual protected InlineKeyboardMarkup getBeforeMarkup(string pm)
        {
            InlineKeyboardButton[][] buttons = new InlineKeyboardButton[1][];
            buttons[0] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: "📩MESSAGES📩", $"https://t.me/{pm.Replace("@", "")}") };            
            return buttons;
        }

        virtual protected InlineKeyboardMarkup getActivatedMarkup(string pm, string link)
        {
            InlineKeyboardButton[][] buttons = new InlineKeyboardButton[2][];
            buttons[0] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: "📩MESSAGES📩", $"https://t.me/{pm.Replace("@", "")}") };
            buttons[1] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: "🚀PLAY🚀", getGameUrl(link)) };
            return buttons;
        }

        virtual protected InlineKeyboardMarkup getRegPushMarkup(string? link, string uuid)
        {
            InlineKeyboardButton[][] buttons = new InlineKeyboardButton[2][];
            buttons[0] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: "🔥REGISTER", getRegUrl(link, uuid)) };
            buttons[1] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: "⚠️CHECK REGISTRATION", callbackData: "check_register") };
            return buttons;
        }

        virtual protected InlineKeyboardMarkup getFdPushMarkup(string? link, string uuid)
        {
            InlineKeyboardButton[][] buttons = new InlineKeyboardButton[2][];
            buttons[0] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: "💰DEPOSIT", getFDUrl(link, uuid)) };
            buttons[1] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: "⚠️CHECK DEPOSIT", callbackData: $"check_fd") };            
            return buttons;
        }

        virtual protected InlineKeyboardMarkup getRdPushMarkup(string? link, string pm)
        {
            InlineKeyboardButton[][] dep_buttons = new InlineKeyboardButton[2][];
            dep_buttons[0] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: "💰EARN 💰", getGameUrl(link)) };
            dep_buttons[1] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: "📩TEXT ME", $"https://t.me/{pm.Replace("@", "")}") };
            return dep_buttons;
        }



        StateMessage getMessage(string status, string uuid, string? link, string? pm, string? channel, bool? isnegative, int? paid_sum = null, int? add_pay_sum = null)
        {
            
            InlineKeyboardMarkup markUp = null;
            string code = string.Empty;

            switch (status)
            {
                case "start":
                    markUp = getSubscribeMarkup(channel);
                    code = "start";
                    break;

                case "video":
                    //markUp = getSubscribeMarkup(channel);
                    code = "video";
                    break;

                case "before":
                    markUp = getBeforeMarkup(pm);
                    code = "before";
                    break;

                case "reg":
                case "WREG":
                    markUp = getRegMarkup(link, uuid);
                    code = (isnegative == true) ? "reg_fail" : "reg";
                    break;

                case "WFDEP":
                    if (paid_sum != null && paid_sum > 0)
                        code = "push_sum";
                    else
                        code = (isnegative == true) ? "fd_fail" : "fd";

                    markUp = getFDMarkup(link, uuid);
                    break;

                case "WREDEP1":
                    code = "pm_access";
                    markUp = getActivatedMarkup(pm, link);
                    break;

                default:
                    break;
            }

            if (status.Contains("WREDEP"))
            {
                code = "pm_access";
                markUp = getActivatedMarkup(pm, link);
            }

            StateMessage msg = null;

            if (messages.ContainsKey(code))
            {
                msg = messages[code];//.Clone();

                if (code.Equals("push_sum") && add_pay_sum != null)
                {
                    List<AutoChange> autoChange = new List<AutoChange>()
                    {
                        new AutoChange() {
                            OldText = "_sum_",
                            NewText = $"{add_pay_sum}"
                        }
                    };

                    var _msg = msg.Clone();
                    _msg.MakeAutochange(autoChange);
                    _msg.Message.ReplyMarkup = markUp;
                    return _msg;
                }

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

        public override StateMessage GetMessage(string status, string? link = null, string? support_pm = null, string? pm = null, string? uuid = null, string? channel = null, bool? isnegative = false, string? training = null, string? vip = null, string? help = null)
        {
            return getMessage(status, uuid, link: link, pm: pm, channel: channel, isnegative: isnegative);
        }

        public override StateMessage GetMessage(tgFollowerStatusResponse? resp, string? link = null, string? support_pm = null, string? pm = null, string? channel = null, bool? isnegative = false, string? training = null, string? vip = null, string? help = null)
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
                    markup = getRegPushMarkup(link, uuid);
                }
                else
                    if (code.Contains("WFDEP"))
                {
                    markup = getFdPushMarkup(link, uuid);
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

        public override StateMessage GetChatJoinMessage()
        {
            throw new NotImplementedException();
        }

    }
}
