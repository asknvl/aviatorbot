﻿using aksnvl.messaging;
using asknvl.logger;
using asknvl.messaging;
using asknvl.server;
using aviatorbot.Models.bot;
using botservice.Model.bot;
using botservice.Models.bot;
using botservice.Models.messages;
using botservice.Models.storage;
using botservice.Operators;
using botservice.rest;
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
using static asknvl.server.TGBotFollowersStatApi;

namespace botservice.Models.bot.aviator
{
    public abstract class LandingBot_raceup_tier1_postback_base : BotBase, IPushObserver, IStatusObserver
    {
        #region vars
        IMessageProcessorFactory messageProcessorFactory;
        BotModel tmpBotModel;
        Dictionary<long, int> prevRegIds = new();
        List<ChatJoinRequest> chatJoinRequests = new();
        System.Timers.Timer chatJoinRequestTimer = new();
        object chatJoinLock = new object();
        #endregion

        #region properties    
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

        string? support_pm;
        public string? SUPPORT_PM
        {
            get => support_pm;
            set => this.RaiseAndSetIfChanged(ref support_pm, value);
        }

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

        bool? chApprove = true;
        public bool? ChApprove
        {
            get => chApprove;
            set => this.RaiseAndSetIfChanged(ref chApprove, value);
        }

        string help;
        public string Help
        {
            get => help;
            set => this.RaiseAndSetIfChanged(ref help, value);
        }

        string training;
        public string Training
        {
            get => training;
            set => this.RaiseAndSetIfChanged(ref training, value);
        }

        string reviews;
        public string Reviews
        {
            get => reviews;
            set => this.RaiseAndSetIfChanged(ref reviews, value);
        }

        string strategy;
        public string Strategy
        {
            get => strategy;
            set => this.RaiseAndSetIfChanged(ref strategy, value);
        }

