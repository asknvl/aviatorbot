using aksnvl.messaging;
using asknvl.logger;
using asknvl.server;
using Avalonia.X11;
using botservice.Model.bot;
using botservice.Models.bot;
using botservice.Models.messages;
using botservice.Models.storage;
using botservice.Operators;
using botservice.rest;
using botservice.ViewModels;
using HarfBuzzSharp;
using motivebot.Model.storage;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reactive;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Requests;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using static System.Net.Mime.MediaTypeNames;

namespace botservice.Models.bot.aviator
{
    public abstract class AviatorBotBase : BotBase, IPushObserver, IStatusObserver
    {

        #region vars                
        IMessageProcessorFactory messageProcessorFactory;
        BotModel tmpBotModel;
        #endregion

        #region properties                
        string? link;
        public string? Link
        {
            get => link;
            set => this.RaiseAndSetIfChanged(ref link, value);
        }

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

        string? channel_tag;
        public string? ChannelTag
        {
            get => channel_tag;
            set => this.RaiseAndSetIfChanged(ref channel_tag, value);
        }

        string? channel;
        public string? Channel
        {
            get => channel;
            set => this.RaiseAndSetIfChanged(ref channel, value);
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

        MessageProcessorBase messageProcessor;
        public MessageProcessorBase MessageProcessor
        {
            get => messageProcessor;
            set => this.RaiseAndSetIfChanged(ref messageProcessor, value);
        }

        string awaitedMessageCode;
        public string AwaitedMessageCode
        {
            get => awaitedMessageCode;
            set => this.RaiseAndSetIfChanged(ref awaitedMessageCode, value);
        }
        #endregion

        public AviatorBotBase(BotModel model, IOperatorStorage operatorStorage, IBotStorage botStorage, ILogger logger) : base(model, operatorStorage, botStorage, logger)
        {
            this.logger = logger;
            this.operatorStorage = operatorStorage;
            this.botStorage = botStorage;

            #region commands
            editCmd = ReactiveCommand.Create(() =>
            {

                tmpBotModel = new BotModel()
                {
                    type = Type,
                    geotag = Geotag,
                    token = Token,
                    link = Link,
                    pm = PM,
                    channel = Channel,

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
                PM = tmpBotModel.pm;
                Channel = tmpBotModel.channel;

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
                    pm = PM,
                    channel = Channel,

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

        #region private
        override protected async Task processOperator(Message message, Operator op)
        {

            var chat = message.From.Id;

            try
            {
                if (state == State.waiting_new_message)
                {
                    MessageProcessor.Add(AwaitedMessageCode, message, PM);
                    state = State.free;
                    return;
                }
            }
            catch (Exception ex)
            {
                logger.err(Geotag, ex.Message);
            }
        }

        override protected async Task processSubscribe(Update update)
        {

            long chat = 0;
            string fn = string.Empty;
            string ln = string.Empty;
            string un = string.Empty;
            string uuid = string.Empty;
            string status = string.Empty;
            string direction = "";

            try
            {

                if (update.MyChatMember != null)
                {

                    var mychatmember = update.MyChatMember;

                    chat = mychatmember.From.Id;
                    fn = mychatmember.From.FirstName;
                    ln = mychatmember.From.LastName;
                    un = mychatmember.From.Username;
                    uuid = "";
                    status = "";

                    List<Follower> followers = new();
                    var follower = new Follower()
                    {
                        tg_chat_id = ID,
                        tg_user_id = chat,
                        username = un,
                        firstname = fn,
                        lastname = ln,
                        office_id = (int)Offices.KRD,
                        tg_geolocation = Geotag
                    };

                    switch (mychatmember.NewChatMember.Status)
                    {
                        case ChatMemberStatus.Member:
                            follower.is_subscribed = true;
                            direction = "UNBLOCK";
                            followers.Add(follower);
                            await server.UpdateFollowers(followers);
                            break;

                        case ChatMemberStatus.Kicked:
                            direction = "BLOCK";
                            follower.is_subscribed = false;
                            followers.Add(follower);
                            await server.UpdateFollowers(followers);
                            break;

                        default:
                            return;
                    }

                    (uuid, status) = await server.GetFollowerState(Geotag, chat);
                }

            }
            catch (Exception ex)
            {
                logger.err(Geotag, $"processSubscribe: {ex.Message}");
            }
            finally
            {
                var msg = $"{direction}: {chat} {fn} {ln} {un} {uuid} {status}";
                logger.inf(Geotag, msg); // logout JOIN or LEFT
            }
        }

        public override async Task Start()
        {
            await base.Start().ContinueWith(t =>
            {

                messageProcessorFactory = new MessageProcessorFactory(logger);

                MessageProcessor = messageProcessorFactory.Get(Type, Geotag, Token, bot);

                if (MessageProcessor != null)
                {
                    MessageProcessor.UpdateMessageRequestEvent += async (code, description) =>
                    {
                        AwaitedMessageCode = code;
                        state = State.waiting_new_message;

                        //var operators = operatorsProcessor.GetAll(geotag).Where(o => o.permissions.Any(p => p.type.Equals(OperatorPermissionType.all)));
                        var operators = operatorStorage.GetAll(Geotag).Where(o => o.permissions.Any(p => p.type.Equals(OperatorPermissionType.all)));

                        foreach (var op in operators)
                        {
                            try
                            {
                                await bot.SendTextMessageAsync(op.tg_id, $"Перешлите сообщение для: \n{description.ToLower()}");
                            }
                            catch (Exception ex)
                            {
                                logger.err(Geotag, $"UpdateMessageRequestEvent: {ex.Message}");
                            }
                        }
                    };

                    MessageProcessor.ShowMessageRequestEvent += async (message, code) =>
                    {
                        //var operators = operatorsProcessor.GetAll(geotag).Where(o => o.permissions.Any(p => p.type.Equals(OperatorPermissionType.all)));                
                        var operators = operatorStorage.GetAll(Geotag).Where(o => o.permissions.Any(p => p.type.Equals(OperatorPermissionType.all)));

                        foreach (var op in operators)
                        {
                            try
                            {
                                int id = await message.Send(op.tg_id, bot);
                            }
                            catch (Exception ex)
                            {
                                logger.err(Geotag, $"ShowMessageRequestEvent: {ex.Message}");
                            }
                        }
                    };

                    MessageProcessor.Init();
                }

            });
        }
        #endregion

        #region callbacks
        public abstract Task<bool> Push(long id, string code, int notification_id);
        public abstract Task UpdateStatus(StatusUpdateDataDto updateData);
        #endregion
    }
}
