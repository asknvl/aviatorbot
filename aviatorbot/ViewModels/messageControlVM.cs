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
        #region
        MessageType type;
        #endregion

        #region properties        
        bool isset;
        public bool IsSet
        {
            get => isset;
            set => this.RaiseAndSetIfChanged(ref isset, value); 

        }
        #endregion

        #region commands
        public ReactiveCommand<Unit, Unit> updateCmd { get; }
        #endregion        

        public messageControlVM(MessageProcessorBase processor, MessageType type)
        {
            this.type = type;

            processor.MessageUpdatedEvent += (code, isset) => { 
                if (code.Equals(type.Code))
                {

                }
            };

            #region commands
            updateCmd = ReactiveCommand.CreateFromTask(async () => {
                processor?.UpdateMessageRequest(type.Code);
            });
            #endregion
        }

    }
}
