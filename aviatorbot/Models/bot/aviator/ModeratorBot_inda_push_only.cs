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

namespace botservice.Models.bot.latam
{
    public class ModeratorBot_inda_push_only : LatamBotBase
    {

        #region vars
        Dictionary<long, int> pushStartCounters = new Dictionary<long, int>();
        List<pushStartProcess> pushStartProcesses = new List<pushStartProcess>();
        object lockObject = new object();
        IInviteLinksProcessor linksProcessor;
        ITGFollowerTrackApi api;
        long channelID;        
        #endregion

        public override BotType Type => BotType.moderator_v2_strategies;

        public ModeratorBot_inda_push_only(BotModel model, IOperatorStorage operatorStorage, IBotStorage botStorage, ILogger logger) : base(model, operatorStorage, botStorage, logger)
        {
             api = new TGFollowerTrackApi_v1("https://app.flopasda.site");            
        }

        #region override
        //подписка на бота по нажатию на кнопку или любое действие
        protected override async Task processFollower(Message message)
        {

            if (message == null || string.IsNullOrEmpty(message.Text))
                return;

            string userInfo = "";

            try
            {
                long chat = message.Chat.Id;
                var fn = message.From.FirstName;
                var ln = message.From.LastName;
                var un = message.From.Username;
                bool is_new = true;

                var found = pushStartProcesses.FirstOrDefault(p => p.chat ==  chat);
                if (found != null)
                {
                    found.stop();
                    await Task.Delay(100);
                    lock (lockObject)
                    {
                        pushStartProcesses.Remove(found);
                        logger.dbg(Geotag, $"{chat} > pushStartProcess removed total={pushStartProcesses.Count}");
                    }
                }

                if (!pushStartCounters.ContainsKey(chat))
                {
                    pushStartCounters.Add(chat, 0);
                    is_new = true;
                } else
                {
                    var cnt = pushStartCounters[chat];
                    cnt++;
                    cnt %= ((MP_latam_basic_v2)MessageProcessor).start_push_number;
                    pushStartCounters[chat] = cnt;
                    is_new = false;
                }


                //var code = $"hi_{pushStartCounters[chat]}_out";
                //var m = MessageProcessor.GetMessage(code, pm: PM);
                //checkMessage(m, code, "processFollower");
                //await m.Send(chat, bot);

                if (is_new)
                {
                    List<Follower> followers = new();
                    var follower = new Follower()
                    {
                        tg_chat_id = ID,
                        tg_user_id = message.From.Id,
                        username = un,
                        firstname = fn,
                        lastname = ln,
                        office_id = (int)Offices.KRD,
                        tg_geolocation = Geotag,
                        fb_event_send = false,
                        is_subscribed = true
                    };
                    followers.Add(follower);

                    try
                    {
                        await server.UpdateFollowers(followers);
                        logger.inf_urgent(Geotag, $"BTJOINED: {Geotag} {chat} {fn} {ln} {un}");
                    }
                    catch (Exception ex)
                    {
                        logger.err(Geotag, $"{userInfo} DB ERROR {ex.Message}");
                    }
                }

                userInfo = $"{chat} {fn} {ln} {un}";

                var index = MessageProcessor.hi_outs.IndexOf(message.Text);
                if (index == -1)
                    //index = 0;
                    index = pushStartCounters[chat];

                try
                {
                    var m = MessageProcessor.GetMessage($"hi_out", pm: PM);
                    checkMessage(m, $"hi_out", "processFollower");
                    await m.Send(chat, bot);
                    pushStartCounters[chat] = index;
                } catch (Exception ex)
                {
                    logger.err(Geotag, $"processFollower {ex.Message}");
                }

            }
            catch (Exception ex)
            {
            }
        }

