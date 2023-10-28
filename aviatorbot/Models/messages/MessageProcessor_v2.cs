using aviatorbot.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

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
            InlineKeyboardButton[][] buttons = new InlineKeyboardButton[1][];
            buttons[0] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: "💰GET SOFTWARE ", callbackData: $"show_reg") };
            buttons[1] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: "🧑🏻‍💻MESSAGE ME", $"https://t.me/{pm.Replace("@", "")}") };
            return buttons;
        }

        override protected InlineKeyboardMarkup getRegMarkup(string link, string pm, string uuid)
        {
            InlineKeyboardButton[][] reg_buttons = new InlineKeyboardButton[3][];
            reg_buttons[0] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: "🔥REGISTER", $"{link}/casino/list?open=register&sub1={uuid}&sub2={uuid}") };
            reg_buttons[1] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: "⚠️CHECK REGISTRATION", callbackData: "check_register") };
            reg_buttons[2] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: "🧑🏻‍💻MESSAGE ME", $"https://t.me/{pm.Replace("@", "")}") };

            return reg_buttons;
        }

        override protected InlineKeyboardMarkup getFDMarkup(string pm, string link, string uuid)
        {
            InlineKeyboardButton[][] dep_buttons = new InlineKeyboardButton[3][];
            dep_buttons[0] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: "💰DEPOSIT", $"{link}/casino/list?open=deposit&sub1={uuid}&sub2={uuid}") };
            dep_buttons[1] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: "⚠️CHECK DEPOSIT", callbackData: $"check_fd") };
            dep_buttons[2] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: "🧑🏻‍💻MESSAGE ME", $"https://t.me/{pm.Replace("@", "")}") };
            return dep_buttons;
        }

        override protected InlineKeyboardMarkup getVipMarkup(string pm, string link, string channel, string uuid)
        {
            InlineKeyboardButton[][] vip_buttons = new InlineKeyboardButton[3][];            
            vip_buttons[0] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: "💰PLAY💰", $"{link}/casino/play/aviator?sub1={uuid}&sub2={uuid}") };
            vip_buttons[1] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: "🥰VIP CHANNEL 🥰", $"{channel}") };
            vip_buttons[2] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: "🔥MESSAGE ME🔥", $"https://t.me/{pm.Replace("@", "")}") };
            return vip_buttons;
        }

    }
}
