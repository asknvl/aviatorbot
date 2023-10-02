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

                    InactiveUsers inactiveUsers = new InactiveUsers(geotag);

                    if (observer != null)                    {
                        foreach (var item in pushdata.data)
                        {
                            try
                            {
                                await observer.Push(item.id, (PushType)item.code);
                            } catch (Exception ex)
                            {
                                inactiveUsers.data.Add(item.id);
                            }
                        }
                    }

                    responseText = JsonConvert.SerializeObject(inactiveUsers);
                });

            } catch (Exception ex)
            {                
            } finally
            {
                code = HttpStatusCode.OK;

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
        public long id { get; set; }
        public int code { get; set; } 
    }

    public class PushRequestDto
    {
        public string geotag { get; set; }
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
