using aksnvl.messaging;
using asknvl.logger;
using asknvl.server;
using Avalonia.X11;
using aviatorbot.Models.bot;
using aviatorbot.Models.messages;
using aviatorbot.ViewModels;
using HarfBuzzSharp;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
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

namespace aviatorbot.Model.bot
{
    public abstract class AviatorBotBase : ViewModelBase, IAviatorBot, IPushObserver
    {
        #region const
        public const string TAG = "BOT";
        #endregion

        #region vars
        protected ILogger logger;
        protected ITelegramBotClient bot;
        protected CancellationTokenSource cts;
        protected State state = State.free;
        protected ITGBotFollowersStatApi server;
        protected long ID;
        #endregion

        #region properties
        string geotag;
        public string Geotag
        {
            get => geotag;
            set => this.RaiseAndSetIfChanged(ref geotag, value);
        }

        string name;
        public string Name
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

        string channel;
        public string Channel
        {
            get => channel;
            set => this.RaiseAndSetIfChanged(ref channel, value);
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

        MessageProcessorBase messageProcessor;
        public MessageProcessorBase MessageProcessor
        {
            get => messageProcessor;
            set => this.RaiseAndSetIfChanged(ref messageProcessor, value);
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
        #endregion

        public AviatorBotBase(ILogger logger)
        {
            this.logger = logger;

            #region commands
            startCmd = ReactiveCommand.CreateFromTask(async () =>
            {
                await Start();
            });

            stopCmd = ReactiveCommand.Create(() =>
            {
                Stop();
            });
            #endregion
        }

        #region private
        async Task processFollower(Message message)
        {

            if (message.Text == null)
                return;

            try
            {

                long chat = message.Chat.Id;
                var fn = message.From.Username;
                var ln = message.From.FirstName;
                var un = message.From.LastName;

                string uuid = string.Empty;
                string status = string.Empty;

                if (message.Text.Equals("/start"))
                {

                    var msg = $"START: {chat} {fn} {ln} {un} ?";
                    logger.inf(Geotag, msg);

                    List<Follower> followers = new();
                    var follower = new Follower()
                    {
                        tg_chat_id = ID,
                        tg_user_id = message.From.Id,
                        username = message.From.Username,
                        firstname = message.From.FirstName,
                        lastname = message.From.LastName,
                        office_id = (int)Offices.KRD,
                        tg_geolocation = Geotag,
                        is_subscribed = true
                    };
                    followers.Add(follower);

                    await server.UpdateFollowers(followers);
                    (uuid, status) = await server.GetFollowerState(chat);
                    
                    var m = MessageProcessor.GetMessage(status, Link, PM, uuid, Channel, false);
                    int id = 0;
                    id = await m.Send(chat, bot, null);

                    while (true)
                    {
                        try
                        {
                            await bot.DeleteMessageAsync(chat, --id);
                        }
                        catch (Exception ex)
                        {
                            break;
                        }
                    }

                    msg = $"STARTED: {chat} {fn} {ln} {un} {uuid} {status}";
                    logger.inf(Geotag, msg);

                }
                else
                {
                    (uuid, status) = await server.GetFollowerState(chat);
                    var msg = $"TEXT: {chat} {fn} {ln} {un} {uuid} {status}\n{message.Text}";
                    logger.inf(Geotag, msg);
                }

            }
            catch (Exception ex)
            {
                logger.err(Geotag, $"processFollower: {ex.Message}");
            }
        }

        async Task processCallbackQuery(CallbackQuery query)
        {
            long chat = query.Message.Chat.Id;
            PushMessageBase message = null;
            string uuid = string.Empty;
            string status = string.Empty;

            try
            {
                (uuid, status) = await server.GetFollowerState(chat);
                string msg = $"STATUS: {chat} {uuid} {status}";
                logger.inf(Geotag, msg);

                switch (query.Data)
                {
                    case "check_register":

                        if (status.Equals("WREG"))
                        {
                            message = MessageProcessor.GetMessage(status, Link, PM, uuid, Channel, true);
                        }
                        else
                            message = MessageProcessor.GetMessage(status, Link, PM, uuid, Channel, false);
                        break;

                    case "check_fd":
                        if (status.Equals("WFDEP"))
                        {
                            message = MessageProcessor.GetMessage(status, Link, PM, uuid, Channel, true);
                        }
                        else
                            message = MessageProcessor.GetMessage(status, Link, PM, uuid, Channel, false);
                        break;

                    case "check_rd1":
                        if (status.Equals("WREDEP1"))
                        {
                            message = MessageProcessor.GetMessage(status, Link, PM, uuid, Channel, true);
                        }
                        else
                            message = MessageProcessor.GetMessage(status, Link, PM, uuid, Channel, false);

                        break;

                    default:
                        break;
                }

                if (message != null)
                {

                    int id = await message.Send(query.From.Id, bot);

                    while (true)
                    {
                        try
                        {
                            await bot.DeleteMessageAsync(query.From.Id, --id);
                        }
                        catch (Exception ex)
                        {
                            break;
                        }
                    }
                }

                await bot.AnswerCallbackQueryAsync(query.Id);

            }
            catch (Exception ex)
            {
                logger.err(Geotag, $"processCallbackQuery: {ex.Message}");
            }
        }

        async Task processOperator(Message message)
        {
            try
            {
                if (message.Text != null)
                {
                    if (message.Text.Equals("/start"))
                    {

                    }

                    if (message.Text.Equals("/updatemessages"))
                    {
                        MessageProcessor.Clear();
                        state = State.waiting_new_message;
                        return;
                    }
                }

                switch (state)
                {
                    case State.waiting_new_message:
                        MessageProcessor.Add(AwaitedMessageCode, message, PM);
                        state = State.free;
                        break;
                }
            }
            catch (Exception ex)
            {
                logger.err(Geotag, ex.Message);
            }
        }

        async Task processSubscribe(Update update)
        {

            long chat = 0;
            string fn = string.Empty;
            string ln = string.Empty;
            string un = string.Empty;
            string uuid = string.Empty;
            string status = string.Empty;
            string direction = "";

            try
            {

                if (update.MyChatMember != null)
                {

                    var mychatmember = update.MyChatMember;

                    chat = mychatmember.From.Id;
                    fn = mychatmember.From.FirstName;
                    ln = mychatmember.From.LastName;
                    un = mychatmember.From.Username;
                    uuid = "";
                    status = "";

                    List<Follower> followers = new();
                    var follower = new Follower()
                    {
                        tg_chat_id = ID,
                        tg_user_id = chat,
                        username = un,
                        firstname = fn,
                        lastname = ln,
                        office_id = (int)Offices.KRD,
                        tg_geolocation = Geotag
                    };

                    switch (mychatmember.NewChatMember.Status)
                    {
                        case ChatMemberStatus.Member:
                            direction = "UNBLOCK";                            
                            break;

                        case ChatMemberStatus.Kicked:
                            direction = "BLOCK";
                            follower.is_subscribed = false;
                            followers.Add(follower);
                            await server.UpdateFollowers(followers);                          
                            break;

                        default:
                            return;
                    }

                    (uuid, status) = await server.GetFollowerState(chat);                
                }

            }
            catch (Exception ex)
            {
                logger.err(Geotag, $"processSubscribe: {ex.Message}");
            } finally
            {
                var msg = $"{direction}: {chat} {fn} {ln} {un} {uuid} {status}";
                logger.inf(Geotag, msg); // logout JOIN or LEFT
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
            logger.inf(Geotag, "Starting bot...");

            if (IsActive)
            {
                logger.err(Geotag, "Bot already started");
                return;
            }


#if DEBUG
            server = new TGBotFollowersStatApi("http://185.46.9.229:4000");
#else
            server = new TGBotFollowersStatApi("http://136.243.74.153:4000");
#endif

            bot = new TelegramBotClient(Token);
            var u = await bot.GetMeAsync();
            Name = u.Username;
            ID = u.Id;

            cts = new CancellationTokenSource();

            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = new UpdateType[] { UpdateType.Message, UpdateType.CallbackQuery, UpdateType.MyChatMember, UpdateType.ChatMember, UpdateType.ChatJoinRequest }
            };


            MessageProcessor = new MessageProcessor_v1(geotag, bot);
            MessageProcessor.UpdateMessageRequestEvent += async (code, description) =>
            {

                AwaitedMessageCode = code;
                state = State.waiting_new_message;

                foreach (var op in Operators)
                {
                    try
                    {
                        await bot.SendTextMessageAsync(op, $"Перешлите сообщение для: \n{description.ToLower()}");

                    }
                    catch (Exception ex)
                    {
                        logger.err("BOT", $"UpdateMessageRequestEvent: {ex.Message}");
                    }
                }

            };

            MessageProcessor.ShowMessageRequestEvent += async (message) =>
            {
                foreach (var op in Operators)
                {
                    try
                    {
                        int id = await message.Send(op, bot);
                    }
                    catch (Exception ex)
                    {
                        logger.err(Geotag, $"ShowMessageRequestEvent: {ex.Message}");
                    }
                }

            };

            MessageProcessor.Init();

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

        public async Task<bool> Push(long id, string code)
        {
            bool res = false;
            try
            {
                var push = messageProcessor.GetPush(code);

                if (push != null)
                {
                    try
                    {

                        await push.Send(id, bot);
                        res = true;
                        logger.inf(Geotag, $"PUSHED: {id} {code}");

                    }
                    catch (Exception ex)
                    {

                        //List<Follower> followers = new();
                        //var follower = new Follower()
                        //{
                        //    tg_chat_id = 0,
                        //    tg_user_id = id,
                        //    office_id = (int)Offices.KRD,
                        //    tg_geolocation = Geotag,
                        //    is_subscribed = false
                        //};
                        //followers.Add(follower);
                        //await server.UpdateFollowers(followers);                        
                    }

                }
            }
            catch (Exception ex)
            {
                logger.err(Geotag, $"Push: {ex.Message}");
            }
            return res;
        }
        #endregion
    }
}
