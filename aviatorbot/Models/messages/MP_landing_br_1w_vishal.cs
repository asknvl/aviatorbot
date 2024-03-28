using asknvl.logger;
using asknvl.messaging;
using asknvl.server;
using botservice.Models.param_decoder;
using botservice.ViewModels;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using static asknvl.server.TGBotFollowersStatApi;

namespace botservice.Models.messages
{
    public class MP_landing_br_1w_vishal : MessageProcessorBase
    {

        #region vars
        ILogger logger;
        string payment_address;
        #endregion

        #region properties
        public override ObservableCollection<messageControlVM> MessageTypes { get; }
        #endregion
        public MP_landing_br_1w_vishal(string geotag, string token, ITelegramBotClient bot, ILogger logger) : base(geotag, token, bot)
        {
            payment_address = "https://xirfloven.space";

            this.logger = logger;

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
                    Code = "tarrifs",
                    Description = "Тарифы"
                },
                new messageControlVM(this)
                {
                    Code = "reg",
                    Description = "Регистрация"
                },
                new messageControlVM(this)
                {
                    Code = "reg_push",
                    Description = "Регистрация пуш"
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
                    Code = "activated",
                    Description = "Бот активирован"
                },
                new messageControlVM(this)
                {
                    Code = "pm_access",
                    Description = "Доступ в личку перса"
                },
                new messageControlVM(this)
                {
                    Code = "rd1_ok",
                    Description = "РД2 Тренинг"
                },
                new messageControlVM(this)
                {
                    Code = "rd4_ok_1",
                    Description = "РД4 VIP TRAIN"
                },
                new messageControlVM(this)
                {
                    Code = "rd4_ok_2",
                    Description = "РД4 VIP"
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

            for (int i = 1; i <= 11; i++)
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

        virtual protected InlineKeyboardMarkup getSubscribeMarkup(string landing_channel)
        {
            InlineKeyboardButton[][] buttons = new InlineKeyboardButton[1][];
            buttons[0] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: "✅FREE CHANNEL✅", $"{landing_channel}") };
            return buttons;
        }
        virtual protected InlineKeyboardMarkup getTarrifsMarkup()
        {
            InlineKeyboardButton[][] buttons = new InlineKeyboardButton[3][];
            buttons[0] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: "₹80,000(permanent access)", $"{payment_address}?sum=80000") };
            buttons[1] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: "₹15,000(access for a month)", $"{payment_address}?sum=15000") };
            buttons[2] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: "✅3 DAYS FREE✅", callbackData: "reg") };
            return buttons;
        }
        virtual protected InlineKeyboardMarkup getRegMarkup(string? link, string uuid, string help)
        {
            InlineKeyboardButton[][] reg_buttons = new InlineKeyboardButton[3][];
            reg_buttons[0] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: "📲REGISTER", getRegUrl(link, uuid)) };
            reg_buttons[1] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: "🔎VERIFY REGISTRATION", callbackData: "check_register") };
            reg_buttons[2] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: "🧑‍💻HELP🧑‍💻", $"{help}") };

            return reg_buttons;
        }

        virtual protected InlineKeyboardMarkup getFDMarkup(string? link, string uuid, string help)
        {
            InlineKeyboardButton[][] dep_buttons = new InlineKeyboardButton[3][];
            dep_buttons[0] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: "💳TOP UP BALANCE💳", getFDUrl(link, uuid)) };
            dep_buttons[1] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: "🔎VERIFY DEPOSIT", callbackData: $"check_fd") };
            dep_buttons[2] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: "🧑‍💻HELP🧑‍💻", $"{help}") };
            return dep_buttons;
        }

        virtual protected InlineKeyboardMarkup getActivatedMarkup()
        {
            InlineKeyboardButton[][] buttons = new InlineKeyboardButton[1][];
            buttons[0] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: "🚀START🚀", callbackData: $"pm_access") };
            return buttons;
        }

        virtual protected InlineKeyboardMarkup getTrainingMarkup(string training)
        {
            InlineKeyboardButton[][] buttons = new InlineKeyboardButton[1][];
            buttons[0] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: "💪 TRAINING CHANNEL", training) };
            return buttons;
        }

        virtual protected InlineKeyboardMarkup getVipTrainMarkup(string vip, string training, string pm)
        {
            InlineKeyboardButton[][] buttons = new InlineKeyboardButton[3][];
            buttons[0] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: "🚨Amir | VIP SIGNALS", vip) };
            buttons[1] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: "📚Amir | TRAINING ", training) };
            buttons[2] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: "📩My contact", $"https://t.me/{pm.Replace("@", "")}") };
            return buttons;
        }

        virtual protected InlineKeyboardMarkup getVipMarkup(string vip)
        {
            InlineKeyboardButton[][] buttons = new InlineKeyboardButton[1][];
            buttons[0] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: "💰VIP CHANNEL💰", vip) };

            return buttons;
        }

        virtual protected InlineKeyboardMarkup getFirstPmMarkup(string pm)
        {
            InlineKeyboardButton[][] buttons = new InlineKeyboardButton[2][];
            buttons[0] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: "💪I am registered💪", $"https://t.me/{pm.Replace("@", "")}") };
            buttons[1] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: "✉️MESSAGE✉️", $"https://t.me/{pm.Replace("@", "")}") };
            return buttons;
        }

        virtual protected InlineKeyboardMarkup getSecondPmMarkup(string pm)
        {
            InlineKeyboardButton[][] buttons = new InlineKeyboardButton[1][];
            buttons[0] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: "✉️Write me to activate✉️", $"https://t.me/{pm.Replace("@", "")}") };
            return buttons;
        }

        virtual protected InlineKeyboardMarkup getRegPushMarkup(string? link, string support_pm, string uuid)
        {
            InlineKeyboardButton[][] reg_buttons = new InlineKeyboardButton[3][];
            reg_buttons[0] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: "📲REGISTER", getRegUrl(link, uuid)) };
            reg_buttons[1] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: "🔎VERIFY REGISTRATION", callbackData: "check_register") };
            reg_buttons[2] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: "🧑‍💻HELP🧑‍💻", $"https://t.me/{support_pm.Replace("@", "")}") };

            return reg_buttons;
        }

        virtual protected InlineKeyboardMarkup getFdPushMarkup(string? link, string support_pm, string uuid)
        {
            InlineKeyboardButton[][] dep_buttons = new InlineKeyboardButton[3][];
            dep_buttons[0] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: "💳TOP UP BALANCE💳", getFDUrl(link, uuid)) };
            dep_buttons[1] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: "🔎VERIFY DEPOSIT", callbackData: $"check_fd") };
            dep_buttons[2] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: "🧑‍💻HELP🧑‍💻", $"https://t.me/{support_pm.Replace("@", "")}") };
            return dep_buttons;
        }

        virtual protected InlineKeyboardMarkup getRdPushMarkup(string? link, string pm, string uuid)
        {
            InlineKeyboardButton[][] dep_buttons = new InlineKeyboardButton[2][];
            dep_buttons[0] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: "💸TOP UP THE BALANCE💸", getFDUrl(link, uuid)) };
            dep_buttons[1] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: "🆘 HELP", $"https://t.me/{pm.Replace("@", "")}") };
            return dep_buttons;
        }

        public override StateMessage GetChatJoinMessage()
        {
            throw new NotImplementedException();
        }

        public override StateMessage GetMessage(tgFollowerStatusResponse? resp,
                                                string? link = null,
                                                string? support_pm = null,
                                                string? pm = null,
                                                string? channel = null,
                                                bool? isnegative = false,
                                                string? training = null,
                                                string? vip = null,
                                                string? help = null)
        {
            string code = string.Empty;
            InlineKeyboardMarkup markUp = null;

            var uuid = resp.uuid;
            int paid_sum = (int)resp.amount_local_currency;
            int add_pay_sum = (int)resp.target_amount_local_currency;
            string start_params = resp.start_params;

            switch (resp.status_code)
            {
                case "start":
                    markUp = getSubscribeMarkup(channel);
                    code = "start";
                    break;

                case "video":
                    ///startmarkUp = getSubscribeMarkup(channel);
                    code = "video";
                    break;

                case "tarrifs":
                    markUp = getTarrifsMarkup();
                    code = "tarrifs";
                    break;

                case "WREG":
                    markUp = getRegMarkup(link, uuid, help);
                    code = (isnegative == true) ? "reg_fail" : "reg";
                    break;

                case "WFDEP":
                    if (paid_sum > 0)
                        code = "push_sum";
                    else
                        code = (isnegative == true) ? "fd_fail" : "fd";

                    markUp = getFDMarkup(link, uuid, help);
                    break;

                case "WREDEP1":
                    code = "activated";
                    markUp = getActivatedMarkup();
                    break;

                case "pm_access":
                    code = "pm_access";
                    markUp = getFirstPmMarkup(pm);
                    break;

                case "pm_notify":
                    code = "pm_notify";
                    markUp = getSecondPmMarkup(pm);
                    break;

                case "rd1_ok":
                    code = "rd1_ok";
                    markUp = getTrainingMarkup(training);
                    break;

                case "rd4_ok_1":
                    code = "rd4_ok_1";
                    markUp = getVipTrainMarkup(vip, training, pm);
                    break;

                case "rd4_ok_2":
                    code = "rd4_ok_2";
                    markUp = getVipMarkup(vip);
                    break;

                default:
                    break;
            }

            StateMessage msg = null;

            if (messages.ContainsKey(code))
            {
                msg = messages[code];//.Clone();

                if (code.Equals("push_sum"))
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
        public override StateMessage GetMessage(string status,
                                                string? link = null,
                                                string? support_pm = null,
                                                string? pm = null,
                                                string? uuid = null,
                                                string? channel = null,
                                                bool? isnegative = false,
                                                string? training = null,
                                                string? vip = null,
                                                string? help = null)
        {
            string code = string.Empty;
            InlineKeyboardMarkup markUp = null;

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

                case "tarrifs":
                    markUp = getTarrifsMarkup();
                    code = "tarrifs";
                    break;

                case "WREG":
                    markUp = getRegMarkup(link, help, uuid);
                    code = (isnegative == true) ? "reg_fail" : "reg";
                    break;

                case "WFDEP":
                    code = (isnegative == true) ? "fd_fail" : "fd";
                    markUp = getFDMarkup(link, uuid, help);
                    break;

                case "WREDEP1":
                    code = "activated";
                    markUp = getActivatedMarkup();
                    break;

                case "pm_access":
                    code = "pm_access";
                    markUp = getFirstPmMarkup(pm);
                    break;

                case "pm_notify":
                    code = "pm_notify";
                    markUp = getSecondPmMarkup(pm);
                    break;

                case "rd1_ok":
                    code = "rd1_ok";
                    markUp = getTrainingMarkup(training);
                    break;

                case "rd4_ok_1":
                    code = "rd4_ok_1";
                    markUp = getVipTrainMarkup(vip, training, pm);
                    break;

                case "rd4_ok_2":
                    code = "rd4_ok_2";
                    markUp = getVipMarkup(vip);
                    break;

                default:
                    break;
            }

            if (status.Contains("WREDEP"))
            {
                code = "activated";
                markUp = getActivatedMarkup();
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


                //if (code.Equals("WFDEP11") || code.Equals("WREDEP10"))
                //{
                //    markup = getVipMarkup(vip);
                //} else
                //{
                //    if (code.Contains("WREG"))
                //    {
                //        markup = getRegPushMarkup(link, support_pm, uuid);
                //    }
                //    else
                //    if (code.Contains("WFDEP"))
                //    {
                //        markup = getFdPushMarkup(link, support_pm, uuid);
                //    }
                //    else
                //    if (code.Contains("WREDEP"))
                //    {
                //        markup = getRdPushMarkup(link, pm, uuid);
                //    }
                //}

                if (code.Contains("WREG"))
                {
                    markup = getRegPushMarkup(link, help, uuid);
                }
                else
                if (code.Contains("WFDEP"))
                {
                    markup = getFdPushMarkup(link, help, uuid);
                }
                else
                if (code.Contains("WREDEP"))
                {
                    markup = getRdPushMarkup(link, pm, uuid);
                }


                push = messages[code].Clone();
                push.Message.ReplyMarkup = markup;
            }
            return push;
        }
        public override StateMessage GetPush(string? code, string? link = null, string? pm = null, string? uuid = null, string? channel = null, bool? isnegative = false)
        {
            throw new NotImplementedException();
        }

    }
}
