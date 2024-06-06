using aviatorbot.Models.bot;
using botservice.Models.bot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace aviatorbot.rest
{
    public interface IAutoReplyObservable
    {
        void Add(IAutoReplyObserver observer);
        void Remove(IAutoReplyObserver observer);
    }
}
