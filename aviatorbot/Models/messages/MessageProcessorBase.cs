using aksnvl.messaging;
using aksnvl.storage;
using asknvl.messaging;
using asknvl.storage;
using aviatorbot.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace aviatorbot.Models.messages
{
    public abstract class MessageProcessorBase : ViewModelBase, IMessageUpdater
    {
        #region vars
        protected Dictionary<string, StateMessage> messages = new();
        IStorage<Dictionary<string, StateMessage>> messageStorage;
        string geotag;
        ITelegramBotClient bot;
        #endregion

        #region properties
        public abstract ObservableCollection<messageControlVM> MessageTypes { get; }
        #endregion

        public MessageProcessorBase(string geotag, ITelegramBotClient bot)
        {
            this.geotag = geotag;
            this.bot = bot;

            messageStorage = new Storage<Dictionary<string, StateMessage>>("messages.json", "messages", messages);
            messages = messageStorage.load();
        }

        #region private        
        #endregion

        #region public        
        public async void Add(string type, Message message, string pm)
        {
            var pattern = await StateMessage.Create(bot, message, geotag);
            AutoChange pm_autochange = new AutoChange()
            {
                OldText = "@booowos",
                NewText = pm
            };
            var autochanges = new List<AutoChange>() { pm_autochange };
            pattern.MakeAutochange(autochanges);
            pattern.Id = messages.Count();

            if (messages.ContainsKey(type))
                messages[type] = pattern;
            else
                messages.Add(type, pattern);

            messageStorage.save(messages);
        }

        public void Clear()
        {
            messages.Clear();
            messageStorage.save(messages);
        }

        public abstract StateMessage GetMessage(string status,
                                                        string link = null,
                                                        string pm = null,
                                                        string uuid = null,
                                                        string channel = null,
                                                        bool? isnegative = false);
        
        public async Task UpdateMessageRequest(string code)
        {
            await Task.Run(() => { 
                UpdateMessageRequestEvent?.Invoke(code);
            });
        }
        #endregion

        #region callbacks
        public event Action<string> UpdateMessageRequestEvent;
        public event Action<string, bool> MessageUpdatedEvent;
        #endregion
    }
}
