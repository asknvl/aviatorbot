using asknvl.logger;
using asknvl.server;
using botservice.Model.bot;
using botservice.Models.bot;
using botservice.Models.bot.latam;
using botservice.Models.messages;
using botservice.Models.storage;
using botservice.rest;
using DynamicData;
using motivebot.Model.storage;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace botservice.Models.bot.aviator
{
    public abstract class AviatorModeratorBotBase : ModeratorBotBase, IPushObserver, IStatusObserver
    {
        #region vars
        BotModel tmpBotModel;
        #endregion

        #region properies      
        string? support_pm;
        public string? SUPPORT_PM
        {
            get => support_pm;
            set => this.RaiseAndSetIfChanged(ref support_pm, value);
        }

        string? pm;
        public string? PM
        {
            get => pm;
            set => this.RaiseAndSetIfChanged(ref pm, value);
        }

        string help;
        public string Help
        {
            get => help;
            set => this.RaiseAndSetIfChanged(ref help, value);
        }

        string training;
        public string Training
        {
            get => training;
            set => this.RaiseAndSetIfChanged(ref training, value);
        }

        string reviews;
        public string Reviews
        {
            get => reviews;
            set => this.RaiseAndSetIfChanged(ref reviews, value);
        }

        string strategy;
        public string Strategy
        {
            get => strategy;
            set => this.RaiseAndSetIfChanged(ref strategy, value);
        }

        string vip;
        public string Vip
        {
            get => vip;
            set => this.RaiseAndSetIfChanged(ref vip, value);
        }
        #endregion

        protected AviatorModeratorBotBase(BotModel model, IOperatorStorage operatorStorage, IBotStorage botStorage, ILogger logger) : base(model, operatorStorage, botStorage, logger)
        {

            Link = model.link;

            SUPPORT_PM = model.support_pm;
            PM = model.pm;
            ChannelTag = model.channel_tag;
            Channel = model.channel;
            ChApprove = model.channel_approve;

            Help = model.help;
            Training = model.training;
            Reviews = model.reveiews;
            Strategy = model.strategy;
            Vip = model.vip;

            Postbacks = model.postbacks;

            #region commands
            editCmd = ReactiveCommand.Create(() =>
            {

                tmpBotModel = new BotModel()
                {
                    type = Type,
                    geotag = Geotag,
                    token = Token,

                    support_pm = SUPPORT_PM,

                    link = Link,
                    pm = PM,
                    channel = Channel,
                    channel_approve = ChApprove,

                    help = Help,
                    training = Training,
                    reveiews = Reviews,
                    strategy = Strategy,
                    vip = Vip,

                    postbacks = Postbacks
                };

                IsEditable = true;
            });

            cancelCmd = ReactiveCommand.Create(() =>
            {

                Geotag = tmpBotModel.geotag;
                Token = tmpBotModel.token;
                Link = tmpBotModel.link;

                SUPPORT_PM = tmpBotModel.support_pm;
                PM = tmpBotModel.pm;
                Channel = tmpBotModel.channel;
                ChApprove = tmpBotModel.channel_approve;

                Help = tmpBotModel.help;
                Training = tmpBotModel.training;
                Reviews = tmpBotModel.reveiews;
                Strategy = tmpBotModel.strategy;
                Vip = tmpBotModel.vip;

                Postbacks = tmpBotModel.postbacks;

                IsEditable = false;

            });

            saveCmd = ReactiveCommand.Create(() =>
            {


                var updateModel = new BotModel()
                {
                    type = Type,
                    geotag = Geotag,
                    token = Token,
                    link = Link,
                    support_pm = SUPPORT_PM,
                    pm = PM,
                    channel = Channel,
                    channel_approve = ChApprove,

                    help = Help,
                    training = Training,
                    reveiews = Reviews,
                    strategy = Strategy,
                    vip = Vip,

                    postbacks = Postbacks
                };

                botStorage.Update(updateModel);

                IsEditable = false;

            });
            #endregion

        }

        #region override
        //лид пишет боту или нажимает на кнопку
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

                var found = pushStartProcesses.FirstOrDefault(p => p.chat == chat);
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
                }
                else
                {
                    var cnt = pushStartCounters[chat];
                    cnt++;
                    cnt %= MessageProcessor.start_push_number;
                    pushStartCounters[chat] = cnt;
                    is_new = false;
                }

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

                string uuid = "udef";

                try
                {
                    var statusResponce = await server.GetFollowerStateResponse(Geotag, chat);
                    uuid = statusResponce.uuid;
                }
                catch (Exception ex)
                {
                    logger.err(Geotag, $"{userInfo} GET UUID {ex.Message}");
                }

                try
                {
                    var m = MessageProcessor.GetMessage($"hi_out", link: Link, pm: PM, uuid: uuid);
                    checkMessage(m, $"hi_out", "processFollower");
                    await m.Send(chat, bot);
                    pushStartCounters[chat] = index;
                }
                catch (Exception ex)
                {
                    logger.err(Geotag, $"processFollower {ex.Message}");
                }

            }
            catch (Exception ex)
            {
            }
        }
        #endregion

        #region callbacs
        public async Task<bool> Push(long id, string code, string uuid, int notification_id, string? firstname)
        {

            var op = operatorStorage.GetOperator(Geotag, id);
            if (op != null)
            {
                logger.err(Geotag, $"Push: {id} Попытка отправки пуша оператору");
                return false;
            }

            bool res = false;
            try
            {

                var statusResponce = await server.GetFollowerStateResponse(Geotag, id);
                var status = statusResponce.status_code;

                StateMessage push = null;

                try
                {
                    push = MessageProcessor.GetPush(statusResponce, code, link: Link, support_pm: SUPPORT_PM, pm: PM, isnegative: false, vip: Vip, help: Help);
                    checkMessage(push, code, "Push");
                }
                catch (Exception ex)
                {
                    logger.err(Geotag, $"Push: {id} {ex.Message} (0)");
                    await server.SlipPush(notification_id, false);
                }

                if (push != null)
                {
                    try
                    {
                        await push.Send(id, bot);
                        res = true;
                        logger.inf(Geotag, $"PUSHED: {id} {status} {code}");

                    }
                    catch (Exception ex)
                    {
                        logger.err(Geotag, $"Push: {ex.Message} (1)");

                    } finally
                    {
                        await server.SlipPush(notification_id, res);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.err(Geotag, $"Push: {ex.Message} (2)");
            }
            return res;
        }
        public abstract Task UpdateStatus(StatusUpdateDataDto updateData);
        #endregion
    }
}
