using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace aviatorbot.Models.bot
{
    public interface IDiagnosticsResulter
    {
        string Geotag { get; set; }
        Task<DiagnosticsResult> GetDiagnosticsResult();
    }

    public class DiagnosticsResult
    {        
        public string botGeotag {  get; set; }
        public bool isOk { get; set; } = true;
        public List<string> errorsList { get; set; } = new();

        public string GetErrorsDescription()
        {            
            string res = string.Empty;
            if (!isOk && errorsList.Count > 0)
            {
                foreach (var error in errorsList)
                {
                    res += $"{error}\n";
                }
            }
            return res;
            
        }
        
    }
}
