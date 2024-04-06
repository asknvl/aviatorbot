using aviatorbot.Models.bot;
using botservice.Models.bot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace aviatorbot.rest
{
    public interface IDiagnosticsObservable
    {
        void Add(IDiagnosticsResulter observer);
        void Remove(IDiagnosticsResulter observer);
    }
}
