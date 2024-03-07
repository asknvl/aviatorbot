using aksnvl.messaging;
using asknvl.logger;
using asknvl.server;
using aviatorbot.Model.bot;
using aviatorbot.Models.storage;
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
using static asknvl.server.TGBotFollowersStatApi;

namespace aviatorbot.Models.bot
{
    public class LandingBot_cana37 : LandingBot_v0
    {

        public override BotType Type => BotType.landing_v0_cut_cana37;

        public LandingBot_cana37(BotModel model, IOperatorStorage operatorStorage, IBotStorage botStorage, ILogger logger) : base(model, operatorStorage, botStorage, logger)
        {

            Geotag = model.geotag;
            Token = model.token;
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
                            fb_event_send = need_fb_event,
                            is_subscribed = true
                        };
                        followers.Add(follower);

                        try
                        {
                            await server.UpdateFollowers(followers);
                            msg = $"UPDATED: {userInfo} uuid={uuid} event={follower.fb_event_send}";
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

                                await Task.Delay(10000);

                                m = MessageProcessor.GetMessage("video",
                                                                link: Link,
                                                                support_pm: SUPPORT_PM,
                                                                pm: PM,
                                                                uuid: uuid,
                                                                channel: Channel);
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

                    case "WREG":
                        message = MessageProcessor.GetMessage(status, link: Link, support_pm: SUPPORT_PM, pm: PM, uuid: uuid, isnegative: negative);
                        break;

                    case "register_done":
                        try
                        {
                            await server.SetFollowerRegistered("00000", uuid);
                        } catch (Exception ex)
                        {
                            logger.err(Geotag, $"register_done: {ex.Message}");
                        } finally
                        {
                            await bot.AnswerCallbackQueryAsync(query.Id);
                        }
                        return;

                    case "fd_done":
                        try
                        {
                            await server.SetFollowerMadeDeposit(uuid, 00000, 25);
                        } catch (Exception ex)
                        {
                            logger.err(Geotag, $"fd_done: {ex.Message}");
                        } finally
                        {
                            await bot.AnswerCallbackQueryAsync(query.Id);
                        }
                        return;
                }

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

        public override async Task UpdateStatus(StatusUpdateDataDto updateData)
        {

            if (Postbacks != true)
                return;

            //tgFollowerStatusResponse tmp = new tgFollowerStatusResponse()
            //{
            //    status_code = updateData.status_new,
            //    uuid = updateData.uuid,
            //    start_params = updateData.start_params,
            //    amount_local_currency = updateData.amount_local_currency,
            //    target_amount_local_currency = updateData.target_amount_local_currency
            //};

            var status = updateData.status_new;
            string uuid = updateData.uuid;

            try
            {
                var message = MessageProcessor.GetMessage(status, link: Link, support_pm: SUPPORT_PM, pm: PM, uuid: uuid, isnegative: false);
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

            if (Postbacks != true)
                return false;

            bool res = false;
            try
            {

                var statusResponce = await server.GetFollowerStateResponse(Geotag, id);
                var status = statusResponce.status_code;               
                string uuid = statusResponce.uuid;


                var push = MessageProcessor.GetPush(code);

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

        int appCntr = 0;
        protected override async Task processChatJoinRequest(ChatJoinRequest chatJoinRequest, CancellationToken cancellationToken)
        {
            try
            {
                await bot.ApproveChatJoinRequest(chatJoinRequest.Chat.Id, chatJoinRequest.From.Id);
                logger.inf_urgent(Geotag, $"CHREQUEST: ({++appCntr}) " +
                                $"{Channel} " +
                                $"{chatJoinRequest.From.Id} " +
                                $"{chatJoinRequest.From.FirstName} " +
                                $"{chatJoinRequest.From.LastName} " +
                                $"{chatJoinRequest.From.Username}");
            }
            catch (Exception ex)
            {
                logger.err(Geotag, $"processChatJoinRequest {ex.Message}");

            }

        }

    }
}
