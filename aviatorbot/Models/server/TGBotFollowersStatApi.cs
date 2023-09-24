using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace asknvl.server
{
    public class TGBotFollowersStatApi : ITGBotFollowersStatApi
    {
        #region const
        string token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpZCI6MjksImxldHRlcl9pZCI6IllCIiwiaWF0IjoxNjU5MTk2Nzc1fQ.8qzVaYVky9m4m3aa0f8mMFI6mk3-wyhAiSZVmiHKwmg";
        #endregion

        #region vars
        string url;
        ServiceCollection serviceCollection;
        IHttpClientFactory httpClientFactory;
        #endregion

        public TGBotFollowersStatApi(string url)
        {
            this.url = url;
            serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            var services = serviceCollection.BuildServiceProvider();
            httpClientFactory = services.GetRequiredService<IHttpClientFactory>();
        }

        #region private
        private static void ConfigureServices(ServiceCollection services)
        {
            services.AddHttpClient();
        }
        #endregion

        #region public
        public class followersDto
        {
            public List<Follower> users { get; set; }
            public followersDto(List<Follower> followers)
            {
                this.users = followers;
            }
        }

        public async Task UpdateFollowers(List<Follower> followers)
        {
            var addr = $"{url}/v1/telegram";
            var httpClient = httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            followersDto flwrs = new followersDto(followers);
            var json = JsonConvert.SerializeObject(flwrs);
            var data = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var response = await httpClient.PostAsync(addr, data);
                var result = await response.Content.ReadAsStringAsync();
                var jres = JObject.Parse(result);
                bool res = jres["success"].ToObject<bool>();
                if (!res)
                    throw new Exception($"success=false");

            }
            catch (Exception ex)
            {
                throw new Exception($"UpdateFollowers {ex.Message}");
            }
        }


        public class tgFollowerStatusResponse
        {
            public bool success { get; set; }
            public string uuid { get; set; }
            public string status_code { get; set; }
        }

        public async Task<(string, string)> GetFollowerState(long id)
        {
            string status;
            string uuid;

            var addr = $"{url}/v1/telegram/telegramBotStatus?userID={id}";
            var httpClient = httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            try
            {
                var response = await httpClient.GetAsync(addr);
                response.EnsureSuccessStatusCode();
                var result = await response.Content.ReadAsStringAsync();
                var resp = JsonConvert.DeserializeObject<tgFollowerStatusResponse>(result);

                if (resp.success)
                {
                    uuid = resp.uuid;
                    status = resp.status_code;
                }
                else
                    throw new Exception($"sucess=false");
            }
            catch (Exception ex)
            {
                throw new Exception($"GetFollowerState {ex.Message}");
            }

            return (uuid, status);
        }


        #endregion
    }
}
