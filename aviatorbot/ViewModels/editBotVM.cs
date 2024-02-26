using aksnvl.storage;
using aviatorbot.Model.bot;
using motivebot.Model.storage;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace aviatorbot.ViewModels
{
    public class editBotVM : SubContentVM
    {
        #region properties
        IBotStorage botStorage;
        BotModel botModel;
        #endregion

        #region properties
        string geotag;
        public string Geotag
        {
            get => geotag;
            set => this.RaiseAndSetIfChanged(ref geotag, value);
        }

        string name;
        public string Name
        {
            get => name;
            set => this.RaiseAndSetIfChanged(ref name, value);
        }

        string token;
        public string Token
        {
            get => token;
            set => this.RaiseAndSetIfChanged(ref token, value);
        }

        string link;
        public string Link
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

        string pm;
        public string PM
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

        string channel;
        public string Channel
        {
            get => channel;
            set => this.RaiseAndSetIfChanged(ref channel, value);
        }

        bool? postbacks;
        public bool? Postbacks
        {
            get => postbacks;
            set => this.RaiseAndSetIfChanged(ref postbacks, value);
        }
        #endregion

        #region commands
        ReactiveCommand<Unit, Unit> saveCmd { get; }
        ReactiveCommand<Unit, Unit> cancelCmd { get; }
        #endregion

        public editBotVM(IBotStorage botstorage, AviatorBotBase bot)
        {

            Geotag = bot.Geotag;
            Token = bot.Token;
            Link = bot.Link;
            SUPPORT_PM = bot.SUPPORT_PM;
            PM = bot.PM;
            ChannelTag = bot.ChannelTag;
            Channel = bot.Channel;

            botStorage = botstorage;
            var models = botStorage.GetAll();
            botModel = models.FirstOrDefault(m => m.geotag.Equals(bot.Geotag));

            #region commands
            saveCmd = ReactiveCommand.Create(() => {

                var newModel = new BotModel()
                {
                    geotag = Geotag,
                    token = Token,
                    link = Link,
                    support_pm = SUPPORT_PM,
                    pm = PM,
                    channel_tag = ChannelTag,
                    channel = Channel,
                    postbacks = Postbacks,
                };

            });

            cancelCmd = ReactiveCommand.Create(() => {
                
            });
            #endregion
        }

        #region public
        #endregion      
    }
}
