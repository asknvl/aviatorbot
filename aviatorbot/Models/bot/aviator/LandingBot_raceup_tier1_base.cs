﻿using asknvl.logger;
using asknvl.messaging;
using asknvl.server;
using aviatorbot.Models.bot;
using botservice.Model.bot;
using botservice.Models.bot;
using botservice.Models.messages;
using botservice.Models.storage;
using botservice.Operators;
using motivebot.Model.storage;
using ReactiveUI;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace botservice.Models.bot.aviator
{
    public abstract class LandingBot_raceup_tier1_base : BotBase, IPushObserver
    {
        #region vars        
        IMessageProcessorFactory messageProcessorFactory;
        BotModel tmpBotModel;
        #endregion

        #region properties
        
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

        bool? chApprove;
        public bool? ChApprove
        {
            get => chApprove;
            set => this.RaiseAndSetIfChanged(ref chApprove, value);
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

        public LandingBot_raceup_tier1_base(BotModel model, IOperatorStorage operatorStorage, IBotStorage botStorage, ILogger logger) : base(model, operatorStorage, botStorage, logger)
        {
            Geotag = model.geotag;
            Token = model.token;
            ChannelTag = model.channel_tag;
            Channel = model.channel;
            ChApprove = model.channel_approve;
            PM = model.pm;

            #region commands
            editCmd = ReactiveCommand.Create(() =>
            {

                tmpBotModel = new BotModel()
                {
                    type = Type,
                    geotag = Geotag,
                    token = Token,
                    pm = PM,
                    channel_tag = ChannelTag,
                    channel = Channel,
                    channel_approve = ChApprove
                };

                IsEditable = true;
            });

            cancelCmd = ReactiveCommand.Create(() =>
            {

                Geotag = tmpBotModel.geotag;
                Token = tmpBotModel.token;

                PM = tmpBotModel.pm;
                ChannelTag = tmpBotModel.channel_tag;
                Channel = tmpBotModel.channel;
                ChApprove = tmpBotModel.channel_approve;

                IsEditable = false;

            });

            saveCmd = ReactiveCommand.Create(() =>
            {
                var updateModel = new BotModel()
                {
                    type = Type,
                    geotag = Geotag,
                    token = Token,
                    pm = PM,
                    channel_tag = ChannelTag,
                    channel = Channel,
                    channel_approve = ChApprove
                };

                botStorage.Update(updateModel);

                IsEditable = false;

            });
            #endregion
        }

        ConcurrentDictionary<long, Task> userPostingTasks = new();
        ConcurrentDictionary<long, CancellationTokenSource> cancellationTokens = new();

        void startPostingTask(long chat)
        {
            if (!userPostingTasks.ContainsKey(chat) || userPostingTasks[chat].IsCompleted)
            {
                var cts = new CancellationTokenSource();
                var token = cts.Token;
                cancellationTokens[chat] = cts;

                var task = Task.Run(async () => {

                    try
                    {
                        var m = MessageProcessor.GetMessage("start", pm: PM, channel: Channel);
                        await m.Send(chat, bot);
                        token.ThrowIfCancellationRequested();
                        await Task.Delay(3000, token);
                        m = MessageProcessor.GetMessage("circle", pm: PM, channel: Channel);
                        await m.Send(chat, bot);
                        token.ThrowIfCancellationRequested();
                        await Task.Delay(5 * 60 * 1000, token);
                        m = MessageProcessor.GetMessage("video", pm: PM, channel: Channel);
                        await m.Send(chat, bot);
                        token.ThrowIfCancellationRequested();
                        await Task.Delay(5 * 60 * 1000, token);
                        m = MessageProcessor.GetMessage("reg", pm: PM, channel: Channel);
                        await m.Send(chat, bot);

                        logger.inf(Geotag, $"startPostingTask: {chat} OK");
                    }
                    catch (OperationCanceledException ex)
                    {
                        logger.err(Geotag, $"startPostingTask: {chat} cancelled");
                    }
                    catch (Exception ex)
                    {
                        logger.err(Geotag, $"startPostingTask: {chat} {ex.Message}");
                    } finally
                    {
                        userPostingTasks.TryRemove(chat, out _);
                        cancellationTokens.TryRemove(chat, out _);
                    }                   

                });

                userPostingTasks[chat] = task;
            } else
            {
                logger.inf(Geotag, $"startPostingTask: {chat} restart was not allowed");
            }

        }

        protected virtual async Task<(string, bool)> getUserStatusOnStart(long tg_id)
        {
            string code = "";
            bool is_new = false;

            try
            {

                var subscribe = await server.GetFollowerSubscriprion(Geotag, tg_id);
                var isSubscribed = subscribe.Any(s => s.is_subscribed);

                code = "start";
                is_new = !isSubscribed;

            }
            catch (Exception ex)
            {
                logger.err(Geotag, $"getUserStatus: {ex.Message}");
            }

            return (code, is_new);
        }

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

                userInfo = $"{chat} {fn} {ln} {un}";

                if (message.Text.Contains("/start"))
                {
                    var parse_uuid = message.Text.Replace("/start", "").Trim();
                    var uuid = string.IsNullOrEmpty(parse_uuid) ? null : parse_uuid;

                    var msg = $"START: {userInfo} ?";
                    logger.inf(Geotag, msg);

                    string code = "";
                    bool is_new = false;

                    (code, is_new) = await getUserStatusOnStart(chat);

                    bool need_fb_event = is_new && !string.IsNullOrEmpty(uuid);

                    if (code.Equals("start"))
                    {
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
                            uuid = uuid,
                            fb_event_send = need_fb_event,
                            is_subscribed = true
                        };
                        followers.Add(follower);

                        try
                        {
                            await server.UpdateFollowers(followers);
                            msg = $"DB UPDATED: {userInfo} uuid={uuid} event={follower.fb_event_send}";
                            logger.inf(Geotag, msg);
                        }
                        catch (Exception ex)
                        {
                            logger.err(Geotag, $"{userInfo} DB ERROR {ex.Message}");
                            errCollector.Add(errorMessageGenerator.getAddUserBotDBError(userInfo));
                        }
                    }

                    startPostingTask(chat);
                }

            } catch (Exception ex)
            {
                logger.err(Geotag, $"processFollower: {ex.Message}");
            }            
        }

        protected override async Task processOperator(Message message, Operator op)
        {
            try
            {
                if (state == State.waiting_new_message)
                {
                    MessageProcessor.Add(AwaitedMessageCode, message, PM, channel: Channel);
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
                            try
                            {
                                if (cancellationTokens.TryGetValue(chat, out var ct))
                                    ct?.Cancel();
                            } catch (Exception ex)
                            {
                            }
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
            } finally
            {
                var msg = $"{direction}: {chat} {fn} {ln} {un} {uuid} {status}";
                logger.inf(Geotag, msg); // logout JOIN or LEFT
            }
        }

        int appCntr = 0;
        protected override async Task processChatJoinRequest(ChatJoinRequest chatJoinRequest, CancellationToken cancellationToken)
        {
            try
            {
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

        protected override Task processCallbackQuery(CallbackQuery query)
        {
            throw new NotImplementedException();
        }

        public override Task Notify(object notifyObject)
        {
            throw new NotImplementedException();
        }

        #region public
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

                StateMessage push = null;

                try
                {
                    var tmp = MessageProcessor.GetPush(code, channel: Channel, pm: PM);

                    if (!string.IsNullOrEmpty(firstname))
                    {
                        List<AutoChange> autoChange = new List<AutoChange>()
                        {
                            new AutoChange() {
                                OldText = "_fn_",
                                NewText = $"{firstname}"
                            }
                        };

                        push = tmp.Clone();
                        push.MakeAutochange(autoChange);
                    }
                    else
                        push = tmp;

                    if (push != null)
                    {
                        if (push.Message.Text != null && push.Message.Text.Contains("_fn_"))
                        {
                            int len = Math.Min(push.Message.Text.Length - 1, 20);
                            logger.err(Geotag, $"AutochangeErr msg: {id} {firstname} {push.Message.Text.Substring(0, len)}...");
                            errCollector.Add($"{code} ошибка автозамены имени лида id={id} fn={firstname}");
                        }

                        if (push.Message.Caption != null && push.Message.Caption.Contains("_fn_"))
                        {
                            int len = Math.Min(push.Message.Caption.Length - 1, 20);
                            logger.err(Geotag, $"AutochangeErr cap: {id} {firstname} {push.Message.Caption.Substring(0, len)}...");
                            errCollector.Add($"{code} ошибка автозамены имени лида id={id} fn={firstname}");
                        }
                    }

                    checkMessage(push, code, "Push");
                }
                catch (Exception ex)
                {
                    logger.err(Geotag, $"Push: {id} {code} {firstname} {ex.Message} (0)");
                    errCollector.Add($"{code} ошибка разметки");
                    await server.SlipPush(notification_id, false);
                }

                if (push != null)
                {
                    try
                    {
                        await push.Send(id, bot);
                        res = true;
                        logger.inf(Geotag, $"PUSHED: {id} {code} {firstname}");

                    }
                    catch (Exception ex)
                    {
                        logger.err(Geotag, $"Push: {id} {code} {firstname} {ex.Message} (1)");

                    } finally
                    {
                        await server.SlipPush(notification_id, res);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.err(Geotag, $"Push: {id} {code} {firstname} {ex.Message} (2)");
                await server.SlipPush(notification_id, false);
            }
            return res;
        }

        public virtual async Task<DiagnosticsResult> GetDiagnosticsResult()
        {
            DiagnosticsResult result = new DiagnosticsResult();

            result.botGeotag = Geotag;

            if (!IsActive)
            {
                result.isOk = false;
                result.errorsList.Add("Бот не активен");
            }
            else
            {
                try
                {
                    var me = await bot.GetMeAsync();
                }
                catch (Exception ex)
                {
                    result.isOk = false;
                    result.errorsList.Add(errorMessageGenerator.getBotApiError($"Не удалось выполнить запрос"));
                }

                var errors = errCollector.Get();
                if (errors.Length > 0)
                {
                    if (result.isOk)
                        result.isOk = false;

                    foreach (var error in errors)
                    {
                        result.errorsList.Add(error);
                    }
                }
            }

            errCollector.Clear();

            return result;
        }
        #endregion
    }
}