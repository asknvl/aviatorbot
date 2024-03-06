using asknvl.logger;
using asknvl.messaging;
using asknvl.server;
using aviatorbot.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace aviatorbot.Models.messages
{   
    public class MP_Landing_Raceup_cana : MessageProcessorBase
    {
        #region vars
        ILogger logger;
        string reg_link_part;
        string fd_link_part;
        string play_link_part;        
        #endregion

        #region properties
        public override ObservableCollection<messageControlVM> MessageTypes { get; }
        #endregion

        public MP_Landing_Raceup_cana(string geotag, string token, ITelegramBotClient bot, ILogger logger,                                      
                                      string reg_link_part,
                                      string fd_link_part,
                                      string play_link_part) : base(geotag, token, bot)
        {

            this.logger = logger;

            this.reg_link_part = reg_link_part;
            this.fd_link_part = fd_link_part;
            this.play_link_part = play_link_part;            

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
                    Code = "WREG",
                    Description = "Регистрация"
                },                            
                new messageControlVM(this)
                {
                    Code = "WFDEP",
                    Description = "ФД"
                },                                                
                new messageControlVM(this)
                {
                    Code = "pm_access",
                    Description = "Доступ в личку перса"
                },
                new messageControlVM(this)
                {
                    Code = "rd1_ok",
                    Description = "РД1"
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

            for (int i = 1; i <= 8; i++)
            {
                var mcv = new messageControlVM(this)
                {
                    Code = $"WREDEP{i}",
                    Description = $"Пуш деп {i}"
                };

                MessageTypes.Add(mcv);
            }

        }


        protected string getRegUrl(string link, string uuid)
        {
            return $"{reg_link_part}?uuid={uuid}";
        }

        protected string getFDUrl(string link, string uuid)
        {
            return $"{fd_link_part}?uuid={uuid}";
        }

        protected InlineKeyboardMarkup getSubscribeMarkup(string landing_channel)
        {
            InlineKeyboardButton[][] buttons = new InlineKeyboardButton[1][];
            buttons[0] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: "✅SUBSCRIBE✅", $"{landing_channel}") };
            return buttons;
        }

        protected virtual InlineKeyboardMarkup getReadyMarkup()
        {
            InlineKeyboardButton[][] reg_buttons = new InlineKeyboardButton[1][];
            reg_buttons[0] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: "😎I’M READY😎", callbackData: "WREG") };            
            return reg_buttons;
        }

        protected virtual InlineKeyboardMarkup getRegMarkup(string uuid)
        {
            InlineKeyboardButton[][] reg_buttons = new InlineKeyboardButton[2][];
            reg_buttons[0] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: "🔥REGISTER", getRegUrl(reg_link_part, uuid))};
            reg_buttons[1] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: "✅DONE✅", callbackData: "register_done") };            
            return reg_buttons;
        }

        protected virtual InlineKeyboardMarkup getFdMarkup(string uuid)
        {
            InlineKeyboardButton[][] reg_buttons = new InlineKeyboardButton[2][];
            reg_buttons[0] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: "💰DEPOSIT", getRegUrl(fd_link_part, uuid)) };
            reg_buttons[1] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: "✅DONE✅", callbackData: "fd_done") };
            return reg_buttons;
        }

        virtual protected InlineKeyboardMarkup getPmMarkup(string pm)
        {
            InlineKeyboardButton[][] buttons = new InlineKeyboardButton[2][];
            buttons[0] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: "📩MESSAGES📩", $"https://t.me/{pm.Replace("@", "")}") };
            buttons[1] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: "🚀 PLAY", play_link_part) };
            return buttons;
        }

        public override StateMessage GetMessage(string status, string? link = null, string? support_pm = null, string? pm = null, string? uuid = null, string? channel = null, bool? isnegative = false)
        {
            string code = string.Empty;
            InlineKeyboardMarkup markUp = null;

            string friendUrl = $"{link}?sub1=friend";

            switch (status)
            {
                case "start":
                    markUp = getSubscribeMarkup(channel);
                    code = "start";
                    break;

                case "video":
                    markUp = getReadyMarkup();
                    code = "video";
                    break;

                case "WREG":
                    markUp = getRegMarkup(uuid);                    
                    code = "WREG";
                    break;

                case "WFDEP":
                    markUp = getFdMarkup(uuid);
                    code = "WFDEP";
                    break;

                case "WREDEP1":
                    code = "pm_access";
                    markUp = getPmMarkup(pm);
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

        public override StateMessage GetMessage(string status, string? link = null, string? pm = null, string? uuid = null, string? channel = null, bool? isnegative = false)
        {
            throw new NotImplementedException();
        }

        public override StateMessage GetMessage(TGBotFollowersStatApi.tgFollowerStatusResponse? resp, string? link = null, string? pm = null, string? channel = null, bool? isnegative = false)
        {
            throw new NotImplementedException();
        }

        public override StateMessage GetMessage(TGBotFollowersStatApi.tgFollowerStatusResponse? resp, string? link = null, string? support_pm = null, string? pm = null, string? channel = null, bool? isnegative = false, string? training = null)
        {
            throw new NotImplementedException();
        }

        public override StateMessage GetChatJoinMessage()
        {
            throw new NotImplementedException();
        }

        public override StateMessage GetPush(string? code, string? link = null, string? pm = null, string? uuid = null, string? channel = null, bool? isnegative = false)
        {
            throw new NotImplementedException();
        }

        public override StateMessage GetPush(TGBotFollowersStatApi.tgFollowerStatusResponse? resp, string? code, string? link = null, string? pm = null, string? channel = null, bool? isnegative = false)
        {
            throw new NotImplementedException();
        }

        public override StateMessage GetPush(TGBotFollowersStatApi.tgFollowerStatusResponse? resp, string? code, string? link = null, string? support_pm = null, string? pm = null, string? channel = null, bool? isnegative = false)
        {
            throw new NotImplementedException();
        }
    }
}
