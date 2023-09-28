using asknvl.logger;
using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Requests.Abstractions;

namespace aviatorbot.Models.server
{
    public class RestService : IRestService
    {
        #region const
        string TAG = "RST";
        #endregion

        #region vars
        ILogger logger;
        #endregion

        #region properties
        public int Port { get; set; } = 5000;
        #endregion

        public RestService(ILogger logger)
        {
            this.logger = logger;
        }

        #region private
        async Task<string> processGetRequest(HttpListenerContext context)
        {
            string res = string.Empty;
            await Task.Run(() => {             
            });
            return res;
        }

        async Task<string> processPostRequest(HttpListenerContext context)
        {
            string res = string.Empty;
            await Task.Run(async () => {

                var request = context.Request;
                string path = request.Url.AbsolutePath;

                using var reader = new StreamReader(request.InputStream, request.ContentEncoding);
                var requestBody = await reader.ReadToEndAsync();

                var splt = path.Split('/');

                switch (splt[1])
                {
                    case "pushes":
                        break;
                    default:
                        break;
                }

                

            });
            return res;
        }
        async Task processRequest(HttpListenerContext context)
        {
            var request = context.Request;
            var response = context.Response;

            string responseText = string.Empty;

            switch (request.HttpMethod)
            {
                case "GET":
                    responseText = await processGetRequest(context);
                    break;

                case "POST":
                    responseText = await processPostRequest(context);
                    break;

                default:
                    response.StatusCode = (int)HttpStatusCode.MethodNotAllowed;
                    responseText = "Method not allowed";
                    break;
            }

            var buffer = Encoding.UTF8.GetBytes(responseText);
            response.ContentLength64 = buffer.Length;
            var output = response.OutputStream;
            await output.WriteAsync(buffer, 0, buffer.Length);

        }
        #endregion

        #region public
        public async void Listen()
        {
            var listener = new HttpListener();
            listener.Prefixes.Add($"http://localhost:{Port}/pushes/");

            try
            {
                logger.inf(TAG, "Starting rest server...");
                listener.Start();
            }
            catch (Exception ex)
            {
                logger.err(TAG, $"Rest server not started {ex.Message}");
            }

            logger.inf(TAG, "Rest server started");

            while (true)
            {
                var context = await listener.GetContextAsync();
                await processRequest(context);
            }
        }
        #endregion
    }
}
