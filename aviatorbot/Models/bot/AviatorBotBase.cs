using aksnvl.messaging;
using asknvl.logger;
using asknvl.server;
using Avalonia.X11;
using aviatorbot.Models.bot;
using aviatorbot.Models.messages;
using aviatorbot.ViewModels;
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
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace aviatorbot.Model.bot
{
    public abstract class AviatorBotBase : ViewModelBase, IAviatorBot, IPushObserver
    {
        #region vars
        protected ILogger logger;
        protected ITelegramBotClient bot;
        protected CancellationTokenSource cts;
        protected IMessageProcessor messageProcessor;
        protected State state = State.free;
        protected ITGBotFollowersStatApi server;
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
        #endregion

        #region commands
        public ReactiveCommand<Unit, Unit> startCmd { get; }
        public ReactiveCommand<Unit, Unit> stopCmd { get; }
        #endregion

        public AviatorBotBase(ILogger logger)
        {
            this.logger = logger;

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

            List<Follower> followers = new();
            var follower = new Follower()
            {
                tg_chat_id = 0,
                tg_user_id = message.From.Id,
                username = message.From.Username,
                firstname = message.From.FirstName,
                lastname = message.From.LastName,
                office_id = (int)Offices.KRD,
                tg_geolocation = Geotag
            };
            followers.Add(follower);


            try
            {
                await server.UpdateFollowers(followers);
            } catch (Exception ex)
            {
                logger.err(Geotag, ex.Message);
            }

            string uuid = "0";
            string status = "WREDEP2";

            try
            {
                (uuid, status) = await server.GetFollowerState(chat);

                string msg = $"JOINED: {follower.tg_user_id} {follower.firstname} {follower.lastname} {follower.username} {uuid} {status}";
                logger.inf(Geotag, msg);


            } catch (Exception ex)
            {
                logger.err(Geotag, ex.Message); 
            }

            if (message.Text.Equals("/start"))
            {
                var m = await messageProcessor.GetMessage(status, Link, PM, uuid, Channel, false);


                int id = 0;

                try
                {
                    id = await m.Send(chat, bot, null);
                } catch (Exception ex)
                {
                    logger.err(Geotag, ex.Message);
                }

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
            }
        }

        async Task processCallbackQuery(CallbackQuery query)
        {
            long chat = query.Message.Chat.Id;
            PushMessageBase message = null;

            string uuid = "0";
            string status = "WREDEP2";

            try
            {
                (uuid, status) = await server.GetFollowerState(chat);

                string msg = $"STATUS: {chat} {uuid} {status}";
                logger.inf(Geotag, msg);
            }
            catch (Exception ex)
            {
                logger.err(Geotag, ex.Message);
            }

            switch (query.Data)
            {
                case "check_register":

                    if (status.Equals("WREG"))
                    {
                        message = await messageProcessor.GetMessage(status, Link, PM, uuid, Channel, true);
                    }
                    else
                        message = await messageProcessor.GetMessage(status, Link, PM, uuid, Channel, false);
                    break;

                case "check_fd":
                    if (status.Equals("WFDEP"))
                    {
                        message = await messageProcessor.GetMessage(status, Link, PM, uuid, Channel, true);
                    } else
                        message = await messageProcessor.GetMessage(status, Link, PM, uuid, Channel, false);
                    break;

                case "check_rd1":
                    if (status.Equals("WREDEP1"))
                    {
                        message = await messageProcessor.GetMessage(status, Link, PM, uuid, Channel, true);
                    }
                    else
                        message = await messageProcessor.GetMessage(status, Link, PM, uuid, Channel, false);

                    break;

                default:
                    break;
            }

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

            await bot.AnswerCallbackQueryAsync(query.Id);
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
                        messageProcessor.Clear();
                        state = State.waiting_new_message;
                        return;
                    }
                }

                switch (state)
                {
                    case State.waiting_new_message:
                        messageProcessor.Add(message, PM);
                        break;
                }
            } catch (Exception ex)
            {
                logger.err(Geotag, ex.Message);
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
                logger.err("BOT", "Bot already started");
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
                logger.inf(Geotag, "Bot started");

            } catch (Exception ex)
            {

            }            
        }

        public virtual void Stop()
        {
            IsActive = false;
        }

        public string GetGeotag()
        {
            return Geotag;
        }

        public async Task Push(long id, PushType type)
        {
            try
            {



            } catch (Exception ex)
            {

            }
        }
        #endregion
    }
}
