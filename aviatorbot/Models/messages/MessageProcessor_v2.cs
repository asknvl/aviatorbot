using asknvl.messaging;
using aviatorbot.Models.param_decoder;
using aviatorbot.ViewModels;
using DynamicData;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;
using static asknvl.server.TGBotFollowersStatApi;

namespace aviatorbot.Models.messages
{
    public class MessageProcessor_v2 : MessageProcessor_v1
    {
        public override ObservableCollection<messageControlVM> MessageTypes { get; }

        public MessageProcessor_v2(string geotag, string token, ITelegramBotClient bot) : base(geotag, token, bot)
        {
            MessageTypes = new ObservableCollection<messageControlVM>() {

                new messageControlVM(this)
                {
                    Code = "video",
                    Description = "Видео сообщение"
                },

                new messageControlVM(this)
                {
                    Code = "reg",
                    Description = "Приветственное сообщение"
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
                    Code = "vip",
                    Description = "Доступ в VIP"
                },                
                new messageControlVM(this)
                {
                    Code = "join",
                    Description = "Пуш Доступ в VIP"
                },
                new messageControlVM(this)
                {
                    Code = "PUSH_NO_WREG_3H",
                    Description = "Нет регистрации 3ч"
                },
                new messageControlVM(this)
                {
                    Code = "PUSH_NO_WFDEP_3H",
                    Description = "Нет ФД 3ч"
                },
                new messageControlVM(this)
                {
                    Code = "PUSH_NO_WREDEP_3H",
                    Description = "Нет РД 3ч"
                },
                new messageControlVM(this)
                {
                    Code = "PUSH_NO_WREG_12H",
                    Description = "Нет регистрации 12ч"
                },
                new messageControlVM(this)
                {
                    Code = "PUSH_NO_WFDEP_12H",
                    Description = "Нет ФД 12ч"
                },
                new messageControlVM(this)
                {
                    Code = "PUSH_NO_WREDEP_12H",
                    Description = "Нет РД 12ч"
                }
            };
        }

