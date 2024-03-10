using aksnvl.messaging;
using asknvl.logger;
using asknvl.server;
using Avalonia.X11;
using aviatorbot.Models.bot;
using aviatorbot.Models.messages;
using aviatorbot.Models.storage;
using aviatorbot.Operators;
using aviatorbot.rest;
using aviatorbot.ViewModels;
using HarfBuzzSharp;
using motivebot.Model.storage;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reactive;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Requests;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using static System.Net.Mime.MediaTypeNames;

namespace aviatorbot.Model.bot
{
    public abstract class BotBase : ViewModelBase, INotifyObserver
    {

        #region vars        
        protected IOperatorStorage operatorStorage;
        protected IBotStorage botStorage;
        protected ILogger logger;
        protected ITelegramBotClient bot;
        protected CancellationTokenSource cts;
        protected State state = State.free;
        protected ITGBotFollowersStatApi server;
        protected long ID;        
        BotModel tmpBotModel;
        #endregion

        #region properties        
        public abstract BotType Type { get; }

        string geotag;
        public string Geotag
        {
            get => geotag;
            set => this.RaiseAndSetIfChanged(ref geotag, value);
        }

        string? name;
        public string? Name
        {
            get => name;
            set => this.RaiseAndSetIfChanged(ref name, value);
        }

        string token;
        public string Token
        {
            get => token;
            set => this.RaiseAndSetIfChanged(ref token, value);
        }

        bool? postbacks;
        public bool? Postbacks
        {
            get => postbacks;
            set
            {
                if (value == null)
                    value = false;
                this.RaiseAndSetIfChanged(ref postbacks, value);
            }
        }

        bool isActive = false;
        public bool IsActive
        {
            get => isActive;
            set
            {
                IsEditable = false;
                this.RaiseAndSetIfChanged(ref isActive, value);
            }
        }

        bool isEditable;
        public bool IsEditable
        {
            get => isEditable;
            set => this.RaiseAndSetIfChanged(ref isEditable, value);
        }

        string awaitedMessageCode;
        public string AwaitedMessageCode
        {
            get => awaitedMessageCode;
            set => this.RaiseAndSetIfChanged(ref awaitedMessageCode, value);
        }
        #endregion

        #region commands
        public ReactiveCommand<Unit, Unit> startCmd { get; }
        public ReactiveCommand<Unit, Unit> stopCmd { get; }
        public ReactiveCommand<Unit, Unit> editCmd { get; }
        public ReactiveCommand<Unit, Unit> cancelCmd { get; }
        public ReactiveCommand<Unit, Unit> saveCmd { get; }
        #endregion

        public BotBase(IOperatorStorage operatorStorage, IBotStorage botStorage, ILogger logger)
        {
            this.logger = logger;
            this.operatorStorage = operatorStorage;
            this.botStorage = botStorage;

            #region commands
            startCmd = ReactiveCommand.CreateFromTask(async () =>
            {
                await Start();
            });

            stopCmd = ReactiveCommand.Create(() =>
            {
                Stop();
            });

            editCmd = ReactiveCommand.Create(() => {

                tmpBotModel = new BotModel()
                {
                    type = Type,
                    geotag = Geotag,
                    token = Token,
                    postbacks = Postbacks
                };

                IsEditable = true;
            });

            cancelCmd = ReactiveCommand.Create(() => {
                Geotag = tmpBotModel.geotag;
                Token = tmpBotModel.token;              
                Postbacks = tmpBotModel.postbacks;
                IsEditable = false;
            });

            saveCmd = ReactiveCommand.Create(() => {

                var updateModel = new BotModel()
                {
                    type = Type,
                    geotag = Geotag,
                    token = Token,
                    postbacks = Postbacks
                };

                botStorage.Update(updateModel);

                IsEditable = false;

            });
            #endregion
        }

        #region private
        protected abstract Task processFollower(Message message);       

        protected abstract Task processCallbackQuery(CallbackQuery query);

        protected abstract Task processOperator(Message message, Operator op);

        protected abstract Task processSubscribe(Update update);
       
        async Task processMessage(Message message)
        {
            long chat = message.Chat.Id;

            var op = operatorStorage.GetOperator(geotag, chat);
            if (op != null)
            {
                await processOperator(message, op);
            }
            else
            {
                await processFollower(message);
            }
        }

        protected abstract Task processChatJoinRequest(ChatJoinRequest chatJoinRequest, CancellationToken cancellationToken);

        protected abstract Task processChatMember(Update update, CancellationToken cancellationToken);

        async Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken cancellationToken)
        {
            if (update == null)
                return;

            switch (update.Type)
            {
                case UpdateType.MyChatMember:
                    await processSubscribe(update);
                    break;

                case UpdateType.Message:
                    if (update.Message != null)
                        await processMessage(update.Message);
                    break;

                case UpdateType.CallbackQuery:
                    if (update.CallbackQuery != null)
                        await processCallbackQuery(update.CallbackQuery);
                    break;

                case UpdateType.ChatJoinRequest:
                    if (update.ChatJoinRequest != null)
                        await processChatJoinRequest(update.ChatJoinRequest, cancellationToken);
                    break;
                case UpdateType.ChatMember:
                    if (update.ChatMember != null)
                        await processChatMember(update, cancellationToken);
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
            logger.err(Geotag, ErrorMessage);
            return Task.CompletedTask;
        }
        #endregion

        #region public
        public virtual async Task Start()
        {
            logger.inf(Geotag, $"Starting {Type} bot...");

            if (IsActive)
            {
                logger.err(Geotag, "Bot already started");
                return;
            }


#if DEBUG
            server = new TGBotFollowersStatApi("http://185.46.9.229:4000");
            bot = new TelegramBotClient(new TelegramBotClientOptions(Token, "http://localhost:8081/bot/"));            
#elif DEBUG_TG_SERV

            server = new TGBotFollowersStatApi("http://185.46.9.229:4000");
            //server = new TGBotFollowersStatApi("https://ru.flopasda.site");
            bot = new TelegramBotClient(Token);
#else
            server = new TGBotFollowersStatApi("https://ru.flopasda.site");
            bot = new TelegramBotClient(new TelegramBotClientOptions(Token, "http://localhost:8081/bot/"));
#endif

            var u = await bot.GetMeAsync();
            Name = u.Username;
            ID = u.Id;

            cts = new CancellationTokenSource();

            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = new UpdateType[] { UpdateType.Message,
                                                    UpdateType.CallbackQuery,
                                                    UpdateType.MyChatMember,
                                                    UpdateType.ChatMember,
                                                    UpdateType.ChatJoinRequest }
            };           

            bot.StartReceiving(HandleUpdateAsync, HandleErrorAsync, receiverOptions, cts.Token);

            try
            {
                await Task.Run(() => { });
                IsActive = true;
                logger.inf(Geotag, "Bot started");

            }
            catch (Exception ex)
            {
            }
        }

        public virtual async void Stop()
        {
            cts.Cancel();
            IsActive = false;
            logger.inf(Geotag, "Bot stopped");
        }

        public string GetGeotag()
        {
            return Geotag;
        }
        #endregion

        #region callbacks        
        public abstract Task Notify(object notifyObject);
        #endregion
    }
}
