﻿using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace botplatform.Models.server
{
    public class AIServer : IAIserver
    {
        #region const
        string token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1c2VyIjoiYm90MDEiLCJpYXQiOjE3MTMwNzgyMzJ9.ibCadqPOLluTcpp5_QPTlKc_AZMvDNkcN_2zSPzJdOM";
        #endregion

        #region vars
        string url;
        ServiceCollection serviceCollection;
        IHttpClientFactory httpClientFactory;
        HttpClient httpClient;
        #endregion

        public AIServer(string url)
        {
            this.url = url;
            serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            var services = serviceCollection.BuildServiceProvider();
            httpClientFactory = services.GetRequiredService<IHttpClientFactory>();

            httpClient = httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        private static void ConfigureServices(ServiceCollection services)
        {
            services.AddHttpClient();
        }

        #region public
        class messageDto
        {
            public string message { get; set; }
            public long tg_user_id { get; set; }
            public string source { get; set; }
        }


        public async Task SendMessageToAI(string geotag, long tg_user_id, string text)
        {
            var addr = $"{url}/api/message";

            messageDto msg = new messageDto()
            {
                source = geotag,
                tg_user_id = tg_user_id,
                message = text
            };

            var json = JsonConvert.SerializeObject(msg, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            var data = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var response = await httpClient.PostAsync(addr, data);                
                //var result = await response.Content.ReadAsStringAsync();
                response.EnsureSuccessStatusCode();                
            }
            catch (Exception ex)
            {
                throw new Exception($"SendToAI {ex.Message}");
            }
        }

        public async Task SendToAI(string geotag, long tg_user_id, string? fn, string? ln, string? un, string? message = null, string? base64_image = null)
        {
            var addr = $"{url}/api/message";

            universalMessageDto msg = new universalMessageDto(geotag, tg_user_id, fn, ln, un, text: message, base64_image: base64_image);

            var json = JsonConvert.SerializeObject(msg, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            var data = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var response = await httpClient.PostAsync(addr, data);
                //var result = await response.Content.ReadAsStringAsync();
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                throw new Exception($"SendToAI {ex.Message}");
            }
        }

        public async Task<string> Moderate(string? text = null, string? base64_image = null)
        {
            string res = "";

            var addr = $"{url}/api/message/moderator";
            try
            {

                moderateDto moder = new moderateDto(text, base64_image);
                var json = JsonConvert.SerializeObject(moder, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                var data = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync(addr, data);
                response.EnsureSuccessStatusCode();
                var result = await response.Content.ReadAsStringAsync();

                JObject parsedJson = JObject.Parse(result);
                try
                {
                    res = parsedJson["response_message"].ToString();
                } catch (Exception ex)
                {
                }

            } catch (Exception ex)
            {
                throw new Exception($"Moderate {ex.Message}");
            }

            return res;            
        }

        public async Task<linkDto> GetLink(string geotag, long tg_user_id)
        {
            linkDto res = null;
            var addr = $"{url}/api/leads/link?source={geotag}&tgUserId={tg_user_id}";

            try
            {
                var response = await httpClient.GetAsync(addr);                
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadAsStringAsync();
                var resp = JsonConvert.DeserializeObject<linkDto>(result);

                return resp;

            }
            catch (Exception ex)
            {
                throw new Exception($"GetLink {ex.Message}");
            }
        }        
        #endregion
    }
}