        //начало стартовых пушей, аппрув в канал
        int appCntr = 0;
        int decCntr = 0;
        protected override async Task processChatJoinRequest(ChatJoinRequest chatJoinRequest, CancellationToken cancellationToken)
        {
            var chat = chatJoinRequest.From.Id;
            bool isAllowed = true;

            try
            {

                isAllowed = await server.IsSubscriptionAvailable(ChannelTag, chat);

            } catch (Exception ex)
            {
                logger.err(Geotag, $"processChatJoinRequest checkSubs: {ChannelTag} {chat} {ex.Message}");
                errCollector.Add(errorMessageGenerator.getCheckSubscriprionAvailableError(ChannelTag, chat, ex));
            }

            try
            {

                if (isAllowed)
                {

                    var found = pushStartProcesses.FirstOrDefault(p => p.chat == chat);
                    if (found == null)
                    {
                        var newProcess = new pushStartProcess(Geotag, chat, bot, MessageProcessor, logger, checkMessage);
                        lock (lockObject)
                        {
                            pushStartProcesses.Add(newProcess);
                        }
                        newProcess.start();
                    }

                    await bot.ApproveChatJoinRequest(chatJoinRequest.Chat.Id, chatJoinRequest.From.Id);
                    logger.inf_urgent(Geotag, $"CHREQUEST: ({++appCntr}) " +
                                    $"{chatJoinRequest.InviteLink?.InviteLink} " +
                                    $"{chatJoinRequest.From.Id} " +
                                    $"{chatJoinRequest.From.FirstName} " +
                                    $"{chatJoinRequest.From.LastName} " +
                                    $"{chatJoinRequest.From.Username}");                  

                } else
                {
                    await bot.DeclineChatJoinRequest(chatJoinRequest.Chat.Id, chatJoinRequest.From.Id);
                    logger.err(Geotag, $"DECLINED: ({++decCntr}) " +
                                    $"{chatJoinRequest.InviteLink?.InviteLink} " +
                                    $"{chatJoinRequest.From.Id} " +
                                    $"{chatJoinRequest.From.FirstName} " +
                                    $"{chatJoinRequest.From.LastName} " +
                                    $"{chatJoinRequest.From.Username}");

                    errCollector.Add($"Отказ на подписку в канал {ChannelTag}: {chatJoinRequest.From.Id} {chatJoinRequest.From.FirstName} {chatJoinRequest.From.LastName} {chatJoinRequest.From.Username}");

                    try
                    {
                        await server.MarkFollowerWasDeclined(ChannelTag, chat);

                    } catch (Exception ex)
                    {
                        logger.err(Geotag, $"processChatJoinRequest declineSubs: {ChannelTag} {chat} {ex.Message}");
                        errCollector.Add(errorMessageGenerator.getSubscriptionDeclinedError(ChannelTag, chat, ex));
                    }

                }

                await linksProcessor.Revoke(channelID, chatJoinRequest.InviteLink.InviteLink);
                
            }
            catch (Exception ex)
            {
                logger.err(Geotag, $"processChatJoinRequest {ex.Message}");
            }
        }

        //регистрация подписки/в канал в канал в бд
        protected override async Task processChatMember(Update update, CancellationToken cancellationToken)
        {
            try
            {

                if (update.ChatMember == null)
                    return;

                var member = update.ChatMember;
                long user_id = member.NewChatMember.User.Id;
                long chat_id = update.ChatMember.Chat.Id;

                string fn = member.NewChatMember.User.FirstName;
                string ln = member.NewChatMember.User.LastName;
                string un = member.NewChatMember.User.Username;

                string uuid = "";

                string link = member.InviteLink?.InviteLink;

                List<Follower> followers = new();
                var follower = new Follower()
                {
                    tg_chat_id = chat_id,
                    tg_user_id = user_id,
                    username = un,
                    firstname = fn,
                    lastname = ln,
                    invite_link = link,
                    office_id = (int)Offices.KRD,
                    tg_geolocation = ChannelTag
                };

                switch (member.NewChatMember.Status)
                {
                   case ChatMemberStatus.Member:

                        if (member.InviteLink != null && member.InviteLink.CreatesJoinRequest)
                        {
                            follower.is_subscribed = true;
                            follower.fb_event_send = true;

                            try
                            {
                                followers.Add(follower);
                                await server.UpdateFollowers(followers);
                            }
                            catch (Exception ex)
                            {
                                logger.err(Geotag, $"processChatMember: JOIN DB ERROR {user_id} {ex.Message}");
                            }

                            logger.inf_urgent(Geotag, $"CHJOINED: {ChannelTag} {link} {user_id} {fn} {ln} {un} event={follower.fb_event_send}");

                        }
                        break;

                    case ChatMemberStatus.Left:

                        try
                        {
                            var m = MessageProcessor.GetMessage($"BYE", pm: PM);
                            checkMessage(m, $"BYE", "processChatMember");
                            await m.Send(user_id, bot);                            
                        }
                        catch (Exception ex)
                        {
                            logger.err(Geotag, $"processChatMember {ex.Message}");
                        }

                        follower.is_subscribed = false;
                        followers.Add(follower);

                        try
                        {
                            await server.UpdateFollowers(followers);
                        }
                        catch (Exception ex)
                        {
                            logger.err(Geotag, $"processChatMember: LEFT DB ERROR {user_id}");
                        }

                        logger.inf(Geotag, $"CHLEFT: {Channel} {user_id} {fn} {ln} {un}");
                        break;
                }
            }
            catch (Exception ex)
            {
                logger.err(Geotag, $"processChatMember: {ex.Message}");
            }
        }
        #endregion

