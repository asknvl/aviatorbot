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

namespace aviatorbot.Models.messages.raceup_tier1
{
    public class MP_raceup_tier1_postback : MessageProcessorBase
    {
        #region vars
        Languages language;
        #endregion

        #region properties
        public override ObservableCollection<messageControlVM> MessageTypes { get; }
        #endregion
        public MP_raceup_tier1_postback(string geotag, string token, ITelegramBotClient bot, Languages language) : base(geotag, token, bot)
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
                case Languages.de:
                    title = "Abonnieren";
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
                case Languages.de:
                    title = "ICH BIN BEREIT";
                    break;

                default:
                    break;
            }

            buttons[0] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: $"✅{title}✅", $"{landing_channel}") };
            return buttons;
        }

        string getRegUrl(string link, string uuid)
        {
            return $"{link}?uuid={uuid}";
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
                    title1 = "ICH BIN BEREIT";
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
        public override StateMessage GetMessage(string status, string? link = null, string? support_pm = null, string? pm = null, string? uuid = null, string? channel = null, bool? isnegative = false, string? training = null, string? vip = null, string? help = null, string? param1 = null)
        {
            throw new NotImplementedException();
        }

        public override StateMessage GetMessage(TGBotFollowersStatApi.tgFollowerStatusResponse? resp, string? link = null, string? support_pm = null, string? pm = null, string? channel = null, bool? isnegative = false, string? training = null, string? vip = null, string? help = null)
        {
            throw new NotImplementedException();
        }

        public override StateMessage GetPush(string? code, string? link = null, string? pm = null, string? uuid = null, string? channel = null, bool? isnegative = false)
        {
            throw new NotImplementedException();
        }

        public override StateMessage GetPush(TGBotFollowersStatApi.tgFollowerStatusResponse? resp, string? code, string? link = null, string? support_pm = null, string? pm = null, string? channel = null, bool? isnegative = false, string? vip = null, string? help = null)
        {
            throw new NotImplementedException();
        }
        public override StateMessage GetChatJoinMessage()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
