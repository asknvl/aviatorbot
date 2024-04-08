using aviatorbot.Models.bot;
using botservice.bot;
using botservice.rest;
using JetBrains.Annotations;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace aviatorbot.rest
{
    public class DiagnosticsRequestProcessor : IRequestProcessor, IDiagnosticsObservable
    {
        #region vars
        List<IDiagnosticsResulter> diagnosticResulters = new List<IDiagnosticsResulter>();

        public void Add(IDiagnosticsResulter resulter)
        {
            if (!diagnosticResulters.Contains(resulter))
                diagnosticResulters.Add(resulter);
        }
        public void Remove(IDiagnosticsResulter resulter)
        {
            diagnosticResulters.Remove(resulter);
        }
        #endregion

        public Task<(HttpStatusCode, string)> ProcessRequestData(string data)
        {
            throw new NotImplementedException();
        }

        public async Task<(HttpStatusCode, string)> ProcessRequest()
        {
            HttpStatusCode code = HttpStatusCode.OK;
            string responseText = code.ToString();            

            serviceDiagnosticsDto result = new serviceDiagnosticsDto();
            result.service_name = "Aviator&Latam bots";

            try
            {

                foreach (var item in diagnosticResulters)
                {
                    var dresult = await item.GetDiagnosticsResult();

                    if (!dresult.isOk)
                    {
                        result.cheсk_result = false;
                        result.addError(dresult.botGeotag, dresult.GetErrorsDescription());
                    }
                }


            } catch (Exception ex)
            {

            }

            if (!result.cheсk_result)
            {
                var sresult = JsonConvert.SerializeObject(result, Formatting.Indented);
                responseText = sresult.ToString();
            }                

            
            return (code, responseText);
        }
    }
}
