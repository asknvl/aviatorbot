﻿using asknvl.logger;
using csb.server;
using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace csb.invitelinks
{
    public class DynamicInviteLinkProcessor : IInviteLinksProcessor
    {
        #region const
        int link_pool_capacity = 50;
        #endregion

        #region vars
        string geotag;        
        ITelegramBotClient bot;
        ITGFollowerTrackApi trackApi;
        ILogger logger;
        long? channelId;
        #endregion

        public DynamicInviteLinkProcessor(string geotag, ITelegramBotClient bot, ITGFollowerTrackApi trackApi, ILogger logger)
        {   
            this.geotag = geotag;            
            this.bot = bot;
            this.trackApi = trackApi; //new TGFollowerTrackApi("https://app.alopadsa.ru");

            this.logger = logger;
        }

        #region public
        public async Task<string> Generate(long? channelid)
        {
            string link = "";
            
            logger.dbg(geotag, "Link generate request:");
            if (channelid == null)
            {
                logger.err(geotag, "Unable to generate link: ChannelID=null");
                ExceptionEvent?.Invoke("Не установлен ID канала");

                return link;
            }

            try
            {
                await Task.Run(async () => {

                    var invitelink = await bot.CreateChatInviteLinkAsync(channelid, null, null, null, true);
                    link = invitelink.InviteLink;
                    logger.dbg(geotag, $"generated:{link}");

                    await trackApi.EnqueueInviteLink(geotag, link);
                    logger.dbg(geotag, "server: enqueued");

                });

            } catch (Exception ex)
            {
                ExceptionEvent?.Invoke("Ошибка формирования одноразовой ссылки");
                logger.err(geotag, $"linkGeneration: {ex.Message}");
                throw;
            }

            logger.dbg(geotag, $"Link generated {link}");

            return link;
        }

        //public async Task<int> Generate(long? channelid, int n)
        //{
        //    logger.inf_urgent($"Link generate request n={n}:");
        //    if (channelid == null)
        //    {
        //        logger.err($"Unable to generate link n={n}: ChannelID=null");
        //        return 0;
        //    }

        //    int res = 0;
        //    await Task.Run(async () => {

        //        for (int i = 0; i < n; i++)
        //        {
        //            await Generate(channelid);
        //            res++;
        //            Thread.Sleep(1000);
        //        }            
        //    });

        //    logger.inf($"Links generated n={n}:");
        //    return res;
        //}

        public async Task Revoke(long? channelid, string link)
        {
            logger.dbg(geotag, $"Link revoke request {link}");
            if (channelid == null)
            {
                logger.dbg(geotag, "Unable to revoke link: ChannelID=null");
                ExceptionEvent?.Invoke("Не установлен ID канала");
                return;
            }

            try
            {
                await bot.RevokeChatInviteLinkAsync(channelid, link);
                logger.dbg(geotag, $"revoked: CH={channelid} link={link}");
            } catch (Exception ex) {

                logger.err(geotag, $"linkRevoke {ex.Message}");
                throw;
            }

        }

        public Task StartLinkNumberControl(long? channelid, CancellationTokenSource cts)
        {

            channelId = channelid;

            return Task.Run(async () =>
            {

                logger.inf(geotag, $"Link control started");

                while (true)
                {
                    try
                    {
                        cts.Token.ThrowIfCancellationRequested();
                        int n = await trackApi.GetInviteLinksAvailable(geotag);

                        if (n < link_pool_capacity)
                        {

                            string s = $"{geotag} Links available: {n}";
                            logger.dbg(geotag, s);                            
                            await Generate(channelId);
                            logger.dbg(geotag, $"Generated by controller");
                        }

                    }
                    catch (OperationCanceledException ex)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        logger.err(geotag, $"Link control exception {ex.Message}");
                        ExceptionEvent?.Invoke("Ошибка формирования одноразовой ссылки");
                        Thread.Sleep(30000);
                    }

                    Thread.Sleep(500);
                }

                logger.inf(geotag, $"Link control stoped");

            });
        }

        public void UpdateChannelID(long? channelid)
        {
            channelId = channelid;
        }
        #endregion

        #region events
        public event Action<string> ExceptionEvent;
        #endregion
    }
}