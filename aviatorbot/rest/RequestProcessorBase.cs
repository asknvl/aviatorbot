using aviatorbot.Models.bot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace aviatorbot.rest
{
    public interface IRequestProcessor
    {
        Task<(HttpStatusCode, string)> ProcessRequestData(string data);
    }
}
