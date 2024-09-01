using asknvl.logger;
using asknvl.server;
using botservice.Model.bot;
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
        #endregion

        protected GroupManagerBotBase(BotModel model, IOperatorStorage operatorStorage, IBotStorage botStorage, ILogger logger) : base(model, operatorStorage, botStorage, logger)
        {
            this.model = model;
            ChannelTag = model.channel_tag;            
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

        protected override async Task processFollower(Message message)
        {
            await Task.CompletedTask;
        }

        protected override async Task processOperator(Message message, Operator op)
        {
            try
            {
                var chat = message.From.Id;

                if (message.Text != null)
                {
                    switch (message.Text)
                    {
                        case "/start":
                            await sendOperatorTextMessage(op, chat, $"{op.first_name} {op.last_name}, вы вошли как оператор");
                            op.state = State.free;
                            op.ClearCash();
                            break;

                        //case OperatorCommands.ADD_VOUCHER:
                        //    break;
                    }
                }

            } catch (Exception ex)
            {
                logger.err(Geotag, $"");
            }
        }

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
                        ChannelId = chatid;

                        //var link = await bot.CreateChatInviteLinkAsync(channelID, memberLimit: 1);
                    }

                }
                catch (Exception ex)
                {
                    Stop();
                    logger.err(Geotag, $"GetChannels: {ex.Message}");
                }               

            });
        }

        public override async Task Notify(object notifyObject)
        {
            await Task.CompletedTask;
        }

    }
}
