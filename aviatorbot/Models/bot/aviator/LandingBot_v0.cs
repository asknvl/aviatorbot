using aksnvl.messaging;
using asknvl.logger;
using asknvl.server;
using Avalonia.X11;
using aviatorbot.Models.bot;
using botservice.Model.bot;
using botservice.Models.messages;
using botservice.Models.storage;
using botservice.Operators;
using botservice.rest;
using DynamicData.Kernel;
using motivebot.Model.storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using static asknvl.server.TGBotFollowersStatApi;

namespace botservice.Models.bot.aviator
{
    public class LandingBot_v0 : AviatorBotBase
    {
        #region vars
        Dictionary<long, int> prevRegIds = new();        
        #endregion
        public override BotType Type => BotType.landing_v0_1win_wv_eng;
        public LandingBot_v0(BotModel model, IOperatorStorage operatorStorage, IBotStorage botStorage, ILogger logger) : base(model, operatorStorage, botStorage, logger)
        {
            Geotag = model.geotag;
            Token = model.token;
            Link = model.link;
            SUPPORT_PM = model.support_pm;
            PM = model.pm;
            ChannelTag = model.channel_tag;
            Channel = model.channel;

            Help = model.help;
            Training = model.training;
            Reviews = model.reveiews;
            Strategy = model.strategy;
            Vip = model.vip;

            Postbacks = model.postbacks;
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
                var fn = message.From.Username;
                var ln = message.From.FirstName;
                var un = message.From.LastName;

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


                    var m = MessageProcessor.GetMessage(code,
                                                        link: Link,
                                                        support_pm: SUPPORT_PM,
                                                        pm: PM,
                                                        uuid: uuid,
                                                        channel: Channel,
                                                        help: Help
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

                                await Task.Delay(10000);

                                m = MessageProcessor.GetMessage("video",
                                                                link: Link,
                                                                support_pm: SUPPORT_PM,
                                                                pm: PM,
                                                                uuid: _uuid,
                                                                channel: Channel);

                                checkMessage(m, "/start", "video");

                                await m.Send(chat, bot);

                                await Task.Delay(10000);

                                m = MessageProcessor.GetMessage("tarrifs",
                                                               link: Link,
                                                               support_pm: SUPPORT_PM,
                                                               pm: PM,
                                                               uuid: _uuid,
                                                               channel: Channel);

                                checkMessage(m, "/start", "tarrifs");

                                await m.Send(chat, bot);
                            }
                            catch (Exception ex)
                            {
                                logger.err(Geotag, $"video&tarrifs error");
                            }

                        });

                    }

                    //try
                    //{
                    //    while (true)
                    //        await bot.DeleteMessageAsync(chat, --id);
                    //}
                    //catch (Exception ex) { }

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
                        message = MessageProcessor.GetMessage(status, link: Link, support_pm: SUPPORT_PM, pm: PM, uuid: uuid, isnegative: negative, help: Help);
                        checkMessage(message, "reg", "processCallbackQuery");
                        break;

                    case "check_register":
                        negative = status.Equals("WREG");
                        message = MessageProcessor.GetMessage(status, link: Link, support_pm: SUPPORT_PM, pm: PM, uuid: uuid, isnegative: negative, help: Help);
                        needDelete = true;
                        checkMessage(message, "WREG", "processCallbackQuery");
                        break;

                    case "check_fd":
                        negative = status.Equals("WFDEP");
                        message = MessageProcessor.GetMessage(statusResponce, link: Link, support_pm: SUPPORT_PM, pm: PM, isnegative: negative, help: Help);
                        needDelete = true;
                        checkMessage(message, "WFDEP", $"processCallbackQuery data={query.Data} status={statusResponce.status_code}");
                        break;

                    case "pm_access":
                        message = MessageProcessor.GetMessage("pm_access", link: Link, support_pm: SUPPORT_PM, pm: PM, uuid: uuid, isnegative: negative);
                        checkMessage(message, "pm_access", "processCallbackQuery");
                        break;
                }

                if (message != null)
                {
                    try
                    {
                        int id = await message.Send(chat, bot);
                        if (needDelete)
                            await clearPrevId(chat, id);
                    } catch (Exception ex)
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
        int appCntr = 0;
        protected override async Task processChatJoinRequest(ChatJoinRequest chatJoinRequest, CancellationToken cancellationToken)
        {           

            var chat = chatJoinRequest.From.Id;
            var fn = chatJoinRequest.From.FirstName;
            var ln = chatJoinRequest.From.LastName;
            var un = chatJoinRequest.From.Username;

            string userinfo = $"{Channel} {chat} {fn} {ln} {un}";

            logger.inf_urgent(Geotag, $"CHREQUEST: ({++appCntr}) {userinfo}");

            if (ChApprove == false)
            {                
                return;
            }

            try
            {
                await bot.ApproveChatJoinRequest(chatJoinRequest.Chat.Id, chatJoinRequest.From.Id);
                logger.inf_urgent(Geotag, $"CHAPPROVED: ({++appCntr}) {userinfo}");
            }
            catch (Exception ex)
            {
                logger.err(Geotag, $"processChatJoinRequest {userinfo} {ex.Message}");
                errCollector.Add(errorMessageGenerator.getProcessChatJoinRequestError(chatJoinRequest.From.Id, ChannelTag, ex));
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

        protected override async Task processOperator(Message message, Operator op)
        {

            var chat = message.From.Id;

            try
            {
                if (state == State.waiting_new_message)
                {
                    MessageProcessor.Add(AwaitedMessageCode, message, PM, channel: Channel, support_pm: SUPPORT_PM,
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
        #endregion

        #region public
        public override async Task UpdateStatus(StatusUpdateDataDto updateData)
        {

            if (Postbacks != true)
                return;

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

                        message = MessageProcessor.GetMessage(tmp, link: Link, support_pm: SUPPORT_PM, pm: PM, channel: Channel, false, training: Training, help: Help);
                        checkMessage(message, "WFDEP/WREDEP1", "UpdateStatus");

                        id = await message.Send(updateData.tg_id, bot);

                        //try
                        //{
                        //    await bot.DeleteMessageAsync(updateData.tg_id, id - 1);
                        //}
                        //catch (Exception ex) { }

                        break;

                    case "WREDEP2":
                        message = MessageProcessor.GetMessage("rd1_ok", training: Training, pm: PM);
                        checkMessage(message, "rd1_ok", "UpdateStatus");

                        await message.Send(updateData.tg_id, bot);                        
                        break;

                    case "WREDEP5":

                        Task.Run(async () =>
                        {
                            message = MessageProcessor.GetMessage("rd4_ok_1", pm: PM, vip: Vip, training: Training);
                            checkMessage(message, "rd4_ok_1", "UpdateStatus");
                            await message.Send(updateData.tg_id, bot);
                            
                            await Task.Delay(60 * 1000);

                            message = MessageProcessor.GetMessage("rd4_ok_2", vip: Vip);
                            checkMessage(message, "rd4_ok_2", "UpdateStatus");

                            await message.Send(updateData.tg_id, bot);
                            
                        });
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

        public override async Task<bool> Push(long id, string code, int notification_id)
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

                try
                {
                    push = MessageProcessor.GetPush(statusResponce, code, link: Link, support_pm: SUPPORT_PM, pm: PM, isnegative: false, vip: Vip, help: Help);
                    checkMessage(push, code, "Push");
                }
                catch (Exception ex)
                {
                    logger.err(Geotag, $"Push: {id} {ex.Message} (0)");
                    await server.SlipPush(notification_id, false);
                }

                if (push != null)
                {
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

        public override async Task Notify(object notifyObject)
        {
            await Task.CompletedTask;
        }

        #endregion
    }
}
