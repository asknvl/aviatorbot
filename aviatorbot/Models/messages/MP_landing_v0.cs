﻿using asknvl.logger;
using asknvl.messaging;
using asknvl.server;
using aviatorbot.Models.param_decoder;
using aviatorbot.ViewModels;
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

namespace aviatorbot.Models.messages
{
    public class MP_landing_v0 : MessageProcessorBase
    {

        #region vars
        ILogger logger;

        string payment_address;
        string strategy_channel;
        string vip_channel;
        string trainig_channel;
        #endregion

        #region properties
        public override ObservableCollection<messageControlVM> MessageTypes { get; }
        #endregion
        public MP_landing_v0(string geotag, string token, ITelegramBotClient bot, ILogger logger,
                string payment_address,
                string strategy_channel,
                string vip_channel,
                string trainig_channel) : base(geotag, token, bot)
        {
            this.logger = logger;

            this.payment_address = payment_address;
            this.strategy_channel = strategy_channel;
            this.vip_channel = vip_channel;
            this.trainig_channel = trainig_channel;

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

        }

        string getRegUrl(string link, string uuid)
        {            
            var res = $"{link}/casino/list?open=register&sub1={uuid}";
            if (string.IsNullOrEmpty(uuid) || uuid.Length != 10)
                logger.err("getRegUrl", res);
            else
                logger.dbg("getRegUrl", res);

            return res;
        }

