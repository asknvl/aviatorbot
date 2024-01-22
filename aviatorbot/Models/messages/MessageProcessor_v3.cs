using asknvl.logger;
using asknvl.server;
using aviatorbot.Models.param_decoder;
using aviatorbot.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace aviatorbot.Models.messages
{
    public class MessageProcessor_v3 : MessageProcessor_v2
    {
        #region vars
        ILogger logger;
        #endregion

        public MessageProcessor_v3(string geotag, string token, ITelegramBotClient bot, ILogger logger) : base(geotag, token, bot)
        {            
            this.logger = logger;   
        }

        string getRegUrl(string start_param, string? link, string pm, string uuid)
        {
            var decode = StartParamDecoder.Decode(start_param);
            var res = $"{link}/casino/list?open=register&sub1={uuid}&sub2={decode.buyer}&sub3={decode.closer}&sub4={decode.source}&sub5={decode.num}";
            if (string.IsNullOrEmpty(uuid) || uuid.Length != 10)
                logger.err("getRegUrl", res);
            else
                logger.dbg("getRegUrl", res);

            return res;
        }

        string getFDUrl(string start_param, string? link, string pm, string uuid)
        {
            var decode = StartParamDecoder.Decode(start_param);
            var res = $"{link}/casino/list?open=deposit&sub1={uuid}&sub2={decode.buyer}&sub3={decode.closer}&sub4={decode.source}&sub5={decode.num}";
            if (string.IsNullOrEmpty(uuid) || uuid.Length != 10)
                logger.err("getFDUrl", res);
            else
                logger.dbg("getFDUrl", res);
            return res;

        }

        string getVipUrl(string start_param, string? link, string pm, string uuid)
        {
            var decode = StartParamDecoder.Decode(start_param);
            var res = $"{link}/casino/play/aviator?&sub1={uuid}&sub2={decode.buyer}&sub3={decode.closer}&sub4={decode.source}&sub5={decode.num}";
            if (string.IsNullOrEmpty(uuid) || uuid.Length != 10)
                logger.err("getVipUrl", res);
            else
                logger.dbg("getVipUrl", res);
            return res;
        }

        override protected InlineKeyboardMarkup getRegMarkup(string start_param, string? link, string pm, string uuid)
        {
            var decode = StartParamDecoder.Decode(start_param);

            InlineKeyboardButton[][] reg_buttons = new InlineKeyboardButton[3][];
            reg_buttons[0] = new InlineKeyboardButton[] { InlineKeyboardButton.WithWebApp(text: "🔥REGISTER", new WebAppInfo() { Url = getRegUrl(start_param, link, pm, uuid)}) };
            reg_buttons[1] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: "⚠️CHECK REGISTRATION", callbackData: "check_register") };
            reg_buttons[2] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: "🧑🏻‍💻MESSAGE ME", $"https://t.me/{pm.Replace("@", "")}") };

            return reg_buttons;
        }

        override protected InlineKeyboardMarkup getFDMarkup(string start_param, string pm, string? link, string uuid)
        {

            var decode = StartParamDecoder.Decode(start_param);

            InlineKeyboardButton[][] dep_buttons = new InlineKeyboardButton[3][];            
            dep_buttons[0] = new InlineKeyboardButton[] { InlineKeyboardButton.WithWebApp(text: "💰DEPOSIT", new WebAppInfo() { Url = getFDUrl(start_param, link, pm, uuid)}) };
            dep_buttons[1] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: "⚠️CHECK DEPOSIT", callbackData: $"check_fd") };
            dep_buttons[2] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: "🧑🏻‍💻MESSAGE ME", $"https://t.me/{pm.Replace("@", "")}") };
            return dep_buttons;
        }

        override protected InlineKeyboardMarkup getVipMarkup(string start_param, string pm, string? link, string channel, string uuid)
        {

            var decode = StartParamDecoder.Decode(start_param);

            InlineKeyboardButton[][] vip_buttons = new InlineKeyboardButton[3][];            
            vip_buttons[0] = new InlineKeyboardButton[] { InlineKeyboardButton.WithWebApp(text: "💰PLAY💰", new WebAppInfo() { Url = getVipUrl(start_param, link, pm, uuid) }) };
            vip_buttons[1] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: "🥰VIP CHANNEL 🥰", $"{channel}") };
            vip_buttons[2] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: "🔥MESSAGE ME🔥", $"https://t.me/{pm.Replace("@", "")}") };
            return vip_buttons;
        }

        override protected InlineKeyboardMarkup getRegPushMarkup(string start_param, string? link, string pm, string uuid)
        {
            var decode = StartParamDecoder.Decode(start_param);

            var buttons = new InlineKeyboardButton[3][];
            buttons[0] = new InlineKeyboardButton[] { InlineKeyboardButton.WithWebApp(text: "🔥REGISTER", new WebAppInfo() { Url = getRegUrl(start_param, link, pm, uuid) }) };
            buttons[1] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: "⚠️CHECK REGISTRATION", callbackData: "check_register") };
            buttons[2] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: "🧑🏻‍💻MESSAGE ME", $"https://t.me/{pm.Replace("@", "")}") };
            return buttons;
        }

        override protected InlineKeyboardMarkup getFdPushMarkup(string start_param, string? link, string pm, string uuid)
        {
            var decode = StartParamDecoder.Decode(start_param);

            InlineKeyboardButton[][] buttons = new InlineKeyboardButton[3][];
            buttons[0] = new InlineKeyboardButton[] { InlineKeyboardButton.WithWebApp(text: "💰DEPOSIT", new WebAppInfo() { Url = getFDUrl(start_param, link, pm, uuid) }) };
            buttons[1] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: "⚠️CHECK DEPOSIT", callbackData: $"check_fd") };
            buttons[2] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: "🧑🏻‍💻MESSAGE ME", $"https://t.me/{pm.Replace("@", "")}") };
            return buttons;
        }

        override protected InlineKeyboardMarkup getRdPushMarkup(string start_param, string? link, string pm, string uuid)
        {
            var decode = StartParamDecoder.Decode(start_param);

            InlineKeyboardButton[][] buttons = new InlineKeyboardButton[3][];
            buttons[0] = new InlineKeyboardButton[] { InlineKeyboardButton.WithWebApp(text: "💰DEPOSIT", new WebAppInfo() { Url = getFDUrl(start_param, link, pm, uuid) }) };
            buttons[1] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: "⚠️CHECK DEPOSIT", callbackData: $"check_fd") };
            buttons[2] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: "🧑🏻‍💻MESSAGE ME", $"https://t.me/{pm.Replace("@", "")}") };
            return buttons;
        }


    }
}
