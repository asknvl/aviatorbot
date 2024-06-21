using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace botservice.Models.bot
{
    public interface IPushObserver
    {
        string GetGeotag();
        Task<bool> Push(long tg_id, string code, string uuid, int notification_id, string? firstname);
    }
}
