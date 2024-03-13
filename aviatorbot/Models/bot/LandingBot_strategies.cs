using asknvl.logger;
using aviatorbot.Model.bot;
using aviatorbot.Models.messages;
using aviatorbot.Models.storage;
using aviatorbot.rest;
using motivebot.Model.storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static asknvl.server.TGBotFollowersStatApi;
using Telegram.Bot;

namespace aviatorbot.Models.bot
{
    public class LandingBot_strategies : LandingBot_v0
    {
        public override BotType Type => BotType.landing_v0_strategies;
        public LandingBot_strategies(BotModel model, IOperatorStorage operatorStorage, IBotStorage botStorage, ILogger logger) : base(model, operatorStorage, botStorage, logger)
        {
        }

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

                        message = MessageProcessor.GetMessage(tmp, link: Link, support_pm: SUPPORT_PM, pm: PM, channel: Channel, false, training: Training);
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
