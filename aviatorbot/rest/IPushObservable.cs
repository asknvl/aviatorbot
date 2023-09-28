using aviatorbot.Models.bot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace aviatorbot.rest
{
    public interface IPushObservable
    {
        void Add(IPushObserver observer);
        void Remove(IPushObserver observer);        
    }
}
