using asknvl.logger;
using aviatorbot.Models.messages;
using aviatorbot.ViewModels;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace aviatorbot.Model.bot
{
    public abstract class AviatorBotBase : ViewModelBase, IAviatorBot
    {
        #region vars
        protected ILogger logger;
        protected ITelegramBotClient bot;
        protected CancellationTokenSource cts;
        protected IMessageProcessor messageProcessor;
        protected State state = State.free;
        #endregion

        #region properties
        string geotag;
        public string Geotag {
            get => geotag;
            set => this.RaiseAndSetIfChanged(ref geotag, value);
        }

        string name;
        public string Name {
            get => name;
            set => this.RaiseAndSetIfChanged(ref name, value);
        }

        string token;
        public string Token {
            get => token;
            set => this.RaiseAndSetIfChanged(ref token, value);
        }

        string link;
        public string Link
        {
            get => link;
            set => this.RaiseAndSetIfChanged(ref link, value);
        }

        string pm;
        public string PM
        {
            get => pm;
            set => this.RaiseAndSetIfChanged(ref pm, value);
        }

        public ObservableCollection<long> Operators { get; } = new();

        bool isActive = false;
        public bool IsActive
        {
            get => isActive;
            set => this.RaiseAndSetIfChanged(ref isActive, value);
        }

        bool isEditable = false;
        public bool IsEditable
        {
            get => isEditable & !IsActive;
            set => this.RaiseAndSetIfChanged(ref isEditable, value);
        }
        #endregion

        #region commands
        public ReactiveCommand<Unit, Unit> startCmd { get; }
        public ReactiveCommand<Unit, Unit> stopCmd { get; }
        #endregion

        public AviatorBotBase()
        {
            #region commands
            startCmd = ReactiveCommand.CreateFromTask(async () => {
                await Start();
            });

            stopCmd = ReactiveCommand.Create(() => {
                Stop();
            });
            #endregion
        }

        #region private
        async Task processFollower(Message message) 
        {

            long chat = message.Chat.Id;

            if (message.Text.Equals("/start"))
            {
                var m = await messageProcessor.GetMessage(0, Link, PM);
                var id = await m.Send(chat, bot, null);

                while (true)
                {
                    try
                    {
                        await bot.DeleteMessageAsync(chat, --id);
                    } catch (Exception ex)
                    {
                        break;
                    }
                }

                

            }
        }

        async Task processOperator(Message message)
        {

            if (message.Text != null)
            {
                if (message.Text.Equals("/start"))
                {

                }

                if (message.Text.Equals("/updatemessages"))
                {
                    state = State.waiting_new_message;
                    return;
                }
            }

            switch (state)
            {
                case State.waiting_new_message:
                    messageProcessor.Add(message);
                    break;
            }
        }

        async Task processMessage(Message message)
        {
            long chat = message.Chat.Id;
            if (Operators.Contains(chat))
            {
                await processOperator(message);
            }
            else
            {
                await processFollower(message);
            }
        }

        async Task processCallbackQuery(CallbackQuery query)
        {
            //long chat = query.Message.Chat.Id;

            switch (query.Data)
            {
                case "check_state":
                    await bot.AnswerCallbackQueryAsync(query.Id);
                    break;

                default:
                    break;
            }
        }

        async Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken cancellationToken)
        {
            if (update == null)
                return;

            switch (update.Type)
            {
                case UpdateType.Message:
                    if (update.Message != null)
                        await processMessage(update.Message);
                    break;

                case UpdateType.CallbackQuery:
                    if (update.CallbackQuery != null) 
                        await processCallbackQuery(update.CallbackQuery);

                    break;
            }            
        }

        Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException
                    => $"{Geotag} Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };
            logger.err(ErrorMessage);
            return Task.CompletedTask;
        }
        #endregion

        #region public
        public virtual async Task Start()
        {

            logger = new Logger("BOT", "BOTS", $"{Geotag}");

            logger.inf("Starting bot...");

            if (IsActive)
            {
                logger.err("Bot already started");
                return;
            }

            bot = new TelegramBotClient(Token);            
            var u = await bot.GetMeAsync();
            Name = u.Username;

            cts = new CancellationTokenSource();

            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = new UpdateType[] { UpdateType.Message, UpdateType.CallbackQuery }
            };


            messageProcessor = new MessageProcessor(geotag, bot);

            bot.StartReceiving(HandleUpdateAsync, HandleErrorAsync, receiverOptions, cts.Token);

            try
            {
               await Task.Run(() => { });
               IsActive = true;
                logger.inf("Bot started");

            } catch (Exception ex)
            {

            }            
        }

        public virtual void Stop()
        {
            IsActive = false;
        }
        #endregion
    }
}
