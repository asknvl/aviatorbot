using aviatorbot.Models.bot;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace aviatorbot.rest
{
    public class PushRequestProcessor : IRequestProcessor, IPushObservable
    {
        #region vars
        List<IPushObserver> pushObservers = new List<IPushObserver>();
        #endregion

        #region public
        public void Add(IPushObserver observer)
        {
            if (!pushObservers.Contains(observer))
                pushObservers.Add(observer);
        }

        public async Task<(HttpStatusCode, string)> ProcessRequestData(string data)
        {
            string responseText = "Incorrect parameters";
            HttpStatusCode code = HttpStatusCode.BadRequest;

            try
            {
                await Task.Run(async () => {

                    var pushdata = JsonConvert.DeserializeObject<PushRequestDto>(data);
                    var geotag = pushdata.geotag;
                    var observer = pushObservers.FirstOrDefault(o => o.GetGeotag().Equals(geotag));

                    //InactiveUsers inactiveUsers = new InactiveUsers(geotag);
                    int cntr = 0;
                    if (observer != null) {
                        foreach (var item in pushdata.data)
                        {
                            try
                            {
                                bool res = await observer.Push(item.tg_id, item.code);
                                if (res)
                                 cntr++;
                            } catch (Exception ex)
                            {
                                //inactiveUsers.data.Add(item.tg_id);                                
                            }
                        }
                        code = HttpStatusCode.OK;
                        responseText = $"{cntr} users pushed";
                    }
                    else
                    {
                        code = HttpStatusCode.NotFound;
                        responseText = "No push observers found";
                    }

                    //responseText = JsonConvert.SerializeObject(inactiveUsers);
                });

            } catch (Exception ex)
            {                

            } 

            return (code, responseText);

        }

        public void Remove(IPushObserver observer)
        {
            pushObservers.Remove(observer);
        }
        #endregion
    }

    public class PushInfoDto
    {
        [JsonRequired]
        public long tg_id { get; set; }
        [JsonRequired]
        public string code { get; set; } 
    }

    public class PushRequestDto
    {
        [JsonRequired]
        public string geotag { get; set; }
        [JsonRequired]
        public List<PushInfoDto> data { get; set; } = new();
    }

    public class InactiveUsers
    {
        public string geotag { get; set; }
        public List<long> data { get; set; } = new();
        public InactiveUsers(string geotag)
        {
            this.geotag = geotag;
        }
    }

}
