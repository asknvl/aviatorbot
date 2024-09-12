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

namespace aviatorbot.Models.messages.raceup_tier1
{    
    public class MP_raceup_tier1 : MessageProcessorBase
    {
        public override ObservableCollection<messageControlVM> MessageTypes { get; }

        public MP_raceup_tier1(string geotag, string token, ITelegramBotClient bot) : base(geotag, token, bot)
        {
            MessageTypes = new ObservableCollection<messageControlVM>() {
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
                    Code = "reg",
                    Description = "Регистрация"
                 },
            };

            for (int i = 1; i <= 30; i++)
            {
                MessageTypes.Add(new messageControlVM(this)
                {
                    Code = $"WREG{i}",
                    Description = $"Пуш WREG{i}"
                });
            }
        }

        virtual protected InlineKeyboardMarkup getPostingMarkup(string channel, string pm, Languages language, int button_set)
        {

            string pm_title = "";
            string ch_title = "";

            Dictionary<Languages, (string, string)[]> buttonSet = new();
            buttonSet.Add(Languages.en, new (string, string)[] { 
                ("✅ SUBSCRIBE ✅", "💌 TEXT ME"),
                ("✅ Channel ✅", "💌 TEXT ME"),
                ("✅ My Channel ✅", "✅ I’M READY ✅"),
                ("🔥 Channel 🔥", "💌 TEXT ME 💌"),
            });
            buttonSet.Add(Languages.it, new (string, string)[] {
                ("✅ ISCRIVITI ✅", "💌 Scrivimi"),
                ("✅ Il mio canale ✅", "💌 Scrivimi"),
                ("✅ Sono pronto ✅", "💌 Scrivimi"),
                ("✅ Canale ✅", "💌 Scrivimi 💌"),
            });
            buttonSet.Add(Languages.en, new (string, string)[] {
                ("✅ ABONNIEREN ✅", "💌 Schreib mir"),
                ("✅ ICH BIN BEREIT ✅", "💌 Schreib mir"),
                ("✅ ICH BIN BEREIT ✅", "💌 Schreib mir"),                
                ("✅ Kanal ✅", "💌 Schreib mir"),
            });

            InlineKeyboardButton[][] buttons = new InlineKeyboardButton[2][];
            buttons[0] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: buttonSet[language][button_set].Item1, $"{channel}") };
            buttons[1] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: buttonSet[language][button_set].Item2, $"https://t.me/{pm.Replace("@", "")}") };
            return buttons;
        }

        virtual protected InlineKeyboardMarkup getPushMarkup(string channel, string pm, Languages language)
        {

            string pm_title = "";
            string ch_title = "";

            switch (language)
            {
                case Languages.en:
                    pm_title = "💌 TEXT ME";
                    ch_title = "✅ My channel ✅";
                    break;

                case Languages.it:
                    pm_title = "💌 Scrivimi";
                    ch_title = "✅ Canale ✅";
                    break;

                case Languages.de:
                    pm_title = "💌 Schreib mir";
                    ch_title = "✅ Kanal ✅";
                    break;

                default:
                    break;
            }

            InlineKeyboardButton[][] buttons = new InlineKeyboardButton[2][];
            buttons[0] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: pm_title, $"https://t.me/{pm.Replace("@", "")}") };
            buttons[1] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: ch_title, $"{channel}") };
            return buttons;
        }



        public override StateMessage GetChatJoinMessage()
        {
            throw new NotImplementedException();
        }

        public override StateMessage GetMessage(string status, string? link = null, string? support_pm = null, string? pm = null, string? uuid = null, string? channel = null, bool? isnegative = false, string? training = null, string? vip = null, string? help = null, string? param1 = null)
        {
            string code = string.Empty;
            InlineKeyboardMarkup markUp = null;
            StateMessage msg = null;

            code = status;

            if (messages.ContainsKey(code))
            {
                msg = messages[code];//.Clone();

                if (code.Equals("push_sum"))
                {
                    List<AutoChange> autoChange = new List<AutoChange>()
                    {
                        new AutoChange() {
                            OldText = "https://lndchannel.chng",
                            NewText = $"{channel}"
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
    }
}
