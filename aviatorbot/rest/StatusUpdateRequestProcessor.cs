using botservice.Models.bot;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace botservice.rest
{
    public class StatusUpdateRequestProcessor : IRequestProcessor, IStatusObservable
    {
        #region vars
        List<IStatusObserver> statusObservers = new List<IStatusObserver>();
        #endregion

        #region public        
        public void Add(IStatusObserver observer)
        {
            if (observer != null && !statusObservers.Contains(observer))
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
                await Task.Run(async () => {
                    var updateData = JsonConvert.DeserializeObject<StatusUpdateDataDto>(data);                   
                    var observers = statusObservers.Where(o => o.GetGeotag().Equals(updateData.geotag) || o.GetRegisterSource().Equals(updateData.geotag));

                    foreach (var observer in observers)
                    {
                        observer?.UpdateStatus(updateData);
                    }

                    //if (observer != null)
                    //{
                    //    observer.UpdateStatus(updateData);
                    //}
                });

                code = HttpStatusCode.OK;
                responseText = $"{code.ToString()}";

            } catch (Exception ex)
            {
            }
            return (code, responseText);
        }

        public Task<(HttpStatusCode, string)> ProcessRequest()
        {
            throw new NotImplementedException();
        }
        #endregion
    }

    public class StatusUpdateDataDto
    {
        [JsonRequired]
        public string geotag { get; set; }
        [JsonRequired]
        public long tg_id { get; set; }
        [JsonRequired]
        public string uuid { get; set; }
        [JsonProperty]
        public string? start_params { get; set; }
        [JsonProperty]
        public string? status_old { get; set; }
        [JsonRequired]
        public string? status_new { get; set;}
        [JsonProperty]
        public long? time { get; set; }
        [JsonRequired]
        public double amount_local_currency { get; set; }
        [JsonRequired]
        public double target_amount_local_currency { get; set; }

        public override string ToString()
        {
            return $"{tg_id} {uuid} {status_old}->{status_new} (paid:{amount_local_currency} need:{target_amount_local_currency})";
        }
    }
}
