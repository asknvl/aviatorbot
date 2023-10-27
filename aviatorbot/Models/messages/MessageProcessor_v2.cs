using aviatorbot.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;

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
                    Description = "Текст видео сообщения"
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


    }
}
