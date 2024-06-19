using asknvl.logger;
using botservice.Model.bot;
using botservice.Models.messages;
using botservice.Models.storage;
using botservice.rest;
using motivebot.Model.storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static asknvl.server.TGBotFollowersStatApi;
using Telegram.Bot;
using Telegram.Bot.Types;
using asknvl.server;

namespace botservice.Models.bot.aviator
{
    public class LandingBot_strategies : LandingBot_v0
    {
        #region vars
        Dictionary<long, int> prevRegIds = new();
        #endregion

        public override BotType Type => BotType.landing_v0_strategies;
        public LandingBot_strategies(BotModel model, IOperatorStorage operatorStorage, IBotStorage botStorage, ILogger logger) : base(model, operatorStorage, botStorage, logger)
        {
            Geotag = model.geotag;
            Token = model.token;
            Link = model.link;

            SUPPORT_PM = "-";
            PM = model.pm;

            ChannelTag = model.channel_tag;
            Channel = model.channel;
            ChApprove = model.channel_approve;

            Help = "-";
            Training = "-";
            Reviews = "-";
            Strategy = "-";
            Vip = "-";

            Postbacks = model.postbacks;
        }

        #region protected
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

                        var _ = Task.Run(async () =>
                        {

                            try
                            {

                                await Task.Delay(1000);

                                m = MessageProcessor.GetMessage("text",                                                                
                                                                channel: Channel);                                

                                await m.Send(chat, bot);

                                await Task.Delay(15000);

                                m = MessageProcessor.GetMessage("video",
                                                                link: Link,
                                                                support_pm: SUPPORT_PM,
                                                                pm: PM,
                                                                uuid: uuid,
                                                                channel: Channel);
                                await m.Send(chat, bot);

                                await Task.Delay(15000);

                                //m = MessageProcessor.GetMessage("before", pm: PM);

                                //await m.Send(chat, bot);

                                //await Task.Delay(15000);

                                //m = MessageProcessor.GetMessage("reg",
                                //                               link: Link,
                                //                               support_pm: SUPPORT_PM,
                                //                               pm: PM,
                                //                               uuid: uuid,
                                //                               channel: Channel);

                                m = MessageProcessor.GetMessage("tarrifs");

                                await m.Send(chat, bot);
                            }
                            catch (Exception ex)
                            {
                                logger.err(Geotag, $"start messages error {ex.Message}");
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

        protected override Task processCallbackQuery(CallbackQuery query)
        {
            return base.processCallbackQuery(query);
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

                        message = MessageProcessor.GetMessage(tmp, link: Link, pm: PM, channel: Channel, isnegative: false);
                        id = await message.Send(updateData.tg_id, bot);

                        try
                        {
                            await bot.DeleteMessageAsync(updateData.tg_id, id - 1);
                        }
                        catch (Exception ex) { }

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
            }
        }

        public override async Task<bool> Push(long id, string code, int notification_id, string? firstname)
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
                        logger.err(Geotag, $"Push: {ex.Message} (1)");

                    }
                    finally
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
