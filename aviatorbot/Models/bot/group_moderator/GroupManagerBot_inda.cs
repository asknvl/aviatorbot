using asknvl.logger;
using botservice.Model.bot;
using botservice.Models.storage;
using botservice.Operators;
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
using static asknvl.server.TGBotFollowersStatApi;

namespace botservice.Models.bot.gmanager
{
    public class GroupManagerBot_inda : GroupManagerBotBase
    {

        #region properties
        public override BotType Type => BotType.group_manager_inda;        
        #endregion

        public GroupManagerBot_inda(BotModel model, IOperatorStorage operatorStorage, IBotStorage botStorage, ILogger logger) : base(model, operatorStorage, botStorage, logger)
        {            
        }

        int appCntr = 0;
        protected override async Task processChatJoinRequest(ChatJoinRequest chatJoinRequest, CancellationToken cancellationToken)
        {
            try
            {

                bool approve = false;
                List<getIdUserInfoDto>? sources = null;
                getIdUserInfoDto? sourced = null;

                try
                {
                    sources = await server.GetUserInfoByTGid(chatJoinRequest.Chat.Id);
                } catch (Exception ex)
                {
                    logger.err(Geotag, $"processChatJoinRequest: {ex.Message}");
                }

                if (sources != null)
                {
                    sourced = sources.FirstOrDefault(s => s.geo.Equals(RegisterSource));
                    if (sourced != null)
                    {
                        approve = sourced.fd;
                    }
                }

                if (approve)
                {

                    await bot.ApproveChatJoinRequest(chatJoinRequest.Chat.Id, chatJoinRequest.From.Id);

                    logger.inf_urgent(Geotag, $"GREQUEST: ({++appCntr}) " +
                                        $"{chatJoinRequest.InviteLink?.InviteLink} " +
                                        $"{chatJoinRequest.From.Id} " +
                                        $"{chatJoinRequest.From.FirstName} " +
                                        $"{chatJoinRequest.From.LastName} " +
                                        $"{chatJoinRequest.From.Username} " +
                                        $"{sourced.sum_fd} " +
                                        $"{sourced.sum_rd}");
                } else
                {
                    var m = MessageProcessor.GetMessage("DECLINE", link: RegisterSourceLink);
                    await m.Send(chatJoinRequest.From.Id, bot);

                    await bot.DeclineChatJoinRequest(chatJoinRequest.Chat.Id, chatJoinRequest.From.Id);
                }
            }
            catch (Exception ex)
            {
                logger.err(Geotag, $"processChatJoinRequest {ex.Message}");
            }
        }

    }
}
