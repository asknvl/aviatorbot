﻿using aksnvl.messaging;
using asknvl.logger;
using asknvl.server;
using aviatorbot.Model.bot;
using aviatorbot.Models.storage;
using aviatorbot.Operators;
using aviatorbot.rest;
using motivebot.Model.storage;
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

namespace aviatorbot.Models.bot
{
    public class LandingBot_v0 : AviatorBot_v2
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
            Postbacks = model.postbacks;
        }

        async Task<(string, bool)> getUserStatusOnStart(long tg_id)
        {
            string code = "";
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

                    //subscribe = await server.GetFollowerSubscriprion(ChannelTag, tg_id);
                    //isSubscribed = subscribe.Any(s => s.is_subscribed);

                    //if (!isSubscribed)
                    //{
                    //    code = "start";
                    //}
                    //else
                    //{
                    //    var statusResponce = await server.GetFollowerStateResponse(Geotag, tg_id);
                    //    var status = statusResponce.status_code;

                    //    switch (status)
                    //    {
                    //        case "WREG":
                    //            code = "tarrifs";
                    //            break;
                    //        default:
                    //            if (status.Contains("WFDEP"))
                    //                code = "WFDEP";
                    //            else
                    //            if (status.Contains("WREDEP"))
                    //                code = "WREDEP1";

                    //            else
                    //            {
                    //                code = "start";
                    //                logger.err(Geotag, $"getUserStatusOnStart: {tg_id} udefined status");
                    //            }
                    //            break;
                    //    }
                    //}
                }

            }
            catch (Exception ex)
            {
                logger.err(Geotag, $"getUserStatus: {ex.Message}");
            }

            return (code, is_new);
        }
        public override async Task processFollower(Message message)
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
                    var uuid = (string.IsNullOrEmpty(parse_uuid)) ? null : parse_uuid;

                    //if (string.IsNullOrEmpty(uuid))
                    //    logger.err(Name, $"START: empty uuid {chat} {fn} {ln} {un}");

                    var msg = $"START: {userInfo} ?";
                    logger.inf(Geotag, msg);

                    string code = "";
                    bool is_new = false;

                    (code, is_new) = await getUserStatusOnStart(chat);

                    //if (!is_new)
                    //    return;

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
                            sent_fb_event = need_fb_event,
                            is_subscribed = true
                        };
                        followers.Add(follower);

                        try
                        {
                            await server.UpdateFollowers(followers);
                            msg = $"UPDATED: {userInfo} uuid={uuid} event={follower.sent_fb_event}";
                            logger.inf(Geotag, msg);
                        }
                        catch (Exception ex)
                        {
                            logger.err(Geotag, $"{userInfo} DB ERROR {ex.Message}");
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
                                                        channel: Channel
                                                        );
                    int id = await m.Send(chat, bot);

                    

                    if (code.Equals("start"))
                    {

                        Task.Run(async () => {

                            try
                            {

                                await Task.Delay(2000);

                                m = MessageProcessor.GetMessage("video",
                                                                link: Link,
                                                                support_pm: SUPPORT_PM,
                                                                pm: PM,
                                                                uuid: uuid,
                                                                channel: Channel);
                                await m.Send(chat, bot);

                                await Task.Delay(2000);

                                m = MessageProcessor.GetMessage("tarrifs",
                                                               link: Link,
                                                               support_pm: SUPPORT_PM,
                                                               pm: PM,
                                                               uuid: uuid,
                                                               channel: Channel);
                                await m.Send(chat, bot);
                            } catch (Exception ex)
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
                    var resp = await server.GetFollowerStateResponse(Geotag, chat);
                    var msg = $"TEXT: {userInfo}\n{message.Text}";
                    logger.inf(Geotag, msg);
                }


            }
            catch (Exception ex)
            {
                logger.err(Geotag, $"processFollower: {userInfo} {ex.Message}");
            }
        }

        async Task clearPrevId(long chat, int id)
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

                //(uuid, status) = await server.GetFollowerState(Geotag, chat);

                //status = "WFDEP";

                bool negative = false;
                bool needDelete = false;

                string msg = $"STATUS: {userInfo} uuid={uuid} {status}";
                logger.inf(Geotag, msg);

                switch (query.Data)
                {

                    case "reg":
                        message = MessageProcessor.GetMessage(status, link: Link, support_pm: SUPPORT_PM, pm: PM, uuid: uuid, isnegative: negative);
                        break;

                    case "check_register":
                        negative = status.Equals("WREG");
                        message = MessageProcessor.GetMessage(status, link: Link, support_pm: SUPPORT_PM, pm: PM, uuid: uuid, isnegative: negative);
                        needDelete = true;
                        break;

                    case "check_fd":
                        //if (status.Equals("WFDEP"))
                        //{
                        //    message = MessageProcessor.GetMessage(status, add_pay_sum, Link, PM, uuid, Channel, true);
                        //}
                        //else
                        //    message = MessageProcessor.GetMessage(status, add_pay_sum, Link, PM, uuid, Channel, false);

                        negative = status.Equals("WFDEP");
                        message = MessageProcessor.GetMessage(statusResponce, link: Link, support_pm: SUPPORT_PM, pm: PM, isnegative: negative);
                        needDelete = true;
                        break;

                    case "pm_access":
                        message = MessageProcessor.GetMessage("pm_access", link: Link, support_pm: SUPPORT_PM, pm: PM, uuid: uuid, isnegative: negative);
                        break;
                }

                //if (message != null)
                //{

                if (message != null)
                {
                    int id = await message.Send(chat, bot);
                    if (needDelete)
                        await clearPrevId(chat, id);

                }
                else
                    logger.err(Geotag, $"{query.Data} message not set");



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
            await bot.ApproveChatJoinRequest(chatJoinRequest.Chat.Id, chatJoinRequest.From.Id);
            logger.inf_urgent(Geotag, $"CHREQUEST: ({++appCntr}) " +
                            $"{Channel} " +
                            $"{chatJoinRequest.From.Id} " +
                            $"{chatJoinRequest.From.FirstName} " +
                            $"{chatJoinRequest.From.LastName} " +
                            $"{chatJoinRequest.From.Username}");

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
                    tg_geolocation = Channel
                };

                switch (member.NewChatMember.Status)
                {
                    case ChatMemberStatus.Member:

                        follower.is_subscribed = true;

                        //if (member.InviteLink != null && member.InviteLink.CreatesJoinRequest)
                        //{
                        //} Накрутка?

                        //try
                        //{
                        //    var statusResponce = await server.GetFollowerStateResponse(Name, user_id);
                        //    uuid = statusResponce.uuid;

                        //}
                        //catch (Exception ex)
                        //{
                        //    logger.err(Name, $"processChatMember: id={user_id} uuid={uuid} {ex.Message}");
                        //}

                        //var message = MessageProcessor.GetMessage("reg", link: Link, pm: PM, uuid: uuid);
                        //if (message != null)
                        //{
                        //    int id = await message.Send(user_id, bot);
                        //}
                        //else
                        //    logger.err(Name, $"reg message not set");

                        //Task.Run(async () =>
                        //{
                        //    try
                        //    {
                        //        await Task.Delay(2 * 60 * 1000);
                        //        message = MessageProcessor.GetMessage("push_reg", link: Link, pm: PM, lead_uuid: uuid);
                        //        await message.Send(user_id, bot);
                        //    }
                        //    catch (Exception ex)
                        //    {
                        //        logger.err(Name, $"unable to send push_reg");
                        //    }
                        //});

                        try
                        {
                            followers.Add(follower);
                            await server.UpdateFollowers(followers);
                        }
                        catch (Exception ex)
                        {
                            logger.err(Geotag, $"processChatMember: JOIN DB ERROR {user_id}");
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
                            logger.err(Name, $"processChatMember: LEFT DB ERROR {user_id}");
                        }

                        logger.inf(Name, $"CHLEFT: {Channel} {user_id} {fn} {ln} {un}");
                        break;
                }
            }
            catch (Exception ex)
            {
                logger.err(Name, $"processChatMember: {ex.Message}");
            }
        }

        protected override async Task processOperator(Message message, Operator op)
        {

            var chat = message.From.Id;

            try
            {
                if (state == State.waiting_new_message)
                {
                    MessageProcessor.Add(AwaitedMessageCode, message, PM, channel: Channel, support_pm: SUPPORT_PM);
                    state = State.free;
                    return;
                }
            }
            catch (Exception ex)
            {
                logger.err(Geotag, ex.Message);
            }
        }

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

            try
            {
                var message = MessageProcessor.GetMessage(tmp, link: Link, support_pm: SUPPORT_PM, pm: PM, channel: Channel, false);
                int id = await message.Send(updateData.tg_id, bot);
                try
                {
                    await bot.DeleteMessageAsync(updateData.tg_id, id - 1);
                }
                catch (Exception ex) { }

                logger.inf(Geotag, $"UPDATED: {updateData.tg_id}" +
                    $" {updateData.uuid}" +
                    $" {updateData.start_params}" +
                    $" {updateData.status_old}->{updateData.status_new}" +
                    $" paid:{updateData.amount_local_currency} need:{updateData.target_amount_local_currency}");

            }
            catch (Exception ex)
            {
                logger.err(Geotag, $"UpadteStatus: {ex.Message}");
            }
        }

        public override async Task<bool> Push(long id, string code, int notification_id)
        {
            bool res = false;
            try
            {

                var statusResponce = await server.GetFollowerStateResponse(Geotag, id);
                var status = statusResponce.status_code;

                var push = MessageProcessor.GetPush(statusResponce, code, link: Link, support_pm: SUPPORT_PM, pm: PM, isnegative: false);

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

    }
}