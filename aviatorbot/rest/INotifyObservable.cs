using aviatorbot.Models.bot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace aviatorbot.rest
{
    public interface INotifyObservable
    {
        void Add(INotifyObserver observer);
        void Remove(INotifyObserver observer);
    }
}
