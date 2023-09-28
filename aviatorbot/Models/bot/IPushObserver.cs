using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace aviatorbot.Models.bot
{
    public interface IPushObserver
    {
        string GetGeotag();
        Task Push(long id, PushType type);
    }

    public enum PushType
    {
        _3h_reg = 0,
        _3h_dep = 1,
        _3h_redep = 2,
        _24h_no_activity = 3
    }
}
