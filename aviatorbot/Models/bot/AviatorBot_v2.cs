﻿using aksnvl.messaging;
using asknvl.logger;
using asknvl.server;
using aviatorbot.Model.bot;
using aviatorbot.Operators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace aviatorbot.Models.bot
{
    internal class AviatorBot_v2 : AviatorBot_v1
    {
        public override BotType Type => BotType.aviator_v2;

        public AviatorBot_v2(BotModel model, IOperatorsProcessor operatorsProcessor, ILogger logger) : base(model, operatorsProcessor, logger)
        {
            Geotag = model.geotag;
            Token = model.token;
            Link = model.link;
            PM = model.pm;
            Channel = model.channel;
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

                    //(uuid, status) = await server.GetFollowerState(Geotag, chat);

                    //var m = MessageProcessor.GetMessage(status, Link, PM, uuid, Channel, false);
                    //var m = MessageProcessor.GetMessage("video", PM);

                    var m = MessageProcessor.GetMessage("video", Link, PM, uuid, Channel, false);
                    await m.Send(chat, bot, null, Path.Combine(Directory.GetCurrentDirectory(), "resources", "thumb.jpg"));

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

        protected override async Task processCallbackQuery(CallbackQuery query)
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

                bool delete = true;

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

                        message = MessageProcessor.GetMessage(status, Link, PM, uuid, Channel, false);

                        //if (status == "WREG")                        
                        delete = false;
                        break;

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
                logger.err(Geotag, $"processCallbackQuery: {ex.Message}");
            }
        }
    }
}