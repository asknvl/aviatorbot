using aviatorbot.Model.bot;
using ReactiveUI;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace aviatorbot.ViewModels
{
    public class addBotVM : SubContentVM
    {
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

        string pm;
        public string PM
        {
            get => pm;
            set => this.RaiseAndSetIfChanged(ref pm, value);
        }

        string channel;
        public string Channel
        {
            get => channel;
            set => this.RaiseAndSetIfChanged(ref channel, value);
        }
        #endregion

        #region commands
        public ReactiveCommand<Unit, Unit> closeCmd { get; }
        public ReactiveCommand<Unit, Unit> addCmd { get; }
        #endregion

        public addBotVM()
        {
            #region commands
            closeCmd = ReactiveCommand.Create(() => {
                Close();
            });
            addCmd = ReactiveCommand.Create(() => {
                BotModel model = new BotModel()
                {
                    geotag = Geotag,
                    token = Token,
                    link = Link,
                    pm = PM,
                    channel = Channel
                };
                BotCreatedEvent?.Invoke(model);
                Close();
            });
            #endregion
        }

        #region callbacks
        public event Action<BotModel> BotCreatedEvent;
        #endregion

    }
}
