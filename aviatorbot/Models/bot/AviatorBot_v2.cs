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
using Telegram.Bot.Types;

namespace aviatorbot.Models.bot
{
    internal class AviatorBot_v2 : AviatorBotBase
    {
        public override BotType Type => BotType.aviator_v2;

        public AviatorBot_v2(BotModel model, IOperatorsProcessor operatorsProcessor, ILogger logger) : base(operatorsProcessor, logger)
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
    }
}
