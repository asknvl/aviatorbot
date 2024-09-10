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

namespace aviatorbot.Models.messages.group_manager
{
    public class MP_group_manager_tier1 : MessageProcessorBase
    {
        public override ObservableCollection<messageControlVM> MessageTypes { get; }

        public MP_group_manager_tier1(string geotag, string token, ITelegramBotClient bot) : base(geotag, token, bot)
        {
            MessageTypes = new ObservableCollection<messageControlVM>()
            {
                new messageControlVM(this)
                {
                    Code = "3_HOUR",
                    Description = "Раз в 3 часа"
                },
                new messageControlVM(this)
                {
                    Code = "24_HOUR",
                    Description = "Раз в 24 часа"
                },
                new messageControlVM(this)
                {
                    Code = "LINK",
                    Description = "Ссылка в личке"
                }
            };
        }

        public override StateMessage GetChatJoinMessage()
        {
            throw new NotImplementedException();
        }

        public override StateMessage GetMessage(string status, string? link = null, string? support_pm = null, string? pm = null, string? uuid = null, string? channel = null, bool? isnegative = false, string? training = null, string? vip = null, string? help = null, string? param1 = null)
        {
            string code = string.Empty;
            InlineKeyboardMarkup markUp = null;
            StateMessage msg = null!;
            code = status;

            if (messages.ContainsKey(code))
            {
                msg = messages[code];

                if (code.Equals("24_HOUR"))
                {
                    if (!string.IsNullOrEmpty(link))
                    {
                        List<AutoChange> autoChange = new List<AutoChange>()
                        {
                            new AutoChange() {
                                OldText = "https://partner.chng",
                                NewText = $"{link}"
                            }
                        };

                        var _msg = msg.Clone();
                        _msg.MakeAutochange(autoChange);
                        _msg.Message.ReplyMarkup = markUp;
                        return _msg;
                    }
                }

                if (code.Equals("LINK"))
                {
                    if (!string.IsNullOrEmpty(link))
                    {
                        List<AutoChange> autoChange = new List<AutoChange>()
                        {
                            new AutoChange() {
                                OldText = "https://partner.chng",
                                NewText = $"{link}"
                            }
                        };

                        var _msg = msg.Clone();
                        _msg.MakeAutochange(autoChange);
                        _msg.Message.ReplyMarkup = markUp;
                        return _msg;
                    }
                }

                msg.Message.ReplyMarkup = markUp;
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
