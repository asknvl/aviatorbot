using aksnvl.messaging;
using asknvl.logger;
using asknvl.server;
using aviatorbot.Models.bot;
using aviatorbot.Models.messages.latam;
using botservice;
using botservice.Model.bot;
using botservice.Models.bot.latam;
using botservice.Models.messages;
using botservice.Models.storage;
using botservice.Operators;
using botservice.rest;
using csb.invitelinks;
using csb.server;
using DynamicData;
using motivebot.Model.storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using static asknvl.server.TGBotFollowersStatApi;

namespace botservice.Models.bot.aviator
{
    public class ModeratorBot_strategies_basic_v2 : AviatorModeratorBotBase
    {

        #region vars       
        #endregion

        public override BotType Type => BotType.moderator_v2_strategies;

        public ModeratorBot_strategies_basic_v2(BotModel model, IOperatorStorage operatorStorage, IBotStorage botStorage, ILogger logger) : base(model, operatorStorage, botStorage, logger)
        {                       
        }

        #region override       
        protected override async Task processCallbackQuery(CallbackQuery query)
        {
            long chat = query.From.Id;
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
                }

                if (message != null)
                {
                    try
                    {
                        int id = await message.Send(chat, bot);
                        //if (needDelete)
                        //    await clearPrevId(chat, id);
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
        #endregion                
    }    

}
