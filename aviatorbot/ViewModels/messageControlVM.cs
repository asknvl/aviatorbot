using aviatorbot.Models.messages;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace aviatorbot.ViewModels
{
    public class messageControlVM : ViewModelBase
    {
        #region vars        
        IMessageUpdater updater;
        #endregion

        #region properties        
        bool isset;
        public bool IsSet
        {
            get => isset;
            set => this.RaiseAndSetIfChanged(ref isset, value); 

        }

        string code;
        public string Code
        {
            get => code;
            set => this.RaiseAndSetIfChanged(ref code, value); 

        }

        string description;
        public string Description
        {
            get => description;
            set => this.RaiseAndSetIfChanged(ref description, value);
        }
        #endregion

        #region commands
        public ReactiveCommand<Unit, Unit> updateCmd { get; }
        #endregion        

        public messageControlVM(IMessageUpdater updater)
        {
                        
            this.updater = updater;

            //processor.MessageUpdatedEvent += (code, isset) => { 
            //    if (code.Equals(type.Code))
            //    {

            //    }
            //};

            updater.MessageUpdatedEvent += (code, isset) => {

            };

            #region commands
            updateCmd = ReactiveCommand.CreateFromTask(async () => {
                updater?.UpdateMessageRequest(Code);
            });
            #endregion
        }

    }
}
