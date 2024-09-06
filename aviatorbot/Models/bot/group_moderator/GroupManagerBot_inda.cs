using asknvl.logger;
using botservice.Model.bot;
using botservice.Models.storage;
using botservice.Operators;
using botservice.rest;
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
    public class GroupManagerBot_inda : GroupManagerBotBase, IStatusObserver
    {
        #region const
        const int allow_write_rd_number = 2;
        #endregion

        #region properties
        public override BotType Type => BotType.group_manager_inda;        
        #endregion

        public GroupManagerBot_inda(BotModel model, IOperatorStorage operatorStorage, IBotStorage botStorage, ILogger logger) : base(model, operatorStorage, botStorage, logger)
        {            
        }

        int appCntr = 0;
        int decCntr = 0;
        protected override async Task processChatJoinRequest(ChatJoinRequest chatJoinRequest, CancellationToken cancellationToken)
        {
            try
            {

                bool approve = false;
                List<getIdUserInfoDto>? sources = null;
                getIdUserInfoDto? sourced = null;

                try
                {
                    sources = await server.GetUserInfoByTGid(chatJoinRequest.From.Id);
                } catch (Exception ex)
                {
                    //logger.err(Geotag, $"processChatJoinRequest: {chatJoinRequest.From.Id} {ex.Message}");
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
                    if (!sourced.last_rd_iteration.HasValue || sourced.last_rd_iteration < 2)
                    {
                        var m = MessageProcessor.GetMessage("RESTRICT_FALSE_RD", param1: chatJoinRequest.From.FirstName, pm: PM);
                        await m.Send(chatJoinRequest.From.Id, bot);

                        await bot.ApproveChatJoinRequest(chatJoinRequest.Chat.Id, chatJoinRequest.From.Id);

                        await bot.RestrictChatMemberAsync(chatJoinRequest.Chat.Id, chatJoinRequest.From.Id, new ChatPermissions()
                        {
                            CanSendMessages = false,
                            CanSendDocuments = false,
                            CanSendAudios = false,
                            CanSendPhotos = false,
                            CanSendOtherMessages = false,
                            CanSendVideoNotes = false,
                            CanSendPolls = false,
                            CanSendVideos = false,
                            CanSendVoiceNotes = false
                        });
                        
                    } else
                    {
                        await bot.ApproveChatJoinRequest(chatJoinRequest.Chat.Id, chatJoinRequest.From.Id);
                    }   

                    logger.inf_urgent(Geotag, $"GREQUEST: ({++appCntr}) " +
                                        $"{chatJoinRequest.InviteLink?.InviteLink} " +
                                        $"{chatJoinRequest.From.Id} " +
                                        $"{chatJoinRequest.From.FirstName} " +
                                        $"{chatJoinRequest.From.LastName} " +
                                        $"{chatJoinRequest.From.Username} " +
                                        $"fd={sourced.sum_fd} " +
                                        $"rd={sourced.sum_rd} ({sourced.last_rd_iteration}) "+
                                        $"id={sourced.player_id}");
                } else
                {
                    var m = MessageProcessor.GetMessage("DECLINE", link: RegisterSourceLink);
                    await m.Send(chatJoinRequest.From.Id, bot);

                    await bot.DeclineChatJoinRequest(chatJoinRequest.Chat.Id, chatJoinRequest.From.Id);

                    if (sourced != null)
                    {

                        logger.err(Geotag, $"DECLINED: ({++decCntr}) " +
                                            $"{chatJoinRequest.InviteLink?.InviteLink} " +
                                            $"{chatJoinRequest.From.Id} " +
                                            $"{chatJoinRequest.From.FirstName} " +
                                            $"{chatJoinRequest.From.LastName} " +
                                            $"{chatJoinRequest.From.Username} " +
                                            $"fd={sourced.sum_fd} " +
                                            $"rd={sourced.sum_fd} " +
                                            $"id={sourced.player_id}");
                    } else
                    {
                        logger.err(Geotag, $"DECLINED: ({++decCntr}) " +
                                            $"{chatJoinRequest.InviteLink?.InviteLink} " +
                                            $"{chatJoinRequest.From.Id} " +
                                            $"{chatJoinRequest.From.FirstName} " +
                                            $"{chatJoinRequest.From.LastName} " +
                                            $"{chatJoinRequest.From.Username}");
                                            
                    }

                }
            }
            catch (Exception ex)
            {
                logger.err(Geotag, $"processChatJoinRequest {ex.Message}");
            }
        }

        public string? GetRegisterSource()
        {
            return RegisterSource;
        }

        public async Task UpdateStatus(StatusUpdateDataDto updateData)
        {
            var chat = updateData.tg_id;
            var status_old = updateData.status_old;
            var status_new = updateData.status_new;
            var uuid = updateData.uuid;

            try
            {
                if (status_new.Contains("WREDEP"))
                {
                    int rdNum = int.Parse(status_new.Replace("WREDEP", "")) - 1;
                    if (rdNum >= allow_write_rd_number)
                    {
                        await bot.RestrictChatMemberAsync(ChannelId, chat, new ChatPermissions() {

                            CanSendMessages = true,
                            CanSendDocuments = true,
                            CanSendAudios = true,
                            CanSendPhotos = true,
                            CanSendOtherMessages = true,
                            CanSendVideoNotes = true,
                            CanSendPolls = true,
                            CanSendVideos = true,
                            CanSendVoiceNotes = true

                        });

                        logger.inf_urgent(Geotag, $"WRALLOW: {status_new} {chat} {uuid}");
                    }
                }

            } catch (Exception ex)
            {
                logger.err(Geotag, $"updateStatus: {chat} {status_old}->{status_new} {ex.Message}");
            }
        }       
    }
}
