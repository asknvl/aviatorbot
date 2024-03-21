using botservice.Models.bot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace botservice.rest
{
    public interface INotifyObservable
    {
        void Add(INotifyObserver observer);
        void Remove(INotifyObserver observer);
    }
}
