using aviatorbot.Models.bot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace aviatorbot.rest
{
    public interface IStatusObservable
    {
        void Add(IStatusObserver observer);
        void Remove(IStatusObserver observer);
    }
}
