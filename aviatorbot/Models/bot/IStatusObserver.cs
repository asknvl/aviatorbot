using aviatorbot.rest;
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
        string GetGeotag();
        Task UpdateStatus(StatusUpdateDataDto updateData);
    }
}
