using asknvl.messaging;
using asknvl.server;
using botservice.Models.messages;
using botservice.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;
using static asknvl.server.TGBotFollowersStatApi;

namespace botservice.Models.messages.raceup
{
    public class MP_landing_mostbet_postback : MessageProcessorBase
    {
        #region vars
        Languages language;
        #endregion

        #region properties
        public override ObservableCollection<messageControlVM> MessageTypes { get; }
        #endregion
        public MP_landing_mostbet_postback(string geotag, string token, ITelegramBotClient bot, Languages language) : base(geotag, token, bot)
        {
            MessageTypes = new ObservableCollection<messageControlVM>()
            {
                new messageControlVM(this)
                {
                    Code = "start",
                    Description = "Первое сообщение"
                },
                new messageControlVM(this)
                {
                    Code = "circle",
                    Description = "Кружок"
                },
                new messageControlVM(this)
                {
                    Code = "video",
                    Description = "Видео"
                },
                new messageControlVM(this)
                {
                    Code = "reg",
                    Description = "Регистрация"
                },                
                new messageControlVM(this)
                {
                    Code = "fd",
                    Description = "ФД"
                },                                
                new messageControlVM(this)
                {
                    Code = "activated",
                    Description = "Доступ в ВИП"
                },
                new messageControlVM(this)
                {
                    Code = "rd1_ok",
                    Description = "РД2 Тренинг"
                },
                new messageControlVM(this)
                {
                    Code = "rd4_ok_1",
                    Description = "РД4 VIP"
                }
            };

            for (int i = 1; i <= 8; i++)
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

            this.language = language;
        }

