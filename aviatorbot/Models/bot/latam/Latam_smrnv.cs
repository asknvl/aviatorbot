using aksnvl.messaging;
using asknvl.logger;
using asknvl.server;
using aviatorbot.Models.messages.latam;
using botservice.Model.bot;
using botservice.Models.bot.latam;
using botservice.Models.messages;
using botservice.Models.storage;
using botservice.Operators;
using botservice.rest;
using motivebot.Model.storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace aviatorbot.Models.bot.latam
{
    public class Latam_smrnv : LatamBotBase
    {

        #region vars
        Dictionary<long, int> pushStartCounters = new Dictionary<long, int>();
        List<pushStartProcess> pushStartProcesses = new List<pushStartProcess>();
        object lockObject = new object();
        #endregion

        public override BotType Type => BotType.latam_smrnv;

        public Latam_smrnv(BotModel model, IOperatorStorage operatorStorage, IBotStorage botStorage, ILogger logger) : base(model, operatorStorage, botStorage, logger)
        {

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
                var fn = message.From.Username;
                var ln = message.From.FirstName;
                var un = message.From.LastName;
                bool is_new = true;

                var found = pushStartProcesses.FirstOrDefault(p => p.chat ==  chat);
                if (found != null)
                {
                    found.stop();
                    await Task.Delay(1000);
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
                    pushStartCounters[chat] = pushStartCounters[chat]++ % ((MP_latam_smrnv)MessageProcessor).start_push_number;
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
                    }
                    catch (Exception ex)
                    {
                        logger.err(Geotag, $"{userInfo} DB ERROR {ex.Message}");
                    }
                }

                userInfo = $"{chat} {fn} {ln} {un}";

            }
            catch (Exception ex)
            {
            }
        }

        //начало стартовых пушей, аппрув в канал
        int appCntr = 0;
        protected override async Task processChatJoinRequest(ChatJoinRequest chatJoinRequest, CancellationToken cancellationToken)
        {
            try
            {
                var chat = chatJoinRequest.From.Id;

                var found = pushStartProcesses.FirstOrDefault(p => p.chat == chat);
                if (found == null)
                {
                    var newProcess = new pushStartProcess(Geotag, chat, bot, (MP_latam_smrnv)MessageProcessor, logger, checkMessage);
                    lock (lockObject)
                    {
                        pushStartProcesses.Add(newProcess);
                    }
                    newProcess.start();
                }

                await bot.ApproveChatJoinRequest(chatJoinRequest.Chat.Id, chatJoinRequest.From.Id);
                logger.inf_urgent(Geotag, $"CHREQUEST: ({++appCntr}) " +
                                $"{Channel} " +
                                $"{chatJoinRequest.From.Id} " +
                                $"{chatJoinRequest.From.FirstName} " +
                                $"{chatJoinRequest.From.LastName} " +
                                $"{chatJoinRequest.From.Username}");
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

                        follower.is_subscribed = true;

                        try
                        {
                            followers.Add(follower);
                            await server.UpdateFollowers(followers);
                        }
                        catch (Exception ex)
                        {
                            logger.err(Geotag, $"processChatMember: JOIN DB ERROR {user_id}");
                        }

                        logger.inf_urgent(Geotag, $"CHJOINED: {Channel} {user_id} {fn} {ln} {un}");
                        break;

                    case ChatMemberStatus.Left:

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
        #endregion

    }

    public class pushStartProcess
    {
        #region vars        
        CancellationTokenSource cts;
        ITelegramBotClient bot;
        MP_latam_smrnv mp;
        ILogger logger;
        string geotag;
        Action<PushMessageBase?, string, string> checkMessage;        
        #endregion

        #region properties
        public long chat { get; set; }
        public bool is_running { get; set; }
        #endregion

        public pushStartProcess(string geotag, long chat, ITelegramBotClient bot, MP_latam_smrnv mp, ILogger logger, Action<PushMessageBase?, string, string> checkMessage)
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
                        await Task.Delay(5000, cancellationToken: cts.Token);
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
