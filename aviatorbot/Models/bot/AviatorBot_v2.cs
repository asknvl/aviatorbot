using aksnvl.messaging;
using asknvl.logger;
using asknvl.server;
using Avalonia.X11;
using aviatorbot.Model.bot;
using aviatorbot.Models.storage;
using aviatorbot.Operators;
using aviatorbot.rest;
using DynamicData;
using motivebot.Model.storage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using static asknvl.server.TGBotFollowersStatApi;

namespace aviatorbot.Models.bot
{
    public class AviatorBot_v2 : AviatorBot_v1
    {
        public override BotType Type => BotType.aviator_v2;

        public AviatorBot_v2(BotModel model, IOperatorStorage operatorStorage, IBotStorage botStorage, ILogger logger) : base(model, operatorStorage, botStorage, logger)
        {
            Geotag = model.geotag;
            Token = model.token;
            Link = model.link;
            PM = model.pm;
            Channel = model.channel;
            Postbacks = model.postbacks;
        }

        override public async Task processFollower(Message message)
        {

            if (message.Text == null)
                return;

            try
            {

                long chat = message.Chat.Id;
                var fn = message.From.Username;
                var ln = message.From.FirstName;
                var un = message.From.LastName;

                //string uuid = string.Empty;
                //string status = string.Empty;

                if (message.Text.Contains("/start"))
                {
                    var start_params = message.Text.Replace("/start", "").Trim();

                    if (string.IsNullOrEmpty(start_params))                    
                        logger.err(Geotag, $"START: empty start params {chat} {fn} {ln} {un}");
                    else
                        if (start_params.Length < 8)
                        logger.err(Geotag, $"START: start params corrupt {start_params} {chat} {fn} {ln} {un}");


                    var msg = $"START: {chat} {fn} {ln} {un} p:{start_params} ?";
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
                        start_params = start_params,
                        is_subscribed = true
                    };
                    followers.Add(follower);
                    await server.UpdateFollowers(followers);

                    //(uuid, status) = await server.GetFollowerState(Geotag, chat);

                    //var m = MessageProcessor.GetMessage(status, Link, PM, uuid, Channel, false);
                    //var m = MessageProcessor.GetMessage("video", PM);

                    var m = MessageProcessor.GetMessage("video", Link, PM, "", Channel, false);
                    await m.Send(chat, bot, null, Path.Combine(Directory.GetCurrentDirectory(), "resources", "thumb.jpg"));

                    msg = $"STARTED: {chat} {fn} {ln} {un}";
                    logger.inf(Geotag, msg);
                }
                else
                {
                    var resp = await server.GetFollowerStateResponse(Geotag, chat);
                    var msg = $"TEXT: {chat} {fn} {ln} {un} {resp.uuid} {resp.start_params} {resp.player_id} {resp.status_code}\n{message.Text}";
                    logger.inf(Geotag, msg);
                }

            }
            catch (Exception ex)
            {
                logger.err(Geotag, $"processFollower: {ex.Message}");
            }
        }

        protected override async Task processCallbackQuery(CallbackQuery query)
        {
            long chat = query.Message.Chat.Id;
            PushMessageBase message = null;

            logger.inf(Geotag, $"processCallbackQuery {chat} ?");

            string uuid = string.Empty;
            string status = string.Empty;
            string? start_params = string.Empty;
            string? player_id = string.Empty;
            int paid_sum = 0;
            int add_pay_sum = 0;

            string sstatus = "";

            try
            {
                //(uuid, status) = await server.GetFollowerState(Geotag, chat);

                var statusResponce = await server.GetFollowerStateResponse(Geotag, chat);
                sstatus = statusResponce.ToString();

                status = statusResponce.status_code;
                uuid = statusResponce.uuid;                
                paid_sum = (int)statusResponce.amount_local_currency;
                add_pay_sum = (int)statusResponce.target_amount_local_currency;
                start_params = statusResponce.start_params;
                player_id = statusResponce.player_id;

                string msg = $"STATUS: {chat} {uuid} {start_params} {player_id} {status} paid: {paid_sum} need: {add_pay_sum}";

                if (!string.IsNullOrEmpty(start_params))
                    logger.inf(Geotag, msg);
                else
                    logger.err(Geotag, msg);

                bool delete = true;
                bool negative = false;

                switch (query.Data)
                {
                    case "show_reg":

                        //switch (status)
                        //{
                        //    case "WFDEP":
                        //        message = MessageProcessor.GetMessage("WFDEP", Link, PM, uuid, Channel, false);
                        //        break;
                        //    case "WREDEP1":
                        //        break;
                        //}

                        message = MessageProcessor.GetMessage(/*status, start_params, paid_sum,*/statusResponce, Link, PM, Channel, false);

                        //if (status == "WREG")                        
                        delete = false;
                        break;

                    case "check_register":
                        //if (status.Equals("WREG"))
                        //{
                        //    message = MessageProcessor.GetMessage(status, add_pay_sum, Link, PM, uuid, Channel, true);
                        //}
                        //else
                        //    message = MessageProcessor.GetMessage(status, add_pay_sum, Link, PM, uuid, Channel, false);

                        negative = status.Equals("WREG");
                        message = MessageProcessor.GetMessage(statusResponce, Link, PM, Channel, negative);
                        break;

                    case "check_fd":
                        //if (status.Equals("WFDEP"))
                        //{
                        //    message = MessageProcessor.GetMessage(status, add_pay_sum, Link, PM, uuid, Channel, true);
                        //}
                        //else
                        //    message = MessageProcessor.GetMessage(status, add_pay_sum, Link, PM, uuid, Channel, false);

                        negative = status.Equals("WFDEP");
                        message = MessageProcessor.GetMessage(statusResponce, Link, PM, Channel, negative);
                        break;

                    case "check_rd1":
                        //if (status.Equals("WREDEP1"))
                        //{
                        //    message = MessageProcessor.GetMessage(status, add_pay_sum, Link, PM, uuid, Channel, true);
                        //}
                        //else
                        //    message = MessageProcessor.GetMessage(status,add_pay_sum, Link, PM, uuid, Channel, false);

                        negative = status.Equals("WREDEP1");
                        message = MessageProcessor.GetMessage(statusResponce, Link, PM, Channel, negative);
                        break;

                    default:
                        break;
                }

                if (message != null)
                {

                    int id = await message.Send(query.From.Id, bot);

                    if (delete)
                        try
                        {
                            await bot.DeleteMessageAsync(query.From.Id, id - 1);
                        }
                        catch (Exception ex) { }

                    //while (true)
                    //{
                    //    try
                    //    {
                    //        await bot.DeleteMessageAsync(query.From.Id, --id);
                    //    }
                    //    catch (Exception ex)
                    //    {
                    //        break;
                    //    }
                    //}
                }

                await bot.AnswerCallbackQueryAsync(query.Id);

            }
            catch (Exception ex)
            {
                logger.err(Geotag, $"processCallbackQuery: {ex.Message} : {sstatus}");
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
                var message = MessageProcessor.GetMessage(tmp, Link, PM, Channel, false);
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
    }
}
