using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace csb.server
{    
    public class TGFollowerTrackApi_v1 : ITGFollowerTrackApi
    {
        #region const
        string token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpZCI6MjksImxldHRlcl9pZCI6IllCIiwiaWF0IjoxNjU5MTk2Nzc1fQ.8qzVaYVky9m4m3aa0f8mMFI6mk3-wyhAiSZVmiHKwmg";
        #endregion

        #region vars
        string url;        
        ServiceCollection serviceCollection;
        IHttpClientFactory httpClientFactory;
        HttpClient httpClient;
        #endregion

        public TGFollowerTrackApi_v1(string url)
        {
            this.url = url;
            serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            var services = serviceCollection.BuildServiceProvider();
            httpClientFactory = services.GetRequiredService<IHttpClientFactory>();
            httpClient = httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        #region public
        private static void ConfigureServices(ServiceCollection services)
        {
            services.AddHttpClient();
        }

        public class InviteLinkDto
        {
            [JsonProperty]
            public string geolocation { get; set; }
            [JsonProperty]
            public string invite_link { get; set; }
        }

        public async Task EnqueueInviteLink(string geotag, string invite_link)
        {
            var addr = $"{url}/v1/telegram/telegramLink";         

            InviteLinkDto param = new InviteLinkDto()
            {
                geolocation = geotag,
                invite_link = invite_link
            };

            var json = JsonConvert.SerializeObject(param);
            var data = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var response = await httpClient.PostAsync(addr, data);
                var result = await response.Content.ReadAsStringAsync();
                var jres = JObject.Parse(result);
                bool res = jres["success"].ToObject<bool>();
                if (!res)
                    throw new Exception("success=false");

            } catch (Exception ex)
            {
                throw new Exception($"EnqueueInviteLink {ex.Message}");
            }
        }

        public class availableInviteLinksDtoResponse
        {
            [JsonProperty]
            public bool success { get; set; }
            [JsonProperty]
            public int available_count { get; set; }
        }

        public async Task<int> GetInviteLinksAvailable(string geotag)
        {

            int res = 0;
            var addr = $"{url}/v1/telegram/availableTelegramLinks?geo={geotag}";            

            try
            {
                var response = await httpClient.GetAsync(addr);
                response.EnsureSuccessStatusCode();
                var result = await response.Content.ReadAsStringAsync();
                var resp = JsonConvert.DeserializeObject<availableInviteLinksDtoResponse>(result);

                if (resp.success)
                    res = resp.available_count;
                else
                    throw new Exception($"success=false");
            }
            catch (Exception ex)
            {
                throw new Exception($"GetInviteLinksAvailable {ex.Message}");
            }

            return res;
        }        
        #endregion
    }
}