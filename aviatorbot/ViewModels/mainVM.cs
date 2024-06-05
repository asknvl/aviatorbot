using aviatorbot.Models.bot;
using aviatorbot.Models.user_storage;
using aviatorbot.rest;
using botservice.Model.bot;
using botservice.Models.bot;
using botservice.Models.bot.pusher;
using botservice.Models.storage;
using botservice.Models.storage.local;
using botservice.Operators;
using botservice.rest;
using botservice.WS;
using motivebot.Model.storage;
using motivebot.Model.storage.local;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http.Headers;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Polling;

namespace botservice.ViewModels
{
    public class mainVM : LifeCycleViewModelBase
    {

        #region vars
        IBotStorage botStorage;        
        IBotFactory botFactory;        
        IOperatorStorage operatorStorage;
        IDBStorage dbStorage;
        #endregion

        #region properties
        public ObservableCollection<BotBase> Bots { get; set; } = new();
        public ObservableCollection<BotBase> SelectedBots { get; set; } = new(); 
        
        BotBase selectedBot;
        public BotBase SelectedBot
        {
            get => selectedBot;
            set
            {
                SubContent = value;
                this.RaiseAndSetIfChanged(ref selectedBot, value);                
            }
        }

        object subContent;
        public object SubContent
        {
            get => subContent;
            set
            {
                this.RaiseAndSetIfChanged(ref subContent, value);
                if (subContent is SubContentVM) {
                    ((SubContentVM)subContent).OnCloseRequest += () =>
                    {
                        SubContent = null;
                    };
                }
            }
        }

        loggerVM logger;
        public loggerVM Logger {
            get => logger;
            set => this.RaiseAndSetIfChanged(ref logger, value);
        }

        operatorsVM operators;
        public operatorsVM OperatorsVM
        {
            get => operators;
            set => this.RaiseAndSetIfChanged(ref operators, value);
        }
        #endregion

        #region commands
        public ReactiveCommand<Unit, Unit> addCmd { get; }
        public ReactiveCommand<Unit, Unit> removeCmd { get;  }
        public ReactiveCommand<Unit, Unit> editCmd { get; }        
        #endregion
        public mainVM()
        {

            Logger = new loggerVM();            

            RestService restService = new RestService(Logger);

            PushRequestProcessor pushRequestProcessor = new PushRequestProcessor();
            StatusUpdateRequestProcessor statusUpdateRequestProcessor = new StatusUpdateRequestProcessor();
            NotifyRequestProcessor notifyRequestProcessor = new NotifyRequestProcessor();
            DiagnosticsRequestProcessor diagnosticsRequestProcessor = new DiagnosticsRequestProcessor();

            restService.RequestProcessors.Add(pushRequestProcessor);
            restService.RequestProcessors.Add(statusUpdateRequestProcessor);
            restService.RequestProcessors.Add(notifyRequestProcessor);
            restService.RequestProcessors.Add(diagnosticsRequestProcessor);


            restService.Listen();

            botStorage = new LocalBotStorage();
            operatorStorage = new LocalOperatorStorage();

            ApplicationContext context = new ApplicationContext();
            context.Database.EnsureCreated();
            IDBStorage dbStorage = new DBStorage(context);

            botFactory = new BotFactory(operatorStorage, botStorage, dbStorage);

            var models = botStorage.GetAll();

            OperatorsVM = new operatorsVM(operatorStorage);            
            
            foreach (var model in models)
            {
                //ar bot = new AviatorBot_v0(model, Logger); 
                var bot = /*new AviatorBot_v0(model, Logger);*/ botFactory.Get(model, logger);
                Bots.Add(bot);

                pushRequestProcessor.Add(bot as IPushObserver);
                statusUpdateRequestProcessor.Add(bot as IStatusObserver);
                notifyRequestProcessor.Add(bot);

                var dresulter = bot as IDiagnosticsResulter;
                if (dresulter != null)
                    diagnosticsRequestProcessor.Add(dresulter);

                operatorStorage.Add(model.geotag);                
            }

            Task.Run(async () => {

                foreach (var bot in Bots)
                {
                    try
                    {
                        if (!(bot is pushbot))
                        await bot.Start();

                    } catch (Exception ex)
                    {
                        logger.err(bot.Geotag, $"UNABLE TO START {bot.Geotag}");
                    }
                }
            
            });

            #region commands
            addCmd = ReactiveCommand.Create(() => {

                SelectedBot = null;

                var addvm = new addBotVM();
                addvm.BotCreatedEvent += (model) => {
                    try
                    {
                        botStorage.Add(model);
                    } catch (Exception ex)
                    {
                        throw;
                        //сообщение об ошибке
                    }

                    var bot = /*new AviatorBot_v0(model, Logger);*/ botFactory.Get(model, logger);
                    Bots.Add(bot);

                    operatorStorage.Add(model.geotag);

                    pushRequestProcessor.Add(bot as IPushObserver);
                    statusUpdateRequestProcessor.Add(bot as IStatusObserver);
                    notifyRequestProcessor.Add(bot);
                    diagnosticsRequestProcessor.Add(bot as IDiagnosticsResulter);
                };

                addvm.CancelledEvent += () => {                    
                };

                SubContent = addvm;
            });
            removeCmd = ReactiveCommand.Create(() =>
            {

                if (SelectedBot == null)
                    return;

                if (SelectedBot.IsActive)
                    return;

                var geotag = SelectedBot.Geotag;
                try
                {
                    botStorage.Remove(geotag);
                }
                catch (Exception ex)
                {
                    throw;
                    //сообщение об ошибке
                }

                Bots.Remove(SelectedBot);
                pushRequestProcessor.Remove(SelectedBot as IPushObserver);

            });

            editCmd = ReactiveCommand.Create(() => {
                if (SelectedBot == null) 
                    return;
                var geotag = SelectedBot.Geotag;
                var editvm = new editBotVM(botStorage, SelectedBot);

            });            
            #endregion

            #region helpers
            #endregion
        }

    }
}