        #region private
        virtual protected InlineKeyboardMarkup getStartMarkup(string landing_channel)
        {
            InlineKeyboardButton[][] buttons = new InlineKeyboardButton[1][];

            string title = "";

            switch (language)
            {                
                case Languages.en:
                    title = "Subscribe";
                    break;

                default:
                    break;
            }

            buttons[0] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: $"✅{title}✅", $"{landing_channel}") };
            return buttons;
        }

        virtual protected InlineKeyboardMarkup getCircleMarkup(string landing_channel)
        {
            return getStartMarkup(landing_channel);
        }

        virtual protected InlineKeyboardMarkup getVideoMarkup(string landing_channel)
        {
            InlineKeyboardButton[][] buttons = new InlineKeyboardButton[1][];

            string title = "";

            switch (language)
            {                
                case Languages.en:
                    title = "I'M READY"; //я готов
                    break;

                default:
                    break;
            }
            
            buttons[0] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: $"✅{title}✅", callbackData: "reg") };
            return buttons;
        }

        protected string getAtributedLink(string link, string? uuid, string? src)
        {
            //https://yf6nramb.com/RCFF/0/{subid}/{sub_id_7}
            var tmp = link.Replace("sub_id_7", uuid);
            return tmp;
        }

        virtual protected InlineKeyboardMarkup getRegMarkup(string link, string support_pm, string uuid)
        {
            InlineKeyboardButton[][] buttons = new InlineKeyboardButton[3][];

            string title1 = "";
            string title2 = "";
            string title3 = "";

            switch (language)
            {
                case Languages.de:
                    title1 = "REGISTRIERUNG";
                    title2 = "REGISTRIERUNG BESTÄTIGEN";
                    title3 = "HILFE";
                    break;

                default:
                    break;
            }

            buttons[0] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: $"💰 {title1}", getRegUrl(link, uuid)) };
            buttons[1] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: $"💸 {title2}", callbackData: "check_register") };
            buttons[2] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: $"🆘 {title3}", $"https://t.me/{support_pm.Replace("@", "")}") };

            return buttons;
        }

        string getFdUrl(string link, string uuid)
        {
            return $"{link}?uuid={uuid}";
        }

        virtual protected InlineKeyboardMarkup getFdMarkup(string link, string support_pm, string uuid)
        {
            InlineKeyboardButton[][] buttons = new InlineKeyboardButton[3][];

            string title1 = "";
            string title2 = "";
            string title3 = "";

            switch (language)
            {
                case Languages.de:
                    title1 = "GUTHABEN AUFFÜLLEN";
                    title2 = "SIE GUTHABEN ZU ÜBERPRÜFEN";
                    title3 = "HILFE";
                    break;

                default:
                    break;
            }

            InlineKeyboardButton[][] reg_buttons = new InlineKeyboardButton[3][];
            buttons[0] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: $"💸 {title1}", getFdUrl(link, uuid)) };
            buttons[1] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: $"🕒 {title2}", callbackData: "check_fd") };
            buttons[2] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: $"🆘 {title3}", $"https://t.me/{support_pm.Replace("@", "")}") };

            return buttons;
        }

        virtual protected InlineKeyboardMarkup getVipMarkup(string vip)
        {
            InlineKeyboardButton[][] buttons = new InlineKeyboardButton[1][];

            string title = "";

            switch (language)
            {
                case Languages.de:
                    title = "VIP CHAT";
                    break;

                default:
                    break;
            }
            
            buttons[0] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: $"💰 {title}", vip) };
            return buttons;
        }

        virtual protected InlineKeyboardMarkup getTrainingMarkup(string training)
        {
            InlineKeyboardButton[][] buttons = new InlineKeyboardButton[1][];

            string title = "";

            switch (language)
            {
                case Languages.de:
                    title = "LERNKANAL";
                    break;

                default:
                    break;
            }

            buttons[0] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: $"💪 {title}", training) };
            return buttons;
        }
        #endregion

        #region public
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
                    markUp = getStartMarkup(channel);
                    code = "start";
                    break;

                case "circle":
                    code = "circle";
                    markUp = getCircleMarkup(channel);//!!!
                    break;

                case "video":
                    markUp = getVideoMarkup(channel);
                    code = "video";
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

                    markUp = getFdMarkup(link, support_pm, uuid);
                    break;

                case "WREDEP1":
                    code = "activated";
                    markUp = getVipMarkup(vip);
                    break;

                case "WREDEP2":
                    code = "activated";
                    markUp = getVipMarkup(vip);
                    break;

                case "rd1_ok":
                    code = "rd1_ok";
                    markUp = getTrainingMarkup(training);
                    break;

                case "rd4_ok_1":
                    code = "rd4_ok_1";
                    markUp = getVipMarkup(vip);
                    break;

                default:
                    break;
            }

            StateMessage msg = null;

            if (messages.ContainsKey(code))
            {
                msg = messages[code];

                List<AutoChange> autoChange = new List<AutoChange>()
                {
                    new AutoChange()
                    {
                        OldText = "https://lndchannel.chng",
                        NewText = $"{channel}"
                    },
                    new AutoChange()
                    {
                        OldText = "https://help.chng",
                        NewText = $"{help}"
                    },
                    new AutoChange()
                    {
                        OldText = "_sum_",
                        NewText = $"{add_pay_sum}"
                    },
                    new AutoChange()
                    {
                        OldText = "https://vip.chng",
                        NewText = $"{vip}"
                    },
                    new AutoChange()
                    {
                        OldText = "https://training.chng",
                        NewText = $"{training}"
                    }
                };

                var _msg = msg.Clone();
                _msg.MakeAutochange(autoChange);
                _msg.Message.ReplyMarkup = markUp;
                return _msg;
                
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
                                                string? help = null,
                                                string? param1 = null)
        {

            tgFollowerStatusResponse statusResponse = new tgFollowerStatusResponse()
            {
                status_code = status,
                uuid = uuid                
            };


            return GetMessage(statusResponse, 
                              link: link,       
                              support_pm: support_pm,
                              pm: pm,                              
                              channel: channel,
                              isnegative: isnegative,
                              training: training,
                              vip: vip,
                              help: help);
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
                    markup = getRegMarkup(link, support_pm, uuid);
                }
                else
                if (code.Contains("WFDEP"))
                {
                    markup = getFdMarkup(link, support_pm, uuid);
                }
                else
                if (code.Contains("WREDEP"))
                {
                    markup = getFdMarkup(link, pm, uuid);
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

        public override StateMessage GetPush(string? code, string? link = null, string? pm = null, string? uuid = null, string? channel = null, bool? isnegative = false)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
