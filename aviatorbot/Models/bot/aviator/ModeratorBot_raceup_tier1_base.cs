using aksnvl.messaging;
using asknvl.logger;
using asknvl.messaging;
using asknvl.server;
using Avalonia.X11;
using aviatorbot.Models.bot;
using aviatorbot.Models.messages.latam;
using botplatform.Models.server;
using botservice.Model.bot;
using botservice.Models.messages;
using botservice.Models.storage;
using botservice.Operators;
using botservice.rest;
using csb.invitelinks;
using csb.server;
using DynamicData;
using JetBrains.Annotations;
using motivebot.Model.storage;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace botservice.Models.bot.aviator
{
    public abstract class ModeratorBot_raceup_tier1_base : BotBase, IPushObserver
    {
        #region vars
        IMessageProcessorFactory messageProcessorFactory;
        BotModel tmpBotModel;

        Dictionary<long, int> pushStartCounters = new Dictionary<long, int>();
        List<pushStartProcess> pushStartProcesses = new List<pushStartProcess>();
        object lockObject = new object();
        IInviteLinksProcessor linksProcessor;
        ITGFollowerTrackApi api;
        IAIserver ai;
        long channelID;

        Dictionary<long, string> uuids = new Dictionary<long, string>();
        #endregion

        #region properties
        string? pm;
        public string? PM
        {
            get => pm;
            set => this.RaiseAndSetIfChanged(ref pm, value);
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

        string link_reg;
        public string LinkReg
        {
            get => link_reg;
            set => this.RaiseAndSetIfChanged(ref link_reg, value);
        }

        string link_dep;
        public string LinkDep
        {
            get => link_dep;
            set => this.RaiseAndSetIfChanged(ref link_dep, value);
        }

        string link_gam;
        public string LinkGam
        {
            get => link_gam;
            set => this.RaiseAndSetIfChanged(ref link_gam, value);
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

        protected ModeratorBot_raceup_tier1_base(BotModel model, IOperatorStorage operatorStorage, IBotStorage botStorage, ILogger logger) : base(model, operatorStorage, botStorage, logger)
        {
            ChannelTag = model.channel_tag;
            Channel = model.channel;            
            PM = model.pm;

            LinkReg = model.link_reg;
            LinkDep = model.link_dep;
            LinkGam = model.link_gam;

            api = new TGFollowerTrackApi_v1("https://app.flopasda.site");
            ai = new AIServer("https://gpt.raceup.io");

            #region commands
            editCmd = ReactiveCommand.Create(() =>
            {

                tmpBotModel = new BotModel()
                {
                    type = Type,
                    geotag = Geotag,
                    token = Token,
                    pm = PM,
                    channel_tag = ChannelTag,
                    channel = Channel,
                    link_reg = LinkReg,
                    link_dep = LinkDep,
                    link_gam = LinkGam
                };

                IsEditable = true;
            });

            cancelCmd = ReactiveCommand.Create(() =>
            {

                Geotag = tmpBotModel.geotag;
                Token = tmpBotModel.token;

                PM = tmpBotModel.pm;
                ChannelTag = tmpBotModel.channel_tag;
                Channel = tmpBotModel.channel;

                LinkReg = tmpBotModel.link_reg;
                LinkDep = tmpBotModel.link_dep;
                LinkGam = tmpBotModel.link_gam;

                IsEditable = false;

            });

            saveCmd = ReactiveCommand.Create(() =>
            {
                var updateModel = new BotModel()
                {
                    type = Type,
                    geotag = Geotag,
                    token = Token,
                    pm = PM,
                    channel_tag = ChannelTag,
                    channel = Channel,
                    
                    link_reg = LinkReg,
                    link_dep= LinkDep,
                    link_gam = LinkGam
                };

                botStorage.Update(updateModel);

                IsEditable = false;

            });
            #endregion
        }

        #region private
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

        
        #endregion

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
            } finally
            {
                var msg = $"{direction}: {chat} {fn} {ln} {un} {uuid} {status}";
                logger.inf(Geotag, msg); // logout JOIN or LEFT
            }
        }
        protected override async Task processFollower(Message message)
        {

            if (message == null || string.IsNullOrEmpty(message.Text))
                return;

            string userInfo = "";

            try
            {
                long chat = message.Chat.Id;
                var fn = message.From.FirstName;
                var ln = message.From.LastName;
                var un = message.From.Username;
                bool is_new = true;

                var found = pushStartProcesses.FirstOrDefault(p => p.chat == chat);
                if (found != null)
                {
                    var idsToDelete = found.messageIds.ToArray();                    
                    foreach (var id in idsToDelete)
                    {
                        try
                        {
                            await bot.DeleteMessageAsync(chat, id);
                        } catch (Exception ex)
                        {
                        }
                    }

                    found.stop();
                    await Task.Delay(100);
                    lock (lockObject)
                    {
                        pushStartProcesses.Remove(found);
                        logger.dbg(Geotag, $"{chat} > pushStartProcess removed total={pushStartProcesses.Count}");
                    }
                }

                if (!pushStartCounters.ContainsKey(chat))
                {
                    pushStartCounters.Add(chat, 0);
                    is_new = true;
                }
                else
                {
                    var cnt = pushStartCounters[chat];
                    cnt++;
                    cnt %= MessageProcessor.start_push_number;
                    pushStartCounters[chat] = cnt;
                    is_new = false;
                }

                if (is_new)
                {
                    List<Follower> followers = new();
                    var follower = new Follower()
                    {
                        tg_chat_id = ID,
                        tg_user_id = message.From.Id,
                        username = un,
                        firstname = fn,
                        lastname = ln,
                        office_id = (int)Offices.KRD,
                        tg_geolocation = Geotag,
                        fb_event_send = false,
                        is_subscribed = true
                    };
                    followers.Add(follower);

                    try
                    {
                        await server.UpdateFollowers(followers);
                        logger.inf_urgent(Geotag, $"BTJOINED: {Geotag} {chat} {fn} {ln} {un}");
                    }
                    catch (Exception ex)
                    {
                        logger.err(Geotag, $"{userInfo} DB ERROR {ex.Message}");
                    }
                }

                userInfo = $"{chat} {fn} {ln} {un}";

                var index = MessageProcessor.hi_outs.IndexOf(message.Text);
                if (index == -1)
                    //index = 0;
                    index = pushStartCounters[chat];

                try
                {
                    string uuid = "";
                    if (uuids.ContainsKey(chat))
                        uuid = uuids[chat]; 

                    var m = MessageProcessor.GetMessage($"hi_out", pm: PM, link: LinkReg, uuid: uuid);
                    checkMessage(m, $"hi_out", "processFollower");
                    await m.Send(chat, bot);
                    pushStartCounters[chat] = index;
                }
                catch (Exception ex)
                {
                    logger.err(Geotag, $"processFollower {ex.Message}");
                }

            }
            catch (Exception ex)
            {
            }
        }

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

                        string? uuid = "";
                        if (uuids.ContainsKey(chat))
                            uuid = uuids[chat]; 

                        var newProcess = new pushStartProcess(Geotag, chat, bot, MessageProcessor, logger, checkMessage, pm: PM, link: LinkReg, uuid: uuid);
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

                //string uuid = "";
                //string status = "";

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

                                try
                                {
                                    if (!uuids.ContainsKey(user_id))
                                    {

                                        var lnk = await ai.GetLink(ChannelTag, user_id);
                                        var uuid = lnk.uuid;

                                        //(var uuid, var status) = await server.GetFollowerState(ChannelTag, user_id);
                                        uuids.Add(user_id, uuid);
                                    }
                                } catch (Exception ex)
                                {
                                    logger.err(Geotag, $"GetLink subscribe: {user_id} {ex.Message}");
                                }


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
                            string uuid = "";
                            if (uuids.ContainsKey(user_id))
                            {
                                uuid = uuids[user_id];
                                uuids.Remove(user_id);
                            }
                            else
                            {
                                try
                                {
                                    var lnk = await ai.GetLink(ChannelTag, user_id);
                                    uuid = lnk.uuid;
                                }
                                catch (Exception ex)
                                {
                                    logger.err(Geotag, $"GetLink unsubscribe: {user_id} {ex.Message}");
                                }
                            }

                            var m = MessageProcessor.GetMessage($"BYE", pm: PM, channel: Channel, link: LinkReg, uuid: uuid);
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
        protected override async Task processCallbackQuery(CallbackQuery query)
        {
            await Task.CompletedTask;
        }
        #endregion

        #region public
        public override Task Start()
        {
            return base.Start().ContinueWith(async (t) => {

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

            });
        }
        public override void Stop()
        {
            base.Stop();
        }
        public override Task Notify(object notifyObject)
        {
            throw new NotImplementedException();
        }
        public async Task<bool> Push(long id, string code, string _uuid, int notification_id, string? firstname)
        {
            var op = operatorStorage.GetOperator(Geotag, id);
            if (op != null)
            {
                logger.err(Geotag, $"Push: {id} Попытка отправки пуша оператору");
                return false;
            }

            bool res = false;
            try
            {

                StateMessage push = null;
                string? uuid = "";

                try
                {
                    if (!uuids.ContainsKey(id))
                    {
                        //(uuid, var status) = await server.GetFollowerState(ChannelTag, id);

                        var lnk = await ai.GetLink(ChannelTag, id);
                        uuid = lnk.uuid;
                        uuids.Add(id, uuid);
                    }
                }
                catch (Exception ex)
                {
                    logger.err(Geotag, $"GetFollowerState push: {id} {ex.Message}");
                }

                try
                {

                    if (uuids.ContainsKey(id))
                        uuid = uuids[id];   

                    var tmp = MessageProcessor.GetPush(code, pm: PM, link: LinkReg, uuid: uuid);

                    if (!string.IsNullOrEmpty(firstname))
                    {
                        List<AutoChange> autoChange = new List<AutoChange>()
                        {
                            new AutoChange() {
                                OldText = "_fn_",
                                NewText = $"{firstname}"
                            }
                        };

                        push = tmp.Clone();
                        push.MakeAutochange(autoChange);
                    }
                    else
                        push = tmp;

                    if (push != null)
                    {
                        if (push.Message.Text != null && push.Message.Text.Contains("_fn_"))
                        {
                            int len = Math.Min(push.Message.Text.Length - 1, 20);
                            logger.err(Geotag, $"AutochangeErr msg: {id} {firstname} {push.Message.Text.Substring(0, len)}...");
                            errCollector.Add($"{code} ошибка автозамены имени лида id={id} fn={firstname}");
                        }

                        if (push.Message.Caption != null && push.Message.Caption.Contains("_fn_"))
                        {
                            int len = Math.Min(push.Message.Caption.Length - 1, 20);
                            logger.err(Geotag, $"AutochangeErr cap: {id} {firstname} {push.Message.Caption.Substring(0, len)}...");
                            errCollector.Add($"{code} ошибка автозамены имени лида id={id} fn={firstname}");
                        }
                    }

                    checkMessage(push, code, "Push");
                }
                catch (Exception ex)
                {
                    logger.err(Geotag, $"Push: {id} {code} {firstname} {ex.Message} (0)");
                    errCollector.Add($"{code} ошибка разметки");
                    await server.SlipPush(notification_id, false);
                }

                if (push != null)
                {
                    try
                    {
                        await push.Send(id, bot);
                        res = true;
                        logger.inf(Geotag, $"PUSHED: {id} {code} {firstname}");

                    }
                    catch (Exception ex)
                    {
                        logger.err(Geotag, $"Push: {id} {code} {firstname} {ex.Message} (1)");

                    } finally
                    {
                        await server.SlipPush(notification_id, res);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.err(Geotag, $"Push: {id} {code} {firstname} {ex.Message} (2)");
                await server.SlipPush(notification_id, false);
            }
            return res;
        }
        #endregion

        #region push start
        class pushStartProcess
        {
            #region vars        
            CancellationTokenSource cts;
            ITelegramBotClient bot;
            MessageProcessorBase mp;
            ILogger logger;
            string geotag;
            Action<PushMessageBase?, string, string> checkMessage;            
            string? pm, link, uuid;
            #endregion

            #region properties
            public long chat { get; set; }
            public bool is_running { get; set; }
            public List<int> messageIds { get; } = new List<int>();
            #endregion

            public pushStartProcess(string geotag,
                                    long chat,
                                    ITelegramBotClient bot,
                                    MessageProcessorBase mp,
                                    ILogger logger,                                    
                                    Action<PushMessageBase?, string, string> checkMessage,
                                    string? pm,
                                    string? link,
                                    string? uuid)
            {
                this.geotag = geotag;
                this.chat = chat;
                this.bot = bot;
                this.mp = mp;
                this.logger = logger;
                this.checkMessage = checkMessage;

                this.pm = pm;
                this.link = link;
                this.uuid = uuid;

                cts = new CancellationTokenSource();
            }

            async void worker()
            {
                is_running = true;

                try
                {
                    int id_prev = -1;

                    for (int i = 0; i < mp.start_push_number; i++)
                    {
                        try
                        {
                            cts.Token.ThrowIfCancellationRequested();

                            PushMessageBase m = null;
                            ReplyKeyboardMarkup b = null;

                            (m, b) = mp.GetMessageAndReplyMarkup($"hi_{i}_in", pm: pm, link: link, uuid: uuid);
                            checkMessage(m, $"hi_{i}_in", "pushStartProcess");

                            if (id_prev != -1 && i <= mp.start_push_number - 1)
                            {
                                try
                                {
                                    await bot.DeleteMessageAsync(chat, id_prev);
                                } catch (Exception ex) { }
                            }

                            var id = await m.Send(chat, bot, b);                            
                            messageIds.Add(id);
                            id_prev = id;         
                            
                            logger.dbg(geotag, $"{chat} > pushStartProcess sent {i}");
                            await Task.Delay(40000, cancellationToken: cts.Token);
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
        #endregion
    }

}
