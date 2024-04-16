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

namespace aviatorbot.Models.messages.latam
{
    public class MP_latam_smrnv : MessageProcessorBase
    {
        public override ObservableCollection<messageControlVM> MessageTypes { get; }   

        public MP_latam_smrnv(string geotag, string token, ITelegramBotClient bot) : base(geotag, token, bot)
        {
            MessageTypes = new ObservableCollection<messageControlVM>()
            {
                new messageControlVM(this)
                {
                    Code = "hi_1",
                    Description = "Приветственное 1"
                },
                new messageControlVM(this)
                {
                    Code = "hi_1_resp",
                    Description = "Ответ на приветственное 1"
                },
                new messageControlVM(this)
                {
                    Code = "hi_2",
                    Description = "Приветственное 2"
                },
                new messageControlVM(this)
                {
                    Code = "hi_2_resp",
                    Description = "Ответ на приветственное 2"
                },
                new messageControlVM(this)
                {
                    Code = "bye",
                    Description = "Прощальное"
                },
            };
        }

        public StateMessage GetMessage(string code)
        {
            throw new NotImplementedException();
        }

        public StateMessage GetPush(string code)
        {
            throw new NotImplementedException();
        }

        public override StateMessage GetChatJoinMessage()
        {
            throw new NotImplementedException();
        }

        public override StateMessage GetMessage(string status, string? link = null, string? support_pm = null, string? pm = null, string? uuid = null, string? channel = null, bool? isnegative = false, string? training = null, string? vip = null, string? help = null)
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
    }
}