        #region public
        public override Task Start()
        {
            return base.Start().ContinueWith(async (t) => {
                try
                {
                    var channels = await ChannelsProvider.getInstance().GetChannels();
                    var found = channels.FirstOrDefault(c => c.geotag == ChannelTag);
                    if (found == null)
                    {
                        logger.err(Geotag, $"GetChannels: No channel ID");
                        Stop();
                    }
                    else
                    {

                        string schatID = $"-100{found.tg_id}";
                        long chatid = long.Parse(schatID);
                        channelID = chatid;

                        linksProcessor = new DynamicInviteLinkProcessor(ChannelTag, bot, api, logger);
                        linksProcessor.UpdateChannelID(channelID);
                        linksProcessor.StartLinkNumberControl(channelID, cts);
                    }
                } catch (Exception ex)
                {
                    Stop();
                    logger.err(Geotag, $"GetChannels: {ex.Message}");
                }
            
            });
        }

        public override void Stop()
        {
            base.Stop();            
        }
        #endregion

        public class pushStartProcess
        {
            #region vars        
            CancellationTokenSource cts;
            ITelegramBotClient bot;
            MessageProcessorBase mp;
            ILogger logger;
            string geotag;
            Action<PushMessageBase?, string, string> checkMessage;
            #endregion

            #region properties
            public long chat { get; set; }
            public bool is_running { get; set; }
            #endregion

            public pushStartProcess(string geotag, long chat, ITelegramBotClient bot, MessageProcessorBase mp, ILogger logger, Action<PushMessageBase?, string, string> checkMessage)
            {
                this.geotag = geotag;
                this.chat = chat;
                this.bot = bot;
                this.mp = mp;
                this.logger = logger;
                this.checkMessage = checkMessage;


                cts = new CancellationTokenSource();
            }

            async void worker()
            {
                is_running = true;

                try
                {

                    for (int i = 0; i < mp.start_push_number; i++)
                    {
                        try
                        {
                            cts.Token.ThrowIfCancellationRequested();

                            PushMessageBase m = null;
                            ReplyKeyboardMarkup b = null;

                            (m, b) = mp.GetMessageAndReplyMarkup($"hi_{i}_in");
                            checkMessage(m, $"hi_{i}_in", "pushStartProcess");
                            await m.Send(chat, bot, b);
                            logger.dbg(geotag, $"{chat} > pushStartProcess sent {i}");
                            await Task.Delay(45000, cancellationToken: cts.Token);
                        }
                        catch (OperationCanceledException ex)
                        {
                            logger.dbg(geotag, $"{chat} > pushStartProcess stopped");
                            break;
                        }
                        catch (Exception ex)
                        {
                            logger.err(geotag, $"{chat} > pushStartProcess: unable to send start message {i}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.err(geotag, $"{chat} > worker {ex.Message}");
                } finally
                {
                    is_running = false;
                }
            }

            public void start()
            {
                Task.Run(() => worker());
            }
            public void stop()
            {
                cts?.Cancel();
            }
        }
    }    
}
