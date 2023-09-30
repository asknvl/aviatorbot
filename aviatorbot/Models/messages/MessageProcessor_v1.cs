using aksnvl.messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;

namespace aviatorbot.Models.messages
{
    class MessageProcessor_v1 : MessageProcessorBase
    {
        public MessageProcessor_v1(string geotag, ITelegramBotClient bot) : base(geotag, bot)
        {
        }

        public override List<MessageType> MessageTypes {
            get => new List<MessageType>() { 

                new MessageType()
                {
                    Code = "reg",
                    Descritption = "Приветственное сообщение"
                },
                new MessageType()
                {
                    Code = "reg_fail",
                    Descritption = "Регистрация не завершена"
                },
                new MessageType()
                {
                    Code = "fd",
                    Descritption = "ФД"
                },
                new MessageType()
                {
                    Code = "fd_fail",
                    Descritption = "нет ФД"
                },
                new MessageType()
                {
                    Code = "rd",
                    Descritption = "РД"
                },
                new MessageType()
                {
                    Code = "fd_fail",
                    Descritption = "нет ФД"
                },
                new MessageType()
                {
                    Code = "vip",
                    Descritption = "Доступ в VIP"
                },
                new MessageType()
                {
                    Code = "push_sum",
                    Descritption = "Неполная сумма"
                },
                new MessageType()
                {
                    Code = "push_no_reg3",
                    Descritption = "Нет регистрации 3ч"
                },
                new MessageType()
                {
                    Code = "push_no_fd3",
                    Descritption = "Нет ФД 3ч"
                },
                new MessageType()
                {
                    Code = "push_no_rd3",
                    Descritption = "Нет РД 3ч"
                },
                new MessageType()
                {
                    Code = "push_no_reg12",
                    Descritption = "Нет регистрации 12ч"
                },
                new MessageType()
                {
                    Code = "push_no_fd12",
                    Descritption = "Нет ФД 12ч"
                },
                new MessageType()
                {
                    Code = "push_no_rd12",
                    Descritption = "Нет РД 12ч"
                }
            };
        }

        public override Task<PushMessageBase> GetMessage(string status, string link = null, string pm = null, string uuid = null, string channel = null, bool? isnegative = false)
        {
            throw new NotImplementedException();
        }
    }
}