        string vip;
        public string Vip
        {
            get => vip;
            set => this.RaiseAndSetIfChanged(ref vip, value);
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

        public LandingBot_raceup_tier1_postback_base(BotModel model, IOperatorStorage operatorStorage, IBotStorage botStorage, ILogger logger) : base(model, operatorStorage, botStorage, logger)
        {
            this.logger = logger;
            this.operatorStorage = operatorStorage;
            this.botStorage = botStorage;

            Geotag = model.geotag;
            Token = model.token;

            LinkReg = model.link_reg;
            LinkDep = model.link_dep;
            LinkGam = model.link_gam;

            SUPPORT_PM = model.support_pm;
            PM = model.pm;
            ChannelTag = model.channel_tag;
            Channel = model.channel;
            ChApprove = model.channel_approve;

            Help = model.help;
            Training = model.training;
            Reviews = model.reveiews;
            Strategy = model.strategy;
            Vip = model.vip;

            Postbacks = model.postbacks;

            #region commands
            editCmd = ReactiveCommand.Create(() =>
            {

                tmpBotModel = new BotModel()
                {
                    type = Type,
                    geotag = Geotag,
                    token = Token,
                    link_reg = LinkReg,
                    link_dep = LinkDep,
                    link_gam = LinkGam,
                    support_pm = SUPPORT_PM,
                    pm = PM,
                    channel = Channel,
                    channel_approve = ChApprove,

                    help = Help,
                    training = Training,
                    reveiews = Reviews,
                    strategy = Strategy,
                    vip = Vip,

                    postbacks = Postbacks
                };

                IsEditable = true;
            });

            cancelCmd = ReactiveCommand.Create(() =>
            {

                Geotag = tmpBotModel.geotag;
                Token = tmpBotModel.token;

                LinkReg = tmpBotModel.link_reg;
                LinkDep = tmpBotModel.link_dep;
                LinkGam = tmpBotModel.link_gam;

                SUPPORT_PM = tmpBotModel.support_pm;
                PM = tmpBotModel.pm;

                Channel = tmpBotModel.channel;
                ChApprove = tmpBotModel.channel_approve;

                Help = tmpBotModel.help;
                Training = tmpBotModel.training;
                Reviews = tmpBotModel.reveiews;
                Strategy = tmpBotModel.strategy;
                Vip = tmpBotModel.vip;

                Postbacks = tmpBotModel.postbacks;

                IsEditable = false;

            });

            saveCmd = ReactiveCommand.Create(() =>
            {
                var updateModel = new BotModel()
                {
                    type = Type,
                    geotag = Geotag,
                    token = Token,
                    
                    link_reg = LinkReg,
                    link_dep = LinkDep,
                    link_gam = LinkGam,

                    support_pm = SUPPORT_PM,
                    pm = PM,
                    channel = Channel,
                    channel_approve = ChApprove,

                    help = Help,
                    training = Training,
                    reveiews = Reviews,
                    strategy = Strategy,
                    vip = Vip,

                    postbacks = Postbacks
                };

                botStorage.Update(updateModel);

                IsEditable = false;

            });
            #endregion
        }

        #region helpers
        protected virtual async Task<(string, bool)> getUserStatusOnStart(long tg_id)
        {
            string code = "start";
            bool is_new = false;

            try
            {
                var subscribe = await server.GetFollowerSubscriprion(Geotag, tg_id);
                var isSubscribed = subscribe.Any(s => s.is_subscribed);

                if (!isSubscribed)
                {
                    code = "start";
                    is_new = true;
                }
                else
                {

                    var statusResponce = await server.GetFollowerStateResponse(Geotag, tg_id);
                    var status = statusResponce.status_code;

                    switch (status)
                    {
                        case "WREG":
                            code = "start";
                            break;

                        default:
                            if (status.Contains("WFDEP"))
                                code = "WFDEP";
                            else
                            if (status.Contains("WREDEP"))
                                code = "WREDEP1";

                            else
                            {
                                code = "start";
                                logger.err(Geotag, $"getUserStatusOnStart: {tg_id} udefined status");
                            }
                            break;
                    }
                }

            }
            catch (Exception ex)
            {
                logger.err(Geotag, $"getUserStatus: {ex.Message}");
                errCollector.Add(errorMessageGenerator.getUserStatusOnStartError(ex));
            }

            return (code, is_new);
        }
        protected async Task clearPrevId(long chat, int id)
        {
            if (prevRegIds.ContainsKey(chat))
            {

                try
                {
                    await bot.DeleteMessageAsync(chat, prevRegIds[chat]);
                }
                catch (Exception ex)
                {
                }

                prevRegIds[chat] = id;
            }
            else
                prevRegIds.Add(chat, id);
        }
        #endregion

        #region override
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

                userInfo = $"{chat} {fn} {ln} {un}";

                if (message.Text.Contains("/start"))
                {

                    var parse_uuid = message.Text.Replace("/start", "").Trim();
                    var uuid = string.IsNullOrEmpty(parse_uuid) ? null : parse_uuid;

                    var msg = $"START: {userInfo} ?";
                    logger.inf(Geotag, msg);

                    string code = "";
                    bool is_new = false;

                    (code, is_new) = await getUserStatusOnStart(chat);

                    bool need_fb_event = is_new && !string.IsNullOrEmpty(uuid);

                    if (code.Equals("start"))
                    {

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
                            uuid = uuid,
                            fb_event_send = need_fb_event,
                            is_subscribed = true
                        };
                        followers.Add(follower);

                        try
                        {
                            await server.UpdateFollowers(followers);
                            msg = $"DB UPDATED: {userInfo} uuid={uuid} event={follower.fb_event_send}";
                            logger.inf(Geotag, msg);
                        }
                        catch (Exception ex)
                        {
                            logger.err(Geotag, $"{userInfo} DB ERROR {ex.Message}");
                            errCollector.Add(errorMessageGenerator.getAddUserBotDBError(userInfo));
                        }
                    }

                    if (uuid == null)
                    {
                        try
                        {
                            var statusResponce = await server.GetFollowerStateResponse(Geotag, chat);
                            uuid = statusResponce.uuid;
                        }
                        catch (Exception ex)
                        {
                            logger.err(Geotag, $"{userInfo} GET UUID {ex.Message}");
                        }

                        msg = $"STARTED: {userInfo} uuid={uuid}";

                        if (uuid != null)
                            logger.inf(Geotag, msg);
                        else
                            logger.err(Geotag, msg);
                    }
                    else
                    {
                        msg = $"STARTED: {userInfo} uuid={uuid}";
                        logger.inf_urgent(Geotag, msg);
                    }

                    string sLink = LinkReg;
                    switch (code)
                    {
                        case "WREG":
                            sLink = LinkReg;
                            break;

                        default:
                            sLink = LinkDep;
                            break;
                    }

                    var m = MessageProcessor.GetMessage(code,
                                                        link: sLink,
                                                        support_pm: SUPPORT_PM,
                                                        pm: PM,
                                                        uuid: uuid,
                                                        channel: Channel,
                                                        help: Help,
                                                        vip: Vip
                                                        );

                    checkMessage(m, code, "/start");

                    int id = await m.Send(chat, bot);

                    if (code.Equals("start"))
                    {
                        var _ = Task.Run(async () =>
                        {
                            string _uuid = (uuid != null) ? uuid : "undf";

                            try
                            {
                                await Task.Delay(5000);

                                m = MessageProcessor.GetMessage("circle", channel: Channel);

                                checkMessage(m, "/start", "circle");

                                await m.Send(chat, bot);

                                await Task.Delay(10000);

                                m = MessageProcessor.GetMessage("video");

                                checkMessage(m, "/start", "video");

                                await m.Send(chat, bot);
                            }
                            catch (Exception ex)
                            {
                                logger.err(Geotag, $"circle&video error");
                            }
                        });
                    }

                    logger.dbg(Geotag, $"{userInfo}");

                }
                else
                {
                    //var resp = await server.GetFollowerStateResponse(Geotag, chat);
                    var msg = $"TEXT: {userInfo}\n{message.Text}";
                    logger.inf(Geotag, msg);
                }


            }
            catch (Exception ex)
            {
                logger.err(Geotag, $"processFollower: {userInfo} {ex.Message}");
                errCollector.Add(errorMessageGenerator.getStartUserError(userInfo));
            }
        }
        protected override async Task processOperator(Message message, Operator op)
        {

            var chat = message.From.Id;

            try
            {
                if (state == State.waiting_new_message)
                {
                    MessageProcessor.Add(AwaitedMessageCode,
                                         message, PM,
                                         channel: Channel,
                                         support_pm: SUPPORT_PM,
                                         help: Help,
                                         training: Training,
                                         reviews: Reviews,
                                         strategy: Strategy,
                                         vip: Vip);
                    state = State.free;
                    return;
                }
            }
            catch (Exception ex)
            {
                logger.err(Geotag, ex.Message);
                errCollector.Add(errorMessageGenerator.getOpertatorProcessError(chat, ex));
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

        int reqCntr = 0;
        int appCntr = 0;
        protected override async Task processChatJoinRequest(ChatJoinRequest chatJoinRequest, CancellationToken cancellationToken)
        {
            var chat = chatJoinRequest.From.Id;
            var fn = chatJoinRequest.From.FirstName;
            var ln = chatJoinRequest.From.LastName;
            var un = chatJoinRequest.From.Username;

            string userinfo = $"{Channel} {chat} {fn} {ln} {un}";

            lock (chatJoinLock)
            {
                var found = chatJoinRequests.Any(r => r.From.Id == chatJoinRequest.From.Id);
                if (!found)
                {
                    chatJoinRequests.Add(chatJoinRequest);
                    logger.inf_urgent(Geotag, $"CHREQUEST ENQUEUED: ({++reqCntr}) {userinfo}");
                }
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

                        follower.is_subscribed = true;

                        try
                        {
                            followers.Add(follower);
                            await server.UpdateFollowers(followers);
                        }
                        catch (Exception ex)
                        {
                            logger.err(Geotag, $"processChatMember: JOIN DB ERROR {user_id}");
                            errCollector.Add(errorMessageGenerator.getAddUserChatDBError(user_id, ChannelTag));
                        }

                        logger.inf_urgent(Geotag, $"CHJOINED: {Channel} {user_id} {fn} {ln} {un}");
                        break;

                    case ChatMemberStatus.Left:

                        follower.is_subscribed = false;
                        followers.Add(follower);

                        try
                        {
                            await server.UpdateFollowers(followers);
                        }
                        catch (Exception ex)
                        {
                            logger.err(Geotag, $"processChatMember: LEFT DB ERROR {user_id}");
                            errCollector.Add(errorMessageGenerator.getRemoveUserChatDBError(user_id, ChannelTag));
                        }

                        logger.inf(Geotag, $"CHLEFT: {Channel} {user_id} {fn} {ln} {un}");
                        break;
                }
            }
            catch (Exception ex)
            {
                logger.err(Geotag, $"processChatMember: {ex.Message}");
                errCollector.Add(errorMessageGenerator.getProcessChatMemberError(ex));
            }
        }
        protected override async Task processCallbackQuery(CallbackQuery query)
        {
            long chat = query.Message.Chat.Id;
            PushMessageBase message = null;
            string uuid = string.Empty;
            string status = string.Empty;
            var userInfo = $"{chat} {status} {uuid}";

            try
            {
                var statusResponce = await server.GetFollowerStateResponse(Geotag, chat);
                status = statusResponce.status_code;
                uuid = statusResponce.uuid;

                bool negative = false;
                bool needDelete = false;

                string msg = $"STATUS: {userInfo} uuid={uuid} {status}";
                logger.inf(Geotag, msg);

                switch (query.Data)
                {

                    case "reg":
                        message = MessageProcessor.GetMessage(status, link: LinkReg, support_pm: SUPPORT_PM, pm: PM, uuid: uuid, isnegative: negative, help: Help, vip: Vip);
                        checkMessage(message, "reg", "processCallbackQuery");
                        break;                    

                    case "check_register":
                        negative = status.Equals("WREG");
                        message = MessageProcessor.GetMessage(status, link: LinkReg, support_pm: SUPPORT_PM, pm: PM, uuid: uuid, isnegative: negative, help: Help, vip: Vip);
                        needDelete = true;
                        checkMessage(message, "WREG", "processCallbackQuery");
                        break;

                    case "check_fd":
                        negative = status.Equals("WFDEP");
                        message = MessageProcessor.GetMessage(statusResponce, link: LinkDep, support_pm: SUPPORT_PM, pm: PM, isnegative: negative, help: Help, vip: Vip);
                        needDelete = true;
                        checkMessage(message, "WFDEP", $"processCallbackQuery data={query.Data} status={statusResponce.status_code}");
                        break;
                }

                if (message != null)
                {
                    try
                    {
                        int id = await message.Send(chat, bot);
                        if (needDelete)
                            await clearPrevId(chat, id);
                    }
                    catch (Exception ex)
                    {
                        errCollector.Add(errorMessageGenerator.getProcessCallbackQueryError(userInfo));
                    }
                }

                await bot.AnswerCallbackQueryAsync(query.Id);

            }
            catch (Exception ex)
            {
                logger.err(Geotag, $"processCallbackQuery: {ex.Message}");

            }
        }
        #endregion

        #region public
        public async Task<bool> Push(long id, string code, string uuid, int notification_id, string? firstname)
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

                var statusResponce = await server.GetFollowerStateResponse(Geotag, id);
                var status = statusResponce.status_code;

                StateMessage push = null;

                string sLink = LinkReg;
                switch (code)
                {
                    case "WREG":
                        sLink = LinkReg;
                        break;

                    default:
                        sLink = LinkDep;
                        break;
                }

                var tmp = MessageProcessor.GetPush(statusResponce, code, link: sLink, support_pm: SUPPORT_PM, pm: PM, isnegative: false, vip: Vip, help: Help);
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

                    checkMessage(push, code, "Push");

                    try
                    {
                        await push.Send(id, bot);
                        res = true;
                        logger.inf(Geotag, $"PUSHED: {id} {status} {code}");

                    }
                    catch (Exception ex)
                    {
                        logger.err(Geotag, $"Push: {ex.Message} (1)");

                    } finally
                    {
                        await server.SlipPush(notification_id, res);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.err(Geotag, $"Push: {ex.Message} (2)");
            }
            return res;
        }

        public async Task UpdateStatus(StatusUpdateDataDto updateData)
        {  

            tgFollowerStatusResponse tmp = new tgFollowerStatusResponse()
            {
                status_code = updateData.status_new,
                uuid = updateData.uuid,
                start_params = updateData.start_params,
                amount_local_currency = updateData.amount_local_currency,
                target_amount_local_currency = updateData.target_amount_local_currency
            };

            bool needDelete = false;

            try
            {

                logger.inf(Geotag, $"UPDATE REQ: {updateData.tg_id}" +
                    $" {updateData.uuid}" +
                    $" {updateData.start_params}" +
                    $" {updateData.status_old}->{updateData.status_new}" +
                    $" paid:{updateData.amount_local_currency} need:{updateData.target_amount_local_currency}");

                StateMessage message = null;
                int id;

                switch (tmp.status_code)
                {

                    case "WFDEP":                        
                    case "WREDEP1":
                        message = MessageProcessor.GetMessage(tmp, link: LinkDep, support_pm: SUPPORT_PM, pm: PM, channel: Channel, false, training: Training, help: Help, vip: Vip);
                        checkMessage(message, "WFDEP/WREDEP1", "UpdateStatus");
                        id = await message.Send(updateData.tg_id, bot);
                        break;

                    case "WREDEP2":
                        message = MessageProcessor.GetMessage("rd1_ok", training: Training);
                        checkMessage(message, "rd1_ok", "UpdateStatus");
                        await message.Send(updateData.tg_id, bot);
                        break;

                    case "WREDEP5":
                        message = MessageProcessor.GetMessage("rd4_ok_1", vip: Vip);
                        checkMessage(message, "rd4_ok_1", "UpdateStatus");
                        await message.Send(updateData.tg_id, bot);
                        break;
                }

                logger.inf(Geotag, $"UPDATED: {updateData.tg_id}" +
                    $" {updateData.uuid}" +
                    $" {updateData.start_params}" +
                    $" {updateData.status_old}->{updateData.status_new}" +
                    $" paid:{updateData.amount_local_currency} need:{updateData.target_amount_local_currency}");

            }
            catch (Exception ex)
            {
                logger.err(Geotag, $"UpadteStatus {updateData.tg_id} {updateData.status_old}->{updateData.status_new}: {ex.Message}");

                if (!ex.Message.Contains("bot was blocked"))
                    errCollector.Add(errorMessageGenerator.getUserStatusUpdateError(ex));
            }
        }

        public override Task Start()
        {
            return base.Start().ContinueWith(t =>
            {
                messageProcessorFactory = new MessageProcessorFactory(logger);

                MessageProcessor = messageProcessorFactory.Get(Type, Geotag, Token, bot);

                if (MessageProcessor != null)
                {
                    MessageProcessor.UpdateMessageRequestEvent += async (code, description) =>
                    {
                        AwaitedMessageCode = code;
                        state = State.waiting_new_message;

                        //var operators = operatorsProcessor.GetAll(geotag).Where(o => o.permissions.Any(p => p.type.Equals(OperatorPermissionType.all)));
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
                        //var operators = operatorsProcessor.GetAll(geotag).Where(o => o.permissions.Any(p => p.type.Equals(OperatorPermissionType.all)));                
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

                chatJoinRequestTimer = new System.Timers.Timer();
                chatJoinRequestTimer.Interval = 5 * 1000;

                List<ChatJoinRequest> tmpRequests = new();

                chatJoinRequestTimer.Elapsed += (s, e) =>
                {

                    lock (chatJoinLock)
                    {
                        tmpRequests = chatJoinRequests.ToList();
                        chatJoinRequests.Clear();
                    }

                    var _ = Task.Run(async () =>
                    {


                        foreach (var request in tmpRequests.ToList())
                        {

                            var chat = request.From.Id;
                            var fn = request.From.FirstName;
                            var ln = request.From.LastName;
                            var un = request.From.Username;

                            string userinfo = $"{Channel} {chat} {fn} {ln} {un}";

                            try
                            {
                                await bot.ApproveChatJoinRequest(request.Chat.Id, request.From.Id);
                                logger.inf_urgent(Geotag, $"CHAPPROVED: ({++appCntr}) {userinfo}");
                                await Task.Delay(1000);
                            }
                            catch (Exception ex)
                            {
                                logger.err(Geotag, $"processChatJoinRequest {userinfo} {ex.Message}");
                                errCollector.Add(errorMessageGenerator.getProcessChatJoinRequestError(request.From.Id, ChannelTag, ex));
                            }
                        }

                    });

                };

                chatJoinRequestTimer.Start();

            });
        }

        public override void Stop()
        {
            base.Stop();
            chatJoinRequestTimer?.Stop();
        }

        public override Task Notify(object notifyObject)
        {
            throw new NotImplementedException();
        }

        public string? GetRegisterSource()
        {
            throw new NotImplementedException();
        }       
        #endregion
    }
}