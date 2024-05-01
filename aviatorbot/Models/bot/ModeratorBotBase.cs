using aksnvl.messaging;
using asknvl.logger;
using asknvl.server;
using aviatorbot.Models.bot;
using aviatorbot.Models.messages.latam;
using botservice.Model.bot;
using botservice.Models.bot;
using botservice.Models.messages;
using botservice.Models.storage;
using botservice.Operators;
using csb.invitelinks;
using csb.server;
using DynamicData;
using motivebot.Model.storage;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace botservice.Models.bot.aviator
{
    public abstract class ModeratorBotBase : BotBase 
    {
        #region vars        
        IMessageProcessorFactory messageProcessorFactory;
        BotModel tmpBotModel;
        protected Dictionary<long, int> pushStartCounters = new Dictionary<long, int>();        
        protected List<pushStartProcess> pushStartProcesses = new List<pushStartProcess>();
        protected object lockObject = new object();

        ITGFollowerTrackApi api;
        long channelID;
        IInviteLinksProcessor linksProcessor;
        #endregion

        #region properties
        string? pm;
        public string? PM
        {
            get => pm;
            set => this.RaiseAndSetIfChanged(ref pm, value);
        }

        string? link;
        public string? Link
        {
            get => link;
            set => this.RaiseAndSetIfChanged(ref link, value);
        }


        string? channel_tag;
        public string? ChannelTag
        {
            get => channel_tag;
            set => this.RaiseAndSetIfChanged(ref channel_tag, value);
        }

        string? channel;
        public string? Channel
        {
            get => channel;
            set => this.RaiseAndSetIfChanged(ref channel, value);
        }

        bool? chApprove;
        public bool? ChApprove
        {
            get => chApprove;
            set => this.RaiseAndSetIfChanged(ref chApprove, value); 
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

        protected ModeratorBotBase(BotModel model, IOperatorStorage operatorStorage, IBotStorage botStorage, ILogger logger) : base(model, operatorStorage, botStorage, logger)
        {

            api = new TGFollowerTrackApi_v1("https://app.flopasda.site");

            Geotag = model.geotag;
            Token = model.token;

            Link = model.link;

            ChannelTag = model.channel_tag;
            Channel = model.channel;
            ChApprove = model.channel_approve;
            PM = model.pm;

            #region commands
            editCmd = ReactiveCommand.Create(() =>
            {

                tmpBotModel = new BotModel()
                {
                    type = Type,
                    geotag = Geotag,
                    token = Token,

                    link = Link,
                    pm = PM,

                    channel_tag = ChannelTag,
                    channel = Channel,
                    channel_approve = ChApprove
                };

                IsEditable = true;
            });

            cancelCmd = ReactiveCommand.Create(() =>
            {

                Geotag = tmpBotModel.geotag;
                Token = tmpBotModel.token;
                
                Link = tmpBotModel.link;
                PM = tmpBotModel.pm;
                ChannelTag = tmpBotModel.channel_tag;
                Channel = tmpBotModel.channel;
                ChApprove = tmpBotModel.channel_approve;

                IsEditable = false;

            });

            saveCmd = ReactiveCommand.Create(() =>
            {
                var updateModel = new BotModel()
                {
                    type = Type,
                    geotag = Geotag,
                    token = Token,  
                    link = Link,
                    pm = PM,
                    channel_tag = ChannelTag,
                    channel = Channel,
                    channel_approve = ChApprove                    
                };

                botStorage.Update(updateModel);

                IsEditable = false;

            });
            #endregion
        }

        #region override
        protected override async Task processOperator(Message message, Operator op)
        {
            try
            {
                if (state == State.waiting_new_message)
                {
                    MessageProcessor.Add(AwaitedMessageCode, message, PM, channel: Channel);
                    state = State.free;
                    return;
                }
            }
            catch (Exception ex)
            {
                logger.err(Geotag, ex.Message);
            }
        }

        override protected async Task processSubscribe(Update update)
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
                            follower.is_subscribed = true;
                            direction = "UNBLOCK";
                            followers.Add(follower);
                            await server.UpdateFollowers(followers);
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

                    (uuid, status) = await server.GetFollowerState(Geotag, chat);
                }

            }
            catch (Exception ex)
            {
                logger.err(Geotag, $"processSubscribe: {ex.Message}");
            }
            finally
            {
                var msg = $"{direction}: {chat} {fn} {ln} {un} {uuid} {status}";
                logger.inf(Geotag, msg); // logout JOIN or LEFT
            }
        }

        protected virtual async Task<(string, bool)> getUserStatusOnStart(long tg_id)
        {
            string code = "";
            bool is_new = false;

            try
            {

                var subscribe = await server.GetFollowerSubscriprion(Geotag, tg_id);
                var isSubscribed = subscribe.Any(s => s.is_subscribed);

                code = "start";
                is_new = !isSubscribed;

            }
            catch (Exception ex)
            {
                logger.err(Geotag, $"getUserStatus: {ex.Message}");
            }

            return (code, is_new);
        }

        //начало стартовых пушей, аппрув в канал
        int appCntr = 0;
        int decCntr = 0;
        protected override async Task processChatJoinRequest(ChatJoinRequest chatJoinRequest, CancellationToken cancellationToken)
        {
            var chat = chatJoinRequest.From.Id;
            bool isAllowed = true;

            try
            {

                isAllowed = await server.IsSubscriptionAvailable(ChannelTag, chat);

            }
            catch (Exception ex)
            {
                logger.err(Geotag, $"processChatJoinRequest checkSubs: {ChannelTag} {chat} {ex.Message}");
                errCollector.Add(errorMessageGenerator.getCheckSubscriprionAvailableError(ChannelTag, chat, ex));
            }

            try
            {

                if (isAllowed)
                {

                    var found = pushStartProcesses.FirstOrDefault(p => p.chat == chat);
                    if (found == null)
                    {
                        var newProcess = new pushStartProcess(Geotag, chat, bot, MessageProcessor, logger, checkMessage);
                        lock (lockObject)
                        {
                            pushStartProcesses.Add(newProcess);
                        }
                        newProcess.start();
                    }

                    await bot.ApproveChatJoinRequest(chatJoinRequest.Chat.Id, chatJoinRequest.From.Id);
                    logger.inf_urgent(Geotag, $"CHREQUEST: ({++appCntr}) " +
                                    $"{chatJoinRequest.InviteLink?.InviteLink} " +
                                    $"{chatJoinRequest.From.Id} " +
                                    $"{chatJoinRequest.From.FirstName} " +
                                    $"{chatJoinRequest.From.LastName} " +
                                    $"{chatJoinRequest.From.Username}");

                }
                else
                {
                    await bot.DeclineChatJoinRequest(chatJoinRequest.Chat.Id, chatJoinRequest.From.Id);
                    logger.err(Geotag, $"DECLINED: ({++decCntr}) " +
                                    $"{chatJoinRequest.InviteLink?.InviteLink} " +
                                    $"{chatJoinRequest.From.Id} " +
                                    $"{chatJoinRequest.From.FirstName} " +
                                    $"{chatJoinRequest.From.LastName} " +
                                    $"{chatJoinRequest.From.Username}");

                    errCollector.Add($"Отказ на подписку в канал {ChannelTag}: {chatJoinRequest.From.Id} {chatJoinRequest.From.FirstName} {chatJoinRequest.From.LastName} {chatJoinRequest.From.Username}");

                    try
                    {
                        await server.MarkFollowerWasDeclined(ChannelTag, chat);

                    }
                    catch (Exception ex)
                    {
                        logger.err(Geotag, $"processChatJoinRequest declineSubs: {ChannelTag} {chat} {ex.Message}");
                        errCollector.Add(errorMessageGenerator.getSubscriptionDeclinedError(ChannelTag, chat, ex));
                    }
                }

                await linksProcessor.Revoke(channelID, chatJoinRequest.InviteLink.InviteLink);

            }
            catch (Exception ex)
            {
                logger.err(Geotag, $"processChatJoinRequest {ex.Message}");
            }
        }

        //регистрация подписки/в канал в канал в бд
        protected override async Task processChatMember(Update update, CancellationToken cancellationToken)
        {
            try
            {

                if (update.ChatMember == null)
                    return;

                var member = update.ChatMember;
                long user_id = member.NewChatMember.User.Id;
                long chat_id = update.ChatMember.Chat.Id;

                string fn = member.NewChatMember.User.FirstName;
                string ln = member.NewChatMember.User.LastName;
                string un = member.NewChatMember.User.Username;

                string uuid = "";

                string link = member.InviteLink?.InviteLink;

                List<Follower> followers = new();
                var follower = new Follower()
                {
                    tg_chat_id = chat_id,
                    tg_user_id = user_id,
                    username = un,
                    firstname = fn,
                    lastname = ln,
                    invite_link = link,
                    office_id = (int)Offices.KRD,
                    tg_geolocation = ChannelTag
                };

                switch (member.NewChatMember.Status)
                {
                    case ChatMemberStatus.Member:

                        if (member.InviteLink != null && member.InviteLink.CreatesJoinRequest)
                        {
                            follower.is_subscribed = true;
                            follower.fb_event_send = true;

                            try
                            {
                                followers.Add(follower);
                                await server.UpdateFollowers(followers);
                            }
                            catch (Exception ex)
                            {
                                logger.err(Geotag, $"processChatMember: JOIN DB ERROR {user_id} {ex.Message}");
                            }

                            logger.inf_urgent(Geotag, $"CHJOINED: {ChannelTag} {link} {user_id} {fn} {ln} {un} event={follower.fb_event_send}");

                        }
                        break;

                    case ChatMemberStatus.Left:

                        try
                        {
                            var m = MessageProcessor.GetMessage($"BYE", pm: PM, link: Link);
                            checkMessage(m, $"BYE", "processChatMember");
                            await m.Send(user_id, bot);
                        }
                        catch (Exception ex)
                        {
                            logger.err(Geotag, $"processChatMember {ex.Message}");
                        }

                        follower.is_subscribed = false;
                        followers.Add(follower);

                        try
                        {
                            await server.UpdateFollowers(followers);
                        }
                        catch (Exception ex)
                        {
                            logger.err(Geotag, $"processChatMember: LEFT DB ERROR {user_id}");
                        }

                        logger.inf(Geotag, $"CHLEFT: {Channel} {user_id} {fn} {ln} {un}");
                        break;
                }
            }
            catch (Exception ex)
            {
                logger.err(Geotag, $"processChatMember: {ex.Message}");
            }
        }        
        public override async Task Notify(object notifyObject)
        {
            await Task.CompletedTask;
        }
        #endregion

        #region public
        public override async Task Start()
        {
            await base.Start().ContinueWith(t =>
            {

                messageProcessorFactory = new MessageProcessorFactory(logger);

                MessageProcessor = messageProcessorFactory.Get(Type, Geotag, Token, bot);

                if (MessageProcessor != null)
                {
                    MessageProcessor.UpdateMessageRequestEvent += async (code, description) =>
                    {
                        AwaitedMessageCode = code;
                        state = State.waiting_new_message;

                        var operators = operatorStorage.GetAll(Geotag).Where(o => o.permissions.Any(p => p.type.Equals(OperatorPermissionType.all)));

                        foreach (var op in operators)
                        {
                            try
                            {
                                await bot.SendTextMessageAsync(op.tg_id, $"Перешлите сообщение для: \n{description.ToLower()}");
                            }
                            catch (Exception ex)
                            {
                                logger.err(Geotag, $"UpdateMessageRequestEvent: {ex.Message}");
                            }
                        }
                    };

                    MessageProcessor.ShowMessageRequestEvent += async (message, code) =>
                    {
                        var operators = operatorStorage.GetAll(Geotag).Where(o => o.permissions.Any(p => p.type.Equals(OperatorPermissionType.all)));

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
                }

            });

            try
            {
                var channels = await ChannelsProvider.getInstance().GetChannels();
                var found = channels.FirstOrDefault(c => c.geotag == ChannelTag);
                if (found == null)
                {
                    logger.err(Geotag, $"GetChannels: No channel ID");
                    Stop();
                }
                else
                {
                    string schatID = $"-100{found.tg_id}";
                    long chatid = long.Parse(schatID);
                    channelID = chatid;

                    linksProcessor = new DynamicInviteLinkProcessor(ChannelTag, bot, api, logger);
                    linksProcessor.UpdateChannelID(channelID);
                    linksProcessor.StartLinkNumberControl(channelID, cts);
                }
            }
            catch (Exception ex)
            {
                Stop();
                logger.err(Geotag, $"GetChannels: {ex.Message}");
            }

        }        
        #endregion

        public class pushStartProcess
        {
            #region vars        
            CancellationTokenSource cts;
            ITelegramBotClient bot;
            MessageProcessorBase mp;
            ILogger logger;
            string geotag;
            Action<PushMessageBase?, string, string> checkMessage;
            #endregion

            #region properties
            public long chat { get; set; }
            public bool is_running { get; set; }
            #endregion

            public pushStartProcess(string geotag, long chat, ITelegramBotClient bot, MessageProcessorBase mp, ILogger logger, Action<PushMessageBase?, string, string> checkMessage)
            {
                this.geotag = geotag;
                this.chat = chat;
                this.bot = bot;
                this.mp = mp;
                this.logger = logger;
                this.checkMessage = checkMessage;


                cts = new CancellationTokenSource();
            }

            async void worker()
            {
                is_running = true;

                try
                {

                    for (int i = 0; i < mp.start_push_number; i++)
                    {
                        try
                        {
                            cts.Token.ThrowIfCancellationRequested();

                            PushMessageBase m = null;
                            ReplyKeyboardMarkup b = null;

                            (m, b) = mp.GetMessageAndReplyMarkup($"hi_{i}_in");
                            checkMessage(m, $"hi_{i}_in", "pushStartProcess");
                            await m.Send(chat, bot, b);
                            logger.dbg(geotag, $"{chat} > pushStartProcess sent {i}");
                            await Task.Delay(45000, cancellationToken: cts.Token);
                        }
                        catch (OperationCanceledException ex)
                        {
                            logger.dbg(geotag, $"{chat} > pushStartProcess stopped");
                            break;
                        }
                        catch (Exception ex)
                        {
                            logger.err(geotag, $"{chat} > pushStartProcess: unable to send start message {i}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.err(geotag, $"{chat} > worker {ex.Message}");
                } finally
                {
                    is_running = false;
                }
            }

            public void start()
            {
                Task.Run(() => worker());
            }
            public void stop()
            {
                cts?.Cancel();
            }
        }
    }
}
