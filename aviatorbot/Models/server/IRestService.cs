using Avalonia.Remote.Protocol.Designer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace aviatorbot.Models.server
{
    public interface IRestService
    {
        int Port { get; set; }
        void Listen();
    }
}
