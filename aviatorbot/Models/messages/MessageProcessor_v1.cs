using aviatorbot.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace aviatorbot.Models.messages
{
    public class MessageProcessor_v1 : MessageProcessor_v0
    {        
        #region vars        
        StateMessage videoMessage = null;
        #endregion

        public override ObservableCollection<messageControlVM> MessageTypes
        {
            get => new ObservableCollection<messageControlVM>() {

                new messageControlVM(this)
                {
                    Code = "video",
                    Description = "Текст видео сообщения"
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

        virtual protected InlineKeyboardMarkup getVideoMarkup(string pm)
        {
            InlineKeyboardButton[][] buttons = new InlineKeyboardButton[2][];
            buttons[0] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: "📲GET SOFTWARE", callbackData: $"show_reg") };            
            buttons[1] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: "🧑🏻‍💻Message me", $"https://t.me/{pm.Replace("@", "")}") };
            return buttons;
        }

        override protected InlineKeyboardMarkup getRegMarkup(string link, string pm, string uuid)
        {
            InlineKeyboardButton[][] reg_buttons = new InlineKeyboardButton[3][];
            reg_buttons[0] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: "📲REGISTER", $"{link}/casino/list?open=register&sub1={uuid}&sub2={uuid}") };
            reg_buttons[1] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: "🔍CHECK REGISTRATION", callbackData: "check_register") };
            reg_buttons[2] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: "🧑🏻‍💻Help", $"https://t.me/{pm.Replace("@", "")}") };

            return reg_buttons;
        }

        override protected InlineKeyboardMarkup getFDMarkup(string link, string uuid)
        {
            InlineKeyboardButton[][] dep_buttons = new InlineKeyboardButton[2][];
            dep_buttons[0] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: "💸DEPOSIT", $"{link}/casino/list?open=deposit&sub1={uuid}&sub2={uuid}") };
            dep_buttons[1] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: "🔍CHECK DEPOSIT", callbackData: $"check_fd") };
            return dep_buttons;
        }

        override protected InlineKeyboardMarkup getRD1Markup(string link, string uuid)
        {
            InlineKeyboardButton[][] dep_buttons = new InlineKeyboardButton[2][];
            dep_buttons[0] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: "💸DEPOSIT", $"{link}/casino/list?open=deposit&sub1={uuid}&sub2={uuid}") };
            dep_buttons[1] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: "🔍CHECK DEPOSIT", callbackData: $"check_rd1") };
            return dep_buttons;
        }

        override protected InlineKeyboardMarkup getVipMarkup(string link, string channel, string uuid)
        {
            InlineKeyboardButton[][] vip_buttons = new InlineKeyboardButton[2][];
            vip_buttons[0] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: "🔥GO TO VIP🔥", $"{channel}") };
            vip_buttons[1] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: "PLAY💰", $"{link}/casino/play/aviator?sub1={uuid}&sub2={uuid}") };
            return vip_buttons;
        }

        override protected InlineKeyboardMarkup getRegPushMarkup(string link, string pm, string uuid)
        {
            var buttons = new InlineKeyboardButton[3][];
            buttons[0] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: "📲REGISTER", $"{link}/casino/list?open=deposit&sub1={uuid}&sub2={uuid}") };
            buttons[1] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: "🔍CHECK REGISTRATION", callbackData: "check_register") };
            buttons[2] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: "🧑🏻‍💻Help", $"https://t.me/{pm.Replace("@", "")}") };
            return buttons;
        }

        override protected InlineKeyboardMarkup getFdPushMarkup(string link, string pm, string uuid)
        {
            InlineKeyboardButton[][] buttons = new InlineKeyboardButton[3][];
            buttons[0] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: "💸DEPOSIT", $"{link}/casino/list?open=deposit&sub1={uuid}&sub2={uuid}") };
            buttons[1] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: "🔍CHECK DEPOSIT", callbackData: $"check_fd") };
            buttons[2] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: "🧑🏻‍💻Help", $"https://t.me/{pm.Replace("@", "")}") };
            return buttons;
        }

        override protected InlineKeyboardMarkup getRdPushMarkup(string link, string pm, string uuid)
        {
            InlineKeyboardButton[][] buttons = new InlineKeyboardButton[3][];
            buttons[0] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: "💸DEPOSIT", $"{link}/casino/list?open=deposit&sub1={uuid}&sub2={uuid}") };
            buttons[1] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: "🔍CHECK DEPOSIT", callbackData: $"check_rd1") };
            buttons[2] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: "🧑🏻‍💻Help", $"https://t.me/{pm.Replace("@", "")}") };
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

        public MessageProcessor_v1(string geotag,  string token, ITelegramBotClient bot) : base(geotag, token, bot)
        {
            

        }
    }
}
