using aksnvl.messaging;
using asknvl.logger;
using asknvl.server;
using Avalonia.X11;
using aviatorbot.Models.bot;
using aviatorbot.Models.messages;
using aviatorbot.Operators;
using aviatorbot.ViewModels;
using HarfBuzzSharp;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
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
using Telegram.Bot.Types.ReplyMarkups;

namespace aviatorbot.Model.bot
{
    public abstract class AviatorBotBase : ViewModelBase, IPushObserver
    {        

        #region vars
        protected IOperatorsProcessor operatorsProcessor;
        protected ILogger logger;
        protected ITelegramBotClient bot;
        protected CancellationTokenSource cts;
        protected State state = State.free;
        protected ITGBotFollowersStatApi server;
        protected long ID;
        IMessageProcessorFactory messageProcessorFactory = new MessageProcessorFactory();   
        #endregion

        #region properties        
        public abstract BotType Type { get; }

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

        public ObservableCollection<Operator> Operators { get; } = new();

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

        public AviatorBotBase(IOperatorsProcessor operatorsProcessor, ILogger logger)
        {
            this.logger = logger;
            this.operatorsProcessor = operatorsProcessor;

            var operatros = operatorsProcessor.GetAll(geotag);
            Operators = new ObservableCollection<Operator>(operatros);

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
        public virtual async Task processFollower(Message message)
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
                    (uuid, status) = await server.GetFollowerState(Geotag, chat);
                    
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
                    (uuid, status) = await server.GetFollowerState(Geotag, chat);
                    var msg = $"TEXT: {chat} {fn} {ln} {un} {uuid} {status}\n{message.Text}";
                    logger.inf(Geotag, msg);
                }

            }
            catch (Exception ex)
            {
                logger.err(Geotag, $"processFollower: {ex.Message}");
            }
        }

        protected virtual async Task processCallbackQuery(CallbackQuery query)
        {
            long chat = query.Message.Chat.Id;
            PushMessageBase message = null;
            string uuid = string.Empty;
            string status = string.Empty;

            try
            {
                (uuid, status) = await server.GetFollowerState(Geotag, chat);
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

        async Task sendOperatorTextMessage(long chat, string text)
        {
            ReplyKeyboardMarkup replyKeyboardMarkup = new(new[]
                        {
                            new KeyboardButton[] { $"GIVE REG" },
                            new KeyboardButton[] { $"GIVE FD" },                            
                            new KeyboardButton[] { $"GIVE VIP" }

                        })
            {
                ResizeKeyboard = true,
                OneTimeKeyboard = false,
            };

            await bot.SendTextMessageAsync(
                chat,
                text: text,
                replyMarkup: replyKeyboardMarkup);
        }

        async Task processOperator(Message message)
        {

            var chat = message.From.Id;
          

            try
            {
                if (message.Text != null)
                {
                    if (message.Text.Equals("/start"))
                    {
                        await sendOperatorTextMessage(chat, "Вы вошли как оператор");
                    }

                    if (message.Text.Equals("/updatemessages"))
                    {
                        MessageProcessor.Clear();
                        state = State.waiting_new_message;
                        return;
                    }
                    if (message.Text.Equals("GIVE REG"))
                    {
                        await bot.SendTextMessageAsync(message.From.Id, "Введите TG id для установки статуса РЕГИСТРАЦИЯ:");
                        state = State.waiting_reg_access;
                        return;
                    }
                    if (message.Text.Equals("GIVE FD"))
                    {
                        await bot.SendTextMessageAsync(message.From.Id, "Введите TG id для установки статуса ФД:");
                        state = State.waiting_fd_access;
                        return;
                    }
                    if (message.Text.Equals("GIVE VIP"))
                    {
                        await bot.SendTextMessageAsync(message.From.Id, "Введите TG id для предоставления VIP:");
                        state = State.waiting_vip_access;
                        return;
                    }
                }

                switch (state)
                {
                    case State.waiting_new_message:
                        MessageProcessor.Add(AwaitedMessageCode, message, PM);
                        state = State.free;
                        break;

                    case State.waiting_reg_access:
                        try
                        {
                            long tg_id = long.Parse(message.Text);
                            string uuid = string.Empty;
                            string status = string.Empty;
                            (uuid, status) = await server.GetFollowerState(Geotag, tg_id);

                            await server.SetFollowerRegistered(uuid);
                            
                            string msg = $"Пользователю {tg_id} установлен статус ЗАРЕГИСТРИРОВАН";
                            await sendOperatorTextMessage(chat, msg);
                            logger.inf(geotag, msg);
                            
                        } catch (Exception ex)
                        {
                            await bot.SendTextMessageAsync(message.From.Id, $"{ex.Message}");
                        } finally
                        {
                            state = State.free;
                        }
                        break;

                    case State.waiting_fd_access:
                        try
                        {
                            long tg_id = long.Parse(message.Text);
                            string uuid = string.Empty;
                            string status = string.Empty;
                            (uuid, status) = await server.GetFollowerState(Geotag, tg_id);

                            await server.SetFollowerMadeDeposit(uuid);

                            string msg = $"Пользователю {tg_id} установлен статус ФД";
                            await sendOperatorTextMessage(chat, msg);
                            logger.inf(geotag, msg);

                        }
                        catch (Exception ex)
                        {
                            await bot.SendTextMessageAsync(message.From.Id, $"{ex.Message}");
                        } finally
                        {
                            state = State.free;
                        }
                        break;

                    case State.waiting_vip_access:
                        try
                        {
                            long tg_id = long.Parse(message.Text);
                            string uuid = string.Empty;
                            string status = string.Empty;
                            (uuid, status) = await server.GetFollowerState(Geotag, tg_id);

                            if (!string.IsNullOrEmpty(uuid))
                            {
                                switch (status)
                                {
                                    case "WREG":
                                        await server.SetFollowerRegistered(uuid);
                                        await server.SetFollowerMadeDeposit(uuid);
                                        await server.SetFollowerMadeDeposit(uuid);
                                        break;
                                    case "WFDEP":                                        
                                        await server.SetFollowerMadeDeposit(uuid);
                                        await server.SetFollowerMadeDeposit(uuid);
                                        break;
                                    case "WREDEP1":
                                        await server.SetFollowerMadeDeposit(uuid);
                                        break;
                                    case "WREDEP2":
                                        break;
                                    default:
                                        return;
                                }

                                string msg = $"Пользователю {tg_id} предоставлен доступ к каналу";
                                await sendOperatorTextMessage(chat, msg);
                                logger.inf(geotag, msg);
                            }
                        }
                        catch (Exception ex)
                        {
                            await bot.SendTextMessageAsync(message.From.Id, $"{ex.Message}");
                        } finally
                        {
                            state = State.free;
                        }
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

                    (uuid, status) = await server.GetFollowerState(Geotag,chat);                
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

            var operators = operatorsProcessor.GetAll(geotag).Select(o => o.tg_id);

            if (operators.Contains(chat))
            {
                await processOperator(message);
            }
            else
            {
                await processFollower(message);
            }
        }

        async Task processChatJoinRequest(ChatJoinRequest chatJoinRequest, CancellationToken cancellationToken)
        {
            //try
            //{
            //    var message = MessageProcessor.GetChatJoinMessage();
            //    if (message != null)
            //    {
            //        await message.Send(chatJoinRequest.From.Id, bot);
            //    }
            //}
            //catch (Exception ex)
            //{
            //    logger.err(Geotag, $"processChatJoinRequest: {ex.Message}");
            //}
        }

        async Task processChatMember(ChatMemberUpdated chatMember, CancellationToken cancellationToken)
        {
            try
            {
                long id = chatMember.From.Id;
                string fn = chatMember.From.FirstName;
                string ln = chatMember.From.LastName;
                string un = chatMember.From.Username;

                switch (chatMember.NewChatMember.Status)
                {
                    case ChatMemberStatus.Member:
                        var message = MessageProcessor.GetChatJoinMessage();
                        if (message != null)
                        {
                            await message.Send(chatMember.From.Id, bot);
                        }
                        logger.inf(Geotag, $"VIP JOIN: {id} {fn} {ln} {un}");
                        break;

                    case ChatMemberStatus.Left:
                        logger.inf(Geotag, $"VIP LEFT: {id} {fn} {ln} {un}");
                        break;
                }
            } catch (Exception ex)
            {
                logger.err(Geotag, $"processChatMember: {ex.Message}");
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

                case UpdateType.ChatJoinRequest:
                    if (update.ChatJoinRequest != null) 
                        await processChatJoinRequest(update.ChatJoinRequest, cancellationToken);
                    break;
                case UpdateType.ChatMember:
                    if (update.ChatMember != null)
                        await processChatMember(update.ChatMember, cancellationToken);
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
            bot = new TelegramBotClient(Token);
#else
            server = new TGBotFollowersStatApi("http://136.243.74.153:4000");
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

            //MessageProcessor = new MessageProcessor_v0(geotag, bot);
            MessageProcessor = messageProcessorFactory.Get(Type, Geotag, Token, bot);

            MessageProcessor.UpdateMessageRequestEvent += async (code, description) =>
            {
                AwaitedMessageCode = code;
                state = State.waiting_new_message;

                var operators = operatorsProcessor.GetAll(geotag).Where(o => o.permissions.Any(p => p.type.Equals(OperatorPermissionType.all)));

                foreach (var op in operators)
                {
                    try
                    {
                        await bot.SendTextMessageAsync(op.tg_id, $"Перешлите сообщение для: \n{description.ToLower()}");
                    }
                    catch (Exception ex)
                    {
                        logger.err("BOT", $"UpdateMessageRequestEvent: {ex.Message}");
                    }
                }
            };

            MessageProcessor.ShowMessageRequestEvent += async (message, code) =>
            {
                var operators = operatorsProcessor.GetAll(geotag).Where(o => o.permissions.Any(p => p.type.Equals(OperatorPermissionType.all)));                

                foreach (var op in operators)
                {
                    try
                    {
                        int id = await message.Send(op.tg_id, bot);
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

        public async Task<bool> Push(long id, string code, int notification_id)
        {
            bool res = false;
            try
            {
                string status = string.Empty;
                string uuid = string.Empty;

                (uuid, status) = await server.GetFollowerState(geotag, id);

                var push = messageProcessor.GetPush(code, Link, PM, uuid, Channel, false);

                if (push != null)
                {
                    try
                    {

                        await push.Send(id, bot);
                        res = true;
                        logger.inf(Geotag, $"PUSHED: {id} {status} {code}");

                    } catch (Exception ex)
                    {                 
                    } finally
                    {                       
                        await server.SlipPush(notification_id, res);
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
