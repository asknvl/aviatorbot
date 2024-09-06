using asknvl.logger;
using asknvl.server;
using botplatform.Models.server;
using botservice.Model.bot;
using botservice.Models.messages;
using botservice.Models.storage;
using botservice.Operators;
using csb.invitelinks;
using motivebot.Model.storage;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace botservice.Models.bot.gmanager
{
    public abstract class GroupManagerBotBase : BotBase
    {
        #region const
        public class OperatorCommands {
            public static string ADD_VOUCHER = "ADD_VOUCHER";
            public static string GIVE_VOUCHER = "GIVE_VOUCHER";
        }
        #endregion

        #region vars
        BotModel model;
        BotModel tmpBotModel;
        IMessageProcessorFactory messageProcessorFactory;

        ChatMember[] admins;
        ChatMember[] members;

        AIServer ai = new AIServer("https://gpt.raceup.io");
        Dictionary<long, string> aggrMessages = new();
        #endregion

        #region properties
        string channelTag;
        public string ChannelTag
        {
            get => channelTag;
            set => this.RaiseAndSetIfChanged(ref  channelTag, value);
        }

        long? channelId;
        public long? ChannelId
        {
            get => channelId;
            set => this.RaiseAndSetIfChanged(ref channelId, value);
        }

        string? pm;
        public string? PM
        {
            get => pm;
            set => this.RaiseAndSetIfChanged(ref pm, value);
        }

        string? registerSource;
        public string? RegisterSource
        {
            get => registerSource;
            set => this.RaiseAndSetIfChanged(ref registerSource, value);
        }

        string? registerSourceLink;
        public string? RegisterSourceLink
        {
            get => registerSourceLink;
            set => this.RaiseAndSetIfChanged(ref registerSourceLink, value);
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

        protected GroupManagerBotBase(BotModel model, IOperatorStorage operatorStorage, IBotStorage botStorage, ILogger logger) : base(model, operatorStorage, botStorage, logger)
        {
            this.model = model;

            this.logger = logger;
            this.operatorStorage = operatorStorage;
            this.botStorage = botStorage;

            ChannelTag = model.channel_tag;
            RegisterSource = model.register_source;
            RegisterSourceLink = model.register_source_link;
            PM = model.pm;
            

            editCmd = ReactiveCommand.Create(() => {
                tmpBotModel = new BotModel()
                {
                    geotag = Geotag,
                    token = Token,
                    channel_tag = ChannelTag,       
                    pm = PM,
                    register_source = RegisterSource,
                    register_source_link = RegisterSourceLink,
                    channel_approve = ChApprove,
                    postbacks = Postbacks,

                };
                IsEditable = true;
            });

            cancelCmd = ReactiveCommand.Create(() => {
                Geotag = tmpBotModel.geotag;
                Token = tmpBotModel.token;
                ChannelTag = tmpBotModel.channel_tag;
                PM = tmpBotModel.pm;
                RegisterSource = tmpBotModel.register_source;
                RegisterSourceLink = tmpBotModel.register_source_link;
                ChApprove = tmpBotModel.channel_approve;
                Postbacks = tmpBotModel.postbacks;
                IsEditable = false;
            });

            saveCmd = ReactiveCommand.Create(() => {
                var updateModel = new BotModel()
                {
                    geotag = Geotag,
                    token = Token,
                    channel_tag = ChannelTag,
                    pm = PM,
                    register_source = RegisterSource,
                    register_source_link = RegisterSourceLink,
                    channel_approve = ChApprove,
                    postbacks = Postbacks
                };
                botStorage.Update(updateModel);
                IsEditable = false;
            });
        }

        #region helpers
        async Task sendOperatorTextMessage(Operator op, long chat, string text)
        {
            ReplyKeyboardMarkup replyKeyboardMarkup = null;
            if (op.permissions.Any(p => p.type.Equals(OperatorPermissionType.set_user_status)) || op.permissions.Any(p => p.type.Equals(OperatorPermissionType.all)))
            {

                replyKeyboardMarkup = new(new[]
                        {
                            new KeyboardButton[] { $"{OperatorCommands.ADD_VOUCHER}" },
                            new KeyboardButton[] { $"{OperatorCommands.GIVE_VOUCHER}" }                            
                        })
                {
                    ResizeKeyboard = true,
                    OneTimeKeyboard = false,
                };
            }
            else
            {
            }

            await bot.SendTextMessageAsync(
                chat,
                text: text,
                replyMarkup: replyKeyboardMarkup
                /*parseMode: ParseMode.MarkdownV2*/);
        }
        #endregion

        //подписали бота в группу
        protected override async Task processSubscribe(Update update)
        {   
            await Task.CompletedTask;
        }

        //запрос на добавление в группу
        int appCntr = 0;
        protected override async Task processChatJoinRequest(ChatJoinRequest chatJoinRequest, CancellationToken cancellationToken)
        {
            try
            {

                await bot.ApproveChatJoinRequest(chatJoinRequest.Chat.Id, chatJoinRequest.From.Id);

                logger.inf_urgent(Geotag, $"GREQUEST: ({++appCntr}) " +
                                    $"{chatJoinRequest.InviteLink?.InviteLink} " +
                                    $"{chatJoinRequest.From.Id} " +
                                    $"{chatJoinRequest.From.FirstName} " +
                                    $"{chatJoinRequest.From.LastName} " +
                                    $"{chatJoinRequest.From.Username}");


            } catch (Exception ex)
            {
                logger.err(Geotag, $"processChatJoinRequest {ex.Message}");
            }
        }

        //добавлили/удалили нового члена группы
        protected override async Task processChatMember(Update update, CancellationToken cancellationToken)
        {
            try
            {
                var chatMember = update.ChatMember;
                var newChatMember = update.ChatMember.NewChatMember;

                var tg_chat_id = chatMember.Chat.Id;
                var tg_user_id = newChatMember.User.Id;
                var fn = newChatMember.User.FirstName;
                var ln = newChatMember.User.LastName;
                var un = newChatMember.User.Username;

                List<Follower> followers = new();
                var follower = new Follower()
                {
                    tg_chat_id = tg_chat_id,
                    tg_user_id = tg_user_id,
                    firstname = fn,
                    lastname = ln,
                    username = un,
                    office_id = (int)Offices.KRD,
                    tg_geolocation = ChannelTag
                };

                switch (newChatMember)
                {
                    case ChatMemberMember:
                        follower.is_subscribed = true;
                        follower.invite_link = chatMember.InviteLink?.InviteLink;
                        break;
                    case ChatMemberLeft:
                        follower.is_subscribed = false;
                        break;
                }

                followers.Add(follower);
                await server.UpdateFollowers(followers);
                logger.inf(Geotag, $"{follower}");

            } catch (Exception ex)
            {
                logger.err(Geotag, $"processChatMember: {ex.Message}");
            }            
        }

        protected override async Task processCallbackQuery(CallbackQuery query)
        {
            await Task.CompletedTask;
        }


        string normalize(string text)
        {
            return text?.Replace(" ", "");
        }

        protected override async Task processFollower(Message message)
        {
            long? from_id = null;
            string? from_fn;
            string? from_ln;


            switch (message.Type)
            {
                case Telegram.Bot.Types.Enums.MessageType.ChatMemberLeft:
                case Telegram.Bot.Types.Enums.MessageType.ChatMembersAdded:
                    return;
            }

            try
            {
                if (message != null)
                {
                    if (message.Chat.Id == ChannelId)
                    {
                        from_id = message.From.Id;
                        from_fn = message.From.FirstName;
                        from_ln = message.From.LastName;

                        var isAdmin = admins.Any(a => a.User.Id == from_id);
                        if (!isAdmin)
                        {
                            var chkRes = await ai.Moderate(text: normalize(message.Text));

                            var msg = $"GRMSG: {from_id} {from_fn} {from_ln} text={message.Text} adm={isAdmin} chk={chkRes}";

                            switch (chkRes)
                            {
                                case "BAN":                                  
                                    await bot.DeleteMessageAsync(message.Chat.Id, message.MessageId);
                                    logger.err(Geotag, msg);
                                    break;

                                default:
                                    logger.inf(Geotag, msg);
                                    break;
                            }
                            
                        }                        
                    }
                }
            } catch (Exception ex)
            {
                logger.err(Geotag, $"processFollower: {from_id} {ex.Message}");
            }
        }

        protected override async Task processOperator(Message message, Operator op)
        {
            try
            {
                var chat = message.From.Id;

                //if (message.Text != null)
                //{
                //    switch (message.Text)
                //    {
                //        case "/start":
                //            await sendOperatorTextMessage(op, chat, $"{op.first_name} {op.last_name}, вы вошли как оператор");
                //            op.state = State.free;
                //            op.ClearCash();
                //            break;

                //        //case OperatorCommands.ADD_VOUCHER:
                //        //    break;
                //    }
                //}

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

            } catch (Exception ex)
            {
                logger.err(Geotag, $"");
            }
        }

        
        public override Task Start()
        {
            return base.Start().ContinueWith(async (t) => {

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

                //ChannelId = -1002186025715;

                ChannelId = null;

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
                        ChannelId = chatid;                      

                        //var link = await bot.CreateChatInviteLinkAsync(channelID, memberLimit: 1);
                    }

                }
                catch (Exception ex)
                {
                    Stop();
                    logger.err(Geotag, $"GetChannels: {ex.Message}");
                }

                try
                {
                    admins = await bot.GetChatAdministratorsAsync(ChannelId);

                } catch (Exception ex)
                {
                    logger.err(Geotag, $"getAdmins: {ex.Message}");
                }
                

            });
        }

        public override async Task Notify(object notifyObject)
        {
            await Task.CompletedTask;
        }

    }
}
