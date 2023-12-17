using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace aviatorbot.Models.bot
{
    public interface IStatusObserver
    {
        Task UpdateStatus(long tg_id, string old_status, string new_status);
    }
}
