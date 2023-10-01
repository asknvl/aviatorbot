using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace aviatorbot.Models.messages
{
    public interface IMessageUpdater
    {
        Task UpdateMessageRequest(string code);
        event Action<string> UpdateMessageRequestEvent;
        event Action<string, bool> MessageUpdatedEvent;
    }
}