        override protected InlineKeyboardMarkup getVideoMarkup(string pm)
        {
            InlineKeyboardButton[][] buttons = new InlineKeyboardButton[2][];
            buttons[0] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: "💰GET SOFTWARE ", callbackData: $"show_reg") };
            buttons[1] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: "🧑🏻‍💻MESSAGE ME", $"https://t.me/{pm.Replace("@", "")}") };
            return buttons;
        }

        virtual protected InlineKeyboardMarkup getRegMarkup(string start_param, string? link, string pm, string uuid)
        {
            var decode = StartParamDecoder.Decode(start_param);

            InlineKeyboardButton[][] reg_buttons = new InlineKeyboardButton[3][];
            //reg_buttons[0] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: "🔥REGISTER", $"{link}/casino/list?open=register&sub1={uuid}&sub2={uuid}") };
            reg_buttons[0] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: "🔥REGISTER", $"{link}/casino/list?open=register&sub1={uuid}&sub2={decode.buyer}&sub3={decode.closer}&sub4={decode.source}&sub5={decode.num}") };
            reg_buttons[1] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: "⚠️CHECK REGISTRATION", callbackData: "check_register") };
            reg_buttons[2] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: "🧑🏻‍💻MESSAGE ME", $"https://t.me/{pm.Replace("@", "")}") };

            return reg_buttons;
        }

        virtual protected InlineKeyboardMarkup getFDMarkup(string start_param, string pm, string? link, string uuid)
        {

            var decode = StartParamDecoder.Decode(start_param);

            InlineKeyboardButton[][] dep_buttons = new InlineKeyboardButton[3][];
            //dep_buttons[0] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: "💰DEPOSIT", $"{link}/casino/list?open=deposit&sub1={uuid}&sub2={uuid}") };
            dep_buttons[0] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: "💰DEPOSIT", $"{link}/casino/list?open=deposit&sub1={uuid}&sub2={decode.buyer}&sub3={decode.closer}&sub4={decode.source}&sub5={decode.num}") };
            dep_buttons[1] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: "⚠️CHECK DEPOSIT", callbackData: $"check_fd") };
            dep_buttons[2] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: "🧑🏻‍💻MESSAGE ME", $"https://t.me/{pm.Replace("@", "")}") };
            return dep_buttons;
        }

        virtual protected InlineKeyboardMarkup getVipMarkup(string start_param, string pm, string? link, string channel, string uuid)
        {

            var decode = StartParamDecoder.Decode(start_param);

            InlineKeyboardButton[][] vip_buttons = new InlineKeyboardButton[3][];
            //vip_buttons[0] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: "💰PLAY💰", $"{link}/casino/play/aviator?sub1={uuid}&sub2={uuid}") };
            vip_buttons[0] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: "💰PLAY💰", $"{link}/casino/play/aviator?&sub1={uuid}&sub2={decode.buyer}&sub3={decode.closer}&sub4={decode.source}&sub5={decode.num}") };
            vip_buttons[1] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: "🥰VIP CHANNEL 🥰", $"{channel}") };
            vip_buttons[2] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: "🔥MESSAGE ME🔥", $"https://t.me/{pm.Replace("@", "")}") };
            return vip_buttons;
        }

        virtual protected InlineKeyboardMarkup getVipPushMarkup(string pm, string channel)
        {
            InlineKeyboardButton[][] vip_buttons = new InlineKeyboardButton[1][];            
            vip_buttons[0] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: "🥰VIP CHANNEL 🥰", $"{channel}") };            
            return vip_buttons;
        }

        virtual protected InlineKeyboardMarkup getRegPushMarkup(string start_param, string? link, string pm, string uuid)
        {
            var decode = StartParamDecoder.Decode(start_param);

            var buttons = new InlineKeyboardButton[3][];
            buttons[0] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: "🔥REGISTER", $"{link}/casino/list?open=register&sub1={uuid}&sub2={decode.buyer}&sub3={decode.closer}&sub4={decode.source}&sub5={decode.num}") };
            buttons[1] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: "⚠️CHECK REGISTRATION", callbackData: "check_register") };
            buttons[2] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: "🧑🏻‍💻MESSAGE ME", $"https://t.me/{pm.Replace("@", "")}") };
            return buttons;
        }

        virtual protected InlineKeyboardMarkup getFdPushMarkup(string start_param, string? link, string pm, string uuid)
        {
            var decode = StartParamDecoder.Decode(start_param);

            InlineKeyboardButton[][] buttons = new InlineKeyboardButton[3][];
            buttons[0] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: "💰DEPOSIT", $"{link}/casino/list?open=deposit&sub1={uuid}&sub2={decode.buyer}&sub3={decode.closer}&sub4={decode.source}&sub5={decode.num}") };
            buttons[1] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: "⚠️CHECK DEPOSIT", callbackData: $"check_fd") };
            buttons[2] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: "🧑🏻‍💻MESSAGE ME", $"https://t.me/{pm.Replace("@", "")}") };
            return buttons;
        }

        virtual protected InlineKeyboardMarkup getRdPushMarkup(string start_param,string? link, string pm, string uuid)
        {
            var decode = StartParamDecoder.Decode(start_param);

            InlineKeyboardButton[][] buttons = new InlineKeyboardButton[3][];
            buttons[0] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: "💰DEPOSIT", $"{link}/casino/list?open=deposit&sub1={uuid}&sub2={decode.buyer}&sub3={decode.closer}&sub4={decode.source}&sub5={decode.num}") };
            buttons[1] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: "⚠️CHECK DEPOSIT", callbackData: $"check_rd1") };
            buttons[2] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: "🧑🏻‍💻MESSAGE ME", $"https://t.me/{pm.Replace("@", "")}") };
            return buttons;
        }


        public override StateMessage GetMessage(string status, string? link = null, string? pm = null, string? uuid = null, string? channel = null, bool? isnegative = false)
        {
            string code = string.Empty;
            InlineKeyboardMarkup markUp = null;

            switch (status)
            {
                case "video":

                    //if (videoMessage == null)
                    //{
                    //    videoMessage = new StateMessage();
                    //    videoMessage.Message = new();
                    //    videoMessage.Message.Video = new Telegram.Bot.Types.Video();                        
                    //    videoMessage.FilePath = Path.Combine(Directory.GetCurrentDirectory(), "resources", "aviator_v1_0.mp4");

                    //    if (messages.ContainsKey("video"))
                    //    {
                    //        var m = messages["video"];                            
                    //        videoMessage.Message.CaptionEntities = m.Message.Entities;
                    //        videoMessage.Message.Caption = m.Message.Text;
                    //    }

                    //    videoMessage.Message.ReplyMarkup = getVideoMarkup(pm);
                    //}

                    //return videoMessage;
                    markUp = getVideoMarkup(pm);
                    code = "video";
                    break;

                case "reg":
                    markUp = getRegMarkup(link, pm, uuid);
                    code = "reg";
                    break;

                case "WREG":
                    markUp = getRegMarkup(link, pm, uuid);
                    code = (isnegative == true) ? "reg_fail" : "reg";
                    break;

                case "WFDEP":
                    code = (isnegative == true) ? "fd_fail" : "fd";
                    markUp = getFDMarkup(pm, link, uuid);
                    break;

                case "WREDEP1":
                    code = (isnegative == true) ? "rd_fail" : "vip";
                    markUp = getVipMarkup(pm, link, channel, uuid);
                    break;

                default:
                    code = "vip";
                    markUp = getVipMarkup(pm, link, channel, uuid);
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

        public override StateMessage GetMessage(tgFollowerStatusResponse? resp, string? link = null, string? pm = null, string? channel = null, bool? isnegative = false)
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
                case "video":

                    //if (videoMessage == null)
                    //{
                    //    videoMessage = new StateMessage();
                    //    videoMessage.Message = new();
                    //    videoMessage.Message.Video = new Telegram.Bot.Types.Video();                        
                    //    videoMessage.FilePath = Path.Combine(Directory.GetCurrentDirectory(), "resources", "aviator_v1_0.mp4");

                    //    if (messages.ContainsKey("video"))
                    //    {
                    //        var m = messages["video"];                            
                    //        videoMessage.Message.CaptionEntities = m.Message.Entities;
                    //        videoMessage.Message.Caption = m.Message.Text;
                    //    }

                    //    videoMessage.Message.ReplyMarkup = getVideoMarkup(pm);
                    //}

                    //return videoMessage;
                    markUp = getVideoMarkup(pm);
                    code = "video";
                    break;

                case "reg":
                    markUp = getRegMarkup(start_params, link, pm, uuid);
                    code = "reg";
                    break;

                case "WREG":
                    markUp = getRegMarkup(start_params, link, pm, uuid);
                    code = (isnegative == true) ? "reg_fail" : "reg";
                    break;

                case "WFDEP":

                    //if (isnegative == true)
                    //{
                    //    code = (add_pay_sum > 0) ? "push_sum" : "fd_fail";
                    //}
                    //else
                    //    code = "fd";

                    if (paid_sum > 0)
                        code = "push_sum";
                    else
                        code = (isnegative == true) ? "fd_fail" : "fd";

                    markUp = getFDMarkup(start_params, pm, link, uuid);
                    break;

                case "WREDEP1":
                    code = (isnegative == true) ? "rd_fail" : "vip";
                    markUp = getVipMarkup(start_params, pm, link, channel, uuid);
                    break;

                default:
                    code = "vip";
                    markUp = getVipMarkup(start_params, pm, link, channel, uuid);
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

        public override StateMessage GetPush(tgFollowerStatusResponse? resp, string? code, string? link = null, string? pm = null, string? channel = null, bool? isnegative = false)
        {
            StateMessage push = null;
            var start_params = resp.start_params;
            var uuid = resp.uuid;

            var found = messages.ContainsKey(code);
            if (found)
            {
                InlineKeyboardMarkup markup = null;


                switch (code)
                {
                    case "PUSH_NO_WREG_3H":
                    case "PUSH_NO_WREG_12H":
                        markup = getRegPushMarkup(start_params, link, pm, uuid);
                        break;

                    case "PUSH_NO_WFDEP_3H":
                    case "PUSH_NO_WFDEP_12H":
                        markup = getFdPushMarkup(start_params, link, pm, uuid);
                        break;

                    case "PUSH_NO_WREDEP_3H":
                    case "PUSH_NO_WREDEP_12H":
                        markup = getRdPushMarkup(start_params, link, pm, uuid);
                        break;

                    default:
                        break;
                }

                push = messages[code].Clone();
                push.Message.ReplyMarkup = markup;
            }
            return push;
        }

        public override StateMessage GetChatJoinMessage()
        {
            StateMessage msg = null;
            string code = "join";

            if (messages.ContainsKey(code))
            {
                msg = messages[code];
            }
            else
            {
                var found = MessageTypes.FirstOrDefault(m => m.Code.Equals(code));
                if (found != null)
                    found.IsSet = false;
            }

            return msg;            
        }
    }
}
