using asknvl.logger;
using botservice.Model.bot;
using botservice.Models.bot;
using botservice.Models.messages;
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
using Telegram.Bot.Types;

namespace aviatorbot.Models.bot.aviator
{
    public abstract class LandingBot_raceup_tier1_postback_base : BotBase, IPushObserver
    {
        #region vars
        IMessageProcessorFactory messageProcessorFactory;
        BotModel tmpBotModel;
        #endregion

        #region properties    
        string link_reg;
        public string LinkReg
        {
            get => link_reg;
            set => this.RaiseAndSetIfChanged(ref link_reg, value);
        }

        string link_dep;
        public string LinkDep
        {
            get => link_dep;
            set => this.RaiseAndSetIfChanged(ref link_dep, value);
        }

        string link_gam;
        public string LinkGam
        {
            get => link_gam;
            set => this.RaiseAndSetIfChanged(ref link_gam, value);
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

        bool? chApprove;
        public bool? ChApprove
        {
            get => chApprove;
            set => this.RaiseAndSetIfChanged(ref chApprove, value);
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

        public LandingBot_raceup_tier1_postback_base(BotModel model, IOperatorStorage operatorStorage, IBotStorage botStorage, ILogger logger) : base(model, operatorStorage, botStorage, logger)
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
                    link_reg = LinkReg,
                    link_dep = LinkDep,
                    link_gam = LinkGam,
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

                IsEditable = true;
            });

            cancelCmd = ReactiveCommand.Create(() =>
            {

                Geotag = tmpBotModel.geotag;
                Token = tmpBotModel.token;

                LinkReg = tmpBotModel.link_reg;
                LinkDep = tmpBotModel.link_dep;
                LinkGam = tmpBotModel.link_gam;

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
                    
                    link_reg = LinkReg,
                    link_dep = LinkDep,
                    link_gam = LinkGam,

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

        public override Task Notify(object notifyObject)
        {
            throw new NotImplementedException();
        }

        protected override Task processFollower(Message message)
        {
            throw new NotImplementedException();
        }

        protected override Task processOperator(Message message, Operator op)
        {
            throw new NotImplementedException();
        }

        protected override Task processSubscribe(Update update)
        {
            throw new NotImplementedException();
        }

        protected override Task processChatJoinRequest(ChatJoinRequest chatJoinRequest, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        protected override Task processChatMember(Update update, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        protected override Task processCallbackQuery(CallbackQuery query)
        {
            throw new NotImplementedException();
        }

        public Task<bool> Push(long tg_id, string code, string uuid, int notification_id, string? firstname)
        {
            throw new NotImplementedException();
        }
    }
}
