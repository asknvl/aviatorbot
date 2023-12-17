using aviatorbot.Models.bot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace aviatorbot.rest
{
    internal class StatusUpdateRequestProcessor : IRequestProcessor, IStatusObservable
    {
        #region vars
        List<IStatusObserver> statusObservers = new List<IStatusObserver>();
        #endregion

        #region public        
        public void Add(IStatusObserver observer)
        {
            if (!statusObservers.Contains(observer))
                statusObservers.Add(observer);
        }
        public void Remove(IStatusObserver observer)
        {
            statusObservers.Remove(observer);
        }

        public async Task<(HttpStatusCode, string)> ProcessRequestData(string data)
        {            
            HttpStatusCode code = HttpStatusCode.BadRequest;
            string responseText = "Incorrect parameters";

            try
            {
                await Task.Run(() => { });
            } catch (Exception ex)
            {
            }

            return (code, responseText);
        }
        #endregion
    }
}