        string getFDUrl(string link, string uuid)
        {            
            var res = $"{link}/casino/list?open=deposit&sub1={uuid}";
            if (string.IsNullOrEmpty(uuid) || uuid.Length != 10)
                logger.err("getFDUrl", res);
            else
                logger.dbg("getFDUrl", res);
            return res;
        }


        
        virtual protected InlineKeyboardMarkup getSubscribeMarkup(string landing_channel)
        {
            InlineKeyboardButton[][] buttons = new InlineKeyboardButton[1][];            
            buttons[0] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: "✅SUBSCRIBE✅", $"{landing_channel}") };
            return buttons;
        }
        virtual protected InlineKeyboardMarkup getTarrifsMarkup()
        {
            InlineKeyboardButton[][] buttons = new InlineKeyboardButton[3][];
            buttons[0] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: "₹80,000(permanent access)", $"{payment_address}") };
            buttons[1] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: "₹15,000(access for a month)", $"{payment_address}") };
            buttons[2] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: "✅3 DAYS FREE✅", callbackData: "reg") };
            return buttons;
        }
        virtual protected InlineKeyboardMarkup getRegMarkup(string? link, string support_pm, string uuid)
        {
            InlineKeyboardButton[][] reg_buttons = new InlineKeyboardButton[3][];
            reg_buttons[0] = new InlineKeyboardButton[] { InlineKeyboardButton.WithWebApp(text: "🔥REGISTER", new WebAppInfo() { Url = getRegUrl(link, uuid) }) };
            reg_buttons[1] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: "💸 Verify REGISTRATION", callbackData: "check_register") };
            reg_buttons[2] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: "🆘 HELP", $"https://t.me/{support_pm.Replace("@", "")}") };

            return reg_buttons;
        }

        virtual protected InlineKeyboardMarkup getFDMarkup(string? link, string support_pm, string uuid)
        {
            InlineKeyboardButton[][] dep_buttons = new InlineKeyboardButton[3][];
            dep_buttons[0] = new InlineKeyboardButton[] { InlineKeyboardButton.WithWebApp(text: "💸TOP UP THE BALANCE💸", new WebAppInfo() { Url = getFDUrl(link, uuid) }) };
            dep_buttons[1] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: "🕒CHECK  BALANCE🕒", callbackData: $"check_fd") };
            dep_buttons[2] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: "🆘 HELP", $"https://t.me/{support_pm.Replace("@", "")}") };
            return dep_buttons;
        }

        virtual protected InlineKeyboardMarkup getActivatedMarkup()
        {
            InlineKeyboardButton[][] buttons = new InlineKeyboardButton[1][];
            buttons[0] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: "✅START✅", callbackData: $"pm_access") };
            return buttons;
        }

        virtual protected InlineKeyboardMarkup getPmMarkup(string pm)
        {
            InlineKeyboardButton[][] buttons = new InlineKeyboardButton[1][];
            buttons[0] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: "🆘 HELP", $"https://t.me/{pm.Replace("@", "")}") };
            return buttons;
        }

        virtual protected InlineKeyboardMarkup getRegPushMarkup(string? link, string support_pm, string uuid)
        {
            InlineKeyboardButton[][] reg_buttons = new InlineKeyboardButton[3][];
            reg_buttons[0] = new InlineKeyboardButton[] { InlineKeyboardButton.WithWebApp(text: "🔥REGISTER", new WebAppInfo() { Url = getRegUrl(link, uuid) }) };
            reg_buttons[1] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: "💸 Verify REGISTRATION", callbackData: "check_register") };
            reg_buttons[2] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: "🆘 HELP", $"https://t.me/{support_pm.Replace("@", "")}") };

            return reg_buttons;
        }

        virtual protected InlineKeyboardMarkup getFdPushMarkup(string? link, string support_pm, string uuid)
        {
            InlineKeyboardButton[][] dep_buttons = new InlineKeyboardButton[3][];
            dep_buttons[0] = new InlineKeyboardButton[] { InlineKeyboardButton.WithWebApp(text: "💸TOP UP THE BALANCE💸", new WebAppInfo() { Url = getFDUrl(link, uuid) }) };
            dep_buttons[1] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: "🕒CHECK  BALANCE🕒", callbackData: $"check_fd") };
            dep_buttons[2] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: "🆘 HELP", $"https://t.me/{support_pm.Replace("@", "")}") };
            return dep_buttons;
        }

        public override StateMessage GetChatJoinMessage()
        {
            throw new NotImplementedException();
        }
        public override StateMessage GetMessage(tgFollowerStatusResponse? resp, string? link = null, string? support_pm = null, string? pm = null, string? channel = null, bool? isnegative = false)
        {
            string code = string.Empty;
            InlineKeyboardMarkup markUp = null;

            var uuid = resp.uuid;
            int paid_sum = (int)resp.amount_local_currency;
            int add_pay_sum = (int)resp.target_amount_local_currency;
            string start_params = resp.start_params;

            //status = statusResponce.status_code;
            //uuid = statusResponce.uuid;
            //paid_sum = (int)statusResponce.amount_local_currency;
            //add_pay_sum = (int)statusResponce.target_amount_local_currency;
            //start_params = statusResponce.start_params;
            //player_id = statusResponce.player_id;


            switch (resp.status_code)
            {
                case "start":
                    markUp = getSubscribeMarkup(channel);
                    code = "start";
                    break;

                case "video":
                    markUp = getSubscribeMarkup(channel);
                    code = "video";
                    break;

                case "tarrifs":
                    markUp = getTarrifsMarkup();
                    code = "tarrifs";
                    break;

                case "WREG":
                    markUp = getRegMarkup(link, support_pm, uuid);
                    code = (isnegative == true) ? "reg_fail" : "reg";
                    break;

                case "WFDEP":
                    if (paid_sum > 0)
                        code = "push_sum";
                    else
                        code = (isnegative == true) ? "fd_fail" : "fd";

                    markUp = getFDMarkup(link, support_pm, uuid);
                    break;

                case "WREDEP1":
                    code = "activated";
                    markUp = getActivatedMarkup();
                    break;

                case "pm_access":
                    code = "pm_access";
                    markUp = getPmMarkup(pm);
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
        public override StateMessage GetMessage(string status, string? link = null, string? support_pm = null, string? pm = null, string? uuid = null, string? channel = null, bool? isnegative = false)
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
                    markUp = getSubscribeMarkup(channel);
                    code = "video";
                    break;

                case "tarrifs":
                    markUp = getTarrifsMarkup();
                    code = "tarrifs";
                    break;

                case "WREG":
                    markUp = getRegMarkup(link, support_pm, uuid);
                    code = (isnegative == true) ? "reg_fail" : "reg";
                    break;

                case "WFDEP":
                    code = (isnegative == true) ? "fd_fail" : "fd";
                    markUp = getFDMarkup(link, support_pm, uuid);
                    break;

                case "WREDEP1":
                    code = "activated";
                    markUp = getActivatedMarkup();
                    break;

                case "pm_access":
                    code = "pm_access";
                    markUp = getPmMarkup(pm);
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

        public override StateMessage GetPush(tgFollowerStatusResponse? resp, string? code, string? link = null, string? support_pm = null, string? pm = null, string? channel = null, bool? isnegative = false)
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
                    markup = getRegPushMarkup(link, support_pm, uuid);
                } else
                    if (code.Contains("WFDEP"))
                {
                    markup = getRegPushMarkup(link, support_pm, uuid);
                } else
                    if (code.Contains("WREDEP"))
                {
                    
                } 


                //switch (code)
                //{
                //    case "PUSH_NO_WREG_3H":
                //    case "PUSH_NO_WREG_12H":
                //        markup = getRegPushMarkup(link, support_pm, uuid);
                //        break;

                //    case "PUSH_NO_WFDEP_3H":
                //    case "PUSH_NO_WFDEP_12H":
                //        markup = getFdPushMarkup(link, support_pm, uuid);
                //        break;

                //    default:
                //        break;
                //}

                push = messages[code].Clone();
                push.Message.ReplyMarkup = markup;
            }
            return push;
        }
        public override StateMessage GetPush(string? code, string? link = null, string? pm = null, string? uuid = null, string? channel = null, bool? isnegative = false)
        {
            throw new NotImplementedException();
        }

        public override StateMessage GetPush(TGBotFollowersStatApi.tgFollowerStatusResponse? resp, string? code, string? link = null, string? pm = null, string? channel = null, bool? isnegative = false)
        {
            throw new NotImplementedException();
        }

        public override StateMessage GetMessage(string status, string? link = null, string? pm = null, string? uuid = null, string? channel = null, bool? isnegative = false)
        {
            throw new NotImplementedException();
        }

        public override StateMessage GetMessage(tgFollowerStatusResponse? resp, string? link = null, string? pm = null, string? channel = null, bool? isnegative = false)
        {
            throw new NotImplementedException();
        }
    }
}