using aviatorbot.Model.bot;
using aviatorbot.Models.bot;
using aviatorbot.Models.server;
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

namespace aviatorbot.ViewModels
{
    public class mainVM : LifeCycleViewModelBase
    {

        #region vars
        IBotStorage botStorage;
        #endregion

        #region properties
        public ObservableCollection<AviatorBotBase> Bots { get; set; } = new();

        AviatorBotBase selectedBot;
        public AviatorBotBase SelectedBot
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
        #endregion

        #region commands
        public ReactiveCommand<Unit, Unit> addCmd { get; }
        public ReactiveCommand<Unit, Unit> removeCmd { get;  }
        public ReactiveCommand<Unit, Unit> editCmd { get; }
        #endregion
        public mainVM()
        {

            

            Logger = new loggerVM();

            //RestService rest = new RestService(Logger);
            //rest.Listen();

            botStorage = new LocalBotStorage();
            var models = botStorage.GetAll();
            
            foreach (var model in models)
            {
                var bot = new AviatorBot_v0(model, Logger); 
                Bots.Add(bot);
            }

            //Bots.Add(new AviatorBot_v0() { Geotag = "TEST0" });
            //Bots.Add(new AviatorBot_v0() { Geotag = "TEST1" });
            //Bots.Add(new AviatorBot_v0() { Geotag = "TEST2" });

            #region commands
            addCmd = ReactiveCommand.Create(() => {

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
                    Bots.Add(new AviatorBot_v0(model, Logger));
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
