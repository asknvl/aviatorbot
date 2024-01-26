using aviatorbot.Models.param_decoder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types;
using System.Reflection.Emit;

namespace aviatorbot.Models.messages
{
    public abstract class MessageProcessor_v4_base : MessageProcessor_v2
    {

        #region vars
        string reg_link_part;
        string fd_link_part;
        string vip_link_part;
        #endregion

        public MessageProcessor_v4_base(string geotag, string token, ITelegramBotClient bot, string reg_link_part, string fd_link_part, string vip_link_part) : base(geotag, token, bot)
        {
            this.reg_link_part = reg_link_part;
            this.fd_link_part = fd_link_part;
            this.vip_link_part = vip_link_part;
        }

        string getRegString(string start_param, string uuid)
        {
            var decode = StartParamDecoder.Decode(start_param);
            return $"{reg_link_part}?uuid={uuid}&buyer_id={decode.buyer}&closer={decode.closer}&source={decode.source}&acc_num={decode.num}";
        }

        string getFDString(string start_param, string uuid)
        {
            var decode = StartParamDecoder.Decode(start_param);
            return $"{fd_link_part}?uuid={uuid}&buyer_id={decode.buyer}&closer={decode.closer}&source={decode.source}&acc_num={decode.num}";
        }

        string getVipString(string start_param, string uuid)
        {
            var decode = StartParamDecoder.Decode(start_param);
            return $"{vip_link_part}?uuid={uuid}&buyer_id={decode.buyer}&closer={decode.closer}&source={decode.source}&acc_num={decode.num}";
        }

        override protected InlineKeyboardMarkup getRegMarkup(string start_param, string? link, string pm, string uuid)
        {
            var decode = StartParamDecoder.Decode(start_param);

            InlineKeyboardButton[][] reg_buttons = new InlineKeyboardButton[2][];            
            reg_buttons[0] = new InlineKeyboardButton[] { InlineKeyboardButton.WithWebApp(text: "🔥REGISTER", new WebAppInfo() { Url = getRegString(start_param, uuid) }) };
            //reg_buttons[1] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: "⚠️CHECK REGISTRATION", callbackData: "check_register") };
            reg_buttons[1] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: "🧑🏻‍💻MESSAGE ME", $"https://t.me/{pm.Replace("@", "")}") };

            return reg_buttons;
        }

        override protected InlineKeyboardMarkup getFDMarkup(string start_param, string pm, string? link, string uuid)
        {

            var decode = StartParamDecoder.Decode(start_param);

            InlineKeyboardButton[][] dep_buttons = new InlineKeyboardButton[2][];            
            dep_buttons[0] = new InlineKeyboardButton[] { InlineKeyboardButton.WithWebApp(text: "💰DEPOSIT", new WebAppInfo() { Url = getFDString(start_param, uuid) }) };
            //dep_buttons[1] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: "⚠️CHECK DEPOSIT", callbackData: $"check_fd") };
            dep_buttons[1] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: "🧑🏻‍💻MESSAGE ME", $"https://t.me/{pm.Replace("@", "")}") };
            return dep_buttons;
        }

        override protected InlineKeyboardMarkup getVipMarkup(string start_param, string pm, string? link, string channel, string uuid)
        {

            var decode = StartParamDecoder.Decode(start_param);

            InlineKeyboardButton[][] vip_buttons = new InlineKeyboardButton[3][];            
            vip_buttons[0] = new InlineKeyboardButton[] { InlineKeyboardButton.WithWebApp(text: "💰PLAY💰", new WebAppInfo() { Url = getVipString(start_param, uuid) }) };
            vip_buttons[1] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: "🥰VIP CHANNEL 🥰", $"{channel}") };
            vip_buttons[2] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: "🔥MESSAGE ME🔥", $"https://t.me/{pm.Replace("@", "")}") };
            return vip_buttons;
        }

        override protected InlineKeyboardMarkup getRegPushMarkup(string start_param, string? link, string pm, string uuid)
        {
            var decode = StartParamDecoder.Decode(start_param);

            var buttons = new InlineKeyboardButton[2][];
            buttons[0] = new InlineKeyboardButton[] { InlineKeyboardButton.WithWebApp(text: "🔥REGISTER", new WebAppInfo() { Url = getRegString(start_param, uuid) }) };
            //buttons[1] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: "⚠️CHECK REGISTRATION", callbackData: "check_register") };
            buttons[1] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: "🧑🏻‍💻MESSAGE ME", $"https://t.me/{pm.Replace("@", "")}") };
            return buttons;
        }

        override protected InlineKeyboardMarkup getFdPushMarkup(string start_param, string? link, string pm, string uuid)
        {
            var decode = StartParamDecoder.Decode(start_param);

            InlineKeyboardButton[][] buttons = new InlineKeyboardButton[2][];
            buttons[0] = new InlineKeyboardButton[] { InlineKeyboardButton.WithWebApp(text: "💰DEPOSIT", new WebAppInfo() { Url = getFDString(start_param, uuid) }) };
            //buttons[1] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: "⚠️CHECK DEPOSIT", callbackData: $"check_fd") };
            buttons[1] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: "🧑🏻‍💻MESSAGE ME", $"https://t.me/{pm.Replace("@", "")}") };
            return buttons;
        }

        override protected InlineKeyboardMarkup getRdPushMarkup(string start_param, string link, string pm, string uuid)
        {
            var decode = StartParamDecoder.Decode(start_param);

            InlineKeyboardButton[][] buttons = new InlineKeyboardButton[2][];
            buttons[0] = new InlineKeyboardButton[] { InlineKeyboardButton.WithWebApp(text: "💰DEPOSIT", new WebAppInfo() { Url = getFDString(start_param, uuid) }) };
            //buttons[1] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: "⚠️CHECK DEPOSIT", callbackData: $"check_fd") };
            buttons[1] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: "🧑🏻‍💻MESSAGE ME", $"https://t.me/{pm.Replace("@", "")}") };
            return buttons;
        }

    }
}
