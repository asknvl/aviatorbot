using aksnvl.messaging;
using asknvl.server;
using aviatorbot.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace aviatorbot.Models.messages
{
    public class MessageProcessor_v0 : MessageProcessorBase
    {

        #region properties
        public override ObservableCollection<messageControlVM> MessageTypes { get; }
        #endregion

        public MessageProcessor_v0(string geotag, string token, ITelegramBotClient bot) : base(geotag, token, bot)
        {

            MessageTypes = new ObservableCollection<messageControlVM>() {

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
                    Code = "rd",
                    Description = "РД"
                },
                new messageControlVM(this)
                {
                    Code = "rd_fail",
                    Description = "нет РД"
                },
                new messageControlVM(this)
                {
                    Code = "vip",
                    Description = "Доступ в VIP"
                },
                new messageControlVM(this)
                {
                    Code = "push_sum",
                    Description = "Неполная сумма"
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

        #region protected
        protected virtual InlineKeyboardMarkup getRegMarkup(string link, string pm, string uuid)
        {
            InlineKeyboardButton[][] reg_buttons = new InlineKeyboardButton[3][];
            reg_buttons[0] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: "📲REGISTER", $"{link}/?id={uuid}") };
            reg_buttons[1] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: "🔍CHECK REGISTRATION", callbackData: "check_register") };
            reg_buttons[2] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: "🧑🏻‍💻Help", $"https://t.me/{pm.Replace("@", "")}") };

            return reg_buttons;
        }

        protected virtual InlineKeyboardMarkup getFDMarkup(string link, string uuid)
        {
            InlineKeyboardButton[][] dep_buttons = new InlineKeyboardButton[2][];
            dep_buttons[0] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: "💸DEPOSIT", $"{link}/?id={uuid}&p=d") };
            dep_buttons[1] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: "🔍CHECK DEPOSIT", callbackData: $"check_fd") };
            return dep_buttons;
        }

        protected virtual InlineKeyboardMarkup getRD1Markup(string link, string uuid)
        {
            InlineKeyboardButton[][] dep_buttons = new InlineKeyboardButton[2][];
            dep_buttons[0] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: "💸DEPOSIT", $"{link}/?id={uuid}&p=d") };
            dep_buttons[1] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: "🔍CHECK DEPOSIT", callbackData: $"check_rd1") };
            return dep_buttons;
        }

        protected virtual InlineKeyboardMarkup getVipMarkup(string link, string channel, string uuid)
        {
            InlineKeyboardButton[][] vip_buttons = new InlineKeyboardButton[2][];
            vip_buttons[0] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: "🔥GO TO VIP🔥", $"{channel}") };
            vip_buttons[1] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: "PLAY💰", $"{link}/?id={uuid}&p=g") };
            return vip_buttons;
        }

        protected virtual InlineKeyboardMarkup getRegPushMarkup(string link, string pm, string uuid)
        {
            var buttons = new InlineKeyboardButton[3][];
            buttons[0] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: "📲REGISTER", $"{link}/?id={uuid}") };
            buttons[1] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: "🔍CHECK REGISTRATION", callbackData: "check_register") };
            buttons[2] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: "🧑🏻‍💻Help", $"https://t.me/{pm.Replace("@", "")}") };
            return buttons;
        }

        protected virtual InlineKeyboardMarkup getFdPushMarkup(string link, string pm, string uuid)
        {
            InlineKeyboardButton[][] buttons = new InlineKeyboardButton[3][];
            buttons[0] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: "💸DEPOSIT", $"{link}/?id={uuid}&p=d") };
            buttons[1] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: "🔍CHECK DEPOSIT", callbackData: $"check_fd") };
            buttons[2] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: "🧑🏻‍💻Help", $"https://t.me/{pm.Replace("@", "")}") };
            return buttons;
        }

        protected virtual InlineKeyboardMarkup getRdPushMarkup(string link, string pm, string uuid)
        {
            InlineKeyboardButton[][] buttons = new InlineKeyboardButton[3][];
            buttons[0] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: "💸DEPOSIT", $"{link}/?id={uuid}&p=d") };
            buttons[1] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: "🔍CHECK DEPOSIT", callbackData: $"check_rd1") };
            buttons[2] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: "🧑🏻‍💻Help", $"https://t.me/{pm.Replace("@", "")}") };
            return buttons;
        }
        #endregion

        public override StateMessage GetMessage(string status, string? link = null, string? pm = null, string? uuid = null, string? channel = null, bool? isnegative = false)
        {
            string code = string.Empty;
            InlineKeyboardMarkup markUp = null;

            switch (status)
            {

                case "WREG":
                    markUp = getRegMarkup(link, pm, uuid);
                    code = (isnegative == true) ? "reg_fail" : "reg";
                    break;

                case "WFDEP":
                    code = (isnegative == true) ? "fd_fail" : "fd";
                    markUp = getFDMarkup(link, uuid);
                    break;

                case "WREDEP1":
                    code = (isnegative == true) ? "rd_fail" : "rd";
                    markUp = getRD1Markup(link, uuid);
                    break;

                //case "WREDEP2":                    
                //    break;
                default:
                    code = "vip";
                    markUp = getVipMarkup(link, channel, uuid);
                    break;
            }

            StateMessage msg = null;

            if (messages.ContainsKey(code))
            {
                msg = messages[code].Clone();
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

        public override StateMessage GetPush(string code, string link = null, string pm = null, string uuid = null, string channel = null, bool? isnegative = false)
        {
            StateMessage push = null;

            var found = messages.ContainsKey(code);
            if (found)
            {
                InlineKeyboardMarkup markup = null;

                switch (code)
                {
                    case "PUSH_NO_WREG_3H":
                    case "PUSH_NO_WREG_12H":
                        markup = getRegPushMarkup(link, pm, uuid);
                        break;

                    case "PUSH_NO_WFDEP_3H":
                    case "PUSH_NO_WFDEP_12H":
                        markup = getFdPushMarkup(link, pm, uuid);
                        break;

                    case "PUSH_NO_WREDEP_3H":
                    case "PUSH_NO_WREDEP_12H":
                        markup = getRdPushMarkup(link, pm, uuid);
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
            throw new NotImplementedException();
        }

        //public override StateMessage GetMessage(TGBotFollowersStatApi.tgFollowerStatusResponse? resp, string? link = null, string? pm = null, string? channel = null, bool? isnegative = false)
        //{
        //    throw new NotImplementedException();
        //}

        public override StateMessage GetPush(TGBotFollowersStatApi.tgFollowerStatusResponse? resp, string? code, string? link = null, string? pm = null, string? channel = null, bool? isnegative = false)
        {
            throw new NotImplementedException();
        }

        public override StateMessage GetMessage(string status, string? link = null, string? pm = null, string? support_pm = null, string? uuid = null, string? channel = null, bool? isnegative = false)
        {
            throw new NotImplementedException();
        }

        public override StateMessage GetMessage(TGBotFollowersStatApi.tgFollowerStatusResponse? resp, string? link = null, string? support_pm = null, string? pm = null, string? channel = null, bool? isnegative = false, string? training = null)
        {
            throw new NotImplementedException();
        }

        public override StateMessage GetPush(TGBotFollowersStatApi.tgFollowerStatusResponse? resp, string? code, string? link = null, string? support_pm = null, string? pm = null, string? channel = null, bool? isnegative = false)
        {
            throw new NotImplementedException();
        }
    }
}
